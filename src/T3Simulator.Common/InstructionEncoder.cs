using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Encodes instructions into 18-trit words without string operations.
    /// Format: [Pred(3)] [RegGroup(1)] [Fmt(1)] [Opcode(4)] [Args(9)]
    /// Value = pred*3^15 + regGroupField*3^14 + fmtField*3^13 + opcode*3^9 + args
    /// </summary>
    public static class InstructionEncoder
    {
        // Precomputed powers for performance
        private const long P3_15 = 14348907L; // 3^15
        private const long P3_14 = 4782969L;  // 3^14
        private const long P3_13 = 1594323L;  // 3^13
        private const long P3_9  = 19683L;    // 3^9
        private const long P3_6  = 729L;      // 3^6
        private const long P3_3  = 27L;       // 3^3

        // Ranges for balanced ternary fields
        private const long RANGE_3 = 27L;     // 3^3
        private const long RANGE_6 = 729L;    // 3^6

        // Offsets for balanced ternary (range-1)/2
        private const long OFFSET_3 = 13L;    // (27-1)/2
        private const long OFFSET_6 = 364L;   // (729-1)/2

        /// <summary>
        /// Converts a signed balanced ternary value to unsigned representation for a field of given width.
        /// </summary>
        private static long ToUnsignedField(long value, long range, long offset)
        {
            if (value < -offset || value > offset)
                throw new ArgumentOutOfRangeException($"Value {value} out of range [{-offset}, {offset}]");
            return value + offset;
        }

        /// <summary>Convert RegGroup trit (-1,0,+1) to unsigned (0,1,2).</summary>
        private static long ToRegGroupField(int regGroup) => regGroup + 1;

        /// <summary>Convert Fmt trit (-1,0,+1) to unsigned (0,1,2).</summary>
        private static long ToFmtField(int fmt) => fmt + 1;

        /// <summary>Combined header: Pred*3^15 + RegGroup*3^14 + Fmt*3^13 + Opcode*3^9</summary>
        private static long EncodeHeader(int pred, int regGroup, int fmt, int opcode)
        {
            return pred * P3_15
                 + ToRegGroupField(regGroup) * P3_14
                 + ToFmtField(fmt) * P3_13
                 + opcode * P3_9;
        }

        /// <summary>R-type (Fmt=0): [Op1(3)][Op2(3)][Op3(3)]</summary>
        public static long EncodeR(int pred, int regGroup, int fmt, int opcode, int op1, int op2, int op3)
        {
            long args = ToUnsignedField(op1, RANGE_3, OFFSET_3) * P3_6
                      + ToUnsignedField(op2, RANGE_3, OFFSET_3) * P3_3
                      + ToUnsignedField(op3, RANGE_3, OFFSET_3);
            return EncodeHeader(pred, regGroup, fmt, opcode) + args;
        }

        /// <summary>I-type (Fmt=+1): [Op1(3)][Imm(6)]</summary>
        public static long EncodeI(int pred, int regGroup, int fmt, int opcode, int op1, long imm)
        {
            long args = ToUnsignedField(op1, RANGE_3, OFFSET_3) * P3_6
                      + ToUnsignedField(imm, RANGE_6, OFFSET_6);
            return EncodeHeader(pred, regGroup, fmt, opcode) + args;
        }

        /// <summary>J-type (Fmt=-1): [Reg(3)][000000]</summary>
        public static long EncodeJ(int pred, int regGroup, int fmt, int opcode, int reg)
        {
            long args = ToUnsignedField(reg, RANGE_3, OFFSET_3) * P3_6;
            return EncodeHeader(pred, regGroup, fmt, opcode) + args;
        }

        /// <summary>S-type (Fmt=0 slots for LD/ST): [Op1(3)][Op2(3)][Imm(3)]</summary>
        public static long EncodeS(int pred, int regGroup, int fmt, int opcode, int op1, int op2, long imm)
        {
            long args = ToUnsignedField(op1, RANGE_3, OFFSET_3) * P3_6
                      + ToUnsignedField(op2, RANGE_3, OFFSET_3) * P3_3
                      + ToUnsignedField(imm, RANGE_3, OFFSET_3);
            return EncodeHeader(pred, regGroup, fmt, opcode) + args;
        }

        // Word18 wrappers
        public static Word18 EncodeR18(int pred, int regGroup, int fmt, int opcode, int op1, int op2, int op3) =>
            Word18.FromLong(EncodeR(pred, regGroup, fmt, opcode, op1, op2, op3));

        public static Word18 EncodeI18(int pred, int regGroup, int fmt, int opcode, int op1, long imm) =>
            Word18.FromLong(EncodeI(pred, regGroup, fmt, opcode, op1, imm));

        public static Word18 EncodeJ18(int pred, int regGroup, int fmt, int opcode, int reg) =>
            Word18.FromLong(EncodeJ(pred, regGroup, fmt, opcode, reg));

        public static Word18 EncodeS18(int pred, int regGroup, int fmt, int opcode, int op1, int op2, long imm) =>
            Word18.FromLong(EncodeS(pred, regGroup, fmt, opcode, op1, op2, imm));
    }
}