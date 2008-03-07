using System;
using System.Collections.Specialized;
using System.IO;

namespace SvnBridge.Net
{
    public interface IHttpRequest
    {
        string ApplicationPath { get; }
        NameValueCollection Headers { get; }
        string HttpMethod { get; }
        Stream InputStream { get; }
        Uri Url { get; }

        string LocalPath { get; }
    }
}