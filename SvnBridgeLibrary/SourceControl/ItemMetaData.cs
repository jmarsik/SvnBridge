using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class ItemMetaData
    {
        public int Id;
        public int Revision;
        public string Name;
        public string Author;
        public DateTime LastModifiedDate;
        public ItemType ItemType;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public bool DataLoaded = false;
        public byte[] Data = null;
        public string DownloadUrl = null;
    }
}