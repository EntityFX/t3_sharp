using System;using System.Collections.Generic;using System.Linq;using System.Text;using T3Compiler.Parser;using T3Simulator.Common;using TritTypes;
namespace T3Compiler.CodeGen
{
    public class CodeGenerator
    {
        readonly AstProgram _program;readonly StringBuilder _output;readonly StringBuilder _codeOutput;
        int _labelCounter;
        readonly Dictionary<string,int> _varSlots=new(),_varSizes=new(),_varElemSizes=new();
        readonly Dictionary<string,string> _globalLabels=new();
        readonly Dictionary<string,int> _enumConstants=new();
        readonly List<(string label, string value)> _stringsToEmit=new();
        readonly List<(string label, Word18 value)> _floatsToEmit=new();
        readonly List<(string label, long value)> _globalsToEmit=new();
        readonly Dictionary<string,List<int>> _arrDims=new();readonly Dictionary<string,List<FieldDef>> _structFields=new();
        readonly Stack<(string brk,string cont)> _loopStack=new();readonly Dictionary<string,List<FieldDef>> _structDefs=new();
        string? _epilogueLabel;
        readonly HashSet<string> _liveVars = new();
        bool _fpuLive = false;
        readonly Dictionary<string,int> _globalSlots = new();
        // Label-based local variable allocation (dynamic, no hardcoded addresses)
        int _localSlotCounter;     // per-function slot counter
        int _currentLocalSize;     // size of current function's local frame
        string _currentFunc = "";  // current function name

        // Struct size cache for sizeof operator
        readonly Dictionary<string,int> _structSizes = new();

        // ABI v4 register assignments
        const int FP = 3;    // RZ
        const int CALLREG = 5; // R1
        const int RETREG = 6;  // R2
        const int ADDRREG = 8; // R4 (callee-saved)

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
            // Allocate global slots indices
            int gSlotIdx = 0;
            foreach(var g in _program.Globals){
                int sz = 1;
                if(g.Type.Dims.Count>0) sz = g.Type.Dims.Aggregate(1,(a,b)=>a*b);
                _globalSlots[g.Name] = gSlotIdx;
                gSlotIdx += sz;
            }
            
            // Generate all code
            EmitCode("; T→T3");
            EmitCode("__entry:");
            EmitCode("    LIMM R1,main");
            EmitCode("    CALL R1");
            EmitCode("    HALT");
            foreach(var f in _program.Functions)GenFunc(f);
            
            // Compute code size
            string codeText = _codeOutput.ToString();
            int codeWords = 0;
            foreach(var line in codeText.Split(new[]{"\r\n","\r","\n"},StringSplitOptions.None)){
                string t = line.Trim();
                if(string.IsNullOrEmpty(t)||t.StartsWith(";"))continue;
                codeWords++;
            }
            _output.Append(codeText);
            
            // Global slots at absolute addresses: codeWords + 100
            int baseAddr = codeWords + 100;
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
            
            Emit("\n; --- Global Variables ---");
            foreach(var g in _program.Globals){
                int sz = 1;
                if(g.Type.Dims.Count>0) sz = g.Type.Dims.Aggregate(1,(a,b)=>a*b);
                var vals = new List<long>(new long[sz]);
                Emit($"{g.Name}: .word {string.Join(", ", vals)}");
            }
            
            
            Emit("\n; --- Data Section ---");
            foreach(var (lbl, val) in _stringsToEmit) Emit($".string {lbl} \"{val}\"");
            foreach(var (lbl, val) in _floatsToEmit) Emit($"{lbl}: .word {val.ToLong()}");
            foreach(var (lbl, val) in _globalsToEmit) Emit($"{lbl}: .word {val}");
            
            Emit("\n; --- StdLib ---");
            Emit("strlen:");
            Emit("    PUSH RZ");Emit("    PUSH R3");Emit("    PUSH R4");
            Emit("    LI R2, 0");
            Emit("strlen_loop:");
            Emit("    LOADI R1, RW, 0");
            Emit("    CMPI R1, 0");
            Emit("    JE strlen_end");
            Emit("    ADDI R2, 1");
            Emit("    ADDI RW, 1");
            Emit("    JMP strlen_loop");
            Emit("strlen_end:");
            Emit("    POP R4");Emit("    POP R3");Emit("    POP RZ");
            Emit("    RET");
            // putchar(c) — write char to port 0
            Emit("putchar:");
            Emit("    PUSH RZ");Emit("    PUSH R3");Emit("    PUSH R4");
            Emit("    OUTI RW, 0");
            Emit("    POP R4");Emit("    POP R3");Emit("    POP RZ");
            Emit("    RET");
            // getchar() — read char from port 0
            Emit("getchar:");
            Emit("    PUSH RZ");Emit("    PUSH R3");Emit("    PUSH R4");
            Emit("    INI R2, 0");
            Emit("    POP R4");Emit("    POP R3");Emit("    POP RZ");
            Emit("    RET");
            
