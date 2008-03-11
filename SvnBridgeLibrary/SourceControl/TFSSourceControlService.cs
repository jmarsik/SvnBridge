using System;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Exceptions;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.Proxies;
using SvnBridge.Utility;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlService : SourceControlService, ITFSSourceControlService
    {
        private readonly IRepositoryWebSvcFactory _webSvcFactory;
        private readonly ICache cache;

        public TFSSourceControlService(IRegistrationService registrationService,
                                       IRepositoryWebSvcFactory webSvcFactory,
                                       IWebTransferService webTransferService,
                                       IFileSystem fileSystem,
                                       ICache cache)
            : base(registrationService, webSvcFactory, webTransferService, fileSystem)
        {
            _webSvcFactory = webSvcFactory;
            this.cache = cache;
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
            
           // NetworkCredential credential = credentials.GetCredential(new Uri(tfsUrl), "");
           // string cacheKey = new StringBuilder("QueryItems_")
           //     .Append(tfsUrl).Append("_")
           //     .Append(credential.UserName).Append("@").Append(credential.Domain)
           //         .Append("#").Append(credential.Password.GetHashCode())
           //     .Append(workspaceName).Append("_")
           //     .Append(workspaceOwner).Append("_")
           //     .Append(GetChangeSet(version, tfsUrl, credential)).Append("_")
           //     .Append(Helper.SerializeXmlString(deletedState)).Append("_")
           //     .Append(Helper.SerializeXmlString(itemType)).Append("_")
           //     .Append(generateDownloadUrls).Append("_")
           //     .ToString();

           //CachedResult result = cache.Get(cacheKey);
           //if (result != null)
           //    return (ItemSet[]) result.Value;

            Repository webSvc = TryCreateProxy(tfsUrl, credentials);
            try
            {
                ItemSet[] queryItems = webSvc.QueryItems(workspaceName,
                                                         workspaceOwner,
                                                         items,
                                                         version,
                                                         deletedState,
                                                         itemType,
                                                         generateDownloadUrls);
                //cache.Set(cacheKey, queryItems);
                return queryItems;
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

        private int GetChangeSet(VersionSpec version, string tfsUrl, ICredentials credentials)
        {
            if(version is LatestVersionSpec)
                return GetLatestChangeset(tfsUrl, credentials);
            return ((ChangesetVersionSpec) version).cs;
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