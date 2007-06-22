using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using SvnBridge.Handlers;

namespace SvnBridge
{
    public class HttpListenerHttpRequest : IHttpRequest
    {
        HttpListenerContext context;
        UnclosableStream unclosableOutputStream;

        public HttpListenerHttpRequest(HttpListenerContext context)
        {
            this.context = context;
            unclosableOutputStream = new UnclosableStream(context.Response.OutputStream);
        }

        public string HttpMethod
        {
            get { return context.Request.HttpMethod; }
        }

        public string Path
        {
            get { return context.Request.RawUrl; }
        }

        public Stream InputStream
        {
            get { return context.Request.InputStream; }
        }

        public Stream OutputStream
        {
            get { return unclosableOutputStream; }
        }

        public NameValueCollection Headers
        {
            get { return context.Request.Headers; }
        }

        public int StatusCode
        {
            set
            {
                context.Response.StatusCode = value;
                if (value == 401)
                {
                    context.Response.StatusDescription = "Authorization Required";
                }
            }
        }

        public string ContentType
        {
            set { context.Response.ContentType = value; }
        }

        public Encoding ContentEncoding
        {
            set { context.Response.ContentEncoding = value; }
        }

        public void Write(string output)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(output);
            context.Response.ContentLength64 = buffer.Length;
            Stream outputStream = context.Response.OutputStream;
            outputStream.Write(buffer, 0, buffer.Length);
        }

        public void AddHeader(string name,
                              string value)
        {
            context.Response.AddHeader(name, value);
        }

        public NetworkCredential Credentials
        {
            get
            {
                string auth = context.Request.Headers["Authorization"];
                if (auth != null)
                {
                    auth = auth.Substring(auth.IndexOf(' ') + 1);
                    auth = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(auth));
                    string username = auth.Split(':')[0];
                    string password = auth.Split(':')[1];
                    if (username.IndexOf('\\') >= 0)
                    {
                        string domain = username.Substring(0, username.IndexOf('\\'));
                        username = username.Substring(username.IndexOf('\\') + 1);
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
            set { context.Response.SendChunked = value; }
        }

        public void RemoveHeader(string name)
        {
            context.Response.Headers.Remove(name);
        }

        class UnclosableStream : Stream
        {
            // Fields

            Stream innerStream;

            // Lifetime

            public UnclosableStream(Stream innerStream)
            {
                this.innerStream = innerStream;
            }

            // Properties

            public override bool CanRead
            {
                get { return innerStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return innerStream.CanSeek; }
            }

            public override bool CanWrite
            {
                get { return innerStream.CanWrite; }
            }

            public override long Length
            {
                get { return innerStream.Length; }
            }

            public override long Position
            {
                get { return innerStream.Position; }
                set { innerStream.Position = value; }
            }

            // Methods

            public override void Close()
            {
            }

            public override void Flush()
            {
                innerStream.Flush();
            }

            public override int Read(byte[] buffer,
                                     int offset,
                                     int count)
            {
                return innerStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset,
                                      SeekOrigin origin)
            {
                return innerStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                innerStream.SetLength(value);
            }

            public override void Write(byte[] buffer,
                                       int offset,
                                       int count)
            {
                innerStream.Write(buffer, offset, count);
            }
        }
    }
}