            return _output.ToString();
        }
        

        void GenFunc(FunctionDef f){
            _varSlots.Clear();_varSizes.Clear();_arrDims.Clear();_structFields.Clear();
            _nextReg = 0;_freeRegs.Clear();
            _epilogueLabel = Lbl("epilogue");
            _localSlotCounter = 0;
            _currentFunc = f.Name;

            // Pre-scan for all local variable declarations to reserve space in the prologue
            void ScanBody(IEnumerable<Statement> body) {
                foreach(var s in body) {
                    if(s is VarDeclaration vd) Alloc(vd.Name, vd.Type);
                    else if(s is CompoundStmt cs) ScanBody(cs.Body);
                    else if(s is IfStmt ifs) {
                        if(ifs.ThenBody is CompoundStmt tcs) ScanBody(tcs.Body); else if(ifs.ThenBody is VarDeclaration tvd) Alloc(tvd.Name, tvd.Type);
                        if(ifs.ElseBody is CompoundStmt ecs) ScanBody(ecs.Body); else if(ifs.ElseBody is VarDeclaration evd) Alloc(evd.Name, evd.Type);
                        if(ifs.MaybeBody is CompoundStmt mcs) ScanBody(mcs.Body); else if(ifs.MaybeBody is VarDeclaration mvd) Alloc(mvd.Name, mvd.Type);
                    }
                    else if(s is WhileStmt ws) {
                        if(ws.Body is CompoundStmt wcs) ScanBody(wcs.Body); else if(ws.Body is VarDeclaration wvd) Alloc(wvd.Name, wvd.Type);
                    }
                    else if(s is DoWhileStmt dws) {
                        if(dws.Body is CompoundStmt dwcs) ScanBody(dwcs.Body); else if(dws.Body is VarDeclaration dwvd) Alloc(dwvd.Name, dwvd.Type);
                    }
                    else if(s is ForStmt fs) {
                        if(fs.Init is VarDeclaration fivd) Alloc(fivd.Name, fivd.Type);
                        if(fs.Body is CompoundStmt fcs) ScanBody(fcs.Body); else if(fs.Body is VarDeclaration fvd) Alloc(fvd.Name, fvd.Type);
                    }
                }
            }
            ScanBody(f.Body.Body);

            // Allocate local slots (0-based indices)
            foreach(var param in f.Parameters) Alloc(param.Name, param.Type);

            EmitCode($"{f.Name}:");

            // === ABI v4 Prologue ===
            EmitCode("    PUSH RZ");           // save old FP
            EmitCode("    PUSH R3");            // callee-saved
            EmitCode("    PUSH R4");            // callee-saved
            EmitCode("    MOV RZ, SP");         // RZ = FP, points to Saved R4

            // Allocate frame for locals
            int localSize = 0;
            foreach(var sz in _varSizes.Values) localSize += sz;
            _currentLocalSize = localSize;
            if(localSize > 0){
                if(localSize <= 364) EmitCode($"    SUBI SP, SP, {localSize}");
                else {
                    int r = AllocR();
                    EmitCode($"    LIMM {RegName(r)},{localSize}");
                    EmitCode($"    SUB SP, SP, {RegName(r)}");
                    FreeR(r);
                }
            }

            // Save register args 0..3 to local slots
            int[] argRegs = {0, 1, 2, 4}; // RW, RX, RY, R0
            int nParams = f.Parameters.Count;
            for(int i=0;i<4 && i<nParams;i++)
                StoreV(f.Parameters[i].Name, argRegs[i], 0);
            
            // Handle args 4+ from caller's stack (FP-relative)
            // Stack Layout (relative to RZ):
            // RZ + 2: Arg 4
            // RZ + 3: Arg 5 ...
            if(nParams > 4){
                for(int i=4;i<nParams;i++){
                    // Arg 4 is at RZ + 4, Arg 5 at +5, etc.
                    // Layout: RZ(0)=SavedR4, RZ+1=SavedR3, RZ+2=SavedRZ, RZ+3=RetAddr, RZ+4=Arg4
                    int stackOff = (i - 4) + 4;
                    int t=AllocR();
                    if (stackOff >= -13 && stackOff <= 13) {
                        EmitCode($"    LOADI {RegName(t)}, RZ, {stackOff}");
                    } else {
                        int offR = Imm(stackOff);
                        int addrR = AllocR();
                        EmitCode($"    ADD {RegName(addrR)}, RZ, {RegName(offR)}");
                        EmitCode($"    LOADI {RegName(t)}, {RegName(addrR)}, 0");
                        FreeR(offR); FreeR(addrR);
                    }
                    StoreV(f.Parameters[i].Name, t, 0);FreeR(t);
                }
            }

            foreach(var s in f.Body.Body)GenStmt(s);

            // === ABI v4 Epilogue ===
            EmitCode($"{_epilogueLabel}:");
            if (localSize > 0)
            {
                if (localSize <= 364) EmitCode($"    ADDI SP, SP, {localSize}");
                else
                {
                    int r = AllocR();
                    EmitCode($"    LIMM {RegName(r)},{localSize}");
                    EmitCode($"    ADD SP, SP, {RegName(r)}");
                    FreeR(r);
                }
            }
            EmitCode("    POP R4");
            EmitCode("    POP R3");
            EmitCode("    POP RZ");
            EmitCode("    RET");
        }
        
