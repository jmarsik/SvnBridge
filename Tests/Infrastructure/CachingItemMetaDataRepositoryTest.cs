using System;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class CachingItemMetaDataRepositoryTest : IDisposable
    {
        #region Setup/Teardown

        public CachingItemMetaDataRepositoryTest()
        {
            mocks = new MockRepository();
            mockCache = mocks.DynamicMock<ICache>();
            mockSourceControlProvider = mocks.DynamicMock<ISourceControlProvider>();

            repository = new CachingItemMetaDataRepository(mockSourceControlProvider, mockCache);
        }

        public void Dispose()
        {
            mocks.VerifyAll();
        }

        #endregion

        private ICache mockCache;
        private ISourceControlProvider mockSourceControlProvider;
        private MockRepository mocks;
        private CachingItemMetaDataRepository repository;

        [Fact]
        public void GetChangedItems_WhenCanFindItemInCache_WillSkipCallingToSourceCodeProvider()
        {
            SetupResult.For(mockCache.Get(null)).IgnoreArguments().Return(new CachedResult(new FolderMetaData()));

            UpdateReportData reportData = new UpdateReportData();
            DoNotExpect.Call(mockSourceControlProvider.GetChangedItems("blah", 5, 10, reportData));

            mocks.ReplayAll();

            FolderMetaData items = repository.GetChangedItems("blah", 5, 10, reportData);
            Assert.NotNull(items);
        }

        [Fact]
        public void GetChangedItems_WhenCannotFindItemInCache_WillCallSourceCodeProvider()
        {
            SetupResult.For(mockCache.Get(null)).IgnoreArguments().Return(null);

            UpdateReportData reportData = new UpdateReportData();
            mockSourceControlProvider.GetChangedItems("blah", 5, 10, reportData);
            LastCall.Return(new FolderMetaData());

            mocks.ReplayAll();

            FolderMetaData items = repository.GetChangedItems("blah", 5, 10, reportData);
            Assert.NotNull(items);
        }

        [Fact]
        public void GetItems_WhenCanFindItemInCache_WillSkipCallingToSourceCodeProvider()
        {
            SetupResult.For(mockCache.Get(null)).IgnoreArguments().Return(new CachedResult(new ItemMetaData()));

            DoNotExpect.Call(mockSourceControlProvider.GetItems(5, "blah", Recursion.Full));

            mocks.ReplayAll();

            ItemMetaData items = repository.GetItems(5, "blah", Recursion.Full);
            Assert.NotNull(items);
        }

        [Fact]
        public void GetItems_WhenCannotFindItemInCache_WillGoToSourceCodeProvider()
        {
            SetupResult.For(mockCache.Get(null)).IgnoreArguments().Return(null);

            mockSourceControlProvider.GetItems(5, "blah", Recursion.Full);
            LastCall.Return(new ItemMetaData());

            mocks.ReplayAll();

            ItemMetaData items = repository.GetItems(5, "blah", Recursion.Full);
            Assert.NotNull(items);
        }
    }
}