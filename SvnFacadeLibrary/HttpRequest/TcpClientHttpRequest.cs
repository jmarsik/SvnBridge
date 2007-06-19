using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace SvnBridge.Handlers
{
    public class TcpClientHttpRequest : IHttpRequest
    {
        string _httpMethod;
        string _path;
        NameValueCollection _inputHeaders = new NameValueCollection();
        List<KeyValuePair<string, string>> _outputHeaders = new List<KeyValuePair<string, string>>();
        Stream _inputStream;
        string _contentType;
        Encoding _contentEncoding;
        int _statusCode;
        Stream _outputStream;
        bool _sendChunked = false;

        public void SetHttpMethod(string httpMethod)
        {
            _httpMethod = httpMethod;
        }

        public void SetPath(string path)
        {
            _path = path;
        }

        public void SetInputStream(Stream inputStream)
        {
            _inputStream = inputStream;
        }

        public void SetOutputStream(Stream outputStream)
        {
            _outputStream = outputStream;
        }

        public Stream GetOutputStream()
        {
            return _outputStream;
        }

        public int GetStatusCode()
        {
            return _statusCode;
        }

        public List<KeyValuePair<string, string>> GetOutputHeaders()
        {
            return _outputHeaders;
        }

        public string GetContentType()
        {
            return _contentType;
        }

        public bool GetSendChunked()
        {
            return _sendChunked;
        }

        public string HttpMethod
        {
            get { return _httpMethod; }
        }

        public string Path
        {
            get { return _path; }
        }

        public Stream InputStream
        {
            get { return _inputStream; }
        }

        public NameValueCollection Headers
        {
            get { return _inputHeaders; }
        }

        public int StatusCode
        {
            set { _statusCode = value; }
        }

        public string ContentType
        {
            set { _contentType = value; }
        }

        public void Write(string output)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(output);
            _outputStream.Write(buffer, 0, buffer.Length);
        }

        public Stream OutputStream
        {
            get { return _outputStream; }
        }

        public void AddHeader(string name,
                              string value)
        {
            _outputHeaders.Add(new KeyValuePair<string, string>(name, value));
        }

        public void RemoveHeader(string name)
        {
            for (int i = _outputHeaders.Count - 1; i >= 0; i--)
            {
                if (_outputHeaders[i].Key == name)
                {
                    _outputHeaders.RemoveAt(i);
                }
            }
        }

        public Encoding ContentEncoding
        {
            set { _contentEncoding = value; }
        }

        public NetworkCredential Credentials
        {
            get
            {
                string auth = Headers["Authorization"];
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
            set { _sendChunked = value; }
        }
    }
}