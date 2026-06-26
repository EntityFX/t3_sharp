using System;using System.Collections.Generic;using System.Linq;using System.Text;using T3Compiler.Parser;using T3Simulator.Common;using TritTypes;
namespace T3Compiler.CodeGen
{
    public class CodeGenerator
    {
        readonly AstProgram _program;readonly StringBuilder _output;readonly StringBuilder _codeOutput;
        int _labelCounter;
        readonly Dictionary<string,int> _varSlots=new(),_varSizes=new();
        readonly Dictionary<string,string> _globalLabels=new();
        readonly Dictionary<string,int> _enumConstants=new();
        readonly List<(string label, string value)> _stringsToEmit=new();
        readonly List<(string label, Word18 value)> _floatsToEmit=new();
        readonly List<(string label, long value)> _globalsToEmit=new();
        readonly Dictionary<string,List<int>> _arrDims=new();readonly Dictionary<string,List<FieldDef>> _structFields=new();
        readonly Stack<(string brk,string cont)> _loopStack=new();readonly Dictionary<string,List<FieldDef>> _structDefs=new();
        string? _epilogueLabel;
        int _nextAddr = 2000;   // allocated well past any possible code
        int _globalSlotAddr = 2000;
        readonly HashSet<string> _liveVars = new();
        bool _fpuLive = false;
        readonly Dictionary<string,int> _globalSlots = new();

        // Working regs: 0(RW),1(RX),2(RY),4(R0),3(RZ),7(R3)
        // R4(8) = AddrReg, R1(5) = call temp, R2(6) = retval

        public CodeGenerator(AstProgram p){
            _program=p;_output=new();_codeOutput=new();
            foreach(var s in p.Structs)_structDefs[s.Name]=s.Fields;
        }
        public string Generate(){
            foreach(var ed in _program.Enums){
                int cur=0;
                foreach(var m in ed.Members){
                    int v=m.Value??cur;
                    _enumConstants[m.Name]=v;
                    cur=v+1;
                }
            }
            // Allocate global slots BEFORE codegen (use incremental indexing from 0)
            int gSlotIdx = 0;
            foreach(var g in _program.Globals){
                int sz = 1;
                if(g.Type.Dims.Count>0) sz = g.Type.Dims.Aggregate(1,(a,b)=>a*b);
                _globalSlots[g.Name] = gSlotIdx;
                gSlotIdx += sz;
            }
            
            // Generate all code
            EmitCode("; T→T3");EmitCode("__entry:");EmitCode("    LI RW,main");EmitCode("    CALL RW");EmitCode("    HALT");
            foreach(var f in _program.Functions)GenFunc(f);
            
            // Compute code size
            string codeText = _codeOutput.ToString();
            int codeWords = 0;
            foreach(var line in codeText.Split(new[]{"\r\n","\r","\n"},StringSplitOptions.None)){
                string t = line.Trim();
                if(string.IsNullOrEmpty(t)||t.StartsWith(";"))continue;
                codeWords++;
            }
            // Emit code
            _output.Append(codeText);
            
            // Update global slots to absolute addresses (codeWords + 100 + index)
            int baseAddr = codeWords + 100;
            _globalSlotAddr = baseAddr;
            _nextAddr = baseAddr;
            var globalsUpdated = new Dictionary<string,int>();
            foreach(var kv in _globalSlots){
                globalsUpdated[kv.Key] = baseAddr + kv.Value;
                int sz = 1;
                var g = _program.Globals.FirstOrDefault(x=>x.Name==kv.Key);
                if(g!=null && g.Type.Dims.Count>0) sz = g.Type.Dims.Aggregate(1,(a,b)=>a*b);
                baseAddr += sz;
            }
            _globalSlots.Clear();
            foreach(var kv in globalsUpdated) _globalSlots[kv.Key] = kv.Value;
            _globalSlotAddr = baseAddr;
            _nextAddr = baseAddr;
            
            // Emit global variables as .word labels AFTER code
            Emit("\n; --- Global Variables ---");
            foreach(var g in _program.Globals){
                int sz = 1;
                if(g.Type.Dims.Count>0) sz = g.Type.Dims.Aggregate(1,(a,b)=>a*b);
                var vals = new List<long>(new long[sz]);
                Emit($"{g.Name}: .word {string.Join(", ", vals)}");
            }
            
            Emit("\n; --- Data Section ---");
            foreach(var (lbl, val) in _stringsToEmit){
                Emit($".string {lbl} \"{val}\"");
            }
            foreach(var (lbl, val) in _floatsToEmit){
                Emit($"{lbl}: .word {val.ToLong()}");
            }
            foreach(var (lbl, val) in _globalsToEmit){
                Emit($"{lbl}: .word {val}");
            }
            
            Emit("\n; --- StdLib ---");
            Emit("strlen:");
            Emit("    POP R1");
            Emit("    POP R0");
            Emit("    PUSH R1");
            Emit("    LI R2, 0");
            Emit("strlen_loop:");
            Emit("    LOAD R1, R0");
            Emit("    CMPI R1, 0");
            Emit("    JE strlen_end");
            Emit("    ADDI R2, 1");
            Emit("    ADDI R0, 1");
            Emit("    JMP strlen_loop");
            Emit("strlen_end:");
            Emit("    RET");
            
            return _output.ToString();}
        
