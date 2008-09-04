using System;
using System.Collections;
using System.IO;
using System.Net;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    public delegate TFSSourceControlProvider CreateSourceControlProvider(string serverUrl, NetworkCredential credentials);

    public static class SourceControlProviderFactory
    {
        public static TFSSourceControlProvider CreateOverride;

        public static TFSSourceControlProvider Create(string serverUrl, string projectName, NetworkCredential credentials)
        {
            if (CreateOverride != null)
                return CreateOverride;

            Hashtable deps = new Hashtable();
            deps["serverUrl"] = serverUrl;
            deps["projectName"] = projectName;
            deps["credentials"] = credentials;
            return Container.Resolve<TFSSourceControlProvider>(deps);
        }
    }
}