using System.IO;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers.Renderers
{
    internal class FolderRenderer
    {
        private readonly string serverUrl;
        private readonly StreamWriter writer;
        private readonly string applicationPath;

        public FolderRenderer(string serverUrl, IHttpContext context)
        {
            this.serverUrl = serverUrl;
            applicationPath = context.Request.ApplicationPath;
            writer = new StreamWriter(context.Response.OutputStream);
        }

        public void Render(FolderMetaData folder)
        {
            writer.WriteLine("<html>");
            writer.Write("<title>");
            writer.Write(GetFolderName(folder));
            writer.WriteLine("</title>");
            writer.Write("<body>");
            writer.Write("<h1>Contents of ");
            writer.Write(GetFolderName(folder));
            writer.WriteLine("</h1>");
            writer.Write("<ul>");
            writer.Write("<li><a href='..'>..</a></li>");
            foreach (ItemMetaData item in folder.Items)
            {
                writer.Write("<li><a href='");
                writer.Write(applicationPath);
                writer.Write("/");
                writer.Write(item.Name);
                writer.WriteLine("'>");
                writer.Write(item.Name);
                writer.WriteLine("</a></li>");
            }
            writer.WriteLine("</ul>");

            writer.Write("</body>");
            writer.WriteLine("</html>");
            writer.Flush();
        }

        private string GetFolderName(ItemMetaData folder)
        {
           return serverUrl + "/" + folder.Name;
        }
    }
}