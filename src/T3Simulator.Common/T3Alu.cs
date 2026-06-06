using System;
using System.Numerics;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Shared ALU logic for T3 processors.
    /// </summary>
    public static class T3Alu
    {
        public static BigInteger Execute(Opcode op, BigInteger a, BigInteger b, T3Config config)
        {
            switch (op)
            {
                case Opcode.ADD: return a + b;
                case Opcode.SUB: return a - b;
                case Opcode.MUL: return a * b;
                case Opcode.DIV:
                    if (b == 0) throw new DivideByZeroException();
                    // Balanced ternary floor division
                    BigInteger resDiv = BigInteger.Divide(a, b);
                    BigInteger remDiv = a % b;
                    if (remDiv != 0 && ((b.Sign < 0) != (remDiv.Sign < 0)))
                        resDiv--;
                    return resDiv;
                case Opcode.MOD:
                    if (b == 0) throw new DivideByZeroException();
                    BigInteger resMod = a % b;
                    if (resMod != 0 && ((b.Sign < 0) != (resMod.Sign < 0)))
                        resMod += b;
                    return resMod;
                case Opcode.NEG: return -a;
                case Opcode.LI: return b; // In LI, 'b' is the immediate
                case Opcode.MOV: return b;
                default:
                    throw new NotSupportedException($"ALU does not support opcode {op}. Use specialized handlers for Control Flow/Memory.");
            }
        }

        public static int Compare(BigInteger a, BigInteger b)
        {
            return a > b ? 1 : (a < b ? -1 : 0);
        }

        // Tritwise logical operations for Word27/54 would go here or stay in TritTypes
    }
}