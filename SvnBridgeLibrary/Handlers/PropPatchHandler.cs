using System.IO;
using System.Text;
using System.Xml;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
	public class PropPatchHandler : HttpContextHandlerBase
	{
		protected override void Handle(IHttpContext context,
									   ISourceControlProvider sourceControlProvider)
		{
			IHttpRequest request = context.Request;
			IHttpResponse response = context.Response;

			string path = GetPath(request);
			string correctXml;
			using (StreamReader sr = new StreamReader(request.InputStream))
			{
				correctXml = BrokenXml.Escape(sr.ReadToEnd());
			}
			PropertyUpdateData data = Helper.DeserializeXml<PropertyUpdateData>(correctXml);
			SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

			using (StreamWriter output = new StreamWriter(response.OutputStream))
			{
				PropPatch(sourceControlProvider, data, path, output);
			}
		}

		private void PropPatch(ISourceControlProvider sourceControlProvider,
							   PropertyUpdateData request,
							   string path,
							   TextWriter output)
		{
			string activityPath = path.Substring(10);
			if (activityPath.StartsWith("/"))
			{
				activityPath = activityPath.Substring(1);
			}

			string itemPath = Helper.Decode(activityPath.Substring(activityPath.IndexOf('/')));
			string activityId = activityPath.Split('/')[0];
			if (request.Set.Prop.Properties.Count > 0)
			{
				if (request.Set.Prop.Properties[0].LocalName == "log")
					OutputLogResponse(path, request, sourceControlProvider, activityId, output);
				else
					OutputSetPropertiesResponse(path, request, sourceControlProvider, activityId, output, itemPath);
			}
			else if (request.Remove.Prop.Properties.Count > 0)
			{
				output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
				output.Write(
					"<D:multistatus xmlns:D=\"DAV:\" xmlns:ns3=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns2=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
				output.Write("<D:response>\n");
				output.Write("<D:href>" + GetLocalPath("/"+ Helper.Encode(path)) + "</D:href>\n");
				output.Write("<D:propstat>\n");

				foreach (XmlElement element in request.Remove.Prop.Properties)
				{
					sourceControlProvider.RemoveProperty(activityId, itemPath, GetPropertyName(element));
					OutputElement(output, element);
				}

				output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
				output.Write("</D:propstat>\n");
				output.Write("</D:response>\n");
				output.Write("</D:multistatus>\n");
			}
		}

		private static string GetPropertyName(XmlElement element)
		{
			string propertyName = element.LocalName;
			if (element.NamespaceURI == WebDav.Namespaces.TIGRISSVN)
				propertyName = "svn:" + propertyName;
			return propertyName;
		}

		private void OutputSetPropertiesResponse(string path, PropertyUpdateData request, ISourceControlProvider sourceControlProvider, string activityId, TextWriter output, string itemPath)
		{
			foreach (XmlElement prop in request.Set.Prop.Properties)
			{
				sourceControlProvider.SetProperty(activityId,
												  itemPath,
												  GetPropertyName(prop),
												  prop.InnerText);
			}
			output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
			output.Write(
				"<D:multistatus xmlns:D=\"DAV:\" xmlns:ns3=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns2=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
			output.Write("<D:response>\n");
			output.Write("<D:href>" + GetLocalPath("/"+Helper.Encode(path)) + "</D:href>\n");
			output.Write("<D:propstat>\n");
			foreach (XmlElement element in request.Set.Prop.Properties)
			{
				OutputElement(output, element);
			}
			output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
			output.Write("</D:propstat>\n");
			output.Write("</D:response>\n");
			output.Write("</D:multistatus>\n");
		}

		private static void OutputElement(TextWriter output, XmlElement element)
		{
			output.Write("<D:prop>\n");
			if (element.NamespaceURI == WebDav.Namespaces.SVNDAV)
				output.Write("<ns3:" + element.LocalName + "/>\r\n");
			else if (element.NamespaceURI == WebDav.Namespaces.TIGRISSVN)
				output.Write("<ns1:" + element.LocalName + "/>\r\n");
			else if (element.NamespaceURI == WebDav.Namespaces.DAV)
				output.Write("<ns0:" + element.LocalName + "/>\r\n");
			else//custome
				output.Write("<ns2:" + element.LocalName + "/>\r\n");
			output.Write("</D:prop>\n");
		}

		private void OutputLogResponse(string path, PropertyUpdateData request, ISourceControlProvider sourceControlProvider, string activityId, TextWriter output)
		{
			sourceControlProvider.SetActivityComment(activityId, request.Set.Prop.Properties[0].InnerText);
			output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
			output.Write(
				"<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
			output.Write("<D:response>\n");
			output.Write("<D:href>" + GetLocalPath("/"+path) + "</D:href>\n");
			output.Write("<D:propstat>\n");
			output.Write("<D:prop>\n");
			output.Write("<ns1:log/>\r\n");
			output.Write("</D:prop>\n");
			output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
			output.Write("</D:propstat>\n");
			output.Write("</D:response>\n");
			output.Write("</D:multistatus>\n");
		}
	}
}