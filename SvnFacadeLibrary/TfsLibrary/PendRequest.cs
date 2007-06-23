//using CodePlex.TfsLibrary.RepositoryWebSvc;

using SvnBridge.RepositoryWebSvc;

namespace SvnBridge.TfsLibrary
{
    public class PendRequest
    {
        // Fields

        public string LocalName;
        public PendRequestType RequestType;
        public int CodePage;
        public ItemType ItemType;

        // Lifetime

        PendRequest() {}

        // Methods

        public static PendRequest AddFile(string localName,
                                          int codePage)
        {
            PendRequest result = new PendRequest();
            result.LocalName = localName;
            result.RequestType = PendRequestType.Add;
            result.ItemType = ItemType.File;
            result.CodePage = codePage;
            return result;
        }

        public static PendRequest AddFolder(string localName)
        {
            PendRequest result = new PendRequest();
            result.LocalName = localName;
            result.RequestType = PendRequestType.Add;
            result.ItemType = ItemType.Folder;
            result.CodePage = TfsUtil.CodePage_ANSI;
            return result;
        }

        public static PendRequest Edit(string localName)
        {
            PendRequest result = new PendRequest();
            result.LocalName = localName;
            result.RequestType = PendRequestType.Edit;
            return result;
        }

        public static PendRequest Delete(string localName)
        {
            PendRequest result = new PendRequest();
            result.LocalName = localName;
            result.RequestType = PendRequestType.Delete;
            return result;
        }
    }
}