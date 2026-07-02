using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes 18-trit instructions into opcode and operands.
    /// Format: [Pred(3)] [RegGroup(1)] [Fmt(1)] [Opcode(4)] [Args(9)]
    /// </summary>
    public static class InstructionDecoder
    {
        private const long P3_15 = 14348907L; // 3^15
        private const long P3_14 = 4782969L;  // 3^14
        private const long P3_13 = 1594323L;  // 3^13
        private const long P3_9  = 19683L;    // 3^9
        private const long P3_6  = 729L;      // 3^6
        private const long P3_3  = 27L;       // 3^3

        private const long OFFSET_3 = 13L;    // (27-1)/2
        private const long OFFSET_6 = 364L;   // (729-1)/2

        private static long ExtractUnsigned(Int128 value, int startPos, int width)
        {
            long divisor = Pow3(startPos);
            long modulo = Pow3(width);
            return (long)(value / divisor % modulo);
        }

        private static long ExtractBalanced(Int128 value, int startPos, int width, long offset)
        {
            return ExtractUnsigned(value, startPos, width) - offset;
        }

        private static long Pow3(int exp)
        {
            long result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }

        public static DecodedInstruction Decode(Word18 word)
        {
            Int128 val = word.ToInt128();

            // Extract: Pred(3) at pos 15, RegGroup(1) at pos 14, Fmt(1) at pos 13, Opcode(4) at pos 9
            int pred          = (int)ExtractUnsigned(val, 15, 3);
            int regGroupField = (int)ExtractUnsigned(val, 14, 1);
            int fmtField      = (int)ExtractUnsigned(val, 13, 1);
            int opcodeVal     = (int)ExtractUnsigned(val,  9, 4);

            // Convert fields back to trits
            int regGroup = regGroupField - 1; // 0→-1, 1→0, 2→+1
            int fmt      = fmtField - 1;      // 0→-1, 1→0, 2→+1

            var op = (Opcode)opcodeVal;
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            if (fmt == -1) // J-type: [Reg(3)][000000]
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                imm = 0;
            }
            else if (fmt == 0) // R-type or S-type
            {
                if (op == Opcode.LD || op == Opcode.ST) // S-type: [Op1(3)][Op2(3)][Imm(3)]
                {
                    op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                    op2 = (int)ExtractBalanced(val, 3, 3, OFFSET_3);
                    imm = ExtractBalanced(val, 0, 3, OFFSET_3);
                }
                else // R-type: [Op1(3)][Op2(3)][Op3(3)]
                {
                    op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                    op2 = (int)ExtractBalanced(val, 3, 3, OFFSET_3);
                    op3 = (int)ExtractBalanced(val, 0, 3, OFFSET_3);
                }
            }
            else // fmt == +1 → I-type: [Op1(3)][Imm(6)]
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                imm = ExtractBalanced(val, 0, 6, OFFSET_6);
            }

            return new DecodedInstruction(op, pred, regGroup, fmt, op1, op2, op3, imm);
        }

        public static DecodedInstruction Decode(Word54 word)
        {
            return Decode(Word18.FromWrappedLong(word.ToAddress()));
        }

        public static DecodedInstruction Decode<TWord>(TWord word) where TWord : IT3Word<TWord>
        {
            if (word is Word54 w54) return Decode(w54);
            if (word is Word18 w18) return Decode(w18);
            return Decode(Word18.FromLong(word.ToLong()));
        }
    }

    public struct DecodedInstruction
    {
        private const int REGISTER_OFFSET = 4;

        public Opcode Opcode;
        public int Predicate;
        /// <summary>Register group: -1=FPU, 0=GP, +1=Special</summary>
        public int RegGroup;
        /// <summary>Format: -1=J-type, 0=R/S-type, +1=I-type</summary>
        public int Fmt;
        public int Op1;
        public int Op2;
        public int Op3;
        public long Immediate;

        public DecodedInstruction(Opcode op, int pred, int regGroup, int fmt,
            int op1, int op2, int op3, long imm)
        {
            Opcode = op; Predicate = pred;
            RegGroup = regGroup; Fmt = fmt;
            Op1 = op1; Op2 = op2; Op3 = op3; Immediate = imm;
        }

        /// <summary>Physical GP register index (balanced→0..8 via +4 offset). For FPU/Special use Op1 directly.</summary>
        public int PhysOp1 => Op1 == 9 ? 9 : Op1 + REGISTER_OFFSET;
        public int PhysOp2 => Op2 == 9 ? 9 : Op2 + REGISTER_OFFSET;
        public int PhysOp3 => Op3 == 9 ? 9 : Op3 + REGISTER_OFFSET;
    }
}