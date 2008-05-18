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
            bool cacheEnabled = bool.Parse(ConfigurationManager.AppSettings["CacheEnabled"]);
            new BootStrapper(cacheEnabled).Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}