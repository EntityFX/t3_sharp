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
                    string prefix = GetGroupPrefix(instr.RegGroup);
                    string reg = GetRegName(instr.RegGroup, instr.Op1);
                    lines.Add($"{pc:X8}: {prefix}LIMM {reg}, {immVal}");
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

        static string GetGroupPrefix(int regGroup) => regGroup switch
        {
            -1 => "F.",
            +1 => "S.",
            _  => ""
        };

        static string GetRegName(int regGroup, int op)
        {
            // Convert balanced trit (-4..+4) to index (0..8)
            int idx = op + 4;
            if (regGroup == -1) // FPU
            {
                return idx switch
                {
                    0 => "FW", 1 => "FX", 2 => "FY", 3 => "FZ",
                    4 => "F0", 5 => "F1", 6 => "F2", 7 => "F3", 8 => "F4",
                    _ => $"F{idx}"
                };
            }
            if (regGroup == +1) // Special
            {
                return idx switch
                {
                    0 => "FP", 1 => "HP", 2 => "SP",
                    3 => "CD", 4 => "PR", 5 => "WD",
                    _ => $"S{idx}"
                };
            }
            // GP
            return idx switch
            {
                0 => "RW", 1 => "RX", 2 => "RY", 3 => "RZ",
                4 => "R0", 5 => "R1", 6 => "R2", 7 => "R3", 8 => "R4",
                _ => $"R{idx}"
            };
        }

        static string FormatInstruction(DecodedInstruction instr, long pc)
        {
            var sb = new StringBuilder();
            if (instr.Predicate > 0) sb.Append($"(p{instr.Predicate}) ");

            string prefix = GetGroupPrefix(instr.RegGroup);
            string mnemonic = GetMnemonic(instr.Opcode);
            sb.Append(prefix).Append(mnemonic);

            if (instr.Opcode.IsZeroOperand()) { /* HALT, NOP, RET */ }
            else if (instr.Fmt == -1) // J-type: [Reg]
            {
                sb.Append(' ').Append(GetRegName(instr.RegGroup, instr.Op1));
            }
            else if (instr.Fmt == 0) // R-type or S-type
            {
                if (instr.Opcode.IsSType()) // S-type: Rd, [Rb + #off]
                {
                    sb.Append(' ').Append(GetRegName(instr.RegGroup, instr.Op1));
                    sb.Append(", [").Append(GetRegName(0, instr.Op2));
                    if (instr.Immediate != 0)
                        sb.Append(" + #").Append(instr.Immediate);
                    sb.Append(']');
                }
                else // R-type: Op1, Op2, Op3
                {
                    sb.Append(' ').Append(GetRegName(instr.RegGroup, instr.Op1));
                    if (instr.Opcode == Opcode.MOV && instr.Op3 == 0)
                        sb.Append(", ").Append(GetRegName(instr.RegGroup, instr.Op2));
                    else if (instr.Opcode == Opcode.NEG || instr.Opcode == Opcode.ABS)
                        sb.Append(", ").Append(GetRegName(instr.RegGroup, instr.Op2));
                    else
                        sb.Append(", ").Append(GetRegName(instr.RegGroup, instr.Op2))
                          .Append(", ").Append(GetRegName(instr.RegGroup, instr.Op3));
                }
            }
            else if (instr.Fmt == +1) // I-type: Op1, #imm
            {
                sb.Append(' ').Append(GetRegName(instr.RegGroup, instr.Op1));
                sb.Append(", #").Append(instr.Immediate);
            }

            return sb.ToString().TrimEnd();
        }

        static string GetMnemonic(Opcode op) => op switch
        {
            Opcode.HALT  => "HALT",
            Opcode.NOP   => "NOP",
            Opcode.LIMM  => "LIMM",
            Opcode.MOV   => "MOV",
            Opcode.LD    => "LD",
            Opcode.ST    => "ST",
            Opcode.PUSH  => "PUSH",
            Opcode.POP   => "POP",
            Opcode.ADD   => "ADD",
            Opcode.SUB   => "SUB",
            Opcode.MUL   => "MUL",
            Opcode.DIV   => "DIV",
            Opcode.MOD   => "MOD",
            Opcode.NEG   => "NEG",
            Opcode.ABS   => "ABS",
            Opcode.CMP   => "CMP",
            Opcode.AND   => "AND",
            Opcode.OR    => "OR",
            Opcode.XOR   => "XOR",
            Opcode.SHL   => "SHL",
            Opcode.SHR   => "SHR",
            Opcode.JMP   => "JMP",
            Opcode.JE    => "JE",
            Opcode.JNE   => "JNE",
            Opcode.JL    => "JL",
            Opcode.JG    => "JG",
            Opcode.JLE   => "JLE",
            Opcode.JGE   => "JGE",
            Opcode.JM    => "JM",
            Opcode.CALL  => "CALL",
            Opcode.RET   => "RET",
            Opcode.IN    => "IN",
            Opcode.OUT   => "OUT",
            Opcode.SQRT  => "SQRT",
            Opcode.FTI   => "FTI",
            Opcode.ITF   => "ITF",
            Opcode.CLASS => "CLASS",
            Opcode.SWAP  => "SWAP",
            _ => "UNKNOWN"
        };
    }
}