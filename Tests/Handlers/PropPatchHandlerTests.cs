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
        public void VerifyHandleCorrectlyCallsSourceControlProviderForCheckinComment()
        {
            Results r = mock.Attach(provider.SetActivityComment);
            request.Path = "http://localhost:8082//!svn/wbl/c512ecbe-7577-ce46-939c-a9e81eb4d98e/5465";
            request.Input = "<D:propertyupdate xmlns:D=\"DAV:\"><D:set><D:prop><log xmlns=\"http://subversion.tigris.org/xmlns/svn/\">Test comment</log></D:prop></D:set>\n</D:propertyupdate>\n";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("c512ecbe-7577-ce46-939c-a9e81eb4d98e", r.Parameters[0]);
            Assert.AreEqual("Test comment", r.Parameters[1]);
        }

        [Test]
        public void VerifyHandleCorrectlyCallsSourceControlProviderForSetProperty()
        {
            Results r = mock.Attach(provider.SetProperty);
            request.Path = "http://localhost:8082//!svn/wrk/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/Folder1";
            request.Input = "<D:propertyupdate xmlns:D=\"DAV:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:C=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:S=\"http://subversion.tigris.org/xmlns/svn/\"><D:set><D:prop><S:ignore>*.bad\n</S:ignore></D:prop></D:set></D:propertyupdate>";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("be05cf36-7514-3f4d-81ea-7822f7b1dfe7", r.Parameters[0]);
            Assert.AreEqual("/Folder1", r.Parameters[1]);
            Assert.AreEqual("ignore", r.Parameters[2]);
            Assert.AreEqual("*.bad\n", r.Parameters[3]);
        }

        [Test]
        public void VerifyHandleCallsSourceControlProviderWithCorrectActivityIdWhenPathHasOneSlashAfterHostname()
        {
            Results r = mock.Attach(provider.SetActivityComment);
            request.Path = "http://localhost:8082/!svn/wbl/c512ecbe-7577-ce46-939c-a9e81eb4d98e/5465";
            request.Input = "<D:propertyupdate xmlns:D=\"DAV:\"><D:set><D:prop><log xmlns=\"http://subversion.tigris.org/xmlns/svn/\">Test comment</log></D:prop></D:set>\n</D:propertyupdate>\n";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("c512ecbe-7577-ce46-939c-a9e81eb4d98e", r.Parameters[0]);
        }
    }
}
