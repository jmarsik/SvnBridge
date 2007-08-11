using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace SvnBridge.Handlers
{
    public interface IHttpRequest
    {
        // Properties

        Encoding ContentEncoding { set; }
        string ContentType { set; }
        NetworkCredential Credentials { get; }
        NameValueCollection Headers { get; }
        string HttpMethod { get; }
        Stream InputStream { get; }
        Stream OutputStream { get; }
        string Path { get; }
        bool SendChunked { set; }
        int StatusCode { set; }

        // Methods

        void AddHeader(string name,
                       string value);

        void RemoveHeader(string name);
        void Write(string output);
    }
}