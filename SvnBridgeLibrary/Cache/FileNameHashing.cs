using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SvnBridge.Cache
{
    public static class FileNameHashing
    {
        /// <summary>
        /// this is actually a bit shorter than the realy legnth,
        /// That is because we need to allow for additional (reasonably named) files 
        /// inside a directory that was hashed using HashIfNeeded
        /// </summary>
        private const int MaxPathLength = 230;

        public static string HashIfNeeded(string rootPath, string filename)
        {
            if (IsValidPath(filename, rootPath))
                return filename;

            byte[] hash = HashAlgorithm.Create("MD5").ComputeHash(Encoding.UTF8.GetBytes(filename));
            string hasedPath = "@hashed";
            StringBuilder sb = new StringBuilder(hasedPath);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            sb.Insert(9, Path.DirectorySeparatorChar);
            sb.Insert(13, Path.DirectorySeparatorChar);
            sb.Insert(16, Path.DirectorySeparatorChar);
            sb.Insert(18, Path.DirectorySeparatorChar);
            return sb.ToString();
        }

        private static bool IsValidPath(string filename, string rootPath)
        {
            foreach (char invalidPathChar in Path.GetInvalidFileNameChars())
            {
                if (filename.Contains(invalidPathChar.ToString()))
                    return false;
            }
            return filename.Length + rootPath.Length + 1 < MaxPathLength;
        }
    }
}