using System;
using System.Runtime.InteropServices;

namespace SvnBridge
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();
    }
}
