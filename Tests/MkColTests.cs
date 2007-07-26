using System;
using System.Collections.Specialized;
using Attach;
using NUnit.Framework;
using SvnBridge;
using SvnBridge.Handlers;
using System.IO;
using SvnBridge.Net;
using SvnBridge.Stubs;

namespace Tests
{
    [TestFixture]
    public class MkColTests : WebDavServiceTestsBase
    {
        [Test]
        public void MkColSvnWrkActivityIdDirectory()
        {
            Results results = mock.Attach(provider.MakeCollection);
            StubHttpContext context = new StubHttpContext();
            StubHttpRequest request = new StubHttpRequest();
            request.Headers = new NameValueCollection();
            context.Request = request;
            StubHttpResponse response = new StubHttpResponse();
            response.Headers = new HttpResponseHeaderCollection();
            response.OutputStream = new MemoryStream(Constants.BufferSize);
            context.Response = response;
            request.HttpMethod = "mkcol";
            request.Url = new Uri("http://localhost:8081//!svn/wrk/5b34ae67-87de-3741-a590-8bda26893532/Spikes/SvnFacade/trunk/Empty");
            request.Headers.Add("Host", "localhost:8081");

            HttpContextDispatcher dispatcher = new HttpContextDispatcher();
            dispatcher.TfsServerUrl = "http://foo";
            dispatcher.Dispatch(context);

            Assert.AreEqual(1, results.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", (string)results.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/Empty", (string)results.Parameters[1]);
        }
    }
}