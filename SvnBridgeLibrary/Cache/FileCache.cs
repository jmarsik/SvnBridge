using System;
using System.IO;
using SvnBridge.Exceptions;
using SvnBridge.Interfaces;
using System.Threading;

namespace SvnBridge.Cache
{
	public class FileCache : IFileCache, ICanValidateMyEnvironment
	{
		private const string verificationExtension = ".verification";

		private readonly string rootCachePath;
		private bool ensuredDirectoryExists;

		public FileCache(string fileCachePath)
		{
			rootCachePath = fileCachePath;
		}

		public byte[] Get(string filename, int revision)
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
				return File.ReadAllBytes(cachedFileName);
			}
			return null;
		}

		public void Set(string filename, int revision, byte[] data)
		{
			EnsureRootDirectoryExists();

			string hashedFilename = FileNameHashing.HashIfNeeded(rootCachePath, filename);
			string directoryName = Path.Combine(rootCachePath, hashedFilename);
			string cachedFileName = Path.Combine(directoryName, revision.ToString());
			EnsureDirectoryExists(directoryName);
			File.WriteAllBytes(cachedFileName, data);
			File.WriteAllText(cachedFileName + verificationExtension, filename);
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