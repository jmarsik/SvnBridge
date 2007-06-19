using System;
using System.IO;

namespace SvnBridge.Utility
{
    public class SvnDiffEngine
    {
        public static byte[] ApplySvnDiff(SvnDiff svnDiff,
                                          byte[] source)
        {
            const int BUF_SIZE = 1000;
            byte[] buffer = new byte[BUF_SIZE];
            int index = 0;

            MemoryStream instructionStream = new MemoryStream(svnDiff.InstructionSectionBytes);
            BinaryReader instructionReader = new BinaryReader(instructionStream);
            MemoryStream dataStream = new MemoryStream(svnDiff.DataSectionBytes);
            BinaryReader dataReader = new BinaryReader(dataStream);

            SvnDiffInstruction instruction = ReadInstruction(instructionReader);
            while (instruction != null)
            {
                switch (instruction.OpCode)
                {
                    case SvnDiffInstruction.CopyFromSource:
                        //sb.AppendFormat("[SOURCE:{0},{1}]", instruction.Length, instruction.Offset);
                    {
                        for (int i = 0; i < (int)instruction.Length; i++)
                        {
                            if (index >= buffer.Length)
                            {
                                Helper.ReDim(ref buffer, buffer.Length + BUF_SIZE);
                            }
                            buffer[index] = source[(int)instruction.Offset + i];
                            index++;
                        }
                    }
                        break;

                    case SvnDiffInstruction.CopyFromTarget:
                        //sb.AppendFormat("[TARGET:{0},{1}]", instruction.Length, instruction.Offset);
                    {
                        if (index + (int)instruction.Length > buffer.Length)
                        {
                            Helper.ReDim(ref buffer, index + (int)instruction.Length + 1000);
                        }

                        //Array.Copy(buffer, (int)instruction.Offset, buffer, index, (int)instruction.Length);
                        //index += (int)instruction.Length;

                        for (int i = 0; i < (int)instruction.Length; i++)
                        {
                            buffer[index] = buffer[(int)instruction.Offset + i];
                            index++;
                        }
                    }
                        break;

                    case SvnDiffInstruction.CopyFromNewData:
                    {
                        byte[] newData = dataReader.ReadBytes((int)instruction.Length);
                        if (index + newData.Length > buffer.Length)
                        {
                            Helper.ReDim(ref buffer, buffer.Length + newData.Length + 1000);
                        }
                        Array.Copy(newData, 0, buffer, index, newData.Length);
                        index += newData.Length;
                    }
                        break;
                }

                instruction = ReadInstruction(instructionReader);
            }

            Helper.ReDim(ref buffer, index);
            return buffer;
        }

        public static SvnDiff CreateReplaceDiff(byte[] bytes)
        {
            SvnDiff svnDiff = null;
            if (bytes.Length > 0)
            {
                svnDiff = new SvnDiff();

                svnDiff.SourceViewOffset = 0;
                svnDiff.SourceViewLength = 0;
                svnDiff.TargetViewLength = (ulong)bytes.Length;

                MemoryStream instructionStream = new MemoryStream();
                BinaryWriter instructionWriter = new BinaryWriter(instructionStream);
                MemoryStream dataStream = new MemoryStream();
                BinaryWriter dataWriter = new BinaryWriter(dataStream);

                dataWriter.Write(bytes);
                dataWriter.Flush();

                svnDiff.DataSectionBytes = dataStream.ToArray();
                svnDiff.DataSectionLength = (ulong)svnDiff.DataSectionBytes.Length;

                SvnDiffInstruction instruction = new SvnDiffInstruction();
                instruction.OpCode = SvnDiffInstruction.CopyFromNewData;
                instruction.Length = (ulong)bytes.Length;

                WriteInstruction(instructionWriter, instruction);
                instructionWriter.Flush();

                svnDiff.InstructionSectionBytes = instructionStream.ToArray();
                svnDiff.InstructionSectionLength = (ulong)svnDiff.InstructionSectionBytes.Length;
            }
            return svnDiff;
        }

        static SvnDiffInstruction ReadInstruction(BinaryReader reader)
        {
            if (reader.BaseStream.Position == reader.BaseStream.Length)
                return null;

            SvnDiffInstruction instruction = new SvnDiffInstruction();

            byte opCodeAndLength = reader.ReadByte();

            instruction.OpCode = (opCodeAndLength & 0xC0) >> 6;

            byte length = (byte)(opCodeAndLength & 0x3F);
            if (length == 0)
            {
                instruction.Length = SvnDiffParser.ReadInt(reader);
            }
            else
            {
                instruction.Length = length;
            }

            if (instruction.OpCode == SvnDiffInstruction.CopyFromSource ||
                instruction.OpCode == SvnDiffInstruction.CopyFromTarget)
            {
                instruction.Offset = SvnDiffParser.ReadInt(reader);
            }

            return instruction;
        }

        static void WriteInstruction(BinaryWriter writer,
                                     SvnDiffInstruction instruction)
        {
            byte opCodeAndLength = (byte)(instruction.OpCode << 6);
            int bytesWritten = 0;

            if ((instruction.Length & 0x3F) == instruction.Length)
            {
                opCodeAndLength |= (byte)(instruction.Length & 0x3F);

                writer.Write(opCodeAndLength);
            }
            else
            {
                writer.Write(opCodeAndLength);
                SvnDiffParser.WriteInt(writer, instruction.Length, out bytesWritten);
            }

            if (instruction.OpCode == SvnDiffInstruction.CopyFromSource ||
                instruction.OpCode == SvnDiffInstruction.CopyFromTarget)
            {
                SvnDiffParser.WriteInt(writer, instruction.Offset, out bytesWritten);
            }
        }
    }
}