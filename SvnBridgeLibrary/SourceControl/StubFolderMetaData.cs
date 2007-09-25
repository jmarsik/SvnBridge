using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class StubFolderMetaData : FolderMetaData
    {
        public FolderMetaData RealFolder;

        public override List<ItemMetaData> Items
        {
            get
            {
                return RealFolder.Items;
            }
        }
    }
}
