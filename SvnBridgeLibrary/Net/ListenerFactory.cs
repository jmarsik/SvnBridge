using System.Net;

namespace SvnBridge.Net
{
    public static class ListenerFactory
    {
        public static void SetProxy(ProxyInformation proxyInformation)
        {
            WebRequest.DefaultWebProxy = CreateProxy(proxyInformation);
        }

        public static IWebProxy CreateProxy(ProxyInformation proxyInformation)
        {
            if (proxyInformation.UseProxy == false)
                return null;
            IWebProxy proxy = new WebProxy(proxyInformation.Url, proxyInformation.Port);
            ICredentials credential;
            if (proxyInformation.UseDefaultCredentails)
            {
                credential = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                credential = new NetworkCredential(proxyInformation.Username, proxyInformation.Password);
            }
            proxy.Credentials = credential;
            return proxy;
        }

        public static IListener Create()
        {
            return new Listener();
        }
    }
}