using System;
using System.IO;
using SvnBridge.RepositoryWebSvc;
//using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.TfsLibrary
{
    public class SourceItem : IComparable<SourceItem>
    {
        // Fields

        public int ItemId;
        public ItemType ItemType;

        public SourceItemStatus LocalItemStatus;
        public SourceItemStatus OriginalLocalItemStatus;
        public string LocalName;
        public string LocalTextBaseName;
        public string LocalConflictTextBaseName;
        public int LocalChangesetId;
        public int LocalConflictChangesetId;
        public string TargetLocalName;

        public SourceItemStatus RemoteItemStatus;
        public string RemoteName;
        public int RemoteChangesetId;
        public long RemoteSize;
        public DateTime RemoteDate;
        public string DownloadUrl;

        // Methods

        public static SourceItem FromLocalItem(int itemId,
                                               ItemType itemType,
                                               SourceItemStatus localStatus,
                                               SourceItemStatus originalStatus,
                                               string localName,
                                               string localTextBaseName,
                                               int localChangesetId,
                                               int localConflictChangesetId,
                                               string localConflictTextBaseName)
        {
            SourceItem result = new SourceItem();

            result.ItemId = itemId;
            result.ItemType = itemType;
            result.LocalItemStatus = localStatus;
            result.OriginalLocalItemStatus = originalStatus;
            result.LocalName = localName;
            result.LocalTextBaseName = localTextBaseName;
            result.LocalChangesetId = localChangesetId;
            result.LocalConflictChangesetId = localConflictChangesetId;
            result.LocalConflictTextBaseName = localConflictTextBaseName;

            return result;
        }

        public static SourceItem FromRemoteItem(int itemId,
                                                ItemType itemType,
                                                string targetLocalName,
                                                SourceItemStatus remoteStatus,
                                                string remoteName,
                                                int remoteChangesetId,
                                                long remoteSize,
                                                DateTime remoteDate,
                                                string downloadUrl)
        {
            SourceItem result = new SourceItem();

            result.ItemId = itemId;
            result.ItemType = itemType;
            result.TargetLocalName = targetLocalName;
            result.RemoteItemStatus = remoteStatus;
            result.RemoteName = remoteName;
            result.RemoteChangesetId = remoteChangesetId;
            result.RemoteSize = remoteSize;
            result.RemoteDate = remoteDate;
            result.DownloadUrl = downloadUrl;

            return result;
        }

        internal static SourceItem FromLocalDirectory(int itemId,
                                                      SourceItemStatus localStatus,
                                                      SourceItemStatus originalStatus,
                                                      string localName,
                                                      int localChangesetId)
        {
            return FromLocalItem(itemId, ItemType.Folder, localStatus, originalStatus, localName, null,
                                 localChangesetId, Constants.NullChangesetId, null);
        }

        internal static SourceItem FromLocalFile(int itemId,
                                                 SourceItemStatus localStatus,
                                                 SourceItemStatus originalStatus,
                                                 string localName,
                                                 string localTextBaseName,
                                                 int localChangesetId,
                                                 int localConflictChangesetId,
                                                 string localConflictTextBaseName)
        {
            return FromLocalItem(itemId, ItemType.File, localStatus, originalStatus, localName, localTextBaseName,
                                 localChangesetId, localConflictChangesetId, localConflictTextBaseName);
        }

        internal static SourceItem FromLocalPath(string localName)
        {
            if (File.Exists(localName))
                return FromLocalFile(Constants.NullItemId, SourceItemStatus.Unversioned, SourceItemStatus.Unversioned, localName,
                                     null, Constants.NullChangesetId, Constants.NullChangesetId, null);
            else
                return FromLocalDirectory(Constants.NullItemId, SourceItemStatus.Unversioned, SourceItemStatus.Unversioned,
                                          localName, Constants.NullChangesetId);
        }

        int IComparable<SourceItem>.CompareTo(SourceItem other)
        {
            string myName = LocalName ?? TargetLocalName ?? RemoteName;
            string otherName = other.LocalName ?? other.TargetLocalName ?? other.RemoteName;
            int nameCompare = string.Compare(myName, otherName, true);

            if (nameCompare != 0)
                return nameCompare;

            return ((int)RemoteItemStatus - (int)other.RemoteItemStatus);
        }

        public override string ToString()
        {
            string result = string.Format("id={0}", ItemId);

            if (LocalItemStatus != SourceItemStatus.None)
                result += string.Format(" st={0} csid={1}", LocalItemStatus, LocalChangesetId);

            if (RemoteItemStatus != SourceItemStatus.None)
                result += string.Format(" r_st={0} r_csid={1}", RemoteItemStatus, RemoteChangesetId);

            result += string.Format(" name={0}", LocalName ?? RemoteName);

            if (TargetLocalName != null)
                result += string.Format(" tname={0}", TargetLocalName);

            return result;
        }
    }
}