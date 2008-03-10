using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PropFindHandlerAllPropForFolderTests : HandlerTestsBase
    {
        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();

            handler = new PropFindHandler();

            item = new FolderMetaData();
            item.Name = "Foo";
            item.ItemRevision = 1234;
            item.Author = "user_foo";
            item.LastModifiedDate = DateTime.Now.ToUniversalTime();
            stub.Attach(provider.GetItems, item);
        }

        #endregion

        private PropFindHandler handler;
        private FolderMetaData item;

        [Test]
        public void TestAllPropBaselineRelativePath()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:baseline-relative-path>Foo</lp2:baseline-relative-path>"));
        }

        [Test]
        public void TestAllPropCheckedIn()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:checked-in><D:href>/!svn/ver/1234/Foo</D:href></lp1:checked-in>"));
        }

        [Test]
        public void TestAllPropContentType()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getcontenttype>text/html; charset=UTF-8</lp1:getcontenttype>"));
        }

        [Test]
        public void TestAllPropCreationDate()
        {
            DateTime dt = DateTime.Now;
            item.LastModifiedDate = dt;
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp1:creationdate>" + dt.ToUniversalTime().ToString("o") + "</lp1:creationdate>"));
        }

        [Test]
        public void TestAllPropCreatorDisplayName()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creator-displayname>user_foo</lp1:creator-displayname>"));
        }

        [Test]
        public void TestAllPropDeadDropCount()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:deadprop-count>0</lp2:deadprop-count>"));
        }

        [Test]
        public void TestAllPropGetETag()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getetag>W/\"1234//Foo\"</lp1:getetag>"));
        }

        [Test]
        public void TestAllPropGetLastModified()
        {
            DateTime dt = DateTime.Now;
            item.LastModifiedDate = dt;
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp1:getlastmodified>" + dt.ToUniversalTime().ToString("R") + "</lp1:getlastmodified>"));
        }

        [Test]
        public void TestAllPropRepositoryUuid()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>"));
        }

        [Test]
        public void TestAllPropResourceType()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:resourcetype><D:collection/></lp1:resourcetype>"));
        }

        [Test]
        public void TestAllPropVersionControlledConfiguration()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains(
                    "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>"));
        }

        [Test]
        public void TestAllPropVersionName()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:version-name>1234</lp1:version-name>"));
        }
    }
}