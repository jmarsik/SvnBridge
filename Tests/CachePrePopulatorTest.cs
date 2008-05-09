using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary.ObjectModel;
using Rhino.Mocks;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;
using Xunit;

namespace SvnBridge
{
    public class CachePrePopulatorTest : IDisposable
    {
        private readonly CachePrePopulator cachePopulator;
        private readonly MockRepository mocks = new MockRepository();
        private readonly ISourceControlProvider sourceControlProvider;

        public CachePrePopulatorTest()
        {
            sourceControlProvider = mocks.CreateMock<ISourceControlProvider>();
            cachePopulator = new CachePrePopulator(sourceControlProvider);
        }

        #region IDisposable Members

        public void Dispose()
        {
            mocks.VerifyAll();
        }

        #endregion

        [Fact]
        public void WillCallGetItemsOnSeparateItemsInTheCache()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo"}},
                }
            };

            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/foo", Recursion.Full))
                .Return(null);

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }


        [Fact]
        public void WillNotCallToChildrenOfItemAlreadyLoaded()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1/2"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1/2/3"}},
                }
            };

            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/foo", Recursion.Full))
                .Return(null);

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }

        [Fact]
        public void WillNotCallToChildren_WhenFolderDepthIsGreaterThanOne()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1/2/3"}},
                }
            };

            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/foo", Recursion.Full))
                .Return(null);

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }


        [Fact]
        public void WillMergeHierarchyCallToCallsToOneParent()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1/2"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo/1/2/3"}},
                }
            };

            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/foo", Recursion.Full))
                .Return(null);

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }

        [Fact]
        public void WillMergeManyCallsOfSubFoldersToCallsToOneParent()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/bar"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/fubar"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/baz"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/bay"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/fey"}},
                }
            };

            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge", Recursion.Full))
                .Return(null);

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }

        [Fact]
        public void WillNotMergeFewCallsOfSubFoldersToCallsToOneParent()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/foo"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/bar"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/fubar"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge/baz"}},
                }
            };

            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/foo", Recursion.Full))
                .Return(null);
            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/bar", Recursion.Full))
                            .Return(null);
            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/fubar", Recursion.Full))
                            .Return(null);
            Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge/baz", Recursion.Full))
                            .Return(null);

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }

        [Fact]
        public void WillNotMergeManyCallsToDifferentProjects()
        {
            var history = new SourceItemHistory
            {
                Changes = new List<SourceItemChange>
                {
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge1"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge2"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge3"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge4"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge5"}},
                    new SourceItemChange {Item = new SourceItem {RemoteName = "$/SvnBridge6"}},
                }
            };
            for (int i = 0; i < 6; i++)
            {
                Expect.Call(sourceControlProvider.GetItems(15, "$/SvnBridge" + (i+1), Recursion.Full))
                          .Return(null);
            }

            mocks.ReplayAll();

            cachePopulator.PrePopulateCacheWithChanges(history, 15);
        }
    }
}