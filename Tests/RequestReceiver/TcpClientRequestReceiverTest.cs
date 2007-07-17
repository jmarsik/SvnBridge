using System;
using NUnit.Framework;
using SvnBridge.RequestReceiver;
using Assert=CodePlex.NUnitExtensions.Assert;
using System.IO;

namespace Tests.RequestReceiver
{
    [TestFixture]
    public class TcpClientRequestReceiverTest
    {
        [Test]
        public void InvalidTfsServerUrlThrows()
        {
            TcpClientRequestReceiver receiver = new TcpClientRequestReceiver();

            receiver.Port = 8081;
            receiver.TfsServerUrl = "not valid";
            Assert.Throws<UriFormatException>(delegate
                                              {
                                                  receiver.Start();
                                              });
        }

        [Test]
        public void ShouldIgnoreIOExceptionsDuringProcessRequest()
        {
            MyMocks mock = new MyMocks();
            MemoryStream stream = mock.CreateObject<MemoryStream>();
            mock.Attach(stream.Read, new IOException());
            TcpClientRequestReceiver receiver = new TcpClientRequestReceiver();

            receiver.ProcessRequest(null, stream);
        }
    }
}