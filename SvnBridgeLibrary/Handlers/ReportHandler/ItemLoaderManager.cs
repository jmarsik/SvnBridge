using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.SourceControl;
using System.Collections;
using System.Threading;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.Handlers
{
    public class ItemLoaderManager
    {
        private const int INITIAL_LOADING_THREADS = 2;
        private const int MAX_LOADING_THREADS = 20;

        private bool _cancel = false;
        private FolderMetaData _folderInfo;
        private ISourceControlProvider _sourceControlProvider;
        private Queue<ItemMetaData> _loadingQueue = new Queue<ItemMetaData>();
        private List<ItemLoader> _itemLoaders = new List<ItemLoader>();
        private List<Thread> _loadingThreads = new List<Thread>();

        public ItemLoaderManager(FolderMetaData folderInfo, ISourceControlProvider sourceControlProvider)
        {
            _folderInfo = folderInfo;
            _sourceControlProvider = sourceControlProvider;
        }

        public void Cancel()
        {
            _cancel = true;
        }

        public void Start()
        {
            QueueItemsInFolder(_folderInfo);

            for (int i = 0; i < INITIAL_LOADING_THREADS; i++)
                AddLoadingThread();

            bool threadsStillActive;
            do
            {
                Thread.Sleep(500);

                threadsStillActive = false;
                foreach (Thread thread in _loadingThreads)
                    if (thread.ThreadState != ThreadState.Stopped)
                        threadsStillActive = true;

                int count = CountLoadedItemsInFolder(_folderInfo);
                if (count < 5 && _loadingThreads.Count < MAX_LOADING_THREADS && (_loadingQueue.Count > 20 || _loadingThreads.Count == 0))
                    AddLoadingThread();

                if (count > 50 && _loadingThreads.Count > 0)
                    RemoveLoadingThread();

                if (_cancel)
                    foreach (ItemLoader itemLoader in _itemLoaders)
                        itemLoader.Cancel();

            } while (threadsStillActive || (!_cancel && _loadingQueue.Count > 0));
        }

        private void RemoveLoadingThread()
        {
            _itemLoaders[_itemLoaders.Count - 1].Cancel();
            _itemLoaders.RemoveAt(_itemLoaders.Count - 1);
            _loadingThreads.RemoveAt(_loadingThreads.Count - 1);
        }

        private void AddLoadingThread()
        {
            ItemLoader itemLoader = new ItemLoader(_loadingQueue, _sourceControlProvider);
            Thread loadingThread = new Thread(itemLoader.Start);
            _itemLoaders.Add(itemLoader);
            _loadingThreads.Add(loadingThread);
            loadingThread.Start();
        }

        private int CountLoadedItemsInFolder(FolderMetaData folder)
        {
            int count = 0;
            foreach (ItemMetaData item in folder.Items)
            {
                if (item.ItemType == ItemType.Folder)
                    count += CountLoadedItemsInFolder((FolderMetaData)item);
                else if (!(item is DeleteMetaData))
                    if (item.DataLoaded)
                        count++;
            }
            return count;
        }

        private void QueueItemsInFolder(FolderMetaData folder)
        {
            foreach (ItemMetaData item in folder.Items)
                if (item.ItemType == ItemType.Folder)
                    QueueItemsInFolder((FolderMetaData)item);
                else if (!(item is DeleteMetaData))
                    lock (((ICollection)_loadingQueue).SyncRoot)
                        _loadingQueue.Enqueue(item);
        }
    }
}