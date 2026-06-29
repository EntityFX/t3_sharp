using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Encodes instructions into 18-trit words without string operations.
    /// Format: [Pred(3)] [Opcode(6)] [Args(9)]
    /// Value = pred*3^15 + opcode*3^9 + args
    /// The args sub-fields are encoded as unsigned balanced ternary to prevent field bleeding.
    /// Pred and opcode are used as-is (they are non-negative in practice).
    /// </summary>
    public static class InstructionEncoder
    {
        // Precomputed powers for performance
        private const long P3_15 = 14348907L; // 3^15
        private const long P3_12 = 531441L;   // 3^12
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
        /// The result is in range [0, 3^width - 1].
        /// This prevents negative values from bleeding into adjacent fields.
        /// </summary>
        private static long ToUnsignedField(long value, long range, long offset)
        {
            if (value < -offset || value > offset)
                throw new ArgumentOutOfRangeException($"Immediate value {value} out of range [{-offset}, {offset}]");
            return value + offset;
        }

        /// <summary>R-type: [Pred(3)][Opcode(6)][Op1(3)][Op2(3)][Op3(3)]</summary>
        public static long EncodeR(int pred, int opcode, int op1, int op2, int op3)
        {
            long args = ToUnsignedField(op1, RANGE_3, OFFSET_3) * P3_6
                      + ToUnsignedField(op2, RANGE_3, OFFSET_3) * P3_3
                      + ToUnsignedField(op3, RANGE_3, OFFSET_3);
            return pred * P3_15 + opcode * P3_9 + args;
        }

        /// <summary>I-type: [Pred(3)][Opcode(6)][Op1(3)][Imm(6)]</summary>
        public static long EncodeI(int pred, int opcode, int op1, long imm)
        {
            long args = ToUnsignedField(op1, RANGE_3, OFFSET_3) * P3_6
                      + ToUnsignedField(imm, RANGE_6, OFFSET_6);
            return pred * P3_15 + opcode * P3_9 + args;
        }

        /// <summary>J-type (register-indirect): [Pred(3)][Opcode(6)][Reg(3)][000000]</summary>
        public static long EncodeJ(int pred, int opcode, int reg)
        {
            long args = ToUnsignedField(reg, RANGE_3, OFFSET_3) * P3_6;
            return pred * P3_15 + opcode * P3_9 + args;
        }

        /// <summary>S-type: [Pred(3)][Opcode(6)][Op1(3)][Op2(3)][Imm(3)]</summary>
        public static long EncodeS(int pred, int opcode, int op1, int op2, long imm)
        {
            long args = ToUnsignedField(op1, RANGE_3, OFFSET_3) * P3_6
                      + ToUnsignedField(op2, RANGE_3, OFFSET_3) * P3_3
                      + ToUnsignedField(imm, RANGE_3, OFFSET_3);
            return pred * P3_15 + opcode * P3_9 + args;
        }

        // Word18 wrappers
        public static Word18 EncodeR18(int pred, int opcode, int op1, int op2, int op3) =>
            Word18.FromLong(EncodeR(pred, opcode, op1, op2, op3));

        public static Word18 EncodeI18(int pred, int opcode, int op1, long imm) =>
            Word18.FromLong(EncodeI(pred, opcode, op1, imm));

        public static Word18 EncodeJ18(int pred, int opcode, int reg) =>
            Word18.FromLong(EncodeJ(pred, opcode, reg));
    }
}