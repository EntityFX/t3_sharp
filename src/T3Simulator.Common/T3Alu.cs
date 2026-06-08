using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Shared ALU logic for T3 processors.
    /// Provides core arithmetic and logical operations for T3 words.
    /// </summary>
    public static class T3Alu
    {
        /// <summary>
        /// Executes a basic arithmetic operation.
        /// </summary>
        public static TWord Execute<TWord>(Opcode op, TWord a, TWord b, T3Config config) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;

            switch (op)
            {
                case Opcode.ADD: return (TWord)(da + db);
                case Opcode.SUB: return (TWord)(da - db);
                case Opcode.MUL: return (TWord)(da * db);
                case Opcode.DIV: return (TWord)(da / db);
                case Opcode.MOD: return (TWord)(da % db);
                case Opcode.NEG: return (TWord)(-da);
                case Opcode.MOV: return b;
                case Opcode.LI: return b;
                
                // Tritwise operations
                case Opcode.TRITAND: return (TWord)TritAndInternal(da, db);
                case Opcode.TRITOR: return (TWord)TritOrInternal(da, db);
                case Opcode.TRITXOR: return (TWord)TritXorInternal(da, db);
                
                // Shifts
                case Opcode.SHL:
                    int shiftL = (int)db.ToLong();
                    return (TWord)(da << shiftL);
                case Opcode.SHR:
                    int shiftR = (int)db.ToLong();
                    return (TWord)(da >> shiftR);
                
                default:
                    throw new NotSupportedException($"ALU does not support opcode {op}. Use specialized handlers for Control Flow/Memory.");
            }
        }

        /// <summary>
        /// Compares two values and returns the sign of (a - b).
        /// Result: 1 if a > b, -1 if a < b, 0 if a == b.
        /// </summary>
        public static int Compare<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;
            return da > db ? 1 : (da < db ? -1 : 0);
        }

        // Specialized methods for clarity and direct access
        public static TWord TritAnd<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            return (TWord)TritAndInternal(a, b);
        }

        public static TWord TritOr<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            return (TWord)TritOrInternal(a, b);
        }

        public static TWord TritXor<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            return (TWord)TritXorInternal(a, b);
        }

        public static TWord ShiftLeft<TWord>(TWord a, int shift) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            return (TWord)(da << shift);
        }

        public static TWord ShiftRight<TWord>(TWord a, int shift) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            return (TWord)(da >> shift);
        }

        private static dynamic TritAndInternal(dynamic a, dynamic b)
        {
            if (a is Word18 wa && b is Word18 wb) return Word18.TritAnd(wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54) return Word54.TritAnd(wa54, wb54);
            throw new NotSupportedException($"Unsupported types for TritAnd: {a.GetType()}, {b.GetType()}");
        }

        private static dynamic TritOrInternal(dynamic a, dynamic b)
        {
            if (a is Word18 wa && b is Word18 wb) return Word18.TritOr(wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54) return Word54.TritOr(wa54, wb54);
            throw new NotSupportedException($"Unsupported types for TritOr: {a.GetType()}, {b.GetType()}");
        }

        private static dynamic TritXorInternal(dynamic a, dynamic b)
        {
            if (a is Word18 wa && b is Word18 wb) return Word18.TritXor(wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54) return Word54.TritXor(wa54, wb54);
            throw new NotSupportedException($"Unsupported types for TritXor: {a.GetType()}, {b.GetType()}");
        }
    }
}