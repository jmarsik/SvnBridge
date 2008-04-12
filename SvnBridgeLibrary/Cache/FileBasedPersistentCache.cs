using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Proxies;

namespace SvnBridge.Cache
{
	[Interceptor(typeof(RetryOnExceptionsInterceptor<IOException>))]
	public class FileBasedPersistentCache : IPersistentCache, ICanValidateMyEnvironment
	{
		private readonly string rootPath;
		private bool ensuredDirectoryExists;

		private static FileStream GetCurrentStream(string key)
		{
			return (FileStream)PerRequest.Items["current.file.stream." + key];
		}

		private class SetCurrentFile : IDisposable
		{
			private readonly string key;

			public SetCurrentFile(string key, FileStream stream)
			{
				this.key = key;
				PerRequest.Items["current.file.stream." + key] = stream;
			}

			#region IDisposable Members

			public void Dispose()
			{
				PerRequest.Items["current.file.stream." + key] = null;
			}

			#endregion
		}

		public FileBasedPersistentCache(string persistentCachePath)
		{
			this.rootPath = persistentCachePath;
		}

		#region ICanValidateMyEnvironment Members

		public void ValidateEnvironment()
		{
			EnsureRootDirectoryExists();

			File.WriteAllText("test.write", "can write to directory");
		}

		#endregion

		#region IPersistentCache Members

		public CachedResult Get(string key)
		{
			if (Contains(key) == false)
				return null;
			CachedResult result = null;
			Lock(key, delegate
			{
				if (GetCurrentStream(key).Length == 0)//empty file
					return;

				BinaryReader br = new BinaryReader(GetCurrentStream(key));
				string keyFromFile = br.ReadString();
				// we need to do this because we may get collisions
				// in the keys, the chances are not good for this, because we use 
				// cryptographically significant hashing for this, but we need to take this
				// into account
				// Note: we need to make a case insensitive comparision
				if (string.Equals(key, keyFromFile, StringComparison.InvariantCultureIgnoreCase) == false)
					return;
				BinaryFormatter formatter = new BinaryFormatter();
				object deserialized = formatter.Deserialize(GetCurrentStream(key));
				result = new CachedResult(deserialized);
			});
			return result;
		}

		public void Set(string key, object obj)
		{
			Lock(key, delegate
			{
				GetCurrentStream(key).SetLength(0);//truncate the file
				BinaryWriter bw = new BinaryWriter(GetCurrentStream(key));
				bw.Write(key);
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(GetCurrentStream(key), obj);
			});
		}

		public void Lock(string key, Action<string> action)
		{
			if (GetCurrentStream(key) != null)//already locked
			{
				action(key);
				return;
			}

			string fileNameFromKey = GetFileNameFromKey(key);
			EnsureDirectoryExists(Path.GetDirectoryName(fileNameFromKey));

			using (FileStream fileStream = File.Open(fileNameFromKey, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			using (new SetCurrentFile(key, fileStream))
			{
				action(key);
			}
		}

		/// <summary>
		/// We always 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private string GetFileNameFromKey(string key)
		{
			// Note: we need to make a case insensitive comparision
			string filename = key.ToLowerInvariant();
			return Path.Combine(rootPath, FileNameHashing.HashIfNeeded(rootPath, filename));
		}

		public bool Contains(string key)
		{
			string fileNameFromKey = GetFileNameFromKey(key);
			return File.Exists(fileNameFromKey);
		}

		public void Clear()
		{
			if (Directory.Exists(rootPath))
				Directory.Delete(rootPath, true);
			EnsureRootDirectoryExists();
		}

		public void Add(string key, string value)
		{
			Lock(key, delegate
			{
				CachedResult result = Get(key);
				ISet<string> set = new HashSet<string>();
				if (result != null)
					set = (ISet<string>)result.Value;
				set.Add(value);
				Set(key, set);
			});
		}

		public List<T> GetList<T>(string key)
		{
			List<T> items = new List<T>();
			CachedResult result = Get(key);
			if (result == null)
				return items;

			if (result.Value is T)
			{
				items.Add((T)result.Value);
				return items;
			}

			foreach (string itemKey in (IEnumerable<string>)result.Value)
			{
				CachedResult itemResult = Get(itemKey);
				if (itemResult != null)
					items.Add((T)itemResult.Value);
			}
			return items;
		}

		private void EnsureRootDirectoryExists()
		{
			if (ensuredDirectoryExists)
				return;
			ensuredDirectoryExists = true;
			EnsureDirectoryExists(rootPath);
		}

		private void EnsureDirectoryExists(string directoryName)
		{
			if (Directory.Exists(directoryName) == false)
			{
				Directory.CreateDirectory(directoryName);
			}
		}
		#endregion
	}
}