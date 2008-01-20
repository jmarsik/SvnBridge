using System;
using System.Collections.Generic;
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
        public void TestHandleProducesCorrectOutput()
        {
            Results r = stub.Attach(provider.CopyItem);
            request.Path = "http://localhost:8082/!svn/bc/5522/File.txt";
            request.Headers["Destination"] = "http://localhost:8082//!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt";

            handler.Handle(context, tfsUrl);

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
            Assert.IsTrue(response.Headers.Contains(new KeyValuePair<string, string>("Location", "http://localhost:8082//!svn/wrk/cdfcf93f-8649-5e44-a8ec-b3f40e10e907/FileRenamed.txt")));
            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("cdfcf93f-8649-5e44-a8ec-b3f40e10e907", r.Parameters[0]);
            Assert.AreEqual("/File.txt", r.Parameters[1]);
            Assert.AreEqual("/FileRenamed.txt", r.Parameters[2]);
        }

        [Test]
        public void TestLocationResponseHeaderIsDecoded()
        {
            Results r = stub.Attach(provider.CopyItem);
            request.Path = "http://localhost:8082/!svn/bc/5730/B%20!@%23$%25%5E&()_-+=%7B%5B%7D%5D%3B',.~%60";
            request.Headers["Destination"] = "http://localhost:8084//!svn/wrk/15407bc3-2250-aa4c-aa51-4e65b2c824c3/BB%20!@%23$%25%5E&()_-+=%7B%5B%7D%5D%3B',.~%60";

            handler.Handle(context, tfsUrl);

            Assert.IsTrue(response.Headers.Contains(new KeyValuePair<string, string>("Location", "http://localhost:8084//!svn/wrk/15407bc3-2250-aa4c-aa51-4e65b2c824c3/BB !@#$%^&()_-+={[}];',.~`")));
        }

        [Test]
        public void TestDestinationInResponseMessageIsDecodedAndEncoded()
        {
            Results r = stub.Attach(provider.CopyItem);
            request.Path = "http://localhost:8082/!svn/bc/5730/B%20!@%23$%25%5E&()_-+=%7B%5B%7D%5D%3B',.~%60";
            request.Headers["Destination"] = "http://localhost:8084//!svn/wrk/15407bc3-2250-aa4c-aa51-4e65b2c824c3/BB%20!@%23$%25%5E&()_-+=%7B%5B%7D%5D%3B',.~%60";

            handler.Handle(context, tfsUrl);
            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());

            Assert.IsTrue(result.Contains("<p>Destination //!svn/wrk/15407bc3-2250-aa4c-aa51-4e65b2c824c3/BB !@#$%^&amp;()_-+={[}];',.~` has been created.</p>"));
        }

        [Test]
        public void TestSourceControlProviderCalledCorrectlyWithSpecialCharactersInPath()
        {
            Results r = stub.Attach(provider.CopyItem);
            request.Path = "http://localhost:8082/!svn/bc/5730/B%20!@%23$%25%5E&()_-+=%7B%5B%7D%5D%3B',.~%60";
            request.Headers["Destination"] = "http://localhost:8084//!svn/wrk/15407bc3-2250-aa4c-aa51-4e65b2c824c3/BB%20!@%23$%25%5E&()_-+=%7B%5B%7D%5D%3B',.~%60";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("15407bc3-2250-aa4c-aa51-4e65b2c824c3", r.Parameters[0]);
            Assert.AreEqual("/B !@#$%^&()_-+={[}];',.~`", r.Parameters[1]);
            Assert.AreEqual("/BB !@#$%^&()_-+={[}];',.~`", r.Parameters[2]);
        }
    }
}