using System;
using System.IO;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;

namespace SvnBridge.SourceControl
{
    public class FutureData
    {
        private readonly IAsyncResult result;
        private readonly IWebTransferService webTransferService;
        private byte[] data;

        public FutureData(IAsyncResult result, IWebTransferService webTransferService)
        {
            this.result = result;
            this.webTransferService = webTransferService;
        }

        public byte[] Value
        {
            get
            {
                if (data == null)
                {
                    result.AsyncWaitHandle.WaitOne();
                    data = webTransferService.EndDownloadBytes(result);
                }
                return data;
            }
            set { data = value;}
        }
    }
}