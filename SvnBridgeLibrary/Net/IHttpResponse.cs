using System.IO;
using System.Text;

namespace SvnBridge.Net
{
    public interface IHttpResponse
    {
        Encoding ContentEncoding { get; set; }
        string ContentType { get; set; }
        Stream OutputStream { get; }
        bool SendChunked { get; set; }
        int StatusCode { get; set; }

        void AppendHeader(string name,
                          string value);

        void ClearHeaders();
        void Close();
    }
}