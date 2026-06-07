using System;
using System.Collections.Generic;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    /// <summary>
    /// Assembler for the T3 In-Order processor.
    /// Produces a sequence of standard 27-trit (or 54-trit) words.
    /// </summary>
    public class T3InOrderAssembler : T3AssemblerBase
    {
        public T3InOrderAssembler(T3Config config) : base(config) { }

        public override List<Int128> Assemble(string sourceCode)
        {
            _labels.Clear();
            _lines.Clear();

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

            var instParts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction.");

            string mnemonic = instParts[0].ToUpper();
            int op1 = 0;
            long op2 = 0;

            if (instParts.Length > 1) op1 = ResolveOperand(instParts[1]);
            if (instParts.Length > 2) op2 = ResolveOperandValue(instParts[2]);

            return Encode(mnemonic, op1, op2);
        }

        private Int128 Encode(string mnemonic, int op1, long op2)
        {
            Opcode opcode = GetOpcode(mnemonic);
            
            // Basic 27-trit encoding: Opcode (6), Op1 (9), Op2 (9)
            string sOp = BalancedTernary.ToTernaryString((int)opcode, 6);
            string sOp1 = BalancedTernary.ToTernaryString(op1, 9);
            string sOp2 = BalancedTernary.ToTernaryString(op2, 9);
            
            return BalancedTernary.ParseToInt128(sOp + sOp1 + sOp2 + "000");
        }
    }
}