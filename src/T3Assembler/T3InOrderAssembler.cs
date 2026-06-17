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

            // PASS 1: collect labels
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
                else currentAddress += CountWords(cleaned);
            }

            // PASS 2: assemble
            var binary = new List<Int128>();
            foreach (var line in rawLines)
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;
                int colonIdx = cleaned.IndexOf(':');
                string instr = colonIdx != -1 ? cleaned.Substring(colonIdx + 1).Trim() : cleaned;
                if (!string.IsNullOrWhiteSpace(instr))
                {
                    var result = AssembleLine(instr, binary.Count);
                    binary.AddRange(result);
                }
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
            return 1;
        }

        private List<Int128> AssembleLine(string line, int pc)
        {
            if (line.StartsWith("\"")) return ResolveString(line);
            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new Exception("Invalid .word directive");
                if (parts[1].StartsWith("\"")) return ResolveString(parts[1]);
                return new List<Int128> { ResolveOperandValue(parts[1]) };
            }

            int pred = 0;
            string processingLine = line;
            if (line.StartsWith("("))
            {
                int closeParenIdx = line.IndexOf(')');
                if (closeParenIdx != -1)
                {
                    string predPart = line.Substring(1, closeParenIdx - 1).ToLower();
                    if (predPart.StartsWith("p") && int.TryParse(predPart.Substring(1), out int pIdx))
                    { pred = pIdx; processingLine = line.Substring(closeParenIdx + 1).Trim(); }
                }
            }

            var instParts = processingLine.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction");

            string mnemonic = instParts[0].ToUpper();
            Opcode opcode = GetOpcode(mnemonic);
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            // Parse operands as trit values (-4..+4)
            if (instParts.Length > 1) op1 = IsRegister(instParts[1]) ? GetRegisterTrit(instParts[1]) : 0;
            if (instParts.Length > 2) op2 = IsRegister(instParts[2]) ? GetRegisterTrit(instParts[2]) : 0;
            if (instParts.Length > 3) op3 = IsRegister(instParts[3]) ? GetRegisterTrit(instParts[3]) : 0;

            // Handle immediates
            if (IsIType(opcode) && !IsJumpMnemonic(mnemonic))
            {
                // I-type: op1=reg, imm=literal
                if (instParts.Length > 1) op1 = IsRegister(instParts[1]) ? GetRegisterTrit(instParts[1]) : 0;
                if (instParts.Length > 2) imm = (long)ResolveOperandValue(instParts[2]);
                long encoded = InstructionEncoder.EncodeI(pred, (int)opcode, op1, imm);
                return new List<Int128> { encoded };
            }
            else if (IsJumpMnemonic(mnemonic))
            {
                // J-type jumps use register-indirect (J) or immediate
                string operand = instParts.Length > 1 ? instParts[1] : "0";
                if (IsRegister(operand))
                {
                    int reg = GetRegisterTrit(operand);
                    long enc = InstructionEncoder.EncodeJ(pred, (int)opcode, reg);
                    return new List<Int128> { enc };
                }
                else if (_labels.ContainsKey(operand))
                {
                    long target = (long)ResolveOperandValue(operand);
                    imm = target - (pc + 1); // relative
                    long enc = InstructionEncoder.EncodeI(pred, (int)opcode, 0, imm);
                    return new List<Int128> { enc };
                }
                else
                {
                    imm = (long)ResolveOperandValue(operand);
                    long enc = InstructionEncoder.EncodeI(pred, (int)opcode, 0, imm);
                    return new List<Int128> { enc };
                }
            }
            else if (mnemonic == "LI")
            {
                // LI is I-type with full immediate
                long rawVal = (long)ResolveOperandValue(instParts.Length > 2 ? instParts[2] : "0");
                if (rawVal > 364 || rawVal < -364)
                {
                    // Requires LIMM
                    return new List<Int128>
                    {
                        InstructionEncoder.EncodeR(pred, (int)Opcode.LIMM, op1, 0, 0),
                        ResolveOperandValue(instParts[2])
                    };
                }
                long enc = InstructionEncoder.EncodeI(pred, (int)Opcode.LI, op1, rawVal);
                return new List<Int128> { enc };
            }
            else if (mnemonic == "LIMM")
            {
                return new List<Int128>
                {
                    InstructionEncoder.EncodeR(pred, (int)Opcode.LIMM, op1, 0, 0),
                    ResolveOperandValue(instParts[2])
                };
            }
            else if (mnemonic == "INI" || mnemonic == "OUTI")
            {
                if (instParts.Length > 2) imm = (long)ResolveOperandValue(instParts[2]);
                long enc = InstructionEncoder.EncodeI(pred, (int)opcode, op1, imm);
                return new List<Int128> { enc };
            }
            else
            {
                // R-type
                long enc = InstructionEncoder.EncodeR(pred, (int)opcode, op1, op2, op3);
                return new List<Int128> { enc };
            }
        }

        private bool IsJumpMnemonic(string m) => m is "JMP" or "JE" or "JNE" or "JL" or "JG" or "JM" or "JLE" or "JGE" or "CALL";

        private bool IsIType(Opcode op) => op switch
        {
            Opcode.MOVI or Opcode.LI or Opcode.LIMM or Opcode.ADDI or Opcode.SUBI or
            Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or Opcode.ANDI or
            Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or Opcode.LOADI or
            Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.FZERO => true,
            _ => false
        };
    }
}