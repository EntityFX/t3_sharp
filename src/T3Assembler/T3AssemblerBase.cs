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
            if (int.TryParse(token, out int val)) return val;
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

            // Ternary literal: t+0-
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

            // Decimal
            if (Int128.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int128 val)) return val;
            
            // Label
            if (_labels.TryGetValue(token, out int addr)) return addr;
            
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
            return upper == "RW" || upper == "RX" || upper == "RY" || upper == "RZ" ||
                   upper == "R0" || upper == "R1" || upper == "R2" || upper == "R3" || upper == "R4" ||
                   upper == "A" || upper == "B" || upper == "C" || upper == "D" || upper == "E" ||
                   upper == "F" || upper == "G" || upper == "H" || upper == "I";
        }

        protected int GetRegisterIndex(string token)
        {
            string upper = token.ToUpper();
            return upper switch
            {
                "RW" => 0,
                "A"  => 0,
                "RX" => 1,
                "B"  => 1,
                "RY" => 2,
                "C"  => 2,
                "RZ" => 3,
                "D"  => 3,
                "R0" => 4,
                "E"  => 4,
                "R1" => 5,
                "F"  => 5,
                "R2" => 6,
                "G"  => 6,
                "R3" => 7,
                "H"  => 7,
                "R4" => 8,
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
                "TRITAND" => Opcode.TRITAND,
                "TRITANDI" => Opcode.TRITANDI,
                "TRITOR" => Opcode.TRITOR,
                "TRITORI" => Opcode.TRITORI,
                "TRITXOR" => Opcode.TRITXOR,
                "TRITXORI" => Opcode.TRITXORI,
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