using System;using System.Collections.Generic;using System.Linq;using System.Text;using T3Compiler.Parser;using T3Simulator.Common;using TritTypes;
namespace T3Compiler.CodeGen
{
    public class CodeGenerator
    {
        readonly AstProgram _program;readonly StringBuilder _output;int _labelCounter;
        readonly Dictionary<string,int> _varSlots=new(),_varSizes=new();
        readonly Dictionary<string,int> _enumConstants=new();
        readonly List<(string label, string value)> _stringsToEmit=new();
        readonly Dictionary<string,List<int>> _arrDims=new();readonly Dictionary<string,List<FieldDef>> _structFields=new();
        readonly Stack<(string brk,string cont)> _loopStack=new();readonly Dictionary<string,List<FieldDef>> _structDefs=new();
        string? _epilogueLabel;
        public CodeGenerator(AstProgram p){_program=p;_output=new();foreach(var s in p.Structs)_structDefs[s.Name]=s.Fields;}
        public string Generate(){
            foreach(var ed in _program.Enums){
                int cur=0;
                foreach(var m in ed.Members){
                    int v=m.Value??cur;
                    _enumConstants[m.Name]=v;
                    cur=v+1;
                }
            }
            Emit("; T→T3");Emit("__entry:");Emit("    LI RW,main");Emit("    CALL RW");Emit("    HALT");foreach(var f in _program.Functions)GenFunc(f);
            
            Emit("\n; --- Data Section ---");
            foreach(var (lbl, val) in _stringsToEmit){
                string data = string.Join(", ", val.Select(c => (long)TritTypes.TScii.FromChar(c)));
                Emit($"{lbl}: .word {val.Length}, {data}");
            }
            
            Emit("\n; --- StdLib ---");
            Emit("strlen:");
            Emit("    POP R1"); // Assume address is pushed as arg
            Emit("    LOAD R2,R1");
            Emit("    PUSH R1"); // restore stack if needed, but actually just return R2
            Emit("    MOV R2,R2"); // essentially return R2
            Emit("    RET");
            
            return _output.ToString();}
        
        void GenFunc(FunctionDef f){
            _varSlots.Clear();_varSizes.Clear();_arrDims.Clear();_structFields.Clear();_nextReg=3;
            _epilogueLabel = Lbl("epilogue");
            Emit($"{f.Name}:");
            // Prologue: save return address in R2, pop parameters, then push return address back
            // The caller pushes args first, then saves regs, then CALL.
            // Stack at entry: [caller's saved regs] [args...] [ret addr]
            // We need to pop ret addr first, then args, then push ret addr back, then save our regs.
            if (f.Parameters.Count > 0) {
                Emit("    POP R2");  // save return address in R2 (not saved/restored)
                // Pop parameters in forward order (they were pushed in reverse order by caller)
                // Use R3 (index 7) as temp register for parameter values
                foreach (var param in f.Parameters) {
                    Alloc(param.Name, param.Type);
                    Emit("    POP R3");
                    Store(param.Name, 7, 0);  // R3 = index 7
                }
                Emit("    PUSH R2");  // push return address back
            }
            // Save callee-saved registers (all except R2 which is the return value register)
            Emit("    PUSH RW");Emit("    PUSH RX");Emit("    PUSH RY");Emit("    PUSH RZ");
            Emit("    PUSH R0");Emit("    PUSH R1");Emit("    PUSH R3");Emit("    PUSH R4");
            foreach(var s in f.Body.Body)GenStmt(s);
            // Epilogue: restore callee-saved registers (R2 is the return value, not saved/restored)
            Emit($"{_epilogueLabel}:");
            Emit("    POP R4");Emit("    POP R3");Emit("    POP R1");Emit("    POP R0");
            Emit("    POP RZ");Emit("    POP RY");Emit("    POP RX");Emit("    POP RW");
            Emit("    RET");
        }
        
