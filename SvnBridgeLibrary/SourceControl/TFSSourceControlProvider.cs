using System.Collections;
using System.Net.Sockets;
using CodePlex.TfsLibrary;
using SvnBridge.Net;

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
    using System.Threading;

    [Interceptor(typeof(TracingInterceptor))]
    [Interceptor(typeof(RetryOnExceptionsInterceptor<SocketException>))]
    public class TFSSourceControlProvider : ISourceControlProvider, ICredentialsProvider
    {
        private readonly ISourceControlServicesHub sourceControlServicesHub;

        private static readonly Regex associatedWorkItems =
            new Regex(@"Work ?Items?: (.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

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

        private IMetaDataRepository MetaDataRepository
        {
            get
            {
                return sourceControlServicesHub.MetaDataRepositoryFactory
                    .Create(GetCredentials(), ServerUrl, rootPath);
            }
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
            ICredentials credentials,
            ISourceControlServicesHub sourceControlServicesHub)
        {
            this.sourceControlServicesHub = sourceControlServicesHub;
            this.credentials = credentials;

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
            this.credentials = CredentialsHelper.GetCredentialsForServer(this.serverUrl, sourceControlServicesHub.Credentials);
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
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                activity.CopiedItems.Add(copyAction);
            });
            ProcessCopyItem(activityId, copyAction, false);
        }

        public void DeleteActivity(string activityId)
        {
            SourceControlService.DeleteWorkspace(serverUrl, credentials, activityId);
            ActivityRepository.Delete(activityId);
        }

        public bool DeleteItem(string activityId,
                               string path)
        {
            if ((GetItems(-1, path, Recursion.None, true) == null) && (GetPendingItem(activityId, path) == null))
            {
                return false;
            }

            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
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
            });
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
            if (root == null)
            {
                throw new FileNotFoundException(path);
            }

            UpdateDiffCalculator udc = new UpdateDiffCalculator(this);
            udc.CalculateDiff(path, versionTo, versionFrom, root, reportData);

            return root;
        }


        public ItemMetaData GetItemInActivity(string activityId,
                                              string path)
        {

            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                foreach (CopyAction copy in activity.CopiedItems)
                {
                    if (path.StartsWith(copy.TargetPath))
                    {
                        path = copy.Path + path.Substring(copy.TargetPath.Length);
                    }
                }
            });
            return GetItemsWithoutProperties(-1, path, Recursion.None);
        }

        public ItemMetaData GetItems(int version,
                                     string path,
                                     Recursion recursion)
        {
            return GetItems(version, path, recursion, false);
        }

        public ItemMetaData GetItemsWithoutProperties(int version,
                                                      string path,
                                                      Recursion recursion)
        {
            return GetItems(version, path, recursion, false);
        }

        /// <summary>
        /// We are caching the value, to avoid expensive remote calls. 
        /// This is safe to do because <see cref="TFSSourceControlProvider"/> is a trasient
        /// type, and will only live for the current request.
        /// </summary>
        /// <returns></returns>
        public int GetLatestVersion()
        {
            const string latestVersion = "Repository.Latest.Version";
            if (PerRequest.Items[latestVersion] != null)
                return (int)PerRequest.Items[latestVersion];
            int changeset = SourceControlService.GetLatestChangeset(serverUrl, credentials);
            PerRequest.Items[latestVersion] = changeset;
            return changeset;
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

            LogItem logItem = GetLogItem(serverPath, versionFrom, versionTo, recursionType, maxCount);

            foreach (SourceItemHistory history in logItem.History)
            {
                List<SourceItem> renamedItems = new List<SourceItem>();
                foreach (SourceItemChange change in history.Changes)
                {
                    change.Item.RemoteName = change.Item.RemoteName.Substring(rootPath.Length);
                    if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                    {
                        renamedItems.Add(change.Item);
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
                if (renamedItems.Count > 0)
                {
                    ItemMetaData[] oldItems = GetPreviousVersionOfItems(renamedItems.ToArray(), history.ChangeSetID);
                    Dictionary<int, ItemMetaData> oldItemsByKey = new Dictionary<int, ItemMetaData>();
                    foreach (ItemMetaData oldItem in oldItems)
                    {
                        oldItemsByKey[oldItem.Id] = oldItem;
                    }

                    foreach (SourceItemChange change in history.Changes)
                    {
                        ItemMetaData oldItem;
                        if (oldItemsByKey.TryGetValue(change.Item.ItemId, out oldItem))
                        {
                            change.Item = new RenamedSourceItem(change.Item, oldItem.Name, oldItem.Revision);
                        }
                    }
                }
            }

            return logItem;
        }

        private LogItem GetLogItem(string serverPath, int versionFrom, int versionTo, RecursionType recursionType, int maxCount)
        {
            LogItem log = SourceControlService.QueryLog(serverUrl,
                                                        credentials,
                                                        serverPath,
                                                        new ChangesetVersionSpec { cs = versionFrom },
                                                        new ChangesetVersionSpec { cs = versionTo },
                                                        recursionType,
                                                        maxCount);
            const int queryLimit = 256;
            if (maxCount > queryLimit)
            {
                var histories = new List<SourceItemHistory>(log.History);
                // we might have remaining items
                int logItemsCount = log.History.Length;
                while (logItemsCount == queryLimit)
                {
                    int earliestVersionFound = log.History[queryLimit - 1].ChangeSetID - 1;
                    if (earliestVersionFound == versionFrom)
                        break;
                    LogItem temp = SourceControlService.QueryLog(serverUrl,
                                                        credentials,
                                                        serverPath,
                                                        new ChangesetVersionSpec { cs = versionFrom },
                                                        new ChangesetVersionSpec { cs = earliestVersionFound },
                                                        recursionType,
                                                        maxCount);
                    histories.AddRange(temp.History);
                    logItemsCount = temp.History.Length;
                }
                log.History = histories.ToArray();
            }
            return log;
        }

        public bool IsDirectory(int version,
                                string path)
        {
            ItemMetaData item = GetItemsWithoutProperties(version, path, Recursion.None);
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
            if (version == -1)
                version = GetLatestVersion();

            SourceItem[] items = MetaDataRepository
                .QueryItems(version, path, Recursion.None);

            return (items.Length > 0);
        }

        public void MakeActivity(string activityId)
        {
            ClearExistingTempWorkspaces(true);

            SourceControlService.CreateWorkspace(serverUrl, credentials, activityId, Constants.WorkspaceComment);
            string localPath = GetLocalPath(activityId, "");
            SourceControlService.AddWorkspaceMapping(serverUrl, credentials, activityId, rootPath, localPath);
            ActivityRepository.Create(activityId);
        }

        private void ClearExistingTempWorkspaces(bool skipExistingActivities)
        {
            WorkspaceInfo[] workspaces = SourceControlService.GetWorkspaces(serverUrl, credentials,
                                                                            WorkspaceComputers.ThisComputer);
            foreach (WorkspaceInfo workspace in workspaces)
            {
                if (workspace.Comment != Constants.WorkspaceComment)
                    continue;
                if (skipExistingActivities && ActivityRepository.Exists(workspace.Name))
                    continue;
                SourceControlService.DeleteWorkspace(serverUrl, credentials,
                                                     workspace.Name);
                ActivityRepository.Delete(workspace.Name);
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

                item = GetItemsWithoutProperties(-1, existingPath, Recursion.None);
            } while (item == null);
            string localPath = GetLocalPath(activityId, path);
            UpdateLocalVersion(activityId, item, localPath.Substring(0, localPath.LastIndexOf('\\')));

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.AddFolder(localPath));
            SourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                activity.MergeList.Add(
                    new ActivityItem(rootPath + path, ItemType.Folder, ActivityItemAction.New));
                activity.Collections.Add(path);
            });

        }

        public MergeActivityResponse MergeActivity(string activityId)
        {
            MergeActivityResponse response = null;
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                UpdateProperties(activityId);
                List<string> commitServerList = new List<string>();
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
                    try
                    {
                        changesetId =
                            SourceControlService.Commit(serverUrl,
                                                        credentials,
                                                        activityId,
                                                        activity.Comment,
                                                        commitServerList);
                    }
                    catch (TfsFailureException)
                    {
                        // we just failed a commit, this tends to happen when we have a conflicts 
                        // between previously partially commited changes and the current changes.
                        // We will wipe all the user's temporary workspaces and allow the user to 
                        // try again
                        ClearExistingTempWorkspaces(false);

                        throw;
                    }
                }
                else
                {
                    changesetId = GetLatestVersion();
                }

                if (activity.PostCommitDeletedItems.Count > 0)
                {
                    commitServerList.Clear();
                    foreach (string path in activity.PostCommitDeletedItems)
                    {
                        ProcessDeleteItem(activityId, path);
                        commitServerList.Add(rootPath + path);
                    }
                    changesetId =
                        SourceControlService.Commit(serverUrl,
                                                    credentials,
                                                    activityId,
                                                    activity.Comment,
                                                    commitServerList);
                }
                AssociateWorkItemsWithChangeSet(activity.Comment, changesetId);
                response = GenerateMergeResponse(activityId, changesetId);
            });

            return response;
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
                                                   return FileCache.GetText(item.Name, item.Revision);
                                               });
                item.DataLoaded = true;
                return;
            }
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            DownloadFileAsync(item, resetEvent, 0);
            item.Data = new FutureFile(delegate
            {
                return GetFileData(resetEvent, item);
            });

            item.DataLoaded = true;
        }

        private void DownloadFileAsync(ItemMetaData item, EventWaitHandle waitHandle, int retry)
        {
            Logger.Trace("Starting to download {0}", item.Name);
            WebTransferService.BeginDownloadBytes(item.DownloadUrl, credentials, delegate(IAsyncResult ar)
            {
                try
                {
                    byte[] data = WebTransferService.EndDownloadBytes(ar);
                    FileCache.Set(item.Name, item.Revision, data);
                    Logger.Trace("Finished downloading {0}", item.Name);
                    waitHandle.Set();
                }
                catch (UnauthorizedAccessException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    retry = retry + 1;
                    if (retry == 3)
                    {
                        Logger.Error("Failed to download " + item.Name + ", max retry count reached, aborting", e);
                        Listener.RaiseErrorOccured(e);
                        waitHandle.Set();
                        throw;
                    }
                    Logger.Error("Failed to download " + item.Name + " retry #" + retry, e);
                    DownloadFileAsync(item, waitHandle, retry);
                }
            });
        }

        private FileData GetFileData(WaitHandle resetEvent, ItemMetaData item)
        {
            resetEvent.WaitOne();
            resetEvent.Close();
            FileData results = FileCache.GetText(item.Name, item.Revision);
            if (results == null)
                throw new CacheMissException(item.Name);
            return results;
        }

        public Guid GetRepositoryUuid()
        {
            string cacheKey = "GetRepositoryUuid_" + serverUrl;
            CachedResult result = Cache.Get(cacheKey);
            if (result != null)
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
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                activity.Comment = comment;
            });
        }

        public void SetProperty(string activityId,
                                string path,
                                string property,
                                string value)
        {
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                if (!activity.Properties.ContainsKey(path))
                {
                    activity.Properties[path] = new Properties();
                }

                activity.Properties[path].Added[property] = value;
            });
        }

        public void RemoveProperty(string activityId,
                                   string path,
                                   string property)
        {
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                if (!activity.Properties.ContainsKey(path))
                {
                    activity.Properties[path] = new Properties();
                }
                activity.Properties[path].Removed.Add(property);
            });
        }

        public bool WriteFile(string activityId,
                              string path,
                              byte[] fileData)
        {
            return WriteFile(activityId, path, fileData, false);
        }

        #endregion

        private ItemMetaData GetItems(int version, string path, Recursion recursion, bool returnPropertyFiles)
        {
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            if (version == -1)
            {
                version = GetLatestVersion();
            }

            SourceItem[] items = MetaDataRepository.QueryItems(version, path, recursion);

            Dictionary<string, FolderMetaData> folders = new Dictionary<string, FolderMetaData>();
            Dictionary<string, ItemProperties> properties = new Dictionary<string, ItemProperties>();
            Dictionary<string, int> itemPropertyRevision = new Dictionary<string, int>();
            ItemMetaData firstItem = null;
            foreach (SourceItem sourceItem in items)
            {
                ItemMetaData item = ItemMetaData.ConvertSourceItem(sourceItem, rootPath);
                if (recursion == Recursion.Full && IsPropertyFile(item.Name) && !returnPropertyFiles)
                {
                    string itemPath = GetItemFileNameFromPropertiesFileName(item.Name);
                    itemPropertyRevision[itemPath] = item.Revision;
                    properties[itemPath] = Helper.DeserializeXml<ItemProperties>(ReadFile(item));
                }
                if ((!IsPropertyFile(item.Name) && !IsPropertyFolder(item.Name)) || returnPropertyFiles)
                {
                    if (recursion != Recursion.Full && !returnPropertyFiles)
                    {
                        RetrievePropertiesForItem(item);
                    }
                    if (item.ItemType == ItemType.Folder)
                    {
                        folders[item.Name.ToLower()] = (FolderMetaData)item;
                    }
                    if (firstItem == null)
                    {
                        firstItem = item;
                    }
                    else
                    {
                        string folderName = GetFolderName(item.Name);
                        folders[folderName.ToLower()].Items.Add(item);
                    }
                }
            }
            SetItemProperties(folders, properties);
            UpdateItemRevisionsBasedOnPropertyItemRevisions(folders, itemPropertyRevision);
            return firstItem;
        }

        private static bool IsPropertyFile(string name)
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
                    string folderName = GetFolderName(propertyRevision.Key).ToLowerInvariant();

                    FolderMetaData folder;
                    if (folders.TryGetValue(folderName, out folder) == false)
                        continue;

                    foreach (ItemMetaData folderItem in folder.Items)
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
            bool reverted = false;
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
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
            });
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
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                foreach (ActivityItem item in activity.MergeList)
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
            });
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

                ActivityRepository.Use(activityId, delegate(Activity activity)
                {
                    foreach (ActivityItem folderItem in activity.MergeList)
                    {
                        if (folderItem.FileType == ItemType.Folder && folderItem.Path == folderName)
                        {
                            folderFound = true;
                        }
                    }
                });

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
            bool newFile = true;

            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                ItemMetaData item;
                string existingPath = path.Substring(1);

                do
                {
                    int lastIndexOf = existingPath.LastIndexOf('/');
                    if (lastIndexOf != -1)
                        existingPath = existingPath.Substring(0, lastIndexOf);
                    else
                        existingPath = "";

                    item = GetItems(-1, existingPath, Recursion.None, true);
                } while (item == null);

                string localPath = GetLocalPath(activityId, path);
                List<LocalUpdate> updates = new List<LocalUpdate>();
                updates.Add(LocalUpdate.FromLocal(item.Id,
                                                  localPath.Substring(0, localPath.LastIndexOf('\\')),
                                                  item.Revision));

                item = GetItems(-1, path.Substring(1), Recursion.None, true);
                if (item != null)
                {
                    updates.Add(LocalUpdate.FromLocal(item.Id, localPath, item.Revision));
                }

                SourceControlService.UpdateLocalVersions(serverUrl, credentials, activityId, updates);

                List<PendRequest> pendRequests = new List<PendRequest>();

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
            });

            return newFile;
        }

        private void ConvertCopyToRename(string activityId,
                                         CopyAction copy)
        {

            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
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
            });
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
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                string localPath = GetLocalPath(activityId, copyAction.Path);
                string localTargetPath = GetLocalPath(activityId, copyAction.TargetPath);

                bool copyIsRename = RevertDelete(activityId, copyAction.Path);
                ItemMetaData item = GetItemsWithoutProperties(-1, copyAction.Path, Recursion.None);
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
            });
        }

        private static string GetPropertiesFolderName(string path,
                                                      ItemType itemType)
        {
            if (itemType == ItemType.Folder)
            {
                if (path == "/")
                    return "/" + Constants.PropFolder;
                return path + "/" + Constants.PropFolder;
            }
            if (path.LastIndexOf('/') != -1)
                return path.Substring(0, path.LastIndexOf('/')) + "/" + Constants.PropFolder;
            return Constants.PropFolder;
        }

        private static string GetItemFileNameFromPropertiesFileName(string path)
        {
            string itemPath = path;
            if (itemPath == Constants.PropFolder + "/" + Constants.FolderPropFile)
            {
                itemPath = "";
            }
            else if (itemPath.StartsWith(Constants.PropFolder + "/"))
            {
                itemPath = path.Substring(Constants.PropFolder.Length + 1);
            }
            else
            {
                itemPath = itemPath.Replace("/" + Constants.PropFolder + "/" + Constants.FolderPropFile, "");
                itemPath = itemPath.Replace("/" + Constants.PropFolder + "/", "/");
            }
            return itemPath;
        }

        private static string GetPropertiesFileName(string path,
                                                    ItemType itemType)
        {
            if (itemType == ItemType.Folder)
            {
                if (path == "/")
                    return "/" + Constants.PropFolder + "/" + Constants.FolderPropFile;
                return path + "/" + Constants.PropFolder + "/" + Constants.FolderPropFile;
            }
            if (path.LastIndexOf('/') != -1)
            {
                return
                    path.Substring(0, path.LastIndexOf('/')) + "/" + Constants.PropFolder +
                    path.Substring(path.LastIndexOf('/'));
            }
            return Constants.PropFolder + "/" + path;
        }

        private void ProcessDeleteItem(string activityId,
                                       string path)
        {
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
                string localPath = GetLocalPath(activityId, path);

                ItemMetaData item = GetItems(-1, path, Recursion.None, true);
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

            });
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
                item = GetItems(-1, propertiesPath, Recursion.None, true);
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
            ActivityRepository.Use(activityId, delegate(Activity activity)
            {
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

                    properties.Properties.RemoveAll(
                        delegate(Property obj) { return activity.Properties[path].Removed.Contains(obj.Name); });

                    string propertiesPath = GetPropertiesFileName(path, itemType);
                    string propertiesFolder = GetPropertiesFolderName(path, itemType);
                    ItemMetaData propertiesFolderItem = GetItems(-1, propertiesFolder, Recursion.None, true);
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
            });
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
                return null;
            ItemMetaData pendingItem = new ItemMetaData();
            if (items[0][0].type == ItemType.Folder)
            {
                pendingItem = new FolderMetaData();
            }

            pendingItem.Id = items[0][0].itemid;
            pendingItem.ItemRevision = items[0][0].latest;
            return pendingItem;
        }


        private void SetItemProperties(IDictionary<string, FolderMetaData> folders,
                                       IEnumerable<KeyValuePair<string, ItemProperties>> properties)
        {
            foreach (KeyValuePair<string, ItemProperties> itemProperties in properties)
            {
                ItemMetaData item = null;
                if (folders.ContainsKey(itemProperties.Key.ToLowerInvariant()))
                {
                    item = folders[itemProperties.Key.ToLowerInvariant()];
                }
                else
                {
                    string folderName = GetFolderName(itemProperties.Key)
                        .ToLowerInvariant();
                    if (folders.ContainsKey(folderName))
                    {
                        item = folders[folderName].FindItem(itemProperties.Key);
                    }
                }
                if (item != null)
                {
                    foreach (Property property in itemProperties.Value.Properties)
                    {
                        item.Properties[property.Name] = property.Value;
                    }
                }
            }
        }

        public ICredentials GetCredentials()
        {
            return credentials;
        }

        public ItemMetaData[] GetPreviousVersionOfItems(SourceItem[] items, int changeset)
        {
            int previousRevision = (changeset - 1);

            List<int> itemIds = new List<int>();
            foreach (SourceItem item in items)
                itemIds.Add(item.ItemId);

            SourceItem[] sourceItems = SourceControlService.QueryItems(serverUrl, credentials, itemIds.ToArray(), previousRevision);

            List<ItemMetaData> result = new List<ItemMetaData>();
            foreach (SourceItem sourceItem in sourceItems)
                result.Add(ItemMetaData.ConvertSourceItem(sourceItem, rootPath));

            return result.ToArray();
        }
    }
}
