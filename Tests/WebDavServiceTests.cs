using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Attach;
using NUnit.Framework;
using SvnBridge;
using SvnBridge.Handlers;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Stubs;

namespace Tests
{
    [TestFixture]
    public class WebDavServiceTests
    {
        MyMocks mock;
        StubSourceControlProvider sourceControlProvider;

        public WebDavServiceTests()
        {
            mock = new MyMocks();
            sourceControlProvider = mock.CreateObject<StubSourceControlProvider>();
        }

        [SetUp]
        public void SetUp()
        {
            SourceControlProviderFactory.CreateDelegate = delegate { return sourceControlProvider; };
        }

        [TearDown]
        public void TearDown()
        {
            SourceControlProviderFactory.CreateDelegate = null;
        }

        [Test]
        public void VerifyMkColDecodesPathWhenCallingSourceControlProvider()
        {
            Results results = mock.Attach(sourceControlProvider.MakeCollection);
            StubHttpContext context = new StubHttpContext();
            StubHttpRequest request = new StubHttpRequest();
            request.Headers = new NameValueCollection();
            context.Request = request;
            StubHttpResponse response = new StubHttpResponse();
            response.OutputStream = new MemoryStream(Constants.BufferSize);
            context.Response = response;
            request.HttpMethod = "mkcol";
            request.Url = new Uri("htt://localhost:8081//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Folder%20With%20Spaces");
            request.Headers.Add("Host", "localhost:8081");
            HttpContextDispatcher dispatcher = new HttpContextDispatcher();
            dispatcher.TfsServerUrl = "http://foo";

            dispatcher.Dispatch(context);

            Assert.AreEqual("/Folder With Spaces", results.Parameters[1]);
        }

        [Test]
        public void VerifyPutDecodesPathWhenCallingSourceControlProvider()
        {
            MyMocks mock = new MyMocks();
            StubSourceControlProvider provider = mock.CreateObject<StubSourceControlProvider>();
            WebDavService service = new WebDavService(provider);
            Results result = mock.Attach(provider.WriteFile, false);
            byte[] data = Encoding.Default.GetBytes("SVN\0");
            MemoryStream stream = new MemoryStream(data);

            service.Put("//!svn/wrk/be3dd5c3-e77f-f246-a1e8-640012b047a2/Spikes/SvnFacade/trunk/New%20Folder%207/Empty%20File%202.txt", stream, null, null);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7/Empty File 2.txt", result.Parameters[1]);
        }

        [Test]
        public void VerifyPropFindDecodesPathWhenCallingSourceControlProvider()
        {
            MyMocks mock = new MyMocks();
            StubSourceControlProvider provider = mock.CreateObject<StubSourceControlProvider>();
            WebDavService service = new WebDavService(provider);
            Results result = mock.Attach(provider.ItemExists, true);
            mock.Attach(provider.IsDirectory, true);
            MemoryStream outputStream = new MemoryStream();
            string propfind = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/></prop></propfind>";
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(propfind));
            XmlSerializer serializer = new XmlSerializer(typeof(PropFindData));
            PropFindData propfinddata = (PropFindData)serializer.Deserialize(stream);

            service.PropFind(propfinddata, "/Spikes/SvnFacade/trunk/New%20Folder%207", "0", null, outputStream);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7", result.Parameters[0]);
        }

        [Test]
        public void VerifyPropFindDecodesSvnBcPathWhenCallingSourceControlProvider()
        {
            MyMocks mock = new MyMocks();
            StubSourceControlProvider provider = mock.CreateObject<StubSourceControlProvider>();
            WebDavService service = new WebDavService(provider);
            Results result = mock.Attach(provider.ItemExists, true);
            mock.Attach(provider.IsDirectory, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Test Project";
            mock.Attach(provider.GetItems, item);
            MemoryStream outputStream = new MemoryStream();
            string propfind = "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";
            MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(propfind));
            XmlSerializer serializer = new XmlSerializer(typeof(PropFindData));
            PropFindData propfinddata = (PropFindData)serializer.Deserialize(stream);

            service.PropFind(propfinddata, "/!svn/bc/3444/Test%20Project", "0", null, outputStream);

            Assert.AreEqual("/Test Project", result.Parameters[0]);
        }
    }
}