        void GenFunc(FunctionDef f){
            _varSlots.Clear();_varSizes.Clear();_arrDims.Clear();_structFields.Clear();
            _nextReg = 0;
            _epilogueLabel = Lbl("epilogue");

            foreach(var param in f.Parameters) Alloc(param.Name, param.Type);

            EmitCode($"{f.Name}:");
            
            // Prologue: pop params, then save callee-saved
            EmitCode("    POP R2");   // save ret addr
            for (int i = 0; i < f.Parameters.Count; i++) {
                EmitCode("    POP R0");
                Store(f.Parameters[i].Name, 4, 0);
            }
            EmitCode("    PUSH R2");   // restore ret addr
            
            // Save callee-saved registers
            EmitCode("    PUSH RW");EmitCode("    PUSH RX");EmitCode("    PUSH RY");EmitCode("    PUSH RZ");
            EmitCode("    PUSH R0");EmitCode("    PUSH R3");EmitCode("    PUSH R1");EmitCode("    PUSH R4");
            
            foreach(var s in f.Body.Body)GenStmt(s);
            
            // Epilogue
            EmitCode($"{_epilogueLabel}:");
            EmitCode("    POP R4");EmitCode("    POP R1");EmitCode("    POP R3");EmitCode("    POP R0");
            EmitCode("    POP RZ");EmitCode("    POP RY");EmitCode("    POP RX");EmitCode("    POP RW");
            EmitCode("    RET");
        }
        
        void GenStmt(Statement s){switch(s){case ExpressionStmt e:if(e.Expression!=null)GenExpr(e.Expression);break;case VarDeclaration vd:Alloc(vd.Name,vd.Type);if(vd.Type.StructName!=null&&_structDefs.TryGetValue(vd.Type.StructName,out var sf))_structFields[vd.Name]=sf;if(vd.Initializer!=null){int r=GenExpr(vd.Initializer);Store(vd.Name,r,0);}break;case ReturnStmt rs:if(rs.Value!=null){int r=GenExpr(rs.Value);EmitCode($"    MOV R2,{RegName(r)}");}if(_epilogueLabel!=null){int rj=AllocR();EmitCode($"    LIMM {RegName(rj)},{_epilogueLabel}");EmitCode($"    JMP {RegName(rj)}");}else EmitCode("    RET");break;case CompoundStmt cs:foreach(var ss in cs.Body)GenStmt(ss);break;case IfStmt ifs:GenIf(ifs);break;case WhileStmt ws:GenWhile(ws);break;case DoWhileStmt dws:GenDoWhile(dws);break;case ForStmt fs:GenFor(fs);break;case SwitchStmt ss:GenSwitch(ss);break;case BreakStmt _:{var(brk,_)=_loopStack.Peek();Jmp(brk);}break;case ContinueStmt _:{var(_,cont)=_loopStack.Peek();Jmp(cont);}break;case GotoStmt gs:Jmp($"__glbl_{gs.Label}");break;case LabeledStmt ls:EmitCode($"__glbl_{ls.Label}:");GenStmt(ls.Body);break;}}
        
