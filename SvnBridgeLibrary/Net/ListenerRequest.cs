using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace SvnBridge.Net
{
    public class ListenerRequest : IHttpRequest
    {
        private readonly NameValueCollection headers;
        private string httpMethod;
        private MemoryStream inputStream;
        private string path;
        private Uri url;

        public ListenerRequest(Stream stream)
        {
            headers = new NameValueCollection();

            ParseRequest(stream);
        }

        #region IHttpRequest Members

        public NameValueCollection Headers
        {
            get { return headers; }
        }

        public string HttpMethod
        {
            get { return httpMethod; }
        }

        public Stream InputStream
        {
            get { return inputStream; }
        }

        public Uri Url
        {
            get
            {
                if (url == null)
                {
                    BuildUrl();
                }

                return url;
            }
        }

        #endregion

        private void BuildUrl()
        {
            string host = Headers["host"];

            if (!String.IsNullOrEmpty(host) && !path.StartsWith("http"))
            {
                url = new Uri(String.Format("http://{0}{1}", host, path));
            }
            else
            {
                url = new Uri(path);
            }
        }

        private void ParseRequest(Stream stream)
        {
            MemoryStream buffer = new MemoryStream();

            ReadToBuffer(stream, buffer);

            string startLine = ReadLine(stream, buffer);
            ParseStartLine(startLine);

            string headerLine = ReadLine(stream, buffer);
            while (headerLine != String.Empty)
            {
                ParseHeaderLine(headerLine);
                headerLine = ReadLine(stream, buffer);
            }

            ReadMessageBody(stream, buffer);
        }

        private static string ReadLine(Stream stream,
                                       MemoryStream buffer)
        {
            int offset = (int) buffer.Position;

            int previousByte = -1;
            int nextByte = buffer.ReadByte();

            while (!(previousByte == 13 && nextByte == 10))
            {
                int byteRead = buffer.ReadByte();

                if (byteRead == -1)
                {
                    ReadToBuffer(stream, buffer);
                }
                else
                {
                    previousByte = nextByte;
                    nextByte = byteRead;
                }
            }

            return Encoding.ASCII.GetString(buffer.GetBuffer(), offset, (int) buffer.Position - offset - 2);
        }

        private static void ReadToBuffer(Stream stream,
                                         MemoryStream buffer)
        {
            int originalPosition = (int) buffer.Position;

            byte[] bytes = new byte[Constants.BufferSize];

            int bytesRead = stream.Read(bytes, 0, bytes.Length);

            int availableCapacity = buffer.Capacity - (int) buffer.Length;

            if (availableCapacity < bytesRead)
            {
                buffer.Capacity += (bytesRead - availableCapacity);
            }

            buffer.Position = buffer.Length;

            buffer.Write(bytes, 0, bytesRead);

            buffer.Position = originalPosition;
        }

        private void ReadMessageBody(Stream stream,
                                     MemoryStream buffer)
        {
            int contentLength = GetContentLength();

            bool finished = ((buffer.Length - buffer.Position) >= contentLength);

            while (!finished)
            {
                ReadToBuffer(stream, buffer);

                finished = ((buffer.Length - buffer.Position) >= contentLength);
            }

            byte[] messageBody = new byte[contentLength];

            buffer.Read(messageBody, 0, messageBody.Length);

            inputStream = new MemoryStream(messageBody, false);
        }

        private int GetContentLength()
        {
            int contentLength = 0;

            string contentLengthHeader = Headers["Content-Length"];
            if (!String.IsNullOrEmpty(contentLengthHeader))
            {
                int.TryParse(contentLengthHeader, out contentLength);
            }

            return contentLength;
        }

        private void ParseStartLine(string startLine)
        {
            string[] startLineParts = startLine.Split(' ');
            httpMethod = startLineParts[0].ToLowerInvariant();
            path = startLineParts[1];
            if (path.StartsWith("//"))
            {
                path = path.Substring(1);
            }
        }

        private void ParseHeaderLine(string headerLine)
        {
            string headerName = headerLine.Substring(0, headerLine.IndexOf(": "));

            string headerValue = headerLine.Substring(headerName.Length + 2);

            Headers.Add(headerName, headerValue);
        }
    }
}