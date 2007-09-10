using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using Attach;
using System.IO;
using Tests;
using SvnBridge.Exceptions;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class MergeHandlerTests : HandlerTestsBase
    {
        protected MergeHandler handler = new MergeHandler();

        [Test]
        public void VerifyHandleProducesCorrectOutputForConflict()
        {
            mock.Attach((MyMocks.MergeActivity)provider.MergeActivity, new ConflictException("Conflict at '/Test.txt'"));
            request.Path = "http://localhost:8082/";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:merge xmlns:D=\"DAV:\"><D:source><D:href>/!svn/act/61652fe8-44cd-8d43-810f-c95deccc6db3</D:href></D:source><D:no-auto-merge/><D:no-checkout/><D:prop><D:checked-in/><D:version-name/><D:resourcetype/><D:creationdate/><D:creator-displayname/></D:prop></D:merge>";

            handler.Handle(context, tfsUrl);

            string expected =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:error xmlns:D=\"DAV:\" xmlns:m=\"http://apache.org/dav/xmlns\" xmlns:C=\"svn:\">\n" +
                "<C:error/>\n" +
                "<m:human-readable errcode=\"160024\">\n" +
                "Conflict at '/Test.txt'\n" +
                "</m:human-readable>\n" +
                "</D:error>\n";
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray()));
            Assert.AreEqual(409, response.StatusCode);
            Assert.AreEqual("text/xml; charset=\"utf-8\"", response.ContentType);
            Assert.IsTrue(response.Headers.Contains(new KeyValuePair<string, string>("Cache-Control", "no-cache")));
        }
    }
}
