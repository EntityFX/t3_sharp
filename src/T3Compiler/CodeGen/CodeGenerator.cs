using System;
using System.Collections.Generic;
using System.Text;
using T3Compiler.Parser;
using T3Simulator.Common;
using TritTypes;

namespace T3Compiler.CodeGen
{
    public class CodeGenerator
    {
        private readonly AstProgram _program;
        private readonly StringBuilder _output;
        private int _labelCounter;
        private readonly Dictionary<string, int> _varSlots;

        public CodeGenerator(AstProgram program)
        {
            _program = program;
            _output = new StringBuilder();
            _labelCounter = 0;
            _varSlots = new Dictionary<string, int>();
        }

        public string Generate()
        {
            Emit("; T-lang compiled output → T3 assembly");
            Emit("; ====================================");
            Emit();
            Emit("__entry:");
            Emit("    LI R0, main");
            Emit("    CALL R0");
            Emit("    HALT");
            Emit();
            foreach (var func in _program.Functions) GenerateFunction(func);
            return _output.ToString();
        }

        void GenerateFunction(FunctionDef func)
        {
            _varSlots.Clear();
            Emit($"{func.Name}:");
            foreach (var s in func.Body.Body) GenStmt(s);
            Emit("    RET");
            Emit();
        }

        void GenStmt(Statement s)
        {
            switch (s)
            {
                case ExpressionStmt es:
                    if (es.Expression != null) GenExpr(es.Expression); break;
                case VarDeclaration vd:
                    AllocLocal(vd.Name);
                    if (vd.Initializer != null) { int r = GenExpr(vd.Initializer); StoreLocal(vd.Name, r); }
                    break;
                case ReturnStmt rs:
                    if (rs.Value != null) { int r = GenExpr(rs.Value); Emit($"    MOV R2, R{r}"); }
                    Emit("    RET"); break;
                case CompoundStmt cs:
                    foreach (var ss in cs.Body) GenStmt(ss); break;
                case IfStmt ifs: GenIf(ifs); break;
                case WhileStmt ws: GenWhile(ws); break;
                case ForStmt fs:
                    if (fs.Init != null) GenExpr(fs.Init);
                    if (fs.Condition is BinaryOp bo)
                        GenWhile(new WhileStmt { Condition = bo, Body = new CompoundStmt { Body = new List<Statement> { fs.Body, MakeExprStmt(fs.Step) } } });
                    else if (fs.Step == null) GenStmt(fs.Body);
                    break;
            }
        }

        ExpressionStmt MakeExprStmt(AstNode? node) => new ExpressionStmt { Expression = node };

        // === IF ===
        void GenIf(IfStmt s)
        {
            string lEnd = Label("end");
            if (s.Condition is BinaryOp bo)
            {
                int a = GenExpr(bo.Left), b = GenExpr(bo.Right);
                string lThen = Label("ift");
                Emit($"    CMP R{a}, R{b}");
                EmitCondJump(bo.Operator, lThen, true);   // if true → then
                // else path
                if (s.ElseBody != null) GenStmt(s.ElseBody);
                JmpTo(lEnd);
                // then path
                Emit($"{lThen}:");
                GenStmt(s.ThenBody);
                JmpTo(lEnd);
            }
            else
            {
                int c = GenExpr(s.Condition);
                string lThen = Label("ift");
                Emit($"    LI R1, 0");
                Emit($"    CMP R{c}, R1");
                Jump("JG", lThen);   // value >0 → true
                if (s.ElseBody != null) GenStmt(s.ElseBody);
                JmpTo(lEnd);
                Emit($"{lThen}:");
                GenStmt(s.ThenBody);
                JmpTo(lEnd);
            }
            Emit($"{lEnd}:");
        }

        // === WHILE ===
        void GenWhile(WhileStmt s)
        {
            string lLoop = Label("loop"), lBody = Label("body"), lEnd = Label("wend");
            Emit($"{lLoop}:");
            if (s.Condition is BinaryOp bo)
            {
                int a = GenExpr(bo.Left), b = GenExpr(bo.Right);
                Emit($"    CMP R{a}, R{b}");
                EmitCondJump(bo.Operator, lBody, true);
                JmpTo(lEnd);
            }
            else
            {
                int c = GenExpr(s.Condition);
                Emit($"    LI R1, 0");
                Emit($"    CMP R{c}, R1");
                Jump("JG", lBody);
                JmpTo(lEnd);
            }
            Emit($"{lBody}:");
            GenStmt(s.Body);
            JmpTo(lLoop);
            Emit($"{lEnd}:");
        }

        // === Expressions ===
        int GenExpr(AstNode n) => n switch
        {
            IntegerLiteral il => EmitImm(ParseInt(il.Value)),
            Identifier id => LoadLocal(id.Name),
            BooleanLiteral bl => EmitImm(bl.Value == true ? 1 : (bl.Value == false ? -1 : 0)),
            BinaryOp bo => GenBinOp(bo),
            UnaryOp uo => GenUnary(uo),
            Assignment ass => EmitAssign(ass),
            _ => EmitImm(0)
        };

