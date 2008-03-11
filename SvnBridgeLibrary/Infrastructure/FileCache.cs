using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
    public class FileCache : IFileCache
    {
        private readonly string rootCachePath;
        /// <summary>
        /// this is actually a bit shorter than the realy legnth,
        /// That is because we need to allow for the revision number as well
        /// </summary>
        private const int MaxPathLength = 230;

        public FileCache(string rootCachePath)
        {
            this.rootCachePath = rootCachePath;
            EnsureDirectoryExists(rootCachePath);
        }

        public byte[] Get(string filename, int revision)
        {
            filename = HashIfNeeded(filename);

            string cachedFileName = Path.Combine(Path.Combine(rootCachePath, filename), revision.ToString());

            if (File.Exists(cachedFileName))
            {
                return File.ReadAllBytes(cachedFileName);
            }
            return null;
        }

        private  string HashIfNeeded(string filename)
        {
            if(filename.Length + rootCachePath.Length < MaxPathLength)
                return filename;

            byte[] hash = HashAlgorithm.Create().ComputeHash(Encoding.UTF8.GetBytes(filename));
            StringBuilder sb = new StringBuilder("LongPaths");
            sb.Append(Path.DirectorySeparatorChar);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            for (int i = 5; i < sb.Length; i+=10)
            {
                sb.Insert(i, Path.DirectorySeparatorChar);
            }
            return sb.ToString();
        }

        public void Set(string filename, int revision, byte[] data)
        {
            filename = HashIfNeeded(filename);
            string directoryName = Path.Combine(rootCachePath, filename);
            string cachedFileName = Path.Combine(directoryName, revision.ToString());
            EnsureDirectoryExists(directoryName);
            File.WriteAllBytes(cachedFileName, data);
        }

        private static void EnsureDirectoryExists(string directoryName)
        {
            if (Directory.Exists(directoryName) == false)
            {
                Directory.CreateDirectory(directoryName);
            }
        }
    }
}
