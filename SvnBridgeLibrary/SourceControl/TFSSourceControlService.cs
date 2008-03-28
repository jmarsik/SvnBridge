using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
	public class TFSSourceControlService : SourceControlService, ITFSSourceControlService
	{
		private readonly ICache cache;
		private RepositoryFactoryHelper repositoryFactoryHelper;

		public TFSSourceControlService(IRegistrationService registrationService,
		                               IRepositoryWebSvcFactory webSvcFactory,
		                               IWebTransferService webTransferService,
		                               IFileSystem fileSystem,
		                               ICache cache)
			: base(registrationService, webSvcFactory, webTransferService, fileSystem)
		{
			repositoryFactoryHelper = new RepositoryFactoryHelper(webSvcFactory);
			this.cache = cache;
		}

		#region ITFSSourceControlService Members

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

		#endregion
	}
}