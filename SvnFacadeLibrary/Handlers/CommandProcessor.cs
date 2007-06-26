using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SvnBridge.Exceptions;
using SvnBridge.Protocol;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class CommandProcessor
    {
        IHttpRequest _context;
        WebDavService _webDavService;

        public CommandProcessor(IHttpRequest context,
                                WebDavService webDavService)
        {
            _context = context;
            _webDavService = webDavService;
        }

        public void ProcessPropFindRequest()
        {
            PropFindData propfind = Helper.DeserializeXml<PropFindData>(_context.InputStream);

            try
            {
                SetResponseSettings("text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);
                if (_context.Headers["Label"] != null)
                {
                    _context.AddHeader("Vary", "Label");
                }
                _webDavService.PropFind(propfind, _context.Path, _context.Headers["Depth"], _context.Headers["Label"], _context.OutputStream);
            }
            catch (FileNotFoundException)
            {
                _context.StatusCode = (int)HttpStatusCode.NotFound;
                _context.ContentType = "text/html; charset=iso-8859-1";
                string server = _context.Headers["Host"].Split(':')[0];
                string port = _context.Headers["Host"].Split(':')[1];
                string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                  "<html><head>\n" +
                                  "<title>404 Not Found</title>\n" +
                                  "</head><body>\n" +
                                  "<h1>Not Found</h1>\n" +
                                  "<p>The requested URL " + Helper.Decode(_context.Path) + " was not found on this server.</p>\n" +
                                  "<hr>\n" +
                                  "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                                  "</body></html>\n";
                _context.Write(response);
            }
        }

        public void ProcessReportRequest()
        {
            using (XmlReader reader = XmlReader.Create(_context.InputStream, Helper.InitializeNewXmlReaderSettings()))
            {
                reader.MoveToContent();
                if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "get-locks-report")
                {
                    GetLocksReportData getlocksreport = Helper.DeserializeXml<GetLocksReportData>(reader);

                    SetResponseSettings("text/xml", Encoding.UTF8, 200);
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("D", WebDav.Namespaces.DAV);
                    //SerializeResponse<GetLocksReportData>(getlocksreport, ns, _context.OutputStream);
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "update-report")
                {
                    UpdateReportData request = Helper.DeserializeXml<UpdateReportData>(reader);
                    SetResponseSettings("text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    SendChunked();
                    using (StreamWriter output = new StreamWriter(_context.OutputStream))
                    {
                        _webDavService.UpdateReport(request, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "log-report")
                {
                    LogReportData request = Helper.DeserializeXml<LogReportData>(reader);
                    SetResponseSettings("text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    SendChunked();
                    using (StreamWriter output = new StreamWriter(_context.OutputStream))
                    {
                        _webDavService.LogReport(request, _context.Path, output);
                    }
                }
            }
        }

        public void ProcessOptionsRequest()
        {
            SetResponseSettings("text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
            _context.AddHeader("DAV", "1,2");
            _context.AddHeader("DAV", "version-control,checkout,working-resource");
            _context.AddHeader("DAV", "merge,baseline,activity,version-controlled-collection");
            _context.AddHeader("MS-Author-Via", "DAV");
            _context.AddHeader("Allow", "OPTIONS,GET,HEAD,POST,DELETE,TRACE,PROPFIND,PROPPATCH,COPY,MOVE,LOCK,UNLOCK,CHECKOUT");
            _webDavService.Options(_context.Path, _context.OutputStream);
        }

        public void ProcessMkActivityRequest()
        {
            _webDavService.MkActivity(_context.Path);
            string server = _context.Headers["Host"].Split(':')[0];
            string port = _context.Headers["Host"].Split(':')[1];
            SetResponseSettings("text/html", Encoding.UTF8, 201);
            _context.AddHeader("Cache-Control", "no-cache");
            _context.AddHeader("Location", "http://" + _context.Headers["Host"] + _context.Path);
            _context.AddHeader("X-Pad", "avoid browser bug");
            string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                              "<html><head>\n" +
                              "<title>201 Created</title>\n" +
                              "</head><body>\n" +
                              "<h1>Created</h1>\n" +
                              "<p>Activity " + _context.Path + " has been created.</p>\n" +
                              "<hr />\n" +
                              "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                              "</body></html>\n";
            _context.Write(response);
        }

        public void ProcessCheckoutRequest()
        {
            CheckoutData request = Helper.DeserializeXml<CheckoutData>(_context.InputStream);
            try
            {
                string location = _webDavService.CheckOut(request, _context.Path, _context.Headers["Host"]);
                string server = _context.Headers["Host"].Split(':')[0];
                string port = _context.Headers["Host"].Split(':')[1];
                SetResponseSettings("text/html", Encoding.UTF8, 201);
                _context.AddHeader("Cache-Control", "no-cache");
                _context.AddHeader("Location", "http://" + _context.Headers["Host"] + location);
                string response =
                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                    "<html><head>\n" +
                    "<title>201 Created</title>\n" +
                    "</head><body>\n" +
                    "<h1>Created</h1>\n" +
                    "<p>Checked-out resource " + location + " has been created.</p>\n" +
                    "<hr />\n" +
                    "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                    "</body></html>\n";
                _context.Write(response);
            }
            catch (ConflictException)
            {
                SetResponseSettings("text/xml; charset=\"utf-8\"", Encoding.UTF8, 409);
                string response =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                    "<D:error xmlns:D=\"DAV:\" xmlns:m=\"http://apache.org/dav/xmlns\" xmlns:C=\"svn:\">\n" +
                    "<C:error/>\n" +
                    "<m:human-readable errcode=\"160024\">\n" +
                    "The version resource does not correspond to the resource within the transaction.  Either the requested version resource is out of date (needs to be updated), or the requested version resource is newer than the transaction root (restart the commit).\n" +
                    "</m:human-readable>\n" +
                    "</D:error>\n";
                _context.Write(response);
            }
        }

        public void ProcessPropPatchRequest()
        {
            PropertyUpdateData request = Helper.DeserializeXml<PropertyUpdateData>(_context.InputStream);
            SetResponseSettings("text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);
            using (StreamWriter output = new StreamWriter(_context.OutputStream))
            {
                _webDavService.PropPatch(request, _context.Path, output);
            }
        }

        public void ProcessPutRequest()
        {
            _webDavService.Put(_context.Path, _context.InputStream, _context.Headers["X-SVN-Base-Fulltext-MD5"], _context.Headers["X-SVN-Result-Fulltext-MD5"]);

            string server = _context.Headers["Host"].Split(':')[0];
            string port = _context.Headers["Host"].Split(':')[1];
            if (_context.Headers["X-SVN-Base-Fulltext-MD5"] == null)
            {
                SetResponseSettings("text/html", Encoding.UTF8, 201);
                _context.AddHeader("Location", "http://" + _context.Headers["Host"] + Helper.Decode(_context.Path));
                string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                  "<html><head>\n" +
                                  "<title>201 Created</title>\n" +
                                  "</head><body>\n" +
                                  "<h1>Created</h1>\n" +
                                  "<p>Resource " + Helper.Decode(_context.Path) + " has been created.</p>\n" +
                                  "<hr />\n" +
                                  "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                                  "</body></html>\n";
                _context.Write(response);
            }
            else
            {
                SetResponseSettings("text/plain", Encoding.UTF8, 204);
            }
        }

        public void ProcessMergeRequest()
        {
            MergeData request = Helper.DeserializeXml<MergeData>(_context.InputStream);

            SetResponseSettings("text/xml", Encoding.UTF8, 200);
            _context.AddHeader("Cache-Control", "no-cache");
            SendChunked();
            using (StreamWriter output = new StreamWriter(_context.OutputStream))
            {
                _webDavService.Merge(request, _context.Path, output);
            }
        }

        public void ProcessDeleteRequest()
        {
            bool fileDeleted = _webDavService.Delete(_context.Path);

            if (fileDeleted)
            {
                SetResponseSettings("text/plain", Encoding.UTF8, 204);
            }
            else
            {
                SetResponseSettings("text/html; charset=iso-8859-1", Encoding.UTF8, 404);
                string server = _context.Headers["Host"].Split(':')[0];
                string port = _context.Headers["Host"].Split(':')[1];
                string response =
                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                    "<html><head>\n" +
                    "<title>404 Not Found</title>\n" +
                    "</head><body>\n" +
                    "<h1>Not Found</h1>\n" +
                    "<p>The requested URL " + Helper.Decode(_context.Path) + " was not found on this server.</p>\n" +
                    "<hr>\n" +
                    "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                    "</body></html>\n";
                _context.Write(response);
            }
        }

        public void ProcessMkColRequest()
        {
            _webDavService.MkCol(_context.Path, _context.Headers["Host"]);

            string path = Helper.Decode(_context.Path);
            string server = _context.Headers["Host"].Split(':')[0];
            string port = _context.Headers["Host"].Split(':')[1];
            SetResponseSettings("text/html", Encoding.UTF8, 201);
            _context.AddHeader("Location", "http://" + _context.Headers["Host"] + path);
            string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                              "<html><head>\n" +
                              "<title>201 Created</title>\n" +
                              "</head><body>\n" +
                              "<h1>Created</h1>\n" +
                              "<p>Collection " + path + " has been created.</p>\n" +
                              "<hr />\n" +
                              "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                              "</body></html>\n";
            _context.Write(response);
        }

        public void SendUnauthorizedResponse()
        {
            _context.StatusCode = (int)HttpStatusCode.Unauthorized;
            _context.ContentType = "text/html; charset=iso-8859-1";
            _context.RemoveHeader("DAV");
            _context.RemoveHeader("MS-Author-Via");
            _context.RemoveHeader("Allow");
            _context.AddHeader("WWW-Authenticate", "Basic realm=\"CodePlex Subversion Repository\"");
            string server = _context.Headers["Host"].Split(':')[0];
            string port = _context.Headers["Host"].Split(':')[1];
            string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
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
                              "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                              "</body></html>\n";
            _context.Write(response);
        }

        void SendChunked()
        {
            _context.AddHeader("Transfer-Encoding", "chunked");
            _context.SendChunked = true;
        }

        void SetResponseSettings(string contentType,
                                 Encoding contentEncoding,
                                 int status)
        {
            _context.ContentType = contentType;
            _context.ContentEncoding = contentEncoding;
            _context.StatusCode = status;
        }
    }
}
