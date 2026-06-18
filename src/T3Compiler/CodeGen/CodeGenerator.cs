using System;using System.Collections.Generic;using System.Linq;using System.Text;using T3Compiler.Parser;using T3Simulator.Common;using TritTypes;
namespace T3Compiler.CodeGen
{
    public class CodeGenerator
    {
        readonly AstProgram _program;readonly StringBuilder _output;int _labelCounter;
        readonly Dictionary<string,int> _varSlots=new(),_varSizes=new();
        readonly Dictionary<string,List<int>> _arrDims=new();readonly Dictionary<string,List<FieldDef>> _structFields=new();
        readonly Stack<(string brk,string cont)> _loopStack=new();readonly Dictionary<string,List<FieldDef>> _structDefs=new();
        public CodeGenerator(AstProgram p){_program=p;_output=new();foreach(var s in p.Structs)_structDefs[s.Name]=s.Fields;}
        public string Generate(){Emit("; T→T3");Emit("__entry:");Emit("    LI RW,main");Emit("    CALL RW");Emit("    HALT");foreach(var f in _program.Functions)GenFunc(f);return _output.ToString();}
        void GenFunc(FunctionDef f){_varSlots.Clear();_varSizes.Clear();_arrDims.Clear();_structFields.Clear();_nextReg=3;_nextAddr=200;Emit($"{f.Name}:");foreach(var s in f.Body.Body)GenStmt(s);Emit("    RET");}
        void GenStmt(Statement s){switch(s){case ExpressionStmt e:if(e.Expression!=null)GenExpr(e.Expression);break;case VarDeclaration vd:Alloc(vd.Name,vd.Type);if(vd.Type.StructName!=null&&_structDefs.TryGetValue(vd.Type.StructName,out var sf))_structFields[vd.Name]=sf;if(vd.Initializer!=null){int r=GenExpr(vd.Initializer);Store(vd.Name,r,0);}break;case ReturnStmt rs:if(rs.Value!=null){int r=GenExpr(rs.Value);Emit($"    MOV R2,{RegName(r)}");}Emit("    RET");break;case CompoundStmt cs:foreach(var ss in cs.Body)GenStmt(ss);break;case IfStmt ifs:GenIf(ifs);break;case WhileStmt ws:GenWhile(ws);break;case ForStmt fs:if(fs.Init!=null)GenExpr(fs.Init);if(fs.Condition!=null)GenWhile(new WhileStmt{Condition=fs.Condition,Body=Compound(fs.Body,fs.Step)});else GenStmt(fs.Body);break;}}
        static CompoundStmt Compound(Statement b,AstNode? s){var l=new List<Statement>{b};if(s!=null)l.Add(new ExpressionStmt{Expression=s});return new CompoundStmt{Body=l};}
        void GenIf(IfStmt s){string le=Lbl("end"),lt=Lbl("then");if(s.Condition is BinaryOp bo){int a=GenExpr(bo.Left),b=GenExpr(bo.Right);Emit($"    CMP {RegName(a)},{RegName(b)}");JumpCond(bo.Operator,lt);if(s.ElseBody!=null)GenStmt(s.ElseBody);Jmp(le);Emit($"{lt}:");GenStmt(s.ThenBody);Jmp(le);}else{int c=GenExpr(s.Condition);Emit($"    LI {RegName(5)},0");Emit($"    CMP {RegName(c)},{RegName(5)}");JumpReg("JNE",lt);if(s.ElseBody!=null)GenStmt(s.ElseBody);Jmp(le);Emit($"{lt}:");GenStmt(s.ThenBody);Jmp(le);}Emit($"{le}:");}
        void GenWhile(WhileStmt s){string ll=Lbl("loop"),lb=Lbl("body"),le=Lbl("wend");_loopStack.Push((le,ll));Emit($"{ll}:");if(s.Condition is BinaryOp bo){int a=GenExpr(bo.Left),b=GenExpr(bo.Right);Emit($"    CMP {RegName(a)},{RegName(b)}");JumpCond(bo.Operator,lb);Jmp(le);}else{int c=GenExpr(s.Condition);Emit($"    LI {RegName(5)},0");Emit($"    CMP {RegName(c)},{RegName(5)}");JumpReg("JNE",lb);Jmp(le);}Emit($"{lb}:");GenStmt(s.Body);Jmp(ll);Emit($"{le}:");_loopStack.Pop();}
        void JumpCond(string op,string l){switch(op){case"==":JumpReg("JE",l);break;case"!=":JumpReg("JNE",l);break;case"<":JumpReg("JL",l);break;case">":JumpReg("JG",l);break;case"<=":JumpReg("JLE",l);break;case">=":JumpReg("JGE",l);break;}}
        void JumpReg(string cond,string l){int r=AllocR();Emit($"    LIMM {RegName(r)},{l}");Emit($"    {cond} {RegName(r)}");}
        void Jmp(string l){int r=AllocR();Emit($"    LIMM {RegName(r)},{l}");Emit($"    JMP {RegName(r)}");}
        int GenExpr(AstNode n)=>n switch{IntegerLiteral il=>Imm(ParseInt(il.Value)),Identifier id=>LoadV(id.Name,0),BooleanLiteral bl=>Imm(bl.Value),BinaryOp bo=>GenBin(bo),UnaryOp uo=>GenUn(uo),Assignment ass=>EmitAssign(ass),ArrayAccess aa=>EmitArrAccess(aa),FunctionCall fc=>EmitCall(fc),MemberAccess ma=>EmitMemAccess(ma),TernaryExpr te=>GenTernary(te),_=>Imm(0)};
        int GenTernary(TernaryExpr te){int cr=GenExpr(te.Condition),r=AllocR();Emit($"    LI {RegName(5)},0");Emit($"    CMP {RegName(cr)},{RegName(5)}");string lt=Lbl("t"),lm=Lbl("m"),ld=Lbl("d");JumpReg("JG",lt);JumpReg("JE",lm);int fR=GenExpr(te.FalseExpr);Emit($"    MOV {RegName(r)},{RegName(fR)}");Jmp(ld);Emit($"{lm}:");int mR=GenExpr(te.MaybeExpr);Emit($"    MOV {RegName(r)},{RegName(mR)}");Jmp(ld);Emit($"{lt}:");int tR=GenExpr(te.TrueExpr);Emit($"    MOV {RegName(r)},{RegName(tR)}");Emit($"{ld}:");return r;}
        int EmitMemAccess(MemberAccess ma){if(ma.Object is Identifier id&&_varSlots.TryGetValue(id.Name,out int ba)&&_structFields.TryGetValue(id.Name,out var fl)){int off=fl.FindIndex(f=>f.Name==ma.MemberName);if(off<0)return Imm(0);int r=AllocR();EmitAddr(ba+off);Emit($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}return Imm(0);}
        void EmitMemStore(MemberAccess ma,int v){if(ma.Object is Identifier id&&_varSlots.TryGetValue(id.Name,out int ba)&&_structFields.TryGetValue(id.Name,out var fl)){int off=fl.FindIndex(f=>f.Name==ma.MemberName);if(off>=0){EmitAddr(ba+off);Emit($"    STORE {RegName(v)},{RegName(AddrReg)}");}}}
        int GenUn(UnaryOp uo){if(uo.Operator=="&"){if(uo.Operand is Identifier id&&_varSlots.TryGetValue(id.Name,out int a))return Imm(a);if(uo.Operand is MemberAccess ma&&ma.Object is Identifier id2&&_varSlots.TryGetValue(id2.Name,out int ba2)&&_structFields.TryGetValue(id2.Name,out var fl2)){int off=fl2.FindIndex(f=>f.Name==ma.MemberName);if(off>=0)return Imm(ba2+off);}if(uo.Operand is ArrayAccess aa){int arrB=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;int idx=FlatIdx(aa);int ra=AllocR();Emit($"    LIMM {RegName(ra)},{arrB}");Emit($"    ADD {RegName(ra)},{RegName(ra)},{RegName(idx)}");return ra;}return Imm(0);}if(uo.Operator=="*"){int pr=GenExpr(uo.Operand),r=AllocR();Emit($"    LOAD {RegName(r)},{RegName(pr)}");return r;}int o=GenExpr(uo.Operand),r2=AllocR();Emit($"    {(uo.Operator=="-"?"NEG":"MOV")} {RegName(r2)},{RegName(o)}");return r2;}
        int GenBin(BinaryOp bo)
        {
            if (IsCmp(bo.Operator))
            {
                int a = GenExpr(bo.Left);
                Emit($"    PUSH {RegName(a)}");
                int b = GenExpr(bo.Right);
                int ra = AllocR();
                Emit($"    POP {RegName(ra)}");
                int r = AllocR();
                Emit($"    CMP {RegName(ra)},{RegName(b)}");
                string lt = Lbl("t"), ld = Lbl("d");
                JumpCond(bo.Operator, lt);
                Emit($"    LI {RegName(r)},-1");
                Jmp(ld);
                Emit($"{lt}:");
                Emit($"    LI {RegName(r)},1");
                Emit($"{ld}:");
                return r;
            }
            if (bo.Right is IntegerLiteral il)
            {
                int a = GenExpr(bo.Left);
                int r = AllocR();
                long v = ParseInt(il.Value);
                string op = bo.Operator switch { "+" => "ADDI", "-" => "SUBI", "*" => "MULI", "/" => "DIVI", "%" => "MODI", _ => null };
                if (op != null)
                {
                    Emit($"    MOV {RegName(r)},{RegName(a)}");
                    Emit($"    {op} {RegName(r)},{v}");
                    return r;
                }
            }
            int a2 = GenExpr(bo.Left);
            Emit($"    PUSH {RegName(a2)}");
            int b2 = GenExpr(bo.Right);
            int ra2 = AllocR();
            Emit($"    POP {RegName(ra2)}");
            int r2 = AllocR();
            string op2 = bo.Operator switch { "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD", "&" => "AND", "|" => "OR", "^" => "XOR", "<<" => "SHL", ">>" => "SHR", _ => "ADD" };
            Emit($"    {op2} {RegName(r2)},{RegName(ra2)},{RegName(b2)}");
            return r2;
        }
        int EmitAssign(Assignment ass){int v;if(ass.Operator=="=")v=GenExpr(ass.Value);else{int lh=GenExpr(ass.Target),rh=GenExpr(ass.Value);string op=ass.Operator switch{"+="=>"ADD","-="=>"SUB","*="=>"MUL","/="=>"DIV","%="=>"MOD","&="=>"AND","|="=>"OR","^="=>"XOR","<<="=>"SHL",">>="=>"SHR",_=>"ADD"};v=AllocR();Emit($"    {op} {RegName(v)},{RegName(lh)},{RegName(rh)}");}if(ass.Target is Identifier id)Store(id.Name,v,0);else if(ass.Target is ArrayAccess aa)EmitArrStore(aa,v);else if(ass.Target is MemberAccess ma)EmitMemStore(ma,v);return v;}
        int EmitCall(FunctionCall fc){for(int i=fc.Arguments.Count-1;i>=0;i--)Emit($"    PUSH {RegName(GenExpr(fc.Arguments[i]))}");Emit($"    LI {RegName(5)},{fc.FunctionName}");Emit($"    CALL {RegName(5)}");for(int i=0;i<fc.Arguments.Count;i++)Emit($"    POP {RegName(4)}");int r=AllocR();Emit($"    MOV {RegName(r)},{RegName(6)}");return r;}
        int FlatIdx(ArrayAccess aa){if(_arrDims.TryGetValue(aa.ArrayName,out var dims)&&dims.Count>1&&aa.Indices.Count>=2){int iR=GenExpr(aa.Indices[0]),jR=GenExpr(aa.Indices[1]);int sR=AllocR();Emit($"    LI {RegName(sR)},{dims[1]}");int t=AllocR();Emit($"    MUL {RegName(t)},{RegName(iR)},{RegName(sR)}");int r=AllocR();Emit($"    ADD {RegName(r)},{RegName(t)},{RegName(jR)}");return r;}return GenExpr(aa.Indices[0]);}
        int EmitArrAccess(ArrayAccess aa){int ba=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;int off=FlatIdx(aa);EmitAddr(ba);Emit($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(off)}");int r=AllocR();Emit($"    LOAD {RegName(r)},{RegName(AddrReg)}");return r;}
        void EmitArrStore(ArrayAccess aa,int v){int ba=_varSlots.TryGetValue(aa.ArrayName,out int b)?b:_nextAddr;int off=FlatIdx(aa);EmitAddr(ba);Emit($"    ADD {RegName(AddrReg)},{RegName(AddrReg)},{RegName(off)}");Emit($"    STORE {RegName(v)},{RegName(AddrReg)}");}
        int _nextAddr=200;const int AddrReg=8;
        void EmitAddr(long addr){if(addr>=-364&&addr<=364)Emit($"    LI {RegName(AddrReg)},{addr}");else Emit($"    LIMM {RegName(AddrReg)},{addr}");}
        void Alloc(string name,TypeSpec ts){if(!_varSlots.ContainsKey(name)){_varSlots[name]=_nextAddr;int sz=1;if(ts.StructName!=null&&_structDefs.TryGetValue(ts.StructName,out var sf)){sz=sf.Count;_structFields[name]=sf;}else if(ts.Dims.Count>0){sz=ts.Dims.Aggregate(1,(a,b)=>a*b);_arrDims[name]=ts.Dims;}_varSizes[name]=sz;_nextAddr+=sz;}}
        int LoadV(string name,int idx){int r=AllocR();if(_varSlots.TryGetValue(name,out int a)){EmitAddr(a+idx);Emit($"    LOAD {RegName(r)},{RegName(AddrReg)}");}else Emit($"    LI {RegName(r)},0");return r;}
        void Store(string name,int reg,int idx){if(_varSlots.TryGetValue(name,out int a)){EmitAddr(a+idx);Emit($"    STORE {RegName(reg)},{RegName(AddrReg)}");}}
        static bool IsCmp(string op)=>op is"=="or"!="or"<"or">"or"<="or">=";
        long ParseInt(string v){if(v.StartsWith("0t"))return BalancedTernary.ParseToLong(v[2..].Replace("_",""));if(v.StartsWith("0y"))return P27(v[2..]);if(v.StartsWith("0n"))return P9(v[2..]);return long.TryParse(v,out long n)?n:0;}
        long P27(string s){var a="NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();string t="";foreach(char c in s.ToUpper()){int i=Array.IndexOf(a,c);if(i>=0)t+=TCh(i/9-1)+TCh(i/3%3-1)+TCh(i%3-1);}return BalancedTernary.ParseToLong(t);}
        long P9(string s){string t="";foreach(char c in s.ToUpper())t+=c switch{'W'=>"--",'X'=>"-0",'Y'=>"-+",'Z'=>"0-",'0'=>"00",'1'=>"0+",'2'=>"+-",'3'=>"+0",'4'=>"++",_=>"00"};return BalancedTernary.ParseToLong(t);}
        static string TCh(int t)=>t==-1?"-":t==1?"+":"0";
        int _nextReg=3;
        int AllocR(){while(_nextReg==5||_nextReg==6||_nextReg==8)_nextReg=(_nextReg+1)%9;int r=_nextReg;_nextReg=(_nextReg+1)%9;return r;}
        int Imm(long v){int r=AllocR();if(v>=-364&&v<=364)Emit($"    LI {RegName(r)},{v}");else Emit($"    LIMM {RegName(r)},{v}");return r;}
        string Lbl(string pfx)=>$"{pfx}_{_labelCounter++}";void Emit(string s="")=>_output.AppendLine(s);
        string RegName(int i) => i switch {
            0 => "RW", 1 => "RX", 2 => "RY", 3 => "RZ",
            4 => "R0", 5 => "R1", 6 => "R2", 7 => "R3", 8 => "R4",
            _ => throw new Exception($"Invalid register index {i}")
        };
    }
}