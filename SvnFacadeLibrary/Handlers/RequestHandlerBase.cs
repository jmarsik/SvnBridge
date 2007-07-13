using System.Text;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public abstract class RequestHandlerBase : IRequestHandler
    {   
        public abstract string Method { get; }

        public void Handle(IHttpRequest request, string tfsServer)
        {
            ISourceControlProvider scp = SourceControlProviderFactory.Create(tfsServer, request.Credentials);
            WebDavService webDavService = new WebDavService(scp);

            Handle(request, webDavService);

            request.OutputStream.Flush();
        }

        protected abstract void Handle(IHttpRequest request, WebDavService webDavService);

        protected static void SendChunked(IHttpRequest request)
        {
            request.AddHeader("Transfer-Encoding", "chunked");
            request.SendChunked = true;
        }
        
        protected static void SetResponseSettings(IHttpRequest request, string contentType, Encoding contentEncoding, int status)
        {
            request.ContentType = contentType;
            request.ContentEncoding = contentEncoding;
            request.StatusCode = status;
        }
    }
}
