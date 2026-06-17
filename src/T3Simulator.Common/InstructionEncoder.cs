using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Encodes instructions into 18-trit words without string operations.
    /// Format: [Pred(3)] [Opcode(6)] [Args(9)]
    /// Value = pred*3^15 + opcode*3^9 + args
    /// </summary>
    public static class InstructionEncoder
    {
        // Precomputed powers for performance
        private const long P3_15 = 14348907L; // 3^15
        private const long P3_12 = 531441L;   // 3^12
        private const long P3_9  = 19683L;    // 3^9
        private const long P3_6  = 729L;      // 3^6
        private const long P3_3  = 27L;       // 3^3

        /// <summary>R-type: [Pred(3)][Opcode(6)][Op1(3)][Op2(3)][Op3(3)]</summary>
        public static long EncodeR(int pred, int opcode, int op1, int op2, int op3)
        {
            return pred * P3_15 + opcode * P3_9 + (ToTernary(op1) * P3_6 + ToTernary(op2) * P3_3 + ToTernary(op3));
        }

        /// <summary>I-type: [Pred(3)][Opcode(6)][Op1(3)][Imm(6)]</summary>
        public static long EncodeI(int pred, int opcode, int op1, long imm)
        {
            long args = ToTernary(op1) * P3_6 + ExtendedTernary(imm, 6);
            return pred * P3_15 + opcode * P3_9 + args;
        }

        /// <summary>J-type (register-indirect): [Pred(3)][Opcode(6)][Reg(3)][000000]</summary>
        public static long EncodeJ(int pred, int opcode, int reg)
        {
            long args = ToTernary(reg) * P3_6;
            return pred * P3_15 + opcode * P3_9 + args;
        }

        /// <summary>Convert trit value (-4..+4) to its balanced ternary representation value.</summary>
        private static long ToTernary(int tritValue)
        {
            if (tritValue < -4) tritValue = -4;
            if (tritValue > 4) tritValue = 4;
            return tritValue;
        }

        /// <summary>Convert immediate to a 6-trit balanced ternary value in range [-364, 364]</summary>
        private static long ExtendedTernary(long value, int trits)
        {
            long maxVal = 1;
            for (int i = 0; i < trits; i++) maxVal *= 3;
            maxVal = (maxVal - 1) / 2; // range [-maxVal, +maxVal]
            long minVal = -maxVal;
            if (value < minVal) value = minVal;
            if (value > maxVal) value = maxVal;
            
            return value;
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