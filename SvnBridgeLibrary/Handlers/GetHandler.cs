using System;
using System.IO;
using System.Text;
using System.Web.UI;
using SvnBridge.Handlers.Renderers;
using SvnBridge.Infrastructure;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
	public class GetHandler : HttpContextHandlerBase
	{
		protected override void Handle(IHttpContext context,
									   ISourceControlProvider sourceControlProvider)
		{
			IHttpRequest request = context.Request;
			IHttpResponse response = context.Response;

			string requestPath = GetPath(request);
			int itemVersion = 0;
			string itemPath = null;
            
			if (requestPath.StartsWith("/!svn/bc/"))
			{
				string[] parts = requestPath.Split('/');
				if (parts.Length >= 3)
					int.TryParse(parts[3], out itemVersion);

				itemPath = Helper.Decode(requestPath.Substring(9 + itemVersion.ToString().Length));
			}

			if (itemVersion == 0)
				itemVersion = sourceControlProvider.GetLatestVersion();

			itemPath = itemPath ?? requestPath.Substring(1);//remove first '/'

			ItemMetaData item = sourceControlProvider.GetItemsWithoutProperties(itemVersion, itemPath, Recursion.OneLevel);
			FolderMetaData folder = item as FolderMetaData;
			if (folder != null)
			{
			    folder = (FolderMetaData) item;
				new FolderRenderer(context, PathParser,Credentials).Render(folder);
			}
			else if (item == null)
			{
				context.Response.StatusCode = 404;
				using (StreamWriter writer = new StreamWriter(response.OutputStream))
				{
					writer.Write("Could not find path: " + requestPath);
				}
			}
			else
			{
				SetResponseSettings(response, "text/plain", Encoding.Default, 200);
				string itemData = Encoding.UTF8.GetString(sourceControlProvider.ReadFile(item));

				using (StreamWriter writer = new StreamWriter(response.OutputStream))
				{
					writer.Write(itemData);
				}
			}
		}
	}
}