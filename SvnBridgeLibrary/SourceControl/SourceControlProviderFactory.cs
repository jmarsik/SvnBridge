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