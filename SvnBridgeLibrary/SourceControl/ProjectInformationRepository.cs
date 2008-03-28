using System;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class ProjectInformationRepository : IProjectInformationRepository
    {
		private readonly IMetaDataRepositoryFactory metaDataRepositoryFactory;
        private readonly ICache cache;
        private readonly string serverUrl;

        public ProjectInformationRepository(
            ICache cache,
            IMetaDataRepositoryFactory metaDataRepositoryFactory,
            string serverUrl)
        {
            this.cache = cache;
			this.metaDataRepositoryFactory = metaDataRepositoryFactory;
            this.serverUrl = serverUrl;
        }

        #region IProjectInformationRepository Members

        public ProjectLocationInformation GetProjectLocation(ICredentials credentials,
                                                             string projectName)
        {
            string cacheKey = "GetProjectLocation-" + projectName;
            CachedResult cached = cache.Get(cacheKey);
            if (cached != null)
            {
                return (ProjectLocationInformation) cached.Value;
            }

            projectName = projectName.ToLower();
            string[] servers = serverUrl.Split(',');
            foreach (string server in servers)
            {
				ICredentials credentialsForServer = CredentialsHelper.GetCredentialsForServer(serverUrl, credentials);
				int revision = metaDataRepositoryFactory.GetLatestRevision(serverUrl, credentialsForServer);
            	SourceItem[] items = metaDataRepositoryFactory
						.Create(credentialsForServer, serverUrl, Constants.ServerRootPath + projectName)
							.QueryItems(revision, "", Recursion.None);

                if (items != null && items.Length > 0)
                {
                    string remoteProjectName = items[0].RemoteName.Substring(Constants.ServerRootPath.Length);
                    ProjectLocationInformation information = new ProjectLocationInformation(remoteProjectName, server);
                    cache.Set(cacheKey, information);
                    return information;
                }
                ;
            }
            throw new InvalidOperationException("Could not find project '" + projectName + "' in: " + serverUrl);
        }

        #endregion
    }
}