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

        public delegate FileData GetFileData();

        public FutureFile(GetFileData getFileData)
        {
            this.getFileData = getFileData;
        }


        public FileData Value
        {
            get
            {
                return getFileData();
            }
        }
    }

    public class FileData
    {
        public string Base64DiffData;
        public string Md5;
    }
}
