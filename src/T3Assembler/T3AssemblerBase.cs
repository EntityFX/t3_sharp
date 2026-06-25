using System;using System.Collections.Generic;using System.Globalization;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    /// <summary>9 regs (trit values): RW(-4) RX(-3) RY(-2) RZ(-1) R0(0) R1(1) R2(2) R3(3) R4(4). Phys = trit+4.</summary>
    public abstract class T3AssemblerBase
    {
        protected readonly T3Config _config;
        protected readonly Dictionary<string,int> _labels=new();
        protected readonly Dictionary<string, Int128> _constants=new();
        protected readonly List<string> _lines=new();
        public T3AssemblerBase(T3Config c){_config=c;}
        public abstract List<Int128> Assemble(string src);
        protected string CleanLine(string l){int c=l.IndexOf(';');if(c>=0)l=l[..c];return l.Trim();}
        protected int ResolveOperand(string t){
            if(IsRegister(t))return GetRegisterTrit(t);
            if(long.TryParse(t,out long v))return(int)v;
            if(_labels.TryGetValue(t,out int a))return a;
            throw new Exception($"Unknown: {t}");
        }
        protected List<Int128> ResolveString(string t){string s=t[1..^1];var r=new List<Int128>();foreach(char c in s)r.Add(TScii.FromChar(c));r.Add(0);return r;}
        protected Int128 ResolveOperandValue(string t){
            if(IsRegister(t))return GetRegisterTrit(t);
            if(_constants.TryGetValue(t,out var cv))return cv;
            if(long.TryParse(t,out long v))return v;
            if(_labels.TryGetValue(t,out int a))return a;
            if(t.StartsWith("t",StringComparison.OrdinalIgnoreCase))return BalancedTernary.ParseToInt128(t[1..]);
            if(t.StartsWith("0n",StringComparison.OrdinalIgnoreCase))return P9(t[2..]);
            if(t.StartsWith("0y",StringComparison.OrdinalIgnoreCase))return P27(t[2..]);
            // Expression evaluation: support +, -, *, /, %, <<, >>
            Int128 result = ResolveExpression(t);
            if(result != 0 || t=="0")return result;
            throw new Exception($"Unknown: {t}");
        }
        Int128 ResolveExpression(string expr){
            expr = expr.Replace(" ","").Replace("\t","");
            // Try simple binary expressions: left op right
            char[] ops = {'+','-','*','/'};
            for(int i=1;i<expr.Length-1;i++){
                char c=expr[i];
                if(Array.IndexOf(ops,c)>=0){
                    string left=expr[..i],right=expr[(i+1)..];
                    Int128 lv=ResolveSimple(left);
                    Int128 rv=ResolveSimple(right);
                    return c switch{
                        '+'=>lv+rv,
                        '-'=>lv-rv,
                        '*'=>lv*rv,
                        '/'=>(rv==0?throw new DivideByZeroException():lv/rv),
                        _=>0
                    };
                }
            }
            return ResolveSimple(expr);
        }
        Int128 ResolveSimple(string t){
            if(long.TryParse(t,out long v))return v;
            if(_constants.TryGetValue(t,out var cv))return cv;
            if(_labels.TryGetValue(t,out int a))return a;
            if(t.StartsWith("0t",StringComparison.OrdinalIgnoreCase))return BalancedTernary.ParseToInt128(t[2..]);
            if(t.StartsWith("0n",StringComparison.OrdinalIgnoreCase))return P9(t[2..]);
            if(t.StartsWith("0y",StringComparison.OrdinalIgnoreCase))return P27(t[2..]);
            return 0;
        }
        Int128 P9(string t){string r="";foreach(char c in t.ToUpper())r+=c switch{'W'=>"--",'X'=>"-0",'Y'=>"-+",'Z'=>"0-",'0'=>"00",'1'=>"0+",'2'=>"+-",'3'=>"+0",'4'=>"++",_=>throw new FormatException($"Unknown 0n character: {c}")};return BalancedTernary.ParseToInt128(r);}
        Int128 P27(string t){string r="";foreach(char c in t.ToUpper())r+=c switch{'N'=>"---",'O'=>"--0",'P'=>"--+",'Q'=>"-0-",'R'=>"-00",'S'=>"-0+",'T'=>"-+-",'U'=>"-+0",'V'=>"-++",'5'=>"+--",'6'=>"+-0",'7'=>"+-+",'8'=>"+0-",'9'=>"+00",'A'=>"+0+",'B'=>"++-",'C'=>"++0",'D'=>"+++",_=>throw new FormatException($"Unknown 0y character: {c}")};return BalancedTernary.ParseToInt128(r);}

        protected bool IsRegister(string t){
            string u=t.ToUpper();
            return u is"RW"or"RX"or"RY"or"RZ"or"FW"or"FX"or"FY"or"FZ"
                ||(u.StartsWith('R')&&int.TryParse(u[1..],out int i)&&i>=0&&i<=4)
                ||(u.StartsWith('F')&&int.TryParse(u[1..],out int j)&&j>=0&&j<=4)
                ||(u.Length==1&&u[0]>='A'&&u[0]<='I');
        }

        /// <summary>Returns TRIT value (-4..+4). Phys index = trit + 4.</summary>
        protected int GetRegisterTrit(string t){
            string u=t.ToUpper();
            if(u.StartsWith('R')&&int.TryParse(u[1..],out int i)&&i>=0&&i<=4)return i;
            if(u.StartsWith('F')&&int.TryParse(u[1..],out int j)&&j>=0&&j<=4)return j;
            if(u.Length==1&&u[0]>='A'&&u[0]<='I')return (""+u[0])switch{"A"=>-4,"B"=>-3,"C"=>-2,"D"=>-1,"E"=>0,"F"=>1,"G"=>2,"H"=>3,"I"=>4,_=>0};
            return u switch{"RW"or"FW"=>-4,"RX"or"FX"=>-3,"RY"or"FY"=>-2,"RZ"or"FZ"=>-1,_=>throw new Exception($"Unknown register: {t}")};
        }

        /// <summary>Returns PHYS index (0..8).</summary>
        protected int GetRegisterIndex(string t)=>GetRegisterTrit(t)+4;

        protected Opcode GetOpcode(string m)=>m switch{
            "HALT"=>Opcode.HALT,"LOAD"=>Opcode.LOAD,"LOADI"=>Opcode.LOADI,"STORE"=>Opcode.STORE,"STOREI"=>Opcode.STOREI,
            "MOV"=>Opcode.MOV,"MOVI"=>Opcode.MOVI,"LI"=>Opcode.LI,"LIMM"=>Opcode.LIMM,
            "ADD"=>Opcode.ADD,"ADDI"=>Opcode.ADDI,"SUB"=>Opcode.SUB,"SUBI"=>Opcode.SUBI,
            "MUL"=>Opcode.MUL,"MULI"=>Opcode.MULI,"DIV"=>Opcode.DIV,"DIVI"=>Opcode.DIVI,"MOD"=>Opcode.MOD,"MODI"=>Opcode.MODI,
            "NEG"=>Opcode.NEG,"NEGI"=>Opcode.NEGI,"AND"=>Opcode.AND,"TRITAND"=>Opcode.AND,"ANDI"=>Opcode.ANDI,
            "OR"=>Opcode.OR,"TRITOR"=>Opcode.OR,"ORI"=>Opcode.ORI,"XOR"=>Opcode.XOR,"TRITXOR"=>Opcode.XOR,"XORI"=>Opcode.XORI,
            "SHL"=>Opcode.SHL,"SHLI"=>Opcode.SHLI,"SHR"=>Opcode.SHR,"SHRI"=>Opcode.SHRI,
            "CMP"=>Opcode.CMP,"CMPI"=>Opcode.CMPI,"JMP"=>Opcode.JMP,"JE"=>Opcode.JE,"JNE"=>Opcode.JNE,
            "JL"=>Opcode.JL,"JG"=>Opcode.JG,"JLE"=>Opcode.JLE,"JGE"=>Opcode.JGE,"JM"=>Opcode.JM,
            "CALL"=>Opcode.CALL,"RET"=>Opcode.RET,"PUSH"=>Opcode.PUSH,"POP"=>Opcode.POP,
            "IN"=>Opcode.IN,"OUT"=>Opcode.OUT,"INI"=>Opcode.INI,"OUTI"=>Opcode.OUTI,
            "FADD"=>Opcode.FADD,"FSUB"=>Opcode.FSUB,"FMUL"=>Opcode.FMUL,"FDIV"=>Opcode.FDIV,
            "FSQRT"=>Opcode.FSQRT,"FABS"=>Opcode.FABS,"FNEG"=>Opcode.FNEG,"FCMP"=>Opcode.FCMP,
            "FTOI"=>Opcode.FTOI,"ITOF"=>Opcode.ITOF,"FTOF"=>Opcode.FTOF,
            "FLW"=>Opcode.FLW,"FSW"=>Opcode.FSW,"FMOV"=>Opcode.FMOV,"FCLASS"=>Opcode.FCLASS,"FSWAP"=>Opcode.FSWAP,
            "NOP"=>Opcode.NOP,"FZERO"=>Opcode.FZERO,_=>throw new Exception($"Unknown: {m}")
        };
    }
}