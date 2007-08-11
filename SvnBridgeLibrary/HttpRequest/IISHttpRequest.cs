using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace SvnBridge.Handlers
{
    class IISHttpRequest : IHttpRequest
    {
        HttpContext _context;

        public IISHttpRequest(HttpContext context)
        {
            _context = context;
        }

        public string HttpMethod
        {
            get { return _context.Request.HttpMethod; }
        }

        public string Path
        {
            get { return _context.Request.Path; }
        }

        public Stream InputStream
        {
            get { return _context.Request.InputStream; }
        }

        public NameValueCollection Headers
        {
            get { return _context.Request.Headers; }
        }

        public int StatusCode
        {
            set { _context.Response.StatusCode = value; }
        }

        public string ContentType
        {
            set { _context.Response.ContentType = value; }
        }

        public void Write(string output)
        {
            _context.Response.Write(output);
        }

        public Stream OutputStream
        {
            get { return _context.Response.OutputStream; }
        }

        public void AddHeader(string name,
                              string value)
        {
            _context.Response.AddHeader(name, value);
        }

        public Encoding ContentEncoding
        {
            set { _context.Response.ContentEncoding = value; }
        }

        public NetworkCredential Credentials
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool SendChunked
        {
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public void RemoveHeader(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}