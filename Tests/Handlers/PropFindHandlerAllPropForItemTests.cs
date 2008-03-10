using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PropFindHandlerAllPropForItemTests : HandlerTestsBase
    {
        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();

            handler = new PropFindHandler();

            item = new ItemMetaData();
            item.Name = "Foo/Bar.txt";
            item.ItemRevision = 1234;
            item.Author = "user_foo";
            item.LastModifiedDate = DateTime.Parse("2007-08-14T23:08:22.908519Z");
            stub.Attach(provider.GetItems, item);
            stub.AttachReadFile(provider.ReadFile, new byte[4] { 0, 1, 2, 3 });
        }

        #endregion

        private PropFindHandler handler;
        private ItemMetaData item;

        private void ArrangeRequest()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo/Bar.txt";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";
        }

        [Test]
        public void TestBaselineRelativePath()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:baseline-relative-path>Foo/Bar.txt</lp2:baseline-relative-path>"));
        }

        [Test]
        public void TestCheckedIn()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp1:checked-in><D:href>/!svn/ver/1234/Foo/Bar.txt</D:href></lp1:checked-in>"));
        }

        [Test]
        public void TestContentType()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getcontenttype>text/plain</lp1:getcontenttype>"));
        }

        [Test]
        public void TestCreationDate()
        {
            DateTime dt = DateTime.Now;
            item.LastModifiedDate = dt;
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp1:creationdate>" + dt.ToUniversalTime().ToString("o") + "</lp1:creationdate>"));
        }

        [Test]
        public void TestCreatorDisplayName()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creator-displayname>user_foo</lp1:creator-displayname>"));
        }

        [Test]
        public void TestDeadDropCount()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:deadprop-count>0</lp2:deadprop-count>"));
        }

        [Test]
        public void TestGetETag()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getetag>W/\"1234//Foo/Bar.txt\"</lp1:getetag>"));
        }

        [Test]
        public void TestGetLastModified()
        {
            DateTime dt = DateTime.Now;
            item.LastModifiedDate = dt;
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp1:getlastmodified>" + dt.ToUniversalTime().ToString("R") + "</lp1:getlastmodified>"));
        }

        [Test]
        public void TestHref()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<D:href>/!svn/bc/1234/Foo/Bar.txt</D:href>"));
        }

        [Test]
        public void TestLockDiscovery()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<D:lockdiscovery/>"));
        }

        [Test]
        public void TestMD5Checksum()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp2:md5-checksum>" + Helper.GetMd5Checksum(new byte[4] {0, 1, 2, 3}) +
                                "</lp2:md5-checksum>"));
        }

        [Test]
        public void TestRepositoryUuid()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>"));
        }

        [Test]
        public void TestResourceType()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:resourcetype/>"));
        }

        [Test]
        public void TestSupportedLock()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains(
                              "<D:supportedlock>\n" +
                              "<D:lockentry>\n" +
                              "<D:lockscope><D:exclusive/></D:lockscope>\n" +
                              "<D:locktype><D:write/></D:locktype>\n" +
                              "</D:lockentry>\n" +
                              "</D:supportedlock>\n"));
        }

        [Test]
        public void TestVersionControlledConfiguration()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains(
                    "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>"));
        }

        [Test]
        public void TestVersionName()
        {
            ArrangeRequest();

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:version-name>1234</lp1:version-name>"));
        }
    }
}