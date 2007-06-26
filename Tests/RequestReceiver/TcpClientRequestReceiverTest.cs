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

            Assert.Throws<UriFormatException>(delegate
                                              {
                                                  receiver.Start(8081, "not valid");
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