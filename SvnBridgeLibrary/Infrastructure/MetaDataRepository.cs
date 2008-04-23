using System;
using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Proxies;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    [Interceptor(typeof(TracingInterceptor))]
    public class MetaDataRepository : IMetaDataRepository
    {
        private readonly ITFSSourceControlService sourceControlService;
        private readonly string serverUrl;
        private readonly string rootPath;
        private readonly ICredentials credentials;
        private readonly IPersistentCache persistentCache;

        public MetaDataRepository(
            ITFSSourceControlService sourceControlService,
            ICredentials credentials,
            IPersistentCache persistentCache,
            string serverUrl,
            string rootPath)
        {
            this.sourceControlService = sourceControlService;
            this.serverUrl = serverUrl;
            this.rootPath = rootPath;
            this.credentials = credentials;
            this.persistentCache = persistentCache;
        }

        public SourceItem[] QueryItems(int revision, string path, Recursion recursion)
        {
            List<SourceItem> list = null;
            persistentCache.UnitOfWork(delegate
            {
               string serverPath = GetServerPath(path);

               EnsureRevisionIsCached(revision, path);

               string cacheKey = GetItemsListCacheKey(recursion, revision, serverPath);

               list = persistentCache.GetList<SourceItem>(cacheKey);
               list.Sort(delegate(SourceItem x, SourceItem y)
               {
                   return x.RemoteName.CompareTo(y.RemoteName);
               });
            });
            return list.ToArray();
        }

        private string GetItemsListCacheKey(Recursion recursion, int revision, string path)
        {
            switch (recursion)
            {
                case Recursion.Full:
                    return GetItemFullPathCacheKey(revision, path);
                case Recursion.OneLevel:
                    return GetItemOneLevelCacheKey(revision, path);
                case Recursion.None:
                    return GetItemNoRecursionCacheKey(revision, path);
                default:
                    throw new NotSupportedException();
            }
        }

        private string GetItemNoRecursionCacheKey(int revision, string path)
        {
            return "No recursion of: " + GetItemCacheKey(revision, path);
        }

        private string CurrentUserName
        {
            get
            {
                string currentUserName = PerRequest.Items["CurrentUserName"] as string;
                if(currentUserName!=null)
                    return currentUserName;
                NetworkCredential credential = credentials.GetCredential(new Uri(serverUrl), "Basic");
                currentUserName = credential.UserName + "@" + credential.Domain;
                PerRequest.Items["CurrentUserName"] = currentUserName;
                return currentUserName;
            }
        }

        private string GetServerPath(string path)
        {
            if (path.StartsWith("$/"))
                return path;

            string serverPath = rootPath;

            if (serverPath.EndsWith("/"))
                serverPath = serverPath.Substring(0, serverPath.Length - 1);

            if (path.StartsWith("/") == false)
                serverPath = serverPath + '/' + path;
            else
                serverPath = serverPath + path;

            if (serverPath.EndsWith("/") && serverPath != "$/")
                serverPath = serverPath.Substring(0, serverPath.Length - 1);

            return serverPath;
        }

        public void EnsureRevisionIsCached(int revision, string path)
        {
            string serverPath = GetServerPath(path);

            // already cached this version, skip inserting
            if (IsInCache(revision, serverPath))
                return;
            string cacheKey = CreateRevisionAndPathCacheKey(revision, serverPath);
            persistentCache.UnitOfWork(delegate
            {
                // we have to make a second test here, to ensure that another thread
                // did not already read this version
                if (IsInCache(revision, serverPath))
                    return;

                IEnumerator<SourceItem> items = sourceControlService.QueryItemsReader(serverUrl,
                                                                                      credentials,
                                                                                      serverPath,
                                                                                      RecursionType.Full,
                                                                                      VersionSpec.FromChangeset(revision))
                                                                            .GetEnumerator();
                bool firstRead = true;
                while (items.MoveNext())
                {
                    if (firstRead)
                    {
                        items = QueryFolderIfCurrentlyReadingFile(revision, ref serverPath, items);
                        firstRead = false;
                    }

                    string itemCacheKey = GetItemCacheKey(revision, items.Current.RemoteName);


                    persistentCache.Set(itemCacheKey, items.Current);

                    persistentCache.Add(GetItemNoRecursionCacheKey(revision, items.Current.RemoteName), itemCacheKey);
                    persistentCache.Add(GetItemOneLevelCacheKey(revision, items.Current.RemoteName), itemCacheKey);
                    persistentCache.Add(GetItemFullPathCacheKey(revision, items.Current.RemoteName), itemCacheKey);

                    string parentDirectory = GetParentName(items.Current.RemoteName);
                    persistentCache.Add(GetItemOneLevelCacheKey(revision, parentDirectory), itemCacheKey);

                    do
                    {
                        persistentCache.Add(GetItemFullPathCacheKey(revision, parentDirectory), itemCacheKey);
                        parentDirectory = GetParentName(parentDirectory);
                    } while (parentDirectory != "$" && string.IsNullOrEmpty(parentDirectory) == false);

                }

                persistentCache.Set(cacheKey, true);

                items.Dispose();
            });

        }

        private IEnumerator<SourceItem> QueryFolderIfCurrentlyReadingFile(int revision, ref string serverPath, IEnumerator<SourceItem> items)
        {
            // we optimize it here in case we tried to load a file, we load the entire
            // directory. This tends to save a lot of round trips in many cases
            if (items.Current.ItemType == ItemType.File)
            {
                //change it to the directory name, can't use the Path class
                // because that will change the '/' to '\'
                serverPath = serverPath.Substring(0, serverPath.LastIndexOf('/'));
                items.Dispose();
                items = sourceControlService.QueryItemsReader(serverUrl,
                                                              credentials,
                                                              serverPath,
                                                              RecursionType.Full,
                                                              VersionSpec.FromChangeset(revision))
                                .GetEnumerator();
                items.MoveNext();
            }
            return items;
        }

        private string GetItemFullPathCacheKey(int revision, string parentDirectory)
        {
            return "Full path of " + GetItemCacheKey(revision, parentDirectory);
        }

        private string GetItemOneLevelCacheKey(int revision, string parentDirectory)
        {
            return "One Level of " + GetItemCacheKey(revision, parentDirectory);
        }

        private string GetItemCacheKey(int revision, string path)
        {
            return "ServerUrl: " + serverUrl +
                   ", UserName: " + CurrentUserName +
                   ", Revision: " + revision +
                   ", Path: " + path;
        }

        private string GetParentName(string name)
        {
            int lastIndexOfSlash = name.LastIndexOf('/');
            if (lastIndexOfSlash == -1)
                return name;
            return name.Substring(0, lastIndexOfSlash);
        }

        public bool IsInCache(int revision, string path)
        {
            CachedResult result;
            string serverPath = path;
            do
            {
                string cacheKey = CreateRevisionAndPathCacheKey(revision, serverPath);
                result = persistentCache.Get(cacheKey);

                if (serverPath.IndexOf('/') == -1)
                    break;

                serverPath = serverPath.Substring(0, serverPath.LastIndexOf('/'));
            } while (result == null);


            return result != null;
        }

        private string CreateRevisionAndPathCacheKey(int revision, string serverPath)
        {
            return "Revision: " + revision +
                   ", ServerUrl: " + serverUrl +
                   ", UserName: " + CurrentUserName +
                   ", RootPath: " + serverPath;
        }

        public void ClearCache()
        {
            persistentCache.Clear();
        }
    }
}
