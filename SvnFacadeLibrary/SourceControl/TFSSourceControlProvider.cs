using System;
using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Protocol;
using SvnBridge.Utility;
using ChangeType=CodePlex.TfsLibrary.RepositoryWebSvc.ChangeType;
using SvnBridge.Exceptions;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlProvider : ISourceControlProvider
    {
        const string SERVER_PATH = "$/";
        const string LOCAL_PREFIX = @"C:\";

        static Dictionary<string, Activity> _activities = new Dictionary<string, Activity>();
        ICredentials _credentials;
        string _serverUrl;
        SourceControlService _sourceControlSvc;
        WebTransferService _webTransferSvc;

        class Activity
        {
            public string Comment;
            public List<ActivityItem> MergeList = new List<ActivityItem>();
            public List<string> DeletedItems = new List<string>();
            public List<string> PostCommitDeletedItems = new List<string>();
            public List<CopyAction> CopiedItems = new List<CopyAction>();
            public List<string> FolderWithUpdatedProperties = new List<string>();
            public Dictionary<string, Dictionary<string, string>> AddedFolderProperties = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, string>> AddedFileProperties = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, List<string>> RemovedFolderProperties = new Dictionary<string, List<string>>();
            public Dictionary<string, List<string>> RemovedFileProperties = new Dictionary<string, List<string>>();
        }

        class CopyAction
        {
            public string Path;
            public string TargetPath;

            public CopyAction(string path, string targetPath)
            {
                Path = path;
                TargetPath = targetPath;
            }
        }

        class ActivityItem
        {
            public string Path;
            public ItemType FileType;

            public ActivityItem(string path, ItemType fileType)
            {
                Path = path;
                FileType = fileType;
            }
        }

        public class Property
        {
            public string Name = null;
            public string Value = null;
        }

        public class FileProperties
        {
            public string Path = null;
            public List<Property> Properties = new List<Property>();
        }

        public class FolderProperties
        {
            public List<Property> Properties = new List<Property>();
            public List<FileProperties> FileProperties = new List<FileProperties>();
        }

        public TFSSourceControlProvider(string serverUrl,
                                        NetworkCredential credentials)
        {
            _serverUrl = serverUrl;
            _credentials = credentials;

            if (_credentials == null)
            {
                Uri uri = new Uri(serverUrl);

                if (uri.Host.ToLowerInvariant().EndsWith("codeplex.com"))
                {
                    CredentialCache cache = new CredentialCache();
                    cache.Add(uri, "Basic", new NetworkCredential("anonymous", null));
                    _credentials = cache;
                }
                else
                    _credentials = CredentialCache.DefaultNetworkCredentials;
            }

            RegistrationWebSvcFactory registrationFactory = new RegistrationWebSvcFactory();
            FileSystem fileSystem = new FileSystem();
            RegistrationService registrationSvc = new RegistrationService(registrationFactory);

            _webTransferSvc = new WebTransferService(fileSystem);
            _sourceControlSvc = new SourceControlService(registrationSvc,
                                                        new RepositoryWebSvcFactory(registrationFactory),
                                                        _webTransferSvc,
                                                        fileSystem);
        }

        // Methods

        public void MakeActivity(string activityId)
        {
            string workspaceComment = "Temporary workspace for edit-merge-commit";
            _sourceControlSvc.CreateWorkspace(_serverUrl, _credentials, activityId, workspaceComment);
            string localPath = GetLocalPath(activityId, "");
            _sourceControlSvc.AddWorkspaceMapping(_serverUrl, _credentials, activityId, SERVER_PATH, localPath);
            _activities[activityId] = new Activity();
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

            switch (property)
            {
                case "ignore":
                    if (!activity.FolderWithUpdatedProperties.Contains(path))
                        activity.FolderWithUpdatedProperties.Add(path);

                    if (!activity.AddedFolderProperties.ContainsKey(path))
                        activity.AddedFolderProperties[path] = new Dictionary<string, string>();

                    activity.AddedFolderProperties[path][property] = value;
                    break;

                case "mime-type":
                    string folder = GetFolderName(path);

                    if (!activity.FolderWithUpdatedProperties.Contains(folder))
                        activity.FolderWithUpdatedProperties.Add(folder);

                    if (!activity.AddedFileProperties.ContainsKey(path))
                        activity.AddedFileProperties[path] = new Dictionary<string, string>();

                    activity.AddedFileProperties[path][property] = value;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported property type");
            }
        }

        private string GetLocalPath(string activityId, string path)
        {
            return LOCAL_PREFIX + activityId + path.Replace('/', '\\');
        }

        public bool WriteFile(string activityId,
                              string path,
                              byte[] fileData)
        {
            ItemMetaData item;
            string existingPath = path.Substring(1);

            do
            {
                existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
                item = GetItems(-1, existingPath, Recursion.None);
            }
            while (item == null);

            string localPath = GetLocalPath(activityId, path);
            List<LocalUpdate> updates2 = new List<LocalUpdate>();
            updates2.Add(LocalUpdate.FromLocal(item.Id,
                                               localPath.Substring(0, localPath.LastIndexOf('\\')),
                                               item.Revision));

            item = GetItems(-1, path.Substring(1), Recursion.None);
            if (item != null)
                updates2.Add(LocalUpdate.FromLocal(item.Id,
                                                   localPath,
                                                   item.Revision));

            _sourceControlSvc.UpdateLocalVersions(_serverUrl, _credentials, activityId, updates2);

            List<PendRequest> pendRequests = new List<PendRequest>();
            
            bool newFile;
            if (item == null)
            {
                CopyAction copyAction = null;
                foreach (CopyAction copy in _activities[activityId].CopiedItems)
                    if (copy.TargetPath == path)
                        copyAction = copy;

                if (copyAction != null)
                {
                    _activities[activityId].DeletedItems.Remove(copyAction.Path);
                    _activities[activityId].CopiedItems.Remove(copyAction);

                    item = GetItems(-1, copyAction.Path, Recursion.None);
                    string originalLocalPath = GetLocalPath(activityId, copyAction.Path);
                    UpdateLocalVersion(activityId, item, originalLocalPath);

                    pendRequests.Add(PendRequest.Rename(originalLocalPath, localPath));
                    _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
                    UpdateLocalVersion(activityId, item, localPath);

                    pendRequests.Clear();
                    pendRequests.Add(PendRequest.Edit(localPath));
                    newFile = false;
                }
                else
                {
                    pendRequests.Add(PendRequest.AddFile(localPath, TfsUtil.CodePage_ANSI));
                    newFile = true;
                }
            }
            else
            {
                pendRequests.Add(PendRequest.Edit(localPath));
                newFile = false;
            }
            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
            _sourceControlSvc.UploadFileFromBytes(_serverUrl, _credentials, activityId, fileData, SERVER_PATH + path);
            _activities[activityId].MergeList.Add(new ActivityItem(SERVER_PATH + path, ItemType.File));

            return newFile;
        }

        private void UpdateLocalVersion(string activityId, ItemMetaData item, string localPath)
        {
            List<LocalUpdate> updates = new List<LocalUpdate>();
            updates.Add(LocalUpdate.FromLocal(item.Id, localPath, item.Revision));
            _sourceControlSvc.UpdateLocalVersions(_serverUrl, _credentials, activityId, updates);
        }

        public void CopyItem(string activityId, string path, string targetPath)
        {
            _activities[activityId].CopiedItems.Add(new CopyAction(path, targetPath));
        }

        private void ProcessCopyItem(string activityId, string path, string targetPath)
        {
            string localPath = GetLocalPath(activityId, path);
            string localTargetPath = GetLocalPath(activityId, targetPath);

            ItemMetaData item = GetItems(-1, path, Recursion.None);
            UpdateLocalVersion(activityId, item, localPath);

            bool copyIsRename = false;
            if (_activities[activityId].DeletedItems.Contains(path))
            {
                copyIsRename = true;
                _activities[activityId].DeletedItems.Remove(path);
            }

            List<PendRequest> pendRequests = new List<PendRequest>();
            if (copyIsRename)
                pendRequests.Add(PendRequest.Rename(localPath, localTargetPath));
            else
                pendRequests.Add(PendRequest.Copy(localPath, localTargetPath));

            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
            _activities[activityId].MergeList.Add(new ActivityItem(SERVER_PATH + targetPath, item.ItemType));
        }

        public void DeleteItem(string activityId, string path)
        {
            bool postCommitDelete = false;
            foreach (CopyAction copy in _activities[activityId].CopiedItems)
            {
                if (copy.Path.StartsWith(path + "/"))
                {
                    _activities[activityId].PostCommitDeletedItems.Add(path);
                    postCommitDelete = true;
                }
            }

            if (!postCommitDelete)
                _activities[activityId].DeletedItems.Add(path);
        }

        private void ProcessDeleteItem(string activityId, string path)
        {
            Activity activity = _activities[activityId];
            string localPath = GetLocalPath(activityId, path);

            ItemMetaData item = GetItems(-1, path, Recursion.None);
            UpdateLocalVersion(activityId, item, localPath);

            if (item.ItemType != ItemType.Folder)
            {
                string folder = GetFolderName(path);
                FolderProperties properties = ReadPropertiesFile(folder);

                foreach (FileProperties fileProperties in properties.FileProperties)
                    if (fileProperties.Path == path)
                        foreach (Property property in fileProperties.Properties)
                        {
                            if (!activity.RemovedFileProperties.ContainsKey(path))
                                activity.RemovedFileProperties[path] = new List<string>();

                            if (!activity.RemovedFileProperties[path].Contains(property.Name))
                            {
                                if (!activity.FolderWithUpdatedProperties.Contains(folder))
                                    activity.FolderWithUpdatedProperties.Add(folder);

                                activity.RemovedFileProperties[path].Add(property.Name);
                            }
                        }
            }

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.Delete(localPath));
            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);

            activity.MergeList.Add(new ActivityItem(SERVER_PATH + path, item.ItemType));
        }

        public void MakeCollection(string activityId, string path)
        {
            if (ItemExists(path))
                throw new FolderAlreadyExistsException();

            ItemMetaData item;
            string existingPath = path.Substring(1);
            do
            {
                existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
                item = GetItems(-1, existingPath, Recursion.None);
            }
            while (item == null);
            string localPath = GetLocalPath(activityId, path);
            UpdateLocalVersion(activityId, item, localPath.Substring(0, localPath.LastIndexOf('\\')));

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.AddFolder(localPath));
            _sourceControlSvc.PendChanges(_serverUrl, _credentials, activityId, pendRequests);
            _activities[activityId].MergeList.Add(new ActivityItem(SERVER_PATH + path, ItemType.Folder));
        }

        static void UpdateFileProperty(FolderProperties folderProperties,
                                       string path,
                                       string name,
                                       string value)
        {
            bool fileFound = false;

            foreach (FileProperties fileProperties in folderProperties.FileProperties)
            {
                if (fileProperties.Path == path)
                {
                    fileFound = true;
                    bool propertyFound = false;

                    foreach (Property property in fileProperties.Properties)
                        if (property.Name == name)
                        {
                            propertyFound = true;
                            property.Value = value;
                        }

                    if (!propertyFound)
                    {
                        Property newProperty = new Property();
                        newProperty.Name = name;
                        newProperty.Value = value;
                        fileProperties.Properties.Add(newProperty);
                    }
                }
            }

            if (!fileFound)
            {
                Property newProperty = new Property();
                newProperty.Name = name;
                newProperty.Value = value;
                FileProperties fileProperties = new FileProperties();
                fileProperties.Path = path;
                fileProperties.Properties.Add(newProperty);
                folderProperties.FileProperties.Add(fileProperties);
            }
        }

        private FolderProperties ReadPropertiesFile(string folder)
        {
            string path = folder + "/.svnbridge";
            FolderProperties properties = new FolderProperties();
            ItemMetaData item = GetItems(-1, path, Recursion.None);
            if (item != null)
            {
                properties = Helper.DeserializeXml<FolderProperties>(ReadFile(item));
            }
            return properties;
        }

        void UpdateProperties(string activityId)
        {
            Activity activity = _activities[activityId];

            foreach (string folder in activity.FolderWithUpdatedProperties)
            {
                string path = folder + "/.svnbridge";
                FolderProperties existingProperties = ReadPropertiesFile(folder);

                foreach (KeyValuePair<string, Dictionary<string, string>> folderWithNewProperties in activity.AddedFolderProperties)
                {
                    if (folderWithNewProperties.Key == folder)
                    {
                        foreach (KeyValuePair<string, string> newFolderProperty in folderWithNewProperties.Value)
                        {
                            bool found = false;

                            foreach (Property existingProperty in existingProperties.Properties)
                                if (newFolderProperty.Key == existingProperty.Name)
                                {
                                    existingProperty.Value = newFolderProperty.Value;
                                    found = true;
                                }

                            if (!found)
                            {
                                Property addProperty = new Property();
                                addProperty.Name = newFolderProperty.Key;
                                addProperty.Value = newFolderProperty.Value;
                                existingProperties.Properties.Add(addProperty);
                            }
                        }
                    }
                }

                foreach (KeyValuePair<string, List<string>> folderWithRemovedProperties in activity.RemovedFolderProperties)
                    if (folderWithRemovedProperties.Key == folder)
                        foreach (string removedFolderProperty in folderWithRemovedProperties.Value)
                            for (int i = existingProperties.Properties.Count-1; i >= 0; i--)
                                if (removedFolderProperty == existingProperties.Properties[i].Name)
                                    existingProperties.Properties.RemoveAt(i);

                foreach (KeyValuePair<string, Dictionary<string, string>> fileWithNewProperties in activity.AddedFileProperties)
                {
                    if (GetFolderName(fileWithNewProperties.Key) == folder)
                    {
                        foreach (KeyValuePair<string, string> newFileProperty in fileWithNewProperties.Value)
                        {
                            UpdateFileProperty(existingProperties, fileWithNewProperties.Key, newFileProperty.Key, newFileProperty.Value);
                        }
                    }
                }

                foreach (KeyValuePair<string, List<string>> fileWithRemovedProperties in activity.RemovedFileProperties)
                    if (GetFolderName(fileWithRemovedProperties.Key) == folder)
                        foreach (string removedFileProperty in fileWithRemovedProperties.Value)
                            foreach (FileProperties fileProperties in existingProperties.FileProperties)
                                if (fileProperties.Path == fileWithRemovedProperties.Key)
                                    for (int i = fileProperties.Properties.Count-1; i >= 0; i--)
                                        if (fileProperties.Properties[i].Name == removedFileProperty)
                                            fileProperties.Properties.RemoveAt(i);

                WriteFile(activityId, path, Helper.SerializeXml(existingProperties));
            }
        }

        private string GetFolderName(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        public MergeActivityResponse MergeActivity(string activityId)
        {
            foreach (CopyAction copy in _activities[activityId].CopiedItems)
                ProcessCopyItem(activityId, copy.Path, copy.TargetPath);

            foreach (string path in _activities[activityId].DeletedItems)
                ProcessDeleteItem(activityId, path);

            UpdateProperties(activityId);

            List<string> commitServerList = new List<string>();
            foreach (ActivityItem item in _activities[activityId].MergeList)
            {
                commitServerList.Add(item.Path);
            }
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
                    commitServerList.Add(SERVER_PATH + path);
                }
                changesetId = _sourceControlSvc.Commit(_serverUrl, _credentials, activityId, _activities[activityId].Comment, commitServerList);
            }

            MergeActivityResponse mergeResponse = new MergeActivityResponse(changesetId, DateTime.Now, "unknown");
            foreach (ActivityItem item in _activities[activityId].MergeList)
            {
                mergeResponse.Items.Add(new MergeActivityResponseItem(item.FileType, item.Path.Substring(2)));
            }
            return mergeResponse;
        }

        public void DeleteActivity(string activityId)
        {
            _sourceControlSvc.DeleteWorkspace(_serverUrl, _credentials, activityId);
            _activities.Remove(activityId);
        }

        public byte[] ReadFile(ItemMetaData item)
        {
            return _webTransferSvc.DownloadBytes(item.DownloadUrl, _credentials);
        }

        public LogItem GetLog(string path,
                              int versionFrom,
                              int versionTo,
                              Recursion recursion,
                              int maxCount)
        {
            string serverPath = SERVER_PATH + path.Substring(1);
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

            return _sourceControlSvc.QueryLog(_serverUrl, _credentials, serverPath, changesetFrom, changesetTo, recursionType, maxCount);
        }

        public ItemMetaData GetItems(int version, string path, Recursion recursion)
        {
            Dictionary<string, FolderProperties> properties = new Dictionary<string, FolderProperties>();
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
            SourceItem[] items = _sourceControlSvc.QueryItems(_serverUrl, _credentials, SERVER_PATH + path, recursionType, versionSpec, DeletedState.NonDeleted, ItemType.Any);
            Dictionary<string, FolderMetaData> folders = new Dictionary<string, FolderMetaData>();
            ItemMetaData firstItem = null;
            for (int i = 0; i < items.Length; i++)
            {
                ItemMetaData item = ConvertSourceItem(items[i]);
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
                    string filename = item.Name;
                    if (item.Name.IndexOf('/') != -1)
                    {
                        folderName = item.Name.Substring(0, item.Name.LastIndexOf('/')).ToLower();
                        filename = item.Name.Substring(folderName.Length + 1);
                    }
                    if (filename == ".svnbridge")
                    {
                        FolderProperties folderProperties = Helper.DeserializeXml<FolderProperties>(ReadFile(item));
                        properties[folderName] = folderProperties;
                    }
                    else
                    {
                        folders[folderName].Items.Add(item);
                    }
                }
            }
            SetProperties(folders, properties);
            return firstItem;
        }

        private ItemMetaData ConvertSourceItem(SourceItem sourceItem)
        {
            ItemMetaData item;
            if (sourceItem.ItemType == ItemType.Folder)
            {
                item = new FolderMetaData();
                item.ItemType = ItemType.Folder;
            }
            else
            {
                item = new ItemMetaData();
                item.ItemType = ItemType.File;
            }
            item.Id = sourceItem.ItemId;
            item.Name = sourceItem.RemoteName.Substring(2);
            item.Author = "unknown";
            item.LastModifiedDate = sourceItem.RemoteDate;
            item.Revision = sourceItem.RemoteChangesetId;
            item.DownloadUrl = sourceItem.DownloadUrl;
            return item;
        }

        static void SetProperties(Dictionary<string, FolderMetaData> folders,
                                  Dictionary<string, FolderProperties> properties)
        {
            foreach (KeyValuePair<string, FolderProperties> folderProperties in properties)
            {
                string folderName = folderProperties.Key;
                FolderMetaData folder = folders[folderName];

                foreach (Property property in folderProperties.Value.Properties)
                    folder.Properties[property.Name] = property.Value;

                foreach (FileProperties fileProperties in folderProperties.Value.FileProperties)
                {
                    ItemMetaData file = FindItem(folder, fileProperties.Path.Substring(1));

                    foreach (Property property in fileProperties.Properties)
                        file.Properties[property.Name] = property.Value;
                }
            }
        }

        static ItemMetaData FindItem(FolderMetaData folder,
                                     string name)
        {
            foreach (ItemMetaData item in folder.Items)
                if (item.Name == name)
                    return item;

            return null;
        }

        public FolderMetaData GetChangedItems(string path,
                                              int versionFrom,
                                              int versionTo,
                                              UpdateReportData reportData)
        {
            Dictionary<string, int> clientExistingFiles = new Dictionary<string, int>();
            if (reportData.Entries != null)
                foreach (EntryData entryData in reportData.Entries)
                    clientExistingFiles[path + "/" + entryData.path] = int.Parse(entryData.Rev);

            Dictionary<string, string> clientDeletedFiles = new Dictionary<string, string>();
            if (reportData.Missing != null)
                foreach (string missingPath in reportData.Missing)
                    clientDeletedFiles[path + "/" + missingPath] = missingPath;

            FolderMetaData root = (FolderMetaData)GetItems(versionTo, path, Recursion.None);
            if (versionFrom != versionTo)
            {
                if (root == null && reportData.UpdateTarget != null)
                {
                    root = new FolderMetaData();
                    DeleteMetaData deletedFile = new DeleteMetaData();
                    deletedFile.Name = reportData.UpdateTarget;
                    deletedFile.ItemType = ItemType.File;
                    root.Items.Add(deletedFile);
                }
                else
                {
                    LogItem logItem = GetLog(path, versionFrom + 1, versionTo, Recursion.Full, Int32.MaxValue);

                    foreach (SourceItemHistory history in logItem.History)
                    {
                        foreach (SourceItemChange change in history.Changes)
                        {
                            string[] nameParts = change.Item.RemoteName.Substring(2 + path.Length).Split('/');
                            string changePath = change.Item.RemoteName.Substring(1);
                            if (((change.ChangeType & ChangeType.Add) == ChangeType.Add) || ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit))
                            {
                                if (!IsChangeAlreadyCurrentInClientState(change, clientExistingFiles, clientDeletedFiles))
                                {
                                    ProcessAddedFile(path, change, root, versionTo);
                                }
                            }
                            else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                            {
                                if (!IsChangeAlreadyCurrentInClientState(change, clientExistingFiles, clientDeletedFiles))
                                {
                                    ProcessDeletedFile(path, change.Item.RemoteName, change, root, versionTo);
                                }
                            }
                            else if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                            {
                                ItemMetaData oldItem = GetItem(history.ChangeSetID - 1, change.Item.ItemId);
                                ProcessDeletedFile(path, "$/" + oldItem.Name, change, root, versionTo);
                                ProcessAddedFile(path, change, root, versionTo);
                            }
                            else
                            {
                                throw new Exception("Unrecognized change type " + change.ChangeType);
                            }
                        }
                    }
                }
            }

            return root;
        }

        private void ProcessDeletedFile(string path, string remoteName, SourceItemChange change, FolderMetaData root, int versionTo)
        {
            string[] nameParts = remoteName.Substring(2 + path.Length).Split('/');
            string changePath = remoteName.Substring(1);
            string folderName = path.Substring(1);
            FolderMetaData folder = root;
            for (int i = 0; i < nameParts.Length; i++)
            {
                folderName += "/" + nameParts[i];
                ItemMetaData item = FindItem(folder, nameParts[i]);
                if (item == null)
                {
                    if (i == nameParts.Length - 1)
                    {
                        if (change.Item.ItemType == ItemType.File)
                        {
                            item = new DeleteMetaData();
                            item.ItemType = ItemType.File;
                        }
                        else
                        {
                            item = new DeleteFolderMetaData();
                            item.ItemType = ItemType.Folder;
                        }
                        item.Name = remoteName.Substring(2);
                    }
                    else
                    {
                        item = GetItems(versionTo, folderName, Recursion.None);
                        if (item == null)
                        {
                            item = new DeleteFolderMetaData();
                            item.ItemType = ItemType.Folder;
                            item.Name = folderName;
                        }
                    }
                    folder.Items.Add(item);
                    if (i != nameParts.Length - 1)
                    {
                        folder = (FolderMetaData)item;
                    }
                }
            }
        }

        private void ProcessAddedFile(string path, SourceItemChange change, FolderMetaData root, int versionTo)
        {
            string[] nameParts = change.Item.RemoteName.Substring(2 + path.Length).Split('/');
            string changePath = change.Item.RemoteName.Substring(1);
            string folderName = path.Substring(1);
            FolderMetaData folder = root;
            for (int i = 0; i < nameParts.Length; i++)
            {
                folderName += "/" + nameParts[i];
                ItemMetaData item = FindItem(folder, folderName);
                if (item == null)
                {
                    if ((i == nameParts.Length - 1) && (change.Item.ItemType == ItemType.File))
                        item = GetItems(versionTo, change.Item.RemoteName.Substring(2), Recursion.None);
                    else
                        item = GetItems(versionTo, folderName, Recursion.None);

                    folder.Items.Add(item);
                }
                if (i != nameParts.Length - 1)
                {
                    folder = (FolderMetaData)item;
                }
            }
        }

        private ItemMetaData GetItem(int version, int itemId)
        {
            SourceItem[] items = _sourceControlSvc.QueryItems(_serverUrl, _credentials, new int[] { itemId }, version);
            return ConvertSourceItem(items[0]);
        }

        private bool IsChangeAlreadyCurrentInClientState(SourceItemChange change,
            Dictionary<string, int> clientExistingFiles, Dictionary<string, string> clientDeletedFiles)
        {
            string changePath = change.Item.RemoteName.Substring(1);
            if (((change.ChangeType & ChangeType.Add) == ChangeType.Add) ||
                ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit))
            {
                if ((clientExistingFiles.ContainsKey(changePath)) && (clientExistingFiles[changePath] >= change.Item.RemoteChangesetId))
                {
                    return true;
                }
            }
            else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
            {
                if (clientDeletedFiles.ContainsKey(changePath) || (clientExistingFiles.ContainsKey(changePath) && (clientExistingFiles[changePath] >= change.Item.RemoteChangesetId)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ItemExists(string path)
        {
            return ItemExists(path, -1);
        }

        public bool ItemExists(string path,
                               int version)
        {
            ItemMetaData item = GetItems(version, path, Recursion.None);
            if (item != null)
                return true;
            else
                return false;
        }

        public bool IsDirectory(int version,
                                string path)
        {
            ItemMetaData item = GetItems(version, path, Recursion.None);
            if (item.ItemType == ItemType.Folder)
                return true;
            else
                return false;
        }

        public int GetLatestVersion()
        {
            return _sourceControlSvc.GetLatestChangeset(_serverUrl, _credentials);
        }
    }
}
