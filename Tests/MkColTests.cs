using Attach;
using NUnit.Framework;
using SvnBridge.Handlers;
using System.IO;

namespace Tests
{
    [TestFixture]
    public class MkColTests : WebDavServiceTestsBase
    {
        [Test]
        public void MkColSvnWrkActivityIdDirectory()
        {
            Results results = mock.Attach(provider.MakeCollection);
            TcpClientHttpRequest request = new TcpClientHttpRequest();
            request.SetHttpMethod("mkcol");
            request.SetPath("//!svn/wrk/5b34ae67-87de-3741-a590-8bda26893532/Spikes/SvnFacade/trunk/Empty");
            request.Headers.Add("Host", "localhost:8081");
            MemoryStream outputStream = new MemoryStream();
            request.SetOutputStream(outputStream);

            RequestDispatcherFactory.Create(null).Dispatch(request);

            Assert.AreEqual(1, results.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", (string)results.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/Empty", (string)results.Parameters[1]);

            outputStream.Dispose();
        }
    }
}