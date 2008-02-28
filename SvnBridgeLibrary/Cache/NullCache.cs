using SvnBridge.Interfaces;

namespace SvnBridge.Cache
{
    public class NullCache : ICache
    {
        #region ICache Members

        public object Get(string key)
        {
            return null;
        }

        public void Set(string key,
                        object obj)
        {
        }

        #endregion
    }
}