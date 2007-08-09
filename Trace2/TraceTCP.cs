using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Trace
{
    public partial class TraceTCP : Form
    {
        static bool _keepListening = true;
        static TcpListener _server;
        static string _targetServer;
        static string _targetPort;

        public TraceTCP()
        {
            InitializeComponent();
        }

        void button1_Click(object sender,
                           EventArgs e)
        {
            txtPort.Enabled = false;
            txtTargetPort.Enabled = false;
            txtTargetServer.Enabled = false;
            button1.Enabled = false;

            File.WriteAllText("c:\\output1.txt", "");
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            int port = int.Parse(txtPort.Text);
            _server = new TcpListener(localAddr, port);
            _targetServer = txtTargetServer.Text;
            _targetPort = txtTargetPort.Text;

            File.WriteAllText(@"c:\Tests.txt", "");
            WriteTestLogLine("using System;");
            WriteTestLogLine("using CodePlex.TfsLibrary;");
            WriteTestLogLine("using NUnit.Framework;");
            WriteTestLogLine("using SvnBridge.SourceControl;");
            WriteTestLogLine("using CodePlex.TfsLibrary.RepositoryWebSvc;");

            WriteTestLogLine("");
            WriteTestLogLine("namespace Tests");
            WriteTestLogLine("{");
            WriteTestLogLine("    [TestFixture]");
            WriteTestLogLine("    public class Tests : ProtocolTestsBase");
            WriteTestLogLine("    {");

            Thread requestProcessor = new Thread(StartListening);
            requestProcessor.Start();
        }

        public static void WriteTestLog(string log)
        {
            bool retry = false;
            do
            {
                retry = false;
                try
                {
                    File.AppendAllText(@"c:\Tests.txt", log);
                }
                catch
                {
                    System.Threading.Thread.Sleep(100);
                    retry = true;
                }
            }
            while (retry);
        }

        public static void WriteTestLogLine(string log)
        {
            WriteTestLog(log + "\r\n");
        }

        public static void StartListening()
        {
            List<Thread> threads = new List<Thread>();
            _server.Start();
            while (_keepListening)
            {
                TcpClient client = _server.AcceptTcpClient();
                Thread newConnection = new Thread(HandleConnection);
                threads.Add(newConnection);
                newConnection.Start(new object[] { client, _targetServer, _targetPort });
            }
        }

        public static void HandleConnection(object parameters)
        {
            TcpClient client = (TcpClient)((object[])parameters)[0];
            string serverName = (string)((object[])parameters)[1];
            string port = (string)((object[])parameters)[2];
            NetworkStream clientStream = client.GetStream();

            TcpClient server = new TcpClient(serverName, int.Parse(port));
            NetworkStream serverStream = server.GetStream();
            CopyStreams(clientStream, serverStream);
            serverStream.Close();
            server.Close();

            clientStream.Close();
            client.Close();
        }

        public static void CopyStreams(NetworkStream input,
                                       NetworkStream output)
        {
            byte[] buffer = new byte[5000];
            Thread copyInput = new Thread(CopyStream);
            Thread copyOutput = new Thread(CopyStream);

            copyInput.Start(new object[] { input, output, 1 });
            copyOutput.Start(new object[] { output, input, 2 });

            copyInput.Join();
            copyOutput.Join();
        }

        public static void WriteLog(byte[] buffer, int count)
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    //string data = Encoding.UTF8.GetString(buffer, 0, count);
                    using (FileStream stream = File.OpenWrite("c:\\output1.txt"))
                    {
                        stream.Position = stream.Length;
                        //byte[] start = Encoding.UTF8.GetBytes("++");
                        //stream.Write(start, 0, start.Length);
                        stream.Write(buffer, 0, count);
                        //byte[] end = Encoding.UTF8.GetBytes("--");
                        //stream.Write(end, 0, end.Length);
                    }
                    //System.Diagnostics.Debug.Write(data);
                }
                catch
                {
                    retry = true;
                    System.Threading.Thread.Sleep(100);
                }
            }
            while (retry);
        }

        public static void CopyStream(object parameters)
        {
            Stream input = (Stream)((object[])parameters)[0];
            Stream output = (Stream)((object[])parameters)[1];
            int direction = (int)((object[])parameters)[2];
            byte[] buffer = new byte[5000];
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
            {
                WriteTest(buffer, count, direction);
                WriteLog(buffer, count);
                output.Write(buffer, 0, count);
            }
        }

        static int _lastDirection = 0;
        static int _testCount = 0;

        public static void WriteTest(byte[] buffer,
                                     int count,
                                     int direction)
        {
            StringBuilder output = new StringBuilder();
            if (_lastDirection == 0)
            {
                output.AppendLine("        [Test]");
                _testCount++;
                output.AppendLine("        public void Test" + _testCount.ToString() + "()");
                output.AppendLine("        {");
                output.AppendLine("            string request =");
            }
            else
            {
                if (_lastDirection == direction)
                {
                    output.AppendLine("\" +");
                }
                else
                {
                    output.AppendLine("\";");
                    if (direction == 1)
                    {
                        output.AppendLine("");
                        output.AppendLine("            string actual = ProcessRequest(request, ref expected);");
                        output.AppendLine("");
                        output.AppendLine("            Assert.AreEqual(expected, actual);");
                        output.AppendLine("        }");
                        output.AppendLine("");
                        output.AppendLine("        [Test]");
                        _testCount++;
                        output.AppendLine("        public void Test" + _testCount.ToString() + "()");
                        output.AppendLine("        {");
                        output.AppendLine("            string request =");
                    }
                    else
                    {
                        output.AppendLine("");
                        output.AppendLine("            string expected =");
                    }
                }
            }
            output.Append("                \"");
            for (int i = 0; i < count; i++)
            {
                if (buffer[i] == 0)
                    output.Append("\\0");
                else if (buffer[i] == 10)
                {
                    output.Append("\\n");
                    if (i + 1 < count)
                    {
                        output.AppendLine("\" +");
                        output.Append("                \"");
                    }
                }
                else if (buffer[i] == 13)
                    output.Append("\\r");
                else if (buffer[i] == 34)
                    output.Append("\\\"");
                else if (buffer[i] == 34)
                    output.Append("\\\"");
                else if (buffer[i] == 92)
                    output.Append("\\\\");
                else if (buffer[i] < 32 || buffer[i] > 126)
                    output.Append("\\u00" + string.Format("{0:X2}", buffer[i]));
                else
                    output.Append(Encoding.UTF8.GetString(buffer, i, 1));
            }
            WriteTestLog(output.ToString());
            _lastDirection = direction;
        }
    }
}