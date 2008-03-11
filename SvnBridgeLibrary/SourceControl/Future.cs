using System;
using System.IO;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class FutureData
    {
        private readonly IAsyncResult result;
        private readonly IWebTransferService webTransferService;
        private readonly Action<byte[]> onSet;
        private byte[] data;

        public FutureData(IAsyncResult result, 
            IWebTransferService webTransferService, 
            Action<byte[]> onSet)
        {
            this.result = result;
            this.webTransferService = webTransferService;
            this.onSet = onSet;
        }

        public byte[] Value
        {
            get
            {
                if (data == null)
                {
                    result.AsyncWaitHandle.WaitOne();
                    data = webTransferService.EndDownloadBytes(result);
                    if (onSet != null)
                        onSet(data);
                }
                return data;
            }
            set { data = value;}
        }
    }
}