        void GenIf(IfStmt s){
            string le=Lbl("end"),lt=Lbl("then");
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                EmitCode($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,lt);
                // Binary comparisons produce +1/-1 only; MaybeBody is unreachable
                if(s.ElseBody!=null)GenStmt(s.ElseBody);
                Jmp(le);
                EmitCode($"{lt}:");GenStmt(s.ThenBody);
                EmitCode($"{le}:");
            }else{
                // Ternary condition: +1 → then, 0 → maybe, -1 → else
                int c=GenExpr(s.Condition);
                EmitCode($"    LI R2,0");
                EmitCode($"    CMP {RegName(c)},R2");
                JumpReg("JG",lt);
                if(s.MaybeBody!=null){
                    string lm=Lbl("maybe");
                    JumpReg("JE",lm);
                    if(s.ElseBody!=null)GenStmt(s.ElseBody);
                    Jmp(le);
                    EmitCode($"{lm}:");GenStmt(s.MaybeBody);
                    Jmp(le);
                    EmitCode($"{lt}:");GenStmt(s.ThenBody);
                }else{
                    if(s.ElseBody!=null)GenStmt(s.ElseBody);
                    Jmp(le);
                    EmitCode($"{lt}:");GenStmt(s.ThenBody);
                }
                EmitCode($"{le}:");
            }
        }
        
