using System;
using System.Text;
using Attach;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class UpdateWithNewFileTest : ProtocolTestsBase
    {
        [Test]
        public void Test1()
        {
            mock.Attach((MyMocks.ItemExists)provider.ItemExists, new NetworkAccessDeniedException());

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Keep-Alive: \r\n" +
                "Connection: TE, Keep-Alive\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 300\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 401 Authorization Required\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:16 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "WWW-Authenticate: Basic realm=\"CodePlex Subversion Repository\"\r\n" +
                "Content-Length: 493\r\n" +
                "Keep-Alive: timeout=15, max=100\r\n" +
                "Connection: Keep-Alive\r\n" +
                "Content-Type: text/html; charset=iso-8859-1\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>401 Authorization Required</title>\n" +
                "</head><body>\n" +
                "<h1>Authorization Required</h1>\n" +
                "<p>This server could not verify that you\n" +
                "are authorized to access the document\n" +
                "requested.  Either you supplied the wrong\n" +
                "credentials (e.g., bad password), or your\n" +
                "browser doesn't understand how to supply\n" +
                "the credentials required.</p>\n" +
                "<hr>\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test2()
        {
            mock.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Spikes/SvnFacade/trunk";
            item.ItemType = ItemType.Folder;
            mock.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Keep-Alive: \r\n" +
                "Connection: TE, Keep-Alive\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 300\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:16 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 702\r\n" +
                "Keep-Alive: timeout=15, max=99\r\n" +
                "Connection: Keep-Alive\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/Spikes/SvnFacade/trunk/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>Spikes/SvnFacade/trunk</lp2:baseline-relative-path>\n" +
                "<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test3()
        {
            mock.Attach(provider.GetLatestVersion, 5461);

            string request =
                "PROPFIND /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 111\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><checked-in xmlns=\"DAV:\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:16 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 383\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/vcc/default</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:checked-in><D:href>/!svn/bln/5461</D:href></lp1:checked-in>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test4()
        {
            string request =
                "PROPFIND /!svn/bln/5461 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 148\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-collection xmlns=\"DAV:\"/><version-name xmlns=\"DAV:\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:16 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5461</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5461/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5461</lp1:version-name>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test5()
        {
            mock.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Spikes/SvnFacade/trunk";
            item.ItemType = ItemType.Folder;
            mock.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 300\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:16 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 702\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/Spikes/SvnFacade/trunk/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>Spikes/SvnFacade/trunk</lp2:baseline-relative-path>\n" +
                "<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test6()
        {
            mock.Attach(provider.GetLatestVersion, 5461);

            string request =
                "PROPFIND /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 111\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><checked-in xmlns=\"DAV:\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:17 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 383\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/vcc/default</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:checked-in><D:href>/!svn/bln/5461</D:href></lp1:checked-in>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test7()
        {
            string request =
                "PROPFIND /!svn/bln/5461 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 148\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-collection xmlns=\"DAV:\"/><version-name xmlns=\"DAV:\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:17 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5461</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5461/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5461</lp1:version-name>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test8()
        {
            mock.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Spikes/SvnFacade/trunk";
            item.ItemType = ItemType.Folder;
            mock.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 300\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/><resourcetype xmlns=\"DAV:\"/><baseline-relative-path xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/><repository-uuid xmlns=\"http://subversion.tigris.org/xmlns/dav/\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:17 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 702\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/Spikes/SvnFacade/trunk/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>Spikes/SvnFacade/trunk</lp2:baseline-relative-path>\n" +
                "<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test9()
        {
            FolderMetaData metadata = new FolderMetaData();
            metadata.Name = "Spikes/SvnFacade/trunk";
            metadata.Revision = 5461;
            metadata.Author = "jwanagel";
            metadata.LastModifiedDate = DateTime.Parse("2007-06-07T20:45:13.462598Z");
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "Spikes/SvnFacade/trunk/New Folder 3";
            folder.ItemType = ItemType.Folder;
            folder.Revision = 5461;
            folder.Author = "jwanagel";
            folder.LastModifiedDate = DateTime.Parse("2007-06-07T20:45:13.462598Z");
            metadata.Items.Add(folder);
            ItemMetaData item = new ItemMetaData();
            item.Name = "Spikes/SvnFacade/trunk/New Folder 3/AddedFile.txt";
            item.ItemType = ItemType.File;
            item.Revision = 5461;
            item.Author = "jwanagel";
            item.LastModifiedDate = DateTime.Parse("2007-06-07T20:45:13.462598Z");
            folder.Items.Add(item);
            mock.Attach(provider.GetChangedItems, metadata);
            byte[] fileData = Encoding.UTF8.GetBytes("New file");
            mock.Attach(provider.ReadFile, fileData);
            MultipleReturnValues returnValues = new MultipleReturnValues();
            returnValues.Add(true);
            returnValues.Add(false);
            mock.Attach(provider.ItemExists, returnValues);

            string request =
                "REPORT /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 209\r\n" +
                "Content-Type: text/xml\r\n" +
                "Accept-Encoding: svndiff1;q=0.9,svndiff;q=0.8\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8082/Spikes/SvnFacade/trunk</S:src-path><S:target-revision>5461</S:target-revision><S:entry rev=\"5460\" ></S:entry></S:update-report>";

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Date: Thu, 07 Jun 2007 20:46:17 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                //"6ba\r\n" +
                "6b6\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n" +
                "<S:target-revision rev=\"5461\"/>\n" +
                "<S:open-directory rev=\"5460\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5461/Spikes/SvnFacade/trunk</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5461</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-06-07T20:45:13.462598Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:open-directory name=\"New Folder 3\" rev=\"5460\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5461/Spikes/SvnFacade/trunk/New%20Folder%203</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5461</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-06-07T20:45:13.462598Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:add-file name=\"AddedFile.txt\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5461/Spikes/SvnFacade/trunk/New%20Folder%203/AddedFile.txt</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5461</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2007-06-07T20:45:13.462598Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                //"<S:txdelta>U1ZOAQAACAIJAYgITmV3IGZpbGU=\n" +
                "<S:txdelta>U1ZOAAAACAEIiE5ldyBmaWxl\n" +
                "</S:txdelta><S:prop><V:md5-checksum>681e10aecbafd7dd385fa51798ca0fd6</V:md5-checksum></S:prop>\n" +
                "</S:add-file>\n" +
                "<S:prop></S:prop>\n" +
                "</S:open-directory>\n" +
                "<S:prop></S:prop>\n" +
                "</S:open-directory>\n" +
                "</S:update-report>\n" +
                "\r\n" +
                "0\r\n" +
                "\r\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }
    }
}