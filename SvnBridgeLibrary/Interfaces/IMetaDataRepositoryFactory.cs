using System.Net;

namespace SvnBridge.Interfaces
{
	public interface IMetaDataRepositoryFactory
	{
		IMetaDataRepository Create(ICredentials credentials,
		                           string serverUrl,
		                           string rootPath);

		int GetLatestRevision(string tfsUrl, ICredentials credentials);
	}
}