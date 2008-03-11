namespace SvnBridge.Interfaces
{
    public interface IFileCache
    {
        byte[] Get(string filename, int revision);

        void Set(string filename, int revision, byte[] data);
    }
}