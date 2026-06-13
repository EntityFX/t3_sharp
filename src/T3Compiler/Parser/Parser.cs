using System;
using System.Collections.Generic;
using T3Compiler.Lexer;

namespace T3Compiler.Parser
{
    /// <summary>
    /// Recursive-descent parser for the T language based on EBNF grammar.
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        private Token Peek() => _tokens[_pos];
        private Token Peek(int offset) =>
            (_pos + offset < _tokens.Count) ? _tokens[_pos + offset] : new Token(TokenType.EndOfFile, "", 0, 0);
        private Token Next()
        {
            var t = _tokens[_pos];
            if (_pos < _tokens.Count - 1) _pos++;
            return t;
        }
        private bool Match(TokenType type) { if (Peek().Type == type) { Next(); return true; } return false; }
        private Token Expect(TokenType type)
        {
            if (Peek().Type == type) return Next();
            throw new Exception($"Expected {type} but got {Peek()}");
        }

        // === Program → { FunctionDef | GlobalDeclaration } ===
        public AstProgram ParseProgram()
        {
            var prog = new AstProgram();
            while (Peek().Type != TokenType.EndOfFile)
            {
                if (IsTypeSpecifier(Peek().Type))
                {
                    // Check if it's a function: type ident ( args )
                    int save = _pos;
                    TypeSpec ts = ParseTypeSpec();
                    if (Peek().Type == TokenType.Identifier && Peek(1).Type == TokenType.LParen)
                    {
                        // Function definition
                        string name = Expect(TokenType.Identifier).Value;
                        Expect(TokenType.LParen);
                        var parms = new List<VarDeclaration>();
                        if (Peek().Type != TokenType.RParen)
                            parms = ParseParameterList();
                        Expect(TokenType.RParen);
                        var body = ParseCompoundStmt();
                        prog.Functions.Add(new FunctionDef { ReturnType = ts, Name = name, Parameters = parms, Body = body });
                    }
                    else
                    {
                        // Global variable
                        _pos = save;
                        ParseGlobalDeclaration(prog);
                    }
                }
                else if (Peek().Type == TokenType.Semicolon)
                {
                    Next(); // empty ;
                }
                else
                {
                    throw new Exception($"Unexpected token at top level: {Peek()}");
                }
            }
            return prog;
        }

        private void ParseGlobalDeclaration(AstProgram prog)
        {
            TypeSpec ts = ParseTypeSpec();
            string name = Expect(TokenType.Identifier).Value;
            AstNode? init = null;
            if (Match(TokenType.OpEq))
                init = ParseExpression();
            Expect(TokenType.Semicolon);
            prog.Globals.Add(new VarDeclaration { Name = name, Type = ts, Initializer = init });
        }

        // === Type specifier ===
        private TypeSpec ParseTypeSpec()
        {
            var ts = new TypeSpec { TypeName = "tint" };
            var typeMap = new Dictionary<TokenType, string>
            {
                {TokenType.KwVoid, "void"}, {TokenType.KwTrit, "trit"}, {TokenType.KwTril, "tril"},
                {TokenType.KwTryte, "tryte"}, {TokenType.KwTshort, "tshort"}, {TokenType.KwTint, "tint"},
                {TokenType.KwTlong, "tlong"}, {TokenType.KwTlongLong, "tlong long"},
                {TokenType.KwTfloat, "tfloat"}, {TokenType.KwTdouble, "tdouble"},
            };
            if (typeMap.TryGetValue(Peek().Type, out var tn)) { Next(); ts.TypeName = tn; }
            while (Match(TokenType.OpStar)) ts.PointerLevel++;
            return ts;
        }

        private bool IsTypeSpecifier(TokenType t) =>
            t == TokenType.KwVoid || t == TokenType.KwTrit || t == TokenType.KwTril ||
            t == TokenType.KwTryte || t == TokenType.KwTshort || t == TokenType.KwTint ||
            t == TokenType.KwTlong || t == TokenType.KwTlongLong || t == TokenType.KwTfloat || t == TokenType.KwTdouble ||
            t == TokenType.KwSigned || t == TokenType.KwUnsigned;

        // === Parameter list ===
        private List<VarDeclaration> ParseParameterList()
        {
            var list = new List<VarDeclaration>();
            do
            {
                TypeSpec ts = ParseTypeSpec();
                string name = Expect(TokenType.Identifier).Value;
                list.Add(new VarDeclaration { Name = name, Type = ts });
            } while (Match(TokenType.Comma));
            return list;
        }

