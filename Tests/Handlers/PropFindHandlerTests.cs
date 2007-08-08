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

            handler.Handle(context, tfsServerUrl);
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

            handler.Handle(context, tfsServerUrl);

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

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("/Test Project", r.Parameters[0]);
        }
    }
}
