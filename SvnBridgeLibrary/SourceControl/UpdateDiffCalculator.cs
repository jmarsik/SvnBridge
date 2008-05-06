using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.Utility;

namespace SvnBridge.SourceControl
{
    public class UpdateDiffCalculator
    {
        private readonly ISourceControlProvider sourceControlProvider;

        private readonly IDictionary<ItemMetaData, bool> additionForPropertyChangeOnly =
            new Dictionary<ItemMetaData, bool>();

        private Dictionary<string, int> clientExistingFiles;
        private Dictionary<string, string> clientDeletedFiles;
        private readonly List<string> renamedItemsToBeCheckedForDeletedChildren = new List<string>();

        public UpdateDiffCalculator(ISourceControlProvider sourceControlProvider)
        {
            this.sourceControlProvider = sourceControlProvider;
        }

        public void CalculateDiff(string checkoutRootPath,
                                  int versionTo,
                                  int versionFrom,
                                  FolderMetaData root,
                                  UpdateReportData updateReportData)
        {
            clientExistingFiles = GetClientExistingFiles(checkoutRootPath, updateReportData);
            clientDeletedFiles = GetClientDeletedFiles(checkoutRootPath, updateReportData);
            if (versionFrom != versionTo)
            {
                CalculateChangeBetweenVersions(checkoutRootPath,
                                           root,
                                           versionFrom,
                                           versionTo);
            }

            if (updateReportData.Entries != null)
            {
                foreach (EntryData data in updateReportData.Entries)
                {
                    int itemVersionFrom = int.Parse(data.Rev);
                    // we already checked that version at the root level
                    if (itemVersionFrom == versionFrom)
                        continue;
                    if (itemVersionFrom != versionTo)
                    {
                        FindOrCreateResults results = FindItemOrCreateItem(root, checkoutRootPath, data.path, versionTo, Recursion.None);

                        bool changed = CalculateChangeBetweenVersions(checkoutRootPath + "/" + data.path, root,
                                                       itemVersionFrom, versionTo);
                        if (changed == false)
                            results.RevertAddition();
                    }
                }
            }
            foreach (string missingItem in clientDeletedFiles.Values)
            {
                if (sourceControlProvider.ItemExists(checkoutRootPath + "/" + missingItem, versionTo))
                {
                    FindItemOrCreateItem(root, checkoutRootPath, missingItem, versionTo, Recursion.Full);
                }
            }
            FlattenDeletedFolders(root);
            RemoveMissingItemsWhichAreChildrenOfRenamedItem(root);
            VerifyNoMissingItemMetaDataRemained(root);
        }

        private void RemoveMissingItemsWhichAreChildrenOfRenamedItem(FolderMetaData root)
        {
            foreach (string item in renamedItemsToBeCheckedForDeletedChildren)
            {
                RemoveMissingItemsWhichAreChildrenOfRenamedItem(item, root);
            }
        }

        private static void VerifyNoMissingItemMetaDataRemained(FolderMetaData root)
        {
            foreach (ItemMetaData item in root.Items)
            {
                if (item is MissingFolderMetaData)
                    throw new InvalidOperationException("Found missing item:" + item + " but those should not be returned from UpdateDiffCalculator");
                if (item is FolderMetaData)
                    VerifyNoMissingItemMetaDataRemained((FolderMetaData)item);
            }
        }

