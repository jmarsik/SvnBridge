using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
	public interface IMetaDataRepository
	{
		SourceItem[] QueryItems(int revision, string path, Recursion recursion);
	}
}
