using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SvnBridge.Net;

namespace SvnBridge.Stubs
{
    public class StubHttpResponse : IHttpResponse
    {
        private Encoding contentEncoding;
        private long contentLength64;
        private string contentType;
        private WebHeaderCollection headers;
        private Stream outputStream;
        private bool sendChunked;
        private int statusCode;

        public Encoding ContentEncoding
        {
            get { return contentEncoding; }
            set { contentEncoding = value; }
        }

        public long ContentLength64
        {
            get { return contentLength64; }
            set { contentLength64 = value; }
        }

        public string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return headers; }
            set { headers = value; }
        }

        public Stream OutputStream
        {
            get { return outputStream; }
            set { outputStream = value; }
        }

        public bool SendChunked
        {
            get { return sendChunked; }
            set { sendChunked = value; }
        }

        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public void AppendHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Write(string content)
        {
            byte[] buffer = contentEncoding.GetBytes(content);

            ContentLength64 = buffer.Length;

            outputStream.Write(buffer, 0, buffer.Length);
        }
    }
}