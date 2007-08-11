namespace SvnBridge.Utility
{
    public enum SvnDiffInstructionOpCode
    {
        CopyFromSource = 0,
        CopyFromTarget = 1,
        CopyFromNewData = 2
    }
    
    public class SvnDiffInstruction
    {
        public SvnDiffInstructionOpCode OpCode;
        public ulong Length;
        public ulong Offset;
    }
}