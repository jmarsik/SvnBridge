using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;

namespace SvnBridge.SourceControl
{
    public delegate ISourceControlProvider CreateSourceControlProvider(string serverUrl, NetworkCredential credential);

    public static class SourceControlProviderFactory
    {
        private static CreateSourceControlProvider createDelegate;

        public static CreateSourceControlProvider CreateDelegate
        {
            set { createDelegate = value; }
        }

        public static ISourceControlProvider Create(string serverUrl, string projectName, NetworkCredential credential)
        {
            if (createDelegate == null)
            {
                RegistrationWebSvcFactory factory = new RegistrationWebSvcFactory();
                FileSystem system = new FileSystem();
                RegistrationService service = new RegistrationService(factory);
                RepositoryWebSvcFactory factory1 = new RepositoryWebSvcFactory(factory);
                WebTransferService webTransferService = new WebTransferService(system);
                TFSSourceControlService tfsSourceControlService = new TFSSourceControlService(service,
                                                                                              factory1,
                                                                                              webTransferService,
                                                                                              system);
                return new TFSSourceControlProvider(serverUrl, projectName, credential, webTransferService,
                                                 tfsSourceControlService,
                                                 new ProjectInformationRepository(tfsSourceControlService, serverUrl));
            }
            else
                return createDelegate(serverUrl, credential);
        }
    }
}