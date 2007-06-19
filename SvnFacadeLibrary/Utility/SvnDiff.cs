namespace SvnBridge.Utility
{
    public class SvnDiff
    {
        public ulong SourceViewOffset;
        public ulong SourceViewLength;
        public ulong TargetViewLength;
        public ulong InstructionSectionLength;
        public ulong DataSectionLength;
        public byte[] InstructionSectionBytes;
        public byte[] DataSectionBytes;
    }
}