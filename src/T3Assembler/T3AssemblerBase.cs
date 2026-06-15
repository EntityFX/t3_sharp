using System;
using System.Collections.Generic;
using System.Globalization;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    /// <summary>
    /// Base class for T3 assemblers providing common utility methods for 
    /// line cleaning, operand resolution, and opcode mapping.
    /// </summary>
    public abstract class T3AssemblerBase
    {
        protected readonly T3Config _config;
        protected readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        protected readonly List<string> _lines = new List<string>();

        public T3AssemblerBase(T3Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Assembles source code into a list of words.
        /// Returns Int128 to accommodate both T3-27 and T3-54 words.
        /// </summary>
        public abstract List<Int128> Assemble(string sourceCode);

        protected string CleanLine(string line)
        {
            int commentIdx = line.IndexOf(';');
            if (commentIdx != -1) line = line.Substring(0, commentIdx);
            return line.Trim();
        }

        protected int ResolveOperand(string token)
        {
            if (IsRegister(token))
            {
                return GetRegisterIndex(token);
            }
            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long lval))
                return (int)lval;
            if (_labels.TryGetValue(token, out int addr)) return addr;
            
            throw new Exception($"Unable to resolve operand: {token}");
        }

        protected List<Int128> ResolveString(string token)
        {
            if (!token.StartsWith("\"") || !token.EndsWith("\""))
                throw new Exception($"Invalid string literal: {token}. Strings must be enclosed in double quotes.");

            string content = token.Substring(1, token.Length - 2);
            List<Int128> result = new List<Int128>();
            foreach (char c in content)
            {
                result.Add(TritTypes.TScii.FromChar(c));
            }
            // Null terminator
            result.Add(0);
            return result;
        }

        protected Int128 ResolveOperandValue(string token)
        {
            if (IsRegister(token)) return GetRegisterIndex(token);

            // Decimal integer (most common case)
            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long lval)) return lval;

            // Label reference (check before literal prefixes to avoid conflicts with labels starting with 't', '0n', '0y')
            if (_labels.TryGetValue(token, out int addr)) return addr;

            // Ternary literal: t+0-  (e.g., t+--, t-0+)
            if (token.StartsWith("t", StringComparison.OrdinalIgnoreCase))
            {
                return BalancedTernary.ParseToInt128(token.Substring(1));
            }

            // 9-ary literal: 0n...
            if (token.StartsWith("0n", StringComparison.OrdinalIgnoreCase))
            {
                return Parse9Ary(token.Substring(2));
            }

            // 27-ary literal: 0y...
            if (token.StartsWith("0y", StringComparison.OrdinalIgnoreCase))
            {
                return Parse27Ary(token.Substring(2));
            }

            throw new Exception($"Unable to resolve operand value: {token}");
        }

        private Int128 Parse9Ary(string token)
        {
            string resultTritString = "";
            foreach (char c in token.ToUpper())
            {
                resultTritString += c switch
                {
                    'W' => "--",
                    'X' => "-0",
                    'Y' => "-+",
                    'Z' => "0-",
                    '0' => "00",
                    '1' => "0+",
                    '2' => "+-",
                    '3' => "+0",
                    '4' => "++",
                    _ => throw new Exception($"Invalid 9-ary character: {c}")
                };
            }
            return BalancedTernary.ParseToInt128(resultTritString);
        }

        private Int128 Parse27Ary(string token)
        {
            string resultTritString = "";
            foreach (char c in token.ToUpper())
            {
                resultTritString += c switch
                {
                    'N' => "---",
                    'O' => "--0",
                    'P' => "--+",
                    'Q' => "-0-",
                    'R' => "-00",
                    'S' => "-0+",
                    'T' => "-+-",
                    'U' => "-+0",
                    'V' => "-++",
                    '5' => "+--",
                    '6' => "+-0",
                    '7' => "+-+",
                    '8' => "+0-",
                    '9' => "+00",
                    'A' => "+0+",
                    'B' => "++-",
                    'C' => "++0",
                    'D' => "+++",
                    _ => throw new Exception($"Invalid 27-ary character: {c}")
                };
            }
            return BalancedTernary.ParseToInt128(resultTritString);
        }

        protected bool IsRegister(string token)
        {
            string upper = token.ToUpper();
            if (upper == "RW" || upper == "RX" || upper == "RY" || upper == "RZ") return true;
            if (upper == "FW" || upper == "FX" || upper == "FY" || upper == "FZ") return true;
            if (upper.StartsWith("R") && upper.Length > 1 && int.TryParse(upper.Substring(1), out int idx) && idx >= 0 && idx <= 4) return true;
            if (upper.StartsWith("F") && upper.Length > 1 && int.TryParse(upper.Substring(1), out int fidx) && fidx >= 0 && fidx <= 4) return true;
            return new HashSet<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I" }.Contains(upper);
        }

        protected int GetRegisterIndex(string token)
        {
            string upper = token.ToUpper();
            // Named registers map to physical indices 0-3
            if (upper == "RW" || upper == "FW") return 0;
            if (upper == "RX" || upper == "FX") return 1;
            if (upper == "RY" || upper == "FY") return 2;
            if (upper == "RZ" || upper == "FZ") return 3;
            
            // R0-R4 map to physical indices 4-8 (after RW/RX/RY/RZ at 0-3)
            if (upper.StartsWith("R") && upper.Length > 1 && int.TryParse(upper.Substring(1), out int idx)) 
            {
                if (idx >= 0 && idx <= 4) return idx + 4;
                throw new Exception($"Register R{idx} is out of range (0-4).");
            }
            // F0-F4 map to FPU indices 4-8 (after FW/FX/FY/FZ at 0-3)
            if (upper.StartsWith("F") && upper.Length > 1 && int.TryParse(upper.Substring(1), out int fidx)) 
            {
                if (fidx >= 0 && fidx <= 4) return fidx + 4;
                throw new Exception($"FPU register F{fidx} is out of range (0-4).");
            }
            // Legacy named registers
            return upper switch
            {
                "A"  => 0,
                "B"  => 1,
                "C"  => 2,
                "D"  => 3,
                "E"  => 4,
                "F"  => 5,
                "G"  => 6,
                "H"  => 7,
                "I"  => 8,
                _ => throw new Exception($"Invalid register name: {token}")
            };
        }

        protected Opcode GetOpcode(string mnemonic)
        {
            return mnemonic switch
            {
                "HALT" => Opcode.HALT,
                "LOAD" => Opcode.LOAD,
                "LOADI" => Opcode.LOADI,
                "STORE" => Opcode.STORE,
                "STOREI" => Opcode.STOREI,
                "MOV" => Opcode.MOV,
                "MOVI" => Opcode.MOVI,
                "LI" => Opcode.LI,
                "LIMM" => Opcode.LIMM,
                "ADD" => Opcode.ADD,
                "ADDI" => Opcode.ADDI,
                "SUB" => Opcode.SUB,
                "SUBI" => Opcode.SUBI,
                "MUL" => Opcode.MUL,
                "MULI" => Opcode.MULI,
                "DIV" => Opcode.DIV,
                "DIVI" => Opcode.DIVI,
                "MOD" => Opcode.MOD,
                "MODI" => Opcode.MODI,
                "NEG" => Opcode.NEG,
                "NEGI" => Opcode.NEGI,
                "AND" => Opcode.AND,
                "TRITAND" => Opcode.AND,
                "ANDI" => Opcode.ANDI,
                "TRITANDI" => Opcode.ANDI,
                "OR" => Opcode.OR,
                "TRITOR" => Opcode.OR,
                "ORI" => Opcode.ORI,
                "TRITORI" => Opcode.ORI,
                "XOR" => Opcode.XOR,
                "TRITXOR" => Opcode.XOR,
                "XORI" => Opcode.XORI,
                "TRITXORI" => Opcode.XORI,
                "SHL" => Opcode.SHL,
                "SHLI" => Opcode.SHLI,
                "SHR" => Opcode.SHR,
                "SHRI" => Opcode.SHRI,
                "CMP" => Opcode.CMP,
                "CMPI" => Opcode.CMPI,
                "JMP" => Opcode.JMP,
                "JE" => Opcode.JE,
                "JNE" => Opcode.JNE,
                "JL" => Opcode.JL,
                "JG" => Opcode.JG,
                "JLE" => Opcode.JLE,
                "JGE" => Opcode.JGE,
                "JM" => Opcode.JM,
                "CALL" => Opcode.CALL,
                "RET" => Opcode.RET,
                "PUSH" => Opcode.PUSH,
                "POP" => Opcode.POP,
                "IN" => Opcode.IN,
                "OUT" => Opcode.OUT,
                "INI" => Opcode.INI,
                "OUTI" => Opcode.OUTI,
                "FADD" => Opcode.FADD,
                "FSUB" => Opcode.FSUB,
                "FMUL" => Opcode.FMUL,
                "FDIV" => Opcode.FDIV,
                "FSQRT" => Opcode.FSQRT,
                "FABS" => Opcode.FABS,
                "FNEG" => Opcode.FNEG,
                "FCMP" => Opcode.FCMP,
                "FTOI" => Opcode.FTOI,
                "ITOF" => Opcode.ITOF,
                "FTOF" => Opcode.FTOF,
                "FLW" => Opcode.FLW,
                "FSW" => Opcode.FSW,
                "FMOV" => Opcode.FMOV,
                "FCLASS" => Opcode.FCLASS,
                "FSWAP" => Opcode.FSWAP,
                "NOP" => Opcode.NOP,
                "FZERO" => Opcode.FZERO,
                _ => throw new Exception($"Unknown mnemonic: {mnemonic}")
            };
        }
    }
}