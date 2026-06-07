using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes ternary words into executable Instructions.
    /// </summary>
    public static class InstructionDecoder
    {
        /// <summary>
        /// Generic entry point to decode a word into an instruction.
        /// </summary>
        public static Instruction<TWord> Decode<TWord>(TWord word)
        {
            if (typeof(TWord) == typeof(long))
            {
                long val = (long)(object)word;
                var (opcode, pred, op1, op2) = DecodeRaw(new Word27(val).ToTritString());
                return (Instruction<TWord>)(object)new Instruction<long>((Opcode)opcode, pred, op1, op2);
            }
            if (typeof(TWord) == typeof(Int128))
            {
                Int128 val = (Int128)(object)word;
                var (opcode, pred, op1, op2) = DecodeRaw(new Word54(val).ToTritString());
                return (Instruction<TWord>)(object)new Instruction<Int128>((Opcode)opcode, pred, op1, op2);
            }
            if (word is Word27 w27)
            {
                var (opcode, pred, op1, op2) = DecodeRaw(w27.ToTritString());
                return (Instruction<TWord>)(object)new Instruction<Word27>((Opcode)opcode, pred, op1, op2);
            }
            if (word is Word54 w54)
            {
                var (opcode, pred, op1, op2) = DecodeRaw(w54.ToTritString());
                return (Instruction<TWord>)(object)new Instruction<Word54>((Opcode)opcode, pred, op1, op2);
            }

            throw new NotSupportedException($"Unsupported word type: {typeof(TWord)}");
        }

        private static (int opcode, int pred, long op1, long op2) DecodeRaw(string s)
        {
            if (s.Length > 27)
            {
                s = s.Substring(s.Length - 27);
            }

            string opPart = s.Substring(0, 6);
            string op1Part = s.Substring(6, 9);
            string op2Part = s.Substring(15, 9);

            long fullOpcodeVal = BalancedTernary.ParseToLong(opPart);
            long op1Val = BalancedTernary.ParseToLong(op1Part);
            long op2Val = BalancedTernary.ParseToLong(op2Part);

            int predIndex = (int)(fullOpcodeVal / 45);
            int baseOpcode = (int)(fullOpcodeVal % 45);

            if (baseOpcode < 0 || baseOpcode > 44)
                throw new InvalidOperationException($"Invalid base opcode: {baseOpcode}");

            return (baseOpcode, predIndex, op1Val, op2Val);
        }

        public static Instruction<Word27> Decode27(Word27 word)
        {
            var (opcode, pred, op1, op2) = DecodeRaw(word.ToTritString());
            return new Instruction<Word27>((Opcode)opcode, pred, op1, op2);
        }

        public static Instruction<Word54> Decode54(Word54 word)
        {
            var (opcode, pred, op1, op2) = DecodeRaw(word.ToTritString());
            return new Instruction<Word54>((Opcode)opcode, pred, op1, op2);
        }

        /// <summary>
        /// Decodes a VLIW slot (18 trits) into a generic Instruction.
        /// </summary>
        public static Instruction<TWord> DecodeVliwSlot<TWord>(string slotTritString)
        {
            if (slotTritString.Length != 18)
                throw new ArgumentException("VLIW slot must be 18 trits");

            string opPart = slotTritString.Substring(0, 6);
            string op1Part = slotTritString.Substring(6, 6);
            string op2Part = slotTritString.Substring(12, 6);

            long fullOpcodeVal = BalancedTernary.ParseToLong(opPart);
            long op1Val = BalancedTernary.ParseToLong(op1Part);
            long op2Val = BalancedTernary.ParseToLong(op2Part);

            int predIndex = (int)(fullOpcodeVal / 28);
            int baseOpcode = (int)(fullOpcodeVal % 28);

            if (typeof(TWord) == typeof(Word27))
            {
                return (Instruction<TWord>)(object)new Instruction<Word27>(
                    (Opcode)baseOpcode,
                    predIndex,
                    op1Val,
                    op2Val
                );
            }
            if (typeof(TWord) == typeof(Word54))
            {
                return (Instruction<TWord>)(object)new Instruction<Word54>(
                    (Opcode)baseOpcode,
                    predIndex,
                    op1Val,
                    op2Val
                );
            }

            throw new NotSupportedException($"Unsupported word type for VLIW decoding: {typeof(TWord)}");
        }
    }
}