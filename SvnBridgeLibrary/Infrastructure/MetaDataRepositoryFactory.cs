using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;
using System;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
	public class MetaDataRepositoryFactory : IMetaDataRepositoryFactory
	{
		private readonly ITFSSourceControlService sourceControlService;
		private string connectionString;

		public MetaDataRepositoryFactory(ITFSSourceControlService sourceControlService, string connectionString)
		{
			this.sourceControlService = sourceControlService;
			this.connectionString = connectionString;
		}

		public IMetaDataRepository Create(ICredentials credentials, string serverUrl, string rootPath)
		{
            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
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