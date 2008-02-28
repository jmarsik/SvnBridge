using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;
using SvnBridge.SourceControl;
using SvnBridge.Stubs;
using Tests;

namespace SvnBridge.Infrastructure
{
    public abstract class HandlerTestsBase
    {
        protected StubHttpContext context;
        protected StubSourceControlProvider provider;
        protected StubHttpRequest request;
        protected StubHttpResponse response;
        protected MyMocks stub = new MyMocks();
        protected string tfsUrl;

        [SetUp]
        public virtual void Setup()
        {
            provider = stub.CreateObject<StubSourceControlProvider>();
            SourceControlProviderFactory.CreateDelegate = delegate { return provider; };
            context = new StubHttpContext();
            request = new StubHttpRequest();
            request.Headers = new NameValueCollection();
            context.Request = request;
            response = new StubHttpResponse();
            response.OutputStream = new MemoryStream(Constants.BufferSize);
            context.Response = response;
            tfsUrl = "http://tfsserver";
        }
    }
}