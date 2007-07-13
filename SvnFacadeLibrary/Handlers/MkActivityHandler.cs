using System;
using System.Collections.Generic;
using System.Text;

namespace SvnBridge.Handlers
{
    public class MkActivityHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "mkactivity"; }
        }

        protected override void Handle(IHttpRequest request, WebDavService webDavService)
        {
            webDavService.MkActivity(request.Path);

            string server = request.Headers["Host"].Split(':')[0];
            string port = request.Headers["Host"].Split(':')[1];
            
            SetResponseSettings(request, "text/html", Encoding.UTF8, 201);
            
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Location", "http://" + request.Headers["Host"] + request.Path);
            request.AddHeader("X-Pad", "avoid browser bug");
            
            string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                              "<html><head>\n" +
                              "<title>201 Created</title>\n" +
                              "</head><body>\n" +
                              "<h1>Created</h1>\n" +
                              "<p>Activity " + request.Path + " has been created.</p>\n" +
                              "<hr />\n" +
                              "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                              "</body></html>\n";
            
            request.Write(response);
        }
    }
}
