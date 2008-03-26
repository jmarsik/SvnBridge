using SvnBridge.Interfaces;

namespace SvnBridge.Net
{
    public interface IHttpContext
    {
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
    }
}
