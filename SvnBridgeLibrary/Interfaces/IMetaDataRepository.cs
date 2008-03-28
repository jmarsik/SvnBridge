using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
	public interface IMetaDataRepository
	{
		SourceItem[] QueryItems(int reversion, string path, Recursion recursion);
		SourceItem QueryItems(int itemId, int revision);
	}
}