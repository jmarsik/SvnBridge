using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace SvnBridge.Handlers
{
    public interface IHttpRequest
    {
        string HttpMethod { get; }
        string Path { get; }
        Stream InputStream { get; }
        NameValueCollection Headers { get; }
        int StatusCode { set; }
        string ContentType { set; }
        void Write(string output);
        Stream OutputStream { get; }

        void AddHeader(string name,
                       string value);

        void RemoveHeader(string name);
        Encoding ContentEncoding { set; }
        NetworkCredential Credentials { get; }
        bool SendChunked { set; }
    }
}