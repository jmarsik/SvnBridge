using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
	public interface ISourceControlUtility
	{
		ItemMetaData GetPreviousVersionOfItem(SourceItem item);

		ItemMetaData FindItem(FolderMetaData folder,
		                      string name);
	}
}
