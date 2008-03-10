using System.Net;

namespace SvnBridge.Interfaces
{
    public interface ICredentialsProvider 
    {
        ICredentials GetCredentials();
    }
}