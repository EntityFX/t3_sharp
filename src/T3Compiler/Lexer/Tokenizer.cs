using System;
using System.Collections.Generic;
using System.Text;

namespace T3Compiler.Lexer
{
    public class Tokenizer
    {
        private readonly string _source;
        private int _pos;
        private int _line;
        private int _col;
        private readonly List<Token> _tokens;

        public (int Line, int Column) CurrentPosition => (_line, _col);

        public Tokenizer(string source)
        {
            _source = source;
            _pos = 0;
            _line = 1;
            _col = 1;
            _tokens = new List<Token>();
        }

        public List<Token> Tokenize()
        {
            _tokens.Clear();
            while (_pos < _source.Length)
            {
                char c = Peek();
                if (c == '\n') { _line++; _col = 1; _pos++; continue; }
                if (c == '\r') { _pos++; continue; }
                if (char.IsWhiteSpace(c)) { _pos++; _col++; continue; }

                // Comments: // and /* */
                if (c == '/' && Peek(1) == '/') { SkipLine(); continue; }
                if (c == '/' && Peek(1) == '*') { SkipBlockComment(); continue; }

                // Preprocessor (only at start of line, ignoring whitespace before)
                if (c == '#' && IsStartOfLine())
                {
                    _tokens.Add(new Token(TokenType.Hash, "#", _line, _col));
                    _pos++; _col++;
                    continue;
                }

                // String literal
                if (c == '"') { TokenizeString(); continue; }
                // Char literal
                if (c == '\'') { TokenizeChar(); continue; }

                // Identifiers and keywords
                if (IsLetter(c) || c == '_') { TokenizeIdentifier(); continue; }

                // Numbers and prefixes (0t, 0y, 0n, decimal)
                if (char.IsDigit(c))
                {
                    if (c == '0' && _pos + 1 < _source.Length)
                    {
                        char c1 = _source[_pos + 1];
                        if (c1 == 't') { TokenizeBalancedLiteral(); continue; }
                        if (c1 == 'y') { TokenizeTryxLiteral(); continue; }
                        if (c1 == 'n') { TokenizeNineLiteral(); continue; }
                    }
                    TokenizeNumber(); continue;
                }

                // Operators
                if (TokenizeOperator()) continue;

                // Delimiters
                if (TokenizeDelimiter()) continue;

                // Unknown character
                throw new Exception($"Unexpected character '{c}' at line {_line}, col {_col}");
            }
            _tokens.Add(new Token(TokenType.EndOfFile, "", _line, _col));
            return _tokens;
        }

        // === Helpers ===
        private char Peek(int offset = 0) =>
            (_pos + offset < _source.Length) ? _source[_pos + offset] : '\0';

        private bool IsStartOfLine()
        {
            int p = _pos - 1;
            while (p >= 0 && _source[p] != '\n')
            {
                if (!char.IsWhiteSpace(_source[p])) return false;
                p--;
            }
            return true;
        }

