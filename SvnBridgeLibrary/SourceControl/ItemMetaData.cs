using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class ItemMetaData
    {
        public int Id;
        public int ItemRevision;
        public int PropertyRevision;
        public string Name;
        public string Author;
        public DateTime LastModifiedDate;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public bool DataLoaded = false;
        public byte[] Data = null;
        public string DownloadUrl = null;

        public virtual ItemType ItemType
        {
            get { return ItemType.File; }
        }

        public virtual int Revision
        {
            get
            {
                if (PropertyRevision > ItemRevision)
                    return PropertyRevision;
                else
                    return ItemRevision;
            }
        }
    }
}