        void GenStmt(Statement s){switch(s){case ExpressionStmt e:if(e.Expression!=null)GenExpr(e.Expression);break;case VarDeclaration vd:Alloc(vd.Name,vd.Type);if(vd.Type.StructName!=null&&_structDefs.TryGetValue(vd.Type.StructName,out var sf))_structFields[vd.Name]=sf;if(vd.Initializer!=null){int r=GenExpr(vd.Initializer);StoreV(vd.Name,r,0);}break;case ReturnStmt rs:if(rs.Value!=null){int r=GenExpr(rs.Value);EmitCode($"    MOV R2,{RegName(r)}");}if(_epilogueLabel!=null){int rj=AllocR();EmitCode($"    LIMM {RegName(rj)},{_epilogueLabel}");EmitCode($"    JMP {RegName(rj)}");}else EmitCode("    RET");break;case CompoundStmt cs:foreach(var ss in cs.Body)GenStmt(ss);break;case IfStmt ifs:GenIf(ifs);break;case WhileStmt ws:GenWhile(ws);break;case DoWhileStmt dws:GenDoWhile(dws);break;case ForStmt fs:GenFor(fs);break;case SwitchStmt ss:GenSwitch(ss);break;case BreakStmt _:{var(brk,_)=_loopStack.Peek();Jmp(brk);}break;case ContinueStmt _:{var(_,cont)=_loopStack.Peek();Jmp(cont);}break;case GotoStmt gs:Jmp($"__glbl_{gs.Label}");break;case LabeledStmt ls:EmitCode($"__glbl_{ls.Label}:");GenStmt(ls.Body);break;}}
        
        void GenIf(IfStmt s){
            string le=Lbl("end"),lt=Lbl("then");
            if(s.Condition is BinaryOp bo && bo.Operator is not "&&" and not "||"){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                EmitCode($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,lt);
                if(s.ElseBody!=null)GenStmt(s.ElseBody);
                Jmp(le);
                EmitCode($"{lt}:");GenStmt(s.ThenBody);
                EmitCode($"{le}:");
            }else{
                int c=GenExpr(s.Condition);
                EmitCode($"    LI R2,0");EmitCode($"    CMP {RegName(c)},R2");
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
                EmitCode($"    LI R2,0");EmitCode($"    CMP {RegName(c)},R2");
                JumpReg("JNE",lb);
                Jmp(le);
            }
            EmitCode($"{lb}:");GenStmt(s.Body);Jmp(ll);
            EmitCode($"{le}:");_loopStack.Pop();
        }
        
