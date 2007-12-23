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
using SvnBridge.Utility;
using SvnBridge.Infrastructure;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class ReportHandlerUpdateReportTests : HandlerTestsBase
    {
        protected ReportHandler handler = new ReportHandler();

        [Test]
        public void VerifyHandleProducesCorrectOutputForBranchedFile()
        {
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "";
            folder.Author = "jwanagel";
            folder.Revision = 5700;
            folder.LastModifiedDate = DateTime.Parse("2007-09-05T18:37:14.239559Z");
            folder.Items.Add(new ItemMetaData());
            folder.Items[0].Name = "asfd2.txt";
            folder.Items[0].Author = "jwanagel";
            folder.Items[0].Revision = 5700;
            folder.Items[0].LastModifiedDate = DateTime.Parse("2007-09-05T18:37:14.239559Z");
            Results r = stub.Attach(provider.GetChangedItems, folder);
            stub.Attach(provider.ItemExists, false);
            byte[] fileData = Encoding.UTF8.GetBytes("test");
            stub.Attach(provider.ReadFile, fileData);
            request.Path = "http://localhost:8082/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8082</S:src-path><S:target-revision>5700</S:target-revision><S:entry rev=\"5699\" ></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string expected =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n" +
                "<S:target-revision rev=\"5700\"/>\n" +
                "<S:open-directory rev=\"5699\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5700/</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5700</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-09-05T18:37:14.239559Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:add-file name=\"asfd2.txt\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5700/asfd2.txt</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5700</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-09-05T18:37:14.239559Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                //"<S:txdelta>U1ZOAQAABAIFAYQEdGVzdA==\n" +
                "<S:txdelta>U1ZOAAAABAEEhHRlc3Q=\n" +
                "</S:txdelta><S:prop><V:md5-checksum>098f6bcd4621d373cade4e832627b4f6</V:md5-checksum></S:prop>\n" +
                "</S:add-file>\n" +
                "<S:prop></S:prop>\n" +
                "</S:open-directory>\n" +
                "</S:update-report>\n";
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray()));
            Assert.AreEqual("text/xml; charset=\"utf-8\"", response.ContentType);
            Assert.AreEqual(Encoding.UTF8, response.ContentEncoding);
            Assert.AreEqual(200, response.StatusCode);
            Assert.IsTrue(response.SendChunked);
            Assert.AreEqual("/", r.Parameters[0]);
            Assert.AreEqual(5699, r.Parameters[1]);
            Assert.AreEqual(5700, r.Parameters[2]);
        }

        [Test]
        public void VerifyHandleProducesCorrectOutputForDeletedFileInSubfolder()
        {
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "";
            folder.Author = "jwanagel";
            folder.Revision = 5698;
            folder.LastModifiedDate = DateTime.Parse("2007-08-21T00:41:27.680005Z");
            folder.Items.Add(new FolderMetaData());
            folder.Items[0].Name = "Test9";
            folder.Items[0].Author = "jwanagel";
            folder.Items[0].Revision = 5698;
            folder.Items[0].LastModifiedDate = DateTime.Parse("2007-08-21T00:41:27.680005Z");
            ((FolderMetaData)folder.Items[0]).Items.Add(new DeleteMetaData());
            ((FolderMetaData)folder.Items[0]).Items[0].Name = "Test.txt";
            Results r = stub.Attach(provider.GetChangedItems, folder);
            stub.Attach(provider.ItemExists, true);
            request.Path = "http://localhost:8082/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8082</S:src-path><S:target-revision>5698</S:target-revision><S:entry rev=\"5697\" ></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string expected =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n" +
                "<S:target-revision rev=\"5698\"/>\n" +
                "<S:open-directory rev=\"5697\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5698/</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5698</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-08-21T00:41:27.680005Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:open-directory name=\"Test9\" rev=\"5697\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5698/Test9</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5698</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-08-21T00:41:27.680005Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:delete-entry name=\"Test.txt\"/>\n" +
                "<S:prop></S:prop>\n" +
                "</S:open-directory>\n" +
                "<S:prop></S:prop>\n" +
                "</S:open-directory>\n" +
                "</S:update-report>\n";
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray()));
            Assert.AreEqual("text/xml; charset=\"utf-8\"", response.ContentType);
            Assert.AreEqual(Encoding.UTF8, response.ContentEncoding);
            Assert.AreEqual(200, response.StatusCode);
            Assert.IsTrue(response.SendChunked);
            Assert.AreEqual("/", r.Parameters[0]);
            Assert.AreEqual(5697, r.Parameters[1]);
            Assert.AreEqual(5698, r.Parameters[2]);
        }

        [Test]
        public void VerifyHandleSucceedsWhenTargetRevisionIsNotSpecified()
        {
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "";
            folder.Author = "jwanagel";
            folder.Revision = 5713;
            folder.LastModifiedDate = DateTime.Parse("2007-09-17T02:38:24.225369Z");
            stub.Attach(provider.GetChangedItems, folder);
            stub.Attach(provider.GetLatestVersion, 5713);

            request.Path = "http://localhost:8085/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8085</S:src-path><S:entry rev=\"5713\" ></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string expected =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n" +
                "<S:target-revision rev=\"5713\"/>\n" +
                "<S:open-directory rev=\"5713\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5713/</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5713</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-09-17T02:38:24.225369Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:prop></S:prop>\n" +
                "</S:open-directory>\n" +
                "</S:update-report>\n";

            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray()));
        }

        [Test]
        public void VerifyHandleEncodesAddFileElements()
        {
            FolderMetaData metadata = new FolderMetaData();
            metadata.Name = "Test";
            metadata.Revision = 5722;
            metadata.Author = "bradwils";
            metadata.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            ItemMetaData item = new ItemMetaData();
            item.Name = "Test/C !@#$%^&()_-+={[}];',.~`..txt";
            item.Revision = 5722;
            item.Author = "bradwils";
            item.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            metadata.Items.Add(item);
            stub.Attach(provider.GetItems, metadata);
            byte[] fileData = Encoding.UTF8.GetBytes("test");
            stub.Attach(provider.ReadFile, fileData);

            request.Path = "http://localhost:8084/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8084/Test</S:src-path><S:target-revision>5722</S:target-revision><S:entry rev=\"5722\"  start-empty=\"true\"></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string output = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(output.Contains("<S:add-file name=\"C !@#$%^&amp;()_-+={[}];',.~`..txt\">"));
        }

        [Test]
        public void VerifyHandleEncodesAddFileCheckedInHrefElements()
        {
            FolderMetaData metadata = new FolderMetaData();
            metadata.Name = "Test";
            metadata.Revision = 5722;
            metadata.Author = "bradwils";
            metadata.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            ItemMetaData item = new ItemMetaData();
            item.Name = "Test/C !@#$%^&()_-+={[}];',.~`..txt";
            item.Revision = 5722;
            item.Author = "bradwils";
            item.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            metadata.Items.Add(item);
            stub.Attach(provider.GetItems, metadata);
            byte[] fileData = Encoding.UTF8.GetBytes("test");
            stub.Attach(provider.ReadFile, fileData);

            request.Path = "http://localhost:8084/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8084/Test</S:src-path><S:target-revision>5722</S:target-revision><S:entry rev=\"5722\"  start-empty=\"true\"></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string output = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(output.Contains("<D:checked-in><D:href>/!svn/ver/5722/Test/C%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60..txt</D:href></D:checked-in>"));
        }

        [Test]
        public void VerifyHandleEncodesAddDirectoryElements()
        {
            FolderMetaData metadata = new FolderMetaData();
            metadata.Name = "Test";
            metadata.Revision = 5722;
            metadata.Author = "bradwils";
            metadata.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "Test/B !@#$%^&()_-+={[}];',.~`";
            folder.Revision = 5722;
            folder.Author = "bradwils";
            folder.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            metadata.Items.Add(folder);
            stub.Attach(provider.GetItems, metadata);
            byte[] fileData = Encoding.UTF8.GetBytes("test");
            stub.Attach(provider.ReadFile, fileData);

            request.Path = "http://localhost:8084/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8084/Test</S:src-path><S:target-revision>5722</S:target-revision><S:entry rev=\"5722\"  start-empty=\"true\"></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string output = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(output.Contains("<S:add-directory name=\"B !@#$%^&amp;()_-+={[}];',.~`\" bc-url=\"/!svn/bc/5722/Test/B%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60\">"));
        }

        [Test]
        public void VerifyHandleEncodesAddDirectoryCheckedInHrefElements()
        {
            FolderMetaData metadata = new FolderMetaData();
            metadata.Name = "Test";
            metadata.Revision = 5722;
            metadata.Author = "bradwils";
            metadata.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "Test/B !@#$%^&()_-+={[}];',.~`";
            folder.Revision = 5722;
            folder.Author = "bradwils";
            folder.LastModifiedDate = DateTime.Parse("2007-12-15T00:56:55.541665Z");
            metadata.Items.Add(folder);
            stub.Attach(provider.GetItems, metadata);
            byte[] fileData = Encoding.UTF8.GetBytes("test");
            stub.Attach(provider.ReadFile, fileData);

            request.Path = "http://localhost:8084/!svn/vcc/default";
            request.Input = "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8084/Test</S:src-path><S:target-revision>5722</S:target-revision><S:entry rev=\"5722\"  start-empty=\"true\"></S:entry></S:update-report>";

            handler.Handle(context, tfsUrl);

            string output = Encoding.Default.GetString(((MemoryStream)response.OutputStream).ToArray());
            Assert.IsTrue(output.Contains("<D:checked-in><D:href>/!svn/ver/5722/Test/B%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60</D:href></D:checked-in>"));
        }
    }
}
