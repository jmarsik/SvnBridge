using System.Collections.Generic;
using System.Collections.ObjectModel;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class FolderMetaData : ItemMetaData
    {
        public FolderMetaData(string name) : base(name)
        {
        	_items = new NoNullAllowedItemsCollection(this);
        }

        public FolderMetaData()
        {
			_items = new NoNullAllowedItemsCollection(this);
        }

		private readonly IList<ItemMetaData> _items;

		public virtual IList<ItemMetaData> Items
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

		private class NoNullAllowedItemsCollection : Collection<ItemMetaData>
		{
			public NoNullAllowedItemsCollection(FolderMetaData parent)
			{
				this.parent = parent;
			}

			private readonly FolderMetaData parent;

			protected override void InsertItem(int index, ItemMetaData item)
			{
				Guard.ArgumentNotNull(item, "item");
				item.SetParent(parent);
				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, ItemMetaData item)
			{
				Guard.ArgumentNotNull(item, "item");
				item.SetParent(parent);
				base.SetItem(index, item);
			}
		}
    }
}