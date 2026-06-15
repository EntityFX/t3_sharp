using System;
using System.Collections.Generic;
using System.Linq;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    public class T3InOrderAssembler : T3AssemblerBase
    {
        public T3InOrderAssembler(T3Config config) : base(config) { }

        public override List<Int128> Assemble(string sourceCode)
        {
            _labels.Clear();
            string[] rawLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // ---- PASS 1: collect labels ----
            int currentAddress = 0;
            foreach (var line in rawLines)
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;

                int colonIdx = cleaned.IndexOf(':');
                if (colonIdx != -1 && colonIdx > 0 && cleaned.Substring(0, colonIdx).All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    string label = cleaned.Substring(0, colonIdx);
                    if (!_labels.ContainsKey(label)) _labels[label] = currentAddress;
                    string rest = cleaned.Substring(colonIdx + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(rest)) currentAddress += CountWords(rest);
                }
                else
                {
                    currentAddress += CountWords(cleaned);
                }
            }

            // ---- PASS 2: assemble ----
            List<Int128> binary = new();
            foreach (var line in rawLines)
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;
                int colonIdx = cleaned.IndexOf(':');
                string instr = colonIdx != -1 ? cleaned.Substring(colonIdx + 1).Trim() : cleaned;
                if (!string.IsNullOrWhiteSpace(instr)) binary.AddRange(AssembleLine(instr));
            }
            return binary;
        }

        private int CountWords(string line)
        {
            if (line.StartsWith("\"")) return line.Length - 2 + 1;
            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || !parts[1].StartsWith("\"")) return 1;
                return parts[1].Length - 2 + 1;
            }
            var p = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length == 0) return 0;
            if (p[0].ToUpper() == "LIMM") return 2;
            // LI with a label/forward-reference → assume LIMM (2 words)
            if (p.Length >= 3 && p[0].ToUpper() == "LI")
            {
                // If it's a plain integer literal in range, 1 word
                if (long.TryParse(p[2], out long n) && n >= -364 && n <= 364) return 1;
                // ternary/9ary/27ary literals are always in range
                if (p[2].StartsWith("t") || p[2].StartsWith("0n") || p[2].StartsWith("0y")) return 1;
                // backward label reference → check its address
                if (_labels.TryGetValue(p[2], out int addr) && addr >= -364 && addr <= 364) return 1;
                // forward reference or out‑of‑range → LIMM
                return 2;
            }
            return 1;
        }

        private List<Int128> AssembleLine(string line)
        {
            if (line.StartsWith("\"")) return ResolveString(line);
            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new Exception("Invalid .word directive");
                if (parts[1].StartsWith("\"")) return ResolveString(parts[1]);
                return new List<Int128> { ResolveOperandValue(parts[1]) };
            }

            var instParts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction");

            string mnemonic = instParts[0].ToUpper();
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            if (instParts.Length > 1) op1 = ResolveOperand(instParts[1]);

            // === ASM‑11: Auto‑promote LI→LIMM for immediates > 364 ===
            if (mnemonic == "LI" && instParts.Length >= 3)
            {
                long rawVal = (long)ResolveOperandValue(instParts[2]);
                imm = rawVal;
                if (rawVal > 364 || rawVal < -364)
                {
                    // Use LIMM + data word
                    return new List<Int128>
                    {
                        Encode("LIMM", op1, 0, 0, 0),
                        ResolveOperandValue(instParts[2])
                    };
                }
            }
            else if (mnemonic == "LIMM")
            {
                // second word handled below
            }
            else if (mnemonic == "INI" || mnemonic == "OUTI")
            {
                if (instParts.Length > 2) imm = (long)ResolveOperandValue(instParts[2]);
            }
            else
            {
                if (instParts.Length > 2) op2 = ResolveOperand(instParts[2]);
                if (instParts.Length > 3)
                {
                    string third = instParts[3];
                    if (IsRegister(third)) op3 = ResolveOperand(third);
                    else imm = (long)ResolveOperandValue(third);
                }
                else if (IsRTypeArithmetic(mnemonic))
                {
                    op3 = op2; op2 = op1;
                }
            }

            if (mnemonic == "LIMM")
            {
                return new List<Int128>
                {
                    Encode(mnemonic, op1, 0, 0, 0),
                    ResolveOperandValue(instParts[2])
                };
            }

            return new List<Int128> { Encode(mnemonic, op1, op2, op3, imm) };
        }

        private bool IsRTypeArithmetic(string m) => m is "ADD" or "SUB" or "MUL" or "DIV" or "MOD"
            or "AND" or "TRITAND" or "OR" or "TRITOR" or "XOR" or "TRITXOR" or "SHL" or "SHR";

        private Int128 Encode(string mnemonic, int op1, int op2, int op3, long imm)
        {
            Opcode opcode = GetOpcode(mnemonic);
            int pred = 0;
            bool isIType = false;
            int baseOpcode = (int)opcode;

            if ((int)opcode >= 64 || opcode == Opcode.LI || opcode == Opcode.LI_I
                || opcode == Opcode.INI || opcode == Opcode.OUTI)
            {
                if (opcode == Opcode.LIMM) { isIType = false; baseOpcode = (int)opcode; }
                else if (opcode == Opcode.INI || opcode == Opcode.OUTI) { isIType = true; baseOpcode = (int)opcode; }
                else { isIType = true; baseOpcode = (int)opcode < 64 ? (int)opcode + 64 : (int)opcode; }
            }
            else if (opcode == Opcode.IN || opcode == Opcode.OUT) { baseOpcode = (int)opcode; isIType = false; }
            else if ((int)opcode >= 92 && (int)opcode <= 108) { baseOpcode = (int)opcode; isIType = (opcode == Opcode.FLW || opcode == Opcode.FSW); }
            else { baseOpcode = (int)opcode; isIType = false; }

            int v = baseOpcode + pred * 28;
            string sOp = BalancedTernary.ToTernaryString(v, 6);
            string sOp1 = BalancedTernary.ToTernaryString(op1, 3);
            string sOp2 = BalancedTernary.ToTernaryString(op2, 3);
            string sRest = isIType ? BalancedTernary.ToTernaryString(imm, 6) : BalancedTernary.ToTernaryString(op3, 3) + "000";
            return BalancedTernary.ParseToInt128(sOp + sOp1 + sOp2 + sRest);
        }
    }
}