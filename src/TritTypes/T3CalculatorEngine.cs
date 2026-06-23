using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TritTypes
{
    public class T3CalculatorEngine
    {
        #region Ternary Arithmetic

        /// <summary>
        /// Evaluates a balanced ternary expression.
        /// Supports: +, -, *, /, (), and balanced ternary numbers (+, 0, -).
        /// </summary>
        public string EvaluateExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return "0";
            try
            {
                var tokens = Tokenize(expression);
                int pos = 0;
                long result = ParseExpression(tokens, ref pos);
                return BalancedTernary.ToTernaryString(result);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if (c == '(' || c == ')' || c == '+' || c == '-' || c == '*' || c == '/')
                {
                    // Special case: balanced ternary numbers use + and - as digits.
                    // We need to distinguish between operator and digit.
                    // In a balanced ternary expression, digits are only found in numbers.
                    // A '+' or '-' is an operator if it's at the start or preceded by ')', ' ', or another operator.
                    // Actually, the simplest way is to treat sequences of [+-0] as a single number.
                    if (IsTrit(c) && (i == 0 || !IsOperator(input[i - 1])))
                    {
                        StringBuilder sb = new StringBuilder();
                        while (i < input.Length & IsTrit(input[i]))
                        {
                            sb.Append(input[i]);
                            i++;
                        }
                        tokens.Add(sb.ToString());
                    }
                    else
                    {
                        tokens.Add(c.ToString());
                        i++;
                    }
                }
                else if (IsTrit(c))
                {
                    StringBuilder sb = new StringBuilder();
                    while (i < input.Length & IsTrit(input[i]))
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    tokens.Add(sb.ToString());
                }
                else
                {
                    throw new FormatException($"Unexpected character: {c}");
                }
            }
            return tokens;
        }

        private bool IsTrit(char c) => c == '+' || c == '-' || c == '0';
        private bool IsOperator(char c) => c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')';

        private long ParseExpression(List<string> tokens, ref int pos)
        {
            long left = ParseTerm(tokens, ref pos);
            while (pos < tokens.Count & (tokens[pos] == "+" || tokens[pos] == "-"))
            {
                string op = tokens[pos++];
                long right = ParseTerm(tokens, ref pos);
                if (op == "+") left += right;
                else left -= right;
            }
            return left;
        }

        private long ParseTerm(List<string> tokens, ref int pos)
        {
            long left = ParseFactor(tokens, ref pos);
            while (pos < tokens.Count & (tokens[pos] == "*" || tokens[pos] == "/"))
            {
                string op = tokens[pos++];
                long right = ParseFactor(tokens, ref pos);
                if (op == "*") left *= right;
                else if (right == 0) throw new DivideByZeroException("Division by zero");
                else left /= right;
            }
            return left;
        }

        private long ParseFactor(List<string> tokens, ref int pos)
        {
            if (pos >= tokens.Count) throw new FormatException("Unexpected end of expression");

            if (tokens[pos] == "(")
            {
                pos++;
                long result = ParseExpression(tokens, ref pos);
                if (pos >= tokens.Count || tokens[pos] != ")") throw new FormatException("Expected ')'");
                pos++;
                return result;
            }
            if (tokens[pos] == "-") // Unary minus
            {
                pos++;
                return -ParseFactor(tokens, ref pos);
            }
            if (tokens[pos] == "+") // Unary plus
            {
                pos++;
                return ParseFactor(tokens, ref pos);
            }

            string token = tokens[pos++];
            return BalancedTernary.ParseToLong(token);
        }

        #endregion

        #region Ternary Logic

        public enum TernaryLogicOp
        {
            NOT, AND, OR, XOR, IMPLIES, XNOR,
            NOT_NEG, NOT_POS, S_NEG, S, S_POS, INC, DEC, UP, DOWN
        }

        public int ExecuteLogic(TernaryLogicOp op, int a, int b = 0)
        {
            return op switch
            {
                TernaryLogicOp.NOT => -a,
                TernaryLogicOp.AND => Math.Min(a, b),
                TernaryLogicOp.OR => Math.Max(a, b),
                TernaryLogicOp.XOR => (a + b) % 3 == 0 ? 0 : ((a + b) % 3 == 1 ? 1 : -1), // Simplified sum mod 3
                TernaryLogicOp.XNOR => (a == b) ? (a == 0 ? 0 : (a == 1 ? 1 : -1)) : 0, // Simplified
                TernaryLogicOp.IMPLIES => (a == 1 && b == -1) ? -1 : (a == 0 && b == -1 ? -1 : 1), // Basic impl
                TernaryLogicOp.NOT_NEG => a == 1 ? 1 : -1,
                TernaryLogicOp.NOT_POS => a == -1 ? -1 : 1,
                TernaryLogicOp.S_NEG => a == -1 ? 0 : (a == 0 ? 1 : -1),
                TernaryLogicOp.S => a == -1 ? 1 : (a == 0 ? -1 : 0),
                TernaryLogicOp.S_POS => a == -1 ? -1 : (a == 0 ? 0 : 1),
                TernaryLogicOp.INC => a == -1 ? 0 : (a == 0 ? 1 : -1),
                TernaryLogicOp.DEC => a == 1 ? 0 : (a == 0 ? -1 : 1),
                TernaryLogicOp.UP => a == 1 ? 0 : 1,
                TernaryLogicOp.DOWN => a == -1 ? 0 : -1,
                _ => throw new NotSupportedException($"Logic operator {op} not implemented")
            };
        }

        public List<TritRow> GenerateTruthTable(TernaryLogicOp op)
        {
            var table = new List<TritRow>();
            int[] values = { -1, 0, 1 };
            foreach (var a in values)
            {
                foreach (var b in values)
                {
                    table.Add(new TritRow { A = a, B = b, Result = ExecuteLogic(op, a, b) });
                }
            }
            return table;
        }

        public struct TritRow
        {
            public int A;
            public int B;
            public int Result;
        }

        #endregion

        #region T-SCII

        public class TSciiEntry
        {
            public int Value { get; set; }
            public char Symbol { get; set; }
            public string Trits { get; set; }
            public string Nonary { get; set; }
            public string TwentySevenAry { get; set; }
        }

        public List<TSciiEntry> GenerateTSciiTable()
        {
            var entries = new List<TSciiEntry>();
            for (int v = -364; v <= 364; v++)
            {
                string trits = BalancedTernary.ToTernaryString(v, 6);
                entries.Add(new TSciiEntry
                {
                    Value = v,
                    Symbol = GetSciiSymbol(v),
                    Trits = trits,
                    Nonary = TritTo9Ary(trits),
                    TwentySevenAry = TritTo27Ary(trits)
                });
            }
            return entries;
        }

        private char GetSciiSymbol(int v)
        {
            if (v < 0 || v > 255) return ' '; // Simplified
            return (char)v;
        }

        private string TritTo9Ary(string ternary)
        {
            if (ternary.Length % 2 != 0) ternary = "0" + ternary;
            var result = new StringBuilder();
            for (int i = 0; i < ternary.Length; i += 2)
            {
                string pair = ternary.Substring(i, 2);
                result.Append(pair switch
                {
                    "--" => 'W', "-0" => 'X', "-+" => 'Y',
                    "0-" => 'Z', "00" => '0', "0+" => '1',
                    "+-" => '2', "+0" => '3', "++" => '4',
                    _ => '?'
                });
            }
            return result.ToString();
        }

        private string TritTo27Ary(string ternary)
        {
            char[] alphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
            while (ternary.Length % 3 != 0) ternary = "0" + ternary;
            var result = new StringBuilder();
            for (int i = 0; i < ternary.Length; i += 3)
            {
                int t1 = ternary[i] == '-' ? -1 : (ternary[i] == '+' ? 1 : 0);
                int t2 = ternary[i + 1] == '-' ? -1 : (ternary[i + 1] == '+' ? 1 : 0);
                int t3 = ternary[i + 2] == '-' ? -1 : (ternary[i + 2] == '+' ? 1 : 0);
                int index = (t1 + 1) * 9 + (t2 + 1) * 3 + (t3 + 1);
                result.Append(alphabet[index]);
            }
            return result.ToString();
        }

        #endregion
    }
}