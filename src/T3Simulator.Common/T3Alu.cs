using System;

namespace T3Simulator.Common
{
    /// <summary>
    /// Shared ALU logic for T3 processors.
    /// Uses dynamic dispatch to support different Word types that implement basic arithmetic operators.
    /// </summary>
    public static class T3Alu
    {
        public static T Execute<T>(Opcode op, T a, T b, T3Config config)
        {
            dynamic da = a;
            dynamic db = b;

            switch (op)
            {
                case Opcode.ADD: return (T)(da + db);
                case Opcode.SUB: return (T)(da - db);
                case Opcode.MUL: return (T)(da * db);
                case Opcode.DIV:
                    if (db == 0) throw new DivideByZeroException();
                    // Balanced ternary floor division: result = floor(a/b)
                    dynamic resDiv = da / db;
                    dynamic remDiv = da % db;
                    if (remDiv != 0 && ((db < 0) != (remDiv < 0)))
                        resDiv--;
                    return (T)resDiv;
                case Opcode.MOD:
                    if (db == 0) throw new DivideByZeroException();
                    dynamic resMod = da % db;
                    if (resMod != 0 && ((db < 0) != (resMod < 0)))
                        resMod += db;
                    return (T)resMod;
                case Opcode.NEG: return (T)(-da);
                case Opcode.LI: return b; // In LI, 'b' is the immediate
                case Opcode.MOV: return b;
                default:
                    throw new NotSupportedException($"ALU does not support opcode {op}. Use specialized handlers for Control Flow/Memory.");
            }
        }

        public static int Compare<T>(T a, T b)
        {
            dynamic da = a;
            dynamic db = b;
            return da > db ? 1 : (da < db ? -1 : 0);
        }
    }
}