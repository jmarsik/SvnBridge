using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Attach;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;
using System.IO;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PropFindHandlerTests : HandlerTestsBase
    {
        protected PropFindHandler handler = new PropFindHandler();

        [Test]
        public void VerifyBaselineRelativePathPropertyForFolderReturnsDecoded()
        {
            mock.Attach(provider.ItemExists, true);
            mock.Attach(provider.IsDirectory, true);
            mock.Attach(provider.GetLatestVersion, 1234);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Folder With Spaces";
            mock.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/Folder%20With%20Spaces";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);
            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());

            string expected = "<lp2:baseline-relative-path>Folder With Spaces</lp2:baseline-relative-path>";
            Assert.IsTrue(result.Contains(expected));
        }

        [Test]
        public void VerifyPathIsDecodedWhenInvokingSourceControlProviderForFolderPath()
        {
            Results r = mock.Attach(provider.ItemExists, true);
            mock.Attach(provider.IsDirectory, true);
            request.Path = "http://localhost:8082/Spikes/SvnFacade/trunk/New%20Folder%207";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7", r.Parameters[0]);
        }

        [Test]
        public void VerifyPathIsDecodedWhenInvokingSourceControlProviderForSvnBcFolderPath()
        {
            Results r = mock.Attach(provider.ItemExists, true);
            mock.Attach(provider.IsDirectory, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Test Project";
            mock.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/3444/Test%20Project";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Test Project", r.Parameters[0]);
        }

        [Test]
        public void VerifyDeadPropCountReturnsZero()
        {
            mock.Attach(provider.ItemExists, true);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><deadprop-count xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:deadprop-count>0</lp2:deadprop-count>"));            
        }

        [Test]
        public void VerifyCreatorDisplayName()
        {
            mock.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.Revision = 1234;
            item.Author = "user_foo";
            item.LastModifiedDate = DateTime.Now.ToUniversalTime();
            mock.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><creator-displayname xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creator-displayname>user_foo</lp1:creator-displayname>"));
        }

        [Test]
        public void VerifyCreationDate()
        {
            mock.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.Revision = 1234;
            item.Author = "user_foo";
            DateTime dt = DateTime.Now.ToUniversalTime();
            item.LastModifiedDate = dt;
            mock.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><creationdate xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creationdate>" + dt.ToString("o") + "</lp1:creationdate>"));
        }

        [Test]
        public void VerifyVersionName()
        {
            mock.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.Revision = 1234;
            item.Author = "user_foo";
            DateTime dt = DateTime.Now.ToUniversalTime();
            item.LastModifiedDate = dt;
            mock.Attach(provider.GetItems, item);
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-name xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:version-name>1234</lp1:version-name>"));
        }

        [Test]
        public void VerifyGetContentLengthForFolder()
        {
            mock.Attach(provider.ItemExists, true);
            FolderMetaData item = new FolderMetaData();
            item.Name = "Foo";
            item.Revision = 1234;
            item.Author = "user_foo";
            DateTime dt = DateTime.Now.ToUniversalTime();
            item.LastModifiedDate = dt;
            mock.Attach(provider.GetItems, item);
            mock.Attach(provider.ReadFile, new byte[4] {0, 1, 2, 3});
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><getcontentlength xmlns=\"DAV:\"/></prop></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getcontentlength>4</lp1:getcontentlength>"));
        }
    }

    [TestFixture]
    public class PropFindHandlerAllPropTests : HandlerTestsBase
    {
        private PropFindHandler handler;
        private FolderMetaData item;

        public override void Setup()
        {
            base.Setup();
            
            handler = new PropFindHandler();

            item = new FolderMetaData();
            item.Name = "Foo";
            item.Revision = 1234;
            item.Author = "user_foo";
            item.LastModifiedDate = DateTime.Now.ToUniversalTime();
            mock.Attach(provider.GetItems, item);
        }

        [Test]
        public void TestAllPropContentType()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getcontenttype>text/html; charset=UTF-8</lp1:getcontenttype>"));            
        }

        [Test]
        public void TestAllPropGetETag()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getetag>W/\"1234//Foo\"</lp1:getetag>"));
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

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creationdate>" + dt.ToUniversalTime().ToString("o") + "</lp1:creationdate>"));
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

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:getlastmodified>" + dt.ToUniversalTime().ToString("R") + "</lp1:getlastmodified>"));
        }

        [Test]
        public void TestAllPropCheckedIn()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:checked-in><D:href>/!svn/ver/1234/Foo</D:href></lp1:checked-in>"));
        }

        [Test]
        public void TestAllPropVersionControlledConfiguration()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>"));
        }

        [Test]
        public void TestAllPropVersionName()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:version-name>1234</lp1:version-name>"));
        }

        [Test]
        public void TestAllPropCreatorDisplayName()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:creator-displayname>user_foo</lp1:creator-displayname>"));
        }

        [Test]
        public void TestAllPropBaselineRelativePath()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:baseline-relative-path>Foo</lp2:baseline-relative-path>"));
        }

        [Test]
        public void TestAllPropRepositoryUuid()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>"));
        }

        [Test]
        public void TestAllPropDeadDropCount()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp2:deadprop-count>0</lp2:deadprop-count>"));
        }

        [Test]
        public void TestAllPropResourceType()
        {
            request.Path = "http://localhost/!svn/bc/1234/Foo";
            request.Input = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><allprop/></propfind>";
            request.Headers["Depth"] = "0";

            handler.Handle(context, tfsUrl);

            string result = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(result.Contains("<lp1:resourcetype><D:collection/></lp1:resourcetype>"));
        }
    }
}
