using System;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class CommitIgnoreFileTest : ProtocolTestsBase
    {
        [Test]
        public void Test1()
        {
            stub.Attach((MyMocks.ItemExists)provider.ItemExists, new NetworkAccessDeniedException());

            string request =
                "OPTIONS /Spikes/SvnFacade/trunk/New%20Folder%204 HTTP/1.1\r\n" +
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
                "Date: Sat, 09 Jun 2007 07:59:42 GMT\r\n" +
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
            stub.Attach(provider.ItemExists, true);

            string request =
                "OPTIONS /Spikes/SvnFacade/trunk/New%20Folder%204 HTTP/1.1\r\n" +
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
                "Date: Sat, 09 Jun 2007 07:59:42 GMT\r\n" +
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

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test3()
        {
            stub.Attach(provider.MakeActivity);

            string request =
                "MKACTIVITY /!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n";

            string expected =
                "HTTP/1.1 201 Created\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:42 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Location: http://localhost:8082/!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7\r\n" +
                "Content-Length: 312\r\n" +
                "Content-Type: text/html\r\n" +
                "X-Pad: avoid browser bug\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Activity /!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7 has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test4()
        {
            stub.Attach(provider.ItemExists, true);
            FolderMetaData folder = new FolderMetaData();
            folder.Name = "/Spikes/SvnFacade/trunk/New Folder 4";
            stub.Attach(provider.GetItems, folder);

            string request =
                "PROPFIND /Spikes/SvnFacade/trunk/New%20Folder%204 HTTP/1.1\r\n" +
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
                "Date: Sat, 09 Jun 2007 07:59:42 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 454\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/Spikes/SvnFacade/trunk/New%20Folder%204/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
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
            stub.Attach(provider.GetLatestVersion, 5462);

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
                "Date: Sat, 09 Jun 2007 07:59:43 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5462</D:href></lp1:checked-in>\n" +
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
            string request =
                "CHECKOUT /!svn/bln/5462 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 174\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:checkout xmlns:D=\"DAV:\"><D:activity-set><D:href>/!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7</D:href></D:activity-set></D:checkout>";

            string expected =
                "HTTP/1.1 201 Created\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:43 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Location: http://localhost:8082//!svn/wbl/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/5462\r\n" +
                "Content-Length: 330\r\n" +
                "Content-Type: text/html\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Checked-out resource //!svn/wbl/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/5462 has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test7()
        {
            stub.Attach(provider.SetActivityComment);

            string request =
                "PROPPATCH //!svn/wbl/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/5462 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 202\r\n" +
                "Content-Type: application/xml\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
                "<D:propertyupdate xmlns:D=\"DAV:\"><D:set><D:prop><log xmlns=\"http://subversion.tigris.org/xmlns/svn/\">Added ignore file</log></D:prop></D:set>\n" +
                "</D:propertyupdate>\n";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:43 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 348\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response>\n" +
                "<D:href>//!svn/wbl/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/5462</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<ns1:log/>\r\n" +
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
            ItemMetaData item = new ItemMetaData();
            item.Revision = 0;
            stub.Attach(provider.GetItems, item);

            string request =
                "CHECKOUT /!svn/ver/5389/Spikes/SvnFacade/trunk/New%20Folder%204 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 174\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:checkout xmlns:D=\"DAV:\"><D:activity-set><D:href>/!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7</D:href></D:activity-set></D:checkout>";

            string expected =
                "HTTP/1.1 201 Created\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:44 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Location: http://localhost:8082//!svn/wrk/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/Spikes/SvnFacade/trunk/New%20Folder%204\r\n" +
                "Content-Length: 365\r\n" +
                "Content-Type: text/html\r\n" +
                "\r\n" +
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Checked-out resource //!svn/wrk/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/Spikes/SvnFacade/trunk/New%20Folder%204 has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8082</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test9()
        {
            stub.Attach(provider.SetProperty);

            string request =
                "PROPPATCH //!svn/wrk/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/Spikes/SvnFacade/trunk/New%20Folder%204 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 303\r\n" +
                "Content-Type: text/xml; charset=UTF-8\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:propertyupdate xmlns:D=\"DAV:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:C=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:S=\"http://subversion.tigris.org/xmlns/svn/\"><D:set><D:prop><S:ignore>*.bad\n" +
                "</S:ignore></D:prop></D:set></D:propertyupdate>";

            string expected =
                "HTTP/1.1 207 Multi-Status\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:44 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 493\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns3=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns2=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response>\n" +
                "<D:href>//!svn/wrk/be05cf36-7514-3f4d-81ea-7822f7b1dfe7/Spikes/SvnFacade/trunk/New%20Folder%204</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<ns1:ignore/>\r\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:multistatus>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test10()
        {
            MergeActivityResponse mergeResponse = new MergeActivityResponse(5463, DateTime.Parse("2007-06-09T07:59:44.933153Z"), "jwanagel");
            mergeResponse.Items.Add(new MergeActivityResponseItem(ItemType.Folder, "/Spikes/SvnFacade/trunk/New Folder 4"));
            stub.Attach(provider.MergeActivity, mergeResponse);

            string request =
                "MERGE /Spikes/SvnFacade/trunk/New%20Folder%204 HTTP/1.1\r\n" +
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
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><D:merge xmlns:D=\"DAV:\"><D:source><D:href>/!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7</D:href></D:source><D:no-auto-merge/><D:no-checkout/><D:prop><D:checked-in/><D:version-name/><D:resourcetype/><D:creationdate/><D:creator-displayname/></D:prop></D:merge>";

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:44 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Cache-Control: no-cache\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/xml\r\n" +
                "\r\n" +
                "316\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:merge-response xmlns:D=\"DAV:\">\n" +
                "<D:updated-set>\n" +
                "<D:response>\n" +
                "<D:href>/!svn/vcc/default</D:href>\n" +
                "<D:propstat><D:prop>\n" +
                "<D:resourcetype><D:baseline/></D:resourcetype>\n" +
                "\n" +
                "<D:version-name>5463</D:version-name>\n" +
                "<D:creationdate>2007-06-09T07:59:44.933153Z</D:creationdate>\n" +
                "<D:creator-displayname>jwanagel</D:creator-displayname>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "<D:response>\n" +
                "<D:href>/Spikes/SvnFacade/trunk/New%20Folder%204</D:href>\n" +
                "<D:propstat><D:prop>\n" +
                "<D:resourcetype><D:collection/></D:resourcetype>\n" +
                "<D:checked-in><D:href>/!svn/ver/5463/Spikes/SvnFacade/trunk/New%20Folder%204</D:href></D:checked-in>\n" +
                "</D:prop>\n" +
                "<D:status>HTTP/1.1 200 OK</D:status>\n" +
                "</D:propstat>\n" +
                "</D:response>\n" +
                "</D:updated-set>\n" +
                "</D:merge-response>\n" +
                "\r\n" +
                "0\r\n" +
                "\r\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test11()
        {
            stub.Attach(provider.DeleteActivity);

            string request =
                "DELETE /!svn/act/be05cf36-7514-3f4d-81ea-7822f7b1dfe7 HTTP/1.1\r\n" +
                "Host: localhost:8082\r\n" +
                "User-Agent: SVN/1.4.2 (r22196) neon/0.26.2\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n";

            string expected =
                "HTTP/1.1 204 No Content\r\n" +
                "Date: Sat, 09 Jun 2007 07:59:45 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 0\r\n" +
                "Content-Type: text/plain\r\n" +
                "\r\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }
    }
}