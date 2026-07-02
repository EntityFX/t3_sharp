using System;
using TritTypes;

namespace T3Simulator.Common
{
    public static class T3Alu
    {
        public static TWord Execute<TWord>(Opcode op, TWord a, TWord b, T3Config config) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;
            switch (op)
            {
                case Opcode.ADD: return (TWord)(object)(da + db);
                case Opcode.SUB: return (TWord)(object)(da - db);
                case Opcode.MUL: return (TWord)(object)(da * db);
                case Opcode.DIV: return (TWord)(object)(da / db);
                case Opcode.MOD: return (TWord)(object)(da % db);
                case Opcode.NEG: return (TWord)(object)(-da);
                case Opcode.MOV: return b;
                
                case Opcode.AND: return (TWord)TritAndInternal(a, b);
                case Opcode.OR:  return (TWord)TritOrInternal(a, b);
                case Opcode.XOR: return (TWord)TritXorInternal(a, b);
                
                case Opcode.SHL:
                    return (TWord)(object)(da << (int)db.ToLong());
                case Opcode.SHR:
                    return (TWord)(object)(da >> (int)db.ToLong());
                
                default:
                    throw new NotSupportedException($"ALU does not support opcode {op}. Use specialized handlers.");
            }
        }

        public static int Compare<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            return (a, b) switch
            {
                (Word18 wa, Word18 wb) => wa > wb ? 1 : (wa < wb ? -1 : 0),
                (Word54 wa, Word54 wb) => wa > wb ? 1 : (wa < wb ? -1 : 0),
                _ => ((dynamic)a > (dynamic)b) ? 1 : (((dynamic)a < (dynamic)b) ? -1 : 0)
            };
        }

        public static TWord TritAnd<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord> => (TWord)TritAndInternal(a, b);
        public static TWord TritOr<TWord>(TWord a, TWord b)  where TWord : IT3Word<TWord> => (TWord)TritOrInternal(a, b);
        public static TWord TritXor<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord> => (TWord)TritXorInternal(a, b);

        public static TWord ShiftLeft<TWord>(TWord a, int shift) where TWord : IT3Word<TWord>
        {
            if (a is Word18 wa) return (TWord)(object)(wa << shift);
            if (a is Word54 wa54) return (TWord)(object)(wa54 << shift);
            throw new NotSupportedException($"Unsupported type for ShiftLeft: {a.GetType()}");
        }

        public static TWord ShiftRight<TWord>(TWord a, int shift) where TWord : IT3Word<TWord>
        {
            if (a is Word18 wa) return (TWord)(object)(wa >> shift);
            if (a is Word54 wa54) return (TWord)(object)(wa54 >> shift);
            throw new NotSupportedException($"Unsupported type for ShiftRight: {a.GetType()}");
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