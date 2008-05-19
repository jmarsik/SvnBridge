using System.Collections.Generic;
using System.Threading;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class AsyncItemLoader : IItemLoader
    {
        // private bool firstItem = true;
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
            foreach (var item in IterateItems(folder))
            {
                sourceControlProvider.ReadFileAsync(item);
            }
        }

        private IEnumerable<ItemMetaData> IterateItems(FolderMetaData folder)
        {
            foreach (ItemMetaData item in folder.Items)
            {
                if (item.ItemType == ItemType.Folder)
                {
                    QueueItemsInFolder((FolderMetaData)item);
                }
                else if (!(item is DeleteMetaData))
                {
                    yield return item;
                }
            }
        }
    }
}