        void GenStmt(Statement s){switch(s){case ExpressionStmt e:if(e.Expression!=null)GenExpr(e.Expression);break;case VarDeclaration vd:Alloc(vd.Name,vd.Type);if(vd.Type.StructName!=null&&_structDefs.TryGetValue(vd.Type.StructName,out var sf))_structFields[vd.Name]=sf;if(vd.Initializer!=null){int r=GenExpr(vd.Initializer);Store(vd.Name,r,0);}break;case ReturnStmt rs:if(rs.Value!=null){int r=GenExpr(rs.Value);Emit($"    MOV R2,{RegName(r)}");}if(_epilogueLabel!=null){int rj=AllocR();Emit($"    LIMM {RegName(rj)},{_epilogueLabel}");Emit($"    JMP {RegName(rj)}");}else Emit("    RET");break;case CompoundStmt cs:foreach(var ss in cs.Body)GenStmt(ss);break;case IfStmt ifs:GenIf(ifs);break;case WhileStmt ws:GenWhile(ws);break;case DoWhileStmt dws:GenDoWhile(dws);break;case ForStmt fs:GenFor(fs);break;case SwitchStmt ss:GenSwitch(ss);break;case BreakStmt _:{var(brk,_)=_loopStack.Peek();Jmp(brk);}break;case ContinueStmt _:{var(_,cont)=_loopStack.Peek();Jmp(cont);}break;}}
        
        void GenIf(IfStmt s){
            string le=Lbl("end"),lt=Lbl("then");
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                Emit($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,lt);
                if(s.ElseBody!=null)GenStmt(s.ElseBody);
                Jmp(le);
                Emit($"{lt}:");GenStmt(s.ThenBody);
                Emit($"{le}:");
            }else{
                int c=GenExpr(s.Condition);
                Emit($"    LI R2,0");
                Emit($"    CMP {RegName(c)},R2");
                JumpReg("JNE",lt);
                if(s.ElseBody!=null)GenStmt(s.ElseBody);
                Jmp(le);
                Emit($"{lt}:");GenStmt(s.ThenBody);
                Emit($"{le}:");
            }
        }
        
