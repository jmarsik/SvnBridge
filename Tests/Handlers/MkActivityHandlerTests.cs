using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Infrastructure;
using NUnit.Framework;
using Attach;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class MkActivityHandlerTests : HandlerTestsBase
    {
        protected MkActivityHandler handler = new MkActivityHandler();

        [Test]
        public void VerifyHandleCorrectlyCallsSourceControlService()
        {
            Results r = stub.Attach(provider.MakeActivity);
            request.Path = "http://localhost:8080/!svn/act/c512ecbe-7577-ce46-939c-a9e81eb4d98e";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, r.CallCount);
            Assert.AreEqual("c512ecbe-7577-ce46-939c-a9e81eb4d98e", r.Parameters[0]);
        }
    }
}
