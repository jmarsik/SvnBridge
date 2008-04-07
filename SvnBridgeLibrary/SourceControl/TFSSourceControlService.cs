using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using System.Text;
using System.IO;

namespace SvnBridge.SourceControl
{
	public class TFSSourceControlService : SourceControlService, ITFSSourceControlService
	{
		private readonly RepositoryFactoryHelper repositoryFactoryHelper;

		public TFSSourceControlService(IRegistrationService registrationService, IRepositoryWebSvcFactory webSvcFactory, IWebTransferService webTransferService, IFileSystem fileSystem)
			: base(registrationService, webSvcFactory, webTransferService, fileSystem)
		{
			repositoryFactoryHelper = new RepositoryFactoryHelper(webSvcFactory);
		}

		public ExtendedItem[][] QueryItemsExtended(string tfsUrl,
		                                           ICredentials credentials,
		                                           string workspaceName,
		                                           ItemSpec[] items,
		                                           DeletedState deletedState,
		                                           ItemType itemType)
		{
			Repository webSvc = repositoryFactoryHelper.TryCreateProxy(tfsUrl, credentials);
			string username = TfsUtil.GetUsername(credentials, tfsUrl);
			return webSvc.QueryItemsExtended(workspaceName, username, items, deletedState, itemType);
		}

		public BranchRelative[][] QueryBranches(string tfsUrl,
		                                        ICredentials credentials,
		                                        string workspaceName,
		                                        ItemSpec[] items,
		                                        VersionSpec version)
		{
			Repository webSvc = repositoryFactoryHelper.TryCreateProxy(tfsUrl, credentials);
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
                itemsXml = "<items><ItemSpec recurse=\"" + recursion.ToString() + "\" item=\"" + serverPath + "\" /></items>";
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
            request.UserAgent = "CodePlexClient";
            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();

            return new SourceItemReader(tfsUrl, request.GetResponse().GetResponseStream());
        }
 	}
}