using System;
using System.IO;
using NUnit.Framework;
using SvnBridge.Infrastructure;

namespace IntegrationTests
{
    [TestFixture]
    public class FileCacheTest
    {
        private FileCache fileCache;

        [SetUp]
        public void TestInitialize()
        {
            fileCache = new FileCache(Path.GetTempPath());
        }

        [Test]
        public void IfAskingForNonExistingFile_WillReturnNull()
        {
            byte[] bytes = fileCache.Get("blah", 2);
            Assert.IsNull(bytes);
        }

        [Test]
        public void CanGetCachedFile()
        {
            Guid guid = Guid.NewGuid();
            fileCache.Set("blah", 1, guid.ToByteArray());

            byte[] bytes = fileCache.Get("blah", 1);
            Assert.AreEqual(guid, new Guid(bytes));
        }

        [Test]
        public void WhenAskingForUnCachedVersionOfCachedFile_WillReturnNull()
        {
            Guid guid = Guid.NewGuid();
            fileCache.Set("blah", 1, guid.ToByteArray());

            byte[] bytes = fileCache.Get("blah", 2);
            Assert.IsNull(bytes);
        }
    }
}