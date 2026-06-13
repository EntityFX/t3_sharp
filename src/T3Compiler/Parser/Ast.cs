using System.Collections.Generic;

namespace T3Compiler.Parser
{
    // === Expressions ===
    public abstract class AstNode { }

    public class IntegerLiteral : AstNode { public string Value; public string? Suffix; }
    public class FloatLiteral : AstNode { public string Value; }
    public class StringLiteral : AstNode { public string Value; }
    public class Identifier : AstNode { public string Name; }
    public class BooleanLiteral : AstNode { public bool? Value; } // true=1, false=0, maybe=null

    public class BinaryOp : AstNode
    {
        public string Operator;
        public AstNode Left;
        public AstNode Right;
    }

    public class UnaryOp : AstNode
    {
        public string Operator;
        public AstNode Operand;
    }

    public class Assignment : AstNode
    {
        public AstNode Target;
        public string Operator; // =, +=, -=, etc.
        public AstNode Value;
    }

    public class FunctionCall : AstNode
    {
        public string FunctionName;
        public List<AstNode> Arguments;
    }

    public class TernaryExpr : AstNode
    {
        public AstNode Condition;
        public AstNode TrueExpr;
        public AstNode MaybeExpr;
        public AstNode FalseExpr;
    }

    // === Types ===
    public class TypeSpec
    {
        public string TypeName;       // void, trit, tril, tryte, tshort, tint, tlong, tfloat, tdouble
        public int PointerLevel;      // number of * (0 for non-pointer)
        public bool IsConst;
        public bool IsVolatile;
    }

    // === Statements ===
    public abstract class Statement { }

    public class ExpressionStmt : Statement { public AstNode Expression; }
    public class ReturnStmt : Statement { public AstNode? Value; }
    public class CompoundStmt : Statement { public List<Statement> Body; }
    public class VarDeclaration : Statement { public string Name; public TypeSpec Type; public AstNode? Initializer; }

    public class IfStmt : Statement
    {
        public AstNode Condition;
        public Statement ThenBody;
        public Statement? MaybeBody;
        public Statement? ElseBody;
    }

    public class WhileStmt : Statement
    {
        public AstNode Condition;
        public Statement Body;
    }

    public class ForStmt : Statement
    {
        public AstNode? Init;
        public AstNode? Condition;
        public AstNode? Step;
        public Statement Body;
    }

    public class BreakStmt : Statement { }
    public class ContinueStmt : Statement { }

    // === Top-level ===
    public class FunctionDef
    {
        public TypeSpec ReturnType;
        public string Name;
        public List<VarDeclaration> Parameters;
        public CompoundStmt Body;
    }

    public class AstProgram
    {
        public List<FunctionDef> Functions = new();
        public List<VarDeclaration> Globals = new();
    }
}