        private FindOrCreateResults FindItemOrCreateItem(FolderMetaData root,
            string pathRoot, string path, int targetVersion, Recursion recursion)
        {
            FindOrCreateResults results = new FindOrCreateResults();
            FolderMetaData folder = root;
            string[] parts = path.Split('/');
            string itemName = pathRoot;
            ItemMetaData item = null;
            for (int i = 0; i < parts.Length; i++)
            {
                itemName += "/" + parts[i];
                item = folder.FindItem(itemName);
                bool lastNamePart = i == parts.Length - 1;
                if (item == null)
                {
                    if (lastNamePart)
                    {
                        item = sourceControlProvider.GetItems(targetVersion, itemName, recursion);
                    }
                    else
                    {
                        FolderMetaData subFolder = (FolderMetaData)sourceControlProvider.GetItems(targetVersion, itemName, recursion);
                        item = subFolder;
                    }
                    item = item ?? new MissingFolderMetaData(itemName, targetVersion);
                    folder.Items.Add(item);
                    if (results.FirstItemAdded == null)
                    {
                        results.FirstItemAdded = item;
                        results.FirstItemAddedFolder = folder;
                    }
                }
                if (lastNamePart == false)
                {
                    folder = (FolderMetaData)item;
                }
            }
            results.Item = item;
            return results;
        }

        private bool CalculateChangeBetweenVersions(string checkoutRootPath,
                                                    FolderMetaData root,
                                                    int sourceVersion,
                                                    int targetVersion)
        {
            bool updatingForwardInTime = sourceVersion <= targetVersion;
            int lastVersion = sourceVersion;
            bool changed = false;
            while (targetVersion != lastVersion)
            {
                int previousLoopLastVersion = lastVersion;
                LogItem logItem =
                    sourceControlProvider.GetLog(checkoutRootPath,
                                                 Math.Min(lastVersion, targetVersion) + 1,
                                                 Math.Max(lastVersion, targetVersion),
                                                 Recursion.Full, 256);

                foreach (SourceItemHistory history in
                    Helper.SortHistories(updatingForwardInTime, logItem.History))
                {
                    changed = true;
                    lastVersion = history.ChangeSetID;
                    if (updatingForwardInTime == false)
                    {
                        lastVersion -= 1;
                    }

                    PrePopulateCacheWithChanges(history, lastVersion);
                    // we need to go over the changeset in reverse order so we will process
                    // all the files first, and build the folder hierarchy that way
                    for (int i = history.Changes.Count - 1; i >= 0; i--)
                    {
                        SourceItemChange change = history.Changes[i];
                        if (IsAddOperation(change, updatingForwardInTime))
                        {
                            PerformAdd(targetVersion, checkoutRootPath, change, root);
                        }
                        else if (IsDeleteOperation(change, updatingForwardInTime))
                        {
                            PerformDelete(targetVersion, checkoutRootPath, change, root);
                        }
                        else if (IsEditOperation(change))
                        {
                            // We may have edit & rename operations
                            if (IsRenameOperation(change))
                            {
                                PerformRename(targetVersion, checkoutRootPath, change, root, updatingForwardInTime);
                            }
                            if (updatingForwardInTime == false)
                            {
                                change.Item.RemoteChangesetId -= 1;// we turn the edit around, basically
                            }
                            PerformAdd(targetVersion, checkoutRootPath, change, root);
                        }
                        else if (IsRenameOperation(change))
                        {
                            PerformRename(targetVersion, checkoutRootPath, change, root, updatingForwardInTime);
                        }
                        else
                        {
                            throw new NotSupportedException("Unsupported change type " + change.ChangeType);
                        }
                    }
                }
                // No change was made, break out
                if (previousLoopLastVersion == lastVersion)
                {
                    break;
                }
            }
            return changed;
        }

        private void PrePopulateCacheWithChanges(SourceItemHistory history, int revision)
        {
            List<SourceItemChange> list = new List<SourceItemChange>(history.Changes);
            list.Sort(delegate(SourceItemChange x, SourceItemChange y)
            {
                return x.Item.RemoteName.Length.CompareTo(y.Item.RemoteName.Length);
            });

            list.ForEach(delegate(SourceItemChange obj)
            {
                sourceControlProvider.GetItems(revision, obj.Item.RemoteName, Recursion.None);
            });
        }

        private void PerformAdd(int targetVersion,
                                string checkoutRootPath,
                                SourceItemChange change,
                                FolderMetaData root)
        {
            if (change.Item.RemoteName.EndsWith("/" + Constants.PropFolder))
            {
                return;
            }
            ItemInformation itemInformation = GetItemInformation(change);

            ProcessAddedItem(checkoutRootPath,
                             itemInformation.RemoteName,
                             change,
                             itemInformation.PropertyChange,
                             root,
                             targetVersion);
        }

