using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Handlers;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace Subvert
{
    public class HttpListenerHttpRequest : IHttpRequest
    {
        private HttpListenerContext _context;

        public HttpListenerHttpRequest(HttpListenerContext context)
        {
            _context = context;
        }

        public string HttpMethod
        {
            get { return _context.Request.HttpMethod; }
        }

        public string Path
        {
            get { return _context.Request.RawUrl; }
        }

        public Stream InputStream
        {
            get { return _context.Request.InputStream; }
        }

        public Stream OutputStream
        {
            get { return _context.Response.OutputStream; }
        }

        public NameValueCollection Headers
        {
            get { return _context.Request.Headers; }
        }

        public int StatusCode
        {
            set {
                _context.Response.StatusCode = value;
                if (value == 401)
                {
                    _context.Response.StatusDescription = "Authorization Required";
                }
            }
        }

        public string ContentType
        {
            set { _context.Response.ContentType = value; }
        }

        public Encoding ContentEncoding
        {
            set { _context.Response.ContentEncoding = value; }
        }

        public void Write(string output)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(output);
            _context.Response.ContentLength64 = buffer.Length;
            Stream outputStream = _context.Response.OutputStream;
            outputStream.Write(buffer, 0, buffer.Length);
        }

        public void AddHeader(string name, string value)
        {
            _context.Response.AddHeader(name, value);
        }

        public NetworkCredential Credentials
        {
            get {
                string auth = _context.Request.Headers["Authorization"];
                if (auth != null)
                {
                    auth = auth.Substring(auth.IndexOf(' ') + 1);
                    auth = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(auth));
                    string username = auth.Split(':')[0];
                    string password = auth.Split(':')[1];
                    if (username.IndexOf('\\') >= 0)
                    {
                        string domain = username.Substring(0, username.IndexOf('\\'));
                        username = username.Substring(username.IndexOf('\\')+1);
                        return new NetworkCredential(username, password, domain);
                    }
                    else
                    {
                        return new NetworkCredential(username, password);
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public bool SendChunked
        {
            set { _context.Response.SendChunked = value; }
        }

        #region IHttpRequest Members


        public void RemoveHeader(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
