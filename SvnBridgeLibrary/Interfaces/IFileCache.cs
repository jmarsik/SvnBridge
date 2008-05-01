using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
    public interface IFileCache
    {
        byte[] Get(string filename, int revision);

        void Set(string filename, int revision, byte[] data);

        FileData GetText(string filename, int revision);
    }
}