        private void PerformDelete(int targetVersion,
                                   string checkoutRootPath,
                                   SourceItemChange change,
                                   FolderMetaData root)
        {
            // we ignore it here because this only happens when the related item
            // is delete, and at any rate, this is a SvnBridge implementation detail
            // which the client is not concerned about
            if (change.Item.RemoteName.EndsWith("/" + Constants.PropFolder) ||
                change.Item.RemoteName.Contains("/" + Constants.PropFolder + "/"))
            {
                return;
            }
            ProcessDeletedFile(checkoutRootPath,
                               change.Item.RemoteName,
                               change,
                               root,
                               targetVersion);
        }

        private void PerformRename(int targetVersion, string checkoutRootPath, SourceItemChange change, FolderMetaData root, bool updatingForwardInTime)
        {
            ItemMetaData oldItem = sourceControlProvider.GetPreviousVersionOfItems(new SourceItem[] { change.Item }, change.Item.RemoteChangesetId)[0];

            if (updatingForwardInTime)
            {
                ProcessDeletedFile(checkoutRootPath,
                                   oldItem.Name,
                                   change,
                                   root,
                                   targetVersion);
                ProcessAddedItem(checkoutRootPath,
                                 change.Item.RemoteName,
                                 change,
                                 false,
                                 root,
                                 targetVersion);
            }
            else
            {
                ProcessAddedItem(checkoutRootPath,
                                 oldItem.Name,
                                 change,
                                 false,
                                 root,
                                 targetVersion);

                ProcessDeletedFile(checkoutRootPath,
                                   change.Item.RemoteName,
                                   change,
                                   root,
                                   targetVersion);
            }
            if (change.Item.ItemType == ItemType.Folder)
            {
                string itemName = updatingForwardInTime ? change.Item.RemoteName : oldItem.Name;
                renamedItemsToBeCheckedForDeletedChildren.Add(itemName);
            }

        }

        private static void RemoveMissingItemsWhichAreChildrenOfRenamedItem(string itemName, FolderMetaData root)
        {
            foreach (ItemMetaData data in new List<ItemMetaData>(root.Items))
            {
                string nameMatchingSourceItemConvention = data.Name;
                if (data.Name.StartsWith("/"))
                    nameMatchingSourceItemConvention = data.Name.Substring(1);

                // a child of the currently renamed item
                if (data is MissingFolderMetaData &&
                    nameMatchingSourceItemConvention.StartsWith(itemName, StringComparison.InvariantCultureIgnoreCase))
                {
                    root.Items.Remove(data);
                    continue;
                }
                if (data is FolderMetaData)
                {
                    RemoveMissingItemsWhichAreChildrenOfRenamedItem(itemName, (FolderMetaData)data);
                }

            }
        }

        private static ItemInformation GetItemInformation(SourceItemChange change)
        {
            string remoteName = change.Item.RemoteName;
            bool propertyChange = false;
            if (remoteName.Contains("/" + Constants.PropFolder + "/"))
            {
                propertyChange = true;
                if (remoteName.EndsWith("/" + Constants.PropFolder + "/" + Constants.FolderPropFile))
                {
                    remoteName =
                        remoteName.Substring(0,
                                             remoteName.IndexOf("/" + Constants.PropFolder + "/" +
                                                                Constants.FolderPropFile));
                }
                else
                {
                    remoteName = remoteName.Replace("/" + Constants.PropFolder + "/", "/");
                }
            }
            return new ItemInformation(propertyChange, remoteName);
        }

        private static bool IsRenameOperation(SourceItemChange change)
        {
            return (change.ChangeType & ChangeType.Rename) == ChangeType.Rename;
        }

