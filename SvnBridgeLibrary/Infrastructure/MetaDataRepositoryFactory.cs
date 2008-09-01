using System.Net;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
	public class MetaDataRepositoryFactory
	{
		private readonly ITFSSourceControlService sourceControlService;
		private readonly IPersistentCache persistentCache;
        private readonly bool cacheEnabled;

		public MetaDataRepositoryFactory(ITFSSourceControlService sourceControlService, IPersistentCache persistentCache, bool cacheEnabled)
		{
			this.sourceControlService = sourceControlService;
			this.persistentCache = persistentCache;
            this.cacheEnabled = cacheEnabled;
		}

		public virtual IMetaDataRepository Create(ICredentials credentials, string serverUrl, string rootPath)
		{
            IMetaDataRepository repository;
            if (cacheEnabled)
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

		public virtual int GetLatestRevision(string tfsUrl, ICredentials credentials)
		{
			return sourceControlService.GetLatestChangeset(tfsUrl, credentials);
		}
	}
}