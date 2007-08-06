using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Infrastructure;
using NUnit.Framework;
using Attach;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class DeleteHandlerTests : HandlerTestsBase
    {
        protected DeleteHandler handler = new DeleteHandler();

        [Test]
        public void VerifyHandleCorrectlyInvokesSourceControlProviderForDeleteActivity()
        {
            Results r = mock.Attach(provider.DeleteActivity);
            request.Path = "http://localhost:8082/!svn/act/5b34ae67-87de-3741-a590-8bda26893532";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", r.Parameters[0]);
        }

        [Test]
        public void VerifyHandleDecodesPathWhenInvokingSourceControlProviderForDeleteItem()
        {
            mock.Attach(provider.ItemExists, true);
            Results r = mock.Attach(provider.DeleteItem);
            request.Path = "http://localhost:8082//!svn/wrk/125c1a75-a7a6-104d-a661-54689d30dc99/Spikes/SvnFacade/trunk/New%20Folder%206";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 6", r.Parameters[1]);
        }
    }
}
