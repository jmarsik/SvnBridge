using System.Net;

namespace SvnBridge.SourceControl
{
    public interface ICredentialsProvider 
    {
        ICredentials GetCredentials();
    }
}