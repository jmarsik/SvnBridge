using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SvnBridge.Handlers;
using SvnBridge.Utility;

namespace SvnBridge.RequestReceiver
{
    public class TcpClientRequestReceiverStream : Stream
    {
        protected TcpClientHttpRequest _context;
        protected Stream _output;
        protected bool _headerWritten = false;
        protected bool _finished = false;
        protected MemoryStream _stream = new MemoryStream();
        protected int _maxKeepAliveConnections;

        public TcpClientRequestReceiverStream(TcpClientHttpRequest context,
                                              Stream output,
                                              int maxKeepAliveConnections)
        {
            _context = context;
            _output = output;
            _maxKeepAliveConnections = maxKeepAliveConnections;
        }

        public override bool CanRead
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            if (!_finished)
            {
                WriteHeader();
                if (_context.GetSendChunked())
                {
                    byte[] chunkFooter = Encoding.UTF8.GetBytes("0\r\n\r\n");
                    _output.Write(chunkFooter, 0, chunkFooter.Length);
                }
                else
                {
                    byte[] response = _stream.ToArray();
                    _output.Write(response, 0, response.Length);
                }
                _finished = true;
            }
        }

        public override long Length
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override long Position
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override int Read(byte[] buffer,
                                 int offset,
                                 int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long Seek(long offset,
                                  SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Write(byte[] buffer,
                                   int offset,
                                   int count)
        {
            if (_context.GetSendChunked())
            {
                WriteHeader();
                byte[] chunkHeader = Encoding.UTF8.GetBytes(string.Format("{0:x}", count) + "\r\n");
                byte[] chunkFooter = Encoding.UTF8.GetBytes("\r\n");
                _output.Write(chunkHeader, 0, chunkHeader.Length);
                _output.Write(buffer, offset, count);
                _output.Write(chunkFooter, 0, chunkFooter.Length);
            }
            else
            {
                _stream.Write(buffer, offset, count);
            }
        }

        protected void WriteHeader()
        {
            if (!_headerWritten)
            {
                string responseDescription = "";
                switch (_context.GetStatusCode())
                {
                    case 200:
                        responseDescription = "OK";
                        break;
                    case 201:
                        responseDescription = "Created";
                        break;
                    case 204:
                        responseDescription = "No Content";
                        break;
                    case 207:
                        responseDescription = "Multi-Status";
                        break;
                    case 401:
                        responseDescription = "Authorization Required";
                        break;
                    case 404:
                        responseDescription = "Not Found";
                        break;
                    case 405:
                        responseDescription = "Method Not Allowed";
                        break;
                    case 409:
                        responseDescription = "Conflict";
                        break;
                }
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("HTTP/1.1 " + _context.GetStatusCode().ToString() + " " + responseDescription);
                builder.AppendLine("Date: " + Clock.GetDate().ToUniversalTime().ToString("R"));
                builder.AppendLine("Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2");
                foreach (KeyValuePair<string, string> header in _context.GetOutputHeaders())
                {
                    if (header.Key != "X-Pad")
                    {
                        builder.AppendLine(header.Key + ": " + header.Value);
                    }
                }
                if (!_context.GetSendChunked())
                {
                    builder.AppendLine("Content-Length: " + _stream.Length.ToString());
                }
                string[] connectionHeaderValues = _context.Headers["Connection"].Split(',');
                foreach (string value in connectionHeaderValues)
                {
                    if (value.TrimStart() == "Keep-Alive")
                    {
                        builder.AppendLine("Keep-Alive: timeout=15, max=" + _maxKeepAliveConnections.ToString());
                        builder.AppendLine("Connection: Keep-Alive");
                    }
                }
                builder.AppendLine("Content-Type: " + _context.GetContentType());
                foreach (KeyValuePair<string, string> header in _context.GetOutputHeaders())
                {
                    if (header.Key == "X-Pad")
                    {
                        builder.AppendLine(header.Key + ": " + header.Value);
                    }
                }
                builder.AppendLine("");

                byte[] headers = Encoding.UTF8.GetBytes(builder.ToString());
                _output.Write(headers, 0, headers.Length);
                _headerWritten = true;
            }
        }
    }
}