using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class MergeActivityResponseItem
    {
        public ItemType Type;
        public string Path;

        public MergeActivityResponseItem(ItemType type,
                                         string path)
        {
            Type = type;
            Path = path;
        }
    }
}