using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Tests;
using System.IO;
using SvnBridge.SourceControl;
using System.Net;
using Attach;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class CopyHandlerTests
    {
        protected MyMocks mock = new MyMocks();
        protected StubSourceControlProvider provider;
        protected MockContext context;
        protected CopyHandler handler;

        [SetUp]
        public virtual void Setup()
        {
            provider = mock.CreateObject<StubSourceControlProvider>();
            SourceControlProviderFactory.CreateDelegate = delegate { return provider; };
            context = new MockContext();
            handler = new CopyHandler();
        }

        [Test]
        public void VerifyHandleProducesCorrectOutput()
        {
            Results r = mock.Attach(provider.CopyItem);
            context.Path = "/!svn/bc/5522/File.txt";
            context.Headers["Host"] = "localhost:8082";
            context.Headers["Destination"] = "http://localhost:8082//!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt";

            handler.Handle(context, "http://tfsserver");

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
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)context.OutputStream).ToArray()));
            Assert.AreEqual("text/html", context.ContentType);
            Assert.AreEqual("http://localhost:8082//!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt", context.ResponseHeaders["Location"]);
            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("cdfcf93f-8649-5e44-a8ec-b3f40e10e907", r.Parameters[0]);
            Assert.AreEqual("/File.txt", r.Parameters[1]);
            Assert.AreEqual("/FileRenamed.txt", r.Parameters[2]);
        }
    }
}
