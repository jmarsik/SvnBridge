using System;
using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Protocol;
using SvnBridge.Utility;
using ChangeType = CodePlex.TfsLibrary.RepositoryWebSvc.ChangeType;
using SvnBridge.Exceptions;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlProvider : ISourceControlProvider
    {
        const string SERVER_PATH = "$/";
        const string LOCAL_PREFIX = @"C:\";
        const string PROP_FOLDER = "..svnbridge";
        const string FOLDER_PROP_FILE = ".svnbridge";

        static Dictionary<string, Activity> _activities = new Dictionary<string, Activity>();
        ICredentials _credentials;
        string _serverUrl;
        string _projectName;
        string _rootPath;
        TFSSourceControlService _sourceControlSvc;
        WebTransferService _webTransferSvc;

        class Activity
        {
            public string Comment;
            public List<ActivityItem> MergeList = new List<ActivityItem>();
            public List<string> DeletedItems = new List<string>();
            public List<string> PostCommitDeletedItems = new List<string>();
            public List<CopyAction> CopiedItems = new List<CopyAction>();
            public List<string> Collections = new List<string>();
            public Dictionary<string, Dictionary<string, string>> AddedProperties = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, List<string>> RemovedProperties = new Dictionary<string, List<string>>();
        }

        class CopyAction
        {
            public string Path;
            public string TargetPath;
            public bool Rename;

            public CopyAction(string path, string targetPath, bool rename)
            {
                Path = path;
                TargetPath = targetPath;
                Rename = rename;
            }
        }

        enum ActivityItemAction
        {
            New,
            Updated,
            Deleted,
            Branch,
            RenameDelete
        }

        class ActivityItem
        {
            public string Path;
            public ItemType FileType;
            public ActivityItemAction Action;

            public ActivityItem(string path, ItemType fileType, ActivityItemAction action)
            {
                Path = path;
                FileType = fileType;
                Action = action;
            }
        }

        public class Property
        {
            public string Name = null;
            public string Value = null;
            public Property() { }
            public Property(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        public class ItemProperties
        {
            public List<Property> Properties = new List<Property>();
        }

        public TFSSourceControlProvider(string serverUrl, string projectName, NetworkCredential credentials)
        {
            RegistrationWebSvcFactory registrationFactory = new RegistrationWebSvcFactory();
            FileSystem fileSystem = new FileSystem();
            RegistrationService registrationSvc = new RegistrationService(registrationFactory);

            _webTransferSvc = new WebTransferService(fileSystem);
            _sourceControlSvc = new TFSSourceControlService(registrationSvc,
                                                        new RepositoryWebSvcFactory(registrationFactory),
                                                        _webTransferSvc,
                                                        fileSystem);

            if (projectName != null)
            {
                SetServerAndProject(serverUrl, credentials, projectName);
                _rootPath = SERVER_PATH + _projectName + "/";
            }
            else
            {
                _serverUrl = serverUrl;
                _rootPath = SERVER_PATH;
            }
            _credentials = GetCredentialsForServer(_serverUrl, credentials);
        }

        // Methods

        public void CopyItem(string activityId, string path, string targetPath)
        {
            CopyAction copyAction = new CopyAction(path, targetPath, false);
            _activities[activityId].CopiedItems.Add(copyAction);
            ProcessCopyItem(activityId, copyAction, false);
        }

        public void DeleteActivity(string activityId)
        {
            _sourceControlSvc.DeleteWorkspace(_serverUrl, _credentials, activityId);
            _activities.Remove(activityId);
        }

        public bool DeleteItem(string activityId, string path)
        {
            if ((GetItems(-1, path, Recursion.None, true) == null) && (GetPendingItem(activityId, path) == null))
                return false;

            Activity activity = _activities[activityId];
            bool postCommitDelete = false;
            foreach (CopyAction copy in activity.CopiedItems)
            {
                if (copy.Path.StartsWith(path + "/"))
                {
                    if (!activity.PostCommitDeletedItems.Contains(path))
                        activity.PostCommitDeletedItems.Add(path);

                    if (!copy.Rename)
                        ConvertCopyToRename(activityId, copy);

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

        public FolderMetaData GetChangedItems(string path, int versionFrom, int versionTo, UpdateReportData reportData)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

            Dictionary<string, int> clientExistingFiles = new Dictionary<string, int>();
            if (reportData.Entries != null)
                foreach (EntryData entryData in reportData.Entries)
                    if (string.IsNullOrEmpty(path))
                        clientExistingFiles["/" + entryData.path] = int.Parse(entryData.Rev);
                    else
                        clientExistingFiles["/" + path + "/" + entryData.path] = int.Parse(entryData.Rev);

            Dictionary<string, string> clientDeletedFiles = new Dictionary<string, string>();
            if (reportData.Missing != null)
                foreach (string missingPath in reportData.Missing)
                    if (string.IsNullOrEmpty(path))
                        clientDeletedFiles["/" + missingPath] = missingPath;
                    else
                        clientDeletedFiles["/" + path + "/" + missingPath] = missingPath;

            FolderMetaData root = (FolderMetaData)GetItems(versionTo, path, Recursion.None);
            if (root != null)
                root.Properties.Clear();

            if (versionFrom != versionTo)
            {
                if (root == null && reportData.UpdateTarget != null)
                {
                    root = new FolderMetaData();
                    DeleteMetaData deletedFile = new DeleteMetaData();
                    deletedFile.Name = reportData.UpdateTarget;
                    root.Items.Add(deletedFile);
                }
                else
                {
                    versionFrom++;
                    int lastVersion = versionTo + 1;
                    while (versionFrom != lastVersion)
                    {
                        LogItem logItem = GetLog(path, versionFrom, lastVersion - 1, Recursion.Full, 256);
                        if (logItem.History.Length == 0)
                            lastVersion = versionFrom;

                        foreach (SourceItemHistory history in logItem.History)
                        {
                            lastVersion = history.ChangeSetID;
                            for (int i = history.Changes.Count - 1; i >= 0; i--)
                            {
                                SourceItemChange change = history.Changes[i];
                                if (((change.ChangeType & ChangeType.Add) == ChangeType.Add) || ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit) || ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch))
                                {
                                    if (!change.Item.RemoteName.EndsWith("/" + PROP_FOLDER))
                                    {
                                        string remoteName = change.Item.RemoteName;
                                        bool propertyChange = false;
                                        if (remoteName.Contains("/" + PROP_FOLDER + "/"))
                                        {
                                            propertyChange = true;
                                            if (remoteName.EndsWith("/" + PROP_FOLDER + "/" + FOLDER_PROP_FILE))
                                                remoteName = remoteName.Substring(0, remoteName.IndexOf("/" + PROP_FOLDER + "/" + FOLDER_PROP_FILE));
                                            else
                                                remoteName = remoteName.Replace("/" + PROP_FOLDER + "/", "/");
                                        }
                                        ProcessAddedItem(path, remoteName, change, propertyChange, root, versionTo, clientExistingFiles, clientDeletedFiles);
                                    }
                                }
                                else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                                {
                                    if (!change.Item.RemoteName.EndsWith("/" + PROP_FOLDER) && !change.Item.RemoteName.Contains("/" + PROP_FOLDER + "/"))
                                    {
                                        ProcessDeletedFile(path, change.Item.RemoteName, change, root, versionTo, clientExistingFiles, clientDeletedFiles);
                                    }
                                }
                                else if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                                {
                                    ItemMetaData oldItem = GetItem(history.ChangeSetID - 1, change.Item.ItemId);
                                    ProcessDeletedFile(path, oldItem.Name, change, root, versionTo, clientExistingFiles, clientDeletedFiles);
                                    ProcessAddedItem(path, change.Item.RemoteName, change, false, root, versionTo, clientExistingFiles, clientDeletedFiles);
                                }
                                else
                                {
                                    throw new Exception("Unrecognized change type " + change.ChangeType);
                                }
                            }
                        }
                    }
                }
            }
            return root;
        }

        public ItemMetaData GetItemInActivity(string activityId, string path)
        {
            Activity activity = _activities[activityId];

            foreach (CopyAction copy in activity.CopiedItems)
                if (path.StartsWith(copy.TargetPath))
                    path = copy.Path + path.Substring(copy.TargetPath.Length);

            return GetItems(-1, path, Recursion.None);
        }

        public ItemMetaData GetItems(int version, string path, Recursion recursion)
        {
            return GetItems(version, path, recursion, false);
        }

        private ItemMetaData GetItems(int version, string path, Recursion recursion, bool returnPropertyFiles)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

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
            SourceItem[] items = _sourceControlSvc.QueryItems(_serverUrl, _credentials, _rootPath + path, recursionType, versionSpec, DeletedState.NonDeleted, ItemType.Any);
            Dictionary<string, FolderMetaData> folders = new Dictionary<string, FolderMetaData>();
            Dictionary<string, int> itemPropertyRevision = new Dictionary<string, int>();
            ItemMetaData firstItem = null;
            for (int i = 0; i < items.Length; i++)
            {
                ItemMetaData item = ConvertSourceItem(items[i]);
                if (recursion != Recursion.Full && !returnPropertyFiles)
                    RetrievePropertiesForItem(item);

                if (item.Name.Contains("/" + PROP_FOLDER + "/") && !returnPropertyFiles)
                {
                    string itemPath = item.Name.Replace("/" + PROP_FOLDER + "/" + FOLDER_PROP_FILE, "");
                    itemPath = itemPath.Replace("/" + PROP_FOLDER + "/", "/");
                    ItemProperties itemProperties = Helper.DeserializeXml<ItemProperties>(ReadFile(item));
                    properties[itemPath] = itemProperties;
                    itemPropertyRevision[itemPath] = item.Revision;
                }
                else if (!item.Name.EndsWith("/" + PROP_FOLDER) || item.ItemType != ItemType.Folder || returnPropertyFiles)
                {
                    if (item.ItemType == ItemType.Folder)
                        folders[item.Name.ToLower()] = (FolderMetaData)item;
                    if (i == 0)
                        firstItem = item;
                    else
                    {
                        string folderName = "";
                        string filename = item.Name;
                        if (item.Name.IndexOf('/') != -1)
                        {
                            folderName = GetFolderName(item.Name).ToLower();
                            filename = item.Name.Substring(folderName.Length + 1);
                        }
                        folders[folderName].Items.Add(item);
                    }
                }
            }
            SetItemProperties(folders, properties);
            UpdateItemRevisionsBasedOnPropertyItemRevisions(folders, itemPropertyRevision);
            return firstItem;
        }

        private void UpdateItemRevisionsBasedOnPropertyItemRevisions(Dictionary<string, FolderMetaData> folders, Dictionary<string, int> itemPropertyRevision)
        {
            foreach (KeyValuePair<string, int> propertyRevision in itemPropertyRevision)
            {
                ItemMetaData item = null;
                if (folders.ContainsKey(propertyRevision.Key.ToLower()))
                {
                    item = folders[propertyRevision.Key.ToLower()];
                }
                else
                {
                    string folderName = GetFolderName(propertyRevision.Key).ToLower();
                    foreach (ItemMetaData folderItem in folders[folderName].Items)
                        if (folderItem.Name == propertyRevision.Key)
                            item = folderItem;
                }
                item.PropertyRevision = propertyRevision.Value;
            }
        }

        private bool RevertDelete(string activityId, string path)
        {
            Activity activity = _activities[activityId];
            bool reverted = false;
            if (activity.DeletedItems.Contains(path))
            {
                _sourceControlSvc.UndoPendingChanges(_serverUrl, _credentials, activityId, new string[] { _rootPath + path });
                activity.DeletedItems.Remove(path);
                for (int j = activity.MergeList.Count - 1; j >= 0; j--)
                    if (activity.MergeList[j].Path == _rootPath + path)
                        activity.MergeList.RemoveAt(j);

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
                    item.Properties[property.Name] = property.Value;
            }
        }

        public int GetLatestVersion()
        {
            return _sourceControlSvc.GetLatestChangeset(_serverUrl, _credentials);
        }

        public LogItem GetLog(string path,
                              int versionFrom,
                              int versionTo,
                              Recursion recursion,
                              int maxCount)
        //                              bool getOriginalNames)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

            string serverPath = _rootPath + path;
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

            LogItem logItem = _sourceControlSvc.QueryLog(_serverUrl, _credentials, serverPath, changesetFrom, changesetTo, recursionType, maxCount);

            foreach (SourceItemHistory history in logItem.History)
                foreach (SourceItemChange change in history.Changes)
                {
                    change.Item.RemoteName = change.Item.RemoteName.Substring(_rootPath.Length);
                    if (true)
                        if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                        {
                            ItemMetaData oldItem = GetItem(history.ChangeSetID - 1, change.Item.ItemId);
                            change.Item = new RenamedSourceItem(change.Item, oldItem.Name, oldItem.Revision);
                        }
                        else if ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch)
                        {
                            ChangesetVersionSpec branchChangeset = new ChangesetVersionSpec();
                            branchChangeset.cs = history.ChangeSetID;
                            ItemSpec spec = new ItemSpec();
                            spec.item = _rootPath + change.Item.RemoteName;
                            BranchRelative[][] branches = _sourceControlSvc.QueryBranches(_serverUrl, _credentials, null, new ItemSpec[] { spec }, branchChangeset);
                            string oldName = branches[0][branches[0].GetUpperBound(0)].BranchFromItem.item.Substring(_rootPath.Length);
                            int oldRevision = change.Item.RemoteChangesetId - 1;
                            change.Item = new RenamedSourceItem(change.Item, oldName, oldRevision);
                        }
                }

            return logItem;
        }

        public bool IsDirectory(int version, string path)
        {
            ItemMetaData item = GetItems(version, path, Recursion.None);
            if (item.ItemType == ItemType.Folder)
                return true;
            else
                return false;
        }

        public bool ItemExists(string path)
        {
            return ItemExists(path, -1);
        }

        public bool ItemExists(string path, int version)
        {
            ItemSpec spec = new ItemSpec();
            spec.item = _rootPath + path;
            spec.recurse = RecursionType.None;

            VersionSpec versionSpec = VersionSpec.Latest;
            if (version != -1)
                versionSpec = VersionSpec.FromChangeset(version);

            ItemSet[] item = _sourceControlSvc.QueryItems(_serverUrl, _credentials, null, null, new ItemSpec[1] { spec }, versionSpec, DeletedState.NonDeleted, ItemType.Any, false);

            return (item[0].Items.Length > 0);
        }

        public void MakeActivity(string activityId)
        {
            string workspaceComment = "Temporary workspace for edit-merge-commit";
            _sourceControlSvc.CreateWorkspace(_serverUrl, _credentials, activityId, workspaceComment);
            string localPath = GetLocalPath(activityId, "");
            _sourceControlSvc.AddWorkspaceMapping(_serverUrl, _credentials, activityId, _rootPath, localPath);
            _activities[activityId] = new Activity();
        }

        public void MakeCollection(string activityId, string path)
        {
            if (ItemExists(path))
                throw new FolderAlreadyExistsException();

            ItemMetaData item;
            string existingPath = path.Substring(1);
            do
            {
                if (existingPath.IndexOf('/') != -1)
                    existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
                else
                    existingPath = "";

                item = GetItems(-1, existingPath, Recursion.None);
            }
            while (item == null);
            string localPath = GetLocalPath(activityId, path);
            UpdateLocalVersion(activityId, item, localPath.Substring(0, localPath.LastIndexOf('\\')));

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.AddFolder(localPath));
            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
            _activities[activityId].MergeList.Add(new ActivityItem(_rootPath + path, ItemType.Folder, ActivityItemAction.New));
            _activities[activityId].Collections.Add(path);
        }

        public MergeActivityResponse MergeActivity(string activityId)
        {
            UpdateProperties(activityId);

            List<string> commitServerList = new List<string>();
            foreach (ActivityItem item in _activities[activityId].MergeList)
                if (item.Action != ActivityItemAction.RenameDelete)
                    commitServerList.Add(item.Path);

            int changesetId;
            if (commitServerList.Count > 0)
                changesetId = _sourceControlSvc.Commit(_serverUrl, _credentials, activityId, _activities[activityId].Comment, commitServerList);
            else
                changesetId = GetLatestVersion();

            if (_activities[activityId].PostCommitDeletedItems.Count > 0)
            {
                commitServerList.Clear();
                foreach (string path in _activities[activityId].PostCommitDeletedItems)
                {
                    ProcessDeleteItem(activityId, path);
                    commitServerList.Add(_rootPath + path);
                }
                changesetId = _sourceControlSvc.Commit(_serverUrl, _credentials, activityId, _activities[activityId].Comment, commitServerList);
            }
            return GenerateMergeResponse(activityId, changesetId);
        }

        private MergeActivityResponse GenerateMergeResponse(string activityId, int changesetId)
        {
            MergeActivityResponse mergeResponse = new MergeActivityResponse(changesetId, DateTime.Now, "unknown");
            List<string> baseFolders = new List<string>();
            List<string> sortedMergeResponse = new List<string>();
            foreach (ActivityItem item in _activities[activityId].MergeList)
            {
                ActivityItem newItem = item;
                if (!item.Path.EndsWith("/" + PROP_FOLDER))
                {
                    if (item.Path.Contains("/" + PROP_FOLDER + "/"))
                    {
                        string path = item.Path.Replace("/" + PROP_FOLDER + "/", "/");
                        ItemType newItemType = item.FileType;
                        if (path.EndsWith("/" + FOLDER_PROP_FILE))
                        {
                            path = path.Replace("/" + FOLDER_PROP_FILE, "");
                            newItemType = ItemType.Folder;
                        }
                        newItem = new ActivityItem(path, newItemType, item.Action);
                    }

                    if (!sortedMergeResponse.Contains(newItem.Path))
                    {
                        sortedMergeResponse.Add(newItem.Path);

                        MergeActivityResponseItem responseItem = new MergeActivityResponseItem(newItem.FileType, newItem.Path.Substring(_rootPath.Length));
                        if (newItem.Action != ActivityItemAction.Deleted && newItem.Action != ActivityItemAction.Branch && newItem.Action != ActivityItemAction.RenameDelete)
                            mergeResponse.Items.Add(responseItem);

                        AddBaseFolderIfRequired(activityId, newItem, baseFolders, mergeResponse);
                    }
                }
            }
            return mergeResponse;
        }

        private void AddBaseFolderIfRequired(string activityId, ActivityItem item, List<string> baseFolders, MergeActivityResponse mergeResponse)
        {
            string folderName = GetFolderName(item.Path);
            if (((item.Action == ActivityItemAction.New) || (item.Action == ActivityItemAction.Deleted) || (item.Action == ActivityItemAction.RenameDelete)) && !baseFolders.Contains(folderName))
            {
                baseFolders.Add(folderName);
                bool folderFound = false;
                foreach (ActivityItem folderItem in _activities[activityId].MergeList)
                    if (folderItem.FileType == ItemType.Folder && folderItem.Path == folderName)
                        folderFound = true;

                if (!folderFound)
                {
                    MergeActivityResponseItem responseItem = new MergeActivityResponseItem(ItemType.Folder, GetFolderName(item.Path.Substring(_rootPath.Length)));
                    mergeResponse.Items.Add(responseItem);
                }
            }
        }

        public byte[] ReadFile(ItemMetaData item)
        {
            return _webTransferSvc.DownloadBytes(item.DownloadUrl, _credentials);
        }

        public void SetActivityComment(string activityId, string comment)
        {
            _activities[activityId].Comment = comment;
        }

        public void SetProperty(string activityId, string path, string property, string value)
        {
            Activity activity = _activities[activityId];

            if (!activity.AddedProperties.ContainsKey(path))
                activity.AddedProperties[path] = new Dictionary<string, string>();

            activity.AddedProperties[path][property] = value;
        }

        public bool WriteFile(string activityId, string path, byte[] fileData)
        {
            return WriteFile(activityId, path, fileData, false);
        }

        private bool WriteFile(string activityId, string path, byte[] fileData, bool reportUpdatedFile)
        {
            bool replaced = RevertDelete(activityId, path);

            Activity activity = _activities[activityId];
            ItemMetaData item;
            string existingPath = path.Substring(1);

            do
            {
                existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
                item = GetItems(-1, existingPath, Recursion.None, true);
            }
            while (item == null);

            string localPath = GetLocalPath(activityId, path);
            List<LocalUpdate> updates = new List<LocalUpdate>();
            updates.Add(LocalUpdate.FromLocal(item.Id,
                                               localPath.Substring(0, localPath.LastIndexOf('\\')),
                                               item.Revision));

            item = GetItems(-1, path.Substring(1), Recursion.None, true);
            if (item != null)
                updates.Add(LocalUpdate.FromLocal(item.Id, localPath, item.Revision));

            _sourceControlSvc.UpdateLocalVersions(_serverUrl, _credentials, activityId, updates);

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
                    if (copy.TargetPath == path)
                        addToMergeList = false;
            }

            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
            _sourceControlSvc.UploadFileFromBytes(_serverUrl, _credentials, activityId, fileData, _rootPath + path);

            if (addToMergeList)
            {
                if (!replaced && (!newFile || reportUpdatedFile))
                    activity.MergeList.Add(new ActivityItem(_rootPath + path, ItemType.File, ActivityItemAction.Updated));
                else
                    activity.MergeList.Add(new ActivityItem(_rootPath + path, ItemType.File, ActivityItemAction.New));
            }

            return newFile;
        }

        private void ConvertCopyToRename(string activityId, CopyAction copy)
        {
            Activity activity = _activities[activityId];

            _sourceControlSvc.UndoPendingChanges(_serverUrl, _credentials, activityId, new string[] { _rootPath + copy.TargetPath });
            for (int i = activity.MergeList.Count - 1; i >= 0; i--)
                if (activity.MergeList[i].Path == _rootPath + copy.TargetPath)
                    activity.MergeList.RemoveAt(i);

            ProcessCopyItem(activityId, copy, true);
        }

        private string GetLocalPath(string activityId, string path)
        {
            return LOCAL_PREFIX + activityId + path.Replace('/', '\\');
        }

        private void UpdateLocalVersion(string activityId, ItemMetaData item, string localPath)
        {
            UpdateLocalVersion(activityId, item.Id, item.ItemRevision, localPath);
        }

        private void UpdateLocalVersion(string activityId, int itemId, int itemRevision, string localPath)
        {
            List<LocalUpdate> updates = new List<LocalUpdate>();
            updates.Add(LocalUpdate.FromLocal(itemId, localPath, itemRevision));
            _sourceControlSvc.UpdateLocalVersions(_serverUrl, _credentials, activityId, updates);
        }

        private void ProcessCopyItem(string activityId, CopyAction copyAction, bool forceRename)
        {
            Activity activity = _activities[activityId];
            string localPath = GetLocalPath(activityId, copyAction.Path);
            string localTargetPath = GetLocalPath(activityId, copyAction.TargetPath);

            bool copyIsRename = RevertDelete(activityId, copyAction.Path);
            ItemMetaData item = GetItems(-1, copyAction.Path, Recursion.None);
            UpdateLocalVersion(activityId, item, localPath);

            if (copyIsRename)
                activity.MergeList.Add(new ActivityItem(_rootPath + copyAction.Path, item.ItemType, ActivityItemAction.RenameDelete));

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
                                _sourceControlSvc.UndoPendingChanges(_serverUrl, _credentials, activityId, new string[] { _rootPath + activity.DeletedItems[i] });
                                for (int j = activity.MergeList.Count - 1; j >= 0; j--)
                                    if (activity.MergeList[j].Path == _rootPath + activity.DeletedItems[i])
                                        activity.MergeList.RemoveAt(j);

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
                        _sourceControlSvc.UndoPendingChanges(_serverUrl, _credentials, activityId, new string[] { _rootPath + activity.DeletedItems[i] });
                        for (int j = activity.MergeList.Count - 1; j >= 0; j--)
                            if (activity.MergeList[j].Path == _rootPath + activity.DeletedItems[i])
                                activity.MergeList.RemoveAt(j);

                        activity.DeletedItems.RemoveAt(i);
                    }
                }
            }
            if (!copyIsRename)
                foreach (string deletedItem in activity.PostCommitDeletedItems)
                    if (copyAction.Path.StartsWith(deletedItem + "/"))
                        copyIsRename = true;

            List<PendRequest> pendRequests = new List<PendRequest>();
            if (copyIsRename || forceRename)
            {
                pendRequests.Add(PendRequest.Rename(localPath, localTargetPath));
                copyAction.Rename = true;
            }
            else
                pendRequests.Add(PendRequest.Copy(localPath, localTargetPath));

            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
            if (copyAction.Rename)
                activity.MergeList.Add(new ActivityItem(_rootPath + copyAction.TargetPath, item.ItemType, ActivityItemAction.New));
            else
                activity.MergeList.Add(new ActivityItem(_rootPath + copyAction.TargetPath, item.ItemType, ActivityItemAction.Branch));
        }

        private string GetPropertiesFolderName(string path, ItemType itemType)
        {
            if (itemType == ItemType.Folder)
                return path + "/" + PROP_FOLDER;
            else if (path.LastIndexOf('/') != -1)
                return path.Substring(0, path.LastIndexOf('/')) + "/" + PROP_FOLDER;
            else
                return PROP_FOLDER;
        }

        private string GetPropertiesFileName(string path, ItemType itemType)
        {
            if (itemType == ItemType.Folder)
                return path + "/" + PROP_FOLDER + "/" + FOLDER_PROP_FILE;
            else if (path.LastIndexOf('/') != -1)
                return path.Substring(0, path.LastIndexOf('/')) + "/" + PROP_FOLDER + path.Substring(path.LastIndexOf('/'));
            else
                return PROP_FOLDER + "/" + path;
        }

        private void ProcessDeleteItem(string activityId, string path)
        {
            Activity activity = _activities[activityId];
            string localPath = GetLocalPath(activityId, path);

            ItemMetaData item = GetItems(-1, path, Recursion.None, true);
            if (item == null)
                item = GetPendingItem(activityId, path);

            UpdateLocalVersion(activityId, item, localPath);

            if (item.ItemType != ItemType.Folder)
            {
                string propertiesFile = GetPropertiesFileName(path, item.ItemType);
                DeleteItem(activityId, propertiesFile);
            }

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.Delete(localPath));
            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);

            activity.MergeList.Add(new ActivityItem(_rootPath + path, item.ItemType, ActivityItemAction.Deleted));
        }

        private ItemProperties ReadPropertiesForItem(string path, ItemType itemType)
        {
            int revision;
            return ReadPropertiesForItem(path, itemType, out revision);
        }

        private ItemProperties ReadPropertiesForItem(string path, ItemType itemType, out int revision)
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
                    itemType = item.ItemType;
                else if (activity.Collections.Contains(path))
                    itemType = ItemType.Folder;

                ItemProperties properties = ReadPropertiesForItem(path, itemType);
                if (properties == null)
                    properties = new ItemProperties();

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
                        properties.Properties.Add(new Property(property.Key, property.Value));
                }

                string propertiesPath = GetPropertiesFileName(path, itemType);
                string propertiesFolder = GetPropertiesFolderName(path, itemType);
                ItemMetaData propertiesFolderItem = GetItems(-1, propertiesFolder, Recursion.None, true);
                if ((propertiesFolderItem == null) && !activity.Collections.Contains(propertiesFolder))
                    MakeCollection(activityId, propertiesFolder);

                if (item != null)
                    WriteFile(activityId, propertiesPath, Helper.SerializeXml(properties), true);
                else
                    WriteFile(activityId, propertiesPath, Helper.SerializeXml(properties));
            }
        }

        private string GetFolderName(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        private ItemMetaData GetPendingItem(string activityId, string path)
        {
            ItemSpec spec = new ItemSpec();
            spec.item = _rootPath + path;
            ExtendedItem[][] items = _sourceControlSvc.QueryItemsExtended(_serverUrl, _credentials, activityId, new ItemSpec[1] { spec }, DeletedState.NonDeleted, ItemType.Any);
            if (items[0].Length == 0)
                return null;
            else
            {
                ItemMetaData pendingItem = new ItemMetaData();
                if (items[0][0].type == ItemType.Folder)
                    pendingItem = new FolderMetaData();

                pendingItem.Id = items[0][0].itemid;
                pendingItem.ItemRevision = items[0][0].latest;
                return pendingItem;
            }
        }

        private ItemMetaData ConvertSourceItem(SourceItem sourceItem)
        {
            ItemMetaData item;
            if (sourceItem.ItemType == ItemType.Folder)
                item = new FolderMetaData();
            else
                item = new ItemMetaData();

            item.Id = sourceItem.ItemId;
            if (_rootPath.Length <= sourceItem.RemoteName.Length)
                item.Name = sourceItem.RemoteName.Substring(_rootPath.Length);
            else
                item.Name = "";

            item.Author = "unknown";
            item.LastModifiedDate = sourceItem.RemoteDate;
            item.ItemRevision = sourceItem.RemoteChangesetId;
            item.DownloadUrl = sourceItem.DownloadUrl;
            return item;
        }

        static Dictionary<string, string[]> projectServers = new Dictionary<string, string[]>();

        private void SetServerAndProject(string serverUrl, NetworkCredential credentials, string projectName)
        {
            projectName = projectName.ToLower();
            if (!projectServers.ContainsKey(projectName))
            {
                string[] servers = serverUrl.Split(',');
                foreach (string server in servers)
                {
                    SourceItem[] items = _sourceControlSvc.QueryItems(server, GetCredentialsForServer(server, credentials), SERVER_PATH + projectName, RecursionType.None, new LatestVersionSpec(), DeletedState.NonDeleted, ItemType.Any);
                    if (items.Length > 0)
                        projectServers[projectName] = new string[] { server, items[0].RemoteName.Substring(SERVER_PATH.Length) };
                }
            }
            _serverUrl = projectServers[projectName][0];
            _projectName = projectServers[projectName][1];
        }

        private void SetItemProperties(Dictionary<string, FolderMetaData> folders,
                                  Dictionary<string, ItemProperties> properties)
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
                    item = FindItem(folders[folderName.ToLowerInvariant()], itemProperties.Key);
                }
                foreach (Property property in itemProperties.Value.Properties)
                    item.Properties[property.Name] = property.Value;
            }
        }

        private ItemMetaData FindItem(FolderMetaData folder, string name)
        {
            foreach (ItemMetaData item in folder.Items)
                if (item.Name == name)
                    return item;

            return null;
        }

        private ICredentials GetCredentialsForServer(string tfsUrl, NetworkCredential credentials)
        {
            ICredentials result = credentials;
            if (result == null)
            {
                Uri uri = new Uri(tfsUrl);

                if (uri.Host.ToLowerInvariant().EndsWith("codeplex.com"))
                {
                    CredentialCache cache = new CredentialCache();
                    cache.Add(uri, "Basic", new NetworkCredential("anonymous", null));
                    result = cache;
                }
                else
                    result = CredentialCache.DefaultNetworkCredentials;
            }
            return result;
        }

        private void ProcessDeletedFile(string path, string remoteName, SourceItemChange change,
            FolderMetaData root, int versionTo, Dictionary<string, int> clientExistingFiles,
            Dictionary<string, string> clientDeletedFiles)
        {
            if (!IsChangeAlreadyCurrentInClientState(ChangeType.Delete, remoteName, change.Item.RemoteChangesetId, clientExistingFiles, clientDeletedFiles))
            {
                string[] nameParts = remoteName.Substring(path.Length + 1).Split('/');
                string changePath = "/" + remoteName;
                string folderName = path;
                FolderMetaData folder = root;
                for (int i = 0; i < nameParts.Length; i++)
                {
                    folderName += "/" + nameParts[i];
                    ItemMetaData item = FindItem(folder, folderName);
                    if (item == null)
                    {
                        if (i == nameParts.Length - 1)
                        {
                            if (change.Item.ItemType == ItemType.File)
                                item = new DeleteMetaData();
                            else
                                item = new DeleteFolderMetaData();

                            item.Name = remoteName;
                        }
                        else
                        {
                            item = GetItems(versionTo, folderName, Recursion.None);
                            if (item == null)
                            {
                                item = new DeleteFolderMetaData();
                                item.Name = folderName;
                            }
                        }
                        folder.Items.Add(item);
                        if (i != nameParts.Length - 1)
                        {
                            folder = (FolderMetaData)item;
                        }
                    }
                    else if (item is DeleteFolderMetaData)
                    {
                        return;
                    }
                    if (i != nameParts.Length - 1)
                    {
                        folder = (FolderMetaData)item;
                    }
                }
            }
        }

        private ItemMetaData GetItem(int version, int itemId)
        {
            SourceItem[] items = _sourceControlSvc.QueryItems(_serverUrl, _credentials, new int[] { itemId }, version);
            return ConvertSourceItem(items[0]);
        }

        private bool IsChangeAlreadyCurrentInClientState(ChangeType changeType, string itemPath, int itemRevision,
            Dictionary<string, int> clientExistingFiles, Dictionary<string, string> clientDeletedFiles)
        {
            string changePath = "/" + itemPath;
            if (((changeType & ChangeType.Add) == ChangeType.Add) ||
                ((changeType & ChangeType.Edit) == ChangeType.Edit))
            {
                if ((clientExistingFiles.ContainsKey(changePath)) && (clientExistingFiles[changePath] >= itemRevision))
                    return true;

                foreach (string clientExistingFile in clientExistingFiles.Keys)
                    if (changePath.StartsWith(clientExistingFile + "/") && (clientExistingFiles[clientExistingFile] >= itemRevision))
                        return true;
            }
            else if ((changeType & ChangeType.Delete) == ChangeType.Delete)
            {
                if (clientDeletedFiles.ContainsKey(changePath) || (clientExistingFiles.ContainsKey(changePath) && (clientExistingFiles[changePath] >= itemRevision)))
                    return true;

                foreach (string clientDeletedFile in clientDeletedFiles.Keys)
                    if (changePath.StartsWith(clientDeletedFile + "/"))
                        return true;
            }
            return false;
        }

        private void ProcessAddedItem(string path, string remoteName, SourceItemChange change, bool propertyChange, FolderMetaData root,
            int versionTo, Dictionary<string, int> clientExistingFiles, Dictionary<string, string> clientDeletedFiles)
        {
            if (!IsChangeAlreadyCurrentInClientState(ChangeType.Add, remoteName, change.Item.RemoteChangesetId, clientExistingFiles, clientDeletedFiles))
            {
                if (remoteName.Length == path.Length)
                {
                    ItemMetaData item = GetItems(versionTo, remoteName, Recursion.None);
                    root.Properties = item.Properties;
                }
                else
                {
                    FolderMetaData folder = root;
                    string itemName = path;
                    string[] nameParts = remoteName.Substring(path.Length + 1).Split('/');
                    for (int i = 0; i < nameParts.Length; i++)
                    {
                        bool lastNamePart = false;
                        if (i == nameParts.Length - 1)
                            lastNamePart = true;

                        itemName += "/" + nameParts[i];
                        ItemMetaData item = FindItem(folder, itemName);
                        if (item == null)
                        {
                            item = GetItems(versionTo, itemName, Recursion.None);
                            if (!lastNamePart)
                            {
                                StubFolderMetaData stubFolder = new StubFolderMetaData();
                                stubFolder.RealFolder = (FolderMetaData)item;
                                stubFolder.Name = item.Name;
                                stubFolder.ItemRevision = item.Revision;
                                stubFolder.LastModifiedDate = item.LastModifiedDate;
                                stubFolder.Author = item.Author;
                                item = stubFolder;
                            }
                            folder.Items.Add(item);
                        }
                        else if ((item is StubFolderMetaData) && lastNamePart)
                        {
                            folder.Items.Remove(item);
                            folder.Items.Add(((StubFolderMetaData)item).RealFolder);
                        }
                        else if ((item is DeleteFolderMetaData) && !lastNamePart)
                        {
                            return;
                        }
                        else if (((item is DeleteFolderMetaData) || (item is DeleteMetaData)) && ((change.ChangeType & ChangeType.Add) == ChangeType.Add))
                        {
                            if (!propertyChange)
                                folder.Items.Remove(item);
                        }
                        if (!lastNamePart)
                        {
                            folder = (FolderMetaData)item;
                        }
                    }
                }
            }
        }
    }
}