        // === Statements ===
        private Statement ParseStatement()
        {
            var t = Peek().Type;
            if (t == TokenType.LBrace) return ParseCompoundStmt();
            if (t == TokenType.KwIf) return ParseIfStmt();
            if (t == TokenType.KwWhile) return ParseWhileStmt();
            if (t == TokenType.KwFor) return ParseForStmt();
            if (t == TokenType.KwReturn) return ParseReturnStmt();
            if (t == TokenType.KwBreak) { Next(); Expect(TokenType.Semicolon); return new BreakStmt(); }
            if (t == TokenType.KwContinue) { Next(); Expect(TokenType.Semicolon); return new ContinueStmt(); }
            if (IsTypeSpecifier(t)) return ParseVarDeclaration();
            if (t == TokenType.Semicolon) { Next(); return new ExpressionStmt(); }
            // Expression statement
            var expr = ParseExpression();
            Expect(TokenType.Semicolon);
            return new ExpressionStmt { Expression = expr };
        }

        private CompoundStmt ParseCompoundStmt()
        {
            Expect(TokenType.LBrace);
            var stmts = new List<Statement>();
            while (Peek().Type != TokenType.RBrace && Peek().Type != TokenType.EndOfFile)
                stmts.Add(ParseStatement());
            Expect(TokenType.RBrace);
            return new CompoundStmt { Body = stmts };
        }

        private IfStmt ParseIfStmt()
        {
            Expect(TokenType.KwIf);
            Expect(TokenType.LParen);
            var cond = ParseExpression();
            Expect(TokenType.RParen);
            var thenBody = ParseStatement();
            Statement? maybeBody = null;
            Statement? elseBody = null;
            if (Peek().Type == TokenType.KwMaybe || Peek().Type == TokenType.KwElse)
            {
                if (Peek().Type == TokenType.KwMaybe) { Next(); maybeBody = ParseStatement(); }
                if (Peek().Type == TokenType.KwElse) { Next(); elseBody = ParseStatement(); }
            }
            return new IfStmt { Condition = cond, ThenBody = thenBody, MaybeBody = maybeBody, ElseBody = elseBody };
        }

        private WhileStmt ParseWhileStmt()
        {
            Expect(TokenType.KwWhile); Expect(TokenType.LParen);
            var cond = ParseExpression();
            Expect(TokenType.RParen);
            var body = ParseStatement();
            return new WhileStmt { Condition = cond, Body = body };
        }

        private ForStmt ParseForStmt()
        {
            Expect(TokenType.KwFor); Expect(TokenType.LParen);
            AstNode? init = Peek().Type != TokenType.Semicolon ? ParseExpression() : null;
            Expect(TokenType.Semicolon);
            AstNode? cond = Peek().Type != TokenType.Semicolon ? ParseExpression() : null;
            Expect(TokenType.Semicolon);
            AstNode? step = Peek().Type != TokenType.RParen ? ParseExpression() : null;
            Expect(TokenType.RParen);
            var body = ParseStatement();
            return new ForStmt { Init = init, Condition = cond, Step = step, Body = body };
        }

        private ReturnStmt ParseReturnStmt()
        {
            Expect(TokenType.KwReturn);
            AstNode? val = Peek().Type != TokenType.Semicolon ? ParseExpression() : null;
            Expect(TokenType.Semicolon);
            return new ReturnStmt { Value = val };
        }

        private VarDeclaration ParseVarDeclaration()
        {
            TypeSpec ts = ParseTypeSpec();
            string name = Expect(TokenType.Identifier).Value;
            AstNode? init = null;
            if (Match(TokenType.OpEq))
                init = ParseExpression();
            Expect(TokenType.Semicolon);
            return new VarDeclaration { Name = name, Type = ts, Initializer = init };
        }

        // === Expression parsing (Pratt-like precedence) ===
        private AstNode ParseExpression() => ParseAssignment();

        private AstNode ParseAssignment()
        {
            var left = ParseTernary();
            if (IsAssignmentOp(Peek().Type))
            {
                string op = Next().Value;
                var right = ParseAssignment();
                return new Assignment { Target = left, Operator = op, Value = right };
            }
            return left;
        }

        private static bool IsAssignmentOp(TokenType t) =>
            t == TokenType.OpEq || t == TokenType.OpPlusEq || t == TokenType.OpMinusEq ||
            t == TokenType.OpStarEq || t == TokenType.OpSlashEq || t == TokenType.OpPercentEq;

        private AstNode ParseTernary()
        {
            var left = ParseLogicalOr();
            if (Peek().Type == TokenType.OpTernaryQuestion)
            {
                Next(); // ??
                var trueExpr = ParseExpression();
                Expect(TokenType.OpTernaryMaybe); // :?
                var maybeExpr = ParseExpression();
                Expect(TokenType.OpTernaryFalse); // :!
                var falseExpr = ParseExpression();
                return new TernaryExpr { Condition = left, TrueExpr = trueExpr, MaybeExpr = maybeExpr, FalseExpr = falseExpr };
            }
            return left;
        }

