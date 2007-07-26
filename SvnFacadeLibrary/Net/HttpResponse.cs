using System.IO;
using System.Net;
using System.Text;

namespace SvnBridge.Net
{
    public class HttpResponse : IHttpResponse
    {
        private readonly HttpResponseHeaderCollection headers;
        private readonly HttpResponseStream outputStream;
        private Encoding contentEncoding;
        private long? contentLength64;
        private string contentType;
        private bool sendChunked;
        private int statusCode;

        public HttpResponse(HttpRequest request, Stream stream)
        {
            headers = new HttpResponseHeaderCollection();
            outputStream = new HttpResponseStream(request, this, stream, 100);
        }

        #region IHttpResponse Members

        public void AppendHeader(string name, string value)
        {
            headers.Add(name, value);
        }

        public Encoding ContentEncoding
        {
            get { return contentEncoding; }
            set { contentEncoding = value; }
        }

        public long ContentLength64
        {
            get { return contentLength64.GetValueOrDefault(); }
            set { contentLength64 = value; }
        }

        public string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return headers; }
        }

        public Stream OutputStream
        {
            get { return outputStream; }
        }

        public bool SendChunked
        {
            get { return sendChunked; }
            set { sendChunked = value; }
        }

        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public void Close()
        {
            outputStream.Flush();
            outputStream.Close();
        }

        #endregion
    }
}