using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using NUnit.Framework;
using SvnBridge.Net;
using SvnBridge.Stubs;
using Tests;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.SourceControl;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using System.IO;
using Attach;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class ReportLogHandlerTests
    {
        protected MyMocks mock = new MyMocks();
        protected StubSourceControlProvider provider;
        protected StubHttpContext context;
        protected StubHttpRequest request;
        protected StubHttpResponse response;
        protected ReportHandler handler;

        [SetUp]
        public virtual void Setup()
        {
            provider = mock.CreateObject<StubSourceControlProvider>();
            SourceControlProviderFactory.CreateDelegate = delegate { return provider; };
            context = new StubHttpContext();
            request = new StubHttpRequest();
            request.Headers = new NameValueCollection();
            context.Request = request;
            response = new StubHttpResponse();
            response.Headers = new HttpResponseHeaderCollection();
            response.OutputStream = new MemoryStream(Constants.BufferSize);
            context.Response = response;
            handler = new ReportHandler();
        }

        [Test]
        public void VerifyHandleProducesCorrectOutputForRenamedFile()
        {
            List<SourceItemHistory> histories = new List<SourceItemHistory>();
            SourceItemHistory history1 = new SourceItemHistory(5531, "jwanagel", DateTime.Parse("2007-07-24T07:46:20.635845Z"), "Renamed file");
            history1.Changes.Add(TestHelper.MakeChange(ChangeType.Rename, "$/newFolder3/NewFileRename.txt", "$/newFolder3/NewFile.txt", 5530));
            histories.Add(history1);
            Results r = mock.Attach(provider.GetLog, new LogItem(@"C:\", "$/newFolder2", histories.ToArray()));
            request.Url = new Uri("http://localhost:8082/!svn/bc/5522/File.txt");
            string requestBody = "<S:log-report xmlns:S=\"svn:\"><S:start-revision>5531</S:start-revision><S:end-revision>1</S:end-revision><S:limit>100</S:limit><S:discover-changed-paths/><S:path></S:path></S:log-report>";
            request.InputStream = new MemoryStream(Encoding.Default.GetBytes(requestBody));

            handler.Handle(context, "http://tfsserver");

            string expected =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:log-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n" +
                "<S:log-item>\n" +
                "<D:version-name>5531</D:version-name>\n" +
                "<D:creator-displayname>jwanagel</D:creator-displayname>\n" +
                "<S:date>2007-07-24T07:46:20.635845Z</S:date>\n" +
                "<D:comment>Renamed file</D:comment>\n" +
                "<S:added-path copyfrom-path=\"/newFolder3/NewFile.txt\" copyfrom-rev=\"5530\">/newFolder3/NewFileRename.txt</S:added-path>\n" +
                "<S:deleted-path>/newFolder3/NewFile.txt</S:deleted-path>\n" +
                "</S:log-item>\n" +
                "</S:log-report>\n";

            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray()));
            Assert.AreEqual("text/xml; charset=\"utf-8\"", response.ContentType);
            Assert.AreEqual(Encoding.UTF8, response.ContentEncoding);
            Assert.AreEqual(200, response.StatusCode);
            Assert.IsTrue(response.SendChunked);
            Assert.AreEqual("/File.txt", r.Parameters[0]);
            Assert.AreEqual(1, r.Parameters[1]);
            Assert.AreEqual(5531, r.Parameters[2]);
            Assert.AreEqual(Recursion.Full, r.Parameters[3]);
            Assert.AreEqual(100, r.Parameters[4]);
        }
    }
}
