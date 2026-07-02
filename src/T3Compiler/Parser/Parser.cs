using System;
using System.Collections.Generic;
using T3Compiler.Lexer;

namespace T3Compiler.Parser
{
    /// <summary>
    /// Recursive-descent parser for T-lang: struct, union, multi-dim arrays, functions, trit logic.
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private readonly HashSet<string> _typeNames = new HashSet<string>();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        private Token Peek() => _tokens[_pos];

        private Token Peek(int o) =>
            (_pos + o < _tokens.Count)
                ? _tokens[_pos + o]
                : new Token(TokenType.EndOfFile, "", 0, 0);

        private Token Next()
        {
            var t = _tokens[_pos];
            if (_pos < _tokens.Count - 1)
                _pos++;
            return t;
        }

        private bool Match(TokenType t)
        {
            if (Peek().Type == t)
            {
                Next();
                return true;
            }
            return false;
        }

        private Token Expect(TokenType t)
        {
            if (Peek().Type == t)
                return Next();
            var tk = Peek();
            throw new Exception(
                $"{tk.Line}:{tk.Column}: error: expected {t} but got {tk.Type}({tk.Value})"
            );
        }

        /// <summary>Create an AST node with source position from the current token.</summary>
        T Node<T>()
            where T : AstNode, new()
        {
            var t = Peek();
            return new T { Line = t.Line, Column = t.Column };
        }

        public AstProgram ParseProgram()
        {
            var p = new AstProgram();
            while (Peek().Type != TokenType.EndOfFile)
            {
                if (Peek().Type is TokenType.KwStruct or TokenType.KwUnion)
                {
                    ParseStructDef(p);
                    continue;
                }
                if (Peek().Type == TokenType.KwTypedef)
                {
                    Next();
                    var ts = ParseType();
                    string tn = Expect(TokenType.Identifier).Value;
                    Expect(TokenType.Semicolon);
                    p.Typedefs.Add(new TypedefDef { Type = ts, Name = tn });
                    _typeNames.Add(tn);
                    continue;
                }
                if (Peek().Type == TokenType.KwEnum)
                {
                    ParseEnumDef(p);
                    continue;
                }
                if (IsType(Peek().Type))
                {
                    int s = _pos;
                    var ts = ParseType();
                    if (Peek().Type == TokenType.Identifier && Peek(1).Type == TokenType.LParen)
                    {
                        string n = Expect(TokenType.Identifier).Value;
                        Expect(TokenType.LParen);
                        var par = new List<VarDeclaration>();
                        if (Peek().Type != TokenType.RParen)
                            par = ParseParmList();
                        Expect(TokenType.RParen);
                        // Forward declaration (semicolon) or function definition (brace)
                        if (Peek().Type == TokenType.Semicolon)
                        {
                            Next(); /* forward declaration, skip */
                        }
                        else
                        {
                            var b = ParseCompound();
                            p.Functions.Add(
                                new FunctionDef
                                {
                                    ReturnType = ts,
                                    Name = n,
                                    Parameters = par,
                                    Body = b,
                                }
                            );
                        }
                    }
                    else
                    {
                        _pos = s;
                        ParseGlobal(p);
                    }
                }
                else if (Peek().Type == TokenType.Semicolon)
                    Next();
                else
                    throw new Exception($"Unexpected: {Peek()}");
            }
            return p;
        }

        void ParseStructDef(AstProgram p)
        {
            bool isUnion = Peek().Type == TokenType.KwUnion;
            Next();
            string name = Expect(TokenType.Identifier).Value;
            _typeNames.Add(name);
            var sd = new StructDef { Name = name, IsUnion = isUnion };
            Expect(TokenType.LBrace);
            while (Peek().Type != TokenType.RBrace)
            {
                var ft = ParseType();
                string fn = Expect(TokenType.Identifier).Value;
                Expect(TokenType.Semicolon);
                sd.Fields.Add(new FieldDef { Type = ft, Name = fn });
            }
            Expect(TokenType.RBrace);
            Match(TokenType.Semicolon);
            p.Structs.Add(sd);
        }

