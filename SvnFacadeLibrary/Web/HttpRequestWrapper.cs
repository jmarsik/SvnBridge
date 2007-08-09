using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using SvnBridge.Net;

namespace SvnBridge.Web
{
    public class HttpRequestWrapper : IHttpRequest
    {
        private readonly HttpRequest request;

        public HttpRequestWrapper(HttpRequest request)
        {
            this.request = request;
        }

        #region IHttpRequest Members

        public NameValueCollection Headers
        {
            get { return request.Headers; }
        }

        public string HttpMethod
        {
            get { return request.HttpMethod; }
        }

        public Stream InputStream
        {
            get { return request.InputStream; }
        }

        public Uri Url
        {
            get { return request.Url; }
        }

        #endregion
    }
}