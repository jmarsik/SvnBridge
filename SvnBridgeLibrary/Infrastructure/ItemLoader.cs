using System.Collections;
using System.Collections.Generic;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class ItemLoader
    {
        private bool _cancel = false;
        private readonly Queue<ItemMetaData> _itemQueue;
        private readonly ISourceControlProvider _sourceControlProvider;

        public ItemLoader(Queue<ItemMetaData> itemQueue,
                          ISourceControlProvider sourceControlProvider)
        {
            _itemQueue = itemQueue;
            _sourceControlProvider = sourceControlProvider;
        }

        public void Cancel()
        {
            _cancel = true;
        }

        public void Start()
        {
            bool queueEmpty = false;
            do
            {
                ItemMetaData item = null;
                lock (((ICollection) _itemQueue).SyncRoot)
                {
                    if (_itemQueue.Count > 0)
                    {
                        item = _itemQueue.Dequeue();
                    }
                    else
                    {
                        queueEmpty = true;
                    }
                }

                if (!queueEmpty)
                {
                    item.Data = _sourceControlProvider.ReadFile(item);
                    item.DataLoaded = true;
                }
            } while (!_cancel && !queueEmpty);
        }
    }
}