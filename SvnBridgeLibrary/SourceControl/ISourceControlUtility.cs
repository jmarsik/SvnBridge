using CodePlex.TfsLibrary.ObjectModel;

namespace SvnBridge.SourceControl
{
    public interface ISourceControlUtility
    {
        ItemMetaData GetItem(int version,
                             int itemId);

        ItemMetaData ConvertSourceItem(SourceItem sourceItem);

        ItemMetaData FindItem(FolderMetaData folder,
                              string name);
    }
}