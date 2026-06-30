using System.Collections.Generic;

namespace T3Compiler.Parser
{
    // === Expressions ===
    public abstract class AstNode { }

    public class IntegerLiteral : AstNode { public string Value; public string? Suffix; }
    public class FloatLiteral : AstNode { public string Value; }
    public class StringLiteral : AstNode { public string Value; }
    public class Identifier : AstNode { public string Name; }
    public class BooleanLiteral : AstNode { public int Value; } // trit: true=+1, false=-1, maybe=0
    public class NullLiteral : AstNode { }
    public class InitializerList : AstNode { public List<AstNode> Items = new(); }

    public class BinaryOp : AstNode
    { public string Operator; public AstNode Left; public AstNode Right; }

    public class UnaryOp : AstNode
    { public string Operator; public AstNode Operand; }

    public class Assignment : AstNode
    {
        public AstNode Target;
        public string Operator; public AstNode Value;
    }

    public class FunctionCall : AstNode
    { public string FunctionName; public List<AstNode> Arguments = new(); }

    public class TernaryExpr : AstNode
    { public AstNode Condition; public AstNode TrueExpr; public AstNode MaybeExpr; public AstNode FalseExpr; }

    public class ArrayAccess : AstNode
    { public string ArrayName; public List<AstNode> Indices = new(); }

    public class MemberAccess : AstNode
    { public AstNode Object; public string MemberName; }

    // === Types ===
    public class TypeSpec
    {
        public string TypeName;
        public int PointerLevel;
        public List<int> Dims = new();
        public bool IsConst, IsVolatile;
        public string? StructName; // if this is a struct/union type
    }

    // Struct/union definition
    public class FieldDef
    { public TypeSpec Type; public string Name; }

    public class StructDef
    { public string Name; public List<FieldDef> Fields = new(); public bool IsUnion; }

    public class EnumMember
    { public string Name; public int? Value; }

    public class EnumDef
    { public string Name; public List<EnumMember> Members = new(); }

    // === Statements ===
    public abstract class Statement { }
    public class ExpressionStmt : Statement { public AstNode Expression; }
    public class ReturnStmt : Statement { public AstNode? Value; }
    public class CompoundStmt : Statement { public List<Statement> Body = new(); }
    public class VarDeclaration : Statement { public string Name; public TypeSpec Type; public AstNode? Initializer; }

    public class IfStmt : Statement
    { public AstNode Condition; public Statement ThenBody; public Statement? MaybeBody, ElseBody; }

    public class WhileStmt : Statement
    { public AstNode Condition; public Statement Body; }

    public class DoWhileStmt : Statement
    { public AstNode Condition; public Statement Body; }

    public class ForStmt : Statement
    { public Statement? Init; public AstNode? Condition, Step; public Statement Body; }

    public class BreakStmt : Statement { }
    public class ContinueStmt : Statement { }
    public class GotoStmt : Statement { public string Label; }
    public class LabeledStmt : Statement { public string Label; public Statement Body; }
    public class IntrinsicCall : Statement { public string Name; public List<AstNode> Arguments = new(); }

    public class SwitchStmt : Statement
    { public AstNode Expression; public List<CaseStmt> Cases = new(); }

    public class CaseStmt
    { public AstNode? Value; public List<Statement> Body = new(); } // Value=null means default

    // === Top-level ===
    public class FunctionDef
    { public TypeSpec ReturnType; public string Name; public List<VarDeclaration> Parameters = new(); public CompoundStmt Body; }

    public class TypedefDef
    { public TypeSpec Type; public string Name; }

    public class AstProgram
    {
        public List<FunctionDef> Functions = new();
        public List<VarDeclaration> Globals = new();
        public List<StructDef> Structs = new();  // user-defined struct/union types
        public List<EnumDef> Enums = new();
        public List<TypedefDef> Typedefs = new();
    }
}