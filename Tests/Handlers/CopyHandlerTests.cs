using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Attach;
using NUnit.Framework;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Stubs;
using Tests;
using SvnBridge.Infrastructure;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class CopyHandlerTests : HandlerTestsBase
    {
        protected CopyHandler handler = new CopyHandler();

        [Test]
        public void VerifyHandleProducesCorrectOutput()
        {
            Results r = mock.Attach(provider.CopyItem);
            request.Path = "http://localhost:8082/!svn/bc/5522/File.txt";
            request.Headers["Host"] = "localhost:8082";
            request.Headers["Destination"] = "http://localhost:8082//!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt";

            handler.Handle(context, tfsServerUrl);

            string expected =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Destination //!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray()));
            Assert.AreEqual("text/html", response.ContentType);
            Assert.AreEqual("http://localhost:8082//!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt", response.Headers["Location"]);
            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("cdfcf93f-8649-5e44-a8ec-b3f40e10e907", r.Parameters[0]);
            Assert.AreEqual("/File.txt", r.Parameters[1]);
            Assert.AreEqual("/FileRenamed.txt", r.Parameters[2]);
        }
    }
}