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
            
            var instParts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length > 0 && instParts[0].ToUpper() == "LIMM")
            {
                return 2;
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
            
            if (mnemonic == "LI")
            {
                if (instParts.Length > 2) imm = (long)ResolveOperandValue(instParts[2]);
            }
            else if (mnemonic == "LIMM")
            {
                // LIMM operand 2 is the immediate in the next word, not a register
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
                    string thirdToken = instParts[3];
                    if (IsRegister(thirdToken))
                        op3 = ResolveOperand(thirdToken);
                    else
                        imm = (long)ResolveOperandValue(thirdToken);
                }
                else if (IsRTypeArithmetic(mnemonic))
                {
                    // Support 2-operand form for arithmetic: op1 = op1 <op> op2
                    int temp = op2;
                    op2 = op1;
                    op3 = temp;
                }
            }

            if (mnemonic == "LIMM")
            {
                return new List<Int128> 
                { 
                    Encode(mnemonic, op1, op2, op3, imm), 
                    ResolveOperandValue(instParts[2]) 
                };
            }

            return new List<Int128> { Encode(mnemonic, op1, op2, op3, imm) };
        }

        private bool IsRTypeArithmetic(string mnemonic)
        {
            return mnemonic switch
            {
                "MOV" or "CMP" or "ADD" or "SUB" or "MUL" or "DIV" or "MOD" or 
                "AND" or "TRITAND" or "OR" or "TRITOR" or "XOR" or "TRITXOR" or "SHL" or "SHR" => true,
                _ => false
            };
        }

        private Int128 Encode(string mnemonic, int op1, int op2, int op3, long imm)
        {
            Opcode opcode = GetOpcode(mnemonic);
            int pred = 0; // Simple assembler: no predication in source yet
            
            bool isIType = false;
            int baseOpcode = (int)opcode;

            // Check if it's one of our I-types (64-91) or specific I-type a la LI.
            // LIMM (5) is an R-type that reads the next word, so it must NOT be shifted by 64.
            if ((int)opcode >= 64 || (opcode == Opcode.LI || opcode == Opcode.LI_I || opcode == Opcode.INI || opcode == Opcode.OUTI))
            {
                if (opcode == Opcode.LIMM) 
                {
                    isIType = false;
                    baseOpcode = (int)opcode;
                }
                else if (opcode == Opcode.INI || opcode == Opcode.OUTI)
                {
                    isIType = true;
                    baseOpcode = (int)opcode;
                }
                else
                {
                    isIType = true;
                    baseOpcode = (int)opcode < 64 ? (int)opcode + 64 : (int)opcode;
                }
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
                // Imm6: signed 6-trit value (-364..364)
                sRest = BalancedTernary.ToTernaryString(imm, 6);
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