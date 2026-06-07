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
                
                string line = FormatInstruction(instr, pc);
                lines.Add($"{pc:X8}: {line}");

                if (instr.Opcode == Opcode.LIMM)
                {
                    if (index + 1 < codeList.Count)
                    {
                        TWord imm = codeList[index + 1];
                        lines.Add($"  -> Immediate: {imm}");
                    }
                    else
                    {
                        lines.Add("  -> [End of memory]");
                    }
                    pc += 2;
                    index += 2;
                }
                else
                {
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
            sb.Append(mnemonic + " ");

            // Operands
            // Note: In T3, Operand1 and Operand2 can be register indices or immediate values.
            // Here we represent them as generic values.
            if (mnemonic == "HALT")
            {
                // No operands
            }
            else if (mnemonic == "LI")
            {
                sb.Append($"R{instr.Operand1}, {instr.Operand2}");
            }
            else if (mnemonic == "LIMM")
            {
                sb.Append($"R{instr.Operand1}, [next]");
            }
            else if (mnemonic == "MOV" || mnemonic == "ADD" || mnemonic == "SUB" || 
                     mnemonic == "MUL" || mnemonic == "DIV" || mnemonic == "MOD" || 
                     mnemonic == "CMP" || mnemonic == "TRITAND" || mnemonic == "TRITOR" || 
                     mnemonic == "TRITXOR" || mnemonic == "SHL" || mnemonic == "SHR")
            {
                sb.Append($"R{instr.Operand1}, R{instr.Operand2}");
            }
            else if (mnemonic == "NEG")
            {
                sb.Append($"R{instr.Operand1}");
            }
            else if (mnemonic == "LOAD" || mnemonic == "STORE")
            {
                // LOAD R1, R2 -> R1 = mem[R2]
                // STORE R1, R2 -> mem[R2] = R1
                sb.Append($"R{instr.Operand1}, R{instr.Operand2}");
            }
            else if (mnemonic == "JMP" || mnemonic == "JE" || mnemonic == "JNE" || 
                     mnemonic == "JL" || mnemonic == "JG" || mnemonic == "JM")
            {
                sb.Append($"R{instr.Operand1}");
            }
            else if (mnemonic == "CALL")
            {
                sb.Append($"R{instr.Operand1}");
            }
            else if (mnemonic == "RET")
            {
                // No operands
            }
            else if (mnemonic == "PUSH" || mnemonic == "POP")
            {
                sb.Append($"R{instr.Operand1}");
            }
            else if (mnemonic == "IN" || mnemonic == "OUT")
            {
                sb.Append($"R{instr.Operand1}, R{instr.Operand2}");
            }
            else if (mnemonic == "INI" || mnemonic == "OUTI")
            {
                sb.Append($"R{instr.Operand1}, {instr.Operand2}");
            }
            else
            {
                sb.Append($"R{instr.Operand1}, R{instr.Operand2}");
            }

            return sb.ToString();
        }

        private static string GetMnemonic(Opcode op)
        {
            return op switch
            {
                Opcode.HALT => "HALT",
                Opcode.LOAD => "LOAD",
                Opcode.STORE => "STORE",
                Opcode.MOV => "MOV",
                Opcode.LI => "LI",
                Opcode.LIMM => "LIMM",
                Opcode.ADD => "ADD",
                Opcode.SUB => "SUB",
                Opcode.MUL => "MUL",
                Opcode.DIV => "DIV",
                Opcode.MOD => "MOD",
                Opcode.NEG => "NEG",
                Opcode.TRITAND => "TRITAND",
                Opcode.TRITOR => "TRITOR",
                Opcode.TRITXOR => "TRITXOR",
                Opcode.SHL => "SHL",
                Opcode.SHR => "SHR",
                Opcode.CMP => "CMP",
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