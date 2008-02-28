namespace SvnBridge.Interfaces
{
    public interface ICache
    {
        object Get(string key);
        void Set(string key, object obj);
    }
}