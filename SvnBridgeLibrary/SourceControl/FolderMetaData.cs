using System.Collections.Generic;

namespace SvnBridge.SourceControl
{
    public class FolderMetaData : ItemMetaData
    {
        public List<ItemMetaData> Items = new List<ItemMetaData>();
    }
}