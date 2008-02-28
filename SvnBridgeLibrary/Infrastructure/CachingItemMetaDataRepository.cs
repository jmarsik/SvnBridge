using System.IO;
using System.Xml.Serialization;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class CachingItemMetaDataRepository : IItemMetaDataRepository
    {
        private readonly ISourceControlProvider svc;
        private readonly ICache cache;

        static readonly XmlSerializer serializer = new XmlSerializer(typeof(UpdateReportData));

        public CachingItemMetaDataRepository(ISourceControlProvider svc, ICache cache)
        {
            this.svc = svc;
            this.cache = cache;
        }

        public ItemMetaData GetItems(int version, string path, Recursion recursion)
        {
            string cacheKey = version+"-"+path+"-"+recursion;

            object cached = cache.Get(cacheKey);
            if(cached != null)
                return (ItemMetaData) cached;
            ItemMetaData items = svc.GetItems(version, path, recursion);
            cache.Set(cacheKey, items);
            
            return items;
        }

        public FolderMetaData GetChangedItems(string path, int versionFrom, int versionTo, UpdateReportData reportData)
        {
            string reportDataAsKey;
            using(StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, reportData);
                reportDataAsKey = sw.GetStringBuilder().ToString();
            }
            string cacheKey = path + "-" + versionFrom + "-" + versionTo + "-" + reportDataAsKey;

            object cached = cache.Get(cacheKey);
            if (cached != null)
                return (FolderMetaData) cached;

            FolderMetaData items = svc.GetChangedItems(path, versionFrom, versionTo, reportData);
            cache.Set(cacheKey, items);
            return items;
        }
    }
}