        void GenWhile(WhileStmt s){
            string ll=Lbl("loop"),lb=Lbl("body"),le=Lbl("wend");
            _loopStack.Push((le,ll));
            Emit($"{ll}:");
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                Emit($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,lb);
                Jmp(le);
            }else{
                int c=GenExpr(s.Condition);
                Emit($"    LI R2,0");
                Emit($"    CMP {RegName(c)},R2");
                JumpReg("JNE",lb);
                Jmp(le);
            }
            Emit($"{lb}:");GenStmt(s.Body);Jmp(ll);
            Emit($"{le}:");_loopStack.Pop();
        }
        
        void GenDoWhile(DoWhileStmt s){
            string ll=Lbl("loop"),le=Lbl("wend");
            _loopStack.Push((le,ll));
            Emit($"{ll}:");
            GenStmt(s.Body);
            if(s.Condition is BinaryOp bo){
                int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                Emit($"    CMP {RegName(a)},{RegName(b)}");
                JumpCond(bo.Operator,ll);
            }else{
                int c=GenExpr(s.Condition);
                Emit($"    LI R2,0");
                Emit($"    CMP {RegName(c)},R2");
                JumpReg("JNE",ll);
            }
            Emit($"{le}:");
            _loopStack.Pop();
        }
        
        void GenFor(ForStmt fs){
            string ll=Lbl("floop"),lb=Lbl("fbody"),le=Lbl("fend");
            _loopStack.Push((le,ll));
            if(fs.Init!=null) GenStmt(fs.Init);
            Emit($"{ll}:");
            if(fs.Condition!=null){
                if(fs.Condition is BinaryOp bo){
                    int a=GenExpr(bo.Left);int b=GenExpr(bo.Right);
                    Emit($"    CMP {RegName(a)},{RegName(b)}");
                    JumpCond(bo.Operator,lb);
                }else{
                    int c=GenExpr(fs.Condition);
                    Emit($"    LI R2,0");
                    Emit($"    CMP {RegName(c)},R2");
                    JumpReg("JNE",lb);
                }
                Jmp(le);
            }
            Emit($"{lb}:");
            GenStmt(fs.Body);
            if(fs.Step!=null) GenExpr(fs.Step);
            Jmp(ll);
            Emit($"{le}:");
            _loopStack.Pop();
        }
        
        void GenSwitch(SwitchStmt s){
            int exprReg = GenExpr(s.Expression);
            string end = Lbl("swend");
            var labels = new List<string>();
            for (int i = 0; i < s.Cases.Count; i++)
                labels.Add(Lbl("scase"));
            // Find default case index
            int defaultIdx = -1;
            for (int i = 0; i < s.Cases.Count; i++) {
                if (s.Cases[i].Value == null) { defaultIdx = i; continue; }
                int caseVal = GenExpr(s.Cases[i].Value);
                Emit($"    CMP {RegName(exprReg)},{RegName(caseVal)}");
                JumpReg("JE", labels[i]);
            }
            // Jump to default or end
            if (defaultIdx >= 0)
                Jmp(labels[defaultIdx]);
            else
                Jmp(end);
            // Generate case bodies
            for (int i = 0; i < s.Cases.Count; i++) {
                Emit($"{labels[i]}:");
                foreach (var stmt in s.Cases[i].Body)
                    GenStmt(stmt);
                Jmp(end);  // break after each case
            }
            Emit($"{end}:");
        }
        
        void JumpCond(string op,string l){switch(op){case"==":JumpReg("JE",l);break;case"!=":JumpReg("JNE",l);break;case"<":JumpReg("JL",l);break;case">":JumpReg("JG",l);break;case"<=":JumpReg("JLE",l);break;case">=":JumpReg("JGE",l);break;}}
        void JumpCondInv(string op,string l){switch(op){case"==":JumpReg("JNE",l);break;case"!=":JumpReg("JE",l);break;case"<":JumpReg("JGE",l);break;case">":JumpReg("JLE",l);break;case"<=":JumpReg("JG",l);break;case">=":JumpReg("JL",l);break;}}
        void JumpReg(string cond,string l){int r=AllocR();Emit($"    LIMM {RegName(r)},{l}");Emit($"    {cond} {RegName(r)}");}
        void Jmp(string l){int r=AllocR();Emit($"    LIMM {RegName(r)},{l}");Emit($"    JMP {RegName(r)}");}
        
        int GenExpr(AstNode n){
            if(n is IntegerLiteral il) return Imm(ParseInt(il.Value));
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
            Emit($"    LI R2,0");
            Emit($"    CMP {RegName(cr)},R2");
            string lt=Lbl("t"),lm=Lbl("m"),ld=Lbl("d");
            JumpReg("JG",lt);JumpReg("JE",lm);
            int fR=GenExpr(te.FalseExpr);Emit($"    MOV {RegName(r)},{RegName(fR)}");Jmp(ld);
            Emit($"{lm}:");int mR=GenExpr(te.MaybeExpr);Emit($"    MOV {RegName(r)},{RegName(mR)}");Jmp(ld);
            Emit($"{lt}:");int tR=GenExpr(te.TrueExpr);Emit($"    MOV {RegName(r)},{RegName(tR)}");
            Emit($"{ld}:");return r;
        }
        
        int EmitMemAccess(MemberAccess ma){
            // Case 1: struct variable field access (e.g., p.x)
            if(ma.Object is Identifier id&&_varSlots.TryGetValue(id.Name,out int ba)&&_structFields.TryGetValue(id.Name,out var fl)){
                int off=fl.FindIndex(f=>f.Name==ma.MemberName);
                if(off<0)throw new Exception($"Unknown field: {ma.MemberName}");
                int r=AllocR();EmitAddr(ba+off);Emit($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;
            }
            // Case 2: struct array element field access (e.g., pts[0].x)
            if(ma.Object is ArrayAccess aa&&_structFields.TryGetValue(aa.ArrayName,out var fl2)){
                int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                if(off<0)throw new Exception($"Unknown field: {ma.MemberName}");
                int ba2=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;
                int idx=FlatIdx(aa);
                int r=AllocR();EmitAddr(ba2+off);Emit($"    ADD {RegName(r)},{RegName(AddrReg)},{RegName(idx)}");Emit($"    LOAD {RegName(r)},{RegName(r)}");return r;
            }
            // Case 3: pointer to struct field access via dereference (e.g., (*pp).first)
            // The parser creates UnaryOp("*", Identifier) for *pp.
            // For struct member access, we need the address (pointer value), not the dereferenced value.
            // So we use the identifier directly (it holds the address) and add the field offset.
            if(ma.Object is UnaryOp uo&&uo.Operator=="*"&&uo.Operand is Identifier ptrId){
                // Find the struct definition that contains this field
                foreach(var kv in _structDefs){
                    int off=kv.Value.FindIndex(f=>f.Name==ma.MemberName);
                    if(off>=0){
                        // ptrId holds the address of the struct
                        int ptrR=GenExpr(uo.Operand); // this does LOAD to get the pointer value
                        // But we need the address, not the value at the address!
                        // The pointer variable holds the address, so we load the pointer value
                        // Actually, GenExpr for Identifier loads the VALUE of the variable.
                        // For a pointer, the value IS the address.
                        // So ptrR holds the address of the struct.
                        // Now add field offset and load
                        int r=AllocR();
                        Emit($"    LI {RegName(AddrReg)},{off}");
                        Emit($"    ADD {RegName(r)},{RegName(ptrR)},{RegName(AddrReg)}");
                        Emit($"    LOAD {RegName(r)},{RegName(r)}");
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
                if(off>=0){EmitAddr(ba+off);Emit($"    STORE {RegName(v)},{RegName(AddrReg)}");}
            }
            // Case 2: struct array element field store
            if(ma.Object is ArrayAccess aa&&_structFields.TryGetValue(aa.ArrayName,out var fl2)){
                int off=fl2.FindIndex(f=>f.Name==ma.MemberName);
                if(off>=0){
                    int ba2=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;
                    Emit($"    PUSH {RegName(v)}");
                    int idx=FlatIdx(aa);
                    EmitAddr(ba2+off);
                    Emit($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(idx)}");
                    int v_pop=AllocR();
                    Emit($"    POP {RegName(v_pop)}");
                    Emit($"    STORE {RegName(v_pop)},{RegName(AddrReg)}");
                }
            }
            // Case 3: pointer to struct field store via dereference (e.g., (*pp).first = x)
            if(ma.Object is UnaryOp uo&&uo.Operator=="*"&&uo.Operand is Identifier ptrId){
                foreach(var kv in _structDefs){
                    int off=kv.Value.FindIndex(f=>f.Name==ma.MemberName);
                    if(off>=0){
                        int ptrR=GenExpr(uo.Operand);
                        Emit($"    LI {RegName(AddrReg)},{off}");
                        Emit($"    ADD {RegName(AddrReg)},{RegName(ptrR)},{RegName(AddrReg)}");
                        Emit($"    STORE {RegName(v)},{RegName(AddrReg)}");
                        return;
                    }
                }
            }
        }
        
        int GenUn(UnaryOp uo){
            if(uo.Operator=="&"){
                if(uo.Operand is Identifier id&&_varSlots.TryGetValue(id.Name,out int a))return Imm(a);
                if(uo.Operand is MemberAccess ma&&ma.Object is Identifier id2&&_varSlots.TryGetValue(id2.Name,out int ba2)&&_structFields.TryGetValue(id2.Name,out var fl2)){int off=fl2.FindIndex(f=>f.Name==ma.MemberName);if(off>=0)return Imm(ba2+off);}
                if(uo.Operand is ArrayAccess aa){int arrB=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;int idx=FlatIdx(aa);int ra=AllocR();Emit($"    LIMM {RegName(ra)},{arrB}");Emit($"    ADD {RegName(ra)},{RegName(ra)},{RegName(idx)}");return ra;}
                throw new Exception($"Cannot take address of {uo.Operand?.GetType().Name}");
            }
            if(uo.Operator=="*"){int pr=GenExpr(uo.Operand),r=AllocR();Emit($"    LOAD {RegName(r)},{RegName(pr)}");return r;}
            int o=GenExpr(uo.Operand),r2=AllocR();Emit($"    {(uo.Operator=="-"?"NEG":"MOV")} {RegName(r2)},{RegName(o)}");return r2;
        }
        
        int GenBin(BinaryOp bo)
        {
            int l1 = GenExpr(bo.Left);
            Emit($"    PUSH {RegName(l1)}");
            int r1 = GenExpr(bo.Right);
            int l2 = AllocR();
            while (l2 == r1) l2 = AllocR();
            Emit($"    POP {RegName(l2)}");

            if (IsCmp(bo.Operator))
            {
                int resReg = AllocR();
                Emit($"    CMP {RegName(l2)},{RegName(r1)}");
                string lt = Lbl("t"), ld = Lbl("d");
                JumpCond(bo.Operator, lt);
                Emit($"    LI {RegName(resReg)},-1");
                Jmp(ld);
                Emit($"{lt}:");
                Emit($"    LI {RegName(resReg)},1");
                Emit($"{ld}:");
                return resReg;
            }
            int resultReg = AllocR();
            while (resultReg == l2 || resultReg == r1) resultReg = AllocR();
            string op = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => throw new NotSupportedException($"Unsupported binary operator: {bo.Operator}") };
            Emit($"    {op} {RegName(resultReg)},{RegName(l2)},{RegName(r1)}");
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
                Emit($"    PUSH {RegName(lh)}");
                int rh = GenExpr(ass.Value);
                int r_lh = AllocR();
                while (r_lh == rh) r_lh = AllocR();
                Emit($"    POP {RegName(r_lh)}");
                string op = ass.Operator switch { "+=" => "ADD", "-=" => "SUB", "*=" => "MUL", "/=" => "DIV", "%=" => "MOD", "&=" => "AND", "|=" => "OR", "^=" => "XOR", "<<=" => "SHL", ">>=" => "SHR", _ => throw new NotSupportedException($"Unsupported assignment operator: {ass.Operator}") };
                v = AllocR();
                while (v == rh || v == r_lh) v = AllocR();
                Emit($"    {op} {RegName(v)},{RegName(r_lh)},{RegName(rh)}");
            }
            if (ass.Target is Identifier id) Store(id.Name, v, 0);
            else if (ass.Target is ArrayAccess aa) EmitArrStore(aa, v);
            else if (ass.Target is MemberAccess ma) EmitMemStore(ma, v);
            return v;
        }
        
        int EmitCall(FunctionCall fc){
            // Save caller-saved registers FIRST (RW, RX, RY, RZ, R0, R1, R3, R4)
            // Then push arguments in reverse order.
            // Stack at function entry: [caller's saved regs] [args...] [ret addr]
            // Callee pops ret addr, then args (on top), pushes ret addr back, saves regs.
            Emit("    PUSH RW");Emit("    PUSH RX");Emit("    PUSH RY");Emit("    PUSH RZ");
            Emit("    PUSH R0");Emit("    PUSH R1");Emit("    PUSH R3");Emit("    PUSH R4");
            for(int i=fc.Arguments.Count-1;i>=0;i--)Emit($"    PUSH {RegName(GenExpr(fc.Arguments[i]))}");
            Emit($"    LI R1,{fc.FunctionName}");
            Emit("    CALL R1");
            // After RET, stack has: [caller's saved regs] — callee popped args before saving regs
            // Pop args (they were pushed after saved regs, but callee popped them, so we just pop saved regs)
            // Actually, callee pops args before saving its own regs, so after RET:
            // Stack: [caller's saved regs] — we restore them below
            // No args to pop — callee already popped them
            // Restore caller-saved registers
            Emit("    POP R4");Emit("    POP R3");Emit("    POP R1");Emit("    POP R0");
            Emit("    POP RZ");Emit("    POP RY");Emit("    POP RX");Emit("    POP RW");
            int r=AllocR();Emit($"    MOV {RegName(r)},R2");return r;
        }
        
        int FlatIdx(ArrayAccess aa)
        {
            if (!_arrDims.TryGetValue(aa.ArrayName, out var dims) || dims.Count <= 1)
                return GenExpr(aa.Indices[0]);

            int r = AllocR();
            Emit($"    LI {RegName(r)},0");
            for (int i = 0; i < dims.Count; i++)
            {
                Emit($"    PUSH {RegName(r)}");
                int idxR = GenExpr(aa.Indices[i]);
                int stride = 1;
                for (int j = i + 1; j < dims.Count; j++) stride *= dims[j];
                int sR = AllocR();
                while (sR == idxR) sR = AllocR();
                Emit($"    LI {RegName(sR)},{stride}");
                
                int t = AllocR();
                while (t == idxR || t == sR) t = AllocR();
                Emit($"    MUL {RegName(t)},{RegName(idxR)},{RegName(sR)}");
                
                int r_restored = AllocR();
                while (r_restored == t) r_restored = AllocR();
                Emit($"    POP {RegName(r_restored)}");
                
                int nextR = AllocR();
                while (nextR == r_restored || nextR == t) nextR = AllocR();
                Emit($"    ADD {RegName(nextR)},{RegName(r_restored)},{RegName(t)}");
                r = nextR;
            }
            return r;
        }
        
        int EmitArrAccess(ArrayAccess aa){int ba=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;int off=FlatIdx(aa);EmitAddr(ba);Emit($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(off)}");int r=AllocR();Emit($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
        void EmitArrStore(ArrayAccess aa,int v)
        {
            int ba = _varSlots.TryGetValue(aa.ArrayName, out int b) ? b : _nextAddr;
            Emit($"    PUSH {RegName(v)}");
            int off = FlatIdx(aa);
            EmitAddr(ba);
            Emit($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(off)}");
            int v_pop = AllocR();
            Emit($"    POP {RegName(v_pop)}");
            Emit($"    STORE {RegName(v_pop)},{RegName(AddrReg)}");
        }
        
        int _nextAddr=300;const int AddrReg=8;
        void EmitAddr(long addr){if(addr>=-364&&addr<=364)Emit($"    LI {RegName(AddrReg)},{addr}");else Emit($"    LIMM {RegName(AddrReg)},{addr}");}
        void Alloc(string name,TypeSpec ts){if(!_varSlots.ContainsKey(name)){_varSlots[name]=_nextAddr;int sz=1;if(ts.StructName!=null&&_structDefs.TryGetValue(ts.StructName,out var sf)){sz=sf.Count;_structFields[name]=sf;}else if(ts.Dims.Count>0){sz=ts.Dims.Aggregate(1,(a,b)=>a*b);_arrDims[name]=ts.Dims;}_varSizes[name]=sz;_nextAddr+=sz;}}
        
        int LoadV(string name,int idx){
            int r=AllocR();
            if(_varSlots.TryGetValue(name,out int a)){EmitAddr(a+idx);Emit($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
            throw new Exception($"Undefined variable: {name}");
        }
        
        void Store(string name,int reg,int idx){if(_varSlots.TryGetValue(name,out int a)){EmitAddr(a+idx);Emit($"    STORE {RegName(reg)},{RegName(AddrReg)}");}}

        int EmitString(string value)
        {
            string lbl = Lbl("str");
            // T-SCII format: [Length (1 word)] [Data (N traits)]
            // In T3 assembly, we can use .word to define these.
            _stringsToEmit.Add((lbl, value));
            
            // Return the address of the string (the label)
            return Imm(0); // Placeholder, but usually we want the label as a value.
            // Actually, we need to emit a load of the label.
        }
        static bool IsCmp(string op)=>op is"=="or"!="or"<"or">"or"<="or">=";
        long ParseInt(string v){if(v.StartsWith("0t"))return BalancedTernary.ParseToLong(v[2..].Replace("_",""));if(v.StartsWith("0y"))return P27(v[2..]);if(v.StartsWith("0n"))return P9(v[2..]);return long.TryParse(v,out long n)?n:0;}
        long P27(string s){var a="NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();string t="";foreach(char c in s.ToUpper()){int i=Array.IndexOf(a,c);if(i>=0)t+=TCh(i/9-1)+TCh(i/3%3-1)+TCh(i%3-1);else throw new FormatException($"Unknown 0y character: {c}");}return BalancedTernary.ParseToLong(t);}
        long P9(string s){string t="";foreach(char c in s.ToUpper())t+=c switch{'W'=>"--",'X'=>"-0",'Y'=>"-+",'Z'=>"0-",'0'=>"00",'1'=>"0+",'2'=>"+-",'3'=>"+0",'4'=>"++",_=>throw new FormatException($"Unknown 0n character: {c}")};return BalancedTernary.ParseToLong(t);}
        static string TCh(int t)=>t==-1?"-":t==1?"+":"0";
        int _nextReg=3;
        int AllocR(){while(_nextReg==5||_nextReg==6||_nextReg==8)_nextReg=(_nextReg+1)%9;int r=_nextReg;_nextReg=(_nextReg+1)%9;return r;}
        int Imm(long v){int r=AllocR();if(v>=-364&&v<=364)Emit($"    LI {RegName(r)},{v}");else {Emit($"    LIMM {RegName(r)},{v}");}return r;}
        string Lbl(string pfx)=>$"{pfx}_{_labelCounter++}";void Emit(string s="")=>_output.AppendLine(s);
        string RegName(int i) => i switch {
            0 => "RW", 1 => "RX", 2 => "RY", 3 => "RZ",
            4 => "R0", 5 => "R1", 6 => "R2", 7 => "R3", 8 => "R4",
            _ => throw new Exception($"Invalid register index {i}")
        };
    }
}