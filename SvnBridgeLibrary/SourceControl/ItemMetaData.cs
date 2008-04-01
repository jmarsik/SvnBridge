using System;
using System.Collections.Generic;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class ItemMetaData
    {
        public string Author;
        public FutureFile Data;
        public bool DataLoaded = false;
        public string DownloadUrl = null;
        public int Id;
        public int ItemRevision;
        public DateTime LastModifiedDate;
        public string Name;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public int PropertyRevision;

        public ItemMetaData()
        {
        }


        public ItemMetaData(string name)
        {
            Name = name;
        }

        public virtual ItemType ItemType
        {
            get { return ItemType.File; }
        }

        public virtual int Revision
        {
            get
            {
                if (PropertyRevision > ItemRevision)
                {
                    return PropertyRevision;
                }
                else
                {
                    return ItemRevision;
                }
            }
        }


		public static ItemMetaData ConvertSourceItem(SourceItem sourceItem, string rootPath)
		{
			ItemMetaData item;
			if (sourceItem.ItemType == ItemType.Folder)
			{
				item = new FolderMetaData();
			}
			else
			{
				item = new ItemMetaData();
			}

			item.Id = sourceItem.ItemId;
			if (rootPath.Length <= sourceItem.RemoteName.Length)
			{
				item.Name = sourceItem.RemoteName.Substring(rootPath.Length);
			}
			else
			{
				item.Name = "";
			}

			item.Author = "unknown";
			item.LastModifiedDate = sourceItem.RemoteDate;
			item.ItemRevision = sourceItem.RemoteChangesetId;
			item.DownloadUrl = sourceItem.DownloadUrl;
			return item;
		}

		public override string ToString()
		{
			return Name + " @" + Revision;
		}
    }
}


