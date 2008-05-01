using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
    public interface ICache
    {
        CachedResult Get(string key);

        void Set(string key, object obj);
        void Clear();
    }
}
