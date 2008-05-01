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
        private readonly string serverUrl;

        public ProjectInformationRepository(
            IMetaDataRepositoryFactory metaDataRepositoryFactory,
            string serverUrl)
        {
			this.metaDataRepositoryFactory = metaDataRepositoryFactory;
            this.serverUrl = serverUrl;
        }

        #region IProjectInformationRepository Members

        public ProjectLocationInformation GetProjectLocation(ICredentials credentials,
                                                             string projectName)
        {
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
                    return new ProjectLocationInformation(remoteProjectName, server);
                }
                ;
            }
            throw new InvalidOperationException("Could not find project '" + projectName + "' in: " + serverUrl);
        }

        #endregion
    }
}