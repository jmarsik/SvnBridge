using System.Runtime.InteropServices;

namespace SvnBridge.TfsLibrary
{
    public class TfsUtil
    {
        public const int CodePage_Unknown = -2;
        public const int CodePage_Binary = -1;
        public static readonly int CodePage_UTF8 = 65001;
        public static readonly int CodePage_UTF16_LittleEndian = 1200;
        public static readonly int CodePage_UTF16_BigEndian = 1201;
        public static readonly int CodePage_UTF32_LittleEndian = 12000;
        public static readonly int CodePage_UTF32_BigEndian = 12001;
        static readonly byte[] ByteOrderMark_UTF8 = { 0xEF, 0xBB, 0xBF };
        static readonly byte[] ByteOrderMark_UTF16_LittleEndian = { 0xFF, 0xFE };
        static readonly byte[] ByteOrderMark_UTF16_BigEndian = { 0xFE, 0xFF };
        static readonly byte[] ByteOrderMark_UTF32_LittleEndian = { 0xFF, 0xFE, 0x00, 0x00 };
        static readonly byte[] ByteOrderMark_UTF32_BigEndian = { 0x00, 0x00, 0xFE, 0xFF };

        // Properties

        public static int CodePage_ANSI
        {
            get { return (int)GetACP(); }
        }

        // Externals

        [DllImport("Kernel32.dll")]
        static extern uint GetACP();
    }
}