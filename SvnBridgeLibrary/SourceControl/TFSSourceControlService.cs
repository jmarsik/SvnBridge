using System;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using System.Text;
using System.IO;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
	public class TFSSourceControlService : SourceControlService, ITFSSourceControlService
	{
		private readonly ILogger logger;
		private readonly IRepositoryWebSvcFactory webSvcFactory;

		public TFSSourceControlService(
			IRegistrationService registrationService, 
			IRepositoryWebSvcFactory webSvcFactory, 
			IWebTransferService webTransferService, 
			IFileSystem fileSystem,
			ILogger logger)
			: base(registrationService, webSvcFactory, webTransferService, fileSystem)
		{
			this.webSvcFactory = webSvcFactory;
			this.logger = logger;
		}

		public ExtendedItem[][] QueryItemsExtended(string tfsUrl,
		                                           ICredentials credentials,
		                                           string workspaceName,
		                                           ItemSpec[] items,
		                                           DeletedState deletedState,
		                                           ItemType itemType)
		{
			Repository webSvc = CreateProxy(tfsUrl, credentials);
			string username = TfsUtil.GetUsername(credentials, tfsUrl);
			return webSvc.QueryItemsExtended(workspaceName, username, items, deletedState, itemType);
		}

		private Repository CreateProxy(string tfsUrl, ICredentials credentials)
		{
			return (Repository)webSvcFactory.Create(tfsUrl, credentials);
		}

		public BranchRelative[][] QueryBranches(string tfsUrl,
		                                        ICredentials credentials,
		                                        string workspaceName,
		                                        ItemSpec[] items,
		                                        VersionSpec version)
		{
			Repository webSvc = CreateProxy(tfsUrl, credentials);
			string username = TfsUtil.GetUsername(credentials, tfsUrl);
			return webSvc.QueryBranches(workspaceName, username, items, version);
		}

		public SourceItem QueryItems(string tfsUrl, ICredentials credentials, int itemIds, int changeSet)
		{
			SourceItem[] items = QueryItems(tfsUrl, credentials, new int[]{itemIds}, changeSet);
			if(items.Length==0)
				return null;
			return items[0];
		}

        public SourceItemReader QueryItemsReader(string tfsUrl, ICredentials credentials, string serverPath, RecursionType recursion, VersionSpec version)
        {
            string versionXml = "<version xsi:type=\"LatestVersionSpec\" />";
            if (version is ChangesetVersionSpec)
                versionXml = "<version xsi:type=\"ChangesetVersionSpec\" cs=\"" + ((ChangesetVersionSpec)version).cs + "\" />";

            string itemsXml = "";
            if (serverPath != null)
            {
                itemsXml = "<items><ItemSpec recurse=\"" + recursion + "\" item=\"" + serverPath + "\" /></items>";
            }

            string postData =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soap:Body><QueryItems xmlns=\"http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03\">" + itemsXml + versionXml + "<deletedState>NonDeleted</deletedState><itemType>Any</itemType><generateDownloadUrls>true</generateDownloadUrls></QueryItems></soap:Body></soap:Envelope>";
            byte[] data = Encoding.ASCII.GetBytes(postData);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(tfsUrl + "/VersionControl/v1.0/repository.asmx");
            request.Headers["SOAPAction"] = "\"http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03/QueryItems\"";
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.ContentLength = data.Length;
            request.Credentials = credentials;
            request.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            request.UserAgent = "CodePlexClient";
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();

        	try
        	{
        		return new SourceItemReader(tfsUrl, request.GetResponse().GetResponseStream());
        	}
        	catch (WebException e)
        	{
				logger.Error("Could not query items from server.", e);
        		throw;
        	}
        }
 	}
}