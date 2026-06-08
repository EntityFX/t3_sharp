using System;
using System.Collections.Generic;
using System.Linq;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    /// <summary>
    /// Assembler for the T3 In-Order processor.
    /// Produces a sequence of 18-trit words based on the new ISA specification.
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

                int colonIdx = cleaned.IndexOf(':');
                if (colonIdx != -1 && (colonIdx == 0 || (colonIdx > 0 && cleaned.Substring(0, colonIdx).All(c => char.IsLetterOrDigit(c) || c == '_'))))
                {
                    string label = cleaned.Substring(0, colonIdx);
                    if (_labels.ContainsKey(label))
                        throw new Exception($"Label '{label}' is defined multiple times.");
                    _labels[label] = currentAddress;

                    string rest = cleaned.Substring(colonIdx + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(rest))
                    {
                        int words = CalculateLineWords(rest);
                        _lines.Add(rest);
                        currentAddress += words;
                    }
                }
                else
                {
                    int words = CalculateLineWords(cleaned);
                    _lines.Add(cleaned);
                    currentAddress += words;
                }
            }

            List<Int128> binary = new List<Int128>();
            foreach (var line in _lines)
            {
                binary.AddRange(AssembleLine(line));
            }

            return binary;
        }

        private int CalculateLineWords(string line)
        {
            if (line.StartsWith("\""))
            {
                return line.Length - 2 + 1;
            }
            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return 1;
                if (parts[1].StartsWith("\"")) return parts[1].Length - 2 + 1;
                return 1;
            }
            return 1;
        }

        private List<Int128> AssembleLine(string line)
        {
            if (line.StartsWith("\""))
            {
                return ResolveString(line);
            }

            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new Exception("Invalid .word directive. Expected .word <value>");
                
                if (parts[1].StartsWith("\""))
                {
                    return ResolveString(parts[1]);
                }
                return new List<Int128> { ResolveOperandValue(parts[1]) };
            }

            var instParts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction.");

            string mnemonic = instParts[0].ToUpper();
            
            // we need to handle different number of operands based on the mnemonic
            // standard: op1, op2, (op3 or imm)
            int op1 = 0;
            int op2 = 0;
            int op3 = 0;
            long imm = 0;

            if (instParts.Length > 1) op1 = ResolveOperand(instParts[1]);
            if (instParts.Length > 2) op2 = ResolveOperand(instParts[2]);
            if (instParts.Length > 3)
            {
                string thirdToken = instParts[3];
                if (IsRegister(thirdToken))
                    op3 = ResolveOperand(thirdToken);
                else
                    imm = (long)ResolveOperandValue(thirdToken);
            }

            return new List<Int128> { Encode(mnemonic, op1, op2, op3, imm) };
        }

        private Int128 Encode(string mnemonic, int op1, int op2, int op3, long imm)
        {
            Opcode opcode = GetOpcode(mnemonic);
            int pred = 0; // Simple assembler: no predication in source yet
            
            bool isIType = false;
            int baseOpcode = (int)opcode;

            // Check if it's one of our I-types (64-91) or specific I-type a la LI
            if ((int)opcode >= 64 || opcode == Opcode.LI_I || opcode == Opcode.INI || opcode == Opcode.OUTI)
            {
                isIType = true;
                // For encoding, we need the base_R part
                if ((int)opcode >= 64) baseOpcode = (int)opcode - 64;
                else if (opcode == Opcode.LI_I) baseOpcode = 4;
                else if (opcode == Opcode.INI) baseOpcode = 41; // Special case for I/O
                else if (opcode == Opcode.OUTI) baseOpcode = 42; // Special case for I/O
            }
            else if (opcode == Opcode.IN || opcode == Opcode.OUT)
            {
                baseOpcode = (int)opcode;
                isIType = false;
            }
            else
            {
                baseOpcode = (int)opcode;
                isIType = false;
            }

            // Calculate the 6-trit opcode field: V = base + pred * 28
            int v = baseOpcode + pred * 28;
            string sOp = BalancedTernary.ToTernaryString(v, 6);
            
            // Operands
            string sOp1 = BalancedTernary.ToTernaryString(op1, 3);
            string sOp2 = BalancedTernary.ToTernaryString(op2, 3);
            
            string sRest;
            if (isIType)
            {
                // Imm6: unsigned 0..728 (value + 364)
                long unsignedImm = imm + 364;
                sRest = BalancedTernary.ToTernaryString(unsignedImm, 6);
            }
            else
            {
                // Op3 (3) + Reserve (3)
                sRest = BalancedTernary.ToTernaryString(op3, 3) + "000";
            }

            return BalancedTernary.ParseToInt128(sOp + sOp1 + sOp2 + sRest);
        }
    }
}