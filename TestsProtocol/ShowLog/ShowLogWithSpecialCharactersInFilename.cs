using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class ShowLogWithSpecialCharactersInFilename : ProtocolTestsBase
    {
        [Test]
        public void Test1()
        {
            stub.Attach((MyMocks.ItemExists) provider.ItemExists, new NetworkAccessDeniedException());

            string request =
                "PROPFIND /newFolder4 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:13:59 GMT\r\n" +
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
        public void Test10()
        {
            stub.Attach(provider.GetLatestVersion, 5532);

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
                "Date: Wed, 25 Jul 2007 00:14:04 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5532</D:href></lp1:checked-in>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test11()
        {
            string request =
                "PROPFIND /!svn/bln/5532 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:05 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5532</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5532/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5532</lp1:version-name>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test12()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new FolderMetaData();
            item.Name = "newFolder4";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /newFolder4 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:05 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 678\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/newFolder4/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>newFolder4</lp2:baseline-relative-path>\n" +
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
        public void Test13()
        {
            string request =
                "PROPFIND /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 148\r\n" +
                "Content-Type: text/xml\r\n" +
                "Label: 5532\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><baseline-collection xmlns=\"DAV:\"/><version-name xmlns=\"DAV:\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Wed, 25 Jul 2007 00:14:05 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Vary: Label\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5532</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5532/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5532</lp1:version-name>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test14()
        {
            List<SourceItemHistory> histories = new List<SourceItemHistory>();
            SourceItemHistory history1 =
                new SourceItemHistory(5532, "jwanagel", DateTime.Parse("2007-07-25T00:13:14.466022Z"), "1234");
            history1.Changes.Add(MakeChange(ChangeType.Add, "newFolder4"));
            history1.Changes.Add(MakeChange(ChangeType.Add, "newFolder4/A!@#$%^&()~`_-+={[}];',.txt"));
            histories.Add(history1);

            stub.Attach(provider.GetLog, new LogItem(@"C:\", "newFolder4", histories.ToArray()));

            string request =
                "REPORT /!svn/bc/5532/newFolder4 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 185\r\n" +
                "Content-Type: text/xml\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<S:log-report xmlns:S=\"svn:\"><S:start-revision>5532</S:start-revision><S:end-revision>1</S:end-revision><S:limit>100</S:limit><S:discover-changed-paths/><S:path></S:path></S:log-report>";

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Date: Wed, 25 Jul 2007 00:14:05 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "198\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:log-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n" +
                "<S:log-item>\n" +
                "<D:version-name>5532</D:version-name>\n" +
                "<D:creator-displayname>jwanagel</D:creator-displayname>\n" +
                "<S:date>2007-07-25T00:13:14.466022Z</S:date>\n" +
                "<D:comment>1234</D:comment>\n" +
                "<S:added-path>/newFolder4</S:added-path>\n" +
                "<S:added-path>/newFolder4/A!@#$%^&amp;()~`_-+={[}];',.txt</S:added-path>\n" +
                "</S:log-item>\n" +
                "</S:log-report>\n" +
                "\r\n" +
                "0\r\n" +
                "\r\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test2()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new FolderMetaData();
            item.Name = "newFolder4";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /newFolder4 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:03 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 678\r\n" +
                "Keep-Alive: timeout=15, max=99\r\n" +
                "Connection: Keep-Alive\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/newFolder4/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>newFolder4</lp2:baseline-relative-path>\n" +
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
            stub.Attach(provider.GetLatestVersion, 5532);

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
                "Date: Wed, 25 Jul 2007 00:14:03 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5532</D:href></lp1:checked-in>\n" +
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
                "PROPFIND /!svn/bln/5532 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:03 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5532</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5532/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5532</lp1:version-name>\n" +
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
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new FolderMetaData();
            item.Name = "newFolder4";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /newFolder4 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:03 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 678\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/newFolder4/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>newFolder4</lp2:baseline-relative-path>\n" +
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
            stub.Attach(provider.GetLatestVersion, 5532);

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
                "Date: Wed, 25 Jul 2007 00:14:04 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5532</D:href></lp1:checked-in>\n" +
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
                "PROPFIND /!svn/bln/5532 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:04 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5532</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5532/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5532</lp1:version-name>\n" +
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
            stub.Attach((MyMocks.ItemExists) provider.ItemExists, new NetworkAccessDeniedException());

            string request =
                "PROPFIND /newFolder4 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:04 GMT\r\n" +
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
        public void Test9()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new FolderMetaData();
            item.Name = "newFolder4";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /newFolder4 HTTP/1.1\r\n" +
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
                "Date: Wed, 25 Jul 2007 00:14:04 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 678\r\n" +
                "Keep-Alive: timeout=15, max=99\r\n" +
                "Connection: Keep-Alive\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/newFolder4/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path>newFolder4</lp2:baseline-relative-path>\n" +
                "<lp2:repository-uuid>81a5aebe-f34e-eb42-b435-ac1ecbb335f7</lp2:repository-uuid>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }
    }
}