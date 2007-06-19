namespace SvnBridge.Utility
{
    public class SvnDiffInstruction
    {
        public const int CopyFromSource = 0;
        public const int CopyFromTarget = 1;
        public const int CopyFromNewData = 2;

        public int OpCode;
        public ulong Length;
        public ulong Offset;
    }
}