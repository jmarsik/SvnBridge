using System;
using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Exceptions;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.SourceControl.Dto;
using SvnBridge.Utility;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlProvider : ISourceControlProvider, ICredentialsProvider
    {
        private readonly ISourceControlUtility sourceControlHelper;
        private static readonly Dictionary<string, Activity> _activities = new Dictionary<string, Activity>();
        private readonly ICredentials credentials;
        private readonly string projectName;
        private readonly string rootPath;
        private readonly string serverUrl;
        private readonly ITFSSourceControlService sourceControlService;
        private readonly IWebTransferService webTransferService;

        public TFSSourceControlProvider(
            string serverUrl,
            string projectName,
            ICredentials credentials,
            IWebTransferService webTransferService,
            ITFSSourceControlService tfsSourceControlService,
            IProjectInformationRepository projectInformationRepository)
        {
            this.webTransferService = webTransferService;
            sourceControlService = tfsSourceControlService;

            if (projectName != null)
            {
                ProjectLocationInformation location =
                    projectInformationRepository.GetProjectLocation(credentials, projectName);
                this.projectName = location.RemoteProjectName;
                this.serverUrl = location.ServerUrl;
                rootPath = Constants.ServerRootPath + this.projectName + "/";
            }
            else
            {
                this.serverUrl = serverUrl.Split(',')[0];
                rootPath = Constants.ServerRootPath;
            }
            this.credentials = CredentialsHelper.GetCredentialsForServer(this.serverUrl, credentials);
            sourceControlHelper = new SourceControlUtility(sourceControlService, this, rootPath, serverUrl);
        }

        #region ISourceControlProvider Members

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
            sourceControlService.DeleteWorkspace(serverUrl, credentials, activityId);
            _activities.Remove(activityId);
        }

        public bool DeleteItem(string activityId,
                               string path)
        {
            if ((GetItems(-1, path, Recursion.None, true) == null) && (GetPendingItem(activityId, path) == null))
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

            FolderMetaData root = (FolderMetaData) GetItems(versionTo, path, Recursion.None);

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
            return GetItems(version, path, recursion, false);
        }

        public int GetLatestVersion()
        {
            return sourceControlService.GetLatestChangeset(serverUrl, credentials);
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
                sourceControlService.QueryLog(serverUrl,
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
                            sourceControlService.QueryBranches(serverUrl,
                                                            credentials,
                                                            null,
                                                            new ItemSpec[] {spec},
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
                sourceControlService.QueryItems(serverUrl,
                                             credentials,
                                             null,
                                             null,
                                             new ItemSpec[1] {spec},
                                             versionSpec,
                                             DeletedState.NonDeleted,
                                             ItemType.Any,
                                             false);

            return (item[0].Items.Length > 0);
        }

        public void MakeActivity(string activityId)
        {
            string workspaceComment = "Temporary workspace for edit-merge-commit";
            sourceControlService.CreateWorkspace(serverUrl, credentials, activityId, workspaceComment);
            string localPath = GetLocalPath(activityId, "");
            sourceControlService.AddWorkspaceMapping(serverUrl, credentials, activityId, rootPath, localPath);
            _activities[activityId] = new Activity();
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
            sourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
            _activities[activityId].MergeList.Add(
                new ActivityItem(rootPath + path, ItemType.Folder, ActivityItemAction.New));
            _activities[activityId].Collections.Add(path);
        }

        public MergeActivityResponse MergeActivity(string activityId)
        {
            UpdateProperties(activityId);

            List<string> commitServerList = new List<string>();
            foreach (ActivityItem item in _activities[activityId].MergeList)
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
                    sourceControlService.Commit(serverUrl,
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
                    sourceControlService.Commit(serverUrl,
                                             credentials,
                                             activityId,
                                             _activities[activityId].Comment,
                                             commitServerList);
            }
            return GenerateMergeResponse(activityId, changesetId);
        }

        public byte[] ReadFile(ItemMetaData item)
        {
            return webTransferService.DownloadBytes(item.DownloadUrl, credentials);
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

            if (!activity.AddedProperties.ContainsKey(path))
            {
                activity.AddedProperties[path] = new Dictionary<string, string>();
            }

            activity.AddedProperties[path][property] = value;
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
                                      bool returnPropertyFiles)
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
                sourceControlService.QueryItems(serverUrl,
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
                if (recursion != Recursion.Full && !returnPropertyFiles)
                {
                    RetrievePropertiesForItem(item);
                }

                if (item.Name.Contains("/" + Constants.PROP_FOLDER + "/") && !returnPropertyFiles)
                {
                    string itemPath = item.Name.Replace("/" + Constants.PROP_FOLDER + "/" + Constants.FOLDER_PROP_FILE, "");
                    itemPath = itemPath.Replace("/" + Constants.PROP_FOLDER + "/", "/");
                    ItemProperties itemProperties = Helper.DeserializeXml<ItemProperties>(ReadFile(item));
                    properties[itemPath] = itemProperties;
                    itemPropertyRevision[itemPath] = item.Revision;
                }
                else if (!item.Name.EndsWith("/" + Constants.PROP_FOLDER) || item.ItemType != ItemType.Folder ||
                         returnPropertyFiles)
                {
                    if (item.ItemType == ItemType.Folder)
                    {
                        folders[item.Name.ToLower()] = (FolderMetaData) item;
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

        private void UpdateItemRevisionsBasedOnPropertyItemRevisions(Dictionary<string, FolderMetaData> folders,
                                                                     Dictionary<string, int> itemPropertyRevision)
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
                sourceControlService.UndoPendingChanges(serverUrl,
                                                     credentials,
                                                     activityId,
                                                     new string[] {rootPath + path});
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
                if (!item.Path.EndsWith("/" + Constants.PROP_FOLDER))
                {
                    if (item.Path.Contains("/" + Constants.PROP_FOLDER + "/"))
                    {
                        string path = item.Path.Replace("/" + Constants.PROP_FOLDER + "/", "/");
                        ItemType newItemType = item.FileType;
                        if (path.EndsWith("/" + Constants.FOLDER_PROP_FILE))
                        {
                            path = path.Replace("/" + Constants.FOLDER_PROP_FILE, "");
                            newItemType = ItemType.Folder;
                        }
                        newItem = new ActivityItem(path, newItemType, item.Action);
                    }

                    if (!sortedMergeResponse.Contains(newItem.Path))
                    {
                        sortedMergeResponse.Add(newItem.Path);

                        MergeActivityResponseItem responseItem =
                            new MergeActivityResponseItem(newItem.FileType, newItem.Path.Substring(rootPath.Length));
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
                                             List<string> baseFolders,
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
                    MergeActivityResponseItem responseItem =
                        new MergeActivityResponseItem(ItemType.Folder,
                                                      GetFolderName(item.Path.Substring(rootPath.Length)));
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
                {
                    existingPath = existingPath.Substring(0, lastIndexOf);
                }
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

            sourceControlService.UpdateLocalVersions(serverUrl, credentials, activityId, updates);

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

            sourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
            sourceControlService.UploadFileFromBytes(serverUrl, credentials, activityId, fileData, rootPath + path);

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

            sourceControlService.UndoPendingChanges(serverUrl,
                                                 credentials,
                                                 activityId,
                                                 new string[] {rootPath + copy.TargetPath});
            for (int i = activity.MergeList.Count - 1; i >= 0; i--)
            {
                if (activity.MergeList[i].Path == rootPath + copy.TargetPath)
                {
                    activity.MergeList.RemoveAt(i);
                }
            }

            ProcessCopyItem(activityId, copy, true);
        }

        private string GetLocalPath(string activityId,
                                    string path)
        {
            return Constants.LOCAL_PREFIX + activityId + path.Replace('/', '\\');
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
            sourceControlService.UpdateLocalVersions(serverUrl, credentials, activityId, updates);
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
                                sourceControlService.UndoPendingChanges(serverUrl,
                                                                     credentials,
                                                                     activityId,
                                                                     new string[] {rootPath + activity.DeletedItems[i]});
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
                        sourceControlService.UndoPendingChanges(serverUrl,
                                                             credentials,
                                                             activityId,
                                                             new string[] {rootPath + activity.DeletedItems[i]});
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

            sourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);
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

        private string GetPropertiesFolderName(string path,
                                               ItemType itemType)
        {
            if (itemType == ItemType.Folder)
            {
                return path + "/" + Constants.PROP_FOLDER;
            }
            else if (path.LastIndexOf('/') != -1)
            {
                return path.Substring(0, path.LastIndexOf('/')) + "/" + Constants.PROP_FOLDER;
            }
            else
            {
                return Constants.PROP_FOLDER;
            }
        }

        private string GetPropertiesFileName(string path,
                                             ItemType itemType)
        {
            if (itemType == ItemType.Folder)
            {
                return path + "/" + Constants.PROP_FOLDER + "/" + Constants.FOLDER_PROP_FILE;
            }
            else if (path.LastIndexOf('/') != -1)
            {
                return
                    path.Substring(0, path.LastIndexOf('/')) + "/" + Constants.PROP_FOLDER + path.Substring(path.LastIndexOf('/'));
            }
            else
            {
                return Constants.PROP_FOLDER + "/" + path;
            }
        }

        private void ProcessDeleteItem(string activityId,
                                       string path)
        {
            Activity activity = _activities[activityId];
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
            sourceControlService.PendChanges(serverUrl, credentials, activityId, pendRequests);

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
            ItemMetaData item = GetItems(-1, propertiesPath, Recursion.None, true);
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

            foreach (string path in activity.AddedProperties.Keys)
            {
                ItemType itemType = ItemType.File;
                ItemMetaData item = GetItems(-1, path, Recursion.None);
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

                foreach (KeyValuePair<string, string> property in activity.AddedProperties[path])
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
        }

        private string GetFolderName(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        private ItemMetaData GetPendingItem(string activityId,
                                            string path)
        {
            ItemSpec spec = new ItemSpec();
            spec.item = rootPath + path;
            ExtendedItem[][] items =
                sourceControlService.QueryItemsExtended(serverUrl,
                                                     credentials,
                                                     activityId,
                                                     new ItemSpec[1] {spec},
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
                                       IDictionary<string, ItemProperties> properties)
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
                    string folderName = GetFolderName(itemProperties.Key);
                    item = sourceControlHelper.FindItem(folders[folderName.ToLowerInvariant()], itemProperties.Key);
                }
                foreach (Property property in itemProperties.Value.Properties)
                {
                    item.Properties[property.Name] = property.Value;
                }
            }
        }

        public ICredentials GetCredentials()
        {
            return credentials;
        }
    }
}