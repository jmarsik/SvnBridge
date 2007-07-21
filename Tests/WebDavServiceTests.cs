using System.IO;
using System.Text;
using System.Xml.Serialization;
using Attach;
using NUnit.Framework;
using SvnBridge.Handlers;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

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
            TcpClientHttpRequest request = new TcpClientHttpRequest();
            request.SetHttpMethod("mkcol");
            request.SetPath("//!svn/wrk/0eaf3261-5f80-a140-b21d-c1b0316a256a/Folder%20With%20Spaces");
            request.Headers.Add("Host", "localhost:8081");
            MemoryStream outputStream = new MemoryStream();
            request.SetOutputStream(outputStream);

            RequestDispatcherFactory.Create(null).Dispatch(request);

            Assert.AreEqual("/Folder With Spaces", results.Parameters[1]);

            outputStream.Dispose();
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
        public void VerifyOptionsDecodesPathWhenCallingSourceControlProvider()
        {
            MyMocks mock = new MyMocks();
            StubSourceControlProvider provider = mock.CreateObject<StubSourceControlProvider>();
            WebDavService service = new WebDavService(provider);
            Results result = mock.Attach(provider.ItemExists, true);
            MemoryStream outputStream = new MemoryStream();

            service.Options("/Spikes/SvnFacade/trunk/New%20Folder%207", outputStream);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7", result.Parameters[0]);
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

        [Test]
        public void VerifyDeleteDecodesPathWhenCallingSourceControlProvider()
        {
            MyMocks mock = new MyMocks();
            StubSourceControlProvider provider = mock.CreateObject<StubSourceControlProvider>();
            WebDavService service = new WebDavService(provider);
            mock.Attach(provider.ItemExists, true);
            Results result = mock.Attach(provider.DeleteItem);

            service.Delete("//!svn/wrk/125c1a75-a7a6-104d-a661-54689d30dc99/Spikes/SvnFacade/trunk/New%20Folder%206");

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 6", result.Parameters[1]);
        }
    }
}