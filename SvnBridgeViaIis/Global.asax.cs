using System;
using System.Net;
using System.Web;

namespace SvnBridge
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        	try
        	{
        		new BootStrapper().Start();
        	}
        	catch (WebException we)
        	{
				Console.WriteLine(we);
        	}
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}