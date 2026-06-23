using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;

namespace TritTypes
{
    /// <summary>
    /// Engine for parsing and evaluating balanced ternary arithmetic expressions.
    /// </summary>
    public class T3ArithmeticEngine
    {
        public enum TokenType
        {
            Number,
            Plus,
            Minus,
            Multiply,
            Divide,
            LParen,
            RParen,
            EOF
        }

        public struct Token
        {
            public TokenType Type;
            public string Value;
            public Token(TokenType type, string value = "") { Type = type; Value = value; }
        }

        private List<Token> _tokens;
        private int _pos;

        public string Evaluate(string expression)
        {
            try
            {
                _tokens = Tokenize(expression);
                _pos = 0;
                long result = ParseExpression();
                if (_pos < _tokens.Count && _tokens[_pos].Type != TokenType.EOF)
                    throw new Exception($"Unexpected token: {_tokens[_pos].Value}");
                
                return BalancedTernary.ToTernaryString(result);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public string GetColumnarSolution(string op, string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return "Insufficient data";
            
            return op switch
            {
                "+" => GenerateAdditionColumn(a, b),
                "-" => GenerateAdditionColumn(a, NegateTernary(b)),
                "*" => GenerateMultiplicationColumn(a, b),
                _ => "Columnar solution not available for this operator"
            };
        }

        private string NegateTernary(string t)
        {
            var sb = new StringBuilder();
            foreach (char c in t)
            {
                if (c == '+') sb.Append('-');
                else if (c == '-') sb.Append('+');
                else sb.Append('0');
            }
            return sb.ToString();
        }

        private string GenerateAdditionColumn(string a, string b)
        {
            int maxLen = Math.Max(a.Length, b.Length);
            string s1 = a.PadLeft(maxLen, '0');
            string s2 = b.PadLeft(maxLen, '0');

            StringBuilder result = new StringBuilder();
            
            int carry = 0;
            List<char> resChars = new List<char>();
            List<char> carries = new List<char>();

            for (int i = maxLen - 1; i >= 0; i--)
            {
                int v1 = TritToInt(s1[i]);
                int v2 = TritToInt(s2[i]);
                int sum = v1 + v2 + carry;

                int r = sum;
                int c = 0;
                if (r > 1) { r -= 3; c = 1; }
                else if (r < -1) { r += 3; c = -1; }
                
                resChars.Add(IntToTrit(r));
                carries.Add(IntToTrit(c));
                carry = c;
            }
            
            if (carry != 0)
            {
                resChars.Add(IntToTrit(carry));
                carries.Insert(0, ' ');
            }

            string finalRes = new string(resChars.ToArray()).Reverse();
            
            result.AppendLine($"  {s1}");
            result.AppendLine($"+ {s2}");
            result.AppendLine(new string('-', maxLen + 1));
            result.AppendLine($"C: {new string(carries.ToArray()).Reverse()}");
            result.AppendLine($"R: {finalRes}");

            return result.ToString();
        }

        private string GenerateMultiplicationColumn(string a, string b)
        {
            return "Multiplication detail: " + a + " * " + b + " = " + BalancedTernary.ToTernaryString(
                BalancedTernary.ParseToLong(a) * BalancedTernary.ParseToLong(b));
        }

        private int TritToInt(char c) => c == '+' ? 1 : (c == '-' ? -1 : 0);
        private char IntToTrit(int v) => v == 1 ? '+' : (v == -1 ? '-' : '0');

        private List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }

                if (c == '(') { tokens.Add(new Token(TokenType.LParen, "(")); i++; }
                else if (c == ')') { tokens.Add(new Token(TokenType.RParen, ")")); i++; }
                else if (c == '*') { tokens.Add(new Token(TokenType.Multiply, "*")); i++; }
                else if (c == '/') { tokens.Add(new Token(TokenType.Divide, "/")); i++; }
                else if (char.IsDigit(c))
                {
                    StringBuilder sb = new StringBuilder();
                    while (i < input.Length && (char.IsDigit(input[i]))) { sb.Append(input[i]); i++; }
                    tokens.Add(new Token(TokenType.Number, sb.ToString()));
                }
                else if (c == '+' || c == '-')
                {
                    bool isUnary = false;
                    if (tokens.Count == 0) isUnary = true;
                    else
                    {
                        var last = tokens[tokens.Count - 1];
                        if (last.Type == TokenType.Plus || last.Type == TokenType.Minus || 
                            last.Type == TokenType.Multiply || last.Type == TokenType.Divide || 
                            last.Type == TokenType.LParen)
                            isUnary = true;
                    }

                    if (isUnary)
                    {
                        if (i + 1 < input.Length && (char.IsDigit(input[i + 1])))
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append(c); i++;
                            while (i < input.Length && char.IsDigit(input[i])) { sb.Append(input[i]); i++; }
                            tokens.Add(new Token(TokenType.Number, sb.ToString()));
                        }
                        else if (i + 1 < input.Length && (input[i+1] == '+' || input[i+1] == '-' || input[i+1] == '0'))
                        {
                            StringBuilder sb = new StringBuilder();
                            while (i < input.Length && (input[i] == '+' || input[i] == '-' || input[i] == '0')) { sb.Append(input[i]); i++; }
                            tokens.Add(new Token(TokenType.Number, sb.ToString()));
                        }
                        else
                        {
                            tokens.Add(new Token(c == '+' ? TokenType.Plus : TokenType.Minus, c.ToString()));
                            i++;
                        }
                    }
                    else
                    {
                        tokens.Add(new Token(c == '+' ? TokenType.Plus : TokenType.Minus, c.ToString()));
                        i++;
                    }
                }
                else throw new Exception($"Invalid character in expression: {c}");
            }
            tokens.Add(new Token(TokenType.EOF));
            return tokens;
        }

        private long ParseExpression()
        {
            long left = ParseTerm();
            while (_pos < _tokens.Count && (_tokens[_pos].Type == TokenType.Plus || _tokens[_pos].Type == TokenType.Minus))
            {
                TokenType op = _tokens[_pos].Type;
                _pos++;
                long right = ParseTerm();
                if (op == TokenType.Plus) left += right;
                else left -= right;
            }
            return left;
        }

        private long ParseTerm()
        {
            long left = ParseFactor();
            while (_pos < _tokens.Count && (_tokens[_pos].Type == TokenType.Multiply || _tokens[_pos].Type == TokenType.Divide))
            {
                TokenType op = _tokens[_pos].Type;
                _pos++;
                long right = ParseFactor();
                if (op == TokenType.Multiply) left *= right;
                else
                {
                    if (right == 0) throw new DivideByZeroException("Division by zero");
                    left /= right;
                }
            }
            return left;
        }

        private long ParseFactor()
        {
            if (_pos >= _tokens.Count) throw new Exception("Unexpected end of expression");
            Token token = _tokens[_pos];
            if (token.Type == TokenType.LParen)
            {
                _pos++;
                long result = ParseExpression();
                if (_pos >= _tokens.Count || _tokens[_pos].Type != TokenType.RParen)
                    throw new Exception("Missing closing parenthesis");
                _pos++;
                return result;
            }
            else if (token.Type == TokenType.Number)
            {
                _pos++;
                return BalancedTernary.ParseToLong(token.Value);
            }
            throw new Exception($"Expected number or '(', but found {token.Value}");
        }
    }

    public static class StringExtensions
    {
        public static string Reverse(this IEnumerable<char> sequence)
        {
            var list = sequence.ToList();
            list.Reverse();
            return new string(list.ToArray());
        }
    }
}