        void ParseEnumDef(AstProgram p)
        {
            Next(); // skip enum
            Token nameToken = Expect(TokenType.Identifier);
            string name = nameToken.Value;
            _typeNames.Add(name);
            var ed = new EnumDef { Name = name };
            Expect(TokenType.LBrace);
            while (Peek().Type != TokenType.RBrace)
            {
                string memberName = Expect(TokenType.Identifier).Value;
                int? val = null;
                if (Match(TokenType.OpEq))
                    val = (int)ParseExprAsInt();
                ed.Members.Add(new EnumMember { Name = memberName, Value = val });
                if (!Match(TokenType.Comma))
                    break;
            }
            Expect(TokenType.RBrace);
            Expect(TokenType.Semicolon);
            p.Enums.Add(ed);
        }

        int ParseExprAsInt()
        {
            var node = ParseExpr();
            if (node is IntegerLiteral il)
                return (int)LiteralToLong(il.Value);
            if (node is UnaryOp uo && uo.Operator == "-" && uo.Operand is IntegerLiteral il2)
                return -(int)LiteralToLong(il2.Value);
            throw new Exception("Enum value must be an integer literal");
        }

        long LiteralToLong(string v)
        {
            if (v.StartsWith("0t"))
                return TritTypes.BalancedTernary.ParseToLong(v[2..].Replace("_", ""));
            return long.Parse(v);
        }

        void ParseGlobal(AstProgram p)
        {
            var ts = ParseType();
            string n = Expect(TokenType.Identifier).Value;
            while (Match(TokenType.LBracket))
            {
                if (Peek().Type == TokenType.RBracket)
                {
                    Next();
                    ts.Dims.Add(0);
                }
                else
                {
                    var d = ParseExpr();
                    Expect(TokenType.RBracket);
                    if (d is IntegerLiteral il && int.TryParse(il.Value, out int sz))
                        ts.Dims.Add(sz);
                }
            }
            AstNode? init = null;
            if (Match(TokenType.OpEq))
                init = ParseExpr();
            Expect(TokenType.Semicolon);
            p.Globals.Add(
                new VarDeclaration
                {
                    Name = n,
                    Type = ts,
                    Initializer = init,
                }
            );
        }

        TypeSpec ParseType()
        {
            TypeSpec ts;
            if (Peek().Type == TokenType.Identifier && _typeNames.Contains(Peek().Value))
            {
                ts = new TypeSpec { TypeName = Next().Value };
            }
            else
            {
                var m = new Dictionary<TokenType, string>
                {
                    { TokenType.KwVoid, "void" },
                    { TokenType.KwTrit, "trit" },
                    { TokenType.KwTril, "tril" },
                    { TokenType.KwTryte, "tryte" },
                    { TokenType.KwTshort, "tshort" },
                    { TokenType.KwTint, "tint" },
                    { TokenType.KwTlong, "tlong" },
                    { TokenType.KwTlongLong, "tlong long" },
                    { TokenType.KwTfloat, "tfloat" },
                    { TokenType.KwTdouble, "tdouble" },
                    { TokenType.KwStruct, "struct" },
                    { TokenType.KwUnion, "union" },
                };
                if (m.TryGetValue(Peek().Type, out var tn))
                {
                    Next();
                    ts = new TypeSpec { TypeName = tn };
                }
                else
                {
                    throw new Exception($"Unknown type: {Peek()}");
                }
            }

            if (ts.TypeName is "struct" or "union")
            {
                ts.StructName = Expect(TokenType.Identifier).Value;
            }
            while (Match(TokenType.OpStar))
                ts.PointerLevel++;
            return ts;
        }

        bool IsType(TokenType t)
        {
            if (t == TokenType.Identifier)
                return _typeNames.Contains(Peek().Value);
            return t
                is TokenType.KwVoid
                    or TokenType.KwTrit
                    or TokenType.KwTril
                    or TokenType.KwTryte
                    or TokenType.KwTshort
                    or TokenType.KwTint
                    or TokenType.KwTlong
                    or TokenType.KwTlongLong
                    or TokenType.KwTfloat
                    or TokenType.KwTdouble
                    or TokenType.KwSigned
                    or TokenType.KwUnsigned
                    or TokenType.KwStruct
                    or TokenType.KwUnion;
        }

        List<VarDeclaration> ParseParmList()
        {
            var l = new List<VarDeclaration>();
            do
            {
                var ts = ParseType();
                l.Add(new VarDeclaration { Name = Expect(TokenType.Identifier).Value, Type = ts });
            } while (Match(TokenType.Comma));
            return l;
        }

