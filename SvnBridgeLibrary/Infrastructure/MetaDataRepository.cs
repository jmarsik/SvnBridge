using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;
using System.Data;

namespace SvnBridge.Infrastructure
{
	public class MetaDataRepository : RepositoryBase, IMetaDataRepository
	{
		private readonly ISourceControlService sourceControlService;
		private readonly string serverUrl;
		private readonly string rootPath;
		private readonly ICredentials credentials;

		public MetaDataRepository(
			ISourceControlService sourceControlService,
			ICredentials credentials,
			string serverUrl,
			string rootPath,
			string connectionString)
			: base(connectionString)
		{
			this.sourceControlService = sourceControlService;
			this.serverUrl = serverUrl;
			this.rootPath = rootPath;
			this.credentials = credentials;
		}

		public SourceItem QueryPreviousVersionOfItem(int itemId, int revision)
		{
			int previousRevision = (revision - 1);
			SourceItem[] items = sourceControlService.QueryItems(
				serverUrl, credentials, new int[] { itemId }, previousRevision);
			if (items.Length == 0)
				return null;
			return items[0];
		}

		public SourceItem[] QueryItems(int reversion, string path, Recursion recursion)
		{
			string serverPath = GetServerPath(path);

			EnsureRevisionIsCached(reversion, path);
			List<SourceItem> items = new List<SourceItem>();
			TransactionalCommand(delegate(IDbCommand command)
			{
				SetSelectItemQuery(recursion, command);
				Parameter(command, "Revision", reversion);
				Parameter(command, "UserName", CurrentUserName);
				Parameter(command, "ServerUrl", serverUrl);

				if (recursion == Recursion.Full)
					Parameter(command, "Path", serverPath + '%');
				else
					Parameter(command, "Path", serverPath);

				if (recursion == Recursion.OneLevel)
					Parameter(command, "Parent", serverPath);

				using (IDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						SourceItem item = HydrateSourceItem(reader);

						items.Add(item);
					}
				}
			});
			return items.ToArray();
		}

		private string CurrentUserName
		{
			get
			{
				NetworkCredential credential = credentials.GetCredential(new Uri(serverUrl), "Basic");
				return credential.UserName + "@" + credential.Domain;
			}
		}

		private string GetServerPath(string path)
		{
			if (path.StartsWith("$/"))
				return path;

			string serverPath = rootPath;

			if (serverPath.EndsWith("/"))
				serverPath = serverPath.Substring(0, serverPath.Length - 1);

			if (path.StartsWith("/") == false)
				serverPath = serverPath + '/' + path;
			else
				serverPath = serverPath + path;

			if (serverPath.EndsWith("/"))
				serverPath = serverPath.Substring(0, serverPath.Length - 1);

			return serverPath;
		}

		private SourceItem HydrateSourceItem(IDataRecord reader)
		{
			SourceItem item = new SourceItem();

			item.ItemType = (bool)reader["IsFolder"] ? ItemType.Folder : ItemType.File;
			item.ItemId = (int)reader["ItemId"];
			item.RemoteName = (string)reader["Name"];
			item.RemoteDate = (DateTime)reader["LastModifiedDate"];
			item.RemoteChangesetId = (int)reader["ItemRevision"];
			item.DownloadUrl = (string)reader["DownloadUrl"];
			return item;
		}

		private static void SetSelectItemQuery(Recursion recursion, IDbCommand command)
		{
			switch (recursion)
			{
				case Recursion.None:
					command.CommandText = Queries.SelectItem;
					break;
				case Recursion.OneLevel:
					command.CommandText = Queries.SelectItemOneLevel;
					break;
				case Recursion.Full:
					command.CommandText = Queries.SelectItemFullRecursion;
					break;
			}
		}

