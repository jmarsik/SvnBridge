using System;
using System.IO;
using System.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    /// <summary>
    /// This implementation is probably not the best, but we had have two problems with it.
    /// First, we can't take dependencies on the TFS API, we would need to redistribute it with us, and 
    /// that is problematic. The second is that the ClientService API is complex and undocumented, which 
    /// means that it is actually easier to use this approach than through the SOAP proxy.
    /// </summary>
    public class AssociateWorkItemWithChangeSet : IAssociateWorkItemWithChangeSet
    {
        private readonly static string associateWorkItemWithChangeSetMessage;

        private readonly string serverUrl;
        private readonly ICredentials credentials;


        public AssociateWorkItemWithChangeSet(string serverUrl, ICredentials credentials)
        {
            this.serverUrl = serverUrl;
            this.credentials = CredentialsHelper.GetCredentialsForServer(serverUrl, credentials);
        }

        static AssociateWorkItemWithChangeSet()
        {
            using (Stream stream = typeof (AssociateWorkItemWithChangeSet).Assembly.GetManifestResourceStream(
                "SvnBridge.Infrastructure.AssociateWorkItemWithChangeSetMessage.xml"))
            {
                associateWorkItemWithChangeSetMessage = new StreamReader(stream).ReadToEnd();
            }
        }

        public void Associate(int workItemId, int changeSetId)
        {
            HttpWebRequest request =
                (HttpWebRequest)
                WebRequest.Create(serverUrl + "/WorkItemTracking/v1.0/ClientService.asmx");
            request.UserAgent = "Team Foundation";
            request.Headers.Add("X-TFS-Version", "1.0.0.0");
            request.ContentType =
                "application/soap+xml; charset=utf-8; action=\"http://schemas.microsoft.com/TeamFoundation/2005/06/WorkItemTracking/ClientServices/03/Update\"";

            request.Credentials = credentials;
            request.Method = "POST";
            using (Stream stream = request.GetRequestStream())
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    string text =
                        associateWorkItemWithChangeSetMessage
                            .Replace("{Guid}", Guid.NewGuid().ToString())
                            .Replace("{WorkItemId}", workItemId.ToString())
                            .Replace("{ChangeSetId}", changeSetId.ToString());

                    sw.Write(text);
                }
            }
            try
            {
                // we don't care about the response from here
                request.GetResponse().Close();
            }
            catch(WebException we)
            {
                using(Stream stream = we.Response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    throw new InvalidOperationException("Failed to associated work item "+ workItemId + " with changeset " + changeSetId + Environment.NewLine + reader.ReadToEnd(), we);
                }
            }
        }
    }
}