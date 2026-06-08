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
            return Decode18(word);
        }

        /// <summary>
        /// Specifically decodes an 18-trit word.
        /// </summary>
        public static Instruction<TWord> Decode18<TWord>(TWord word) where TWord : IT3Word<TWord>
        {
            string s = word.ToTritString();
            if (s.Length != 18)
            {
                throw new ArgumentException($"T3-18 decoder expects 18 trits, but got {s.Length}");
            }

            // Layout: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Op3 (3) / Imm6 (6)] [Reserve (3)]
            string opPart = s.Substring(0, 6);
            string op1Part = s.Substring(6, 3);
            string op2Part = s.Substring(9, 3);
            
            long fullOpcodeVal = BalancedTernary.ParseToLong(opPart);
            
            int finalBaseOpcode;
            int finalPredIndex;
            bool isIType = false;

            // The specification says: opcode_field = base_opcode + pred_index * 28.
            // Priority resolution for overlapping ranges:
            
            if (fullOpcodeVal >= 64)
            {
                // Priority 1: I-type (shifted by 64)
                // V = (base_R + 64) + pred * 28
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
            else if (fullOpcodeVal >= 41 && fullOpcodeVal <= 44)
            {
                // Priority 2: I/O Instructions (base 41-44)
                finalBaseOpcode = (int)fullOpcodeVal;
                finalPredIndex = 0; 
            }
            else
            {
                // Priority 3: Base R-type instructions (0-27)
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

            // Map to the Opcode enum
            Opcode op;
            if (isIType)
            {
                op = (Opcode)(finalBaseOpcode + 64);
            }
            else
            {
                op = (Opcode)finalBaseOpcode;
            }

            int op1 = (int)BalancedTernary.ParseToLong(op1Part);
            int op2 = (int)BalancedTernary.ParseToLong(op2Part);
            
            if (isIType || op == Opcode.INI || op == Opcode.OUTI)
            {
                // I-type format: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Imm6 (6)]
                string immPart = s.Substring(12, 6);
                long imm = BalancedTernary.ParseToLong(immPart);
                return new Instruction<TWord>(op, finalPredIndex, op1, op2, 0, imm);
            }
            else
            {
                // R-type format: [Opcode+Pred (6)] [Op1 (3)] [Op2 (3)] [Op3 (3)] [Reserve (3)]
                string op3Part = s.Substring(12, 3);
                int op3 = (int)BalancedTernary.ParseToLong(op3Part);
                return new Instruction<TWord>(op, finalPredIndex, op1, op2, op3, 0);
            }
        }
    }
}