using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
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