using System.Net;
using SvnBridge.Utility;

namespace SvnBridge.Net
{
    public static class Proxy
    {
        public static void Set(ProxyInformation proxyInformation)
        {
            WebRequest.DefaultWebProxy = Helper.CreateProxy(proxyInformation);
        }
    }
}