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
using SvnBridge.Cache;

namespace SvnBridge.Infrastructure
{
    public class FileRepository
    {
        private readonly ICredentials credentials;
        private readonly IWebTransferService webTransferService;
        private readonly DefaultLogger logger;

        public FileRepository(string serverUrl, ICredentials credentials, IWebTransferService webTransferService, DefaultLogger logger)
        {
            this.credentials = CredentialsHelper.GetCredentialsForServer(serverUrl, credentials);
            this.webTransferService = webTransferService;
            this.logger = logger;
        }

        public virtual byte[] GetFile(ItemMetaData item)
        {
            return webTransferService.DownloadBytes(item.DownloadUrl, credentials);
        }

        public virtual void ReadFileAsync(ItemMetaData item)
        {
            byte[] data = webTransferService.DownloadBytes(item.DownloadUrl, credentials);
            FileData fileData = new FileData();
            fileData.Base64DiffData = SvnDiffParser.GetSvnDiffData(data);
            fileData.Md5 = Helper.GetMd5Checksum(data);
            item.Data = new FutureFile(() => fileData);
            item.DataLoaded = true;
        }
    }
}
