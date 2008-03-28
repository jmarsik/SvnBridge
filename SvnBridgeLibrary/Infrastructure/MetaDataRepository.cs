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

		public SourceItem[] QueryItems(int reversion, string path, Recursion recursion)
		{
			string serverPath = rootPath + path;
			EnsureRevisionIsCached(reversion);
			List<SourceItem> items = new List<SourceItem>();
			TransactionalCommand(delegate(IDbCommand command)
			{
				SetSelectItemQuery(recursion, command);
				Parameter(command, "Revision", reversion);
				Parameter(command, "ServerUrl", serverUrl);

				if (recursion == Recursion.Full)
					Parameter(command, "Path", serverPath + '%');
				else
					Parameter(command, "Path", serverPath);

				if(recursion==Recursion.OneLevel)
					Parameter(command, "Parent", serverPath);

				using (IDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						SourceItem item = new SourceItem();

						item.ItemType = (bool)reader["IsFolder"] ? ItemType.Folder : ItemType.File;
						item.ItemId = (int)reader["ItemId"];
						item.RemoteName = (string)reader["Name"];
						item.RemoteDate = (DateTime)reader["LastModifiedDate"];
						item.RemoteChangesetId = (int)reader["ItemRevision"];
						item.DownloadUrl = (string)reader["DownloadUrl"];

						items.Add(item);
					}
				}
			});
			return items.ToArray();
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

		public void EnsureRevisionIsCached(int revision)
		{
			Transaction(delegate
			{
				// another thread already cached this version, skip inserting
				// Note that we rely on transaction semantics to ensure safety here
				if (IsInCache(revision))
					return;

				Events.RaiseStartingCachingRevision(serverUrl, revision);

				SourceItem[] items = sourceControlService.QueryItems(serverUrl,
																	 credentials,
																	 Constants.ServerRootPath,
																	 RecursionType.Full,
																	 VersionSpec.FromChangeset(revision),
																	 DeletedState.NonDeleted,
																	 ItemType.Any);



				Command(delegate(IDbCommand command)
				{
					command.CommandText = Queries.InsertCachedRevision;

					Parameter(command, "Revision", revision);

					command.ExecuteNonQuery();
				});

				foreach (SourceItem sourceItem in items)
				{
					SourceItem item = sourceItem;
					Command(delegate(IDbCommand command)
					{
						command.CommandText = Queries.InsertItemMetaData;

						Parameter(command, "Id", SequentialGuid.Next());
						Parameter(command, "IsFolder", item.ItemType == ItemType.Folder);
						Parameter(command, "ItemId", item.ItemId);
						Parameter(command, "Name", item.RemoteName);
						Parameter(command, "Parent", GetParentName(item.RemoteName));
						Parameter(command, "ServerUrl", serverUrl);
						Parameter(command, "ItemRevision", item.RemoteChangesetId);
						Parameter(command, "EffectiveRevision", revision);
						Parameter(command, "DownloadUrl", item.DownloadUrl);
						Parameter(command, "LastModifiedDate", item.RemoteDate);

						command.ExecuteNonQuery();
					});
				}

				Events.RaiseFinishedCachingRevision(serverUrl, revision);
			});
		}

		private object GetParentName(string name)
		{
			int lastIndexOfSlash = name.LastIndexOf('/');
			if(lastIndexOfSlash==-1)
				return name;
			return name.Substring(0, lastIndexOfSlash);
		}

		public bool IsInCache(int revision)
		{
			int? maybeRevisionFromDb = null;
			TransactionalCommand(delegate(IDbCommand command)
			{
				command.CommandText = Queries.SelectCachedRevision;
				Parameter(command, "Revision", revision);
				maybeRevisionFromDb = (int?)command.ExecuteScalar();
			});
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
	}
}