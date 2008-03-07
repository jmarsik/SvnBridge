using System.Collections.Generic;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class ItemLoaderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            queue = new Queue<ItemMetaData>();
            provider = stubs.CreateObject<StubSourceControlProvider>();
            loader = new ItemLoader(queue, provider);
        }

        #endregion

        private MyMocks stubs = new MyMocks();
        private StubSourceControlProvider provider;
        private ItemLoader loader;
        private Queue<ItemMetaData> queue;

        [Test]
        public void VerifyCancelStopsReadingFromQueue()
        {
            stubs.Attach(provider.ReadFile, new byte[] {1, 2, 3, 4});
            ItemMetaData item = new ItemMetaData();
            queue.Enqueue(item);
            queue.Enqueue(item);

            loader.Cancel();
            loader.Start();

            Assert.AreNotEqual(0, queue.Count);
        }

        [Test]
        public void VerifyStartReadsFromQueueAndLoadsFileFromSourceControl()
        {
            stubs.Attach(provider.ReadFile, new byte[] {1, 2, 3, 4});
            ItemMetaData item = new ItemMetaData();
            queue.Enqueue(item);

            loader.Start();

            Assert.IsTrue(item.DataLoaded);
            Assert.AreEqual(new byte[] {1, 2, 3, 4}, item.Data);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void VerifyStartReadsFromQueueUntilQueueIsEmpty()
        {
            stubs.Attach(provider.ReadFile, new byte[] {1, 2, 3, 4});
            ItemMetaData item = new ItemMetaData();
            queue.Enqueue(item);
            queue.Enqueue(item);

            loader.Start();

            Assert.AreEqual(0, queue.Count);
        }
    }
}