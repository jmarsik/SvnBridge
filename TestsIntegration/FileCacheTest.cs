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
        private string cachePath;

        [SetUp]
        public void TestInitialize()
        {
            cachePath = Path.GetTempPath();
            fileCache = new FileCache(cachePath);
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

        [Test]
        public void WillIgnoreCorruptFiles()
        {
            Guid guid = Guid.NewGuid();
            fileCache.Set("blah", 1, guid.ToByteArray());
            File.Delete(Path.Combine(Path.Combine(cachePath, "blah"), "1.verification"));
            byte[] bytes = fileCache.Get("blah", 1);
            Assert.IsNull(bytes);
        }
    }
}