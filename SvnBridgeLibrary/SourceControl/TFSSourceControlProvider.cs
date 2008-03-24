namespace SvnBridge.SourceControl
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Text.RegularExpressions;
	using CodePlex.TfsLibrary.ObjectModel;
	using CodePlex.TfsLibrary.RepositoryWebSvc;
	using Dto;
	using Exceptions;
	using Infrastructure;
	using Interfaces;
	using Protocol;
	using Proxies;
	using Utility;

	[Interceptor(typeof(RetryOnSocketExceptionsInterceptor))]
	public class TFSSourceControlProvider : ISourceControlProvider, ICredentialsProvider
	{
		private readonly ISourceControlServicesHub sourceControlServicesHub;

		private static readonly Regex associatedWorkItems =
			new Regex(@"Work ?Items?: (.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

		private readonly ISourceControlUtility sourceControlHelper;
		private static readonly Dictionary<string, Activity> _activities = new Dictionary<string, Activity>();
		private readonly ICredentials credentials;

		private readonly string projectName;
		private readonly string rootPath;
		private readonly string serverUrl;

		private ILogger Logger
		{
			get { return sourceControlServicesHub.Logger; }
		}

		private ICache Cache
		{
			get { return sourceControlServicesHub.Cache; }
		}

		private IFileCache FileCache
		{
			get { return sourceControlServicesHub.FileCache; }
		}

		private ITFSSourceControlService SourceControlService
		{
			get { return sourceControlServicesHub.SourceControlService; }
		}

		private IWebTransferService WebTransferService
		{
			get { return sourceControlServicesHub.WebTransferService; }
		}

		private IAssociateWorkItemWithChangeSet AssociateWorkItemWithChangeSet
		{
			get { return sourceControlServicesHub.AssociateWorkItemWithChangeSet; }
		}

		public TFSSourceControlProvider(
			string serverUrl,
			string projectName,
			ISourceControlServicesHub sourceControlServicesHub)
		{
			this.sourceControlServicesHub = sourceControlServicesHub;

			if (projectName != null)
			{
				ProjectLocationInformation location =
					sourceControlServicesHub.ProjectInformationRepository.GetProjectLocation(credentials, projectName);
				this.projectName = location.RemoteProjectName;
				this.serverUrl = location.ServerUrl;
				rootPath = Constants.ServerRootPath + this.projectName + "/";
			}
			else
			{
				this.serverUrl = serverUrl.Split(',')[0];
				rootPath = Constants.ServerRootPath;
			}
			credentials =
				CredentialsHelper.GetCredentialsForServer(this.serverUrl, sourceControlServicesHub.Credentials);
			sourceControlHelper = new SourceControlUtility(SourceControlService, this, rootPath, serverUrl);
		}

		#region ISourceControlProvider Members

		public string ServerUrl
		{
			get { return serverUrl; }
		}

		public void CopyItem(string activityId,
							 string path,
							 string targetPath)
		{
			CopyAction copyAction = new CopyAction(path, targetPath, false);
			_activities[activityId].CopiedItems.Add(copyAction);
			ProcessCopyItem(activityId, copyAction, false);
		}

		public void DeleteActivity(string activityId)
		{
			SourceControlService.DeleteWorkspace(serverUrl, credentials, activityId);
			_activities.Remove(activityId);
		}

		public bool DeleteItem(string activityId,
							   string path)
		{
			if ((GetItems(-1, path, Recursion.None, true, false) == null) && (GetPendingItem(activityId, path) == null))
			{
				return false;
			}

			Activity activity = _activities[activityId];
			bool postCommitDelete = false;
			foreach (CopyAction copy in activity.CopiedItems)
			{
				if (copy.Path.StartsWith(path + "/"))
				{
					if (!activity.PostCommitDeletedItems.Contains(path))
					{
						activity.PostCommitDeletedItems.Add(path);
					}

					if (!copy.Rename)
					{
						ConvertCopyToRename(activityId, copy);
					}

					postCommitDelete = true;
				}
			}

			if (!postCommitDelete)
			{
				bool deleteIsRename = false;
				foreach (CopyAction copy in activity.CopiedItems)
				{
					if (copy.Path == path)
					{
						ConvertCopyToRename(activityId, copy);
						deleteIsRename = true;
					}
				}
				if (!deleteIsRename)
				{
					ProcessDeleteItem(activityId, path);
					activity.DeletedItems.Add(path);
				}
			}
			return true;
		}

		public FolderMetaData GetChangedItems(string path,
											  int versionFrom,
											  int versionTo,
											  UpdateReportData reportData)
		{
			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}

			FolderMetaData root = (FolderMetaData)GetItems(versionTo, path, Recursion.None);

			if (root != null)
			{
				root.Properties.Clear();
			}

			// the item doesn't exist and the reques was for a specific version
			if (root == null && reportData.UpdateTarget != null)
			{
				root = new FolderMetaData();
				DeleteMetaData deletedFile = new DeleteMetaData();
				deletedFile.Name = reportData.UpdateTarget;
				root.Items.Add(deletedFile);
				return root;
			}

			UpdateDiffCalculator udc = new UpdateDiffCalculator(this, sourceControlHelper);
			udc.CalculateDiff(path, versionTo, versionFrom, root, reportData);

			return root;
		}


		public ItemMetaData GetItemInActivity(string activityId,
											  string path)
		{
			Activity activity = _activities[activityId];

			foreach (CopyAction copy in activity.CopiedItems)
			{
				if (path.StartsWith(copy.TargetPath))
				{
					path = copy.Path + path.Substring(copy.TargetPath.Length);
				}
			}

			return GetItems(-1, path, Recursion.None);
		}

		public ItemMetaData GetItems(int version,
									 string path,
									 Recursion recursion)
		{
			return GetItems(version, path, recursion, false, true);
		}

		public ItemMetaData GetItemsWithoutProperties(int version,
													  string path,
													  Recursion recursion)
		{
			return GetItems(version, path, recursion, false, false);
		}

		public int GetLatestVersion()
		{
			return SourceControlService.GetLatestChangeset(serverUrl, credentials);
		}

		public LogItem GetLog(string path,
							  int versionFrom,
							  int versionTo,
							  Recursion recursion,
							  int maxCount)
		{
			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}

			string serverPath = rootPath + path;
			RecursionType recursionType = RecursionType.None;
			switch (recursion)
			{
				case Recursion.OneLevel:
					recursionType = RecursionType.None;
					break;
				case Recursion.Full:
					recursionType = RecursionType.Full;
					break;
			}
			ChangesetVersionSpec changesetFrom = new ChangesetVersionSpec();
			changesetFrom.cs = versionFrom;

			ChangesetVersionSpec changesetTo = new ChangesetVersionSpec();
			changesetTo.cs = versionTo;

			LogItem logItem =
				SourceControlService.QueryLog(serverUrl,
											  credentials,
											  serverPath,
											  changesetFrom,
											  changesetTo,
											  recursionType,
											  maxCount);

			foreach (SourceItemHistory history in logItem.History)
			{
				foreach (SourceItemChange change in history.Changes)
				{
					change.Item.RemoteName = change.Item.RemoteName.Substring(rootPath.Length);
					if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
					{
						ItemMetaData oldItem = sourceControlHelper.GetItem(history.ChangeSetID - 1, change.Item.ItemId);
						change.Item = new RenamedSourceItem(change.Item, oldItem.Name, oldItem.Revision);
					}
					else if ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch)
					{
						ChangesetVersionSpec branchChangeset = new ChangesetVersionSpec();
						branchChangeset.cs = history.ChangeSetID;
						ItemSpec spec = new ItemSpec();
						spec.item = rootPath + change.Item.RemoteName;
						BranchRelative[][] branches =
							SourceControlService.QueryBranches(serverUrl,
															   credentials,
															   null,
															   new ItemSpec[] { spec },
															   branchChangeset);
						string oldName =
							branches[0][branches[0].GetUpperBound(0)].BranchFromItem.item.Substring(rootPath.Length);
						int oldRevision = change.Item.RemoteChangesetId - 1;
						change.Item = new RenamedSourceItem(change.Item, oldName, oldRevision);
					}
				}
			}

			return logItem;
		}

		public bool IsDirectory(int version,
								string path)
		{
			ItemMetaData item = GetItems(version, path, Recursion.None);
			if (item.ItemType == ItemType.Folder)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool ItemExists(string path)
		{
			return ItemExists(path, -1);
		}

		public bool ItemExists(string path,
							   int version)
		{
			ItemSpec spec = new ItemSpec();
			spec.item = rootPath + path;
			spec.recurse = RecursionType.None;

			VersionSpec versionSpec = VersionSpec.Latest;
			if (version != -1)
			{
				versionSpec = VersionSpec.FromChangeset(version);
			}

			ItemSet[] item =
				SourceControlService.QueryItems(serverUrl,
												credentials,
												null,
												null,
												new ItemSpec[1] { spec },
												versionSpec,
												DeletedState.NonDeleted,
												ItemType.Any,
												false);

			return (item[0].Items.Length > 0);
		}

		public void MakeActivity(string activityId)
		{
			ClearExistingTempWorkspaces();

			SourceControlService.CreateWorkspace(serverUrl, credentials, activityId, Constants.WorkspaceComment);
			string localPath = GetLocalPath(activityId, "");
			SourceControlService.AddWorkspaceMapping(serverUrl, credentials, activityId, rootPath, localPath);
			_activities[activityId] = new Activity();
		}

		private void ClearExistingTempWorkspaces()
		{
			WorkspaceInfo[] workspaces = SourceControlService.GetWorkspaces(serverUrl, credentials,WorkspaceComputers.ThisComputer);
			foreach (WorkspaceInfo workspace in workspaces)
			{
				if (workspace.Comment != Constants.WorkspaceComment)
					continue;
				SourceControlService.DeleteWorkspace(serverUrl, credentials, 
				                                     workspace.Name);
			}
		}

		public void MakeCollection(string activityId,
								   string path)
		{
			if (ItemExists(path))
			{
				throw new FolderAlreadyExistsException();
			}

			ItemMetaData item;
			string existingPath = path.Substring(1);
			do
			{
				if (existingPath.IndexOf('/') != -1)
				{
					existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
				}
				else
				{
					existingPath = "";
				}

				item = GetItems(-1, existingPath, Recursion.None);
			} while (item == null);
			string localPath = GetLocalPath(activityId, path);
			UpdateLocalVersion(activityId, item, localPath.Substring(0, localPath.LastIndexOf('\\')));

			List<PendRequest> pendRequests = new List<PendRequest>();
			pendRequests.Add(PendRequest.AddFolder(localPath));
			SourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
			_activities[activityId].MergeList.Add(
				new ActivityItem(rootPath + path, ItemType.Folder, ActivityItemAction.New));
			_activities[activityId].Collections.Add(path);
		}

		public MergeActivityResponse MergeActivity(string activityId)
		{
			UpdateProperties(activityId);

			List<string> commitServerList = new List<string>();
			Activity activity = _activities[activityId];
			foreach (ActivityItem item in activity.MergeList)
			{
				if (item.Action != ActivityItemAction.RenameDelete)
				{
					commitServerList.Add(item.Path);
				}
			}

			int changesetId;
			if (commitServerList.Count > 0)
			{
				changesetId =
					SourceControlService.Commit(serverUrl,
												credentials,
												activityId,
												_activities[activityId].Comment,
												commitServerList);
			}
			else
			{
				changesetId = GetLatestVersion();
			}

			if (_activities[activityId].PostCommitDeletedItems.Count > 0)
			{
				commitServerList.Clear();
				foreach (string path in _activities[activityId].PostCommitDeletedItems)
				{
					ProcessDeleteItem(activityId, path);
					commitServerList.Add(rootPath + path);
				}
				changesetId =
					SourceControlService.Commit(serverUrl,
												credentials,
												activityId,
												_activities[activityId].Comment,
												commitServerList);
			}
			AssociateWorkItemsWithChangeSet(activity.Comment, changesetId);
			return GenerateMergeResponse(activityId, changesetId);
		}

		public void AssociateWorkItemsWithChangeSet(string comment, int changesetId)
		{
			MatchCollection matches = associatedWorkItems.Matches(comment ?? "");
			foreach (Match match in matches)
			{
				Group group = match.Groups[1];
				string[] workItemIds = group.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string workItemId in workItemIds)
				{
					int id;
					if (int.TryParse(workItemId, out id) == false)
					{
						continue;
					}
					try
					{
						AssociateWorkItemWithChangeSet.Associate(id, changesetId);
						AssociateWorkItemWithChangeSet.SetWorkItemFixed(id);
					}
					catch (Exception e)
					{
						// we can't realy raise an error here, because 
						// we would fail the commit from the client side, while the changes
						// were already committed to the source control provider.
						// since we consider associating with work items nice but not essential,
						// we will log the error and ignore it.
						LogError("Failed to associate work item with changeset", e);
					}
				}
			}
		}

		private void LogError(string message, Exception exception)
		{
			Logger.Error(message, exception);
		}

		public byte[] ReadFile(ItemMetaData item)
		{
			byte[] bytes = FileCache.Get(item.Name, item.Revision);
			if (bytes != null)
			{
				return bytes;
			}

			byte[] downloadBytes = WebTransferService.DownloadBytes(item.DownloadUrl, credentials);
			FileCache.Set(item.Name, item.Revision, downloadBytes);
			return downloadBytes;
		}


		public void ReadFileAsync(ItemMetaData item)
		{
			byte[] bytes = FileCache.Get(item.Name, item.Revision);
			if (bytes != null)
			{
				item.Data = new FutureFile(delegate
				{
					return FileCache.Get(item.Name, item.Revision);
				});
				item.DataLoaded = true;
				return;
			}

			IAsyncResult asyncResult = WebTransferService.BeginDownloadBytes(item.DownloadUrl, credentials, delegate(IAsyncResult ar)
			{
				byte[] data = WebTransferService.EndDownloadBytes(ar);
				FileCache.Set(item.Name, item.Revision, data);
			});
			item.Data = new FutureFile(delegate
							   {
								   asyncResult.AsyncWaitHandle.WaitOne();
								   return FileCache.Get(item.Name, item.Revision);
							   });

			item.DataLoaded = true;
		}

		public Guid GetRepositoryUuid()
		{
			string cacheKey = "GetRepositoryUuid_"+serverUrl;
			CachedResult result = Cache.Get(cacheKey);
			if (result!=null)
				return (Guid)result.Value;
			Guid id = SourceControlService.GetRepositoryId(serverUrl, credentials);
			Cache.Set(cacheKey, id);
			return id;
		}

		public int GetVersionForDate(DateTime date)
		{
			DateVersionSpec dateVersion = new DateVersionSpec();
			dateVersion.date = date.ToUniversalTime();

			DateVersionSpec dateSpecForFirstVersion = GetDateSpecForVersion(1);

			if (dateVersion.date < dateSpecForFirstVersion.date)
				return 0; // the date is before the repository has started

			int latestVersion = GetLatestVersion();
			DateVersionSpec latestVersionDate = GetDateSpecForVersion(latestVersion);

			// if the required date is after the latest version, obviously 
			// the latest version is the nearest to it.
			if (latestVersionDate.date < dateVersion.date)
				return latestVersion;

			LogItem logDateToLatest = SourceControlService.QueryLog(
				ServerUrl,
				credentials,
				rootPath,
				dateVersion,
				latestVersionDate,
				RecursionType.Full,
				1);
			// get the change set before that one, which is the nearest changeset
			// to the requested date
			return logDateToLatest.History[0].ChangeSetID - 1;
		}

		private DateVersionSpec GetDateSpecForVersion(int version)
		{
			DateVersionSpec spec = new DateVersionSpec();
			int latestVersion = version;

			ChangesetVersionSpec fromVersion = new ChangesetVersionSpec();
			fromVersion.cs = latestVersion - 1;
			ChangesetVersionSpec toVersion = new ChangesetVersionSpec();
			toVersion.cs = latestVersion;
			LogItem log = SourceControlService.QueryLog(
				ServerUrl,
				credentials,
				rootPath,
				fromVersion,
				toVersion,
				RecursionType.Full,
				1);
			spec.date = log.History[0].CommitDateTime;
			return spec;
		}

		public void SetActivityComment(string activityId,
									   string comment)
		{
			_activities[activityId].Comment = comment;
		}

		public void SetProperty(string activityId,
								string path,
								string property,
								string value)
		{
			Activity activity = _activities[activityId];

			if (!activity.Properties.ContainsKey(path))
			{
				activity.Properties[path] = new Properties();
			}

			activity.Properties[path].Added[property] = value;
		}

		public void RemoveProperty(string activityId,
							string path,
							string property)
		{
			Activity activity = _activities[activityId];
			if (!activity.Properties.ContainsKey(path))
			{
				activity.Properties[path] = new Properties();
			}
			activity.Properties[path].Removed.Add(property);
		}

		public bool WriteFile(string activityId,
							  string path,
							  byte[] fileData)
		{
			return WriteFile(activityId, path, fileData, false);
		}

		#endregion

		private ItemMetaData GetItems(int version,
									  string path,
									  Recursion recursion,
									  bool returnPropertyFiles,
									  bool readItemsProperties)
		{
			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}

			Dictionary<string, ItemProperties> properties = new Dictionary<string, ItemProperties>();
			RecursionType recursionType = RecursionType.None;
			switch (recursion)
			{
				case Recursion.OneLevel:
					recursionType = RecursionType.OneLevel;
					break;
				case Recursion.Full:
					recursionType = RecursionType.Full;
					break;
			}
			VersionSpec versionSpec = new LatestVersionSpec();
			if (version != -1)
			{
				ChangesetVersionSpec changeSetVersionSpec = new ChangesetVersionSpec();
				changeSetVersionSpec.cs = version;
				versionSpec = changeSetVersionSpec;
			}
			SourceItem[] items =
				SourceControlService.QueryItems(serverUrl,
												credentials,
												rootPath + path,
												recursionType,
												versionSpec,
												DeletedState.NonDeleted,
												ItemType.Any);
			Dictionary<string, FolderMetaData> folders = new Dictionary<string, FolderMetaData>();
			Dictionary<string, int> itemPropertyRevision = new Dictionary<string, int>();
			ItemMetaData firstItem = null;
			for (int i = 0; i < items.Length; i++)
			{
				ItemMetaData item = sourceControlHelper.ConvertSourceItem(items[i]);
				if (recursion != Recursion.Full && readItemsProperties && !returnPropertyFiles)
				{
					if (Path.GetFileName(item.Name) != Constants.PropFolder)
					{
						RetrievePropertiesForItem(item);
					}
				}

				if (IsPropertyFile(item.Name) && !returnPropertyFiles)
				{
					string itemPath = item.Name.Replace("/" + Constants.PropFolder + "/" + Constants.FolderPropFile, "");
                    if (itemPath == Constants.PropFolder + "/" + Constants.FolderPropFile)
                    {
                        itemPath = "";
                    }
					itemPath = itemPath.Replace("/" + Constants.PropFolder + "/", "/");
                    if (itemPath.StartsWith(Constants.PropFolder + "/"))
                    {
                        itemPath = itemPath.Substring(Constants.PropFolder.Length + 1);
                    }
					ItemProperties itemProperties = Helper.DeserializeXml<ItemProperties>(ReadFile(item));
					properties[itemPath] = itemProperties;
					itemPropertyRevision[itemPath] = item.Revision;
				}
				else if (!IsPropertyFolder(item.Name) || item.ItemType != ItemType.Folder || returnPropertyFiles)
				{
					if (item.ItemType == ItemType.Folder)
					{
						folders[item.Name.ToLower()] = (FolderMetaData)item;
					}
					if (i == 0)
					{
						firstItem = item;
					}
					else
					{
						string folderName = "";
						if (item.Name.IndexOf('/') != -1)
						{
							folderName = GetFolderName(item.Name).ToLower();
						}
						folders[folderName].Items.Add(item);
					}
				}
			}
			SetItemProperties(folders, properties);
			UpdateItemRevisionsBasedOnPropertyItemRevisions(folders, itemPropertyRevision);
			return firstItem;
		}

        private bool IsPropertyFile(string name)
        {
            if (name.StartsWith(Constants.PropFolder + "/") || name.Contains("/" + Constants.PropFolder + "/"))
                return true;
            else
                return false;
        }

        private bool IsPropertyFolder(string name)
        {
            if (name == Constants.PropFolder || name.EndsWith("/" + Constants.PropFolder))
                return true;
            else
                return false;
        }

		private static void UpdateItemRevisionsBasedOnPropertyItemRevisions(IDictionary<string, FolderMetaData> folders,
																			IEnumerable<KeyValuePair<string, int>>
																				itemPropertyRevision)
		{
			foreach (KeyValuePair<string, int> propertyRevision in itemPropertyRevision)
			{
				if (folders.ContainsKey(propertyRevision.Key.ToLower()))
				{
					ItemMetaData item = folders[propertyRevision.Key.ToLower()];
					item.PropertyRevision = propertyRevision.Value;
				}
				else
				{
					string folderName = GetFolderName(propertyRevision.Key).ToLower();
					foreach (ItemMetaData folderItem in folders[folderName].Items)
					{
						if (folderItem.Name == propertyRevision.Key)
						{
							folderItem.PropertyRevision = propertyRevision.Value;
						}
					}
				}
			}
		}

		private bool RevertDelete(string activityId,
								  string path)
		{
			Activity activity = _activities[activityId];
			bool reverted = false;
			if (activity.DeletedItems.Contains(path))
			{
				SourceControlService.UndoPendingChanges(serverUrl,
														credentials,
														activityId,
														new string[] { rootPath + path });
				activity.DeletedItems.Remove(path);
				for (int j = activity.MergeList.Count - 1; j >= 0; j--)
				{
					if (activity.MergeList[j].Path == rootPath + path)
					{
						activity.MergeList.RemoveAt(j);
					}
				}

				reverted = true;
			}
			return reverted;
		}

		private void RetrievePropertiesForItem(ItemMetaData item)
		{
			int revision;
			ItemProperties properties = ReadPropertiesForItem(item.Name, item.ItemType, out revision);
			if (properties != null)
			{
				item.PropertyRevision = revision;
				foreach (Property property in properties.Properties)
				{
					item.Properties[property.Name] = property.Value;
				}
			}
		}

		private MergeActivityResponse GenerateMergeResponse(string activityId,
															int changesetId)
		{
			MergeActivityResponse mergeResponse = new MergeActivityResponse(changesetId, DateTime.Now, "unknown");
			List<string> baseFolders = new List<string>();
			List<string> sortedMergeResponse = new List<string>();
			foreach (ActivityItem item in _activities[activityId].MergeList)
			{
				ActivityItem newItem = item;
				if (!item.Path.EndsWith("/" + Constants.PropFolder))
				{
					if (item.Path.Contains("/" + Constants.PropFolder + "/"))
					{
						string path = item.Path.Replace("/" + Constants.PropFolder + "/", "/");
						ItemType newItemType = item.FileType;
						if (path.EndsWith("/" + Constants.FolderPropFile))
						{
							path = path.Replace("/" + Constants.FolderPropFile, "");
							newItemType = ItemType.Folder;
						}
						newItem = new ActivityItem(path, newItemType, item.Action);
					}

					if (!sortedMergeResponse.Contains(newItem.Path))
					{
						sortedMergeResponse.Add(newItem.Path);

                        string path = newItem.Path.Substring(rootPath.Length);
                        if (path == "")
                            path = "/";

						MergeActivityResponseItem responseItem =
							new MergeActivityResponseItem(newItem.FileType, path);
						if (newItem.Action != ActivityItemAction.Deleted && newItem.Action != ActivityItemAction.Branch &&
							newItem.Action != ActivityItemAction.RenameDelete)
						{
							mergeResponse.Items.Add(responseItem);
						}

						AddBaseFolderIfRequired(activityId, newItem, baseFolders, mergeResponse);
					}
				}
			}
			return mergeResponse;
		}

		private void AddBaseFolderIfRequired(string activityId,
											 ActivityItem item,
											 ICollection<string> baseFolders,
											 MergeActivityResponse mergeResponse)
		{
			string folderName = GetFolderName(item.Path);
			if (((item.Action == ActivityItemAction.New) || (item.Action == ActivityItemAction.Deleted) ||
				 (item.Action == ActivityItemAction.RenameDelete)) && !baseFolders.Contains(folderName))
			{
				baseFolders.Add(folderName);
				bool folderFound = false;
				foreach (ActivityItem folderItem in _activities[activityId].MergeList)
				{
					if (folderItem.FileType == ItemType.Folder && folderItem.Path == folderName)
					{
						folderFound = true;
					}
				}

				if (!folderFound)
				{
                    folderName = GetFolderName(item.Path.Substring(rootPath.Length));
                    if (folderName == "")
                        folderName = "/";
					MergeActivityResponseItem responseItem = new MergeActivityResponseItem(ItemType.Folder, folderName);
					mergeResponse.Items.Add(responseItem);
				}
			}
		}

		private bool WriteFile(string activityId,
							   string path,
							   byte[] fileData,
							   bool reportUpdatedFile)
		{
			bool replaced = RevertDelete(activityId, path);

			Activity activity = _activities[activityId];
			ItemMetaData item;
			string existingPath = path.Substring(1);

			do
			{
				int lastIndexOf = existingPath.LastIndexOf('/');
				if (lastIndexOf != -1)
					existingPath = existingPath.Substring(0, lastIndexOf);
				else
					existingPath = "";

				item = GetItems(-1, existingPath, Recursion.None, true, false);
			} while (item == null);

			string localPath = GetLocalPath(activityId, path);
			List<LocalUpdate> updates = new List<LocalUpdate>();
			updates.Add(LocalUpdate.FromLocal(item.Id,
											  localPath.Substring(0, localPath.LastIndexOf('\\')),
											  item.Revision));

			item = GetItems(-1, path.Substring(1), Recursion.None, true, false);
			if (item != null)
			{
				updates.Add(LocalUpdate.FromLocal(item.Id, localPath, item.Revision));
			}

			SourceControlService.UpdateLocalVersions(serverUrl, credentials, activityId, updates);

			List<PendRequest> pendRequests = new List<PendRequest>();

			bool newFile = true;
			bool addToMergeList = true;
			if (item != null)
			{
				pendRequests.Add(PendRequest.Edit(localPath));
				newFile = false;
			}
			else
			{
				ItemMetaData pendingItem = GetPendingItem(activityId, path);
				if (pendingItem == null)
				{
					pendRequests.Add(PendRequest.AddFile(localPath, TfsUtil.CodePage_ANSI));
				}
				else
				{
					UpdateLocalVersion(activityId, pendingItem, localPath);
					pendRequests.Add(PendRequest.Edit(localPath));
					newFile = false;
				}
				foreach (CopyAction copy in activity.CopiedItems)
				{
					if (copy.TargetPath == path)
					{
						addToMergeList = false;
					}
				}
			}

			SourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
			SourceControlService.UploadFileFromBytes(serverUrl, credentials, activityId, fileData, rootPath + path);

			if (addToMergeList)
			{
				if (!replaced && (!newFile || reportUpdatedFile))
				{
					activity.MergeList.Add(new ActivityItem(rootPath + path, ItemType.File, ActivityItemAction.Updated));
				}
				else
				{
					activity.MergeList.Add(new ActivityItem(rootPath + path, ItemType.File, ActivityItemAction.New));
				}
			}

			return newFile;
		}

		private void ConvertCopyToRename(string activityId,
										 CopyAction copy)
		{
			Activity activity = _activities[activityId];

			SourceControlService.UndoPendingChanges(serverUrl,
													credentials,
													activityId,
													new string[] { rootPath + copy.TargetPath });
			for (int i = activity.MergeList.Count - 1; i >= 0; i--)
			{
				if (activity.MergeList[i].Path == rootPath + copy.TargetPath)
				{
					activity.MergeList.RemoveAt(i);
				}
			}

			ProcessCopyItem(activityId, copy, true);
		}

		private static string GetLocalPath(string activityId,
										   string path)
		{
			return Constants.LocalPrefix + activityId + path.Replace('/', '\\');
		}

		private void UpdateLocalVersion(string activityId,
										ItemMetaData item,
										string localPath)
		{
			UpdateLocalVersion(activityId, item.Id, item.ItemRevision, localPath);
		}

		private void UpdateLocalVersion(string activityId,
										int itemId,
										int itemRevision,
										string localPath)
		{
			List<LocalUpdate> updates = new List<LocalUpdate>();
			updates.Add(LocalUpdate.FromLocal(itemId, localPath, itemRevision));
			SourceControlService.UpdateLocalVersions(serverUrl, credentials, activityId, updates);
		}

		private void ProcessCopyItem(string activityId,
									 CopyAction copyAction,
									 bool forceRename)
		{
			Activity activity = _activities[activityId];
			string localPath = GetLocalPath(activityId, copyAction.Path);
			string localTargetPath = GetLocalPath(activityId, copyAction.TargetPath);

			bool copyIsRename = RevertDelete(activityId, copyAction.Path);
			ItemMetaData item = GetItems(-1, copyAction.Path, Recursion.None);
			UpdateLocalVersion(activityId, item, localPath);

			if (copyIsRename)
			{
				activity.MergeList.Add(
					new ActivityItem(rootPath + copyAction.Path, item.ItemType, ActivityItemAction.RenameDelete));
			}

			if (!copyIsRename)
			{
				foreach (CopyAction copy in activity.CopiedItems)
				{
					if (copyAction.Path.StartsWith(copy.Path + "/"))
					{
						string path = copy.TargetPath + copyAction.Path.Substring(copy.Path.Length);
						for (int i = activity.DeletedItems.Count - 1; i >= 0; i--)
						{
							if (activity.DeletedItems[i] == path)
							{
								copyIsRename = true;
								SourceControlService.UndoPendingChanges(serverUrl,
																		credentials,
																		activityId,
																		new string[] { rootPath + activity.DeletedItems[i] });
								for (int j = activity.MergeList.Count - 1; j >= 0; j--)
								{
									if (activity.MergeList[j].Path == rootPath + activity.DeletedItems[i])
									{
										activity.MergeList.RemoveAt(j);
									}
								}

								activity.DeletedItems.RemoveAt(i);
								localPath = GetLocalPath(activityId, path);
								ItemMetaData pendingItem = GetPendingItem(activityId, path);
								UpdateLocalVersion(activityId, pendingItem, localPath);
							}
						}
					}
				}
			}
			if (!copyIsRename)
			{
				for (int i = activity.DeletedItems.Count - 1; i >= 0; i--)
				{
					if (copyAction.Path.StartsWith(activity.DeletedItems[i] + "/"))
					{
						copyIsRename = true;
						activity.PostCommitDeletedItems.Add(activity.DeletedItems[i]);
						SourceControlService.UndoPendingChanges(serverUrl,
																credentials,
																activityId,
																new string[] { rootPath + activity.DeletedItems[i] });
						for (int j = activity.MergeList.Count - 1; j >= 0; j--)
						{
							if (activity.MergeList[j].Path == rootPath + activity.DeletedItems[i])
							{
								activity.MergeList.RemoveAt(j);
							}
						}

						activity.DeletedItems.RemoveAt(i);
					}
				}
			}
			if (!copyIsRename)
			{
				foreach (string deletedItem in activity.PostCommitDeletedItems)
				{
					if (copyAction.Path.StartsWith(deletedItem + "/"))
					{
						copyIsRename = true;
					}
				}
			}

			List<PendRequest> pendRequests = new List<PendRequest>();
			if (copyIsRename || forceRename)
			{
				pendRequests.Add(PendRequest.Rename(localPath, localTargetPath));
				copyAction.Rename = true;
			}
			else
			{
				pendRequests.Add(PendRequest.Copy(localPath, localTargetPath));
			}

			SourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
			if (copyAction.Rename)
			{
				activity.MergeList.Add(
					new ActivityItem(rootPath + copyAction.TargetPath, item.ItemType, ActivityItemAction.New));
			}
			else
			{
				activity.MergeList.Add(
					new ActivityItem(rootPath + copyAction.TargetPath, item.ItemType, ActivityItemAction.Branch));
			}
		}

		private static string GetPropertiesFolderName(string path,
													  ItemType itemType)
		{
			if (itemType == ItemType.Folder)
			{
                if (path == "/")
                    return "/" + Constants.PropFolder;
                else
				    return path + "/" + Constants.PropFolder;
			}
			else if (path.LastIndexOf('/') != -1)
			{
				return path.Substring(0, path.LastIndexOf('/')) + "/" + Constants.PropFolder;
			}
			else
			{
				return Constants.PropFolder;
			}
		}

		private static string GetPropertiesFileName(string path,
													ItemType itemType)
		{
			if (itemType == ItemType.Folder)
			{
                if (path == "/")
                    return "/" + Constants.PropFolder + "/" + Constants.FolderPropFile;
                else
				    return path + "/" + Constants.PropFolder + "/" + Constants.FolderPropFile;
			}
			else if (path.LastIndexOf('/') != -1)
			{
				return
					path.Substring(0, path.LastIndexOf('/')) + "/" + Constants.PropFolder +
					path.Substring(path.LastIndexOf('/'));
			}
			else
			{
				return Constants.PropFolder + "/" + path;
			}
		}

		private void ProcessDeleteItem(string activityId,
									   string path)
		{
			Activity activity = _activities[activityId];
			string localPath = GetLocalPath(activityId, path);

			ItemMetaData item = GetItems(-1, path, Recursion.None, true, false);
			if (item == null)
			{
				item = GetPendingItem(activityId, path);
			}

			UpdateLocalVersion(activityId, item, localPath);

			if (item.ItemType != ItemType.Folder)
			{
				string propertiesFile = GetPropertiesFileName(path, item.ItemType);
				DeleteItem(activityId, propertiesFile);
			}

			List<PendRequest> pendRequests = new List<PendRequest>();
			pendRequests.Add(PendRequest.Delete(localPath));
			SourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);

			activity.MergeList.Add(new ActivityItem(rootPath + path, item.ItemType, ActivityItemAction.Deleted));
		}

		private ItemProperties ReadPropertiesForItem(string path,
													 ItemType itemType)
		{
			int revision;
			return ReadPropertiesForItem(path, itemType, out revision);
		}

		private ItemProperties ReadPropertiesForItem(string path,
													 ItemType itemType,
													 out int revision)
		{
			revision = -1;
			ItemProperties properties = null;
			string propertiesPath = GetPropertiesFileName(path, itemType);
			string cacheKey = "ReadPropertiesForItem_" + propertiesPath;
			ItemMetaData item;
			CachedResult cachedResult = Cache.Get(cacheKey);

			if (cachedResult == null)
			{
				item = GetItems(-1, propertiesPath, Recursion.None, true, false);
				Cache.Set(cacheKey, item);
			}
			else
			{
				item = (ItemMetaData)cachedResult.Value;
			}

			if (item != null)
			{
				properties = Helper.DeserializeXml<ItemProperties>(ReadFile(item));
				revision = item.Revision;
			}
			return properties;
		}

		private void UpdateProperties(string activityId)
		{
			Activity activity = _activities[activityId];
			ItemMetaData item;
			ItemType itemType;

			foreach (string path in activity.Properties.Keys)
			{
				ItemProperties properties = GetItemProperties(activity, path, out item, out itemType);

				foreach (KeyValuePair<string, string> property in activity.Properties[path].Added)
				{
					bool found = false;
					foreach (Property currentProperty in properties.Properties)
					{
						if (currentProperty.Name == property.Key)
						{
							currentProperty.Value = property.Value;
							found = true;
						}
					}
					if (!found)
					{
						properties.Properties.Add(new Property(property.Key, property.Value));
					}
				}

				properties.Properties.RemoveAll(delegate(Property obj)
				{
					return activity.Properties[path].Removed.Contains(obj.Name);
				});

				string propertiesPath = GetPropertiesFileName(path, itemType);
				string propertiesFolder = GetPropertiesFolderName(path, itemType);
				ItemMetaData propertiesFolderItem = GetItems(-1, propertiesFolder, Recursion.None, true, false);
				if ((propertiesFolderItem == null) && !activity.Collections.Contains(propertiesFolder))
				{
					MakeCollection(activityId, propertiesFolder);
				}

				if (item != null)
				{
					WriteFile(activityId, propertiesPath, Helper.SerializeXml(properties), true);
				}
				else
				{
					WriteFile(activityId, propertiesPath, Helper.SerializeXml(properties));
				}
			}
		}

		private ItemProperties GetItemProperties(Activity activity, string path, out ItemMetaData item, out ItemType itemType)
		{
			itemType = ItemType.File;
			item = GetItems(-1, path, Recursion.None);
			if (item != null)
			{
				itemType = item.ItemType;
			}
			else if (activity.Collections.Contains(path))
			{
				itemType = ItemType.Folder;
			}

			ItemProperties properties = ReadPropertiesForItem(path, itemType);
			if (properties == null)
			{
				properties = new ItemProperties();
			}
			return properties;
		}

		private static string GetFolderName(string path)
		{
            if (path.Contains("/"))
                return path.Substring(0, path.LastIndexOf('/'));
            else
                return "";
        }

		private ItemMetaData GetPendingItem(string activityId,
											string path)
		{
			ItemSpec spec = new ItemSpec();
			spec.item = rootPath + path;
			ExtendedItem[][] items =
				SourceControlService.QueryItemsExtended(serverUrl,
														credentials,
														activityId,
														new ItemSpec[1] { spec },
														DeletedState.NonDeleted,
														ItemType.Any);
			if (items[0].Length == 0)
			{
				return null;
			}
			else
			{
				ItemMetaData pendingItem = new ItemMetaData();
				if (items[0][0].type == ItemType.Folder)
				{
					pendingItem = new FolderMetaData();
				}

				pendingItem.Id = items[0][0].itemid;
				pendingItem.ItemRevision = items[0][0].latest;
				return pendingItem;
			}
		}


		private void SetItemProperties(IDictionary<string, FolderMetaData> folders,
									   IEnumerable<KeyValuePair<string, ItemProperties>> properties)
		{
			foreach (KeyValuePair<string, ItemProperties> itemProperties in properties)
			{
				ItemMetaData item;
				if (folders.ContainsKey(itemProperties.Key.ToLowerInvariant()))
				{
					item = folders[itemProperties.Key.ToLowerInvariant()];
				}
				else
				{
					string folderName = GetFolderName(itemProperties.Key);
					item = sourceControlHelper.FindItem(folders[folderName.ToLowerInvariant()], itemProperties.Key);
				}
				if (item != null)
					foreach (Property property in itemProperties.Value.Properties)
						item.Properties[property.Name] = property.Value;
			}
		}

		public ICredentials GetCredentials()
		{
			return credentials;
		}
	}
}