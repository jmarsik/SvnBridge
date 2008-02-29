using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl.Dto
{
    public class ActivityItem
    {
        public readonly ActivityItemAction Action;
        public readonly ItemType FileType;
        public readonly string Path;

        public ActivityItem(string path,
                            ItemType fileType,
                            ActivityItemAction action)
        {
            Path = path;
            FileType = fileType;
            Action = action;
        }
    }
}