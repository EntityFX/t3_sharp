using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using T3Compiler.Parser;

namespace T3Compiler.Parser
{
    public static class AstDumper
    {
        public static string Dump(AstProgram program)
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- AST Dump ---");
            
            foreach (var func in program.Functions)
            {
                sb.AppendLine($"Function: {func.Name}");
                sb.AppendLine($"  Return Type: {func.ReturnType.TypeName}");
                foreach (var param in func.Parameters)
                {
                    sb.AppendLine($"  Param: {param.Name} : {param.Type.TypeName}");
                }
                sb.AppendLine("  Body:");
                DumpStatement(sb, func.Body, 2);
            }
            
            foreach (var glob in program.Globals)
            {
                sb.AppendLine($"Global: {glob.Name} : {glob.Type.TypeName}");
            }
            
            foreach (var sdef in program.Structs)
            {
                sb.AppendLine($"Struct: {sdef.Name} ({(sdef.IsUnion ? "Union" : "Struct")})");
                foreach (var field in sdef.Fields)
                {
                    sb.AppendLine($"  Field: {field.Name} : {field.Type.TypeName}");
                }
            }
            
            foreach (var enums in program.Enums)
            {
                sb.AppendLine($"Enum: {enums.Name}");
                foreach (var member in enums.Members)
                {
                    sb.AppendLine($"  Member: {member.Name} {(member.Value.HasValue ? $"= {member.Value}" : "")}");
                }
            }
            
            return sb.ToString();
        }

        private static void DumpStatement(StringBuilder sb, Statement stmt, int indent)
        {
            string pad = new string(' ', indent * 2);
            
            if (stmt is ExpressionStmt exprStmt)
            {
                sb.AppendLine($"{pad}ExprStmt: {DumpExpression(exprStmt.Expression, indent + 1)}");
            }
            else if (stmt is ReturnStmt retStmt)
            {
                sb.AppendLine($"{pad}ReturnStmt: {(retStmt.Value != null ? DumpExpression(retStmt.Value, indent + 1) : "null")}");
            }
            else if (stmt is CompoundStmt compStmt)
            {
                sb.AppendLine($"{pad}CompoundStmt:");
                foreach (var s in compStmt.Body)
                {
                    DumpStatement(sb, s, indent + 1);
                }
            }
            else if (stmt is VarDeclaration varDecl)
            {
                sb.AppendLine($"{pad}VarDecl: {varDecl.Name} : {varDecl.Type.TypeName} {(varDecl.Initializer != null ? $"= {DumpExpression(varDecl.Initializer, indent + 1)}" : "")}");
            }
            else if (stmt is IfStmt ifStmt)
            {
                sb.AppendLine($"{pad}IfStmt:");
                sb.AppendLine($"{pad}  Condition: {DumpExpression(ifStmt.Condition, indent + 1)}");
                sb.AppendLine($"{pad}  Then:");
                DumpStatement(sb, ifStmt.ThenBody, indent + 1);
                if (ifStmt.MaybeBody != null)
                {
                    sb.AppendLine($"{pad}  Maybe:");
                    DumpStatement(sb, ifStmt.MaybeBody, indent + 1);
                }
                if (ifStmt.ElseBody != null)
                {
                    sb.AppendLine($"{pad}  Else:");
                    DumpStatement(sb, ifStmt.ElseBody, indent + 1);
                }
            }
            else if (stmt is WhileStmt whileStmt)
            {
                sb.AppendLine($"{pad}WhileStmt:");
                sb.AppendLine($"{pad}  Condition: {DumpExpression(whileStmt.Condition, indent + 1)}");
                sb.AppendLine($"{pad}  Body:");
                DumpStatement(sb, whileStmt.Body, indent + 1);
            }
            else if (stmt is DoWhileStmt doWhileStmt)
            {
                sb.AppendLine($"{pad}DoWhileStmt:");
                sb.AppendLine($"{pad}  Body:");
                DumpStatement(sb, doWhileStmt.Body, indent + 1);
                sb.AppendLine($"{pad}  Condition: {DumpExpression(doWhileStmt.Condition, indent + 1)}");
            }
            else if (stmt is ForStmt forStmt)
            {
                sb.AppendLine($"{pad}ForStmt:");
                if (forStmt.Init != null)
                {
                    sb.AppendLine($"{pad}  Init:");
                    DumpStatement(sb, forStmt.Init, indent + 2);
                }
                else
                {
                    sb.AppendLine($"{pad}  Init: null");
                }
                sb.AppendLine($"{pad}  Condition:");
                sb.AppendLine($"{pad}    {(forStmt.Condition != null ? DumpExpression(forStmt.Condition, indent + 1) : "true")}");
                sb.AppendLine($"{pad}  Step:");
                sb.AppendLine($"{pad}    {(forStmt.Step != null ? DumpExpression(forStmt.Step, indent + 1) : "null")}");
                sb.AppendLine($"{pad}  Body:");
                DumpStatement(sb, forStmt.Body, indent + 2);
            }
            else if (stmt is BreakStmt)
            {
                sb.AppendLine($"{pad}BreakStmt");
            }
            else if (stmt is ContinueStmt)
            {
                sb.AppendLine($"{pad}ContinueStmt");
            }
            else if (stmt is GotoStmt gotoStmt)
            {
                sb.AppendLine($"{pad}GotoStmt: {gotoStmt.Label}");
            }
            else if (stmt is LabeledStmt labStmt)
            {
                sb.AppendLine($"{pad}LabeledStmt: {labStmt.Label}");
                DumpStatement(sb, labStmt.Body, indent + 1);
            }
            else if (stmt is IntrinsicCall intrinsic)
            {
                sb.AppendLine($"{pad}IntrinsicCall: {intrinsic.Name}");
                foreach (var arg in intrinsic.Arguments)
                {
                    sb.AppendLine($"{pad}  Arg: {DumpExpression(arg, indent + 1)}");
                }
            }
            else if (stmt is SwitchStmt swStmt)
            {
                sb.AppendLine($"{pad}SwitchStmt:");
                sb.AppendLine($"{pad}  Expr: {DumpExpression(swStmt.Expression, indent + 1)}");
                foreach (var caseS in swStmt.Cases)
                {
                    sb.AppendLine($"{pad}  Case:");
                    if (caseS.Value != null)
                    {
                        sb.AppendLine($"{pad}    Value: {DumpExpression(caseS.Value, indent + 1)}");
                    }
                    else
                    {
                        sb.AppendLine($"{pad}    Default");
                    }
                    foreach (var s in caseS.Body)
                    {
                        DumpStatement(sb, s, indent + 1);
                    }
                }
            }
        }

