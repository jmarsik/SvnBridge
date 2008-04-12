using System.Net;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
	public class MetaDataRepositoryFactory : IMetaDataRepositoryFactory
	{
		private readonly ITFSSourceControlService sourceControlService;
		private readonly IPersistentCache persistentCache;

		public MetaDataRepositoryFactory(ITFSSourceControlService sourceControlService, IPersistentCache persistentCache)
		{
			this.sourceControlService = sourceControlService;
			this.persistentCache = persistentCache;
		}

		public IMetaDataRepository Create(ICredentials credentials, string serverUrl, string rootPath)
		{
			MetaDataRepository repository = new MetaDataRepository(sourceControlService, credentials, 
				persistentCache,
				serverUrl, rootPath);
			return repository;
		}

		public int GetLatestRevision(string tfsUrl, ICredentials credentials)
		{
			return sourceControlService.GetLatestChangeset(tfsUrl, credentials);
		}
	}
}