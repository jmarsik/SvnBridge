using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
	public class MetaDataRepositoryFactory : IMetaDataRepositoryFactory
	{
		private readonly ISourceControlService sourceControlService;
		private string connectionString;

		public MetaDataRepositoryFactory(ISourceControlService sourceControlService, string connectionString)
		{
			this.sourceControlService = sourceControlService;
			this.connectionString = connectionString;
		}

		public IMetaDataRepository Create(ICredentials credentials, string serverUrl, string rootPath)
		{
			MetaDataRepository repository = new MetaDataRepository(sourceControlService, credentials, serverUrl, rootPath, connectionString);
			repository.EnsureDbExists();
			return repository;
		}

		public int GetLatestRevision(string tfsUrl, ICredentials credentials)
		{
			return sourceControlService.GetLatestChangeset(tfsUrl, credentials);
		}
	}
}