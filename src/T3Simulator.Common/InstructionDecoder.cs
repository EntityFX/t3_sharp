using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes ternary words into executable Instructions based on the T3 (18-trit) specification.
    /// Format: [Pred (3)] [Opcode (6)] [Args (9)]
    /// </summary>
    public static class InstructionDecoder
    {
        public static Instruction<TWord> Decode<TWord>(TWord word) where TWord : IT3Word<TWord>
        {
            string s = word.ToTritString();
            if (s.Length == 18)
            {
                return Decode18<TWord>(s);
            }
            else if (s.Length == 54)
            {
                // For T3-54, the instruction is encoded in the last 18 trits
                return Decode18<TWord>(s.Substring(s.Length - 18));
            }
            throw new ArgumentException($"Unsupported word length: {s.Length}");
        }

        public static Instruction<TWord> Decode18<TWord>(string s)
        {
            if (s.Length != 18)
            {
                throw new ArgumentException($"T3-18 decoder expects 18 trits, but got {s.Length}");
            }

            // [Pred (0-2)] [Op (3-8)] [Args (9-17)]
            string predPart = s.Substring(0, 3);
            string opPart = s.Substring(3, 6);
            string argsPart = s.Substring(9, 9);

            int predIndex = (int)BalancedTernary.ParseToLong(predPart);
            int opVal = (int)BalancedTernary.ParseToLong(opPart);
            
            // Cap predicate index to valid range [0, 3] to prevent simulator crashes
            if (predIndex < 0) predIndex = 0;
            if (predIndex > 3) predIndex = 3;

            Opcode op = (Opcode)opVal;

            // Decode arguments based on Opcode type
            if (IsRType(op))
            {
                int op1 = (int)BalancedTernary.ParseToLong(argsPart.Substring(0, 3));
                int op2 = (int)BalancedTernary.ParseToLong(argsPart.Substring(3, 3));
                int op3 = (int)BalancedTernary.ParseToLong(argsPart.Substring(6, 3));
                return new Instruction<TWord>(op, predIndex, op1, op2, op3, 0, 0);
            }
            else if (IsIType(op))
            {
                int op1 = (int)BalancedTernary.ParseToLong(argsPart.Substring(0, 3));
                long imm = BalancedTernary.ParseToLong(argsPart.Substring(3, 6));
                return new Instruction<TWord>(op, predIndex, op1, 0, 0, imm, 0);
            }
            else if (IsRType(op) && (op == Opcode.ITOF))
            {
                int op1 = (int)BalancedTernary.ParseToLong(argsPart.Substring(0, 3));
                int op2 = (int)BalancedTernary.ParseToLong(argsPart.Substring(3, 3));
                return new Instruction<TWord>(op, predIndex, op1, op2, 0, 0, 0);
            }
            else if (IsJType(op))
            {
                string regPart = argsPart.Substring(0, 3);
                string immPart = argsPart.Substring(3, 6);

                if (immPart == "000000")
                {
                    int regIdx = (int)BalancedTernary.ParseToLong(regPart);
                    return new Instruction<TWord>(op, predIndex, 0, regIdx, 0, 0, 0);
                }
                else
                {
                    long target = BalancedTernary.ParseToLong(argsPart);
                    return new Instruction<TWord>(op, predIndex, (int)target, 0, 0, target, 0);
                }
            }
            else
            {
                // Default fallback for unknown or simple opcodes (like HALT, RET)
                // For instructions that might use the 'func' field (last 3 trits of argsPart)
                int func = (int)BalancedTernary.ParseToLong(argsPart.Substring(6, 3));
                return new Instruction<TWord>(op, predIndex, 0, 0, 0, 0, func);
            }
        }

        private static bool IsRType(Opcode op)
        {
            return op switch
            {
                Opcode.ADD or Opcode.SUB or Opcode.MUL or Opcode.DIV or Opcode.MOD or Opcode.NEG or
                Opcode.AND or Opcode.OR or Opcode.XOR or 
                Opcode.SHL or Opcode.SHR or 
                Opcode.MOV or Opcode.CMP or
                Opcode.LOAD or Opcode.STORE or Opcode.PUSH or Opcode.POP or
                Opcode.IN or Opcode.OUT or
                Opcode.FADD or Opcode.FSUB or Opcode.FMUL or 
                Opcode.FABS or Opcode.FNEG or Opcode.FCMP or Opcode.FTOF or Opcode.FSWAP or
                Opcode.FMOV or Opcode.FTOI or Opcode.FCLASS => true,
                _ => false
            };
        }

        private static bool IsIType(Opcode op)
        {
            return op switch
            {
                Opcode.MOVI or Opcode.LI or Opcode.LIMM or 
                Opcode.ADDI or Opcode.SUBI or Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or
                Opcode.ANDI or Opcode.ORI or Opcode.XORI or 
                Opcode.SHLI or Opcode.SHRI or 
                Opcode.LOADI or Opcode.STOREI or
                Opcode.CMPI or Opcode.INI or Opcode.OUTI or
                Opcode.FLW or Opcode.FSW or Opcode.FZERO => true,
                _ => false
            };
        }

        private static bool IsJType(Opcode op)
        {
            return op switch
            {
                Opcode.JMP or Opcode.JE or Opcode.JNE or Opcode.JL or Opcode.JG or Opcode.JM or Opcode.JLE or Opcode.JGE or 
                Opcode.CALL => true,
                _ => false
            };
        }
    }
}