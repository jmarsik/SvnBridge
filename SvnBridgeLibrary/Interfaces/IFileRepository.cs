using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
    public interface IFileRepository
    {
        byte[] GetFile(ItemMetaData item);
        void ReadFileAsync(ItemMetaData item);
    }
}
