using System;
using NUnit.Framework;
using SvnBridge.RequestReceiver;
using Assert=CodePlex.NUnitExtensions.Assert;

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
    }
}