		public void EnsureRevisionIsCached(int revision, string path)
		{
			Transaction(delegate
			{
				string serverPath = GetServerPath(path);

				// already cached this version, skip inserting
				// we rely on the transaction to serialize requests here
				if (IsInCache(revision, serverPath))
					return;

				serverPath = ToDirectory(serverPath, revision);

				Events.RaiseStartingCachingRevision(serverUrl, revision);

				try
				{
					SourceItem[] items = sourceControlService.QueryItems(serverUrl,
																		 credentials,
																		 serverPath,
																		 RecursionType.Full,
																		 VersionSpec.FromChangeset(revision),
																		 DeletedState.NonDeleted,
																		 ItemType.Any);

					Command(delegate(IDbCommand command)
					{
						command.CommandText = Queries.InsertCachedRevision;
						Parameter(command, "ServerUrl", serverUrl);
						Parameter(command, "Revision", revision);
						Parameter(command, "UserName", CurrentUserName);
						Parameter(command, "RootPath", serverPath);
						command.ExecuteNonQuery();
					});

					foreach (SourceItem sourceItem in items)
					{
						SourceItem item = sourceItem;

						bool alreadyExists = false;

						Command(delegate(IDbCommand command)
						{
							command.CommandText = Queries.SelectItem;
							Parameter(command, "ServerUrl", serverUrl);
							Parameter(command, "UserName", CurrentUserName);
							Parameter(command, "Revision", revision);
							Parameter(command, "Path", item.RemoteName);

							using (IDataReader reader = command.ExecuteReader())
							{
								alreadyExists = reader.Read();
							}
						});

						if (alreadyExists)
							continue;

						Command(delegate(IDbCommand command)
						{
							command.CommandText = Queries.InsertItemMetaData;

							Parameter(command, "Id", SequentialGuid.Next());
							Parameter(command, "IsFolder", item.ItemType == ItemType.Folder);
							Parameter(command, "ItemId", item.ItemId);
							Parameter(command, "Name", item.RemoteName);
							Parameter(command, "UserName", CurrentUserName);
							Parameter(command, "Parent", GetParentName(item.RemoteName));
							Parameter(command, "ServerUrl", serverUrl);
							Parameter(command, "ItemRevision", item.RemoteChangesetId);
							Parameter(command, "EffectiveRevision", revision);
							Parameter(command, "DownloadUrl", item.DownloadUrl);
							Parameter(command, "LastModifiedDate", item.RemoteDate);

							command.ExecuteNonQuery();
						});
					}
				}
				finally
				{
					Events.RaiseFinishedCachingRevision(serverUrl, revision);
				}
			});
		}

		private string ToDirectory(string serverPath, int revision)
		{
			SourceItem[] items = sourceControlService.QueryItems(
				serverUrl,
				credentials,
				serverPath,
				RecursionType.None,
				VersionSpec.FromChangeset(revision),
				DeletedState.NonDeleted,
				ItemType.Any);

			if (items.Length == 0 || items[0].ItemType == ItemType.Folder)
				return serverPath;
			//can't use Path.GetDirectory() here, will turn / to \
			int lastIndexOfSlash = serverPath.LastIndexOf('/');
			return serverPath.Substring(0, lastIndexOfSlash);
		}

		private object GetParentName(string name)
		{
			int lastIndexOfSlash = name.LastIndexOf('/');
			if (lastIndexOfSlash == -1)
				return name;
			return name.Substring(0, lastIndexOfSlash);
		}

		public bool IsInCache(int revision, string path)
		{
			int? maybeRevisionFromDb = null;

			string serverPath = path;

			do
			{

				TransactionalCommand(delegate(IDbCommand command)
				{
					command.CommandText = Queries.SelectCachedRevision;
					Parameter(command, "Revision", revision);
					Parameter(command, "ServerUrl", serverUrl);
					Parameter(command, "UserName", CurrentUserName);
					Parameter(command, "RootPath", serverPath);
					maybeRevisionFromDb = (int?)command.ExecuteScalar();
				});
				if (serverPath.IndexOf('/') != -1)
				{
					serverPath = serverPath.Substring(0, serverPath.LastIndexOf('/'));
				}
				else
				{
					break;
				}

			} while (maybeRevisionFromDb == null);


			return maybeRevisionFromDb != null;
		}

		public void ClearCache()
		{
			TransactionalCommand(delegate(IDbCommand command)
			{
				ExecuteCommands(Queries.DeleteCache.Split(new char[] { ';' }, StringSplitOptions.None), command);
			});
		}

		public void CreateDatabase()
		{
			SqlCeEngine engine = new SqlCeEngine(connectionString);
			engine.CreateDatabase();

			TransactionalCommand(delegate(IDbCommand command)
			{
				ExecuteCommands(Queries.CreateDatabase.Split(new char[] { ';' }, StringSplitOptions.None), command);
			});
		}

		private void ExecuteCommands(string[] commands, IDbCommand command)
		{
			foreach (string sql in commands)
			{
				command.CommandText = sql.Trim();
				if (string.IsNullOrEmpty(command.CommandText))
					continue;
				command.ExecuteNonQuery();
			}
		}

		public void EnsureDbExists()
		{
			try
			{
				Transaction(delegate
				{
					//empty transaction block to verify that we can access DB
				});
			}
			catch
			{
				CreateDatabase();
			}
		}
	}
}
