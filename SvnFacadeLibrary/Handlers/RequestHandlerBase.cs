using System.Text;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public abstract class RequestHandlerBase : IRequestHandler
    {   
        // Properties
        
        public abstract string Method { get; }

        // Methods

        public void Handle(IHttpRequest request, string tfsServer)
        {
            ISourceControlProvider sourceControlProvider = SourceControlProviderFactory.Create(tfsServer, request.Credentials);
            WebDavService webDavService = new WebDavService(sourceControlProvider);

            Handle(request, sourceControlProvider);

            request.OutputStream.Flush();
        }

        protected abstract void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider);

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