        int GenBinOp(BinaryOp bo)
        {
            int a = GenExpr(bo.Left), b = GenExpr(bo.Right);
            if (IsCompare(bo.Operator))
            {
                int r = AllocReg();
                Emit($"    CMP R{a}, R{b}");
                string lt = Label("cmpt"), ld = Label("cmpd");
                EmitCondJump(bo.Operator, lt, true);
                Emit($"    LI R{r}, -1");
                JmpTo(ld);
                Emit($"{lt}:");
                Emit($"    LI R{r}, 1");
                Emit($"{ld}:");
                return r;
            }
            int r2 = AllocReg();
            string op = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => "ADD" };
            Emit($"    {op} R{r2}, R{a}, R{b}");
            return r2;
        }

        int GenUnary(UnaryOp uo)
        {
            int o = GenExpr(uo.Operand), r = AllocReg();
            Emit($"    {(uo.Operator == "-" ? "NEG" : "MOV")} R{r}, R{o}");
            return r;
        }

        int EmitAssign(Assignment ass)
        {
            int v = GenExpr(ass.Value);
            if (ass.Target is Identifier id) StoreLocal(id.Name, v);
            return v;
        }

        // === Cond jump via register (T3 JE/JL/JG operate on registers) ===
        void EmitCondJump(string op, string label, bool trueCase)
        {
            int r = AllocReg();
            Emit($"    LI R{r}, {label}");
            string j = op switch { "==" => "JE", "!=" => "JNE", "<" => "JL", ">" => "JG", "<=" => "JLE", ">=" => "JGE", _ => "JE" };
            if (!trueCase) j = InvertCond(j);
            // T3 only has JE/JNE/JL/JG. LE/GE need two-jump pattern.
            if (j == "JLE") { Emit($"    JG R{r}"); Emit($"    LI R{r}, {label}"); Emit($"    JE R{r}"); }
            else if (j == "JGE") { Emit($"    JL R{r}"); Emit($"    LI R{r}, {label}"); Emit($"    JE R{r}"); }
            else Emit($"    {j} R{r}");
        }

        void Jump(string cond, string label)
        {
            int r = AllocReg();
            Emit($"    LI R{r}, {label}");
            Emit($"    {cond} R{r}");
        }

        void JmpTo(string label) { int r = AllocReg(); Emit($"    LI R{r}, {label}"); Emit($"    JMP R{r}"); }

        static string InvertCond(string j) => j switch { "JE" => "JNE", "JNE" => "JE", "JL" => "JGE", "JG" => "JLE", "JLE" => "JG", "JGE" => "JL", _ => "JE" };

        static bool IsCompare(string op) => op is "==" or "!=" or "<" or ">" or "<=" or ">=";

        // === Variables ===
        int _nextVarAddr = 150;  // far from program code area (0-50)
        const int AddrReg = 4;  // R4 dedicated for address calculations
        void AllocLocal(string name) { if (!_varSlots.ContainsKey(name)) _varSlots[name] = _nextVarAddr++; }
        int LoadLocal(string name)
        {
            int r = AllocReg();
            if (_varSlots.TryGetValue(name, out int a))
            {
                Emit($"    LI R{AddrReg}, {a}");
                Emit($"    LOAD R{r}, R{AddrReg}");
            }
            else Emit($"    LI R{r}, 0");
            return r;
        }
        void StoreLocal(string name, int reg)
        {
            if (_varSlots.TryGetValue(name, out int a))
            {
                Emit($"    LI R{AddrReg}, {a}");
                Emit($"    STORE R{reg}, R{AddrReg}");
            }
        }

        // === Literals ===
        long ParseInt(string v)
        {
            if (v.StartsWith("0t")) return BalancedTernary.ParseToLong(v.Substring(2).Replace("_", ""));
            if (v.StartsWith("0y")) return Parse27(v.Substring(2));
            if (v.StartsWith("0n")) return Parse9(v.Substring(2));
            return long.TryParse(v, out long n) ? n : 0;
        }
        long Parse27(string s) { char[] a = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray(); string t = ""; foreach (char c in s.ToUpper()) { int i = Array.IndexOf(a, c); if (i >= 0) t += TritChar(i / 9 - 1) + TritChar(i / 3 % 3 - 1) + TritChar(i % 3 - 1); } return BalancedTernary.ParseToLong(t); }
        long Parse9(string s) { string t = ""; foreach (char c in s.ToUpper()) t += c switch { 'W' => "--", 'X' => "-0", 'Y' => "-+", 'Z' => "0-", '0' => "00", '1' => "0+", '2' => "+-", '3' => "+0", '4' => "++", _ => "00" }; return BalancedTernary.ParseToLong(t); }
        static string TritChar(int t) => t == -1 ? "-" : (t == 1 ? "+" : "0");

        // === Reg alloc ===
        int _nextReg = 0;
        int AllocReg()
        {
            // Skip R4 (address register). Registers: R0, R1, R2, R3, R5.
            int r = _nextReg switch { 4 => 5, _ => _nextReg };
            _nextReg = (_nextReg + 1) % 6;
            if (_nextReg == 4) _nextReg = 5;  // skip R4
            return r;
        }
        int EmitImm(long val) { int r = AllocReg(); if (val >= -364 && val <= 364) Emit($"    LI R{r}, {val}"); else Emit($"    LIMM R{r}, {val}"); return r; }

        // === Helpers ===
        string Label(string prefix) => $"{prefix}_{_labelCounter++}";
        void Emit(string s = "") => _output.AppendLine(s);
    }
}