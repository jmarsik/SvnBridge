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
        public void VerifyHandleProducesCorrectOutputForDeletedFileInSubfolder()
        {
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "";
            folder.Author = "jwanagel";
            folder.Revision = 5698;
            folder.LastModifiedDate = DateTime.Parse("2007-08-21T00:41:27.680005Z");
            folder.Items.Add(new FolderMetaData());
            folder.Items[0].ItemType = ItemType.Folder;
            folder.Items[0].Name = "Test9";
            folder.Items[0].Author = "jwanagel";
            folder.Items[0].Revision = 5698;
            folder.Items[0].LastModifiedDate = DateTime.Parse("2007-08-21T00:41:27.680005Z");
            ((FolderMetaData)folder.Items[0]).Items.Add(new DeleteMetaData());
            ((FolderMetaData)folder.Items[0]).Items[0].Name = "Test.txt";
            Results r = mock.Attach(provider.GetChangedItems, folder);
            mock.Attach(provider.ItemExists, true);
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
    }
}
