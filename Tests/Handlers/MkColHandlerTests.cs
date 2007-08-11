using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Infrastructure;
using NUnit.Framework;
using Attach;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class MkColHandlerTests : HandlerTestsBase
    {
        protected MkColHandler handler = new MkColHandler();

        [Test]
        public void VerifyHandleCorrectlyInvokesSourceControlProvider()
        {
            Results r = mock.Attach(provider.MakeCollection);
            request.Path = "http://localhost:8081//!svn/wrk/5b34ae67-87de-3741-a590-8bda26893532/Spikes/SvnFacade/trunk/Empty";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", r.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/Empty", r.Parameters[1]);
        }

        [Test]
        public void VerifyPathIsDecodedWhenCallingSourceControlProvider()
        {
            Results r = mock.Attach(provider.MakeCollection);
            request.Path = "http://localhost:8081//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Folder%20With%20Spaces";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Folder With Spaces", r.Parameters[1]);
        }
    }
}
