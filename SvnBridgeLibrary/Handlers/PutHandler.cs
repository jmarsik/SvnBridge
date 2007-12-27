using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Utility;
using System.IO;
using System;

namespace SvnBridge.Handlers
{
    public class PutHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);
            bool created = Put(sourceControlProvider, path, request.InputStream, request.Headers["X-SVN-Base-Fulltext-MD5"], request.Headers["X-SVN-Result-Fulltext-MD5"]);

            if (created)
            {
                SetResponseSettings(response, "text/html", Encoding.UTF8, 201);

                response.AppendHeader("Location", "http://" + request.Headers["Host"] + "/" + Helper.Decode(path));

                string responseContent = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                         "<html><head>\n" +
                                         "<title>201 Created</title>\n" +
                                         "</head><body>\n" +
                                         "<h1>Created</h1>\n" +
                                         "<p>Resource /" + Helper.Decode(path) + " has been created.</p>\n" +
                                         "<hr />\n" +
                                         "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " + request.Url.Port + "</address>\n" +
                                         "</body></html>\n";

                WriteToResponse(response, responseContent);
            }
            else
            {
                SetResponseSettings(response, "text/plain", Encoding.UTF8, 204);
            }
        }

        private bool Put(ISourceControlProvider sourceControlProvider, string path, Stream inputStream, string baseHash, string resultHash)
        {
            if (!path.StartsWith("//"))
                path = "/" + path;

            string activityId = path.Substring(11, path.IndexOf('/', 11) - 11);
            string serverPath = Helper.Decode(path.Substring(11 + activityId.Length));
            SvnDiff[] diffs = SvnDiffParser.ParseSvnDiff(inputStream);
            byte[] fileData = new byte[0];
            byte[] sourceData = new byte[0];
            if (baseHash != null)
            {
                ItemMetaData item = sourceControlProvider.GetItemInActivity(activityId, serverPath);
                sourceData = sourceControlProvider.ReadFile(item);
                if (Helper.GetMd5Checksum(sourceData) != baseHash)
                    throw new Exception("Checksum mismatch with base file");
            }
            if (diffs.Length > 0)
            {
                int sourceDataStartIndex = 0;
                foreach (SvnDiff diff in diffs)
                {
                    byte[] newData = SvnDiffEngine.ApplySvnDiff(diff, sourceData, sourceDataStartIndex);
                    sourceDataStartIndex += newData.Length;
                    Helper.ReDim(ref fileData, fileData.Length + newData.Length);
                    Array.Copy(newData, 0, fileData, fileData.Length - newData.Length, newData.Length);
                }
                if (Helper.GetMd5Checksum(fileData) != resultHash)
                    throw new Exception("Checksum mismatch with new file");
            }
            return sourceControlProvider.WriteFile(activityId, serverPath, fileData);
        }
    }
}