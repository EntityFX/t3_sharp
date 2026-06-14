using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<string, List<int>> _arrDims;
        private readonly Dictionary<string, List<Parser.FieldDef>> _structFields; // varName → fields list

        public CodeGenerator(AstProgram program)
        {
            _program = program;
            _output = new StringBuilder();
            _labelCounter = 0;
            _varSlots = new Dictionary<string, int>();
            _arrDims = new Dictionary<string, List<int>>();
            _structFields = new Dictionary<string, List<Parser.FieldDef>>();

            // Register struct types from program
            foreach (var sd in _program.Structs)
            {
                if (!_structDefs.ContainsKey(sd.Name))
                    _structDefs[sd.Name] = sd.Fields;
            }
        }

        private readonly Dictionary<string, List<Parser.FieldDef>> _structDefs = new();

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
            _arrDims.Clear();
            _structFields.Clear();
            Emit($"{func.Name}:");
            foreach (var s in func.Body.Body) GenStmt(s);
            Emit("    RET");
            Emit();
        }

        void GenStmt(Statement s)
        {
            switch (s)
            {
                case ExpressionStmt es: if (es.Expression != null) GenExpr(es.Expression); break;
                case VarDeclaration vd:
                    AllocLocal(vd.Name, vd.Type);
                    if (vd.Type.StructName != null && _structDefs.TryGetValue(vd.Type.StructName, out var fields))
                        _structFields[vd.Name] = fields;
                    if (vd.Initializer != null) { int r = GenExpr(vd.Initializer); StoreLocal(vd.Name, r, 0); }
                    break;
                case ReturnStmt rs: if (rs.Value != null) { int r = GenExpr(rs.Value); Emit($"    MOV R2, R{r}"); } Emit("    RET"); break;
                case CompoundStmt cs: foreach (var ss in cs.Body) GenStmt(ss); break;
                case IfStmt ifs: GenIf(ifs); break;
                case WhileStmt ws: GenWhile(ws); break;
                case ForStmt fs:
                    if (fs.Init != null) GenExpr(fs.Init);
                    if (fs.Condition != null) GenWhile(new WhileStmt { Condition = fs.Condition, Body = Compound(fs.Body, fs.Step) });
                    else GenStmt(fs.Body);
                    break;
            }
        }

        CompoundStmt Compound(Statement body, AstNode? step)
        {
            var list = new List<Statement> { body };
            if (step != null) list.Add(new ExpressionStmt { Expression = step });
            return new CompoundStmt { Body = list };
        }

        void GenIf(IfStmt s)
        {
            string lEnd = Label("end");
            if (s.Condition is BinaryOp bo)
            {
                int a = GenExpr(bo.Left), b = GenExpr(bo.Right);
                string lThen = Label("ift");
                Emit($"    CMP R{a}, R{b}");
                EmitCondJump(bo.Operator, lThen);
                if (s.ElseBody != null) GenStmt(s.ElseBody);
                JmpTo(lEnd);
                Emit($"{lThen}:"); GenStmt(s.ThenBody); JmpTo(lEnd);
            }
            else
            {
                int c = GenExpr(s.Condition);
                string lThen = Label("ift");
                Emit($"    LI R1, 0"); Emit($"    CMP R{c}, R1"); JumpReg("JG", lThen);
                if (s.ElseBody != null) GenStmt(s.ElseBody);
                JmpTo(lEnd); Emit($"{lThen}:"); GenStmt(s.ThenBody); JmpTo(lEnd);
            }
            Emit($"{lEnd}:");
        }

        void GenWhile(WhileStmt s)
        {
            string lLoop = Label("loop"), lBody = Label("body"), lEnd = Label("wend");
            Emit($"{lLoop}:");
            if (s.Condition is BinaryOp bo)
            {
                int a = GenExpr(bo.Left), b = GenExpr(bo.Right);
                Emit($"    CMP R{a}, R{b}"); EmitCondJump(bo.Operator, lBody); JmpTo(lEnd);
            }
            else
            {
                int c = GenExpr(s.Condition);
                Emit($"    LI R1, 0"); Emit($"    CMP R{c}, R1"); JumpReg("JG", lBody); JmpTo(lEnd);
            }
            Emit($"{lBody}:"); GenStmt(s.Body); JmpTo(lLoop); Emit($"{lEnd}:");
        }

        void EmitCondJump(string op, string label)
        {
            switch (op)
            {
                case "==": JumpReg("JE", label); break;
                case "!=": JumpReg("JNE", label); break;
                case "<": JumpReg("JL", label); break;
                case ">": JumpReg("JG", label); break;
                case "<=": { var s = Label("skp"); JumpReg("JG", s); JmpTo(label); Emit($"{s}:"); } break;
                case ">=": { var s = Label("skp"); JumpReg("JL", s); JmpTo(label); Emit($"{s}:"); } break;
            }
        }

        void JumpReg(string cond, string label) { int r = AllocReg(); Emit($"    LI R{r}, {label}"); Emit($"    {cond} R{r}"); }
        void JmpTo(string label) { int r = AllocReg(); Emit($"    LI R{r}, {label}"); Emit($"    JMP R{r}"); }

        // === Expressions ===
        int GenExpr(AstNode n) => n switch
        {
            IntegerLiteral il => EmitImm(ParseInt(il.Value)),
            Identifier id => LoadLocal(id.Name, 0),
            BooleanLiteral bl => EmitImm(bl.Value),
            BinaryOp bo => GenBinOp(bo),
            UnaryOp uo => GenUnary(uo),
            Assignment ass => EmitAssign(ass),
            ArrayAccess aa => EmitArrayAccess(aa),
            FunctionCall fc => EmitFuncCall(fc),
            MemberAccess ma => EmitMemberAccess(ma),
            _ => EmitImm(0)
        };

        int EmitMemberAccess(MemberAccess ma)
        {
            if (ma.Object is Identifier id && _varSlots.TryGetValue(id.Name, out int baseAddr) && _structFields.TryGetValue(id.Name, out var fields))
            {
                int offset = fields.FindIndex(f => f.Name == ma.MemberName);
                if (offset < 0) return EmitImm(0);
                int r = AllocReg();
                Emit($"    LI R{AddrReg}, {baseAddr + offset}");
                Emit($"    LOAD R{r}, R{AddrReg}");
                return r;
            }
            return EmitImm(0);
        }

        int GenUnary(UnaryOp uo)
        {
            if (uo.Operator == "&")
            {
                if (uo.Operand is Identifier id && _varSlots.TryGetValue(id.Name, out int a)) return EmitImm(a);
                return EmitImm(0);
            }
            if (uo.Operator == "*")
            {
                int ptrR = GenExpr(uo.Operand); int r = AllocReg();
                Emit($"    LOAD R{r}, R{ptrR}"); return r;
            }
            int o = GenExpr(uo.Operand); int r2 = AllocReg();
            Emit($"    {(uo.Operator == "-" ? "NEG" : "MOV")} R{r2}, R{o}");
            return r2;
        }

        int GenBinOp(BinaryOp bo)
        {
            int a = GenExpr(bo.Left), b = GenExpr(bo.Right);
            if (IsCompare(bo.Operator))
            {
                int r = AllocReg(); Emit($"    CMP R{a}, R{b}");
                string lt = Label("cmpt"), ld = Label("cmpd"); EmitCondJump(bo.Operator, lt);
                Emit($"    LI R{r}, -1"); JmpTo(ld); Emit($"{lt}:"); Emit($"    LI R{r}, 1"); Emit($"{ld}:");
                return r;
            }
            int r2 = AllocReg();
            string op = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => "ADD" };
            Emit($"    {op} R{r2}, R{a}, R{b}");
            return r2;
        }

        int EmitAssign(Assignment ass)
        {
            int v = GenExpr(ass.Value);
            if (ass.Target is Identifier id) StoreLocal(id.Name, v, 0);
            else if (ass.Target is ArrayAccess aa) EmitArrayStore(aa, v);
            return v;
        }

        int EmitFuncCall(FunctionCall fc)
        {
            for (int i = fc.Arguments.Count - 1; i >= 0; i--) { int aR = GenExpr(fc.Arguments[i]); Emit($"    PUSH R{aR}"); }
            Emit($"    LI R1, {fc.FunctionName}"); Emit($"    CALL R1");
            for (int i = 0; i < fc.Arguments.Count; i++) Emit($"    POP R0");
            int r = AllocReg(); Emit($"    MOV R{r}, R2"); return r;
        }

        int EmitArrayAccess(ArrayAccess aa)
        {
            if (!_varSlots.TryGetValue(aa.ArrayName, out int baseAddr)) baseAddr = _nextVarAddr;
            int r = AllocReg();
            if (_arrDims.TryGetValue(aa.ArrayName, out var dims) && dims.Count > 1)
            {
                int iR = GenExpr(aa.Indices[0]), jR = GenExpr(aa.Indices[1]);
                int strideR = AllocReg(); Emit($"    LI R{strideR}, {dims[1]}");
                int tmp = AllocReg(); Emit($"    MUL R{tmp}, R{iR}, R{strideR}");
                Emit($"    ADD R{r}, R{tmp}, R{jR}");
            }
            else r = GenExpr(aa.Indices[0]);
            Emit($"    LI R{AddrReg}, {baseAddr}"); Emit($"    ADD R{AddrReg}, R{AddrReg}, R{r}");
            int res = AllocReg(); Emit($"    LOAD R{res}, R{AddrReg}");
            return res;
        }

        void EmitArrayStore(ArrayAccess aa, int valReg)
        {
            if (!_varSlots.TryGetValue(aa.ArrayName, out int baseAddr)) baseAddr = _nextVarAddr;
            int offR;
            if (_arrDims.TryGetValue(aa.ArrayName, out var dims) && dims.Count > 1)
            {
                int iR = GenExpr(aa.Indices[0]), jR = GenExpr(aa.Indices[1]);
                int sR = AllocReg(); Emit($"    LI R{sR}, {dims[1]}");
                int t = AllocReg(); Emit($"    MUL R{t}, R{iR}, R{sR}");
                offR = AllocReg(); Emit($"    ADD R{offR}, R{t}, R{jR}");
            }
            else offR = GenExpr(aa.Indices[0]);
            Emit($"    LI R{AddrReg}, {baseAddr}"); Emit($"    ADD R{AddrReg}, R{AddrReg}, R{offR}");
            Emit($"    STORE R{valReg}, R{AddrReg}");
        }

        // === Variables ===
        int _nextVarAddr = 150;
        const int AddrReg = 4;

        void AllocLocal(string name, TypeSpec ts)
        {
            if (!_varSlots.ContainsKey(name))
            {
                _varSlots[name] = _nextVarAddr;
                int size = ts.Dims.Count == 0 ? 1 : ts.Dims.Aggregate(1, (a, b) => a * b);
                _nextVarAddr += size;
                if (ts.Dims.Count > 0) _arrDims[name] = ts.Dims;
            }
        }

        int LoadLocal(string name, int index)
        {
            int r = AllocReg();
            if (_varSlots.TryGetValue(name, out int a))
            {
                Emit($"    LI R{AddrReg}, {a + index}");
                Emit($"    LOAD R{r}, R{AddrReg}");
            }
            else Emit($"    LI R{r}, 0");
            return r;
        }

        void StoreLocal(string name, int reg, int index)
        {
            if (_varSlots.TryGetValue(name, out int a))
            {
                Emit($"    LI R{AddrReg}, {a + index}");
                Emit($"    STORE R{reg}, R{AddrReg}");
            }
        }

        // === Literals ===
        static bool IsCompare(string op) => op is "==" or "!=" or "<" or ">" or "<=" or ">=";
        long ParseInt(string v)
        {
            if (v.StartsWith("0t")) return BalancedTernary.ParseToLong(v.Substring(2).Replace("_",""));
            if (v.StartsWith("0y")) return P27(v.Substring(2));
            if (v.StartsWith("0n")) return P9(v.Substring(2));
            return long.TryParse(v, out long n) ? n : 0;
        }
        long P27(string s) { char[] a = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray(); string t = ""; foreach (char c in s.ToUpper()) { int i = Array.IndexOf(a,c); if (i>=0) t += TCh(i/9-1)+TCh(i/3%3-1)+TCh(i%3-1); } return BalancedTernary.ParseToLong(t); }
        long P9(string s) { string t = ""; foreach (char c in s.ToUpper()) t += c switch {'W'=>"--",'X'=>"-0",'Y'=>"-+",'Z'=>"0-",'0'=>"00",'1'=>"0+",'2'=>"+-",'3'=>"+0",'4'=>"++",_=>"00"}; return BalancedTernary.ParseToLong(t); }
        static string TCh(int t) => t==-1?"-":(t==1?"+":"0");

        int _nextReg = 0;
        int AllocReg() { int r = _nextReg switch { 4 => 5, _ => _nextReg }; _nextReg = (_nextReg+1)%6; if (_nextReg==4) _nextReg=5; return r; }
        int EmitImm(long val) { int r = AllocReg(); if (val>=-364&&val<=364) Emit($"    LI R{r}, {val}"); else Emit($"    LIMM R{r}, {val}"); return r; }
        string Label(string prefix) => $"{prefix}_{_labelCounter++}";
        void Emit(string s = "") => _output.AppendLine(s);
    }
}