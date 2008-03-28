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

    	public ItemMetaData GetItem(int version, int itemId)
        {
            SourceItem item = metaDataRepository.QueryItems(itemId, version);
			return ItemMetaData.ConvertSourceItem(item, rootPath);
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
            }

            return null;
        }
    }
}