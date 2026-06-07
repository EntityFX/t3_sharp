using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Shared ALU logic for T3 processors.
    /// Uses dynamic dispatch to support different Word types that implement basic arithmetic operators.
    /// </summary>
    public static class T3Alu
    {
    public static TWord Execute<TWord>(Opcode op, TWord a, TWord b, T3Config config) where TWord : IT3Word<TWord>
    {
        dynamic da = a;
        dynamic db = b;

        switch (op)
        {
            case Opcode.ADD: return (TWord)(da + db);
            case Opcode.SUB: return (TWord)(da - db);
            case Opcode.MUL: return (TWord)(da * db);
            case Opcode.DIV:
                if (db == 0) throw new DivideByZeroException();
                dynamic resDiv = da / db;
                dynamic remDiv = da % db;
                if (remDiv != 0 && ((db < 0) != (remDiv < 0)))
                    resDiv--;
                return (TWord)resDiv;
            case Opcode.MOD:
                if (db == 0) throw new DivideByZeroException();
                dynamic resMod = da % db;
                dynamic remMod = da % db;
                if (remMod != 0 && ((db < 0) != (remMod < 0)))
                    resMod += db;
                return (TWord)resMod;
            case Opcode.NEG: return (TWord)(-da);
            case Opcode.LI: return b;
            case Opcode.MOV: return b;
            default:
                throw new NotSupportedException($"ALU does not support opcode {op}. Use specialized handlers for Control Flow/Memory.");
        }
    }

        public static int Compare<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;
            return da > db ? 1 : (da < db ? -1 : 0);
        }

        public static TWord TritAnd<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;
            return (TWord)TritAndInternal(da, db);
        }

        public static TWord TritOr<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;
            return (TWord)TritOrInternal(da, db);
        }

        public static TWord TritXor<TWord>(TWord a, TWord b) where TWord : IT3Word<TWord>
        {
            dynamic da = a;
            dynamic db = b;
            return (TWord)TritXorInternal(da, db);
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
            if (a is Word27 wa && b is Word27 wb) return Word27.TritAnd(wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54) return Word54.TritAnd(wa54, wb54);
            throw new NotSupportedException($"Unsupported types for TritAnd: {a.GetType()}, {b.GetType()}");
        }

        private static dynamic TritOrInternal(dynamic a, dynamic b)
        {
            if (a is Word27 wa && b is Word27 wb) return Word27.TritOr(wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54) return Word54.TritOr(wa54, wb54);
            throw new NotSupportedException($"Unsupported types for TritOr: {a.GetType()}, {b.GetType()}");
        }

        private static dynamic TritXorInternal(dynamic a, dynamic b)
        {
            if (a is Word27 wa && b is Word27 wb) return Word27.TritXor(wa, wb);
            if (a is Word54 wa54 && b is Word54 wb54) return Word54.TritXor(wa54, wb54);
            throw new NotSupportedException($"Unsupported types for TritXor: {a.GetType()}, {b.GetType()}");
        }
    }
}