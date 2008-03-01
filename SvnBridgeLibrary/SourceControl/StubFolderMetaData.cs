using System.Collections.Generic;

namespace SvnBridge.SourceControl
{
    public class StubFolderMetaData : FolderMetaData
    {
        public FolderMetaData RealFolder;

        public StubFolderMetaData()
        {

        }

        public StubFolderMetaData(string name) : base(name)
        {
        }


        public override List<ItemMetaData> Items
        {
            get { return RealFolder.Items; }
        }
    }
}