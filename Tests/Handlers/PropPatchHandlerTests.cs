using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using System.IO;
using Attach;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PropPatchHandlerTests : HandlerTestsBase
    {
        protected PropPatchHandler handler = new PropPatchHandler();

        [Test]
        public void VerifyHandleCorrectlyCallsSourceControlServiceForCommitComment()
        {
            Results r = mock.Attach(provider.SetActivityComment);
            request.Path = "http://localhost:8082//!svn/wbl/c512ecbe-7577-ce46-939c-a9e81eb4d98e/5465";
            request.Input = "<D:propertyupdate xmlns:D=\"DAV:\"><D:set><D:prop><log xmlns=\"http://subversion.tigris.org/xmlns/svn/\">Test comment</log></D:prop></D:set>\n</D:propertyupdate>\n";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("c512ecbe-7577-ce46-939c-a9e81eb4d98e", r.Parameters[0]);
            Assert.AreEqual("Test comment", r.Parameters[1]);
        }

        [Test]
        public void VerifyHandleCallsSourceControlServiceWithCorrectActivityIdWhenPathHasOneSlashAfterHostname()
        {
            Results r = mock.Attach(provider.SetActivityComment);
            request.Path = "http://localhost:8082/!svn/wbl/c512ecbe-7577-ce46-939c-a9e81eb4d98e/5465";
            request.Input = "<D:propertyupdate xmlns:D=\"DAV:\"><D:set><D:prop><log xmlns=\"http://subversion.tigris.org/xmlns/svn/\">Test comment</log></D:prop></D:set>\n</D:propertyupdate>\n";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("c512ecbe-7577-ce46-939c-a9e81eb4d98e", r.Parameters[0]);
        }
    }
}
