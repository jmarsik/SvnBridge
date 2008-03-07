using System;
using System.IO;
using System.Text;
using Attach;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.Nodes;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PropFindHandlerTests : HandlerTestsBase
    {
        protected PropFindHandler handler = new PropFindHandler();

        [Test]
        public void TestBcFileNodeHrefForFolder()
        {
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo/Bar.txt";
            stub.Attach(provider.GetItems, item);

            BcFileNode node = new BcFileNode(1234, item, provider);

            string expected = "/!svn/bc/1234/Foo/Bar.txt";
            Assert.AreEqual(expected, node.Href(handler));
        }

        [Test]
        public void TestBcMd5Checksum()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo/Bar.txt";
            stub.Attach(provider.GetItems, item);
            stub.Attach(provider.ReadFile, new byte[4] {0, 1, 2, 3});

            request.Path = "http://localhost:8082/!svn/bc/1234/Foo/Bar.txt";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:propfind xmlns:D='DAV:'><D:prop><D:md5-checksum/></D:prop></D:propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp2:md5-checksum>" + Helper.GetMd5Checksum(new byte[4] {0, 1, 2, 3}) +
                                "</lp2:md5-checksum>"));
        }

        [Test]
        public void TestCorrectlyInvokesProviderWithBcPathAndDepthOne()
        {
            stub.Attach(provider.ItemExists, true);
            stub.Attach(provider.IsDirectory, true);
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "Foo";
            Results results = stub.Attach(provider.GetItems, folder);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><resourcetype xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "1";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual(1, results.CallCount);
            Assert.AreEqual(1234, results.Parameters[0]);
            Assert.AreEqual("/Foo", results.Parameters[1]);
            Assert.AreEqual(Recursion.OneLevel, results.Parameters[2]);
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void TestInvalidDepthThrowsEx()
        {
            stub.Attach(provider.ItemExists, true);
            request.Path = "http://localhost:8082/Folder%20With%20Spaces";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "2";

            handler.Handle(context, tfsUrl);
        }

        [Test]
        public void TestPropFindLockDiscovery()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo";
            stub.Attach(provider.GetItems, item);

            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:propfind xmlns:D='DAV:'><D:prop><D:lockdiscovery/></D:prop></D:propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<D:lockdiscovery/>"));
        }

        [Test]
        public void TestPropFindWithDepthOneIncludesFolderAndChildren()
        {
            stub.Attach(provider.ItemExists, true);
            stub.Attach(provider.IsDirectory, Return.MultipleValues(true, false));
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "Foo";
            stub.Attach(provider.GetItems, folder);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo/Bar.txt";
            folder.Items.Add(item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><resourcetype xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "1";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<D:href>/!svn/bc/1234/Foo/</D:href>"));
            Assert.IsTrue(result.Contains("<D:href>/!svn/bc/1234/Foo/Bar.txt</D:href>"));
        }

        [Test]
        public void VerifyBaselineRelativePathPropertyForFolderReturnsDecoded()
        {
            stub.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Folder With Spaces";
            stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/Folder%20With%20Spaces";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);
            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());

            string expected = "<lp2:baseline-relative-path>Folder With Spaces</lp2:baseline-relative-path>";
            Assert.IsTrue(result.Contains(expected));
        }

        [Test]
        public void VerifyCreationDate()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo";
            item.ItemRevision = 1234;
            item.Author = "user_foo";
            DateTime dt = DateTime.Now.ToUniversalTime();
            item.LastModifiedDate = dt;
            stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><creationdate xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(
                result.Contains("<lp1:creationdate>" + Helper.FormatDate(dt.ToUniversalTime()) + "</lp1:creationdate>"));
        }

        [Test]
        public void VerifyCreatorDisplayName()
        {
            stub.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.ItemRevision = 1234;
            item.Author = "user_foo";
            item.LastModifiedDate = DateTime.Now.ToUniversalTime();
            stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><creator-displayname xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creator-displayname>user_foo</lp1:creator-displayname>"));
        }

        [Test]
        public void VerifyDeadPropCountReturnsZero()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo";
            stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><deadprop-count xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:deadprop-count>0</lp2:deadprop-count>"));
        }

        [Test]
        public void VerifyGetContentLengthForFolder()
        {
            stub.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.ItemRevision = 1234;
            item.Author = "user_foo";
            DateTime dt = DateTime.Now.ToUniversalTime();
            item.LastModifiedDate = dt;
            stub.Attach(provider.GetItems, item);
            stub.Attach(provider.ReadFile, new byte[4] {0, 1, 2, 3});
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><getcontentlength xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<g0:getcontentlength/>"));
        }

        [Test]
        public void VerifyPathIsDecodedWhenInvokingSourceControlProviderForFolderPath()
        {
            Results r1 = stub.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "New%20Folder%207";
            Results r2 = stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/Spikes/SvnFacade/trunk/New%20Folder%207";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7", r1.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7", r2.Parameters[1]);
        }

        [Test]
        public void VerifyPathIsDecodedWhenInvokingSourceControlProviderForSvnBcFolderPath()
        {
            Results r = stub.Attach(provider.ItemExists, true);
            stub.Attach(provider.IsDirectory, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Test Project";
            stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/3444/Test%20Project";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Test Project", r.Parameters[0]);
        }

        [Test]
        public void VerifyVersionName()
        {
            stub.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.ItemRevision = 1234;
            item.Author = "user_foo";
            DateTime dt = DateTime.Now.ToUniversalTime();
            item.LastModifiedDate = dt;
            stub.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-name xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:version-name>1234</lp1:version-name>"));
        }
    }

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
            stub.Attach(provider.ReadFile, new byte[4] {0, 1, 2, 3});
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