using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Tests;
using SvnBridge.Stubs;
using SvnBridge.SourceControl;
using System.Collections.Specialized;
using SvnBridge.Net;
using System.IO;

namespace SvnBridge.Infrastructure
{
    public abstract class HandlerTestsBase
    {
        protected MyMocks stub = new MyMocks();
        protected StubSourceControlProvider provider;
        protected StubHttpContext context;
        protected StubHttpRequest request;
        protected StubHttpResponse response;
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
