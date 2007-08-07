using System.IO;

namespace SvnBridge.Net
{
    public class HttpContext : IHttpContext
    {
        private readonly HttpRequest request;
        private readonly HttpResponse response;

        public HttpContext(Stream stream)
        {
            request = new HttpRequest(stream);
            response = new HttpResponse(request, stream);
        }

        #region IHttpContext Members

        public IHttpRequest Request
        {
            get { return request; }
        }

        public IHttpResponse Response
        {
            get { return response; }
        }

        #endregion
    }
}