using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Infrastructure;
using NUnit.Framework;
using Attach;
using System.IO;
using SvnBridge.Exceptions;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class MkColHandlerTests : HandlerTestsBase
    {
        protected MkColHandler handler = new MkColHandler();

        [Test]
        public void VerifyHandleCorrectlyInvokesSourceControlProvider()
        {
            Results r = stub.Attach(provider.MakeCollection);
            request.Path = "http://localhost:8081//!svn/wrk/5b34ae67-87de-3741-a590-8bda26893532/Spikes/SvnFacade/trunk/Empty";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", r.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/Empty", r.Parameters[1]);
        }

        [Test]
        public void VerifyPathIsDecodedWhenCallingSourceControlProvider()
        {
            Results r = stub.Attach(provider.MakeCollection);
            request.Path = "http://localhost:8081//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Folder%20With%20Spaces";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Folder With Spaces", r.Parameters[1]);
        }

        [Test]
        public void VerifyCorrectOutputForSuccessfulCreate()
        {
            Results r = stub.Attach(provider.MakeCollection);
            request.Path = "http://localhost:8082//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Spikes/SvnFacade/trunk/New%20Folder%206";

            handler.Handle(context, tfsUrl);
            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());

            string expected =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Collection //!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Spikes/SvnFacade/trunk/New Folder 6 has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";
            Assert.AreEqual(expected, result);
            Assert.AreEqual(201, response.StatusCode);
            Assert.AreEqual("text/html", response.ContentType);
            Assert.IsTrue(response.Headers.Contains(new KeyValuePair<string, string>("Location", "http://localhost:8082//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Spikes/SvnFacade/trunk/New Folder 6")));
        }

        [Test]
        public void VerifyCorrectOutputWhenFolderAlreadyExists()
        {
            stub.Attach(provider.MakeCollection, new FolderAlreadyExistsException());
            request.Path = "http://localhost:8082//!svn/wrk/de1ec288-d55c-6146-950d-ceaf2ce9403b/newdir";

            handler.Handle(context, tfsUrl);
            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());

            string expected =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>405 Method Not Allowed</title>\n" +
                "</head><body>\n" +
                "<h1>Method Not Allowed</h1>\n" +
                "<p>The requested method MKCOL is not allowed for the URL //!svn/wrk/de1ec288-d55c-6146-950d-ceaf2ce9403b/newdir.</p>\n" +
                "<hr>\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";
            Assert.AreEqual(expected, result);
            Assert.AreEqual(405, response.StatusCode);
            Assert.AreEqual("text/html; charset=iso-8859-1", response.ContentType);
            Assert.IsTrue(response.Headers.Contains(new KeyValuePair<string, string>("Allow", "TRACE")));
        }
    }
}
