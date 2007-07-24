using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using SvnBridge.Handlers;
using SvnBridge.RequestReceiver;
using SvnBridge.SourceControl;
using SvnBridge.Utility;
using Tests.Infrastructure;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace Tests
{
    public abstract class WebDavServiceTestsBase
    {
        protected MyMocks mock = new MyMocks();
        protected StubSourceControlProvider provider;
        protected MockContext context;
        protected WebDavService service;
        protected TcpClientRequestReceiver receiver;
        protected string actual;

        MemoryStream stream;
        StreamWriter writer;

        [SetUp]
        public virtual void Setup()
        {
            provider = mock.CreateObject<StubSourceControlProvider>();
            SourceControlProviderFactory.CreateDelegate = delegate { return provider; };
            service = new WebDavService(provider);
            context = new MockContext();
            receiver = new TestableTcpClientRequestReceiver();
        }

        [TearDown]
        public virtual void TearDown()
        {
            SourceControlProviderFactory.CreateDelegate = null;
        }

        protected Stream GetStream()
        {
            stream = new TestableOutputStream();
            return stream;
        }

        protected byte[] GetBytes(string data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)data[i];
            }
            return result;
        }

        protected SourceItemChange MakeChange(ChangeType changeType, string serverPath)
        {
            SourceItemChange result = new SourceItemChange();
            result.Item = SourceItem.FromRemoteItem(0, ItemType.Folder, serverPath, 0, 0, DateTime.Now, null);
            result.ChangeType = changeType;
            return result;
        }

        protected void SetChunks(int[] chunks)
        {
            ((TestableTcpClientRequestReceiver)receiver).chunks = chunks;
        }

        protected string ProcessRequest(string request,
                                        string expected)
        {
            string time = expected.Substring(expected.IndexOf("Date:") + 6);
            time = time.Substring(0, time.IndexOf("\r\n"));
            Clock.FreezeTime(DateTime.Parse(time));

            if (expected.IndexOf("Keep-Alive:") > -1)
            {
                string keepAliveMax = expected.Substring(expected.IndexOf("Keep-Alive:") + 28);
                keepAliveMax = keepAliveMax.Substring(0, keepAliveMax.IndexOf("\r\n"));
                ((TestableTcpClientRequestReceiver)receiver).keepAliveMax = int.Parse(keepAliveMax);
            }

            ReadWriteMemoryStream stream = new ReadWriteMemoryStream();
            stream.SetInput(GetBytes(request));
            receiver.ProcessRequest(null, stream);
            return Encoding.UTF8.GetString(stream.GetOutput());
        }

        protected string ReadStream()
        {
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        protected StreamWriter GetWriter()
        {
            stream = new MemoryStream();
            writer = new StreamWriter(stream);
            return writer;
        }

        protected string ReadWriter()
        {
            writer.Flush();
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        protected string GetResults()
        {
            context.OutputStream.Position = 0;
            using (StreamReader reader = new StreamReader(context.OutputStream))
            {
                return reader.ReadToEnd();
            }
        }

        protected T DeserializeRequest<T>(string xml)
        {
            return Helper.DeserializeXml<T>(xml);
        }

        protected string SerializeResponse<T>(T response,
                                              XmlSerializerNamespaces ns)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = false;
            settings.Encoding = Encoding.UTF8;
            StringBuilder xml = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(xml, settings);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, response, ns);
            writer.Flush();
            return xml.ToString();
        }
    }

    class TestableTcpClientRequestReceiverStream : TcpClientRequestReceiverStream
    {
        byte[] _data = null;
        int[] _chunks = null;
        int _chunkIndex = 0;

        public TestableTcpClientRequestReceiverStream(TcpClientHttpRequest context,
                                                      Stream output,
                                                      int maxKeepAliveConnections)
            : base(context, output, maxKeepAliveConnections) {}

        public void SetChunks(int[] chunks)
        {
            _chunks = chunks;
        }

        public override void Flush()
        {
            if (!_finished)
            {
                WriteHeader();
                if (_context.GetSendChunked())
                {
                    byte[] chunkFooter;
                    if (_data != null && _data.Length > 0)
                    {
                        byte[] chunkHeader = Encoding.UTF8.GetBytes(string.Format("{0:x}", _data.Length) + "\r\n");
                        chunkFooter = Encoding.UTF8.GetBytes("\r\n");
                        _output.Write(chunkHeader, 0, chunkHeader.Length);
                        _output.Write(_data, 0, _data.Length);
                        _output.Write(chunkFooter, 0, chunkFooter.Length);
                    }
                    chunkFooter = Encoding.UTF8.GetBytes("0\r\n\r\n");
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

        public override void Write(byte[] buffer,
                                   int offset,
                                   int count)
        {
            if (_context.GetSendChunked() && _chunks != null)
            {
                if (_data == null)
                {
                    byte[] newData = new byte[count];
                    Array.Copy(buffer, offset, newData, 0, count);
                    _data = newData;
                }
                else
                {
                    byte[] newData = new byte[_data.Length + count];
                    Array.Copy(_data, 0, newData, 0, _data.Length);
                    Array.Copy(buffer, offset, newData, _data.Length, count);
                    _data = newData;
                }
                if (_chunkIndex >= _chunks.Length)
                {
                    base.Write(_data, 0, _data.Length);
                    _data = null;
                    _chunkIndex++;
                }
                else
                {
                    if (_data.Length >= _chunks[_chunkIndex])
                    {
                        base.Write(_data, 0, _chunks[_chunkIndex]);
                        byte[] newData = new byte[_data.Length - _chunks[_chunkIndex]];
                        Array.Copy(_data, _chunks[_chunkIndex], newData, 0, newData.Length);
                        _data = newData;
                        _chunkIndex++;
                    }
                }
            }
            else
            {
                base.Write(buffer, offset, count);
            }
        }
    }

    class TestableTcpClientRequestReceiver : TcpClientRequestReceiver
    {
        public int keepAliveMax = 100;
        public int[] chunks = null;

        protected override int GetMaxKeepAliveConnections()
        {
            return keepAliveMax;
        }

        protected override Stream GetStream(TcpClientHttpRequest context,
                                            Stream stream)
        {
            TestableTcpClientRequestReceiverStream newStream = new TestableTcpClientRequestReceiverStream(context, stream, GetMaxKeepAliveConnections());
            newStream.SetChunks(chunks);
            return newStream;
        }
    }
}