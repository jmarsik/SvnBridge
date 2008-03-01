using System.Collections.Generic;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class FolderMetaData : ItemMetaData
    {
        public FolderMetaData(string name) : base(name)
        {
        }

        public FolderMetaData()
        {
        }

        private List<ItemMetaData> _items = new List<ItemMetaData>();

        public virtual List<ItemMetaData> Items
        {
            get { return _items; }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Folder; }
        }
    }
}