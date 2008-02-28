using System;
using System.Net;

namespace SvnBridge.SourceControl
{
    public static class CredentialsHelper
    {
        public static ICredentials GetCredentialsForServer(string tfsUrl,
                                                           ICredentials credentials)
        {
            if (credentials != null)
            {
                return credentials;
            }

            Uri uri = new Uri(tfsUrl);

            if (uri.Host.ToLowerInvariant().EndsWith("codeplex.com"))
            {
                CredentialCache cache = new CredentialCache();
                cache.Add(uri, "Basic", new NetworkCredential("anonymous", null));
                return cache;
            }
            else
            {
                return CredentialCache.DefaultNetworkCredentials;
            }
        }
    }
}