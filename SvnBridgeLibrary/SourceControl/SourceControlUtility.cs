using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class SourceControlUtility : ISourceControlUtility
    {
        private readonly IMetaDataRepository metaDataRepository;
    	private readonly string rootPath;

    	public SourceControlUtility(IMetaDataRepository metaDataRepositoryFactory, string rootPath)
    	{
    		this.metaDataRepository = metaDataRepositoryFactory;
    		this.rootPath = rootPath;
    	}

		public ItemMetaData GetPreviousVersionOfItem(SourceItem item)
        {
			SourceItem sourceItem = metaDataRepository
				.QueryPreviousVersionOfItem(item.ItemId, item.RemoteChangesetId);
			return ItemMetaData.ConvertSourceItem(sourceItem, rootPath);
        }

        public ItemMetaData FindItem(FolderMetaData folder,
                                      string name)
        {
            foreach (ItemMetaData item in folder.Items)
            {
                if (item.Name == name)
                {
                    return item;
                }
				FolderMetaData subFolder = item as FolderMetaData;
            	if(subFolder!=null)
            	{
            		ItemMetaData result = FindItem(subFolder, name);
					if(result!=null)
						return result;
            	}
            }

            return null;
        }
    }
}
