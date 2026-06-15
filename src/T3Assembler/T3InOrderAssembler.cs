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
            int pc = 0;
            foreach (var line in rawLines)
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned)) continue;
                int colonIdx = cleaned.IndexOf(':');
                string instr = colonIdx != -1 ? cleaned.Substring(colonIdx + 1).Trim() : cleaned;
                if (!string.IsNullOrWhiteSpace(instr))
                {
                    var result = AssembleLine(instr, pc);
                    binary.AddRange(result);
                    pc += result.Count;
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

            // Handle predicate: (p0), (p1), (p2), (p3)
            if (line.StartsWith("("))
            {
                int closeParenIdx = line.IndexOf(')');
                if (closeParenIdx != -1)
                {
                    string predPart = line.Substring(1, closeParenIdx - 1).ToLower();
                    if (predPart.StartsWith("p") && int.TryParse(predPart.Substring(1), out int pIdx))
                    {
                        pred = pIdx;
                        processingLine = line.Substring(closeParenIdx + 1).Trim();
                    }
                }
            }

            var instParts = processingLine.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (instParts.Length == 0) throw new Exception("Empty instruction");

            string mnemonic = instParts[0].ToUpper();
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            if (instParts.Length > 1) op1 = ResolveOperand(instParts[1]);

            if (mnemonic == "LI" && instParts.Length >= 3)
            {
                long rawVal = (long)ResolveOperandValue(instParts[2]);
                imm = rawVal;
                if (rawVal > 364 || rawVal < -364)
                {
                    return new List<Int128>
                    {
                        Encode("LIMM", op1, 0, 0, 0),
                        ResolveOperandValue(instParts[2])
                    };
                }
            }
            else if (mnemonic == "LIMM")
            {
                // Handled at the end
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

            // Jump Calculation
            if (IsJumpMnemonic(mnemonic) && instParts.Length >= 2)
            {
                string operand = instParts[1];
                if (IsRegister(operand))
                {
                    op2 = ResolveOperand(operand);
                    imm = 0;
                }
                else if (_labels.ContainsKey(operand))
                {
                    long target = (long)ResolveOperandValue(operand);
                    imm = target - (pc + 1);
                }
                else
                {
                    imm = (long)ResolveOperandValue(operand);
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

            return new List<Int128> { Encode(mnemonic, op1, op2, op3, imm, pred) };
        }

        private bool IsJumpMnemonic(string m) => m is "JMP" or "JE" or "JNE" or "JL" or "JG" or "JM" or "JLE" or "JGE" or "CALL";

        private bool IsRTypeArithmetic(string m) => m is "ADD" or "SUB" or "MUL" or "DIV" or "MOD"
            or "AND" or "TRITAND" or "OR" or "TRITOR" or "XOR" or "TRITXOR" or "SHL" or "SHR";

        private Int128 Encode(string mnemonic, int op1, int op2, int op3, long imm, int pred = 0)
        {
            Opcode opcode = GetOpcode(mnemonic);

            string sPred = BalancedTernary.ToTernaryString(pred, 3);
            string sOp = BalancedTernary.ToTernaryString((int)opcode, 6);
            string sArgs = "";

            if (IsJumpMnemonic(mnemonic))
            {
                if (op2 != 0 && imm == 0)
                {
                    // Register-indirect jump: [Reg(3)] [0(6)]
                    sArgs = BalancedTernary.ToTernaryString(op2, 3) + "000000";
                }
                else
                {
                    // Relative jump: [Imm(9)]
                    sArgs = BalancedTernary.ToTernaryString(imm, 9);
                }
            }
            else if (mnemonic == "FLW" || mnemonic == "FSW")
            {
                // FPU Memory: [RegDest/Src (3)] [RegBase (3)] [Offset (3)]
                sArgs = BalancedTernary.ToTernaryString(op1, 3) + 
                        BalancedTernary.ToTernaryString(op2, 3) + 
                        BalancedTernary.ToTernaryString(imm, 3);
            }
            else if (mnemonic == "ITOF")
            {
                // ITOF: [FDest (3)] [RSrc (3)] [Filler (3)]
                sArgs = BalancedTernary.ToTernaryString(op1, 3) + 
                        BalancedTernary.ToTernaryString(op2, 3) + 
                        "000";
            }
            else if (IsIType(opcode))
            {
                sArgs = BalancedTernary.ToTernaryString(op1, 3) + BalancedTernary.ToTernaryString(imm, 6);
            }
            else if (IsRType(opcode))
            {
                sArgs = BalancedTernary.ToTernaryString(op1, 3) + BalancedTernary.ToTernaryString(op2, 3) + BalancedTernary.ToTernaryString(op3, 3);
            }
            else
            {
                sArgs = BalancedTernary.ToTernaryString(op1, 3) + BalancedTernary.ToTernaryString(op2, 3) + "000";
            }

            return BalancedTernary.ParseToInt128(sPred + sOp + sArgs);
        }

        private bool IsRType(Opcode op) => op switch
        {
        Opcode.ADD or Opcode.SUB or Opcode.MUL or Opcode.DIV or Opcode.MOD or Opcode.NEG or
        Opcode.AND or Opcode.OR or Opcode.XOR or Opcode.SHL or Opcode.SHR or 
        Opcode.MOV or Opcode.CMP or Opcode.LOAD or Opcode.STORE or Opcode.PUSH or Opcode.POP or
        Opcode.IN or Opcode.OUT or
        Opcode.FADD or Opcode.FSUB or Opcode.FMUL or 
        Opcode.FDIV or Opcode.FSQRT or Opcode.FABS or Opcode.FNEG or Opcode.FCMP or 
        Opcode.FTOF or Opcode.FSWAP => true,
            _ => false
        };

        private bool IsIType(Opcode op) => op switch
        {
            Opcode.MOVI or Opcode.LI or Opcode.LIMM or Opcode.ADDI or Opcode.SUBI or 
            Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or Opcode.ANDI or 
            Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or Opcode.LOADI or 
            Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.ITOF or 
            Opcode.FLW or Opcode.FSW or Opcode.FZERO => true,
            _ => false
        };
    }
}