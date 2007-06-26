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

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlProvider : ISourceControlProvider
    {
        // Constants

        const string SERVER_PATH = "$/";
        const string LOCAL_PREFIX = @"C:\";

        // Fields

        static Dictionary<string, Activity> activities = new Dictionary<string, Activity>();
        ICredentials credentials;
        string serverUrl;
        SourceControlService sourceControlSvc;
        WebTransferService webTransferSvc;

        // Lifetime

        class Activity
        {
            public string Comment;
            public List<ActivityItem> MergeList = new List<ActivityItem>();
            public Dictionary<string, Dictionary<string, string>> FolderProperties = new Dictionary<string, Dictionary<string, string>>();
            public Dictionary<string, Dictionary<string, Dictionary<string, string>>> FileProperties = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        }

        class ActivityItem
        {
            public string Path;
            public ItemType FileType;

            public ActivityItem(string path,
                                ItemType fileType)
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
            this.serverUrl = serverUrl;
            this.credentials = credentials;

            if (this.credentials == null)
            {
                Uri uri = new Uri(serverUrl);

                if (uri.Host.ToLowerInvariant().EndsWith("codeplex.com"))
                {
                    CredentialCache cache = new CredentialCache();
                    cache.Add(uri, "Basic", new NetworkCredential("anonymous", null));
                    this.credentials = cache;
                }
                else
                    this.credentials = CredentialCache.DefaultNetworkCredentials;
            }

            RegistrationWebSvcFactory registrationFactory = new RegistrationWebSvcFactory();
            FileSystem fileSystem = new FileSystem();
            RegistrationService registrationSvc = new RegistrationService(registrationFactory);

            webTransferSvc = new WebTransferService(fileSystem);
            sourceControlSvc = new SourceControlService(registrationSvc,
                                                        new RepositoryWebSvcFactory(registrationFactory),
                                                        webTransferSvc,
                                                        fileSystem);
        }

        // Methods

        public void MakeActivity(string activityId)
        {
            string workspaceComment = "Temporary workspace for edit-merge-commit";
            sourceControlSvc.CreateWorkspace(serverUrl, credentials, activityId, workspaceComment);
            string localPath = LOCAL_PREFIX + activityId;
            sourceControlSvc.AddWorkspaceMapping(serverUrl, credentials, activityId, SERVER_PATH, localPath);
            activities[activityId] = new Activity();
        }

        public void SetActivityComment(string activityId,
                                       string comment)
        {
            activities[activityId].Comment = comment;
        }

        public void SetProperty(string activityId,
                                string path,
                                string property,
                                string value)
        {
            Activity activity = activities[activityId];

            switch (property)
            {
                case "ignore":
                    if (!activity.FolderProperties.ContainsKey(path))
                        activity.FolderProperties[path] = new Dictionary<string, string>();

                    activity.FolderProperties[path][property] = value;
                    break;

                case "mime-type":
                    string folder = path.Substring(0, path.LastIndexOf('/'));

                    if (!activity.FileProperties.ContainsKey(folder))
                        activity.FileProperties[folder] = new Dictionary<string, Dictionary<string, string>>();

                    if (!activity.FileProperties[folder].ContainsKey(path))
                        activity.FileProperties[folder][path] = new Dictionary<string, string>();

                    if (!activity.FolderProperties.ContainsKey(path))
                        activity.FolderProperties[folder] = new Dictionary<string, string>();

                    activity.FileProperties[folder][path][property] = value;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported property type");
            }
        }

        public void WriteFile(string activityId,
                              string path,
                              byte[] fileData)
        {
            string localPath = path.Replace('/', '\\');
            ItemMetaData item;
            string existingPath = path.Substring(1);

            do
            {
                existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
                item = GetItems(-1, existingPath, Recursion.None);
            }
            while (item == null);

            List<LocalUpdate> updates2 = new List<LocalUpdate>();
            updates2.Add(LocalUpdate.FromLocal(item.Id,
                                               LOCAL_PREFIX + activityId + localPath.Substring(0, localPath.LastIndexOf('\\')),
                                               item.Revision));
            // BUG: Should not be item.Revision!

            item = GetItems(-1, path.Substring(1), Recursion.None);
            if (item != null)
                updates2.Add(LocalUpdate.FromLocal(item.Id,
                                                   LOCAL_PREFIX + activityId + localPath,
                                                   item.Revision));
            // BUG: Should not be item.Revision!

            sourceControlSvc.UpdateLocalVersions(serverUrl, credentials, activityId, updates2);

            List<PendRequest> pendRequests = new List<PendRequest>();

            if (item == null)
                pendRequests.Add(PendRequest.AddFile(LOCAL_PREFIX + activityId + localPath, TfsUtil.CodePage_ANSI));
            else
                pendRequests.Add(PendRequest.Edit(LOCAL_PREFIX + activityId + localPath));

            sourceControlSvc.PendChanges(serverUrl, credentials, activityId, pendRequests);
            sourceControlSvc.UploadFileFromBytes(serverUrl, credentials, activityId, fileData, SERVER_PATH + path);
            activities[activityId].MergeList.Add(new ActivityItem(SERVER_PATH + path, ItemType.File));
        }

        public void DeleteItem(string activityId,
                               string path)
        {
            string localPath = LOCAL_PREFIX + activityId + path.Replace('/', '\\');

            ItemMetaData item = GetItems(-1, path, Recursion.None);
            List<LocalUpdate> updates2 = new List<LocalUpdate>();
            updates2.Add(LocalUpdate.FromLocal(item.Id, localPath, item.Revision));
            sourceControlSvc.UpdateLocalVersions(serverUrl, credentials, activityId, updates2);
            // BUG: Should not be item.Revision!

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.Delete(localPath));
            sourceControlSvc.PendChanges(serverUrl, credentials, activityId, pendRequests);

            activities[activityId].MergeList.Add(new ActivityItem(SERVER_PATH + path, item.ItemType));
        }

        public void MakeCollection(string activityId,
                                   string path)
        {
            ItemMetaData item;
            string existingPath = path.Substring(1);
            do
            {
                existingPath = existingPath.Substring(0, existingPath.LastIndexOf('/'));
                item = GetItems(-1, existingPath, Recursion.None);
            }
            while (item == null);
            string localPath = activityId + path;
            localPath = localPath.Replace('/', '\\');

            List<LocalUpdate> updates2 = new List<LocalUpdate>();
            updates2.Add(LocalUpdate.FromLocal(item.Id, LOCAL_PREFIX + localPath.Substring(0, localPath.LastIndexOf('\\')), item.Revision));
            // BUG: Should not be item.Revision!

            sourceControlSvc.UpdateLocalVersions(serverUrl, credentials, activityId, updates2);

            List<PendRequest> pendRequests = new List<PendRequest>();
            pendRequests.Add(PendRequest.AddFolder(LOCAL_PREFIX + localPath));
            sourceControlSvc.PendChanges(serverUrl, credentials, activityId, pendRequests);
            activities[activityId].MergeList.Add(new ActivityItem(SERVER_PATH + path, ItemType.Folder));
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

        void UpdateProperties(string activityId)
        {
            Activity activity = activities[activityId];

            foreach (KeyValuePair<string, Dictionary<string, string>> folderProperties in activity.FolderProperties)
            {
                string path = folderProperties.Key + "/.svnbridge";
                FolderProperties existingProperties = new FolderProperties();
                ItemMetaData item = GetItems(-1, path, Recursion.None);

                if (item != null)
                {
                    byte[] properties = ReadFile(item);
                    existingProperties = Helper.DeserializeXml<FolderProperties>(properties);
                }

                foreach (KeyValuePair<string, string> newProperty in folderProperties.Value)
                {
                    bool found = false;

                    foreach (Property existingProperty in existingProperties.Properties)
                        if (newProperty.Key == existingProperty.Name)
                        {
                            existingProperty.Value = newProperty.Value;
                            found = true;
                        }

                    if (!found)
                    {
                        Property addProperty = new Property();
                        addProperty.Name = newProperty.Key;
                        addProperty.Value = newProperty.Value;
                        existingProperties.Properties.Add(addProperty);
                    }
                }

                if (activity.FileProperties.ContainsKey(folderProperties.Key))
                    foreach (KeyValuePair<string, Dictionary<string, string>> fileProperties in activity.FileProperties[folderProperties.Key])
                        foreach (KeyValuePair<string, string> fileProperty in fileProperties.Value)
                            UpdateFileProperty(existingProperties, fileProperties.Key, fileProperty.Key, fileProperty.Value);

                WriteFile(activityId, path, Helper.SerializeXml(existingProperties));
            }
        }

        public MergeActivityResponse MergeActivity(string activityId)
        {
            UpdateProperties(activityId);
            List<string> commitServerList = new List<string>();
            foreach (ActivityItem item in activities[activityId].MergeList)
            {
                commitServerList.Add(item.Path);
            }
            int changesetId;
            if (commitServerList.Count > 0)
                changesetId = sourceControlSvc.Commit(serverUrl, credentials, activityId, activities[activityId].Comment, commitServerList);
            else
                changesetId = GetLatestVersion();

            MergeActivityResponse mergeResponse = new MergeActivityResponse(changesetId, DateTime.Now, "unknown");
            foreach (ActivityItem item in activities[activityId].MergeList)
            {
                mergeResponse.Items.Add(new MergeActivityResponseItem(item.FileType, item.Path.Substring(2)));
            }
            return mergeResponse;
        }

        public void DeleteActivity(string activityId)
        {
            sourceControlSvc.DeleteWorkspace(serverUrl, credentials, activityId);
            activities.Remove(activityId);
        }

        public byte[] ReadFile(ItemMetaData item)
        {
            return webTransferSvc.DownloadBytes(item.DownloadUrl, credentials);
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

            return sourceControlSvc.QueryLog(serverUrl, credentials, serverPath, changesetFrom, changesetTo, recursionType, maxCount);
        }

        public ItemMetaData GetItems(int version,
                                     string path,
                                     Recursion recursion)
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
            SourceItem[] items = sourceControlSvc.QueryItems(serverUrl, credentials, SERVER_PATH + path, recursionType, versionSpec, DeletedState.NonDeleted, ItemType.Any);
            Dictionary<string, FolderMetaData> folders = new Dictionary<string, FolderMetaData>();
            ItemMetaData firstItem = null;
            for (int i = 0; i < items.Length; i++)
            {
                ItemMetaData item;
                if (items[i].ItemType == ItemType.Folder)
                {
                    item = new FolderMetaData();
                    item.ItemType = ItemType.Folder;
                }
                else
                {
                    item = new ItemMetaData();
                    item.ItemType = ItemType.File;
                }
                item.Id = items[i].ItemId;
                item.Name = items[i].RemoteName.Substring(2);
                item.Author = "unknown";
                item.LastModifiedDate = items[i].RemoteDate;
                item.Revision = items[i].RemoteChangesetId;
                item.DownloadUrl = items[i].DownloadUrl;
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
                    string folderName = item.Name.Substring(0, item.Name.LastIndexOf('/')).ToLower();
                    string filename = item.Name.Substring(item.Name.LastIndexOf('/') + 1);
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
                LogItem logItem = GetLog(path, versionFrom + 1, versionTo, Recursion.Full, Int32.MaxValue);

                foreach (SourceItemHistory history in logItem.History)
                {
                    foreach (SourceItemChange change in history.Changes)
                    {
                        string[] nameParts = change.Item.RemoteName.Substring(2 + path.Length).Split('/');
                        string changePath = change.Item.RemoteName.Substring(1);
                        if (((change.ChangeType & ChangeType.Add) == ChangeType.Add) || ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit))
                        {
                            if ((!clientExistingFiles.ContainsKey(changePath)) || (clientExistingFiles[changePath] < change.Item.RemoteChangesetId))
                            {
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
                        }
                        else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                        {
                            if (!clientDeletedFiles.ContainsKey(changePath))
                            {
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
                                            item.Name = change.Item.RemoteName.Substring(2);
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
                        }
                        else
                        {
                            throw new Exception("Unrecognized change type " + change.ChangeType);
                        }
                    }
                }
            }

            return root;
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
            return sourceControlSvc.GetLatestChangeset(serverUrl, credentials);
        }

        // Inner types
    }
}
