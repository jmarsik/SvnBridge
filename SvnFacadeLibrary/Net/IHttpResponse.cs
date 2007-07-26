using System.IO;
using System.Net;
using System.Text;

namespace SvnBridge.Net
{
    public interface IHttpResponse
    {
        Encoding ContentEncoding { get; set; }
        long ContentLength64 { get; set; }
        string ContentType { get; set; }
        WebHeaderCollection Headers { get; }
        Stream OutputStream { get; }
        bool SendChunked { get; set; }
        int StatusCode { get; set; }

        void AppendHeader(string name, string value);
        void Close();
    }
}