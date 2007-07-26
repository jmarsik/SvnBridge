using System;
using System.Collections.Specialized;
using System.IO;

namespace SvnBridge.Net
{
    public interface IHttpRequest
    {
        NameValueCollection Headers { get; }
        string HttpMethod { get; }
        Stream InputStream { get; }
        Uri Url { get; }
    }
}