using System;
using System.Collections;
using System.IO;
using System.Net;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public delegate ISourceControlProvider CreateSourceControlProvider(string serverUrl,
                                                                       NetworkCredential credentials);

    public static class SourceControlProviderFactory
    {
        private static CreateSourceControlProvider createDelegate;

        public static CreateSourceControlProvider CreateDelegate
        {
            set { createDelegate = value; }
        }

        public static ISourceControlProvider Create(string serverUrl,
                                                    string projectName,
                                                    NetworkCredential credentials)
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
            {
                return createDelegate(serverUrl, credentials);
            }
        }
    }
}