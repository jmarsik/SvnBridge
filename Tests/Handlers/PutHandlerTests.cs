using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Attach;
using SvnBridge.Infrastructure;
using System.IO;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PutHandlerTests : HandlerTestsBase
    {
        protected PutHandler handler = new PutHandler();

        [Test]
        public void VerifyPathIsDecodedWhenInvokingSourceControlProviderForFolderPath()
        {
            Results r = stub.Attach(provider.WriteFile, false);
            request.Path = "http://localhost:8082//!svn/wrk/be3dd5c3-e77f-f246-a1e8-640012b047a2/Spikes/SvnFacade/trunk/New%20Folder%207/Empty%20File%202.txt";
            request.Input = "SVN\0";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7/Empty File 2.txt", r.Parameters[1]);
        }

        [Test]
        public void VerifyCorrectOutput()
        {
            Results r = stub.Attach(provider.WriteFile, true);
            request.Path = "http://localhost:8082//!svn/wrk/be3dd5c3-e77f-f246-a1e8-640012b047a2/Spikes/SvnFacade/trunk/New%20Folder%207/Empty%20File%202.txt";
            request.Input = "SVN\0";

            handler.Handle(context, tfsUrl);
            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());

            string expected =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Resource //!svn/wrk/be3dd5c3-e77f-f246-a1e8-640012b047a2/Spikes/SvnFacade/trunk/New Folder 7/Empty File 2.txt has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";
            Assert.AreEqual(expected, result);
            Assert.AreEqual(201, response.StatusCode);
            Assert.AreEqual("text/html", response.ContentType);
            Assert.IsTrue(response.Headers.Contains(new KeyValuePair<string, string>("Location", "http://localhost:8082//!svn/wrk/be3dd5c3-e77f-f246-a1e8-640012b047a2/Spikes/SvnFacade/trunk/New Folder 7/Empty File 2.txt")));
        }
    }
}
