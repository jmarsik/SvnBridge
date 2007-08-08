using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Attach;
using NUnit.Framework;
using SvnBridge;
using SvnBridge.Handlers;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Stubs;

namespace Tests
{
    [TestFixture]
    public class WebDavServiceTests
    {
        MyMocks mock;
        StubSourceControlProvider sourceControlProvider;

        public WebDavServiceTests()
        {
            mock = new MyMocks();
            sourceControlProvider = mock.CreateObject<StubSourceControlProvider>();
        }

        [SetUp]
        public void SetUp()
        {
            SourceControlProviderFactory.CreateDelegate = delegate { return sourceControlProvider; };
        }

        [TearDown]
        public void TearDown()
        {
            SourceControlProviderFactory.CreateDelegate = null;
        }

        [Test]
        public void VerifyMkColDecodesPathWhenCallingSourceControlProvider()
        {
            Results results = mock.Attach(sourceControlProvider.MakeCollection);
            StubHttpContext context = new StubHttpContext();
            StubHttpRequest request = new StubHttpRequest();
            request.Headers = new NameValueCollection();
            context.Request = request;
            StubHttpResponse response = new StubHttpResponse();
            response.OutputStream = new MemoryStream(Constants.BufferSize);
            context.Response = response;
            request.HttpMethod = "mkcol";
            request.Url = new Uri("htt://localhost:8081//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Folder%20With%20Spaces");
            request.Headers.Add("Host", "localhost:8081");
            HttpContextDispatcher dispatcher = new HttpContextDispatcher();
            dispatcher.TfsServerUrl = "http://foo";

            dispatcher.Dispatch(context);

            Assert.AreEqual("/Folder With Spaces", results.Parameters[1]);
        }
    }
}