namespace T3Compiler.Lexer
{
    public struct Token
    {
        public TokenType Type;
        public string Value;       // Lexeme text
        public int Line;
        public int Column;

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type; Value = value; Line = line; Column = column;
        }

        public override string ToString() => $"{Type}({Value}) at {Line}:{Column}";
    }
}