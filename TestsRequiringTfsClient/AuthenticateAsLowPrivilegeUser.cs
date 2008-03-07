using System;
using System.Net;
using SvnBridge.SourceControl;
using TestsEndToEnd;
using TestsRequiringTfsClient.Properties;

namespace TestsRequiringTfsClient
{
    /// <summary>
    /// This class is needed so we will authenticate as a non admin user, which is required 
    /// because of the process template used in CodePlex
    /// </summary>
    public class AuthenticateAsLowPrivilegeUser : IDisposable
    {
        private readonly NetworkCredential oldCredentials;

        public AuthenticateAsLowPrivilegeUser()
        {
            oldCredentials = CredentialsHelper.DefaultCredentials;
            CredentialsHelper.DefaultCredentials =
                new NetworkCredential(Settings.Default.NonAdminUserName, Settings.Default.NonAdminUserPassword,
                                      Settings.Default.NonAdminUserDomain);
        }

        public void Dispose()
        {
            CredentialsHelper.DefaultCredentials = oldCredentials;
        }
    }
}