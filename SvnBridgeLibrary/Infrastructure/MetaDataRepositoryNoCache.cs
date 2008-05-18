using System;
using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Proxies;
using SvnBridge.SourceControl;
using System.Configuration;
using SvnBridge.Properties;

namespace SvnBridge.Infrastructure
{
    [Interceptor(typeof(TracingInterceptor))]
    public class MetaDataRepositoryNoCache : IMetaDataRepository
    {
        private readonly ITFSSourceControlService sourceControlService;
        private readonly string serverUrl;
        private readonly string rootPath;
        private readonly ICredentials credentials;
        private readonly IPersistentCache persistentCache;

        public MetaDataRepositoryNoCache(
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

        public SourceItem[] QueryItems(int revision, string[] paths, Recursion recursion)
        {
            List<SourceItem> items = new List<SourceItem>();
            foreach (string path in paths)
                foreach (SourceItem item in QueryItems(revision, path, recursion))
                    items.Add(item);

            return items.ToArray();
        }

        public SourceItem[] QueryItems(int revision, string path, Recursion recursion)
        {
            string serverPath = GetServerPath(path);
            RecursionType recursionType = RecursionType.None;
            switch (recursion)
            {
                case Recursion.OneLevel:
                    recursionType = RecursionType.OneLevel;
                    break;
                case Recursion.Full:
                    recursionType = RecursionType.Full;
                    break;
            }
            return sourceControlService.QueryItems(serverUrl, credentials, serverPath, recursionType, VersionSpec.FromChangeset(revision), DeletedState.NonDeleted, ItemType.Any);
        }

        private string GetServerPath(string path)
        {
            if (path.StartsWith("$//"))
                return Constants.ServerRootPath + path.Substring(3);

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
    }
}
