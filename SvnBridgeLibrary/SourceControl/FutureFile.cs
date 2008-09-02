using System;
using System.IO;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class FileData
    {
        public string Base64DiffData;
        public string Md5;
    }
}
