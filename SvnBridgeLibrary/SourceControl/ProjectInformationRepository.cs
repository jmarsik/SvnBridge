using System;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public class ProjectInformationRepository : IProjectInformationRepository
    {
        private readonly ITFSSourceControlService _sourceControlSvc;
        private readonly ICache cache;
        private readonly string serverUrl;

        public ProjectInformationRepository(
            ICache cache,
            ITFSSourceControlService _sourceControlSvc,
            string serverUrl)
        {
            this.cache = cache;
            this._sourceControlSvc = _sourceControlSvc;
            this.serverUrl = serverUrl;
        }

        #region IProjectInformationRepository Members

        public ProjectLocationInformation GetProjectLocation(ICredentials credentials,
                                                             string projectName)
        {
            string cacheKey = "GetProjectLocation-" + projectName;
            object cached = cache.Get(cacheKey);
            if (cached != null)
            {
                return (ProjectLocationInformation) cached;
            }

            projectName = projectName.ToLower();
            string[] servers = serverUrl.Split(',');
            foreach (string server in servers)
            {
                SourceItem[] items =
                    _sourceControlSvc.QueryItems(server,
                                                 CredentialsHelper.GetCredentialsForServer(server, credentials),
                                                 Constants.ServerRootPath + projectName,
                                                 RecursionType.None,
                                                 new LatestVersionSpec(),
                                                 DeletedState.NonDeleted,
                                                 ItemType.Any);
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