        Statement ParseStmt()
        {
            var t = Peek().Type;
            if (t == TokenType.LBrace)
                return ParseCompound();
            if (t == TokenType.KwIf)
                return ParseIf();
            if (t == TokenType.KwWhile)
                return ParseWhile();
            if (t == TokenType.KwDo)
                return ParseDoWhile();
            if (t == TokenType.KwFor)
                return ParseFor();
            if (t == TokenType.KwSwitch)
                return ParseSwitch();
            if (t == TokenType.KwReturn)
                return ParseRet();
            if (t == TokenType.KwBreak)
            {
                Next();
                Expect(TokenType.Semicolon);
                return new BreakStmt();
            }
            if (t == TokenType.KwContinue)
            {
                Next();
                Expect(TokenType.Semicolon);
                return new ContinueStmt();
            }
            if (t == TokenType.KwGoto)
            {
                Next();
                string label = Expect(TokenType.Identifier).Value;
                Expect(TokenType.Semicolon);
                return new GotoStmt { Label = label };
            }
            if (t == TokenType.Identifier && Peek(1).Type == TokenType.OpColon)
            {
                string label = Expect(TokenType.Identifier).Value;
                Expect(TokenType.OpColon);
                return new LabeledStmt { Label = label, Body = ParseStmt() };
            }
            if (IsType(t))
                return ParseVar(true);
            if (t == TokenType.Semicolon)
            {
                Next();
                return new ExpressionStmt();
            }
            var e = ParseExpr();
            Expect(TokenType.Semicolon);
            return new ExpressionStmt { Expression = e };
        }

        CompoundStmt ParseCompound()
        {
            Expect(TokenType.LBrace);
            var l = new List<Statement>();
            while (Peek().Type != TokenType.RBrace && Peek().Type != TokenType.EndOfFile)
                l.Add(ParseStmt());
            Expect(TokenType.RBrace);
            return new CompoundStmt { Body = l };
        }

        IfStmt ParseIf()
        {
            Expect(TokenType.KwIf);
            Expect(TokenType.LParen);
            var c = ParseExpr();
            Expect(TokenType.RParen);
            var th = ParseStmt();
            Statement? mb = null,
                el = null;
            if (Peek().Type == TokenType.KwMaybe || Peek().Type == TokenType.KwElse)
            {
                if (Peek().Type == TokenType.KwMaybe)
                {
                    Next();
                    mb = ParseStmt();
                }
                if (Peek().Type == TokenType.KwElse)
                {
                    Next();
                    el = ParseStmt();
                }
            }
            return new IfStmt
            {
                Condition = c,
                ThenBody = th,
                MaybeBody = mb,
                ElseBody = el,
            };
        }

        WhileStmt ParseWhile()
        {
            Expect(TokenType.KwWhile);
            Expect(TokenType.LParen);
            var c = ParseExpr();
            Expect(TokenType.RParen);
            return new WhileStmt { Condition = c, Body = ParseStmt() };
        }

        DoWhileStmt ParseDoWhile()
        {
            Expect(TokenType.KwDo);
            var body = ParseStmt();
            Expect(TokenType.KwWhile);
            Expect(TokenType.LParen);
            var c = ParseExpr();
            Expect(TokenType.RParen);
            Expect(TokenType.Semicolon);
            return new DoWhileStmt { Condition = c, Body = body };
        }

        ForStmt ParseFor()
        {
            Expect(TokenType.KwFor);
            Expect(TokenType.LParen);
            Statement? init = null;
            if (Peek().Type != TokenType.Semicolon)
            {
                if (IsType(Peek().Type))
                    init = ParseVar(false);
                else
                    init = new ExpressionStmt { Expression = ParseExpr() };
            }
            Expect(TokenType.Semicolon);
            AstNode? c = Peek().Type != TokenType.Semicolon ? ParseExpr() : null;
            Expect(TokenType.Semicolon);
            AstNode? s = Peek().Type != TokenType.RParen ? ParseExpr() : null;
            Expect(TokenType.RParen);
            return new ForStmt
            {
                Init = init,
                Condition = c,
                Step = s,
                Body = ParseStmt(),
            };
        }

        SwitchStmt ParseSwitch()
        {
            Expect(TokenType.KwSwitch);
            Expect(TokenType.LParen);
            var expr = ParseExpr();
            Expect(TokenType.RParen);
            Expect(TokenType.LBrace);
            var cases = new List<CaseStmt>();
            while (Peek().Type is TokenType.KwCase or TokenType.KwDefault)
            {
                AstNode? val = null;
                if (Match(TokenType.KwCase))
                    val = ParseExpr();
                else
                    Expect(TokenType.KwDefault);
                Expect(TokenType.OpColon);
                var body = new List<Statement>();
                while (
                    Peek().Type
                        is not (
                            TokenType.KwCase
                            or TokenType.KwDefault
                            or TokenType.RBrace
                            or TokenType.EndOfFile
                        )
                )
                    body.Add(ParseStmt());
                cases.Add(new CaseStmt { Value = val, Body = body });
            }
            Expect(TokenType.RBrace);
            return new SwitchStmt { Expression = expr, Cases = cases };
        }

