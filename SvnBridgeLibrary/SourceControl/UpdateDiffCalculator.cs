using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Protocol;

namespace SvnBridge.SourceControl
{
    public class UpdateDiffCalculator
    {
        private readonly ISourceControlProvider sourceControlProvider;
        private readonly ISourceControlUtility sourceControlUtility;
        public UpdateDiffCalculator(ISourceControlProvider sourceControlProvider, ISourceControlUtility sourceControlUtility)
        {
            this.sourceControlProvider = sourceControlProvider;
            this.sourceControlUtility = sourceControlUtility;
        }

        private void CalculateChangeBetweenVersions(string path,
                                                    FolderMetaData root,
                                                    int versionTo,
                                                    int versionFrom,
                                                    IDictionary<string, int> clientExistingFiles,
                                                    IDictionary<string, string> clientDeletedFiles)
        {
            versionFrom++;
            int lastVersion = versionTo + 1;
            while (versionFrom != lastVersion)
            {
                LogItem logItem = sourceControlProvider.GetLog(path, versionFrom, lastVersion - 1, Recursion.Full, 256);
                if (logItem.History.Length == 0)
                {
                    lastVersion = versionFrom;
                }

                foreach (SourceItemHistory history in logItem.History)
                {
                    lastVersion = history.ChangeSetID;
                    for (int i = history.Changes.Count - 1; i >= 0; i--)
                    {
                        SourceItemChange change = history.Changes[i];
                        if (((change.ChangeType & ChangeType.Add) == ChangeType.Add) ||
                            ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit) ||
                            ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch))
                        {
                            if (!change.Item.RemoteName.EndsWith("/" + Constants.PROP_FOLDER))
                            {
                                string remoteName = change.Item.RemoteName;
                                bool propertyChange = false;
                                if (remoteName.Contains("/" + Constants.PROP_FOLDER + "/"))
                                {
                                    propertyChange = true;
                                    if (remoteName.EndsWith("/" + Constants.PROP_FOLDER + "/" + Constants.FOLDER_PROP_FILE))
                                    {
                                        remoteName =
                                            remoteName.Substring(0,
                                                                 remoteName.IndexOf("/" + Constants.PROP_FOLDER + "/" +
                                                                                   Constants.FOLDER_PROP_FILE));
                                    }
                                    else
                                    {
                                        remoteName = remoteName.Replace("/" + Constants.PROP_FOLDER + "/", "/");
                                    }
                                }
                                ProcessAddedItem(path,
                                                 remoteName,
                                                 change,
                                                 propertyChange,
                                                 root,
                                                 versionTo,
                                                 clientExistingFiles,
                                                 clientDeletedFiles);
                            }
                        }
                        else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                        {
                            if (!change.Item.RemoteName.EndsWith("/" + Constants.PROP_FOLDER) &&
                                !change.Item.RemoteName.Contains("/" + Constants.PROP_FOLDER + "/"))
                            {
                                ProcessDeletedFile(path,
                                                   change.Item.RemoteName,
                                                   change,
                                                   root,
                                                   versionTo,
                                                   clientExistingFiles,
                                                   clientDeletedFiles);
                            }
                        }
                        else if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                        {
                            ItemMetaData oldItem = sourceControlUtility.GetItem(history.ChangeSetID - 1, change.Item.ItemId);
                            ProcessDeletedFile(path,
                                               oldItem.Name,
                                               change,
                                               root,
                                               versionTo,
                                               clientExistingFiles,
                                               clientDeletedFiles);
                            ProcessAddedItem(path,
                                             change.Item.RemoteName,
                                             change,
                                             false,
                                             root,
                                             versionTo,
                                             clientExistingFiles,
                                             clientDeletedFiles);
                        }
                        else
                        {
                            throw new InvalidOperationException("Unrecognized change type " + change.ChangeType);
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> GetClientDeletedFiles(string path,
                                                                 UpdateReportData reportData)
        {
            Dictionary<string, string> clientDeletedFiles = new Dictionary<string, string>();
            if (reportData.Missing != null)
            {
                foreach (string missingPath in reportData.Missing)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        clientDeletedFiles["/" + missingPath] = missingPath;
                    }
                    else
                    {
                        clientDeletedFiles["/" + path + "/" + missingPath] = missingPath;
                    }
                }
            }
            return clientDeletedFiles;
        }

        private static Dictionary<string, int> GetClientExistingFiles(string path,
                                                              UpdateReportData reportData)
        {
            Dictionary<string, int> clientExistingFiles = new Dictionary<string, int>();
            if (reportData.Entries != null)
            {
                foreach (EntryData entryData in reportData.Entries)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        clientExistingFiles["/" + entryData.path] = int.Parse(entryData.Rev);
                    }
                    else
                    {
                        clientExistingFiles["/" + path + "/" + entryData.path] = int.Parse(entryData.Rev);
                    }
                }
            }
            return clientExistingFiles;
        }