        private AstNode ParseLogicalOr()
        {
            var left = ParseLogicalAnd();
            while (Peek().Type == TokenType.OpPipePipe)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseLogicalAnd() };
            }
            return left;
        }

        private AstNode ParseLogicalAnd()
        {
            var left = ParseBitwiseOr();
            while (Peek().Type == TokenType.OpAndAnd)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseBitwiseOr() };
            }
            return left;
        }

        private AstNode ParseBitwiseOr()
        {
            var left = ParseBitwiseXor();
            while (Peek().Type == TokenType.OpPipe)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseBitwiseXor() };
            }
            return left;
        }

        private AstNode ParseBitwiseXor()
        {
            var left = ParseBitwiseAnd();
            while (Peek().Type == TokenType.OpCaret)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseBitwiseAnd() };
            }
            return left;
        }

        private AstNode ParseBitwiseAnd()
        {
            var left = ParseEquality();
            while (Peek().Type == TokenType.OpAmpersand && Peek(1).Type != TokenType.OpAmpEq &&
                   Peek(1).Type != TokenType.OpAndAnd) // distinguish & from && and &=
            {
                if (Peek().Type == TokenType.OpAmpersand) { Next(); left = new BinaryOp { Operator = "&", Left = left, Right = ParseEquality() }; }
                else break;
            }
            return left;
        }

        private AstNode ParseEquality()
        {
            var left = ParseRelational();
            while (Peek().Type == TokenType.OpEqEq || Peek().Type == TokenType.OpNeq)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseRelational() };
            }
            return left;
        }

        private AstNode ParseRelational()
        {
            var left = ParseShift();
            while (Peek().Type == TokenType.OpLt || Peek().Type == TokenType.OpGt ||
                   Peek().Type == TokenType.OpLtEq || Peek().Type == TokenType.OpGtEq)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseShift() };
            }
            return left;
        }

        private AstNode ParseShift()
        {
            var left = ParseAdditive();
            while (Peek().Type == TokenType.OpLShift || Peek().Type == TokenType.OpRShift)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseAdditive() };
            }
            return left;
        }

        private AstNode ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (Peek().Type == TokenType.OpPlus || Peek().Type == TokenType.OpMinus)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseMultiplicative() };
            }
            return left;
        }

        private AstNode ParseMultiplicative()
        {
            var left = ParseUnary();
            while (Peek().Type == TokenType.OpStar || Peek().Type == TokenType.OpSlash || Peek().Type == TokenType.OpPercent)
            {
                string op = Next().Value;
                left = new BinaryOp { Operator = op, Left = left, Right = ParseUnary() };
            }
            return left;
        }

        private AstNode ParseUnary()
        {
            if (Peek().Type == TokenType.OpMinus || Peek().Type == TokenType.OpPlus ||
                Peek().Type == TokenType.OpExclamation || Peek().Type == TokenType.OpTilde ||
                Peek().Type == TokenType.OpPlusPlus || Peek().Type == TokenType.OpMinusMinus)
            {
                string op = Next().Value;
                return new UnaryOp { Operator = op, Operand = ParseUnary() };
            }
            return ParsePostfix();
        }

        private AstNode ParsePostfix()
        {
            var node = ParsePrimary();
            while (true)
            {
                if (Peek().Type == TokenType.LParen)
                {
                    Next();
                    var args = new List<AstNode>();
                    if (Peek().Type != TokenType.RParen)
                    {
                        do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
                    }
                    Expect(TokenType.RParen);
                    var id = node as Identifier;
                    node = new FunctionCall { FunctionName = id?.Name ?? "", Arguments = args };
                }
                else if (Peek().Type == TokenType.OpPlusPlus || Peek().Type == TokenType.OpMinusMinus)
                {
                    string op = Next().Value;
                    node = new UnaryOp { Operator = op == "++" ? "post++" : "post--", Operand = node };
                }
                else break;
            }
            return node;
        }

        private AstNode ParsePrimary()
        {
            var t = Peek();
            if (t.Type == TokenType.IntegerLiteral)
            {
                Next();
                string val = t.Value;
                string? suffix = null;
                if (val.EndsWith("y") || val.EndsWith("s") || val.EndsWith("t") || val.EndsWith("tl") || val.EndsWith("tll"))
                {
                    suffix = val.Substring(val.Length - (val.EndsWith("tll") ? 3 : val.EndsWith("tl") ? 2 : 1));
                    val = val.Substring(0, val.Length - suffix.Length);
                }
                return new IntegerLiteral { Value = val, Suffix = suffix };
            }
            if (t.Type == TokenType.FloatLiteral) { Next(); return new FloatLiteral { Value = t.Value }; }
            if (t.Type == TokenType.StringLiteral) { Next(); return new StringLiteral { Value = t.Value }; }
            if (t.Type == TokenType.KwTrue || t.Type == TokenType.KwFalse || t.Type == TokenType.KwMaybe)
            {
                Next();
                return new BooleanLiteral { Value = t.Type == TokenType.KwTrue ? true : (t.Type == TokenType.KwFalse ? false : null) };
            }
            if (t.Type == TokenType.Identifier) { Next(); return new Identifier { Name = t.Value }; }
            if (t.Type == TokenType.LParen)
            {
                Next();
                var expr = ParseExpression();
                Expect(TokenType.RParen);
                return expr;
            }
            throw new Exception($"Unexpected token in expression: {t}");
        }
    }
}