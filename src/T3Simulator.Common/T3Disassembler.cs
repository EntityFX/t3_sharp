using System;
using System.Collections.Generic;
using System.Text;
using TritTypes;

namespace T3Simulator.Common
{
    public static class T3Disassembler
    {
        public static List<string> Disassemble<TWord>(IEnumerable<TWord> code) where TWord : IT3Word<TWord>
        {
            var lines = new List<string>();
            long pc = 0;
            int index = 0;
            var codeList = new List<TWord>(code);

            while (index < codeList.Count)
            {
                TWord word = codeList[index];
                var instr = InstructionDecoder.Decode(word);

                if (instr.Opcode == Opcode.LIMM)
                {
                    string immVal = "[next]";
                    if (index + 1 < codeList.Count)
                        immVal = codeList[index + 1].ToLong().ToString();
                    lines.Add($"{pc:X8}: LIMM {GetRegName(instr.PhysOp1)}, {immVal}");
                    pc += 2; index += 2;
                }
                else
                {
                    lines.Add($"{pc:X8}: {FormatInstruction(instr, pc)}");
                    pc += 1; index += 1;
                }
            }
            return lines;
        }

        static string GetRegName(int index)
        {
            return index switch
            {
                0 => "RW", 1 => "RX", 2 => "RY", 3 => "RZ",
                4 => "R0", 5 => "R1", 6 => "R2", 7 => "R3", 8 => "R4",
                _ => $"R{index}"
            };
        }

        static string FormatInstruction(DecodedInstruction instr, long pc)
        {
            var sb = new StringBuilder();
            if (instr.Predicate > 0) sb.Append($"(p{instr.Predicate}) ");
            string mnemonic = GetMnemonic(instr.Opcode);
            sb.Append(mnemonic);

            if (mnemonic is "HALT" or "RET" or "NOP") { }
            else if (mnemonic == "LI") sb.Append($" {GetRegName(instr.PhysOp1)}, {instr.Immediate}");
            else if (mnemonic == "NEG") sb.Append($" {GetRegName(instr.PhysOp1)}");
            else if (mnemonic is "MOV" or "CMP") sb.Append($" {GetRegName(instr.PhysOp1)}, {GetRegName(instr.PhysOp2)}");
            else if (mnemonic is "ADD" or "SUB" or "MUL" or "DIV" or "MOD" or "AND" or "OR" or "XOR" or "SHL" or "SHR")
            {
                sb.Append($" {GetRegName(instr.PhysOp1)}, {GetRegName(instr.PhysOp2)}, {GetRegName(instr.PhysOp3)}");
            }
            else if (mnemonic is "LOAD" or "STORE") sb.Append($" {GetRegName(instr.PhysOp1)}, {GetRegName(instr.PhysOp2)}");
            else if (mnemonic is "JMP" or "JE" or "JNE" or "JL" or "JG" or "JM" or "JLE" or "JGE" or "CALL")
                sb.Append(instr.Immediate == 0 ? $" {GetRegName(instr.PhysOp2)}" : $" {instr.Immediate}");
            else if (mnemonic is "PUSH" or "POP") sb.Append($" {GetRegName(instr.PhysOp1)}");
            else if (mnemonic is "IN" or "OUT") sb.Append($" {GetRegName(instr.PhysOp1)}, {GetRegName(instr.PhysOp2)}");
            else if (mnemonic is "INI" or "OUTI") sb.Append($" {GetRegName(instr.PhysOp1)}, {instr.Immediate}");
            else sb.Append($" {GetRegName(instr.PhysOp1)}, {GetRegName(instr.PhysOp2)}");

            return sb.ToString().TrimEnd();
        }

        static string GetMnemonic(Opcode op) => op switch
        {
            Opcode.HALT => "HALT", Opcode.LOAD => "LOAD", Opcode.LOADI => "LOADI", Opcode.STORE => "STORE", Opcode.STOREI => "STOREI",
            Opcode.MOV => "MOV", Opcode.MOVI => "MOVI", Opcode.LI => "LI", Opcode.LIMM => "LIMM",
            Opcode.ADD => "ADD", Opcode.ADDI => "ADDI", Opcode.SUB => "SUB", Opcode.SUBI => "SUBI",
            Opcode.MUL => "MUL", Opcode.MULI => "MULI", Opcode.DIV => "DIV", Opcode.DIVI => "DIVI",
            Opcode.MOD => "MOD", Opcode.MODI => "MODI", Opcode.NEG => "NEG", Opcode.NEGI => "NEGI",
            Opcode.AND => "AND", Opcode.ANDI => "ANDI", Opcode.OR => "OR", Opcode.ORI => "ORI",
            Opcode.XOR => "XOR", Opcode.XORI => "XORI", Opcode.SHL => "SHL", Opcode.SHLI => "SHLI",
            Opcode.SHR => "SHR", Opcode.SHRI => "SHRI", Opcode.CMP => "CMP", Opcode.CMPI => "CMPI",
            Opcode.JMP => "JMP", Opcode.JE => "JE", Opcode.JNE => "JNE", Opcode.JL => "JL",
            Opcode.JG => "JG", Opcode.JLE => "JLE", Opcode.JGE => "JGE", Opcode.JM => "JM",
            Opcode.CALL => "CALL", Opcode.RET => "RET", Opcode.PUSH => "PUSH", Opcode.POP => "POP",
            Opcode.IN => "IN", Opcode.OUT => "OUT", Opcode.INI => "INI", Opcode.OUTI => "OUTI",
            Opcode.FADD => "FADD", Opcode.FSUB => "FSUB", Opcode.FMUL => "FMUL", Opcode.FDIV => "FDIV",
            Opcode.FSQRT => "FSQRT", Opcode.FABS => "FABS", Opcode.FNEG => "FNEG", Opcode.FCMP => "FCMP",
            Opcode.FTOI => "FTOI", Opcode.ITOF => "ITOF", Opcode.FTOF => "FTOF",
            Opcode.FLW => "FLW", Opcode.FSW => "FSW", Opcode.FMOV => "FMOV",
            Opcode.FCLASS => "FCLASS", Opcode.FSWAP => "FSWAP", Opcode.FZERO => "FZERO",
            _ => "UNKNOWN"
        };
    }
}