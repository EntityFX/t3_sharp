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
        private readonly Dictionary<string, int> _varSizes;
        private readonly Dictionary<string, List<int>> _arrDims;
        private readonly Dictionary<string, List<FieldDef>> _structFields;
        private readonly Stack<(string brk, string cont)> _loopStack = new();
        private readonly Dictionary<string, List<FieldDef>> _structDefs = new();

        public CodeGenerator(AstProgram program)
        {
            _program = program;
            _output = new();
            _varSlots = new(); _varSizes = new(); _arrDims = new(); _structFields = new();
            foreach (var sd in _program.Structs)
                if (!_structDefs.ContainsKey(sd.Name)) _structDefs[sd.Name] = sd.Fields;
        }

        public string Generate()
        {
            Emit("; T‑lang → T3 assembly");
            Emit("; ===================");
            Emit(); Emit("__entry:"); Emit("    LI R0, main"); Emit("    CALL R0"); Emit("    HALT"); Emit();
            foreach (var f in _program.Functions) GenFunc(f);
            return _output.ToString();
        }

        void GenFunc(FunctionDef f)
        {
            _varSlots.Clear(); _varSizes.Clear(); _arrDims.Clear(); _structFields.Clear();
            _nextReg = 0; _nextAddr = 10;
            Emit($"{f.Name}:");
            foreach (var s in f.Body.Body) GenStmt(s);
            Emit("    RET"); Emit();
        }

        // ═══ STMTS ═══
        void GenStmt(Statement s)
        {
            switch (s)
            {
                case ExpressionStmt es: if (es.Expression != null) GenExpr(es.Expression); break;
                case VarDeclaration vd:
                    Alloc(vd.Name, vd.Type);
                    if (vd.Type.StructName != null && _structDefs.TryGetValue(vd.Type.StructName, out var sf)) _structFields[vd.Name] = sf;
                    if (vd.Initializer != null) { int r = GenExpr(vd.Initializer); Store(vd.Name, r, 0); }
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
                case BreakStmt: if (_loopStack.TryPeek(out var L)) Jmp(L.brk); break;
                case ContinueStmt: if (_loopStack.TryPeek(out var C)) Jmp(C.cont); break;
            }
        }
        static CompoundStmt Compound(Statement b, AstNode? s) { var l = new List<Statement> { b }; if (s != null) l.Add(new ExpressionStmt { Expression = s }); return new CompoundStmt { Body = l }; }

        // ═══ IF / WHILE ═══
        void GenIf(IfStmt s)
        {
            string lEnd = Lbl("end"), lThen = Lbl("then");
            if (s.Condition is BinaryOp bo) { int a = GenExpr(bo.Left), b = GenExpr(bo.Right); Emit($"    CMP R{a}, R{b}"); JumpCond(bo.Operator, lThen); if (s.ElseBody != null) GenStmt(s.ElseBody); Jmp(lEnd); Emit($"{lThen}:"); GenStmt(s.ThenBody); Jmp(lEnd); }
            else { int c = GenExpr(s.Condition); Emit($"    LI R1, 0"); Emit($"    CMP R{c}, R1"); JumpReg("JG", lThen); if (s.ElseBody != null) GenStmt(s.ElseBody); Jmp(lEnd); Emit($"{lThen}:"); GenStmt(s.ThenBody); Jmp(lEnd); }
            Emit($"{lEnd}:");
        }
        void GenWhile(WhileStmt s)
        {
            string lLoop = Lbl("loop"), lBody = Lbl("body"), lEnd = Lbl("wend");
            _loopStack.Push((lEnd, lLoop)); Emit($"{lLoop}:");
            if (s.Condition is BinaryOp bo) { int a = GenExpr(bo.Left), b = GenExpr(bo.Right); Emit($"    CMP R{a}, R{b}"); JumpCond(bo.Operator, lBody); Jmp(lEnd); }
            else { int c = GenExpr(s.Condition); Emit($"    LI R1, 0"); Emit($"    CMP R{c}, R1"); JumpReg("JG", lBody); Jmp(lEnd); }
            Emit($"{lBody}:"); GenStmt(s.Body); Jmp(lLoop); Emit($"{lEnd}:"); _loopStack.Pop();
        }

        void JumpCond(string op, string label) { switch (op) { case "==": JumpReg("JE", label); break; case "!=": JumpReg("JNE", label); break; case "<": JumpReg("JL", label); break; case ">": JumpReg("JG", label); break; case "<=": JumpReg("JLE", label); break; case ">=": JumpReg("JGE", label); break; } }
        void JumpReg(string cond, string label) { int r = AllocR(); Emit($"    LIMM R{r}, {label}"); Emit($"    {cond} R{r}"); }
        void Jmp(string label) { int r = AllocR(); Emit($"    LIMM R{r}, {label}"); Emit($"    JMP R{r}"); }

        // ═══ EXPRESSIONS ═══
        int GenExpr(AstNode n) => n switch
        {
            IntegerLiteral il => Imm(ParseInt(il.Value)), Identifier id => LoadV(id.Name, 0), BooleanLiteral bl => Imm(bl.Value),
            BinaryOp bo => GenBin(bo), UnaryOp uo => GenUn(uo), Assignment ass => EmitAssign(ass),
            ArrayAccess aa => EmitArrAccess(aa), FunctionCall fc => EmitCall(fc), MemberAccess ma => EmitMemAccess(ma),
            TernaryExpr te => GenTernary(te), _ => Imm(0)
        };
        int GenTernary(TernaryExpr te) { int cr = GenExpr(te.Condition), r = AllocR(); Emit($"    LI R1, 0"); Emit($"    CMP R{cr}, R1"); string lt = Lbl("t"), lm = Lbl("m"), ld = Lbl("d"); JumpReg("JG", lt); JumpReg("JE", lm); int fR = GenExpr(te.FalseExpr); Emit($"    MOV R{r}, R{fR}"); Jmp(ld); Emit($"{lm}:"); int mR = GenExpr(te.MaybeExpr); Emit($"    MOV R{r}, R{mR}"); Jmp(ld); Emit($"{lt}:"); int tR = GenExpr(te.TrueExpr); Emit($"    MOV R{r}, R{tR}"); Emit($"{ld}:"); return r; }
        int EmitMemAccess(MemberAccess ma) { if (ma.Object is Identifier id && _varSlots.TryGetValue(id.Name, out int ba) && _structFields.TryGetValue(id.Name, out var fl)) { int off = fl.FindIndex(f => f.Name == ma.MemberName); if (off < 0) return Imm(0); int r = AllocR(); EmitAddr(ba + off); Emit($"    LOAD R{r}, R{AddrReg}"); return r; } return Imm(0); }
        void EmitMemStore(MemberAccess ma, int v) { if (ma.Object is Identifier id && _varSlots.TryGetValue(id.Name, out int ba) && _structFields.TryGetValue(id.Name, out var fl)) { int off = fl.FindIndex(f => f.Name == ma.MemberName); if (off >= 0) { EmitAddr(ba + off); Emit($"    STORE R{v}, R{AddrReg}"); } } }
        int GenUn(UnaryOp uo) { if (uo.Operator == "&") { if (uo.Operand is Identifier id && _varSlots.TryGetValue(id.Name, out int a)) return Imm(a); if (uo.Operand is MemberAccess ma && ma.Object is Identifier id2 && _varSlots.TryGetValue(id2.Name, out int ba2) && _structFields.TryGetValue(id2.Name, out var fl2)) { int off = fl2.FindIndex(f => f.Name == ma.MemberName); if (off >= 0) return Imm(ba2 + off); } if (uo.Operand is ArrayAccess aa) { int arrB = _varSlots.TryGetValue(aa.ArrayName, out int b) ? b : _nextAddr; int idx = FlatIdx(aa); int ra = AllocR(); Emit($"    LIMM R{ra}, {arrB}"); Emit($"    ADD R{ra}, R{ra}, R{idx}"); return ra; } return Imm(0); } if (uo.Operator == "*") { int pr = GenExpr(uo.Operand), r = AllocR(); Emit($"    LOAD R{r}, R{pr}"); return r; } int o = GenExpr(uo.Operand), r2 = AllocR(); Emit($"    {(uo.Operator == "-" ? "NEG" : "MOV")} R{r2}, R{o}"); return r2; }
        int GenBin(BinaryOp bo) { int a = GenExpr(bo.Left), b = GenExpr(bo.Right), r = AllocR(); if (IsCmp(bo.Operator)) { Emit($"    CMP R{a}, R{b}"); string lt = Lbl("t"), ld = Lbl("d"); JumpCond(bo.Operator, lt); Emit($"    LI R{r}, -1"); Jmp(ld); Emit($"{lt}:"); Emit($"    LI R{r}, 1"); Emit($"{ld}:"); return r; } string op = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => "ADD" }; Emit($"    {op} R{r}, R{a}, R{b}"); return r; }
        int EmitAssign(Assignment ass) { int v; if (ass.Operator == "=") v = GenExpr(ass.Value); else { int lh = GenExpr(ass.Target), rh = GenExpr(ass.Value); string op = ass.Operator switch { "+=" => "ADD", "-=" => "SUB", "*=" => "MUL", "/=" => "DIV", "%=" => "MOD", "&=" => "AND", "|=" => "OR", "^=" => "XOR", "<<=" => "SHL", ">>=" => "SHR", _ => "ADD" }; v = AllocR(); Emit($"    {op} R{v}, R{lh}, R{rh}"); } if (ass.Target is Identifier id) Store(id.Name, v, 0); else if (ass.Target is ArrayAccess aa) EmitArrStore(aa, v); else if (ass.Target is MemberAccess ma) EmitMemStore(ma, v); return v; }
        int EmitCall(FunctionCall fc) { for (int i = fc.Arguments.Count - 1; i >= 0; i--) Emit($"    PUSH R{GenExpr(fc.Arguments[i])}"); Emit($"    LI R1, {fc.FunctionName}"); Emit($"    CALL R1"); for (int i = 0; i < fc.Arguments.Count; i++) Emit($"    POP R0"); int r = AllocR(); Emit($"    MOV R{r}, R2"); return r; }

        int FlatIdx(ArrayAccess aa) { if (_arrDims.TryGetValue(aa.ArrayName, out var dims) && dims.Count > 1 && aa.Indices.Count >= 2) { int iR = GenExpr(aa.Indices[0]), jR = GenExpr(aa.Indices[1]); int sR = AllocR(); Emit($"    LI R{sR}, {dims[1]}"); int t = AllocR(); Emit($"    MUL R{t}, R{iR}, R{sR}"); int r = AllocR(); Emit($"    ADD R{r}, R{t}, R{jR}"); return r; } return GenExpr(aa.Indices[0]); }
        int EmitArrAccess(ArrayAccess aa) { int ba = _varSlots.TryGetValue(aa.ArrayName, out int b) ? b : _nextAddr; int off = FlatIdx(aa); EmitAddr(ba); Emit($"    ADD R{AddrReg}, R{AddrReg}, R{off}"); int r = AllocR(); Emit($"    LOAD R{r}, R{AddrReg}"); return r; }
        void EmitArrStore(ArrayAccess aa, int v) { int ba = _varSlots.TryGetValue(aa.ArrayName, out int b) ? b : _nextAddr; int off = FlatIdx(aa); EmitAddr(ba); Emit($"    ADD R{AddrReg}, R{AddrReg}, R{off}"); Emit($"    STORE R{v}, R{AddrReg}"); }

        // ═══ VARIABLES ═══
        int _nextAddr = 10; const int AddrReg = 4;
        void EmitAddr(long addr) { if (addr >= -364 && addr <= 364) Emit($"    LI R{AddrReg}, {addr}"); else Emit($"    LIMM R{AddrReg}, {addr}"); }
        void Alloc(string name, TypeSpec ts) { if (!_varSlots.ContainsKey(name)) { _varSlots[name] = _nextAddr; int sz = 1; if (ts.StructName != null && _structDefs.TryGetValue(ts.StructName, out var sf)) { sz = sf.Count; _structFields[name] = sf; } else if (ts.Dims.Count > 0) { sz = ts.Dims.Aggregate(1, (a, b) => a * b); _arrDims[name] = ts.Dims; } _varSizes[name] = sz; _nextAddr += sz; } }
        int LoadV(string name, int idx) { int r = AllocR(); if (_varSlots.TryGetValue(name, out int a)) { EmitAddr(a + idx); Emit($"    LOAD R{r}, R{AddrReg}"); } else Emit($"    LI R{r}, 0"); return r; }
        void Store(string name, int reg, int idx) { if (_varSlots.TryGetValue(name, out int a)) { EmitAddr(a + idx); Emit($"    STORE R{reg}, R{AddrReg}"); } }

        static bool IsCmp(string op) => op is "==" or "!=" or "<" or ">" or "<=" or ">=";
        long ParseInt(string v) { if (v.StartsWith("0t")) return BalancedTernary.ParseToLong(v[2..].Replace("_", "")); if (v.StartsWith("0y")) return P27(v[2..]); if (v.StartsWith("0n")) return P9(v[2..]); return long.TryParse(v, out long n) ? n : 0; }
        long P27(string s) { var a = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray(); string t = ""; foreach (char c in s.ToUpper()) { int i = Array.IndexOf(a, c); if (i >= 0) t += TCh(i / 9 - 1) + TCh(i / 3 % 3 - 1) + TCh(i % 3 - 1); } return BalancedTernary.ParseToLong(t); }
        long P9(string s) { string t = ""; foreach (char c in s.ToUpper()) t += c switch { 'W' => "--", 'X' => "-0", 'Y' => "-+", 'Z' => "0-", '0' => "00", '1' => "0+", '2' => "+-", '3' => "+0", '4' => "++", _ => "00" }; return BalancedTernary.ParseToLong(t); }
        static string TCh(int t) => t == -1 ? "-" : t == 1 ? "+" : "0";
        int _nextReg; int AllocR() { while (_nextReg == 2 || _nextReg == 4 || _nextReg > 8) _nextReg = (_nextReg + 1) % 9; int r = _nextReg; _nextReg = (_nextReg + 1) % 9; return r; }
        int Imm(long v) { int r = AllocR(); if (v >= -364 && v <= 364) Emit($"    LI R{r}, {v}"); else Emit($"    LIMM R{r}, {v}"); return r; }
        string Lbl(string pfx) => $"{pfx}_{_labelCounter++}"; void Emit(string s = "") => _output.AppendLine(s);
    }
}