        private static bool IsLetter(char c) =>
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
            (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё';

        private void SkipLine() { while (_pos < _source.Length && _source[_pos] != '\n') _pos++; }
        private void SkipBlockComment()
        {
            _pos += 2; _col += 2;
            while (_pos < _source.Length)
            {
                if (Peek() == '*' && Peek(1) == '/') { _pos += 2; _col += 2; return; }
                if (Peek() == '\n') { _line++; _col = 1; } else _col++;
                _pos++;
            }
        }

        private string ReadWhile(Func<char, bool> pred)
        {
            int start = _pos;
            while (_pos < _source.Length && pred(Peek())) { _pos++; _col++; }
            return _source.Substring(start, _pos - start);
        }

        // === Tokenizers ===
        private void TokenizeString()
        {
            _pos++; _col++; // skip opening "
            var sb = new StringBuilder();
            while (_pos < _source.Length && Peek() != '"')
            {
                if (Peek() == '\\') { _pos++; _col++; sb.Append(Peek()); }
                else sb.Append(Peek());
                _pos++; _col++;
            }
            if (_pos < _source.Length) { _pos++; _col++; } // skip closing "
            _tokens.Add(new Token(TokenType.StringLiteral, sb.ToString(), _line, _col));
        }

        private void TokenizeChar()
        {
            _pos++; _col++; // skip '
            string val = "";
            if (Peek() == '\\') { _pos++; _col++; val = "\\" + Peek(); }
            else val = Peek().ToString();
            _pos++; _col++;
            if (Peek() == '\'') { _pos++; _col++; }
            _tokens.Add(new Token(TokenType.CharLiteral, val, _line, _col));
        }

        private void TokenizeIdentifier()
        {
            string id = ReadWhile(c => IsLetter(c) || char.IsDigit(c) || c == '_');
            var kw = Keywords.TryGetKeyword(id);
            _tokens.Add(new Token(kw ?? TokenType.Identifier, id, _line, _col));
        }

        private void TokenizeNumber()
        {
            string num = ReadWhile(char.IsDigit);
            // Check suffix: y, s, t, tl, tll
            string suffix = "";
            if (_pos < _source.Length)
            {
                if (Peek() == 't')
                {
                    if (Peek(1) == 'l')
                    {
                        if (Peek(2) == 'l') { suffix = "tll"; _pos += 3; _col += 3; }
                        else { suffix = "tl"; _pos += 2; _col += 2; }
                    }
                    else { suffix = "t"; _pos++; _col++; }
                }
                else if (Peek() == 's') { suffix = "s"; _pos++; _col++; }
                else if (Peek() == 'y') { suffix = "y"; _pos++; _col++; }
            }
            // Float?
            if (Peek() == '.' && char.IsDigit(Peek(1)))
            {
                _pos++; _col++; num += "." + ReadWhile(char.IsDigit);
                if (Peek() == 'e' || Peek() == 'E') { num += ReadExponent(); }
                string fSuffix = ReadFloatSuffix();
                _tokens.Add(new Token(TokenType.FloatLiteral, num + fSuffix, _line, _col));
            }
            else if (suffix.Length > 0)
            {
                _tokens.Add(new Token(TokenType.IntegerLiteral, num + suffix, _line, _col));
            }
            else
            {
                _tokens.Add(new Token(TokenType.IntegerLiteral, num, _line, _col));
            }
        }

        private string ReadExponent()
        {
            string e = "";
            e += Peek(); _pos++; _col++; // e/E
            if (Peek() == '+' || Peek() == '-') { e += Peek(); _pos++; _col++; }
            e += ReadWhile(char.IsDigit);
            return e;
        }

        private string ReadFloatSuffix()
        {
            if (Peek() == 'f' || Peek() == 'd')
            {
                string s = Peek().ToString();
                _pos++; _col++;
                return s;
            }
            return "";
        }

        private void TokenizeBalancedLiteral()
        {
            _pos += 2; _col += 2; // skip 0t
            string val = ReadWhile(c => c == '-' || c == '0' || c == '+' || c == '_');
            _tokens.Add(new Token(TokenType.IntegerLiteral, "0t" + val, _line, _col));
        }

        private void TokenizeTryxLiteral()
        {
            _pos += 2; _col += 2; // skip 0y
            string val = ReadWhile(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || char.IsDigit(c));
            _tokens.Add(new Token(TokenType.IntegerLiteral, "0y" + val, _line, _col));
        }

        private void TokenizeNineLiteral()
        {
            _pos += 2; _col += 2; // skip 0n
            string val = ReadWhile(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || char.IsDigit(c));
            _tokens.Add(new Token(TokenType.IntegerLiteral, "0n" + val, _line, _col));
        }

        private bool TokenizeOperator()
        {
            char c = Peek();
            char c1 = Peek(1);
            char c2 = Peek(2);

            // 3-char operators (must check first to avoid greedy 2-char match)
            if (c == '<' && c1 == '<' && c2 == '=') { AddOp(TokenType.OpLShiftEq, 3); return true; }
            if (c == '>' && c1 == '>' && c2 == '=') { AddOp(TokenType.OpRShiftEq, 3); return true; }

            // 2-char operators
            var pairs = new (string pair, TokenType type)[]
            {
                ("??", TokenType.OpTernaryQuestion), (":?", TokenType.OpTernaryMaybe), (":!", TokenType.OpTernaryFalse),
                ("+=", TokenType.OpPlusEq), ("-=", TokenType.OpMinusEq),
                ("*=", TokenType.OpStarEq), ("/=", TokenType.OpSlashEq),
                ("%=", TokenType.OpPercentEq), ("&=", TokenType.OpAmpEq),
                ("|=", TokenType.OpPipeEq), ("^=", TokenType.OpCaretEq),
                ("<<", TokenType.OpLShift), (">>", TokenType.OpRShift),
                ("<=", TokenType.OpLtEq), (">=", TokenType.OpGtEq),
                ("==", TokenType.OpEqEq), ("!=", TokenType.OpNeq),
                ("++", TokenType.OpPlusPlus), ("--", TokenType.OpMinusMinus),
                ("&&", TokenType.OpAndAnd), ("||", TokenType.OpPipePipe),
                ("->", TokenType.OpArrow),
            };
            foreach (var (pair, type) in pairs)
            {
                if (c == pair[0] && c1 == pair[1])
                {
                    AddOp(type, 2);
                    return true;
                }
            }

            // 1-char operators
            var singles = new Dictionary<char, TokenType>
            {
                {'+', TokenType.OpPlus}, {'-', TokenType.OpMinus},
                {'*', TokenType.OpStar}, {'/', TokenType.OpSlash},
                {'%', TokenType.OpPercent}, {'&', TokenType.OpAmpersand},
                {'|', TokenType.OpPipe}, {'^', TokenType.OpCaret},
                {'~', TokenType.OpTilde}, {'!', TokenType.OpExclamation},
                {'?', TokenType.OpQuestion}, {':', TokenType.OpColon},
                {'=', TokenType.OpEq}, {'<', TokenType.OpLt},
                {'>', TokenType.OpGt}, {'.', TokenType.OpDot},
            };
            if (singles.TryGetValue(c, out var st))
            {
                AddOp(st, 1);
                return true;
            }
            return false;
        }

        private void AddOp(TokenType type, int len)
        {
            int c = _col;
            string val = _source.Substring(_pos, len);
            _pos += len; _col += len;
            _tokens.Add(new Token(type, val, _line, c));
        }

        private bool TokenizeDelimiter()
        {
            char c = Peek();
            var map = new Dictionary<char, TokenType>
            {
                {'(', TokenType.LParen}, {')', TokenType.RParen},
                {'{', TokenType.LBrace}, {'}', TokenType.RBrace},
                {'[', TokenType.LBracket}, {']', TokenType.RBracket},
                {';', TokenType.Semicolon}, {',', TokenType.Comma},
            };
            if (map.TryGetValue(c, out var t))
            {
                _tokens.Add(new Token(t, c.ToString(), _line, _col));
                _pos++; _col++;
                return true;
            }
            return false;
        }
    }
}