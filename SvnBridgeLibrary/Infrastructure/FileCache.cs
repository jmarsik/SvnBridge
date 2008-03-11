using System.IO;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
    public class FileCache : IFileCache
    {
        private readonly string rootCachePath;

        public FileCache(string rootCachePath)
        {
            this.rootCachePath = rootCachePath;
            EnsureDirectoryExists(rootCachePath);
        }

        public byte[] Get(string filename, int revision)
        {
            string fileName = Path.Combine(Path.Combine(rootCachePath, filename), revision.ToString());
            if (File.Exists(fileName))
            {
                return File.ReadAllBytes(fileName);
            }
            return null;
        }

        public void Set(string filename, int revision, byte[] data)
        {
            string directoryName = Path.Combine(rootCachePath, filename);
            string fileName = Path.Combine(directoryName, revision.ToString());
            EnsureDirectoryExists(directoryName);
            File.WriteAllBytes(fileName, data);
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