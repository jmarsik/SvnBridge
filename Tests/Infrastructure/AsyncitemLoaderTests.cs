﻿using System.Threading;
using Attach;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;
using Tests;
using Xunit;

namespace UnitTests.Infrastructure
{
    public class AsyncitemLoaderTests
    {
        protected MyMocks stubs = new MyMocks();

        [Fact]
        public void Cancel_LoadingItems_StopsLoadingItems()
        {
            var folder = new FolderMetaData();
            folder.Items.Add(new ItemMetaData());
            folder.Items.Add(new ItemMetaData());
            folder.Items.Add(new ItemMetaData());

            TFSSourceControlProvider tfsProvider = stubs.CreateTFSSourceControlProviderStub();
            var loader = new AsyncItemLoader(folder, tfsProvider);
            stubs.Attach(tfsProvider.ReadFileAsync, Return.DelegateResult(delegate
                                                                              {
                                                                                  Thread.Sleep(1000);
                                                                                  return null;
                                                                              }));
            var loadingThread = new Thread(loader.Start);
            loadingThread.Start();

            loader.Cancel();

            Thread.Sleep(2000);

            Assert.Equal(ThreadState.Stopped, loadingThread.ThreadState);
        }

        [Fact]
        public void Cancel_LoopingDueToBufferFull_StopsLoadingItems()
        {
            var folder = new FolderMetaData();
            TFSSourceControlProvider tfsProvider = stubs.CreateTFSSourceControlProviderStub();
            folder.Items.Add(new ItemMetaData
                                 {DataLoaded = true, Base64DiffData = string.Empty.PadRight(100000001, '0')});
            var loader = new AsyncItemLoader(folder, tfsProvider);
            stubs.Attach(tfsProvider.ReadFileAsync, Return.DelegateResult(delegate
                                                    {
                                                        Thread.Sleep(1000);
                                                        return null;
                                                    }));
            var loadingThread = new Thread(loader.Start);
            loadingThread.Start();

            Thread.Sleep(250);

            loader.Cancel();

            Thread.Sleep(1000);

            Assert.Equal(ThreadState.Stopped, loadingThread.ThreadState);
        }
    }
}