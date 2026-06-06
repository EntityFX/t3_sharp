using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    public class T3AssemblerCore
    {
        private readonly T3Config _config;
        private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        private readonly List<string> _lines = new List<string>();

        public T3AssemblerCore(T3Config config)
        {
            _config = config;
        }

        public List<long> Assemble(string sourceCode)
        {
            _labels.Clear();
            _lines.Clear();

            // First pass: remove comments, empty lines, and identify labels
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

            // Second pass: emit instructions
            List<long> binary = new List<long>();
            foreach (var line in _lines)
            {
                binary.Add(AssembleLine(line));
            }

            return binary;
        }

        private string CleanLine(string line)
        {
            // Remove comments starting with ;
            int commentIdx = line.IndexOf(';');
            if (commentIdx != -1) line = line.Substring(0, commentIdx);
            return line.Trim();
        }

        private long AssembleLine(string line)
        {
            // Support for data definition: .word value
            if (line.StartsWith(".word"))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) throw new Exception("Invalid .word directive. Expected .word <value>");
                return ResolveOperandValue(parts[1]);
            }

            // Support for instructions
            var instParts = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction.");

            string mnemonic = instParts[0].ToUpper();
            int op1 = 0;
            long op2 = 0;

            if (instParts.Length > 1) op1 = ResolveOperand(instParts[1]);
            if (instParts.Length > 2) op2 = ResolveOperandValue(instParts[2]);

            return Encode(mnemonic, op1, op2);
        }

        private int ResolveOperand(string token)
        {
            if (IsRegister(token))
            {
                return GetRegisterIndex(token);
            }
            if (int.TryParse(token, out int val)) return val;
            if (_labels.TryGetValue(token, out int addr)) return addr;
            
            throw new Exception($"Unable to resolve operand: {token}");
        }

        private long ResolveOperandValue(string token)
        {
            if (IsRegister(token)) return GetRegisterIndex(token);
            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long val)) return val;
            if (_labels.TryGetValue(token, out int addr)) return addr;
            
            throw new Exception($"Unable to resolve operand value: {token}");
        }

        private bool IsRegister(string token)
        {
            return token.Length == 1 && "ABCDEFGHI".Contains(token.ToUpper());
        }

        private int GetRegisterIndex(string token)
        {
            return "ABCDEFGHI".IndexOf(token.ToUpper());
        }

        private long Encode(string mnemonic, int op1, long op2)
        {
            Opcode opcode = GetOpcode(mnemonic);
            
            // Basic 27-trit encoding logic (as per the requested specification)
            // Opcode (6), Op1 (9), Op2 (9) -> Total 24. Pad with 3 zeros.
            string sOp = BalancedTernary.ToTernaryString( (int)opcode, 6);
            string sOp1 = BalancedTernary.ToTernaryString(op1, 9);
            string sOp2 = BalancedTernary.ToTernaryString(op2, 9);
            
            return BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + "000");
        }

        private Opcode GetOpcode(string mnemonic)
        {
            return mnemonic switch
            {
                "HALT" => Opcode.HALT,
                "LOAD" => Opcode.LOAD,
                "STORE" => Opcode.STORE,
                "MOV" => Opcode.MOV,
                "LI" => Opcode.LI,
                "LIMM" => Opcode.LIMM,
                "ADD" => Opcode.ADD,
                "SUB" => Opcode.SUB,
                "MUL" => Opcode.MUL,
                "DIV" => Opcode.DIV,
                "MOD" => Opcode.MOD,
                "NEG" => Opcode.NEG,
                "TRITAND" => Opcode.TRITAND,
                "TRITOR" => Opcode.TRITOR,
                "TRITXOR" => Opcode.TRITXOR,
                "SHL" => Opcode.SHL,
                "SHR" => Opcode.SHR,
                "CMP" => Opcode.CMP,
                "JMP" => Opcode.JMP,
                "JE" => Opcode.JE,
                "JNE" => Opcode.JNE,
                "JL" => Opcode.JL,
                "JG" => Opcode.JG,
                "JM" => Opcode.JM,
                "CALL" => Opcode.CALL,
                "RET" => Opcode.RET,
                "PUSH" => Opcode.PUSH,
                "POP" => Opcode.POP,
                "IN" => Opcode.IN,
                "OUT" => Opcode.OUT,
                "INI" => Opcode.INI,
                "OUTI" => Opcode.OUTI,
                _ => throw new Exception($"Unknown mnemonic: {mnemonic}")
            };
        }
    }
}