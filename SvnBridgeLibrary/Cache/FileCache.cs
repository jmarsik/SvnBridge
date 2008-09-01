using System;
using System.IO;
using SvnBridge.Exceptions;
using SvnBridge.Interfaces;
using System.Threading;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Cache
{
	public class FileCache : ICanValidateMyEnvironment
	{
		private const string verificationExtension = ".verification";

		private readonly string rootCachePath;
		private bool ensuredDirectoryExists;

		public FileCache(string fileCachePath)
		{
			rootCachePath = fileCachePath;
		}

		public virtual byte[] Get(string filename, int revision)
		{
            byte[] result = null;
            GetInternal(filename, revision,delegate(string path)
                                               {
                                                   result = File.ReadAllBytes(path);
                                               });
		    return result;
		}

	    private void GetInternal(string filename, int revision, Action<string> action)
	    {
	        EnsureRootDirectoryExists();

	        string hashedFilename = FileNameHashing.HashIfNeeded(rootCachePath, filename);

	        string cachedFileName = Path.Combine(Path.Combine(rootCachePath, hashedFilename), revision.ToString());
	        FileInfo verification = new FileInfo(cachedFileName + verificationExtension);
	        FileInfo cached = new FileInfo(cachedFileName);

	        if (
	            // both verification and cached exists, and the verification
	            // file is newer than the cached file
	            cached.Exists && verification.Exists &&
	            cached.LastWriteTimeUtc <= verification.LastWriteTimeUtc
	            )
	        {
	            action(cachedFileName);
	        }
	    }

        public virtual void Set(string filename, int revision, byte[] data)
		{
			EnsureRootDirectoryExists();

			string hashedFilename = FileNameHashing.HashIfNeeded(rootCachePath, filename);
			string directoryName = Path.Combine(rootCachePath, hashedFilename);
			string cachedFileName = Path.Combine(directoryName, revision.ToString());
			EnsureDirectoryExists(directoryName);

	        string base64 = SvnDiffParser.GetSvnDiffData(data);
	        string md5 = Helper.GetMd5Checksum(data);
			File.WriteAllBytes(cachedFileName, data);
            File.WriteAllText(cachedFileName + ".base64", base64);
            File.WriteAllText(cachedFileName + ".md5", md5);
			File.WriteAllBytes(cachedFileName + verificationExtension, new byte[] { 13, 37 });
		}


        public virtual FileData GetText(string filename, int revision)
	    {
            FileData result = null;
            GetInternal(filename, revision, delegate(string path)
            {
                result = new FileData
                             {
                                 Md5 = File.ReadAllText(path + ".md5"),
                                 Base64DiffData = File.ReadAllText(path + ".base64")
                             };
            });
            return result;
	    }

	    private void EnsureRootDirectoryExists()
		{
			if (ensuredDirectoryExists)
				return;
			ensuredDirectoryExists = true;
			EnsureDirectoryExists(rootCachePath);
		}

		private void EnsureDirectoryExists(string directoryName)
		{
			if (Directory.Exists(directoryName) == false)
			{
				Directory.CreateDirectory(directoryName);
			}
		}

		public void ValidateEnvironment()
		{
			try
			{
				EnsureDirectoryExists(rootCachePath);

				Guid guid = Guid.NewGuid();
				File.WriteAllText(Path.Combine(rootCachePath, "test_file_write"), guid.ToString());
				File.ReadAllText(Path.Combine(rootCachePath, "test_file_write"));
				Directory.CreateDirectory(Path.Combine(rootCachePath, "test_directory_create"));
				string test_folder_write_in_subfolder = Path.Combine(Path.Combine(rootCachePath, "test_directory_create"),
																	 "test_file_write_in_subfolder");
				File.WriteAllText(test_folder_write_in_subfolder, "test_file_write_in_subfolder");
				File.ReadAllText(test_folder_write_in_subfolder);
			}
			catch (Exception)
			{
				string message = "Could not validate environment for file cache at: '" + rootCachePath + "'." + Environment.NewLine +
								 "Usually this error occurs because of invalid permissions on the file cache folder. " + Environment.NewLine +
								 "Ensure that the application (user: '" + Thread.CurrentPrincipal.Identity.Name + "' is allowed read/write/create directory permissions in the directory '" + Environment.NewLine + "' and its subdirectories.";
				throw new EnvironmentValidationException(message);
			}
		}
	}
}