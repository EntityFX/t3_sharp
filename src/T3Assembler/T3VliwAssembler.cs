using System;
using System.Collections.Generic;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    /// <summary>
    /// Assembler for the T3 VLIW processor.
    /// Supports bundling 3 instructions into a single Word54.
    /// Syntax for bundles: { inst1 | inst2 | inst3 }
    /// </summary>
    public class T3VliwAssembler : T3AssemblerBase
    {
        public T3VliwAssembler(T3Config config) : base(config) { }

        public override List<Int128> Assemble(string sourceCode)
        {
            _labels.Clear();
            _lines.Clear();

            // First pass: collect labels and separate bundles/instructions
            string[] rawLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int currentAddress = 0;

            foreach (var line in rawLines)
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;

                if (cleaned.EndsWith(":"))
                {
                    string label = cleaned.TrimEnd(':');
                    if (_labels.ContainsKey(label))
                        throw new Exception($"Label '{label}' is defined multiple times.");
                    _labels[label] = currentAddress;
                }
                else
                {
                    _lines.Add(cleaned);
                    currentAddress++;
                }
            }

            List<Int128> binary = new List<Int128>();
            foreach (var line in _lines)
            {
                binary.Add(AssembleLine(line));
            }

            return binary;
        }

        private Int128 AssembleLine(string line)
        {
            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new Exception("Invalid .word directive. Expected .word <value>");
                return ResolveOperandValue(parts[1]);
            }

            if (line.StartsWith("{") && line.EndsWith("}"))
            {
                return AssembleBundle(line);
            }

            // Single instruction: treat as a bundle with 2 NOPs
            var instParts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction.");

            string mnemonic = instParts[0].ToUpper();
            int op1 = 0;
            Int128 op2 = 0;
            if (instParts.Length > 1) op1 = ResolveOperand(instParts[1]);
            if (instParts.Length > 2) op2 = ResolveOperandValue(instParts[2]);

            string s0 = EncodeSlot(mnemonic, op1, op2);
            string s1 = EncodeSlot("NOP", 0, 0);
            string s2 = EncodeSlot("NOP", 0, 0);
            
            return BalancedTernary.ParseToInt128(s0 + s1 + s2);
        }

        private Int128 AssembleBundle(string line)
        {
            // Remove brackets { }
            string content = line.Substring(1, line.Length - 2);
            string[] slots = content.Split(new[] { '|' }, StringSplitOptions.None);

            if (slots.Length > 3) throw new Exception("VLIW bundle cannot contain more than 3 instructions.");

            string bundleString = "";
            for (int i = 0; i < 3; i++)
            {
                if (i < slots.Length)
                {
                    string slotLine = slots[i].Trim();
                    if (string.IsNullOrWhiteSpace(slotLine))
                    {
                        bundleString += EncodeSlot("NOP", 0, 0);
                    }
                    else
                    {
                        var parts = slotLine.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string mnemonic = parts[0].ToUpper();
                        int op1 = 0;
                        Int128 op2 = 0;
                        if (parts.Length > 1) op1 = ResolveOperand(parts[1]);
                        if (parts.Length > 2) op2 = ResolveOperandValue(parts[2]);
                        bundleString += EncodeSlot(mnemonic, op1, op2);
                    }
                }
                else
                {
                    bundleString += EncodeSlot("NOP", 0, 0);
                }
            }

            return BalancedTernary.ParseToInt128(bundleString);
        }

        private string EncodeSlot(string mnemonic, int op1, Int128 op2)
        {
            if (mnemonic == "NOP") return BalancedTernary.ToTernaryString((int)Opcode.NOP, 6) + BalancedTernary.ToTernaryString(0, 6) + BalancedTernary.ToTernaryString(0, 6);

            Opcode opcode = GetOpcode(mnemonic);
            
            // VLIW Slot (18 trits): Opcode (6), Op1 (6), Op2 (6)
            string sOp = BalancedTernary.ToTernaryString((int)opcode, 6);
            string sOp1 = BalancedTernary.ToTernaryString(op1, 6);
            string sOp2 = BalancedTernary.ToTernaryString((long)op2, 6);
            
            return sOp + sOp1 + sOp2;
        }
    }
}