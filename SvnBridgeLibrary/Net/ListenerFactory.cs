using System.Net;
using SvnBridge.Utility;

namespace SvnBridge.Net
{
    public static class ListenerFactory
    {
        public static IListener Create()
        {
            return new Listener();
        }
    }
}