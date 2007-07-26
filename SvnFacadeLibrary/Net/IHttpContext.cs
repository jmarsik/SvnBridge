using System.Security.Principal;

namespace SvnBridge.Net
{
    public interface IHttpContext
    {
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
        IPrincipal User { get; }
    }
}