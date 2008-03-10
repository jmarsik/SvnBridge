using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class SourceControlUtility : ISourceControlUtility
    {
        private readonly string rootPath;
        private readonly string serverUrl;
        private readonly ITFSSourceControlService sourceControlService;
        private readonly ICredentialsProvider credentialsProvider;

        public SourceControlUtility(
            ITFSSourceControlService sourceControlService,
            ICredentialsProvider credentialsProvider,
            string rootPath,
            string serverUrl)
        {
            this.sourceControlService = sourceControlService;
            this.credentialsProvider = credentialsProvider;
            this.rootPath = rootPath;
            this.serverUrl = serverUrl;
        }


        public ItemMetaData GetItem(int version,
                                    int itemId)
        {
            SourceItem[] items = sourceControlService.QueryItems(serverUrl, credentialsProvider.GetCredentials(), new int[] {itemId}, version);
            return ConvertSourceItem(items[0]);
        }

        public ItemMetaData ConvertSourceItem(SourceItem sourceItem)
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