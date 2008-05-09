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

        ///// <summary>
        ///// This is required so if we are using a secured TFS server,
        ///// we will get a 401 before sending any response to the user
        ///// </summary>
        ///// <param name="folder"></param>
        //public void DownloadFirstFile(FolderMetaData folder)
        //{
        //    foreach (var file in IterateFiles(folder))
        //    {
        //        sourceControlProvider.ReadFile(file);
        //        break;
        //    }
        //}

        public void Start()
        {
            QueueItemsInFolder(folderInfo);
        }


        private void QueueItemsInFolder(FolderMetaData folder)
        {
            foreach (var file in IterateFiles(folder))
            {
                sourceControlProvider.ReadFileAsync(file);
            }
        }

        private IEnumerable<ItemMetaData> IterateFiles(FolderMetaData folder)
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