        public void CalculateDiff(string path,
                                  int versionTo,
                                  int versionFrom,
                                  FolderMetaData root,
                                  UpdateReportData updateReportData)
        {
            Dictionary<string, int> clientExistingFiles = GetClientExistingFiles(path, updateReportData);
            Dictionary<string, string> clientDeletedFiles = GetClientDeletedFiles(path, updateReportData);
            CalculateChangeBetweenVersions(path, root, versionTo, versionFrom, clientExistingFiles, clientDeletedFiles);
        }

        private void ProcessAddedItem(string path,
                                     string remoteName,
                                     SourceItemChange change,
                                     bool propertyChange,
                                     FolderMetaData root,
                                     int versionTo,
                                     IDictionary<string, int> clientExistingFiles,
                                     IDictionary<string, string> clientDeletedFiles)
        {
            if (
                !IsChangeAlreadyCurrentInClientState(ChangeType.Add,
                                                     remoteName,
                                                     change.Item.RemoteChangesetId,
                                                     clientExistingFiles,
                                                     clientDeletedFiles))
            {
                if (remoteName.Length == path.Length)
                {
                    ItemMetaData item = sourceControlProvider.GetItems(versionTo, remoteName, Recursion.None);
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
                        {
                            lastNamePart = true;
                        }

                        itemName += "/" + nameParts[i];
                        ItemMetaData item = sourceControlUtility.FindItem(folder, itemName);
                        if (item == null)
                        {
                            item = sourceControlProvider.GetItems(versionTo, itemName, Recursion.None);
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
                        else if (((item is DeleteFolderMetaData) || (item is DeleteMetaData)) &&
                                 ((change.ChangeType & ChangeType.Add) == ChangeType.Add))
                        {
                            if (!propertyChange)
                            {
                                folder.Items.Remove(item);
                            }
                        }
                        if (!lastNamePart)
                        {
                            folder = (FolderMetaData)item;
                        }
                    }
                }
            }
        }

        private void ProcessDeletedFile(string path,
                                     string remoteName,
                                     SourceItemChange change,
                                     FolderMetaData root,
                                     int versionTo,
                                     IDictionary<string, int> clientExistingFiles,
                                     IDictionary<string, string> clientDeletedFiles)
        {
            if (
                !IsChangeAlreadyCurrentInClientState(ChangeType.Delete,
                                                     remoteName,
                                                     change.Item.RemoteChangesetId,
                                                     clientExistingFiles,
                                                     clientDeletedFiles))
            {
                string[] nameParts = remoteName.Substring(path.Length + 1).Split('/');
                string folderName = path;
                FolderMetaData folder = root;
                for (int i = 0; i < nameParts.Length; i++)
                {
                    folderName += "/" + nameParts[i];
                    ItemMetaData item = sourceControlUtility.FindItem(folder, folderName);
                    if (item == null)
                    {
                        if (i == nameParts.Length - 1)
                        {
                            if (change.Item.ItemType == ItemType.File)
                            {
                                item = new DeleteMetaData();
                            }
                            else
                            {
                                item = new DeleteFolderMetaData();
                            }

                            item.Name = remoteName;
                        }
                        else
                        {
                            item = sourceControlProvider.GetItems(versionTo, folderName, Recursion.None);
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


        private static bool IsChangeAlreadyCurrentInClientState(ChangeType changeType,
                                                         string itemPath,
                                                         int itemRevision,
                                                         IDictionary<string, int> clientExistingFiles,
                                                         IDictionary<string, string> clientDeletedFiles)
        {
            string changePath = "/" + itemPath;
            if (((changeType & ChangeType.Add) == ChangeType.Add) ||
                ((changeType & ChangeType.Edit) == ChangeType.Edit))
            {
                if ((clientExistingFiles.ContainsKey(changePath)) && (clientExistingFiles[changePath] >= itemRevision))
                {
                    return true;
                }

                foreach (string clientExistingFile in clientExistingFiles.Keys)
                {
                    if (changePath.StartsWith(clientExistingFile + "/") &&
                        (clientExistingFiles[clientExistingFile] >= itemRevision))
                    {
                        return true;
                    }
                }
            }
            else if ((changeType & ChangeType.Delete) == ChangeType.Delete)
            {
                if (clientDeletedFiles.ContainsKey(changePath) ||
                    (clientExistingFiles.ContainsKey(changePath) && (clientExistingFiles[changePath] >= itemRevision)))
                {
                    return true;
                }

                foreach (string clientDeletedFile in clientDeletedFiles.Keys)
                {
                    if (changePath.StartsWith(clientDeletedFile + "/"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}