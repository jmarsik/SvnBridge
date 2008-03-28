using System;
using System.IO;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;
using Xunit;

namespace IntegrationTests
{
	public class MetaDataRepositoryTest : TFSSourceControlProviderTestsBase
	{
		private readonly ISourceControlService sourceControlService;
		private readonly MetaDataRepository repository;
		private readonly ICredentials credentials;

		public MetaDataRepositoryTest()
		{
			new BootStrapper().Start();
			credentials = GetCredentials();
			sourceControlService = IoC.Resolve<ISourceControlService>();
			string dbFile = "Cache.sdf";
			File.Delete(dbFile);
			repository = new MetaDataRepository(sourceControlService, credentials, ServerUrl,
			                                    Constants.ServerRootPath + PROJECT_NAME,
												@"Data Source=" + dbFile);
			repository.CreateDatabase();
		}

		[Fact]
		public void WhenAskToGetItemOnServer_WillCacheEntireRevision()
		{
			WriteFile(testPath + "/Test.txt", "blah", true);

			Assert.False(repository.IsInCache(_lastCommitRevision));

			repository.QueryItems(_lastCommitRevision, testPath + "/Test.txt", Recursion.None);

			Assert.True(repository.IsInCache(_lastCommitRevision));
		}

		[Fact]
		public void WillRaiseEventWhenCacheIsUpdated()
		{
			bool startEventCalled = false, endEventCalled = false;

			Events.CachingRevisionAction startingCachingRevision = delegate { startEventCalled = true; };
			Events.StartingCachingRevision += startingCachingRevision;

			Events.CachingRevisionAction finishedCachingRevision = delegate { endEventCalled = true; };
			Events.FinishedCachingRevision += finishedCachingRevision;

			repository.QueryItems(_lastCommitRevision, testPath + "/Test.txt", Recursion.None);

			Assert.True(startEventCalled);
			Assert.True(endEventCalled);
		}


		[Fact]
		public void CanGetValidResultFromQueryItems_RecursionNone()
		{
			WriteFile(testPath + "/Test.txt", "blah", true);
			
			SourceItem[] items = repository.QueryItems(_lastCommitRevision, testPath + "/Test.txt", Recursion.None);
			SourceItem[] sourceItems = sourceControlService.QueryItems(
				ServerUrl, 
				credentials,
				Constants.ServerRootPath + PROJECT_NAME + testPath + "/Test.txt", 
				RecursionType.None,
				VersionSpec.FromChangeset(_lastCommitRevision), 
				DeletedState.Any, 
				ItemType.Any);

			AssertEquals(sourceItems, items);
		}

		[Fact]
		public void CanGetValidResultFromQueryItems_RecursionOneLevel()
		{
			WriteFile(testPath + "/Test.txt", "blah", true);

			SourceItem[] items = repository.QueryItems(_lastCommitRevision, "", Recursion.OneLevel);
			SourceItem[] sourceItems = sourceControlService.QueryItems(
				ServerUrl,
				credentials,
				Constants.ServerRootPath + PROJECT_NAME,
				RecursionType.OneLevel,
				VersionSpec.FromChangeset(_lastCommitRevision),
				DeletedState.NonDeleted,
				ItemType.Any);

			AssertEquals(sourceItems, items);
		}

		[Fact]
		public void CanGetValidResultFromQueryItems_RecursionFull()
		{
			WriteFile(testPath + "/Test.txt", "blah", true);

			SourceItem[] items = repository.QueryItems(_lastCommitRevision, "", Recursion.Full);
			SourceItem[] sourceItems = sourceControlService.QueryItems(
				ServerUrl,
				credentials,
				Constants.ServerRootPath + PROJECT_NAME,
				RecursionType.Full,
				VersionSpec.FromChangeset(_lastCommitRevision),
				DeletedState.NonDeleted,
				ItemType.Any);

			AssertEquals(sourceItems, items);
		}

		private void AssertEquals(SourceItem[] sourceItems, SourceItem[] items)
		{
			Assert.Equal(sourceItems.Length, items.Length);
			for (int i = 0; i < items.Length; i++)
			{
				Assert.Equal(sourceItems[i].RemoteName, items[i].RemoteName);
				Assert.Equal(sourceItems[i].RemoteDate, items[i].RemoteDate);

				Assert.Equal(sourceItems[i].ItemId, items[i].ItemId);
				Assert.Equal(sourceItems[i].ItemType, items[i].ItemType);
				Assert.Equal(sourceItems[i].RemoteChangesetId, items[i].RemoteChangesetId);

				WebClient client = new WebClient();
				client.Credentials = credentials;

				if(items[i].ItemType == ItemType.Folder)
					continue;

				Assert.Equal(
					client.DownloadString(sourceItems[i].DownloadUrl),
					client.DownloadString(items[i].DownloadUrl)
					);
			}
		}
	}
}