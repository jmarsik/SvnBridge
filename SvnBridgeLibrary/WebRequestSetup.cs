using System;
using System.Net;

namespace SvnBridge
{
	public static class WebRequestSetup
	{
		public static void OnWebRequest(WebRequest request)
		{
			HttpWebRequest httpWebRequest = request as HttpWebRequest;
			if(httpWebRequest!=null)
			{
				OnHttpWebRequest(httpWebRequest);
			}
		}

		public static void OnHttpWebRequest(HttpWebRequest request)
		{
			request.UnsafeAuthenticatedConnectionSharing = false;
			request.ServicePoint.ConnectionLimit = 5000;
			request.ConnectionGroupName = Guid.NewGuid().ToString();
		}
	}
}