        ReturnStmt ParseRet()
        {
            Expect(TokenType.KwReturn);
            AstNode? v = Peek().Type != TokenType.Semicolon ? ParseExpr() : null;
            Expect(TokenType.Semicolon);
            return new ReturnStmt { Value = v };
        }

        VarDeclaration ParseVar(bool endSemicolon)
        {
            var ts = ParseType();
            string n = Expect(TokenType.Identifier).Value;
            while (Match(TokenType.LBracket))
            {
                if (Peek().Type == TokenType.RBracket)
                {
                    Next();
                    ts.Dims.Add(0);
                }
                else
                {
                    var d = ParseExpr();
                    Expect(TokenType.RBracket);
                    if (d is IntegerLiteral il && int.TryParse(il.Value, out int sz))
                        ts.Dims.Add(sz);
                }
            }
            AstNode? init = null;
            if (Match(TokenType.OpEq))
                init = ParseExpr();
            if (endSemicolon)
            {
                Expect(TokenType.Semicolon);
            }
            return new VarDeclaration
            {
                Name = n,
                Type = ts,
                Initializer = init,
            };
        }

        // === Expressions ===
        AstNode ParseExpr() => ParseAssign();

        AstNode ParseAssign()
        {
            var l = ParseTernary();
            if (IsAss(Peek().Type))
            {
                var t = Peek();
                string o = Next().Value;
                return new Assignment
                {
                    Target = l,
                    Operator = o,
                    Value = ParseAssign(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        static bool IsAss(TokenType t) =>
            t
                is TokenType.OpEq
                    or TokenType.OpPlusEq
                    or TokenType.OpMinusEq
                    or TokenType.OpStarEq
                    or TokenType.OpSlashEq
                    or TokenType.OpPercentEq
                    or TokenType.OpAmpEq
                    or TokenType.OpPipeEq
                    or TokenType.OpCaretEq
                    or TokenType.OpLShiftEq
                    or TokenType.OpRShiftEq;

        AstNode ParseTernary()
        {
            var l = ParseOr();
            if (Peek().Type == TokenType.OpTernaryQuestion)
            {
                var t = Peek();
                Next();
                var te = ParseExpr();
                Expect(TokenType.OpTernaryMaybe);
                var m = ParseExpr();
                Expect(TokenType.OpTernaryFalse);
                var f = ParseExpr();
                return new TernaryExpr
                {
                    Condition = l,
                    TrueExpr = te,
                    MaybeExpr = m,
                    FalseExpr = f,
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseOr()
        {
            var l = ParseAnd();
            while (Match(TokenType.OpPipePipe))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = "||",
                    Left = l,
                    Right = ParseAnd(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseAnd()
        {
            var l = ParseBwOr();
            while (Match(TokenType.OpAndAnd))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = "&&",
                    Left = l,
                    Right = ParseBwOr(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseBwOr()
        {
            var l = ParseBwXor();
            while (Match(TokenType.OpPipe))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = "|",
                    Left = l,
                    Right = ParseBwXor(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseBwXor()
        {
            var l = ParseBwAnd();
            while (Match(TokenType.OpCaret))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = "^",
                    Left = l,
                    Right = ParseBwAnd(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseBwAnd()
        {
            var l = ParseEq();
            while (
                Peek().Type == TokenType.OpAmpersand
                && Peek(1).Type != TokenType.OpAmpEq
                && Peek(1).Type != TokenType.OpAndAnd
            )
            {
                var t = Peek();
                Next();
                l = new BinaryOp
                {
                    Operator = "&",
                    Left = l,
                    Right = ParseEq(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseEq()
        {
            var l = ParseRel();
            while (Match(TokenType.OpEqEq) || Match(TokenType.OpNeq))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = t.Value,
                    Left = l,
                    Right = ParseRel(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseRel()
        {
            var l = ParseShift();
            while (
                Peek().Type
                    is TokenType.OpLt
                        or TokenType.OpGt
                        or TokenType.OpLtEq
                        or TokenType.OpGtEq
            )
            {
                var t = Peek();
                string o = Next().Value;
                l = new BinaryOp
                {
                    Operator = o,
                    Left = l,
                    Right = ParseShift(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseShift()
        {
            var l = ParseAdd();
            while (Match(TokenType.OpLShift) || Match(TokenType.OpRShift))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = t.Value,
                    Left = l,
                    Right = ParseAdd(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseAdd()
        {
            var l = ParseMul();
            while (Match(TokenType.OpPlus) || Match(TokenType.OpMinus))
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = t.Value,
                    Left = l,
                    Right = ParseMul(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseMul()
        {
            var l = ParseUn();
            while (
                Match(TokenType.OpStar) || Match(TokenType.OpSlash) || Match(TokenType.OpPercent)
            )
            {
                var t = Peek(-1);
                l = new BinaryOp
                {
                    Operator = t.Value,
                    Left = l,
                    Right = ParseUn(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return l;
        }

        AstNode ParseUn()
        {
            if (
                Peek().Type
                is TokenType.OpMinus
                    or TokenType.OpPlus
                    or TokenType.OpExclamation
                    or TokenType.OpTilde
                    or TokenType.OpPlusPlus
                    or TokenType.OpMinusMinus
                    or TokenType.OpStar
                    or TokenType.OpAmpersand
            )
            {
                var t = Peek();
                string o = Next().Value;
                return new UnaryOp
                {
                    Operator = o,
                    Operand = ParseUn(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (Peek().Type == TokenType.KwSizeof)
            {
                var t = Peek();
                Next();
                if (Peek().Type == TokenType.LParen)
                {
                    Next();
                    var ts = ParseType();
                    Expect(TokenType.RParen);
                    return new IntegerLiteral
                    {
                        Value = GetTypeSize(ts).ToString(),
                        Suffix = "type",
                        Line = t.Line,
                        Column = t.Column,
                    };
                }
                var e = ParseUn();
                return new IntegerLiteral
                {
                    Value = "1",
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            return ParsePost();
        }

        AstNode ParsePost()
        {
            var n = ParsePri();
            while (true)
            {
                if (Peek().Type == TokenType.LParen)
                {
                    var t = Peek();
                    Next();
                    var a = new List<AstNode>();
                    if (Peek().Type != TokenType.RParen)
                    {
                        do a.Add(ParseExpr());
                        while (Match(TokenType.Comma));
                    }
                    Expect(TokenType.RParen);
                    n = new FunctionCall
                    {
                        FunctionName = (n as Identifier)?.Name ?? "",
                        Arguments = a,
                        Line = t.Line,
                        Column = t.Column,
                    };
                }
                else if (Peek().Type == TokenType.LBracket)
                {
                    var t = Peek();
                    var ar = new ArrayAccess
                    {
                        ArrayName = (n as Identifier)?.Name ?? "",
                        Line = t.Line,
                        Column = t.Column,
                    };
                    while (Match(TokenType.LBracket))
                    {
                        ar.Indices.Add(ParseExpr());
                        Expect(TokenType.RBracket);
                    }
                    n = ar;
                }
                else if (Peek().Type == TokenType.OpDot)
                {
                    var t = Peek();
                    Next();
                    n = new MemberAccess
                    {
                        Object = n,
                        MemberName = Expect(TokenType.Identifier).Value,
                        Line = t.Line,
                        Column = t.Column,
                    };
                }
                else if (Peek().Type == TokenType.OpArrow)
                {
                    var t = Peek();
                    Next();
                    n = new MemberAccess
                    {
                        Object = new UnaryOp { Operator = "*", Operand = n },
                        MemberName = Expect(TokenType.Identifier).Value,
                        Line = t.Line,
                        Column = t.Column,
                    };
                }
                else if (Peek().Type == TokenType.OpPlusPlus)
                {
                    var t = Peek();
                    Next();
                    n = new UnaryOp
                    {
                        Operator = "post++",
                        Operand = n,
                        Line = t.Line,
                        Column = t.Column,
                    };
                }
                else if (Peek().Type == TokenType.OpMinusMinus)
                {
                    var t = Peek();
                    Next();
                    n = new UnaryOp
                    {
                        Operator = "post--",
                        Operand = n,
                        Line = t.Line,
                        Column = t.Column,
                    };
                }
                else
                    break;
            }
            return n;
        }

        int GetTypeSize(TypeSpec ts)
        {
            if (ts.TypeName == "void")
                return 0;
            if (ts.TypeName == "trit" || ts.TypeName == "tril")
                return 3; // trit/tril promote to tint in expressions
            if (ts.TypeName == "tryte" || ts.TypeName == "char")
                return 1;
            if (ts.TypeName == "tshort" || ts.TypeName == "short")
                return 2;
            if (ts.TypeName == "tint" || ts.TypeName == "int")
                return 3;
            if (ts.TypeName == "tlong" || ts.TypeName == "long")
                return 6;
            if (ts.TypeName == "tlong long" || ts.TypeName == "tlonglong")
                return 9;
            if (ts.TypeName == "tfloat")
                return 3;
            if (ts.TypeName == "tdouble")
                return 6;
            if (ts.StructName != null)
                return 1; // struct — placeholder
            return 1;
        }

        /// <summary>Resolve escape sequences in a char literal value.</summary>
        static int ResolveCharEscape(string val)
        {
            if (val.Length == 1)
                return val[0];
            if (val.Length == 2 && val[0] == '\\')
            {
                return val[1] switch
                {
                    'n' => 10, // newline
                    'r' => 13, // carriage return
                    't' => 9, // tab
                    '0' => 0, // null
                    '\\' => 92, // backslash
                    '\'' => 39, // single quote
                    '"' => 34, // double quote
                    _ => val[1], // unknown escape — use the char itself
                };
            }
            return val[0];
        }

        AstNode ParsePri()
        {
            var t = Peek();
            if (t.Type == TokenType.IntegerLiteral)
            {
                Next();
                string v = t.Value;
                string? sf = null;
                if (v.EndsWith("tll"))
                {
                    sf = "tll";
                    v = v[..^3];
                }
                else if (v.EndsWith("tl"))
                {
                    sf = "tl";
                    v = v[..^2];
                }
                else if (v.EndsWith("t"))
                {
                    sf = "t";
                    v = v[..^1];
                }
                else if (v.EndsWith("s"))
                {
                    sf = "s";
                    v = v[..^1];
                }
                else if (v.EndsWith("y"))
                {
                    sf = "y";
                    v = v[..^1];
                }
                return new IntegerLiteral
                {
                    Value = v,
                    Suffix = sf,
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (t.Type == TokenType.FloatLiteral)
            {
                Next();
                return new FloatLiteral
                {
                    Value = t.Value,
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (t.Type == TokenType.StringLiteral)
            {
                Next();
                return new StringLiteral
                {
                    Value = t.Value,
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (t.Type == TokenType.CharLiteral)
            {
                Next();
                return new IntegerLiteral
                {
                    Value = ResolveCharEscape(t.Value).ToString(),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (t.Type is TokenType.KwTrue or TokenType.KwFalse or TokenType.KwMaybe)
            {
                Next();
                return new BooleanLiteral
                {
                    Value = t.Type == TokenType.KwTrue ? 1 : (t.Type == TokenType.KwFalse ? -1 : 0),
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (t.Type == TokenType.KwNull)
            {
                Next();
                return new NullLiteral { Line = t.Line, Column = t.Column };
            }
            if (t.Type == TokenType.LBrace)
            {
                Next();
                return ParseInitializer();
            }
            if (t.Type == TokenType.Identifier)
            {
                Next();
                return new Identifier
                {
                    Name = t.Value,
                    Line = t.Line,
                    Column = t.Column,
                };
            }
            if (t.Type == TokenType.LParen)
            {
                int saved = _pos;
                try
                {
                    if (IsType(Peek().Type))
                    {
                        var ts = ParseType();
                        if (Peek().Type == TokenType.RParen)
                        {
                            Next();
                            return new UnaryOp
                            {
                                Operator = "cast",
                                Operand = ParseUn(),
                                Line = t.Line,
                                Column = t.Column,
                            };
                        }
                    }
                }
                catch { }
                _pos = saved + 1;
                var e = ParseExpr();
                Expect(TokenType.RParen);
                return e;
            }
            throw new Exception($"Unexpected token in expression: {t}");
        }

        AstNode ParseInitializer()
        {
            var items = new List<AstNode>();
            while (Peek().Type != TokenType.RBrace)
            {
                items.Add(ParseExpr());
                if (Peek().Type == TokenType.Comma)
                    Next();
            }
            Expect(TokenType.RBrace);
            return new InitializerList { Items = items };
        }
    }
}
