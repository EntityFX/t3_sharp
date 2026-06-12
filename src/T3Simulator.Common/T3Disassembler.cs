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
                    
                    string line = $"LIMM R{instr.Op1}, {immVal}";
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
            if (mnemonic == "HALT" || mnemonic == "RET")
            {
                // No operands
            }
            else if (mnemonic == "LI")
            {
                sb.Append($" R{instr.Op1}, {instr.Immediate}");
            }
            else if (mnemonic == "NEG")
            {
                sb.Append($" R{instr.Op1}");
            }
            else if (mnemonic == "MOV" || mnemonic == "ADD" || mnemonic == "SUB" || 
                     mnemonic == "MUL" || mnemonic == "DIV" || mnemonic == "MOD" || 
                     mnemonic == "CMP" || mnemonic == "TRITAND" || mnemonic == "TRITOR" || 
                     mnemonic == "TRITXOR" || mnemonic == "SHL" || mnemonic == "SHR")
            {
                if (instr.Op1 == instr.Op2)
                {
                    sb.Append($" R{instr.Op1}, R{instr.Op3}");
                }
                else
                {
                    sb.Append($" R{instr.Op1}, R{instr.Op2}, R{instr.Op3}");
                }
            }
            else if (mnemonic == "LOAD" || mnemonic == "STORE")
            {
                sb.Append($" R{instr.Op1}, R{instr.Op2}");
            }
            else if (mnemonic == "JMP" || mnemonic == "JE" || mnemonic == "JNE" || 
                     mnemonic == "JL" || mnemonic == "JG" || mnemonic == "JM" || mnemonic == "CALL")
            {
                sb.Append($" R{instr.Op1}");
            }
            else if (mnemonic == "PUSH" || mnemonic == "POP")
            {
                sb.Append($" R{instr.Op1}");
            }
            else if (mnemonic == "IN" || mnemonic == "OUT")
            {
                sb.Append($" R{instr.Op1}, R{instr.Op2}");
            }
            else if (mnemonic == "INI" || mnemonic == "OUTI")
            {
                sb.Append($" R{instr.Op1}, {instr.Immediate}");
            }
            else
            {
                sb.Append($" R{instr.Op1}, R{instr.Op2}");
            }

            return sb.ToString().TrimEnd();
        }

        private static string GetMnemonic(Opcode op)
        {
            return op switch
            {
                Opcode.HALT => "HALT",
                Opcode.LOAD or Opcode.LOADI => "LOAD",
                Opcode.STORE or Opcode.STOREI => "STORE",
                Opcode.MOV or Opcode.MOVI => "MOV",
                Opcode.LI or Opcode.LI_I => "LI",
                Opcode.LIMM => "LIMM",
                Opcode.ADD or Opcode.ADDI => "ADD",
                Opcode.SUB or Opcode.SUBI => "SUB",
                Opcode.MUL or Opcode.MULI => "MUL",
                Opcode.DIV or Opcode.DIVI => "DIV",
                Opcode.MOD or Opcode.MODI => "MOD",
                Opcode.NEG or Opcode.NEGI => "NEG",
                Opcode.TRITAND or Opcode.TRITANDI => "TRITAND",
                Opcode.TRITOR or Opcode.TRITORI => "TRITOR",
                Opcode.TRITXOR or Opcode.TRITXORI => "TRITXOR",
                Opcode.SHL or Opcode.SHLI => "SHL",
                Opcode.SHR or Opcode.SHRI => "SHR",
                Opcode.CMP or Opcode.CMPI => "CMP",
                Opcode.JMP => "JMP",
                Opcode.JE => "JE",
                Opcode.JNE => "JNE",
                Opcode.JL => "JL",
                Opcode.JG => "JG",
                Opcode.JM => "JM",
                Opcode.CALL => "CALL",
                Opcode.RET => "RET",
                Opcode.PUSH => "PUSH",
                Opcode.POP => "POP",
                Opcode.IN => "IN",
                Opcode.OUT => "OUT",
                Opcode.INI => "INI",
                Opcode.OUTI => "OUTI",
                _ => "UNKNOWN"
            };
        }
    }
}