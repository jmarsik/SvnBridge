using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
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
        protected MockContext context;
        protected ReportHandler handler;

        [SetUp]
        public virtual void Setup()
        {
            provider = mock.CreateObject<StubSourceControlProvider>();
            SourceControlProviderFactory.CreateDelegate = delegate { return provider; };
            context = new MockContext();
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
            context.Path = "/!svn/bc/5522/File.txt";
            string request = "<S:log-report xmlns:S=\"svn:\"><S:start-revision>5531</S:start-revision><S:end-revision>1</S:end-revision><S:limit>100</S:limit><S:discover-changed-paths/><S:path></S:path></S:log-report>";
            context.InputStream = new MemoryStream(Encoding.Default.GetBytes(request));

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

            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)context.OutputStream).ToArray()));
            Assert.AreEqual("text/xml; charset=\"utf-8\"", context.ContentType);
            Assert.AreEqual(Encoding.UTF8, context.ContentEncoding);
            Assert.AreEqual(200, context.StatusCode);
            Assert.IsTrue(context.SendChunked);
            Assert.AreEqual("/File.txt", r.Parameters[0]);
            Assert.AreEqual(1, r.Parameters[1]);
            Assert.AreEqual(5531, r.Parameters[2]);
            Assert.AreEqual(Recursion.Full, r.Parameters[3]);
            Assert.AreEqual(100, r.Parameters[4]);
        }
    }
}
