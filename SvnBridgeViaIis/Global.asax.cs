using System;
using System.Net;
using System.Web;
using System.Configuration;

namespace SvnBridge
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new BootStrapper().Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}