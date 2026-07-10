using System;
using TritTypes;

namespace T3Simulator.Common
{
    public static class T3Alu
    {
        public static TWord Execute<TWord>(Opcode op, TWord a, TWord b, T3Config config) where TWord : IT3Word<TWord>
        {
            if (a is Word18 wa && b is Word18 wb)
                return (TWord)(object)ExecuteWord18(op, wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54)
                return (TWord)(object)ExecuteWord54(op, wa54, wb54);
            throw new NotSupportedException($"ALU does not support types: {a.GetType()}, {b.GetType()}");
        }

        private static Word18 ExecuteWord18(Opcode op, Word18 a, Word18 b) => op switch
        {
            Opcode.ADD => a + b,
            Opcode.SUB => a - b,
            Opcode.MUL => a * b,
            Opcode.DIV => a / b,
            Opcode.MOD => a % b,
            Opcode.NEG => -a,
            Opcode.AND => Word18.TritAnd(a, b),
            Opcode.OR  => Word18.TritOr(a, b),
            Opcode.XOR => Word18.TritXor(a, b),
            Opcode.SHL => a << (int)b.ToLong(),
            Opcode.SHR => a >> (int)b.ToLong(),
            Opcode.MOV => b,
            _ => throw new NotSupportedException($"ALU Word18 does not support opcode {op}")
        };

        private static Word54 ExecuteWord54(Opcode op, Word54 a, Word54 b) => op switch
        {
            Opcode.ADD => a + b,
            Opcode.SUB => a - b,
            Opcode.MUL => a * b,
            Opcode.DIV => a / b,
            Opcode.MOD => a % b,
            Opcode.NEG => -a,
            Opcode.AND => Word54.TritAnd(a, b),
            Opcode.OR  => Word54.TritOr(a, b),
            Opcode.XOR => Word54.TritXor(a, b),
            Opcode.SHL => a << (int)b.ToLong(),
            Opcode.SHR => a >> (int)b.ToLong(),
            Opcode.MOV => b,
            _ => throw new NotSupportedException($"ALU Word54 does not support opcode {op}")
        };

        public static int Compare<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            return (a, b) switch
            {
                (Word18 wa, Word18 wb) => wa > wb ? 1 : (wa < wb ? -1 : 0),
                (Word54 wa, Word54 wb) => wa > wb ? 1 : (wa < wb ? -1 : 0),
                _ => a.ToLong() > b.ToLong() ? 1 : (a.ToLong() < b.ToLong() ? -1 : 0)
            };
        }

        public static TWord TritAnd<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord> => TritOp(a, b, "AND");
        public static TWord TritOr<TWord>(TWord a, TWord b)  where TWord : IT3Word<TWord> => TritOp(a, b, "OR");
        public static TWord TritXor<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord> => TritOp(a, b, "XOR");

        private static TWord TritOp<TWord>(TWord a, TWord b, string op) where TWord : IT3Word<TWord>
        {
            if (a is Word18 wa && b is Word18 wb)
                return (TWord)(object)(op switch
                {
                    "AND" => Word18.TritAnd(wa, wb),
                    "OR"  => Word18.TritOr(wa, wb),
                    "XOR" => Word18.TritXor(wa, wb),
                    _ => throw new ArgumentException(op)
                });
            if (a is Word54 wa54 && b is Word54 wb54)
                return (TWord)(object)(op switch
                {
                    "AND" => Word54.TritAnd(wa54, wb54),
                    "OR"  => Word54.TritOr(wa54, wb54),
                    "XOR" => Word54.TritXor(wa54, wb54),
                    _ => throw new ArgumentException(op)
                });
            throw new NotSupportedException($"Unsupported types for Trit{op}: {a.GetType()}, {b.GetType()}");
        }

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
    }
}