        void GenWhile(WhileStmt s){
            string ll=Lbl("loop"),lb=Lbl("body"),le=Lbl("wend");
            _loopStack.Push((le,ll));
            EmitCode($"{ll}:");
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                EmitCode($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,lb);
                Jmp(le);
            }else{
                int c=GenExpr(s.Condition);
                EmitCode($"    LI R2,0");
                EmitCode($"    CMP {RegName(c)},R2");
                JumpReg("JNE",lb);
                Jmp(le);
            }
            EmitCode($"{lb}:");GenStmt(s.Body);Jmp(ll);
            EmitCode($"{le}:");_loopStack.Pop();
        }
        
        void GenDoWhile(DoWhileStmt s){
            string ll=Lbl("loop"),le=Lbl("wend");
            _loopStack.Push((le,ll));
            EmitCode($"{ll}:");
            GenStmt(s.Body);
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                EmitCode($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,ll);
            }else{
                int c=GenExpr(s.Condition);
                EmitCode($"    LI R2,0");
                EmitCode($"    CMP {RegName(c)},R2");
                JumpReg("JNE",ll);
            }
            EmitCode($"{le}:");
            _loopStack.Pop();
        }
        
        void GenFor(ForStmt fs){
            string ll=Lbl("floop"),lb=Lbl("fbody"),le=Lbl("fend");
            _loopStack.Push((le,ll));
            if(fs.Init!=null) GenStmt(fs.Init);
            EmitCode($"{ll}:");
            if(fs.Condition!=null){
                if(fs.Condition is BinaryOp bo){
                    int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                    EmitCode($"    CMP {RegName(a)},{RegName(b)}");
                    JumpCond(bo.Operator,lb);
                }else{
                    int c=GenExpr(fs.Condition);
                    EmitCode($"    LI R2,0");
                    EmitCode($"    CMP {RegName(c)},R2");
                    JumpReg("JNE",lb);
                }
                Jmp(le);
            }
            EmitCode($"{lb}:");
            GenStmt(fs.Body);
            if(fs.Step!=null) GenExpr(fs.Step);
            Jmp(ll);
            EmitCode($"{le}:");
            _loopStack.Pop();
        }
        
        void GenSwitch(SwitchStmt s){
            int exprReg = GenExpr(s.Expression);
            string end = Lbl("swend");
            var labels = new List<string>();
            for (int i = 0; i < s.Cases.Count; i++)
                labels.Add(Lbl("scase"));
            int defaultIdx = -1;
            for (int i = 0; i < s.Cases.Count; i++) {
                if (s.Cases[i].Value == null) { defaultIdx = i; continue; }
                int caseVal = GenExpr(s.Cases[i].Value);
                EmitCode($"    CMP {RegName(exprReg)},{RegName(caseVal)}");
                JumpReg("JE", labels[i]);
            }
            if (defaultIdx >= 0)
                Jmp(labels[defaultIdx]);
            else
                Jmp(end);
            for (int i = 0; i < s.Cases.Count; i++) {
                EmitCode($"{labels[i]}:");
                foreach (var stmt in s.Cases[i].Body)
                    GenStmt(stmt);
                Jmp(end);
            }
            EmitCode($"{end}:");
        }
        
        void JumpCond(string op,string l){switch(op){case"==":JumpReg("JE",l);break;case"!=":JumpReg("JNE",l);break;case"<":JumpReg("JL",l);break;case">":JumpReg("JG",l);break;case"<=":JumpReg("JLE",l);break;case">=":JumpReg("JGE",l);break;}}
        void JumpReg(string cond,string l){int r=AllocR();EmitCode($"    LIMM {RegName(r)},{l}");EmitCode($"    {cond} {RegName(r)}");}
        void Jmp(string l){int r=AllocR();EmitCode($"    LIMM {RegName(r)},{l}");EmitCode($"    JMP {RegName(r)}");}
        
        int GenExpr(AstNode n){
        if(n is IntegerLiteral il) return Imm(ParseInt(il.Value));
        if(n is FloatLiteral fl) return EmitFloat(fl.Value);
        if(n is Identifier id){
                if(_enumConstants.TryGetValue(id.Name,out int v)) return Imm(v);
                return LoadV(id.Name,0);
            }
            if(n is BooleanLiteral bl) return Imm(bl.Value);
            if(n is StringLiteral sl) return EmitString(sl.Value);
            if(n is BinaryOp bo) return GenBin(bo);
            if(n is UnaryOp uo) return GenUn(uo);
            if(n is Assignment ass) return EmitAssign(ass);
            if(n is ArrayAccess aa) return EmitArrAccess(aa);
            if(n is FunctionCall fc) return EmitCall(fc);
            if(n is MemberAccess ma) return EmitMemAccess(ma);
            if(n is TernaryExpr te) return GenTernary(te);
            throw new NotSupportedException($"Unsupported expression: {n?.GetType().Name}");
        }
        
        int GenTernary(TernaryExpr te){
            int cr=GenExpr(te.Condition),r=AllocR();
            EmitCode($"    LI R2,0");
            EmitCode($"    CMP {RegName(cr)},R2");
            string lt=Lbl("t"),lm=Lbl("m"),ld=Lbl("d");
            JumpReg("JG",lt);JumpReg("JE",lm);
            int fR=GenExpr(te.FalseExpr);EmitCode($"    MOV {RegName(r)},{RegName(fR)}");Jmp(ld);
            EmitCode($"{lm}:");int mR=GenExpr(te.MaybeExpr);EmitCode($"    MOV {RegName(r)},{RegName(mR)}");Jmp(ld);
            EmitCode($"{lt}:");int tR=GenExpr(te.TrueExpr);EmitCode($"    MOV {RegName(r)},{RegName(tR)}");
            EmitCode($"{ld}:");return r;
        }
        
        int EmitMemAccess(MemberAccess ma){
            if(ma.Object is Identifier id&&_varSlots.TryGetValue(id.Name,out int ba)&&_structFields.TryGetValue(id.Name,out var fl)){
                int off=fl.FindIndex(f=>f.Name==ma.MemberName);
                if(off<0)throw new Exception($"Unknown field: {ma.MemberName}");
                int r=AllocR();EmitAddr(ba+off);EmitCode($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;
            }
            if(ma.Object is ArrayAccess aa&&_structFields.TryGetValue(aa.ArrayName,out var fl2)){
                int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                if(off<0)throw new Exception($"Unknown field: {ma.MemberName}");
                int ba2=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;
                int idx=FlatIdx(aa);
                int r=AllocR();EmitAddr(ba2+off);EmitCode($"    ADD {RegName(r)},{RegName(AddrReg)},{RegName(idx)}");EmitCode($"    LOAD {RegName(r)},{RegName(r)}");return r;
            }
            if(ma.Object is UnaryOp uo&&uo.Operator=="*"&&uo.Operand is Identifier ptrId){
                foreach(var kv in _structDefs){
                    int off=kv.Value.FindIndex(f=>f.Name==ma.MemberName);
                    if(off>=0){
                        int ptrR=GenExpr(uo.Operand);
                        int r=AllocR();
                        EmitCode($"    LI {RegName(AddrReg)},{off}");
                        EmitCode($"    ADD {RegName(r)},{RegName(ptrR)},{RegName(AddrReg)}");
                        EmitCode($"    LOAD {RegName(r)},{RegName(r)}");
                        return r;
                    }
                }
                throw new Exception($"Cannot find struct definition for field '{ma.MemberName}'");
            }
            throw new Exception($"Cannot access member: {ma.MemberName}");
        }
        
        void EmitMemStore(MemberAccess ma,int v){
            if(ma.Object is Identifier id&&_varSlots.TryGetValue(id.Name,out int ba)&&_structFields.TryGetValue(id.Name,out var fl)){
                int off=fl.FindIndex(f=>f.Name==ma.MemberName);
                if(off>=0){EmitAddr(ba+off);EmitCode($"    STORE {RegName(v)},{RegName(AddrReg)}");}
            }
            if(ma.Object is ArrayAccess aa&&_structFields.TryGetValue(aa.ArrayName,out var fl2)){
                int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                if(off>=0){
                    int ba2=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;
                    EmitCode($"    PUSH {RegName(v)}");
                    int idx=FlatIdx(aa);
                    EmitAddr(ba2+off);
                    EmitCode($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(idx)}");
                    int v_pop=AllocR();
                    EmitCode($"    POP {RegName(v_pop)}");
                    EmitCode($"    STORE {RegName(v_pop)},{RegName(AddrReg)}");
                }
            }
            if(ma.Object is UnaryOp uo&&uo.Operator=="*"&&uo.Operand is Identifier ptrId){
                foreach(var kv in _structDefs){
                    int off=kv.Value.FindIndex(f=>f.Name==ma.MemberName);
                    if(off>=0){
                        int ptrR=GenExpr(uo.Operand);
                        EmitCode($"    LI {RegName(AddrReg)},{off}");
                        EmitCode($"    ADD {RegName(AddrReg)},{RegName(ptrR)},{RegName(AddrReg)}");
                        EmitCode($"    STORE {RegName(v)},{RegName(AddrReg)}");
                        return;
                    }
                }
            }
        }
        
        int GenUn(UnaryOp uo){
            if(uo.Operator=="&"){
                if(uo.Operand is Identifier id){
                    if(_varSlots.TryGetValue(id.Name,out int a))return Imm(a);
                    if(_globalLabels.TryGetValue(id.Name,out string glbl)){int r=AllocR();EmitCode($"    LIMM {RegName(r)},{glbl}");return r;}
                    throw new Exception($"Cannot take address of {id.Name}");
                }
                if(uo.Operand is MemberAccess ma&&ma.Object is Identifier id2&&_varSlots.TryGetValue(id2.Name,out int ba2)&&_structFields.TryGetValue(id2.Name,out var fl2)){
                    int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                    if(off>=0)return Imm(ba2+off);
                }
                if(uo.Operand is ArrayAccess aa){
                    int arrB=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;
                    int idx=FlatIdx(aa);
                    int ra=AllocR();EmitCode($"    LIMM {RegName(ra)},{arrB}");EmitCode($"    ADD {RegName(ra)},{RegName(ra)},{RegName(idx)}");return ra;
                }
                throw new Exception($"Cannot take address of {uo.Operand?.GetType().Name}");
            }
            if(uo.Operator=="*"){int pr=GenExpr(uo.Operand),r=AllocR();EmitCode($"    LOAD {RegName(r)},{RegName(pr)}");return r;}
            int o=GenExpr(uo.Operand),r2=AllocR();EmitCode($"    {(uo.Operator=="-"?"NEG":"MOV")} {RegName(r2)},{RegName(o)}");return r2;
        }
        
        int GenBin(BinaryOp bo)
        {
            // Constant folding: if both operands are integer literals, compute at compile time
            if (!IsCmp(bo.Operator) && bo.Left is IntegerLiteral il && bo.Right is IntegerLiteral ir)
            {
                long lv = ParseInt(il.Value), rv = ParseInt(ir.Value);
                long result = bo.Operator switch
                {
                    "+" => lv + rv, "-" => lv - rv, "*" => lv * rv,
                    "/" => lv / rv, "%" => lv % rv,
                    "&" => lv & rv, "|" => lv | rv, "^" => lv ^ rv,
                    "<<" => lv << (int)rv, ">>" => lv >> (int)rv,
                    _ => throw new NotImplementedException(bo.Operator)
                };
                return Imm(result);
            }

            int l1 = GenExpr(bo.Left);
            EmitCode($"    PUSH {RegName(l1)}");
            int r1 = GenExpr(bo.Right);
            int l2 = AllocR();
            while (l2 == r1) l2 = AllocR();
            EmitCode($"    POP {RegName(l2)}");

            if (IsCmp(bo.Operator))
            {
                int resReg = AllocR();
                EmitCode($"    CMP {RegName(l2)},{RegName(r1)}");
                string lt = Lbl("t"), ld = Lbl("d");
                JumpCond(bo.Operator, lt);
                EmitCode($"    LI {RegName(resReg)},-1");
                Jmp(ld);
                EmitCode($"{lt}:");
                EmitCode($"    LI {RegName(resReg)},1");
                EmitCode($"{ld}:");
                return resReg;
            }
            int resultReg = AllocR();
            while (resultReg == l2 || resultReg == r1) resultReg = AllocR();
            string op = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => throw new NotSupportedException($"Unsupported binary operator: {bo.Operator}") };
            EmitCode($"    {op} {RegName(resultReg)},{RegName(l2)},{RegName(r1)}");
            FreeR(l2); FreeR(r1);
            return resultReg;
        }
        
        int EmitAssign(Assignment ass)
        {
            int v;
            if (ass.Operator == "=")
            {
                v = GenExpr(ass.Value);
            }
            else
            {
                int lh = GenExpr(ass.Target);
                EmitCode($"    PUSH {RegName(lh)}");
                int rh = GenExpr(ass.Value);
                int r_lh = AllocR();
                while (r_lh == rh) r_lh = AllocR();
                EmitCode($"    POP {RegName(r_lh)}");
                string op = ass.Operator switch { "+=" => "ADD", "-=" => "SUB", "*=" => "MUL", "/=" => "DIV", "%=" => "MOD", "&=" => "AND", "|=" => "OR", "^=" => "XOR", "<<=" => "SHL", ">>=" => "SHR", _ => throw new NotSupportedException($"Unsupported assignment operator: {ass.Operator}") };
                v = AllocR();
                while (v == rh || v == r_lh) v = AllocR();
                EmitCode($"    {op} {RegName(v)},{RegName(r_lh)},{RegName(rh)}");
            }
            if (ass.Target is Identifier id) Store(id.Name, v, 0);
            else if (ass.Target is ArrayAccess aa) EmitArrStore(aa, v);
            else if (ass.Target is MemberAccess ma) EmitMemStore(ma, v);
            return v;
        }
        
        int EmitCall(FunctionCall fc){
            // ABI v3: Spill live locals to stack before CALL
            // Spill locals (reverse order — pushed last = restored first)
            var liveSlots = new List<(string name, int slot)>();
            foreach(var kv in _varSlots)if(_liveVars.Contains(kv.Key))liveSlots.Add((kv.Key,kv.Value));
            liveSlots.Reverse(); // spill in reverse so first spilled = last restored
            foreach(var (name,_) in liveSlots){
                int vr=LoadV(name,0);
                EmitCode($"    PUSH {RegName(vr)}");
            }
            // Spill FPU registers (FTOI + PUSH for all 9 F-regs if any float used)
            if(_fpuLive){
                EmitCode("    FTOI R0, F4, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F3, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F2, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F1, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F0, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FZ, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FY, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FX, 0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FW, 0");EmitCode("    PUSH R0");
            }
            // Save caller-saved GP registers
            EmitCode("    PUSH RW");EmitCode("    PUSH RX");EmitCode("    PUSH RY");EmitCode("    PUSH RZ");
            EmitCode("    PUSH R0");EmitCode("    PUSH R3");EmitCode("    PUSH R1");EmitCode("    PUSH R4");
            // Push arguments in reverse order
            for(int i=fc.Arguments.Count-1;i>=0;i--)
                EmitCode($"    PUSH {RegName(GenExpr(fc.Arguments[i]))}");
            EmitCode($"    LI R1,{fc.FunctionName}");
            EmitCode("    CALL R1");
            // Restore caller-saved GP registers
            EmitCode("    POP R4");EmitCode("    POP R1");EmitCode("    POP R3");EmitCode("    POP R0");
            EmitCode("    POP RZ");EmitCode("    POP RY");EmitCode("    POP RX");EmitCode("    POP RW");
            // Restore FPU registers
            if(_fpuLive){
                EmitCode("    POP R0");EmitCode("    ITOF FW, R0");
                EmitCode("    POP R0");EmitCode("    ITOF FX, R0");
                EmitCode("    POP R0");EmitCode("    ITOF FY, R0");
                EmitCode("    POP R0");EmitCode("    ITOF FZ, R0");
                EmitCode("    POP R0");EmitCode("    ITOF F0, R0");
                EmitCode("    POP R0");EmitCode("    ITOF F1, R0");
                EmitCode("    POP R0");EmitCode("    ITOF F2, R0");
                EmitCode("    POP R0");EmitCode("    ITOF F3, R0");
                EmitCode("    POP R0");EmitCode("    ITOF F4, R0");
            }
            // Restore live locals (pop and store back to absolute addresses)
            foreach(var (name,_) in liveSlots){
                EmitCode("    POP R0");
                Store(name,4,0);
            }
            int r=AllocR();EmitCode($"    MOV {RegName(r)},R2");
            // Clear tracking for next call (caller state is fully restored)
            _liveVars.Clear();
            _fpuLive = false;
            return r;
        }
        
        int FlatIdx(ArrayAccess aa)
        {
            if (!_arrDims.TryGetValue(aa.ArrayName, out var dims) || dims.Count <= 1)
                return GenExpr(aa.Indices[0]);

            int r = AllocR();
            EmitCode($"    LI {RegName(r)},0");
            for (int i = 0; i < dims.Count; i++)
            {
                EmitCode($"    PUSH {RegName(r)}");
                int idxR = GenExpr(aa.Indices[i]);
                int stride = 1;
                for (int j = i + 1; j < dims.Count; j++) stride *= dims[j];
                int sR = AllocR();
                while (sR == idxR) sR = AllocR();
                EmitCode($"    LI {RegName(sR)},{stride}");
                
                int t = AllocR();
                while (t == idxR || t == sR) t = AllocR();
                EmitCode($"    MUL {RegName(t)},{RegName(idxR)},{RegName(sR)}");
                
                int r_restored = AllocR();
                while (r_restored == t) r_restored = AllocR();
                EmitCode($"    POP {RegName(r_restored)}");
                
                int nextR = AllocR();
                while (nextR == r_restored || nextR == t) nextR = AllocR();
                EmitCode($"    ADD {RegName(nextR)},{RegName(r_restored)},{RegName(t)}");
                r = nextR;
            }
            return r;
        }
        
        int EmitArrAccess(ArrayAccess aa){int ba=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;int off=FlatIdx(aa);EmitAddr(ba);EmitCode($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(off)}");int r=AllocR();EmitCode($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
        void EmitArrStore(ArrayAccess aa,int v)
        {
            int ba = _varSlots.TryGetValue(aa.ArrayName, out int b) ? b : _nextAddr;
            EmitCode($"    PUSH {RegName(v)}");
            int off = FlatIdx(aa);
            EmitAddr(ba);
            EmitCode($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(off)}");
            int v_pop = AllocR();
            EmitCode($"    POP {RegName(v_pop)}");
            EmitCode($"    STORE {RegName(v_pop)},{RegName(AddrReg)}");
        }
        
        const int AddrReg = 8;
        void EmitAddr(long addr){if(addr>=-364&&addr<=364)EmitCode($"    LI {RegName(AddrReg)},{addr}");else EmitCode($"    LIMM {RegName(AddrReg)},{addr}");}
        void Alloc(string name,TypeSpec ts){if(!_varSlots.ContainsKey(name)){_varSlots[name]=_nextAddr;int sz=1;if(ts.StructName!=null&&_structDefs.TryGetValue(ts.StructName,out var sf)){sz=sf.Count;_structFields[name]=sf;}else if(ts.Dims.Count>0){sz=ts.Dims.Aggregate(1,(a,b)=>a*b);_arrDims[name]=ts.Dims;}_varSizes[name]=sz;_nextAddr+=sz;}}
        
        int LoadV(string name,int idx){
            int r=AllocR();
            if(_varSlots.TryGetValue(name,out int a)){_liveVars.Add(name);EmitAddr(a+idx);EmitCode($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
            if(_globalSlots.TryGetValue(name,out int gs)){EmitAddr((long)gs+idx);EmitCode($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
            if(_globalLabels.TryGetValue(name,out string glbl)){EmitCode($"    LIMM {RegName(AddrReg)},{glbl}");EmitCode($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
            throw new Exception($"Undefined variable: {name}");
        }
        
        void Store(string name,int reg,int idx){
            if(_varSlots.TryGetValue(name,out int a)){_liveVars.Add(name);EmitAddr(a+idx);EmitCode($"    STORE {RegName(reg)},{RegName(AddrReg)}");return;}
            if(_globalSlots.TryGetValue(name,out int gs)){EmitAddr((long)gs+idx);EmitCode($"    STORE {RegName(reg)},{RegName(AddrReg)}");return;}
            if(_globalLabels.TryGetValue(name,out string glbl)){EmitCode($"    LIMM {RegName(AddrReg)},{glbl}");EmitCode($"    STORE {RegName(reg)},{RegName(AddrReg)}");return;}
            throw new Exception($"Undefined variable: {name}");
        }

        int EmitString(string value)
        {
            string lbl = Lbl("str");
            _stringsToEmit.Add((lbl, value));
            int r = AllocR();
            EmitCode($"    LIMM {RegName(r)},{lbl}");
            return r;
        }

        int EmitFloat(string value)
        {
            string lbl = Lbl("flt");
            double d = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            T3Float tf = T3Float.FromDouble(d);
            Word18 w = tf.ToWord18();
            _floatsToEmit.Add((lbl, w));
            _fpuLive = true;
            int r = AllocR();
            EmitCode($"    LIMM {RegName(r)},{lbl}");
            EmitCode($"    FLW {RegName(r)},{RegName(r)}");
            return r;
        }
        static bool IsCmp(string op)=>op is"=="or"!="or"<"or">"or"<="or">=";
        long ParseInt(string v) => LiteralParser.ParseInt(v);
        int _nextReg=0;
        readonly Stack<int> _freeRegs = new();

        /// <summary>Allocate a temporary register.</summary>
        int AllocR(){
            while(_freeRegs.Count>0){
                int fr=_freeRegs.Pop();
                if(fr!=5&&fr!=6&&fr!=8)return fr;
            }
            while(true){
                if(_nextReg!=5&&_nextReg!=6&&_nextReg!=8)break;
                _nextReg=(_nextReg+1)%9;
            }
            int r=_nextReg;
            _nextReg=(_nextReg+1)%9;
            return r;
        }

        /// <summary>Release a temporary register for reuse.</summary>
        void FreeR(int r){
            if(r>=0&&r<9&&r!=5&&r!=6&&r!=8)_freeRegs.Push(r);
        }
        int Imm(long v){int r=AllocR();if(v>=-364&&v<=364)EmitCode($"    LI {RegName(r)},{v}");else{EmitCode($"    LIMM {RegName(r)},{v}");}return r;}
        string Lbl(string pfx)=>$"{pfx}_{_labelCounter++}";
        void Emit(string s="")=>_output.AppendLine(s);
        void EmitCode(string s="")=>_codeOutput.AppendLine(s);
        string RegName(int i) => i switch {
            0 => "RW", 1 => "RX", 2 => "RY", 3 => "RZ",
            4 => "R0", 5 => "R1", 6 => "R2", 7 => "R3", 8 => "R4",
            _ => throw new Exception($"Invalid register index {i}")
        };
    }
}