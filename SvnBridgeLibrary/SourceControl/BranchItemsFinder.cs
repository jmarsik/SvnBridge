using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Utility;

namespace SvnBridge.SourceControl
{
	public class BranchItemsFinder
	{
		private readonly ITFSSourceControlService sourceControlService;
		private readonly SourceItemHistory history;
		private readonly string serverUrl;
		private readonly string rootPath;
		private readonly ICredentials credentials;
		private bool initialized = false;
		private readonly IDictionary<SourceItemChange, BranchRelative[][]> branchesToItems = 
			new Dictionary<SourceItemChange, BranchRelative[][]>();

		public BranchItemsFinder(ITFSSourceControlService sourceControlService,
		                         SourceItemHistory history, 
		                         string serverUrl,
		                         string rootPath, 
		                         ICredentials credentials)
		{
			this.sourceControlService = sourceControlService;
			this.history = history;
			this.serverUrl = serverUrl;
			this.rootPath = rootPath;
			this.credentials = credentials;
		}

		public BranchRelative[][] FindBranchesFor(SourceItemChange change)
		{
			if (initialized == false)
			{
				LoadBranches();
				initialized = true;
			}
			BranchRelative[][] branches = FilterBranchesFor(change);
			return branches;
		}

		private BranchRelative[][] FilterBranchesFor(SourceItemChange change)
		{
			return branchesToItems[change];
		}

		private void LoadBranches()
		{
			ChangesetVersionSpec branchChangeset = new ChangesetVersionSpec();
			branchChangeset.cs = history.ChangeSetID;
			foreach (SourceItemChange change in history.Changes)
			{
				if((change.ChangeType & ChangeType.Branch) != ChangeType.Branch)
					continue;

				ItemSpec spec = new ItemSpec();
				spec.item = Helper.CombinePath(rootPath, change.Item.RemoteName);
				if (spec.item.StartsWith("$"))
					spec.item = spec.item.Substring(1);
				BranchRelative[][] branches = sourceControlService.QueryBranches(serverUrl,
				                                                                 credentials,
				                                                                 null,
				                                                                 new ItemSpec[]{spec}, 
				                                                                 branchChangeset);
				branchesToItems.Add(change, branches);
			}
		
		}
	}
}