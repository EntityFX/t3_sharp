using System;
using System.Collections.Generic;
using System.Text;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Provides functionality to disassemble T3 machine code into human-readable assembly.
    /// </summary>
    public static class T3Disassembler
    {
        /// <summary>
        /// Disassembles a sequence of words into assembly instructions.
        /// </summary>
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
                    {
                        TWord immWord = codeList[index + 1];
                        immVal = immWord.ToInt128().ToString();
                    }

                    string line = $"LIMM {GetRegName(instr.Op1)}, {immVal}";
                    lines.Add($"{pc:X8}: {line}");

                    pc += 2;
                    index += 2;
                }
                else
                {
                    string line = FormatInstruction(instr, pc);
                    lines.Add($"{pc:X8}: {line}");
                    pc += 1;
                    index += 1;
                }
            }

            return lines;
        }

        private static string GetRegName(int index)
        {
            return index switch
            {
                0 => "RW",
                1 => "RX",
                2 => "RY",
                3 => "RZ",
                4 => "R0",
                5 => "R1",
                6 => "R2",
                7 => "R3",
                8 => "R4",
                _ when index >= 9 && index <= 30 => $"R{index - 4}",
                _ => $"R{index}"
            };
        }

        private static string FormatInstruction<TWord>(Instruction<TWord> instr, long pc)
        {
            StringBuilder sb = new StringBuilder();

            // Predicate
            if (instr.PredicateIndex > 0)
            {
                sb.Append($"(p{instr.PredicateIndex}) ");
            }

            // Opcode
            string mnemonic = GetMnemonic(instr.Opcode);
            sb.Append(mnemonic);

            // Operands
            if (mnemonic == "HALT" || mnemonic == "RET" || mnemonic == "NOP")
            {
                // No operands
            }
            else if (mnemonic == "LI")
            {
                sb.Append($" {GetRegName(instr.Op1)}, {instr.Immediate}");
            }
            else if (mnemonic == "NEG")
            {
                sb.Append($" {GetRegName(instr.Op1)}");
            }
            else if (mnemonic == "MOV" || mnemonic == "CMP")
            {
                sb.Append($" {GetRegName(instr.Op1)}, {GetRegName(instr.Op2)}");
            }
            else if (mnemonic == "ADD" || mnemonic == "SUB" ||
                     mnemonic == "MUL" || mnemonic == "DIV" || mnemonic == "MOD" ||
                     mnemonic == "AND" || mnemonic == "OR" ||
                     mnemonic == "XOR" || mnemonic == "SHL" || mnemonic == "SHR")
            {
                if (instr.Op1 == instr.Op2)
                {
                    sb.Append($" {GetRegName(instr.Op1)}, {GetRegName(instr.Op3)}");
                }
                else
                {
                    sb.Append($" {GetRegName(instr.Op1)}, {GetRegName(instr.Op2)}, {GetRegName(instr.Op3)}");
                }
            }
            else if (mnemonic == "LOAD" || mnemonic == "STORE")
            {
                sb.Append($" {GetRegName(instr.Op1)}, {GetRegName(instr.Op2)}");
            }
                else if (mnemonic == "JMP" || mnemonic == "JE" || mnemonic == "JNE" ||
                      mnemonic == "JL" || mnemonic == "JG" || mnemonic == "JM" || 
                      mnemonic == "JLE" || mnemonic == "JGE" || mnemonic == "CALL")
                {
                    if (instr.Immediate == 0)
                    {
                        sb.Append($" {GetRegName(instr.Op2)}");
                    }
                    else
                    {
                        sb.Append($" {instr.Immediate}");
                    }
                }
            else if (mnemonic == "PUSH" || mnemonic == "POP")
            {
                sb.Append($" {GetRegName(instr.Op1)}");
            }
            else if (mnemonic == "IN" || mnemonic == "OUT")
            {
                sb.Append($" {GetRegName(instr.Op1)}, {GetRegName(instr.Op2)}");
            }
            else if (mnemonic == "INI" || mnemonic == "OUTI")
            {
                sb.Append($" {GetRegName(instr.Op1)}, {instr.Immediate}");
            }
            else
            {
                sb.Append($" {GetRegName(instr.Op1)}, {GetRegName(instr.Op2)}");
            }

            return sb.ToString().TrimEnd();
        }

        private static string GetMnemonic(Opcode op)
        {
            return op switch
            {
                Opcode.HALT => "HALT",
                Opcode.LOAD => "LOAD",
                Opcode.LOADI => "LOADI",
                Opcode.STORE => "STORE",
                Opcode.STOREI => "STOREI",
                Opcode.MOV => "MOV",
                Opcode.MOVI => "MOVI",
                Opcode.LI => "LI",
                Opcode.LIMM => "LIMM",
                Opcode.ADD => "ADD",
                Opcode.ADDI => "ADDI",
                Opcode.SUB => "SUB",
                Opcode.SUBI => "SUBI",
                Opcode.MUL => "MUL",
                Opcode.MULI => "MULI",
                Opcode.DIV => "DIV",
                Opcode.DIVI => "DIVI",
                Opcode.MOD => "MOD",
                Opcode.MODI => "MODI",
                Opcode.NEG => "NEG",
                Opcode.NEGI => "NEGI",
                Opcode.AND => "AND",
                Opcode.ANDI => "ANDI",
                Opcode.OR => "OR",
                Opcode.ORI => "ORI",
                Opcode.XOR => "XOR",
                Opcode.XORI => "XORI",
                Opcode.SHL => "SHL",
                Opcode.SHLI => "SHLI",
                Opcode.SHR => "SHR",
                Opcode.SHRI => "SHRI",
                Opcode.CMP => "CMP",
                Opcode.CMPI => "CMPI",
                Opcode.JMP => "JMP",
                Opcode.JE => "JE",
                Opcode.JNE => "JNE",
                Opcode.JL => "JL",
                Opcode.JG => "JG",
                Opcode.JLE => "JLE",
                Opcode.JGE => "JGE",
                Opcode.JM => "JM",
                Opcode.CALL => "CALL",
                Opcode.RET => "RET",
                Opcode.PUSH => "PUSH",
                Opcode.POP => "POP",
                Opcode.IN => "IN",
                Opcode.OUT => "OUT",
                Opcode.INI => "INI",
                Opcode.OUTI => "OUTI",
                Opcode.FADD => "FADD",
                Opcode.FSUB => "FSUB",
                Opcode.FMUL => "FMUL",
                Opcode.FDIV => "FDIV",
                Opcode.FSQRT => "FSQRT",
                Opcode.FABS => "FABS",
                Opcode.FNEG => "FNEG",
                Opcode.FCMP => "FCMP",
                Opcode.FTOI => "FTOI",
                Opcode.ITOF => "ITOF",
                Opcode.FTOF => "FTOF",
                Opcode.FLW => "FLW",
                Opcode.FSW => "FSW",
                Opcode.FMOV => "FMOV",
                Opcode.FCLASS => "FCLASS",
                Opcode.FSWAP => "FSWAP",
                Opcode.FZERO => "FZERO",
                _ => "UNKNOWN"
            };
        }
    }
}