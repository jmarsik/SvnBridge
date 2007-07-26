using SvnBridge.Net;

namespace SvnBridge.Net
{
    public interface IHttpContextHandler
    {
        string MethodToHandle { get; }

        void Handle(IHttpContext connection, string tfsServerUrl);
    }
}