        private static string DumpExpression(AstNode node, int indent)
        {
            string pad = new string(' ', indent * 2);
            if (node == null) return "null";

            if (node is IntegerLiteral intLit) return $"IntegerLiteral: {intLit.Value}";
            if (node is FloatLiteral fltLit) return $"FloatLiteral: {fltLit.Value}";
            if (node is StringLiteral strLit) return $"StringLiteral: {strLit.Value}";
            if (node is Identifier id) return $"Identifier: {id.Name}";
            if (node is BooleanLiteral boolLit) return $"BooleanLiteral: {boolLit.Value}";
            if (node is BinaryOp binOp)
            {
                return $"BinaryOp: {binOp.Operator} ({DumpExpression(binOp.Left, indent)} {DumpExpression(binOp.Right, indent)})";
            }
            if (node is UnaryOp unaryOp)
            {
                return $"UnaryOp: {unaryOp.Operator} ({DumpExpression(unaryOp.Operand, indent)})";
            }
            if (node is Assignment assign)
            {
                return $"Assignment: {assign.Operator} ({DumpExpression(assign.Target, indent)} = {DumpExpression(assign.Value, indent)})";
            }
            if (node is AssignmentList assignmentList)
            {
                return $"AssignmentList: {string.Join(", ", assignmentList.Assignments.Select(i => DumpExpression(i, indent)))})";
            }
            if (node is FunctionCall funcCall)
            {
                return $"FunctionCall: {funcCall.FunctionName} ({string.Join(", ", funcCall.Arguments.Select(a => DumpExpression(a, indent)))} )";
            }
            if (node is TernaryExpr tern)
            {
                return $"TernaryExpr: {DumpExpression(tern.Condition, indent)} ? {DumpExpression(tern.TrueExpr, indent)} : {DumpExpression(tern.MaybeExpr, indent)} : {DumpExpression(tern.FalseExpr, indent)}";
            }
            if (node is ArrayAccess arrayAcc)
            {
                return $"ArrayAccess: {arrayAcc.ArrayName} ({string.Join(", ", arrayAcc.Indices.Select(i => DumpExpression(i, indent)))} )";
            }
            if (node is MemberAccess memAcc)
            {
                return $"MemberAccess: {memAcc.Object} ({memAcc.MemberName})";
            }
            
            return "UnknownNode";
        }

        // Helper for DumpStatement to avoid return-value issues with string concatenation
        private static string DumpStatementToString(Statement stmt, int indent)
        {
            return "Statement dump is performed via StringBuilder. Use DumpStatement directly.";
        }
    }
}
