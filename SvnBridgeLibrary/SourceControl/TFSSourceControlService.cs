using System;
using System.Net;
using System.Web.Services.Protocols;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Exceptions;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlService : SourceControlService, ITFSSourceControlService
    {
        private readonly IRepositoryWebSvcFactory _webSvcFactory;

        public TFSSourceControlService(IRegistrationService registrationService,
                                       IRepositoryWebSvcFactory webSvcFactory,
                                       IWebTransferService webTransferService,
                                       IFileSystem fileSystem)
            : base(registrationService, webSvcFactory, webTransferService, fileSystem)
        {
            _webSvcFactory = webSvcFactory;
        }

        #region ITFSSourceControlService Members

        public ExtendedItem[][] QueryItemsExtended(string tfsUrl,
                                                   ICredentials credentials,
                                                   string workspaceName,
                                                   ItemSpec[] items,
                                                   DeletedState deletedState,
                                                   ItemType itemType)
        {
            Repository webSvc = TryCreateProxy(tfsUrl, credentials);
            string username = TfsUtil.GetUsername(credentials, tfsUrl);
            return webSvc.QueryItemsExtended(workspaceName, username, items, deletedState, itemType);
        }

        public BranchRelative[][] QueryBranches(string tfsUrl,
                                                ICredentials credentials,
                                                string workspaceName,
                                                ItemSpec[] items,
                                                VersionSpec version)
        {
            Repository webSvc = TryCreateProxy(tfsUrl, credentials);
            string username = TfsUtil.GetUsername(credentials, tfsUrl);
            return webSvc.QueryBranches(workspaceName, username, items, version);
        }

        public ItemSet[] QueryItems(string tfsUrl,
                                    ICredentials credentials,
                                    string workspaceName,
                                    string workspaceOwner,
                                    ItemSpec[] items,
                                    VersionSpec version,
                                    DeletedState deletedState,
                                    ItemType itemType,
                                    bool generateDownloadUrls)
        {
            Repository webSvc = TryCreateProxy(tfsUrl, credentials);
            try
            {
                return
                    webSvc.QueryItems(workspaceName,
                                      workspaceOwner,
                                      items,
                                      version,
                                      deletedState,
                                      itemType,
                                      generateDownloadUrls);
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("TF14002:"))
                {
                    throw new NetworkAccessDeniedException(e);
                }

                throw;
            }
        }

        #endregion

        private Repository TryCreateProxy(string tfsUrl,
                                          ICredentials credentials)
        {
            try
            {
                Repository repository = (Repository) _webSvcFactory.Create(tfsUrl, credentials);
                repository.PreAuthenticate = true;
                repository.UnsafeAuthenticatedConnectionSharing = true;
                return repository;
            }
            catch (SoapException soapEx)
            {
                throw new RepositoryUnavailableException(
                    "Failed when accessing server at: '" + tfsUrl + "' reason: " + soapEx.Detail.OuterXml, soapEx);
            }
            catch (Exception e)
            {
                throw new RepositoryUnavailableException("Failed when access server at: " + tfsUrl, e);
            }
        }
    }
}