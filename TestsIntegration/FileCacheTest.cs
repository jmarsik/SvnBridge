using System;
using System.IO;
using SvnBridge.Cache;
using SvnBridge.Exceptions;
using Xunit;

namespace IntegrationTests
{
	public class FileCacheTest
	{
		private FileCache fileCache;
		private readonly string cachePath;

		public  FileCacheTest()
		{
			cachePath = Path.GetTempPath();
			fileCache = new FileCache(cachePath);
		}

		[Fact]
		public void IfAskingForNonExistingFile_WillReturnNull()
		{
			byte[] bytes = fileCache.Get("blah", 2);
			Assert.Null(bytes);
		}

		[Fact]
		public void CanGetCachedFile()
		{
			Guid guid = Guid.NewGuid();
			fileCache.Set("blah", 1, guid.ToByteArray());

			byte[] bytes = fileCache.Get("blah", 1);
			Assert.Equal(guid, new Guid(bytes));
		}

		[Fact]
		public void WhenAskingForUnCachedVersionOfCachedFile_WillReturnNull()
		{
			Guid guid = Guid.NewGuid();
			fileCache.Set("blah", 1, guid.ToByteArray());

			byte[] bytes = fileCache.Get("blah", 2);
			Assert.Null(bytes);
		}

		[Fact]
		public void WillIgnoreCorruptFiles()
		{
			Guid guid = Guid.NewGuid();
			fileCache.Set("blah", 1, guid.ToByteArray());
			File.Delete(Path.Combine(Path.Combine(cachePath, "blah"), "1.verification"));
			byte[] bytes = fileCache.Get("blah", 1);
			Assert.Null(bytes);
		}

		[Fact]
		public void WillThrowIfDoesNotHaveAccessToFileSystem()
		{
			Assert.Throws<EnvironmentValidationException>("", delegate
			{
				new FileCache(@"b:\cache").ValidateEnvironment();
			});
		}
	}
}