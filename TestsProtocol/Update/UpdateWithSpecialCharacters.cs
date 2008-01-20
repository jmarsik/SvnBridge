using System;
using CodePlex.TfsLibrary;
using NUnit.Framework;
using SvnBridge.SourceControl;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using System.Text;

namespace Tests
{
    [TestFixture]
    public class UpdateWithSpecialCharacters : ProtocolTestsBase
    {
        [Test]
        public void Test1()
        {
            stub.Attach((MyMocks.ItemExists)provider.ItemExists, new NetworkAccessDeniedException());

            string request =
                "PROPFIND / HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:10 GMT\r\n" +
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
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at localhost Port 8084</address>\n" +
                "</body></html>\n";

            string actual = ProcessRequest(request, ref expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test2()
        {
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new FolderMetaData();
            item.Name = "";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND / HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:18 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 629\r\n" +
                "Keep-Alive: timeout=15, max=99\r\n" +
                "Connection: Keep-Alive\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path/>\n" +
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
            stub.Attach(provider.GetLatestVersion, 5734);

            string request =
                "PROPFIND /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:18 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5734</D:href></lp1:checked-in>\n" +
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
                "PROPFIND /!svn/bln/5734 HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:18 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5734</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5734/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5734</lp1:version-name>\n" +
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
            item.Name = "";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND / HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:18 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 629\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path/>\n" +
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
            stub.Attach(provider.GetLatestVersion, 5734);

            string request =
                "PROPFIND /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:18 GMT\r\n" +
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
                "<lp1:checked-in><D:href>/!svn/bln/5734</D:href></lp1:checked-in>\n" +
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
                "PROPFIND /!svn/bln/5734 HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:18 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 440\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/!svn/bln/5734</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:baseline-collection><D:href>/!svn/bc/5734/</D:href></lp1:baseline-collection>\n" +
                "<lp1:version-name>5734</lp1:version-name>\n" +
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
            stub.Attach(provider.ItemExists, true);
            ItemMetaData item = new FolderMetaData();
            item.Name = "";
            stub.Attach(provider.GetItems, item);

            string request =
                "PROPFIND / HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
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
                "Date: Sun, 20 Jan 2008 08:56:19 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Content-Length: 629\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n" +
                "<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n" +
                "<D:href>/</D:href>\n" +
                "<D:propstat>\n" +
                "<D:prop>\n" +
                "<lp1:version-controlled-configuration><D:href>/!svn/vcc/default</D:href></lp1:version-controlled-configuration>\n" +
                "<lp1:resourcetype><D:collection/></lp1:resourcetype>\n" +
                "<lp2:baseline-relative-path/>\n" +
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
            metadata.Name = "";
            metadata.ItemRevision = 5734;
            metadata.Author = "jwanagel";
            metadata.LastModifiedDate = DateTime.Parse("2008-01-20T08:55:13.330897Z");
            FolderMetaData folder1 = new FolderMetaData();
            folder1.Name = "A !@#$%^&()_-+={[}];',.~`";
            folder1.ItemRevision = 5734;
            folder1.LastModifiedDate = DateTime.Parse("2008-01-20T08:55:13.330897Z");
            folder1.Author = "jwanagel";
            metadata.Items.Add(folder1);
            DeleteFolderMetaData folder2 = new DeleteFolderMetaData();
            folder2.Name = "A !@#$%^&()_-+={[}];',.~`/B !@#$%^&()_-+={[}];',.~`";
            folder1.Items.Add(folder2);
            DeleteMetaData file1 = new DeleteMetaData();
            file1.Name = "A !@#$%^&()_-+={[}];',.~`/F !@#$%^&()_-+={[}];',.~`.txt";
            folder1.Items.Add(file1);
            FolderMetaData folder3 = new FolderMetaData();
            folder3.Name = "A !@#$%^&()_-+={[}];',.~`/T !@#$%^&()_-+={[}];',.~`";
            folder3.ItemRevision = 5734;
            folder3.LastModifiedDate = DateTime.Parse("2008-01-20T08:55:13.330897Z");
            folder3.Author = "jwanagel";
            folder1.Items.Add(folder3);
            ItemMetaData file2 = new ItemMetaData();
            file2.Name = "A !@#$%^&()_-+={[}];',.~`/G !@#$%^&()_-+={[}];',.~`.txt";
            file2.ItemRevision = 5734;
            file2.LastModifiedDate = DateTime.Parse("2008-01-20T08:55:13.330897Z");
            file2.Author = "jwanagel";
            folder1.Items.Add(file2);
            ItemMetaData file3 = new ItemMetaData();
            file3.Name = "A !@#$%^&()_-+={[}];',.~`/H !@#$%^&()_-+={[}];',.~`.txt";
            file3.ItemRevision = 5734;
            file3.LastModifiedDate = DateTime.Parse("2008-01-20T08:55:13.330897Z");
            file3.Author = "jwanagel";
            folder1.Items.Add(file3);
            stub.Attach(provider.GetChangedItems, metadata);
            Attach.MultipleReturnValues returnValues = new Attach.MultipleReturnValues();
            returnValues.Add(true);
            returnValues.Add(false);
            returnValues.Add(true);
            returnValues.Add(false);
            stub.Attach(provider.ItemExists, returnValues);
            byte[] fileData = Encoding.UTF8.GetBytes("1234abcd");
            stub.Attach(provider.ReadFile, fileData);

            string request =
                "REPORT /!svn/vcc/default HTTP/1.1\r\n" +
                "Host: localhost:8084\r\n" +
                "User-Agent: SVN/1.4.4 (r25188) neon/0.26.3\r\n" +
                "Connection: TE\r\n" +
                "TE: trailers\r\n" +
                "Content-Length: 186\r\n" +
                "Content-Type: text/xml\r\n" +
                "Accept-Encoding: svndiff1;q=0.9,svndiff;q=0.8\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Accept-Encoding: gzip\r\n" +
                "Authorization: Basic andhbmFnZWw6UGFzc0B3b3JkMQ==\r\n" +
                "\r\n" +
                "<S:update-report send-all=\"true\" xmlns:S=\"svn:\"><S:src-path>http://localhost:8084</S:src-path><S:target-revision>5734</S:target-revision><S:entry rev=\"5733\" ></S:entry></S:update-report>";

            string expected =
                "HTTP/1.1 200 OK\r\n" +
                "Date: Sun, 20 Jan 2008 08:56:19 GMT\r\n" +
                "Server: Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2\r\n" +
                "Transfer-Encoding: chunked\r\n" +
                "Content-Type: text/xml; charset=\"utf-8\"\r\n" +
                "\r\n" +
                "cb6\r\n" +
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n" +
                "<S:target-revision rev=\"5734\"/>\n" +
                "<S:open-directory rev=\"5733\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5734/</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5734</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2008-01-20T08:55:13.330897Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:open-directory name=\"A !@#$%^&amp;()_-+={[}];',.~`\" rev=\"5733\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5734/A%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5734</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2008-01-20T08:55:13.330897Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:delete-entry name=\"B !@#$%^&amp;()_-+={[}];',.~`\"/>\n" +
                "<S:delete-entry name=\"F !@#$%^&amp;()_-+={[}];',.~`.txt\"/>\n" +
                "<S:add-directory name=\"T !@#$%^&amp;()_-+={[}];',.~`\" bc-url=\"/!svn/bc/5734/A%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60/T%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5734/A%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60/T%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5734</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2008-01-20T08:55:13.330897Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                "<S:prop></S:prop>\n" +
                "</S:add-directory>\n" +
                "<S:open-file name=\"G !@#$%^&amp;()_-+={[}];',.~`.txt\" rev=\"5733\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5734/A%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60/G%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60.txt</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5734</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2008-01-20T08:55:13.330897Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                //"<S:txdelta>U1ZOAQAECAIJAYgIMTIzNGFiY2Q=\n" +
                "<S:txdelta>U1ZOAAAACAEIiDEyMzRhYmNk\n" +
                "</S:txdelta><S:prop><V:md5-checksum>ef73781effc5774100f87fe2f437a435</V:md5-checksum></S:prop>\n" +
                "</S:open-file>\n" +
                "<S:add-file name=\"H !@#$%^&amp;()_-+={[}];',.~`.txt\">\n" +
                "<D:checked-in><D:href>/!svn/ver/5734/A%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60/H%20!@%23$%25%5E&amp;()_-+=%7B%5B%7D%5D%3B',.~%60.txt</D:href></D:checked-in>\n" +
                "<S:set-prop name=\"svn:entry:committed-rev\">5734</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:committed-date\">2008-01-20T08:55:13.330897Z</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:last-author\">jwanagel</S:set-prop>\n" +
                "<S:set-prop name=\"svn:entry:uuid\">81a5aebe-f34e-eb42-b435-ac1ecbb335f7</S:set-prop>\n" +
                //"<S:txdelta>U1ZOAQAACAIJAYgIMTIzNGFiY2Q=\n" +
                "<S:txdelta>U1ZOAAAACAEIiDEyMzRhYmNk\n" +
                "</S:txdelta><S:prop><V:md5-checksum>ef73781effc5774100f87fe2f437a435</V:md5-checksum></S:prop>\n" +
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