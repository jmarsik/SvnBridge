using System.IO;

namespace SvnBridge.Net
{
    public class ListenerContext : IHttpContext
    {
        private readonly ListenerRequest request;
        private readonly ListenerResponse response;

        public ListenerContext(Stream stream)
        {
            request = new ListenerRequest(stream);
            response = new ListenerResponse(request, stream);
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