using System;
using System.Net;
using IntegrationTests.Properties;
using SvnBridge.SourceControl;

namespace IntegrationTests
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
            if (string.IsNullOrEmpty(Settings.Default.Username.Trim()))
                return;
            CredentialsHelper.NullCredentials = CredentialsHelper.DefaultCredentials =
                new NetworkCredential(Settings.Default.Username, Settings.Default.Password,
                                      Settings.Default.Domain);
        }

        public void Dispose()
        {
            CredentialsHelper.DefaultCredentials = oldCredentials;
            CredentialsHelper.NullCredentials = null;
        }
    }
}