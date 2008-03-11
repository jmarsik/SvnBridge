using System;
using System.IO;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class FutureFile
    {
        private readonly GetFileData getFileData;

        public delegate byte[] GetFileData();

        public FutureFile(GetFileData getFileData)
        {
            this.getFileData = getFileData;
        }


        public byte[] Value
        {
            get
            {
                return getFileData();
            }
        }
    }
}
