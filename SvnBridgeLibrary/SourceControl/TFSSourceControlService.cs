using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using System.Net;
using CodePlex.TfsLibrary.Utility;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlService : SourceControlService
    {
        IRepositoryWebSvcFactory _webSvcFactory;

        public TFSSourceControlService(IRegistrationService registrationService,
                                    IRepositoryWebSvcFactory webSvcFactory,
                                    IWebTransferService webTransferService,
                                    IFileSystem fileSystem) : base(registrationService, webSvcFactory, webTransferService, fileSystem)
        {
            _webSvcFactory = webSvcFactory;
        }

        public ExtendedItem[][] QueryItemsExtended(string tfsUrl, ICredentials credentials, string workspaceName, ItemSpec[] items, DeletedState deletedState, ItemType itemType)
        {
            Repository webSvc = (Repository)_webSvcFactory.Create(tfsUrl, credentials);
            string username = TfsUtil.GetUsername(credentials, tfsUrl);
            return webSvc.QueryItemsExtended(workspaceName, username, items, deletedState, itemType);
        }
    }
}
