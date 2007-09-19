using System.Collections.Generic;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class FolderMetaData : ItemMetaData
    {
        public FolderMetaData()
        {
            this.ItemType = ItemType.Folder;
        }

        private List<ItemMetaData> _items = new List<ItemMetaData>();

        public virtual List<ItemMetaData> Items
        {
            get { return _items; }
        }
            
    }
}