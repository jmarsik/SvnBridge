using System;
using CodePlex.TfsLibrary;
using NUnit.Framework;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class CommitDeleteOfAlreadyDeletedFileTest : WebDavServiceTestsBase
    {
        [Test]
        public void Test1()
        {
            mock.Attach((MyMocks.ItemExists)provider.ItemExists, new NetworkAccessDeniedException());

            string request =
                "OPTIONS /Spikes/SvnFacade/trunk/New%20Folder HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Keep-Alive: \r\n" +
                "Connection: TE, Keep-Alive\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 104\r\n" +
                "Content-Type: text/xml\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:options xmlns:D=\"DAV:\"><D:activity-collection-set/></D:options>";

            string expected =
                "HTTP/1.1 401 Authorization Required\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:39 GMT\r\n" +
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

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test2()
        {
            mock.Attach(provider.ItemExists, true);

            string request =
                "OPTIONS /Spikes/SvnFacade/trunk/New%20Folder HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Keep-Alive: \r\n" +
                "Connection: TE, Keep-Alive\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 104\r\n" +
                "Content-Type: text/xml\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:options xmlns:D=\"DAV:\"><D:activity-collection-set/></D:options>";

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:48 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "DAV: 1,2\r\n" +
                "DAV: version-control,checkout,working-resource\r\n" +
                "DAV: merge,baseline,activity,version-controlled-collection\r\n" +
                "MS-Author-Via: DAV\r\n" +
                "Allow: OPTIONS,GET,HEAD,POST,DELETE,TRACE,PROPFIND,PROPPATCH,COPY,MOVE,LOCK,UNLOCK,CHECKOUT\r\n" +
                "Content-Length: 179\r\n" +
                "Keep-Alive: timeout=15, max=99\r\n" +
                "Connection: Keep-Alive\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:options-response xmlns:D=\"DAV:\">\n" +
                "<D:activity-collection-set><D:href>/!svn/act/</D:href></D:activity-collection-set></D:options-response>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test3()
        {
            mock.Attach(provider.MakeActivity);

            string request =
                "MKACTIVITY /!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n";

            string expected =
                "HTTP/1.1 201 Created\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:48 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Location: http://localhost:8082/!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a\r\n" +
                "Content-Length: 312\r\n" +
                "Content-Type: text/html\r\n" +
                "X-Pad: avoid browser bug\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Activity /!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test4()
        {
            mock.Attach(provider.ItemExists, true);

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk/New%20Folder HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 133\r\n" +
                "Content-Type: text/xml\r\n" +
                "Depth: 0\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><propfind xmlns=\"DAV:\"><prop><version-controlled-configuration xmlns=\"DAV:\"/></prop></propfind>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:48 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 450\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/Spikes/SvnFacade/trunk/New%20Folder/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test5()
        {
            mock.Attach(provider.GetLatestVersion, 5488);

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
                "Date: Mon, 25 Jun 2007 19:54:48 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5488</D:href></lp1:checked-in>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test6()
        {
            string request =
                "CHECKOUT /!svn/bln/5488 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 174\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:checkout xmlns:D=\"DAV:\"><D:activity-set><D:href>/!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a</D:href></D:activity-set></D:checkout>";

            string expected =
                "HTTP/1.1 201 Created\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:49 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Location: http://localhost:8082//!svn/wbl/70df3104-9f67-8d4e-add7-6012fe86c03a/5488\r\n" +
                "Content-Length: 330\r\n" +
                "Content-Type: text/html\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Checked-out resource //!svn/wbl/70df3104-9f67-8d4e-add7-6012fe86c03a/5488 has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test7()
        {
            mock.Attach(provider.SetActivityComment);

            string request =
                "PROPPATCH //!svn/wbl/70df3104-9f67-8d4e-add7-6012fe86c03a/5488 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 224\r\n" +
                "Content-Type: application/xml\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
                "<D:propertyupdate xmlns:D=\"DAV:\"><D:set><D:prop><log xmlns=\"http://subversion.tigris.org/xmlns/svn/\">Deleting file which is already deleted.</log></D:prop></D:set>\n" +
                "</D:propertyupdate>\n";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:49 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 348\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response>\n" +
                "<D:href>//!svn/wbl/70df3104-9f67-8d4e-add7-6012fe86c03a/5488</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<ns1:log/>\r\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test8()
        {
            mock.Attach(provider.ItemExists, true);
            ItemMetaData item = new ItemMetaData();
            item.Revision = 5487;
            mock.Attach(provider.GetItems, item);

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk/New%20Folder HTTP/1.1\r\n" +
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
                "Date: Mon, 25 Jun 2007 19:54:49 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 439\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/Spikes/SvnFacade/trunk/New%20Folder/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:checked-in><D:href>/!svn/ver/5487/Spikes/SvnFacade/trunk/New%20Folder</D:href></lp1:checked-in>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test9()
        {
            ItemMetaData item = new ItemMetaData();
            item.Revision = 0;
            mock.Attach(provider.GetItems, item);

            string request =
                "CHECKOUT /!svn/ver/5487/Spikes/SvnFacade/trunk/New%20Folder HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 174\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:checkout xmlns:D=\"DAV:\"><D:activity-set><D:href>/!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a</D:href></D:activity-set></D:checkout>";

            string expected =
                "HTTP/1.1 201 Created\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:49 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Location: http://localhost:8082//!svn/wrk/70df3104-9f67-8d4e-add7-6012fe86c03a/Spikes/SvnFacade/trunk/New%20Folder\r\n" +
                "Content-Length: 361\r\n" +
                "Content-Type: text/html\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Checked-out resource //!svn/wrk/70df3104-9f67-8d4e-add7-6012fe86c03a/Spikes/SvnFacade/trunk/New%20Folder has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test10()
        {
            mock.Attach(provider.ItemExists, false);

            string request =
                "DELETE //!svn/wrk/70df3104-9f67-8d4e-add7-6012fe86c03a/Spikes/SvnFacade/trunk/New%20Folder/Test2.txt HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "X-SVN-Version-Name: 5486\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n";

            string expected =
                "HTTP/1.1 404 Not Found\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:49 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 380\r\n" +
                "Content-Type: text/html; charset=iso-8859-1\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>404 Not Found</title>\n" +
                "</head><body>\n" +
                "<h1>Not Found</h1>\n" +
                "<p>The requested URL //!svn/wrk/70df3104-9f67-8d4e-add7-6012fe86c03a/Spikes/SvnFacade/trunk/New Folder/Test2.txt was not found on this server.</p>\n" +
                "<hr>\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test11()
        {
            MergeActivityResponse mergeResponse = new MergeActivityResponse(5489, DateTime.Parse("2007-06-25T19:54:50.112508Z"), "jwanagel");
            mock.Attach(provider.MergeActivity, mergeResponse);

            string request =
                "MERGE /Spikes/SvnFacade/trunk/New%20Folder HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 297\r\n" +
                "Content-Type: text/xml\r\n" +
                "X-SVN-Options:  release-locks\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:merge xmlns:D=\"DAV:\"><D:source><D:href>/!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a</D:href></D:source><D:no-auto-merge/><D:no-checkout/><D:prop><D:checked-in/><D:version-name/><D:resourcetype/><D:creationdate/><D:creator-displayname/></D:prop></D:merge>";

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:49 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/xml\r\n" +
                "\r\n" +
                "1d9\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:merge-response xmlns:D=\"DAV:\">\n" +
                "<D:updated-set>\n" +
                "<D:response>\n" +
                "<D:href>/!svn/vcc/default</D:href>\n" +
                "<D:propstat><D:prop>\n" +
                "<D:resourcetype><D:baseline/></D:resourcetype>\n" +
                "\n" +
                "<D:version-name>5489</D:version-name>\n" +
                "<D:creationdate>2007-06-25T19:54:50.112508Z</D:creationdate>\n" +
                "<D:creator-displayname>jwanagel</D:creator-displayname>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:updated-set>\n" +
                "</D:merge-response>\n" +
                "\r\n" +
                "0\r\n" +
                "\r\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test12()
        {
            mock.Attach(provider.DeleteActivity);

            string request =
                "DELETE /!svn/act/70df3104-9f67-8d4e-add7-6012fe86c03a HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n";

            string expected =
                "HTTP/1.1 204 No Content\r\n" +
                "Date: Mon, 25 Jun 2007 19:54:50 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 0\r\n" +
                "Content-Type: text/plain\r\n" +
                "\r\n";

            string actual = ProcessRequest(request, expected);

            Assert.AreEqual(expected, actual);
        }
    }
}