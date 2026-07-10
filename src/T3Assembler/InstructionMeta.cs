using System;
using T3Simulator.Common;

namespace T3Assembler
{
    public static class InstructionMeta
    {
        public static int GetSize(string mnemonic, string[] operands, long? immediate = null)
        {
            string mn = mnemonic.ToUpper();

            if (mn == "LIMM")
                return 2;

            if (IsJumpMnemonic(mn))
            {
                if (operands.Length > 1 && !IsRegister(operands[1]))
                    return 3; // LIMM + JMP
                return 1;
            }

            if (mn == "MOV" || mn == "LI")
            {
                if (immediate.HasValue && (immediate.Value > 364 || immediate.Value < -364))
                    return 2; // LIMM + MOV (actually just LIMM in current implementation)
                return 1;
            }

            // General I-type instructions with large immediates
            if (immediate.HasValue && (immediate.Value > 364 || immediate.Value < -364))
                return 3; // LIMM + OP

            return 1;
        }

        private static bool IsJumpMnemonic(string m) =>
            m is "JMP" or "JE" or "JNE" or "JL" or "JG" or "JM" or "JLE" or "JGE" or "CALL";

        private static bool IsRegister(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            s = s.Trim().ToUpperInvariant();
            if (s.StartsWith("S.") || s.StartsWith("F."))
                s = s[2..];
            return s is "RW" or "RX" or "RY" or "RZ"
                     or "R0" or "R1" or "R2" or "R3" or "R4"
                     or "SP" or "FP" or "HP" or "CD" or "PR" or "WD"
                || System.Text.RegularExpressions.Regex.IsMatch(s, @"^F[0-9]+$");
        }
    }
}
