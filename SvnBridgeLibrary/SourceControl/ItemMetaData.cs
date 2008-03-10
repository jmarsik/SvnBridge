using System;
using System.Collections.Generic;
using System.Threading;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class ItemMetaData
    {
        public string Author;
        public FutureData Data;
        public bool DataLoaded = false;
        public string DownloadUrl = null;
        public int Id;
        public int ItemRevision;
        public DateTime LastModifiedDate;
        public string Name;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public int PropertyRevision;

        public ItemMetaData()
        {
        }


        public ItemMetaData(string name)
        {
            Name = name;
        }

        public virtual ItemType ItemType
        {
            get { return ItemType.File; }
        }

        public virtual int Revision
        {
            get
            {
                if (PropertyRevision > ItemRevision)
                {
                    return PropertyRevision;
                }
                else
                {
                    return ItemRevision;
                }
            }
        }
    }
}

