using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SvnBridge.Handlers;
using SvnBridge.Utility;

namespace SvnBridge.RequestReceiver
{
    public class TcpClientRequestReceiver : IRequestReceiver
    {
        TcpListener listener;
        Thread listenerThread;
        bool running = false;

        protected virtual RequestHandler GetRequestHandler(string tfsServer)
        {
            return new RequestHandler(tfsServer);
        }

        protected virtual int GetMaxKeepAliveConnections()
        {
            return 100;
        }

        protected virtual Stream GetStream(TcpClientHttpRequest context,
                                           Stream stream)
        {
            return new TcpClientRequestReceiverStream(context, stream, GetMaxKeepAliveConnections());
        }

        public void Start(int portNumber,
                          string tfsServer)
        {
            if (running)
                throw new InvalidOperationException("Listener is already running!");

            new Uri(tfsServer);

            running = true;

            listener = new TcpListener(IPAddress.Any, portNumber);
            listener.Start();

            listenerThread = new Thread(delegate()
                                        {
                                            ReceiveLoop(tfsServer);
                                        });
            listenerThread.Name = "Listener *:" + portNumber + " to " + tfsServer;
            listenerThread.Start();
        }

        public void Stop()
        {
            if (running)
            {
                running = false;
                listenerThread.Join();
                listener.Stop();
            }
        }

        public void ReceiveLoop(string tfsServer)
        {
            List<Thread> workerThreads = new List<Thread>();

            while (running)
            {
                if (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread workerThread = new Thread(delegate()
                                                     {
                                                         ReceiveRequest(client, tfsServer);
                                                     });
                    workerThread.IsBackground = true;
                    workerThreads.Add(workerThread);
                    IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                    workerThread.Name = "Worker request from " + endPoint.Address + ":" + endPoint.Port;
                    workerThread.Start();
                }
                Thread.Sleep(100);
            }
        }

        public void ReceiveRequest(TcpClient client,
                                   string tfsServer)
        {
            NetworkStream stream = client.GetStream();
            ProcessRequest(tfsServer, stream);
            stream.Close();
            client.Close();
        }

        public void ProcessRequest(string tfsServer,
                                   Stream stream)
        {
            RequestHandler handler = GetRequestHandler(tfsServer);

            while (true)
            {
                bool finishedLoadingRequest = false;
                byte[] buffer = new byte[0];
                TcpClientHttpRequest context = null;
                int bodyStart = 0;

                do
                {
                    byte[] buffer2 = new byte[32000];
                    int count = 0;
                    try
                    {
                        count = stream.Read(buffer2, 0, buffer2.Length);
                    }
                    catch (IOException)
                    {
                        // Ignore failures caused by client canceling request
                    }
                    if (count == 0)
                        return;

                    Helper.ReDim(ref buffer2, count);
                    Helper.ReDim(ref buffer, buffer.Length + count);
                    Array.Copy(buffer2, 0, buffer, buffer.Length - buffer2.Length, buffer2.Length);
                    context = new TcpClientHttpRequest();
                    context.SetOutputStream(GetStream(context, stream));
                    bodyStart = ReadHeader(buffer, context);
                    int contentLength = 0;

                    if (context.Headers["Content-Length"] != null)
                        contentLength = int.Parse(context.Headers["Content-Length"]);

                    if (bodyStart + contentLength == buffer.Length)
                        finishedLoadingRequest = true;
                }
                while (finishedLoadingRequest == false);

                using (MemoryStream inputStream = new MemoryStream(buffer))
                {
                    context.SetInputStream(inputStream);
                    inputStream.Position = bodyStart;
                    try
                    {
                        handler.ProcessRequest(context);
                    }
                    catch (IOException)
                    {
                        // Ignore failures caused by client canceling request
                    }
                }
                try
                {
                    context.GetOutputStream().Flush();
                }
                catch (IOException)
                {
                    // Ignore failures caused by client canceling request
                }
            }
        }

        static int ReadHeader(byte[] data,
                              TcpClientHttpRequest context)
        {
            int lineStart = 0;
            int index = 0;

            while (data[index] != 13)
                index++;

            string line = Encoding.UTF8.GetString(data, lineStart, index - lineStart);
            string[] splitLine = line.Split(' ');
            context.SetHttpMethod(splitLine[0]);
            context.SetPath(splitLine[1]);

            bool finished = false;

            while (!finished)
            {
                index += 2;
                lineStart = index;

                while (data[index] != 13)
                    index++;

                if (index == lineStart)
                    finished = true;
                else
                {
                    line = Encoding.UTF8.GetString(data, lineStart, index - lineStart);
                    string key = line.Substring(0, line.IndexOf(": "));
                    string value = line.Substring(key.Length + 2);
                    context.Headers[key] = value;
                }
            }

            return index + 2;
        }
    }
}