        private static bool IsDeleteOperation(SourceItemChange change, bool updatingForwardInTime)
        {
            if (updatingForwardInTime == false)
            {
                return IsAddOperation(change, true);
            }
            return (change.ChangeType & ChangeType.Delete) == ChangeType.Delete;
        }

        private static bool IsAddOperation(SourceItemChange change, bool updatingForwardInTime)
        {
            if (updatingForwardInTime == false)
            {
                return IsDeleteOperation(change, true);
            }
            return ((change.ChangeType & ChangeType.Add) == ChangeType.Add) ||
                   ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch) ||
                   ((change.ChangeType &ChangeType.Merge) == ChangeType.Merge) || 
                   ((change.ChangeType & ChangeType.Undelete) == ChangeType.Undelete);
        }

        private static bool IsEditOperation(SourceItemChange change)
        {
            return (change.ChangeType & ChangeType.Edit) == ChangeType.Edit;
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


        /// <summary>
        /// This method ensures that we are not sending useless deletes to the client
        /// if a folder is to be deleted, all its children are as well, which we remove
        /// at this phase.
        /// </summary>
        /// <param name="parentFolder"></param>
        private static void FlattenDeletedFolders(FolderMetaData parentFolder)
        {
            foreach (ItemMetaData item in parentFolder.Items)
            {
                FolderMetaData folder = item as FolderMetaData;
                if (folder == null)
                {
                    continue;
                }
                if (folder is DeleteFolderMetaData)
                {
                    folder.Items.Clear();
                }
                else
                {
                    FlattenDeletedFolders(folder);
                }

            }
        }

        private void ProcessAddedItem(string checkoutRootPath,
                                      string remoteName,
                                      SourceItemChange change,
                                      bool propertyChange,
                                      FolderMetaData root,
                                      int targetVersion)
        {
            bool alreadyInClientCurrentState = IsChangeAlreadyCurrentInClientState(ChangeType.Add,
                                                                                   remoteName,
                                                                                   change.Item.RemoteChangesetId,
                                                                                   clientExistingFiles,
                                                                                   clientDeletedFiles);
            if (alreadyInClientCurrentState)
            {
                return;
            }

            if (string.Equals(remoteName, checkoutRootPath, StringComparison.InvariantCultureIgnoreCase))
            {
                ItemMetaData item = sourceControlProvider.GetItems(targetVersion, remoteName, Recursion.None);
                root.Properties = item.Properties;
            }
            else
            {
                FolderMetaData folder = root;
                string itemName = checkoutRootPath;
                if (itemName.StartsWith("/") == false)
                    itemName = "/" + itemName;
                string[] nameParts;
                if (checkoutRootPath != "")
                    nameParts = remoteName.Substring(checkoutRootPath.Length + 1).Split('/');
                else
                    nameParts = remoteName.Split('/');

                for (int i = 0; i < nameParts.Length; i++)
                {
                    bool lastNamePart = false;
                    if (i == nameParts.Length - 1)
                        lastNamePart = true;

                    itemName += "/" + nameParts[i];
                    ItemMetaData item = folder.FindItem(itemName);
                    if (item == null)
                    {
                        item = sourceControlProvider.GetItems(targetVersion, itemName, Recursion.None);
                        if (item == null)
                        {
                            item = new MissingFolderMetaData(itemName, targetVersion);
                        }
                        if (!lastNamePart)
                        {
                            StubFolderMetaData stubFolder = new StubFolderMetaData();
                            stubFolder.RealFolder = (FolderMetaData)item;
                            stubFolder.Name = "/" + item.Name;
                            stubFolder.ItemRevision = item.ItemRevision;
                            stubFolder.PropertyRevision = item.PropertyRevision;
                            stubFolder.LastModifiedDate = item.LastModifiedDate;
                            stubFolder.Author = item.Author;
                            item = stubFolder;
                        }
                        folder.Items.Add(item);
                        SetAdditionForPropertyChangeOnly(item, propertyChange);
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
                    if (lastNamePart == false)
                    {
                        folder = (FolderMetaData)item;
                    }
                }
            }
        }

        private void SetAdditionForPropertyChangeOnly(ItemMetaData item, bool propertyChange)
        {
            if (item == null)
                return;
            if (propertyChange == false)
            {
                additionForPropertyChangeOnly[item] = propertyChange;
            }
            else
            {
                if (additionForPropertyChangeOnly.ContainsKey(item) == false)
                    additionForPropertyChangeOnly[item] = propertyChange;
            }
        }

        private void ProcessDeletedFile(string checkoutRootPath,
                                        string remoteName,
                                        SourceItemChange change,
                                        FolderMetaData root,
                                        int targetVersion)
        {
            bool alreadyChangedInCurrentClientState = IsChangeAlreadyCurrentInClientState(ChangeType.Delete,
                                                                                          remoteName,
                                                                                          change.Item.RemoteChangesetId,
                                                                                          clientExistingFiles,
                                                                                          clientDeletedFiles);
            if (alreadyChangedInCurrentClientState)
            {
                root.RemoveMissingItem(remoteName);
                return;
            }

            string folderName = checkoutRootPath;
            string[] nameParts = remoteName.Substring(checkoutRootPath.Length)
                .Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length < 2)
            {
                HandleDeleteItem(remoteName, change, remoteName, root, true, targetVersion);
                return;
            }

            FolderMetaData folder = root;
            string separator = folderName != "" ? "/" : "";
            folderName += separator + nameParts[0];
            HandleDeleteItem(remoteName, change, folderName, folder, false, targetVersion);
            for (int i = 1; i < nameParts.Length; i++)
            {
                bool isLastNamePart = i == nameParts.Length - 1;

                folderName += "/" + nameParts[i];

                HandleDeleteItem(remoteName, change, folderName, folder, isLastNamePart, targetVersion);
            }
        }

        private void HandleDeleteItem(string remoteName, SourceItemChange change, string folderName, FolderMetaData folder, bool isLastNamePart, int targetVersion)
        {
            ItemMetaData item = folder.FindItem(folderName);
            if (item is DeleteFolderMetaData)
                return;

            if (item == null)
            {
                if (isLastNamePart)
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
                    item.ItemRevision = change.Item.RemoteChangesetId;
                }
                else
                {
                    item = sourceControlProvider.GetItemsWithoutProperties(targetVersion, folderName, Recursion.None);
                    if (item == null)
                    {
                        item = new DeleteFolderMetaData();
                        item.Name = folderName;
                        item.ItemRevision = targetVersion;
                    }
                }
                string parentName = Helper.GetFolderName(item.Name);
                FolderMetaData parentFolder = (FolderMetaData)folder.FindItem(parentName);
                parentFolder.Items.Add(item);
            }
            else if (isLastNamePart)// we need to revert the item addition
            {
                if (item is StubFolderMetaData)
                {
                    DeleteFolderMetaData removeFolder = new DeleteFolderMetaData();
                    removeFolder.Name = item.Name;
                    removeFolder.ItemRevision = targetVersion;
                    folder.Items.Remove(item);
                    folder.Items.Add(removeFolder);
                }
                else if (additionForPropertyChangeOnly.ContainsKey(item) && additionForPropertyChangeOnly[item])
                {
                    ItemMetaData removeFolder = item is FolderMetaData ? (ItemMetaData)new DeleteFolderMetaData() : new DeleteMetaData();
                    removeFolder.Name = item.Name;
                    removeFolder.ItemRevision = targetVersion;
                    folder.Items.Remove(item);
                    folder.Items.Add(removeFolder);
                }
                else
                {
                    folder.Items.Remove(item);
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

        #region Nested type: ItemInformation

        private class ItemInformation
        {
            public readonly bool PropertyChange;
            public readonly string RemoteName;

            public ItemInformation(bool propertyChange,
                                   string remoteName)
            {
                PropertyChange = propertyChange;
                RemoteName = remoteName;
            }
        }

        #endregion
    }
}
