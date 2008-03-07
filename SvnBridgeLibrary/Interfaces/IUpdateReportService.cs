using System.IO;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
    public interface IUpdateReportService
    {
        void ProcessUpdateReportForFile(UpdateReportData updateReportRequest,
                                        ItemMetaData item,
                                        StreamWriter output);

        void ProcessUpdateReportForDirectory(UpdateReportData updateReportRequest,
                                             FolderMetaData folder,
                                             StreamWriter output,
                                             bool rootFolder);
    }
}