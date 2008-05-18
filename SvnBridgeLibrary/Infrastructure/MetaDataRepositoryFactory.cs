using System.Net;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
	public class MetaDataRepositoryFactory : IMetaDataRepositoryFactory
	{
		private readonly ITFSSourceControlService sourceControlService;
		private readonly IPersistentCache persistentCache;
        private readonly bool useCaching;

		public MetaDataRepositoryFactory(ITFSSourceControlService sourceControlService, IPersistentCache persistentCache, bool useCaching)
		{
			this.sourceControlService = sourceControlService;
			this.persistentCache = persistentCache;
            this.useCaching = useCaching;
		}

		public IMetaDataRepository Create(ICredentials credentials, string serverUrl, string rootPath)
		{
            IMetaDataRepository repository;
            if (useCaching)
            {
                repository = new MetaDataRepository(sourceControlService, credentials,
                    persistentCache,
                    serverUrl, rootPath);
            }
            else
            {
                repository = new MetaDataRepositoryNoCache(sourceControlService, credentials,
                    persistentCache,
                    serverUrl, rootPath);
            }
			return repository;
		}

		public int GetLatestRevision(string tfsUrl, ICredentials credentials)
		{
			return sourceControlService.GetLatestChangeset(tfsUrl, credentials);
		}
	}
}