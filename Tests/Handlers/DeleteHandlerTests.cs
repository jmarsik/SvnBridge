using System.IO;
using System.Text;
using Attach;
using NUnit.Framework;
using SvnBridge.Infrastructure;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class DeleteHandlerTests : HandlerTestsBase
    {
        protected DeleteHandler handler = new DeleteHandler();

        [Test]
        public void VerifyHandleCorrectlyInvokesSourceControlProviderForDeleteActivity()
        {
            Results r = stub.Attach(provider.DeleteActivity);
            request.Path = "http://localhost:8082/!svn/act/5b34ae67-87de-3741-a590-8bda26893532";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, r.CallCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", r.Parameters[0]);
        }

        [Test]
        public void VerifyHandleCorrectlyInvokesSourceControlProviderForDeleteFile()
        {
            Results r = stub.Attach(provider.DeleteItem, true);
            request.Path =
                "http://localhost:8082//!svn/wrk/c512ecbe-7577-ce46-939c-a9e81eb4d98e/Spikes/SvnFacade/trunk/Test4.txt";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, r.CallCount);
            Assert.AreEqual("c512ecbe-7577-ce46-939c-a9e81eb4d98e", r.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/Test4.txt", r.Parameters[1]);
        }

        [Test]
        public void VerifyHandleDecodesPathWhenInvokingSourceControlProviderForDeleteItem()
        {
            Results r = stub.Attach(provider.DeleteItem, true);
            request.Path =
                "http://localhost:8082//!svn/wrk/125c1a75-a7a6-104d-a661-54689d30dc99/Spikes/SvnFacade/trunk/New%20Folder%206";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 6", r.Parameters[1]);
        }

        [Test]
        public void VerifyHandleReturnsCorrectResponseWhenDeleteFileNotFound()
        {
            Results r = stub.Attach(provider.DeleteItem, false);
            request.Path =
                "http://localhost:8082//!svn/wrk/70df3104-9f67-8d4e-add7-6012fe86c03a/Spikes/SvnFacade/trunk/New%20Folder/Test2.txt";

            handler.Handle(context, tfsUrl);

            string expected =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>404 Not Found</title>\n" +
                "</head><body>\n" +
                "<h1>Not Found</h1>\n" +
                "<p>The requested URL //!svn/wrk/70df3104-9f67-8d4e-add7-6012fe86c03a/Spikes/SvnFacade/trunk/New Folder/Test2.txt was not found on this server.</p>\n" +
                "<hr>\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray()));
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("text/html; charset=iso-8859-1", response.ContentType);
        }
    }
}