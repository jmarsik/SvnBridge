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

        private readonly List<ItemMetaData> _items = new List<ItemMetaData>();

        public virtual List<ItemMetaData> Items
        {
            get { return _items; }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Folder; }
        }

    	public bool RemoveMissingItem(string name)
    	{
			foreach (ItemMetaData item in Items)
			{
				if (item.Name == name && item is MissingFolderMetaData)
				{
					Items.Remove(item);
					return true;
				}
				FolderMetaData subFolder = item as FolderMetaData;
				if (subFolder != null)
				{
					if(subFolder.RemoveMissingItem(name))
						return true;
				}
			}
    		return false;
    	}

		public ItemMetaData FindItem(string name)
		{
			foreach (ItemMetaData item in Items)
			{
				if (item.Name == name)
				{
					return item;
				}
				FolderMetaData subFolder = item as FolderMetaData;
				if (subFolder != null)
				{
					ItemMetaData result = subFolder.FindItem(name);
					if (result != null)
						return result;
				}
			}
			return null;
		}
    }
}