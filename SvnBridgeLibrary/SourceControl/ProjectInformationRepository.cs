using System;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.SourceControl
{
    public class ProjectInformationRepository : IProjectInformationRepository
    {
        private readonly TFSSourceControlService _sourceControlSvc;
        private readonly string serverUrl;

        public ProjectInformationRepository(TFSSourceControlService _sourceControlSvc, string serverUrl)
        {
            this._sourceControlSvc = _sourceControlSvc;
            this.serverUrl = serverUrl;
        }

        public ProjectLocationInformation GetProjectLocation(ICredentials credentials, string projectName)
        {
            projectName = projectName.ToLower();
            string[] servers = serverUrl.Split(',');
            foreach (string server in servers)
            {
                SourceItem[] items =
                    _sourceControlSvc.QueryItems(server, CredentialsHelper.GetCredentialsForServer(server, credentials),
                                                 Constants.ServerRootPath + projectName, RecursionType.None,
                                                 new LatestVersionSpec(), DeletedState.NonDeleted, ItemType.Any);
                if (items.Length > 0)
                {
                    string remoteProjectName = items[0].RemoteName.Substring(Constants.ServerRootPath.Length);
                    return new ProjectLocationInformation(remoteProjectName, server);
                };

            }
            throw new InvalidOperationException("Could not find project '" + projectName + "' in: " + serverUrl);
        }
    }
}