using System.Collections;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Cache;
using SvnBridge.Infrastructure;

namespace SvnBridge.SourceControl
{
    public delegate ISourceControlProvider CreateSourceControlProvider(string serverUrl, NetworkCredential credentials);

    public static class SourceControlProviderFactory
    {
        private static CreateSourceControlProvider createDelegate;

        public static CreateSourceControlProvider CreateDelegate
        {
            set { createDelegate = value; }
        }

        public static ISourceControlProvider Create(string serverUrl, string projectName, NetworkCredential credentials)
        {
            if (createDelegate == null)
            {
                //RegistrationWebSvcFactory factory = new RegistrationWebSvcFactory();
                //FileSystem system = new FileSystem();
                //RegistrationService service = new RegistrationService(factory);
                //RepositoryWebSvcFactory factory1 = new RepositoryWebSvcFactory(factory);
                //WebTransferService webTransferService = new WebTransferService(system);
                //TFSSourceControlService tfsSourceControlService = new TFSSourceControlService(service,
                //                                                                              factory1,
                //                                                                              webTransferService,
                //                                                                              system);
                //return new TFSSourceControlProvider(serverUrl, projectName, credential, webTransferService,
                //                                 tfsSourceControlService,
                //                                 new ProjectInformationRepository(new WebCache(), tfsSourceControlService, serverUrl));
                Hashtable deps = new Hashtable();
                deps["serverUrl"] = serverUrl;
                deps["projectName"] = projectName;
                deps["credentials"] = credentials;
                return IoC.Resolve<ISourceControlProvider>(deps);

            }
            else
                return createDelegate(serverUrl, credentials);
        }
    }
}