using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class AsyncItemLoader : IItemLoader
    {
        private readonly FolderMetaData folderInfo;
        private readonly ISourceControlProvider sourceControlProvider;

        public AsyncItemLoader(FolderMetaData folderInfo,
                                 ISourceControlProvider sourceControlProvider)
        {
            Guard.ArgumentNotNull(folderInfo, "folderInfo");
            Guard.ArgumentNotNull(sourceControlProvider, "sourceControlProvider");

            this.folderInfo = folderInfo;
            this.sourceControlProvider = sourceControlProvider;
        }

        public void Start()
        {
            QueueItemsInFolder(folderInfo);
        }


        private void QueueItemsInFolder(FolderMetaData folder)
        {
            foreach (ItemMetaData item in folder.Items)
            {
                if (item.ItemType == ItemType.Folder)
                {
                    QueueItemsInFolder((FolderMetaData)item);
                }
                else if (!(item is DeleteMetaData))
                {
                    sourceControlProvider.ReadFileAsync(item);
                }
            }
        }
    }
}