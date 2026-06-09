using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes ternary words into executable Instructions based on the T3 (18-trit) specification.
    /// </summary>
    public static class InstructionDecoder
    {
        /// <summary>
        /// Decodes an 18-trit word into an instruction.
        /// </summary>
        public static Instruction<TWord> Decode<TWord>(TWord word) where TWord : IT3Word<TWord>
        {
            string s = word.ToTritString();
            if (s.Length == 18)
            {
                return Decode18<TWord>(s);
            }
            else if (s.Length == 54)
            {
                // For T3-54, the instruction is encoded in the last 18 trits of the word
                // to be compatible with Word54.FromInt128(instructionValue)
                return Decode18<TWord>(s.Substring(s.Length - 18));
            }
            throw new ArgumentException($"Unsupported word length: {s.Length}");
        }

        /// <summary>
        /// Specifically decodes an 18-trit string into an instruction.
        /// </summary>
        public static Instruction<TWord> Decode18<TWord>(string s)
        {
            if (s.Length != 18)
            {
                throw new ArgumentException($"T3-18 decoder expects 18 trits, but got {s.Length}");
            }

            // Layout: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Op3 (3) / Imm6 (6)] [Reserve (3)]
            string opPart = s.Substring(0, 6);
            string op1Part = s.Substring(6, 3);
            string op2Part = s.Substring(9, 3);
            
            long fullOpcodeVal = BalancedTernary.ParseToLong(opPart);
            
            int finalBaseOpcode = 0;
            int finalPredIndex = 0;
            bool isIType = false;
            int func = 0;

            // Priority 1: FPU (92-108)
            if (fullOpcodeVal >= 92 && fullOpcodeVal <= (108 + 3 * 28))
            {
                for (int p = 0; p <= 3; p++)
                {
                    int baseOp = (int)(fullOpcodeVal - p * 28);
                    if (baseOp >= 92 && baseOp <= 108)
                    {
                        finalBaseOpcode = baseOp;
                        finalPredIndex = p;
                        
                        // FPU specific logic
                        if (finalBaseOpcode == 103 || finalBaseOpcode == 104) isIType = true;
                        
                        string reservePart = s.Substring(15, 3);
                        func = (int)BalancedTernary.ParseToLong(reservePart);
                        
                        Opcode fOp = (Opcode)finalBaseOpcode;
                        int fOp1 = (int)BalancedTernary.ParseToLong(op1Part);
                        int fOp2 = (int)BalancedTernary.ParseToLong(op2Part);
                        
                        if (isIType)
                        {
                            long imm = BalancedTernary.ParseToLong(s.Substring(12, 6));
                            return new Instruction<TWord>(fOp, finalPredIndex, fOp1, fOp2, 0, imm, func);
                        }
                        else
                        {
                            int fOp3 = (int)BalancedTernary.ParseToLong(s.Substring(12, 3));
                            return new Instruction<TWord>(fOp, finalPredIndex, fOp1, fOp2, fOp3, 0, func);
                        }
                    }
                }
            }

            // Priority 2: I-type (shifted by 64)
            if (fullOpcodeVal >= 64)
            {
                long shiftedVal = fullOpcodeVal - 64;
                finalBaseOpcode = (int)(shiftedVal % 28);
                finalPredIndex = (int)(shiftedVal / 28);
                if (finalBaseOpcode < 0)
                {
                    finalBaseOpcode += 28;
                    finalPredIndex -= 1;
                }
                isIType = true;
            }
            // Priority 3: I/O Instructions (base 41-44)
            else if (fullOpcodeVal >= 41 && fullOpcodeVal <= 44)
            {
                finalBaseOpcode = (int)fullOpcodeVal;
                finalPredIndex = 0; 
            }
            // Priority 4: Base R-type instructions (0-27)
            else
            {
                finalBaseOpcode = (int)(fullOpcodeVal % 28);
                finalPredIndex = (int)(fullOpcodeVal / 28);
                if (finalBaseOpcode < 0)
                {
                    finalBaseOpcode += 28;
                    finalPredIndex -= 1;
                }
            }

            if (finalPredIndex < 0 || finalPredIndex > 3)
                throw new InvalidOperationException($"Invalid predicate index: {finalPredIndex} for field value {fullOpcodeVal}");

            // Special case: LI is always I-type
            if (finalBaseOpcode == 4)
            {
                isIType = true;
            }

            Opcode op = isIType ? (Opcode)(finalBaseOpcode + 64) : (Opcode)finalBaseOpcode;
            int op1 = (int)BalancedTernary.ParseToLong(op1Part);
            int op2 = (int)BalancedTernary.ParseToLong(op2Part);
            
            if (isIType || op == Opcode.INI || op == Opcode.OUTI)
            {
                long imm = BalancedTernary.ParseToLong(s.Substring(12, 6));
                return new Instruction<TWord>(op, finalPredIndex, op1, op2, 0, imm, func);
            }
            else
            {
                int op3 = (int)BalancedTernary.ParseToLong(s.Substring(12, 3));
                return new Instruction<TWord>(op, finalPredIndex, op1, op2, op3, 0, func);
            }
        }
    }
}