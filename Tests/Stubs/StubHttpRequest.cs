using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using SvnBridge.Interfaces;

namespace UnitTests
{
    public class StubHttpRequest : IHttpRequest
    {
        private NameValueCollection headers = new NameValueCollection();
        private string httpMethod;
        private string applicationPath = "";
        private Stream inputStream = new MemoryStream();
        private Uri url;

        public string Input
        {
            set { inputStream = new MemoryStream(Encoding.Default.GetBytes(value)); }
        }

        public string Path
        {
            set
            {
                string path = value;
                if (path.Length > path.IndexOf("/", path.IndexOf("://") + 3) + 1)
                {
                    if (path.Substring(path.IndexOf("/", path.IndexOf("://") + 3), 2) == "//")
                    {
                        path = path.Remove(path.IndexOf("/", path.IndexOf("://") + 3), 1);
                    }
                }

                url = new Uri(path);
                headers["Host"] = url.Authority;
            }
        }

        public string ApplicationPath
        {
            get {
                if (applicationPath == "")
                    return "/";
                else
                    return applicationPath;
            }
            set {
                if (value.StartsWith("/"))
                    applicationPath = value;
                else
                    applicationPath = "/" + value;
            }
        }

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

        public string LocalPath
        {
            get
            {
                return url.LocalPath.Substring(applicationPath.Length);
            }
        }
    }
}