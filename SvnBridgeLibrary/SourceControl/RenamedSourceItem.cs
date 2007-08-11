using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.TfsLibrary.ObjectModel;

namespace SvnBridge.SourceControl
{
    public class RenamedSourceItem : SourceItem
    {
        public string OriginalRemoteName;
        public int OriginalRevision;

        public RenamedSourceItem(SourceItem item, string originalRemoteName, int originalRevision)
        {
            this.ItemId = item.ItemId;
            this.ItemType = item.ItemType;
            this.RemoteName = item.RemoteName;
            this.RemoteDate = item.RemoteDate;
            this.DownloadUrl = item.DownloadUrl;
            this.OriginalRemoteName = originalRemoteName;
            this.OriginalRevision = originalRevision;
        }
    }
}
