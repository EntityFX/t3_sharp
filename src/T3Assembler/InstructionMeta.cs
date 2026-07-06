using System;
using T3Simulator.Common;

namespace T3Assembler
{
    public static class InstructionMeta
    {
        public static int GetSize(string mnemonic, string[] operands, long? immediate = null)
        {
            string mn = mnemonic.ToUpper();

            if (mn == "LIMM") return 2;

            if (IsJumpMnemonic(mn))
            {
                if (operands.Length > 0 && !IsRegister(operands[0]))
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

        private static bool IsJumpMnemonic(string m) => m is "JMP" or "JE" or "JNE" or "JL" or "JG" or "JM" or "JLE" or "JGE" or "CALL";

        private static bool IsRegister(string s)
        {
            // Simple check: starts with R, F, S or is a known special reg
            if (string.IsNullOrEmpty(s)) return false;
            char first = char.ToUpper(s[0]);
            return first == 'R' || first == 'F' || first == 'S';
        }
    }
}