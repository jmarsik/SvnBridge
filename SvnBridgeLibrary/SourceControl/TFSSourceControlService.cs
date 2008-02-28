using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using System.Net;
using CodePlex.TfsLibrary.Utility;
using CodePlex.TfsLibrary;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlService : SourceControlService
    {
        readonly IRepositoryWebSvcFactory _webSvcFactory;

        public TFSSourceControlService(IRegistrationService registrationService,
                                    IRepositoryWebSvcFactory webSvcFactory,
                                    IWebTransferService webTransferService,
                                    IFileSystem fileSystem) 
            : base(registrationService, webSvcFactory, webTransferService, fileSystem)
        {
            _webSvcFactory = webSvcFactory;
        }

        public ExtendedItem[][] QueryItemsExtended(string tfsUrl, ICredentials credentials, string workspaceName, ItemSpec[] items, DeletedState deletedState, ItemType itemType)
        {
            Repository webSvc = (Repository)_webSvcFactory.Create(tfsUrl, credentials);
            string username = TfsUtil.GetUsername(credentials, tfsUrl);
            return webSvc.QueryItemsExtended(workspaceName, username, items, deletedState, itemType);
        }

        public BranchRelative[][] QueryBranches(string tfsUrl, ICredentials credentials, string workspaceName, ItemSpec[] items, VersionSpec version)
        {
            Repository webSvc = (Repository)_webSvcFactory.Create(tfsUrl, credentials);
            string username = TfsUtil.GetUsername(credentials, tfsUrl);
            return webSvc.QueryBranches(workspaceName, username, items, version);
        }

        public ItemSet[] QueryItems(string tfsUrl, ICredentials credentials, string workspaceName, string workspaceOwner, ItemSpec[] items, VersionSpec version, DeletedState deletedState, ItemType itemType, bool generateDownloadUrls)
        {
            Repository webSvc = (Repository)_webSvcFactory.Create(tfsUrl, credentials);
            try
            {
                return webSvc.QueryItems(workspaceName, workspaceOwner, items, version, deletedState, itemType, generateDownloadUrls);
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("TF14002:"))
                    throw new NetworkAccessDeniedException(e);

                throw;
            }
        }
    }
}
