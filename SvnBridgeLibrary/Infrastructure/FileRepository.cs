using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.SourceControl;
using System.Net;
using SvnBridge.Interfaces;
using CodePlex.TfsLibrary.ObjectModel;
using System.Threading;
using SvnBridge.Exceptions;
using SvnBridge.Net;
using SvnBridge.Utility;

namespace SvnBridge.Infrastructure
{
    public class FileRepository : IFileRepository
    {
        private readonly ICredentials credentials;
        private readonly IFileCache fileCache;
        private readonly IWebTransferService webTransferService;
        private readonly ILogger logger;
        private readonly bool cacheEnabled;

        public FileRepository(string serverUrl, ICredentials credentials, IFileCache fileCache, IWebTransferService webTransferService, ILogger logger, bool cacheEnabled)
        {
            this.credentials = CredentialsHelper.GetCredentialsForServer(serverUrl, credentials);
            this.fileCache = fileCache;
            this.webTransferService = webTransferService;
            this.logger = logger;
            this.cacheEnabled = cacheEnabled;
        }

        public byte[] GetFile(ItemMetaData item)
        {
            if (!cacheEnabled)
                return webTransferService.DownloadBytes(item.DownloadUrl, credentials);

            byte[] bytes = fileCache.Get(item.Name, item.Revision);
            if (bytes != null)
            {
                SetItemDataIsAtCache(item);
                return bytes;
            }

            byte[] downloadBytes = webTransferService.DownloadBytes(item.DownloadUrl, credentials);
            fileCache.Set(item.Name, item.Revision, downloadBytes);
            SetItemDataIsAtCache(item);
            return downloadBytes;
        }

        public void ReadFileAsync(ItemMetaData item)
        {
            if (!cacheEnabled)
            {
                byte[] data = webTransferService.DownloadBytes(item.DownloadUrl, credentials);
                FileData fileData = new FileData();
                fileData.Base64DiffData = SvnDiffParser.GetSvnDiffData(data);
                fileData.Md5 = Helper.GetMd5Checksum(data);
                item.Data = new FutureFile(() => fileData);
                item.DataLoaded = true;
                return;
            }

            byte[] bytes = fileCache.Get(item.Name, item.Revision);
            if (bytes != null)
            {
                SetItemDataIsAtCache(item);
                return;
            }
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            DownloadFileAsync(item, resetEvent, 0);
            item.Data = new FutureFile(delegate
            {
                return GetFileData(resetEvent, item);
            });

            item.DataLoaded = true;
        }

        private void SetItemDataIsAtCache(ItemMetaData item)
        {
            item.Data = new FutureFile(() => fileCache.GetText(item.Name, item.Revision));
            item.DataLoaded = true;
        }

        private void DownloadFileAsync(ItemMetaData item, EventWaitHandle waitHandle, int retry)
        {
            logger.Trace("Starting to download {0}", item.Name);
            webTransferService.BeginDownloadBytes(item.DownloadUrl, credentials, delegate(IAsyncResult ar)
            {
                try
                {
                    byte[] data = webTransferService.EndDownloadBytes(ar);
                    fileCache.Set(item.Name, item.Revision, data);
                    logger.Trace("Finished downloading {0}", item.Name);
                    waitHandle.Set();
                }
                catch (UnauthorizedAccessException e)
                {
                    item.DataLoadedError = e;
                    waitHandle.Set();
                }
                catch (Exception e)
                {
                    retry = retry + 1;
                    if (retry == 3)
                    {
                        logger.Error("Failed to download " + item.Name + ", max retry count reached, aborting", e);
                        Listener.RaiseErrorOccured(e);
                        item.DataLoadedError = e;
                        waitHandle.Set();
                    }
                    logger.Error("Failed to download " + item.Name + " retry #" + retry, e);
                    DownloadFileAsync(item, waitHandle, retry);
                }
            });
        }

        private FileData GetFileData(WaitHandle resetEvent, ItemMetaData item)
        {
            resetEvent.WaitOne();
            resetEvent.Close();
            if (item.DataLoadedError != null)
            {
                if (item.DataLoadedError is UnauthorizedAccessException)
                    throw item.DataLoadedError;
                throw new InvalidOperationException("Failed to get item data", item.DataLoadedError);
            }
            FileData results = fileCache.GetText(item.Name, item.Revision);
            if (results == null)
                throw new CacheMissException(item.Name);
            return results;
        }
    }
}