        void GenDoWhile(DoWhileStmt s){
            string ll=Lbl("loop"),le=Lbl("wend");
            _loopStack.Push((le,ll));
            EmitCode($"{ll}:");GenStmt(s.Body);
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                EmitCode($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,ll);
            }else{
                int c=GenExpr(s.Condition);
                EmitCode($"    LI R2,0");EmitCode($"    CMP {RegName(c)},R2");
                JumpReg("JNE",ll);
            }
            EmitCode($"{le}:");_loopStack.Pop();
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
                    EmitCode($"    LI R2,0");EmitCode($"    CMP {RegName(c)},R2");
                    JumpReg("JNE",lb);
                }
                Jmp(le);
            }
            EmitCode($"{lb}:");GenStmt(fs.Body);
            if(fs.Step!=null) GenExpr(fs.Step);
            Jmp(ll);
            EmitCode($"{le}:");_loopStack.Pop();
        }
        
        void GenSwitch(SwitchStmt s){
            int exprReg = GenExpr(s.Expression);
            string end = Lbl("swend");
            var labels = new List<string>();
            for (int i = 0; i < s.Cases.Count; i++) labels.Add(Lbl("scase"));
            int defaultIdx = -1;
            for (int i = 0; i < s.Cases.Count; i++) {
                if (s.Cases[i].Value == null) { defaultIdx = i; continue; }
                int caseVal = GenExpr(s.Cases[i].Value);
                EmitCode($"    CMP {RegName(exprReg)},{RegName(caseVal)}");
                JumpReg("JE", labels[i]);
            }
            if (defaultIdx >= 0) Jmp(labels[defaultIdx]); else Jmp(end);
            for (int i = 0; i < s.Cases.Count; i++) {
                EmitCode($"{labels[i]}:");
                foreach (var stmt in s.Cases[i].Body) GenStmt(stmt);
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
            EmitCode($"    LI R2,0");EmitCode($"    CMP {RegName(cr)},R2");
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
                int r=AllocR();
                LabelAddr(ADDRREG, ba + off);
                EmitCode($"    LOADI {RegName(r)},{RegName(ADDRREG)}, 0");return r;
            }
            if(ma.Object is ArrayAccess aa&&_structFields.TryGetValue(aa.ArrayName,out var fl2)){
                int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                if(off<0)throw new Exception($"Unknown field: {ma.MemberName}");
                int ba2=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_localSlotCounter;
                int idx=FlatIdx(aa);
                int r=AllocR();
                LabelAddr(ADDRREG, ba2 + off);
                EmitCode($"    ADD {RegName(r)},{RegName(ADDRREG)},{RegName(idx)}");
                EmitCode($"    LOADI {RegName(r)},{RegName(r)}, 0");return r;
            }
            if(ma.Object is UnaryOp uo&&uo.Operator=="*"&&uo.Operand is Identifier ptrId){
                foreach(var kv in _structDefs){
                    int off=kv.Value.FindIndex(f=>f.Name==ma.MemberName);
                if(off>=0){
                    int ptrR=GenExpr(uo.Operand);
                    int offR=AllocR();
                    EmitCode($"    LI {RegName(offR)},{off}");
                    int r=AllocR();
                    EmitCode($"    ADD {RegName(r)},{RegName(ptrR)},{RegName(offR)}");
                    EmitCode($"    LOADI {RegName(r)},{RegName(r)}, 0");
                    FreeR(offR);
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
                if(off>=0){
                    LabelAddr(ADDRREG, ba + off);
                    EmitCode($"    STOREI {RegName(ADDRREG)}, 0, {RegName(v)}");
                }
            }
            if(ma.Object is ArrayAccess aa&&_structFields.TryGetValue(aa.ArrayName,out var fl2)){
                int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                if(off>=0){
                    int ba2=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_localSlotCounter;
                    EmitCode($"    PUSH {RegName(v)}");
                    int idx=FlatIdx(aa);
                    LabelAddr(ADDRREG, ba2 + off);
                    EmitCode($"    ADD {RegName(ADDRREG)},{RegName(ADDRREG)},{RegName(idx)}");
                    int v_pop=AllocR();
                    EmitCode($"    POP {RegName(v_pop)}");
                EmitCode($"    STOREI {RegName(ADDRREG)}, 0, {RegName(v_pop)}");
                }
            }
            if(ma.Object is UnaryOp uo&&uo.Operator=="*"&&uo.Operand is Identifier ptrId){
                foreach(var kv in _structDefs){
                    int off=kv.Value.FindIndex(f=>f.Name==ma.MemberName);
                if(off>=0){
                    int ptrR=GenExpr(uo.Operand);
                    int offR=AllocR();
                    EmitCode($"    LI {RegName(offR)},{off}");
                    int addrR=AllocR();
                    EmitCode($"    ADD {RegName(addrR)},{RegName(ptrR)},{RegName(offR)}");
                    EmitCode($"    STOREI {RegName(addrR)}, 0, {RegName(v)}");
                    FreeR(offR);
                    FreeR(addrR);
                    return;
                }
                }
            }
        }
        
        int GenUn(UnaryOp uo){
            if(uo.Operator=="&"){
                if(uo.Operand is Identifier id){
                    if(_varSlots.TryGetValue(id.Name,out int a)) return ImmLabel(a);
                    if(_globalLabels.TryGetValue(id.Name,out string glbl)){int r=AllocR();EmitCode($"    LIMM {RegName(r)},{glbl}");return r;}
                    throw new Exception($"Cannot take address of {id.Name}");
                }
                if(uo.Operand is MemberAccess ma&&ma.Object is Identifier id2&&_varSlots.TryGetValue(id2.Name,out int ba2)&&_structFields.TryGetValue(id2.Name,out var fl2)){
                    int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                    if(off>=0) return ImmLabel(ba2 + off);
                }
                if(uo.Operand is ArrayAccess aa){
                    int arrB=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_localSlotCounter;
                    int idx=FlatIdx(aa);
                    int ra=ImmLabel(arrB);
                    EmitCode($"    ADD {RegName(ra)},{RegName(ra)},{RegName(idx)}");return ra;
                }
                throw new Exception($"Cannot take address of {uo.Operand?.GetType().Name}");
            }
            if(uo.Operator=="*"){int pr=GenExpr(uo.Operand),r=AllocR();EmitCode($"    LOADI {RegName(r)},{RegName(pr)}, 0");return r;}
            if(uo.Operator=="!"){
                int o=GenExpr(uo.Operand),r2=AllocR();
                EmitCode($"    CMPI {RegName(o)},0");
                string lt=Lbl("t"),ld=Lbl("d");
                JumpReg("JG",lt);
                EmitCode($"    LI {RegName(r2)},1");Jmp(ld);
                EmitCode($"{lt}:");EmitCode($"    LI {RegName(r2)},-1");
                EmitCode($"{ld}:");return r2;
            }
            int o2=GenExpr(uo.Operand),r3=AllocR();EmitCode($"    {(uo.Operator=="-"?"NEG":"MOV")} {RegName(r3)},{RegName(o2)}");return r3;
        }
        
        int GenBin(BinaryOp bo)
        {
            if (bo.Operator == "||")
            {
                int resReg = AllocR();
                int leftR = GenExpr(bo.Left);
                int zeroR = Imm(0);
                EmitCode($"    CMP {RegName(leftR)},{RegName(zeroR)}");
                string leftTrue = Lbl("or_t"), endOr = Lbl("or_e");
                JumpReg("JG", leftTrue);
                FreeR(zeroR);
                int rightR = GenExpr(bo.Right);
                int zeroR2 = Imm(0);
                EmitCode($"    CMP {RegName(rightR)},{RegName(zeroR2)}");
                string rightTrue = Lbl("or_rt");
                JumpReg("JG", rightTrue);
                FreeR(zeroR2);
                EmitCode($"    LI {RegName(resReg)}, -1");
                Jmp(endOr);
                EmitCode($"{rightTrue}:");
                EmitCode($"    LI {RegName(resReg)}, 1");
                Jmp(endOr);
                EmitCode($"{leftTrue}:");
                EmitCode($"    LI {RegName(resReg)}, 1");
                EmitCode($"{endOr}:");
                FreeR(leftR); FreeR(rightR);
                return resReg;
            }
            if (bo.Operator == "&&")
            {
                int resReg = AllocR();
                int leftR = GenExpr(bo.Left);
                int zeroR = Imm(0);
                EmitCode($"    CMP {RegName(leftR)},{RegName(zeroR)}");
                string leftFalse = Lbl("and_f"), endAnd = Lbl("and_e");
                JumpReg("JLE", leftFalse);
                FreeR(zeroR);
                int rightR = GenExpr(bo.Right);
                int zeroR2 = Imm(0);
                EmitCode($"    CMP {RegName(rightR)},{RegName(zeroR2)}");
                string rightTrue = Lbl("and_rt");
                JumpReg("JG", rightTrue);
                FreeR(zeroR2);
                EmitCode($"    LI {RegName(resReg)}, -1");
                Jmp(endAnd);
                EmitCode($"{rightTrue}:");
                EmitCode($"    LI {RegName(resReg)}, 1");
                Jmp(endAnd);
                EmitCode($"{leftFalse}:");
                EmitCode($"    LI {RegName(resReg)}, -1");
                EmitCode($"{endAnd}:");
                FreeR(leftR); FreeR(rightR);
                return resReg;
            }
            if (!IsCmp(bo.Operator) && bo.Left is IntegerLiteral il && bo.Right is IntegerLiteral ir)
            {
                long lv = ParseInt(il.Value), rv = ParseInt(ir.Value);
                long result = bo.Operator switch{
                    "+" => lv + rv, "-" => lv - rv, "*" => lv * rv, "/" => lv / rv, "%" => lv % rv,
                    "&" => lv & rv, "|" => lv | rv, "^" => lv ^ rv,
                    "<<" => lv << (int)rv, ">>" => lv >> (int)rv,
                    _ => throw new NotImplementedException(bo.Operator)
                };return Imm(result);
            }
            int l1 = GenExpr(bo.Left);
            EmitCode($"    PUSH {RegName(l1)}");
            int r1 = GenExpr(bo.Right);
            int l2 = AllocR();while (l2 == r1) l2 = AllocR();
            EmitCode($"    POP {RegName(l2)}");
            if (IsCmp(bo.Operator))
            {
                int resReg = AllocR();
                EmitCode($"    CMP {RegName(l2)},{RegName(r1)}");
                string lt = Lbl("t"), ld = Lbl("d");
                JumpCond(bo.Operator, lt);
                EmitCode($"    LI {RegName(resReg)},-1");Jmp(ld);
                EmitCode($"{lt}:");EmitCode($"    LI {RegName(resReg)},1");
                EmitCode($"{ld}:");return resReg;
            }
            int resultReg = AllocR();while (resultReg == l2 || resultReg == r1) resultReg = AllocR();
            string op = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => throw new NotSupportedException($"Unsupported binary operator: {bo.Operator}") };
            EmitCode($"    {op} {RegName(resultReg)},{RegName(l2)},{RegName(r1)}");
            FreeR(l2);FreeR(r1);return resultReg;
        }
        
        int EmitAssign(Assignment ass){
            int v;
            if (ass.Operator == "="){v = GenExpr(ass.Value);}
            else{
                int lh = GenExpr(ass.Target);
                EmitCode($"    PUSH {RegName(lh)}");
                int rh = GenExpr(ass.Value);
                int r_lh = AllocR();while (r_lh == rh) r_lh = AllocR();
                EmitCode($"    POP {RegName(r_lh)}");
                string op = ass.Operator switch { "+=" => "ADD", "-=" => "SUB", "*=" => "MUL", "/=" => "DIV", "%=" => "MOD", "&=" => "AND", "|=" => "OR", "^=" => "XOR", "<<=" => "SHL", ">>=" => "SHR", _ => throw new NotSupportedException($"Unsupported assignment operator: {ass.Operator}") };
                v = AllocR();while (v == rh || v == r_lh) v = AllocR();
                EmitCode($"    {op} {RegName(v)},{RegName(r_lh)},{RegName(rh)}");
            }
            if (ass.Target is Identifier id) StoreV(id.Name, v, 0);
            else if (ass.Target is ArrayAccess aa) EmitArrStore(aa, v);
            else if (ass.Target is MemberAccess ma) EmitMemStore(ma, v);
            return v;
        }
        
        int EmitCall(FunctionCall fc){
            // ABI v4: Register args 0..3, stack for 4+.
            // Local variables are now stored in the stack frame and are safe across calls.
            
            int nArgs = fc.Arguments.Count;
            int[] argRegs = {0, 1, 2, 4}; // RW, RX, RY, R0
            var argRegList = new List<int>();
            for(int i=0;i<nArgs;i++) argRegList.Add(GenExpr(fc.Arguments[i]));
            
            // 1. Save caller-saved GP first to avoid corrupting them while saving FPU
            EmitCode("    PUSH RW");EmitCode("    PUSH RX");EmitCode("    PUSH RY");
            EmitCode("    PUSH R0");EmitCode("    PUSH R1");

            // 2. Now use R0 as a scratch to save FPU
            if(_fpuLive){
                EmitCode("    FTOI R0, F4");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F3");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F2");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F1");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, F0");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FZ");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FY");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FX");EmitCode("    PUSH R0");
                EmitCode("    FTOI R0, FW");EmitCode("    PUSH R0");
            }

            // 2. Push stack args 4+ in reverse on top of saved registers
            for(int i=nArgs-1;i>=4;i--)
                EmitCode($"    PUSH {RegName(argRegList[i])}");
            // Overlap-safe moves to arg regs
            var savedTemps = new Stack<int>();
            for(int i=0;i<4 && i<nArgs;i++){
                if(argRegList[i] == argRegs[i]) continue;
                for(int j=i+1;j<4 && j<nArgs;j++){
                    if(argRegList[j] == argRegs[i]){
                        int t = AllocR(); while(t==argRegList[j]||t==argRegs[i]) t=AllocR();
                        EmitCode($"    MOV {RegName(t)},{RegName(argRegList[j])}");
                        savedTemps.Push(t);argRegList[j] = t;
                    }
                }
            }
            for(int i=0;i<4 && i<nArgs;i++){
                if(argRegList[i] != argRegs[i])
                    EmitCode($"    MOV {RegName(argRegs[i])},{RegName(argRegList[i])}");
            }
            while(savedTemps.Count>0) FreeR(savedTemps.Pop());

            EmitCode($"    LIMM R1,{fc.FunctionName}");
            EmitCode("    CALL R1");

            // Clean up stack args FIRST so the stack top is our saved registers
            if(nArgs > 4){
                int stackSize = nArgs - 4;
                EmitCode($"    ADDI SP, SP, {stackSize}");
            }

            // Restore FPU first (they were pushed last)
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
            // Now restore caller-saved GP
            EmitCode("    POP R1");EmitCode("    POP R0");
            EmitCode("    POP RY");EmitCode("    POP RX");EmitCode("    POP RW");

            int r=AllocR();EmitCode($"    MOV {RegName(r)},R2");
            _liveVars.Clear();_fpuLive = false;
            return r;
        }
        
        int FlatIdx(ArrayAccess aa){
            if (!_arrDims.TryGetValue(aa.ArrayName, out var dims) || dims.Count <= 1)
                return GenExpr(aa.Indices[0]);
            int r = AllocR();EmitCode($"    LI {RegName(r)},0");
            for (int i = 0; i < dims.Count; i++){
                EmitCode($"    PUSH {RegName(r)}");
                int idxR = GenExpr(aa.Indices[i]);
                int stride = 1;for (int j = i + 1; j < dims.Count; j++) stride *= dims[j];
                int sR = AllocR();while (sR == idxR) sR = AllocR();
                EmitCode($"    LI {RegName(sR)},{stride}");
                int t = AllocR();while (t == idxR || t == sR) t = AllocR();
                EmitCode($"    MUL {RegName(t)},{RegName(idxR)},{RegName(sR)}");
                int r_restored = AllocR();while (r_restored == t) r_restored = AllocR();
                EmitCode($"    POP {RegName(r_restored)}");
                int nextR = AllocR();while (nextR == r_restored || nextR == t) nextR = AllocR();
                EmitCode($"    ADD {RegName(nextR)},{RegName(r_restored)},{RegName(t)}");
                r = nextR;
            }return r;
        }
        
        int EmitArrAccess(ArrayAccess aa){
            int r = AllocR();
            int idxR = FlatIdx(aa);
            int es = _varElemSizes.TryGetValue(aa.ArrayName, out int s) ? s : 1;
            if(es > 1){
                int esR = Imm(es);
                int scaledIdx = AllocR();
                EmitCode($"    MUL {RegName(scaledIdx)},{RegName(idxR)},{RegName(esR)}");
                idxR = scaledIdx;
                FreeR(esR);
            }
            if(_varSlots.TryGetValue(aa.ArrayName, out int slot)){
                LabelAddr(ADDRREG, slot);
                EmitCode($"    ADD {RegName(ADDRREG)},{RegName(ADDRREG)},{RegName(idxR)}");
            } else if(_globalSlots.TryGetValue(aa.ArrayName, out int absAddr)){
                EmitAbsAddr(ADDRREG, absAddr);
                EmitCode($"    ADD {RegName(ADDRREG)},{RegName(ADDRREG)},{RegName(idxR)}");
            } else throw new Exception($"Undefined array: {aa.ArrayName}");
            EmitCode($"    LOADI {RegName(r)},{RegName(ADDRREG)}, 0");
            FreeR(idxR);
            return r;
        }
        void EmitArrStore(ArrayAccess aa,int v){
            int idxR = FlatIdx(aa);
            int es = _varElemSizes.TryGetValue(aa.ArrayName, out int s) ? s : 1;
            if(es > 1){
                int esR = Imm(es);
                int scaledIdx = AllocR();
                EmitCode($"    MUL {RegName(scaledIdx)},{RegName(idxR)},{RegName(esR)}");
                idxR = scaledIdx;
                FreeR(esR);
            }
            if(_varSlots.TryGetValue(aa.ArrayName, out int slot)){
                LabelAddr(ADDRREG, slot);
                EmitCode($"    ADD {RegName(ADDRREG)},{RegName(ADDRREG)},{RegName(idxR)}");
            } else if(_globalSlots.TryGetValue(aa.ArrayName, out int absAddr)){
                EmitAbsAddr(ADDRREG, absAddr);
                EmitCode($"    ADD {RegName(ADDRREG)},{RegName(ADDRREG)},{RegName(idxR)}");
            } else throw new Exception($"Undefined array: {aa.ArrayName}");
            EmitCode($"    PUSH {RegName(v)}");
            int v_pop = AllocR();
            EmitCode($"    POP {RegName(v_pop)}");
            EmitCode($"    STOREI {RegName(ADDRREG)}, 0, {RegName(v_pop)}");
            FreeR(idxR); FreeR(v_pop);
        }
        
        /// <summary>Load address of local slot into a register (RZ + offset)</summary>
        void LabelAddr(int reg, int slotIndex){
            int offset = slotIndex - _currentLocalSize;
            if (offset < 0) {
                int absOff = -offset;
                if (absOff <= 364) {
                    EmitCode($"    LI {RegName(reg)}, {absOff}");
                    EmitCode($"    SUB {RegName(reg)}, RZ, {RegName(reg)}");
                } else {
                    int offR = AllocR();
                    EmitCode($"    LIMM {RegName(offR)}, {absOff}");
                    EmitCode($"    SUB {RegName(reg)}, RZ, {RegName(offR)}");
                    FreeR(offR);
                }
            } else {
                if (offset <= 364) {
                    EmitCode($"    LI {RegName(reg)}, {offset}");
                    EmitCode($"    ADD {RegName(reg)}, RZ, {RegName(reg)}");
                } else {
                    int offR = AllocR();
                    EmitCode($"    LIMM {RegName(offR)}, {offset}");
                    EmitCode($"    ADD {RegName(reg)}, RZ, {RegName(offR)}");
                    FreeR(offR);
                }
            }
        }
        
        /// <summary>Load label address into a scratch register, return register index</summary>
        int ImmLabel(int slotIndex){
            int r = AllocR();
            LabelAddr(r, slotIndex);
            return r;
        }

        void Alloc(string name,TypeSpec ts){
            if(!_varSlots.ContainsKey(name)){
                int sz=1;
                int elemSize=1;
                if(ts.StructName!=null&&_structDefs.TryGetValue(ts.StructName,out var sf)){
                    sz=sf.Count;
                    _structFields[name]=sf;
                    elemSize=sf.Count;
                }
                else if(ts.Dims.Count>0){
                    int count=ts.Dims.Aggregate(1,(a,b)=>a*b);
                    if(ts.StructName!=null&&_structDefs.TryGetValue(ts.StructName,out var sf2)){
                        elemSize=sf2.Count;
                    }
                    sz=count*elemSize;
                    _arrDims[name]=ts.Dims;
                }
                _varSizes[name]=sz;
                _varElemSizes[name]=elemSize;
                _varSlots[name]=_localSlotCounter;
                _localSlotCounter+=sz;
            }
        }
        
        int LoadV(string name,int idx){
            int r=AllocR();
                if(_varSlots.TryGetValue(name,out int a)){
                    _liveVars.Add(name);
                    int offset = a + idx - _currentLocalSize;
                    if (offset >= -13 && offset <= 13) {
                        EmitCode($"    LOADI {RegName(r)}, RZ, {offset}");
                    } else {
                        int offR = Imm(offset);
                        int addrR = AllocR();
                        EmitCode($"    ADD {RegName(addrR)}, RZ, {RegName(offR)}");
                        EmitCode($"    LOADI {RegName(r)}, {RegName(addrR)}, 0");
                        FreeR(offR); FreeR(addrR);
                    }
                    return r;
                }
            if(_globalSlots.TryGetValue(name,out int gs)){
                EmitAbsAddr(ADDRREG, (long)gs+idx);
                EmitCode($"    LOADI {RegName(r)},{RegName(ADDRREG)}, 0");return r;
            }
            if(_globalLabels.TryGetValue(name,out string glbl)){
                EmitCode($"    LIMM {RegName(ADDRREG)},{glbl}");EmitCode($"    LOADI {RegName(r)},{RegName(ADDRREG)}, 0");return r;
            }
            throw new Exception($"Undefined variable: {name}");
        }
        
        void StoreV(string name,int reg,int idx){
            if(_varSlots.TryGetValue(name,out int a)){
                _liveVars.Add(name);
                int offset = a + idx - _currentLocalSize;
                // STOREI format: srcReg, baseReg, offset
                if (offset >= -13 && offset <= 13) {
                    EmitCode($"    STOREI {RegName(reg)}, RZ, {offset}");
                } else {
                    int offR = Imm(offset);
                    int addrR = AllocR();
                    EmitCode($"    ADD {RegName(addrR)}, RZ, {RegName(offR)}");
                    EmitCode($"    STOREI {RegName(reg)}, {RegName(addrR)}, 0");
                    FreeR(offR); FreeR(addrR);
                }
                return;
            }
            if(_globalSlots.TryGetValue(name,out int gs)){
                EmitAbsAddr(ADDRREG, (long)gs+idx);
                EmitCode($"    STOREI {RegName(reg)}, {RegName(ADDRREG)}, 0");return;
            }
            if(_globalLabels.TryGetValue(name,out string glbl)){
                EmitCode($"    LIMM {RegName(ADDRREG)},{glbl}");EmitCode($"    STOREI {RegName(reg)}, {RegName(ADDRREG)}, 0");return;
            }
            throw new Exception($"Undefined variable: {name}");
        }

        void EmitAbsAddr(int reg, long addr){
            if(addr>=-364&&addr<=364)
                EmitCode($"    LI {RegName(reg)},{addr}");
            else
                EmitCode($"    LIMM {RegName(reg)},{addr}");
        }

        int EmitString(string value){string lbl = Lbl("str");_stringsToEmit.Add((lbl, value));int r = AllocR();EmitCode($"    LIMM {RegName(r)},{lbl}");return r;}
        int EmitFloat(string value){string lbl = Lbl("flt");double d = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);T3Float tf = T3Float.FromDouble(d);Word18 w = tf.ToWord18();_floatsToEmit.Add((lbl, w));_fpuLive = true;int r = AllocR();EmitCode($"    LIMM {RegName(r)},{lbl}");EmitCode($"    FLW {RegName(r)},{RegName(r)}");return r;}
        static bool IsCmp(string op)=>op is"=="or"!="or"<"or">"or"<="or">=";
        long ParseInt(string v) => LiteralParser.ParseInt(v);
        int _nextReg=0;readonly Stack<int> _freeRegs = new();

        int AllocR(){
            while(_freeRegs.Count>0){int fr=_freeRegs.Pop();if(fr!=FP&&fr!=CALLREG&&fr!=RETREG&&fr!=ADDRREG)return fr;}
            while(true){if(_nextReg!=FP&&_nextReg!=CALLREG&&_nextReg!=RETREG&&_nextReg!=ADDRREG)break;_nextReg=(_nextReg+1)%9;}
            int r=_nextReg;_nextReg=(_nextReg+1)%9;return r;
        }
        void FreeR(int r){if(r>=0&&r<9&&r!=FP&&r!=CALLREG&&r!=RETREG&&r!=ADDRREG)_freeRegs.Push(r);}
        int Imm(long v){int r=AllocR();if(v>=-364&&v<=364)EmitCode($"    LI {RegName(r)},{v}");else EmitCode($"    LIMM {RegName(r)},{v}");return r;}
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