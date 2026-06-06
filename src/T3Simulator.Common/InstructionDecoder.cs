using System;
using System.Numerics;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes ternary words into executable Instructions.
    /// </summary>
    public static class InstructionDecoder
    {
        /// <summary>
        /// Decodes a 27-trit word into an Instruction.
        /// Format: opcode(6), operand1(9), operand2(9)
        /// Predicate index is embedded in the opcode: pred_index = opcode / 28, base_opcode = opcode % 28.
        /// </summary>
        public static Instruction Decode27(BigInteger word)
        {
            // We use Word27's ToTritString to easily extract slices, 
            // though in a production version we'd use bit/trit masking.
            string s = new Word27(word).ToTritString();

            string opPart = s.Substring(0, 6);
            string op1Part = s.Substring(6, 9);
            string op2Part = s.Substring(15, 9);

            var fullOpcodeVal = BalancedTernary.ParseToBigInteger(opPart);
            var op1Val = BalancedTernary.ParseToBigInteger(op1Part);
            var op2Val = BalancedTernary.ParseToBigInteger(op2Part);

            int predIndex = (int)(fullOpcodeVal / 28);
            int baseOpcode = (int)(fullOpcodeVal % 28);

            if (baseOpcode < 0 || baseOpcode > 44)
                throw new InvalidOperationException($"Invalid base opcode: {baseOpcode}");

            return new Instruction(
                (Opcode)baseOpcode,
                predIndex,
                op1Val,
                op2Val
            );
        }

        /// <summary>
        /// Decodes a VLIW slot (18 trits).
        /// Format: opcode(6), op1(6), op2(6)
        /// </summary>
        public static Instruction DecodeVliwSlot(string slotTritString)
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

            return new Instruction(
                (Opcode)baseOpcode,
                predIndex,
                op1Val,
                op2Val
            );
        }
    }
}