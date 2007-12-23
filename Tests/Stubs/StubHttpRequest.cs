using System;
using System.Collections.Specialized;
using System.IO;
using SvnBridge.Net;
using System.Text;

namespace SvnBridge.Stubs
{
    public class StubHttpRequest : IHttpRequest
    {
        private NameValueCollection headers;
        private string httpMethod;
        private Stream inputStream;
        private Uri url;

        public NameValueCollection Headers
        {
            get { return headers; }
            set { headers = value; }
        }

        public string HttpMethod
        {
            get { return httpMethod; }
            set { httpMethod = value; }
        }

        public string Input
        {
            set { inputStream = new MemoryStream(Encoding.Default.GetBytes(value)); }
        }

        public Stream InputStream
        {
            get { return inputStream; }
            set { inputStream = value; }
        }

        public Uri Url
        {
            get { return url; }
            set { url = value; }
        }

        public string Path
        {
            set
            {
                string path = value;
                if (path.Length > path.IndexOf("/", path.IndexOf("://") + 3) + 1)
                    if (path.Substring(path.IndexOf("/", path.IndexOf("://") + 3), 2) == "//")
                        path = path.Remove(path.IndexOf("/", path.IndexOf("://") + 3), 1);
                
                url = new Uri(path);
                headers["Host"] = url.Authority;
            }
        }
    }
}