using System.IO;
using System.Xml.Serialization;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class CachingItemMetaDataRepository : IItemMetaDataRepository
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(UpdateReportData));
        private readonly ICache cache;
        private readonly ISourceControlProvider svc;

        public CachingItemMetaDataRepository(ISourceControlProvider svc,
                                             ICache cache)
        {
            this.svc = svc;
            this.cache = cache;
        }

        #region IItemMetaDataRepository Members

        public ItemMetaData GetItems(int version,
                                     string path,
                                     Recursion recursion)
        {
            string cacheKey = "GetItems_" + version + "-" + path + "-" + recursion;

            CachedResult cached = cache.Get(cacheKey);
            if (cached != null)
            {
                return (ItemMetaData)cached.Value;
            }
            ItemMetaData items = svc.GetItems(version, path, recursion);
            cache.Set(cacheKey, items);

            return items;
        }

        public FolderMetaData GetChangedItems(string path,
                                              int versionFrom,
                                              int versionTo,
                                              UpdateReportData reportData)
        {
            string reportDataAsKey;
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, reportData);
                reportDataAsKey = sw.GetStringBuilder().ToString();
            }
            string cacheKey = path + "-" + versionFrom + "-" + versionTo + "-" + reportDataAsKey;

            CachedResult cached = cache.Get(cacheKey);
            if (cached != null)
            {
                return (FolderMetaData)cached.Value;
            }

            FolderMetaData items = svc.GetChangedItems(path, versionFrom, versionTo, reportData);
            cache.Set(cacheKey, items);
            return items;
        }

        #endregion
    }
}