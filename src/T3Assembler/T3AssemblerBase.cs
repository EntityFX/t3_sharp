using System;using System.Collections.Generic;using System.Globalization;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    public abstract class T3AssemblerBase
    {
        protected const string RW="RW",RX="RX",RY="RY",RZ="RZ";
        protected const string R0="R0",R1="R1",R2="R2",R3="R3",R4="R4";
        protected const string FW="FW",FX="FX",FY="FY",FZ="FZ";
        protected const string F0="F0",F1="F1",F2="F2",F3="F3",F4="F4";

        protected readonly T3Config _config;
        protected readonly Dictionary<string,int> _labels=new();
        protected readonly Dictionary<string,Int128> _constants=new();
        protected readonly HashSet<string> _globals=new();
        protected readonly HashSet<string> _externs=new();
        protected readonly List<string> _lines=new();
        public T3AssemblerBase(T3Config c){_config=c;}
        public abstract List<Int128> Assemble(string src);
        protected string CleanLine(string l){int c=l.IndexOf(';');if(c>=0)l=l[..c];return l.Trim();}

        protected int ResolveOperand(string t){
            if(IsRegister(t))return GetRegisterRaw(t);
            if(long.TryParse(t,out long v))return(int)v;
            if(_labels.TryGetValue(t,out int a))return a;
            throw new Exception($"Unknown: {t}");
        }
        protected List<Int128> ResolveString(string t){string s=t[1..^1];var r=new List<Int128>();foreach(char c in s)r.Add(TScii.FromChar(c));r.Add(0);return r;}
        protected string ProcessIncludes(string src,string baseDir){
            var sb=new System.Text.StringBuilder();
            foreach(var line in src.Split(new[]{"\r\n","\r","\n"},System.StringSplitOptions.None)){
                string t=line.Trim();
                if(t.StartsWith("#include")||t.StartsWith(".include")){
                    int q1=t.IndexOf('"');int q2=t.IndexOf('"',q1+1);
                    if(q2<0){q1=t.IndexOf('<');q2=t.IndexOf('>');}
                    if(q1>=0&&q2>q1){
                        string incName=t.Substring(q1+1,q2-q1-1);
                        string incPath=System.IO.Path.Combine(baseDir,incName);
                        if(System.IO.File.Exists(incPath)){
                            string included=System.IO.File.ReadAllText(incPath);
                            sb.AppendLine(ProcessIncludes(included,System.IO.Path.GetDirectoryName(incPath)!));
                        }
                    }
                }else sb.AppendLine(line);
            }
            return sb.ToString();
        }
        protected Int128 ResolveOperandValue(string t){
            if(IsRegister(t))return GetRegisterRaw(t);
            if(_constants.TryGetValue(t,out var cv))return cv;
            if(long.TryParse(t,out long v))return v;
            if(_labels.TryGetValue(t,out int a))return a;
            if(t.StartsWith("t",StringComparison.OrdinalIgnoreCase))return BalancedTernary.ParseToInt128(t[1..]);
            if(t.StartsWith("0n",StringComparison.OrdinalIgnoreCase))return P9(t[2..]);
            if(t.StartsWith("0y",StringComparison.OrdinalIgnoreCase))return P27(t[2..]);
            Int128 result=ResolveExpression(t);
            if(result!=0||t=="0")return result;
            throw new Exception($"Unknown: {t}");
        }
        Int128 ResolveExpression(string expr){
            expr=expr.Replace(" ","").Replace("\t","");
            char[] ops={'+','-','*','/'};
            for(int i=1;i<expr.Length-1;i++){
                char c=expr[i];
                if(Array.IndexOf(ops,c)>=0){
                    string left=expr[..i],right=expr[(i+1)..];
                    Int128 lv=ResolveSimple(left),rv=ResolveSimple(right);
                    return c switch{'+'=>lv+rv,'-'=>lv-rv,'*'=>lv*rv,'/'=>(rv==0?throw new DivideByZeroException():lv/rv),_=>0};
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
        Int128 P9(string t){string r="";foreach(char c in t.ToUpper())r+=c switch{'W'=>"--",'X'=>"-0",'Y'=>"-+",'Z'=>"0-",'0'=>"00",'1'=>"0+",'2'=>"+-",'3'=>"+0",'4'=>"++",_=>throw new FormatException($"Unknown 0n char: {c}")};return BalancedTernary.ParseToInt128(r);}
        Int128 P27(string t){string r="";foreach(char c in t.ToUpper())r+=c switch{'N'=>"---",'O'=>"--0",'P'=>"--+",'Q'=>"-0-",'R'=>"-00",'S'=>"-0+",'T'=>"-+-",'U'=>"-+0",'V'=>"-++",'5'=>"+--",'6'=>"+-0",'7'=>"+-+",'8'=>"+0-",'9'=>"+00",'A'=>"+0+",'B'=>"++-",'C'=>"++0",'D'=>"+++",_=>throw new FormatException($"Unknown 0y char: {c}")};return BalancedTernary.ParseToInt128(r);}

        // ---- ISA v5: RegGroup-aware register parsing ----

        protected bool IsRegister(string t){
            string u=t.ToUpper();
            return u is "RW" or "RX" or "RY" or "RZ"
                or "FW" or "FX" or "FY" or "FZ"
                or "SP" or "FP" or "HP" or "CD" or "PR" or "WD"
                || (u.StartsWith('R') && int.TryParse(u[1..],out int i) && i>=0 && i<=4)
                || (u.StartsWith('F') && int.TryParse(u[1..],out int j) && j>=0 && j<=4)
                || (u.Length==1 && u[0]>='A' && u[0]<='I');
        }

        /// <summary>Returns RAW TRIT value (-4..+4) for encoding.</summary>
        protected int GetRegisterRaw(string t){
            string u=t.ToUpper();
            // Legacy GP aliases
            if(u.Length==1&&u[0]>='A'&&u[0]<='I')return (""+u[0])switch{"A"=>-4,"B"=>-3,"C"=>-2,"D"=>-1,"E"=>0,"F"=>1,"G"=>2,"H"=>3,"I"=>4,_=>0};
            if(u.StartsWith('R')&&int.TryParse(u[1..],out int ri)&&ri>=0&&ri<=4)return ri;
            // FPU registers
            if(u.StartsWith('F')&&int.TryParse(u[1..],out int fi)&&fi>=0&&fi<=4)return fi;
            if(u=="FW")return -4;if(u=="FX")return -3;if(u=="FY")return -2;if(u=="FZ")return -1;
            // GP registers
            if(u=="RW")return -4;if(u=="RX")return -3;if(u=="RY")return -2;if(u=="RZ")return -1;
            // Special registers
            if(u=="FP")return -4;
            if(u=="HP")return -3;
            if(u=="SP")return -2;
            if(u=="CD")return -1;
            if(u=="PR")return 0;
            if(u=="WD")return 1;
            throw new Exception($"Unknown register: {t}");
        }

        protected int GetRegisterIndex(string t)=>GetRegisterRaw(t)+4;

        protected int GetRegGroup(string t){
            string u=t.ToUpper();
            if(u is "FP" or "HP" or "SP" or "CD" or "PR" or "WD")return +1;
            if(u.StartsWith('F')||u is "FW" or "FX" or "FY" or "FZ")return -1;
            return 0;
        }

        // ---- ISA v5 opcode map ----

        protected Opcode GetOpcode(string m)=>m.ToUpper() switch{
            "HALT"=>Opcode.HALT,"NOP"=>Opcode.NOP,"LIMM"=>Opcode.LIMM,
            "MOV"=>Opcode.MOV,
            "LD"=>Opcode.LD,"LOAD"=>Opcode.LD,"LOADI"=>Opcode.LD,
            "ST"=>Opcode.ST,"STORE"=>Opcode.ST,"STOREI"=>Opcode.ST,
            "PUSH"=>Opcode.PUSH,"POP"=>Opcode.POP,
            "ADD"=>Opcode.ADD,"SUB"=>Opcode.SUB,"MUL"=>Opcode.MUL,"DIV"=>Opcode.DIV,"MOD"=>Opcode.MOD,
            "NEG"=>Opcode.NEG,"ABS"=>Opcode.ABS,"CMP"=>Opcode.CMP,
            "AND"=>Opcode.AND,"OR"=>Opcode.OR,"XOR"=>Opcode.XOR,
            "SHL"=>Opcode.SHL,"SHR"=>Opcode.SHR,
            "JMP"=>Opcode.JMP,"JE"=>Opcode.JE,"JNE"=>Opcode.JNE,"JL"=>Opcode.JL,"JG"=>Opcode.JG,
            "JLE"=>Opcode.JLE,"JGE"=>Opcode.JGE,"JM"=>Opcode.JM,
            "CALL"=>Opcode.CALL,"RET"=>Opcode.RET,
            "IN"=>Opcode.IN,"OUT"=>Opcode.OUT,
            "SQRT"=>Opcode.SQRT,"FTI"=>Opcode.FTI,"ITF"=>Opcode.ITF,
            "CLASS"=>Opcode.CLASS,"SWAP"=>Opcode.SWAP,
            // Legacy aliases
            "LI"=>Opcode.MOV,
            "ADDI"=>Opcode.ADD,"SUBI"=>Opcode.SUB,"MULI"=>Opcode.MUL,"DIVI"=>Opcode.DIV,"MODI"=>Opcode.MOD,
            "NEGI"=>Opcode.NEG,"CMPI"=>Opcode.CMP,
            "ANDI"=>Opcode.AND,"ORI"=>Opcode.OR,"XORI"=>Opcode.XOR,
            "SHLI"=>Opcode.SHL,"SHRI"=>Opcode.SHR,
            "INI"=>Opcode.IN,"OUTI"=>Opcode.OUT,"PUSHI"=>Opcode.PUSH,"POPI"=>Opcode.POP,
            "FADD"=>Opcode.ADD,"FSUB"=>Opcode.SUB,"FMUL"=>Opcode.MUL,"FDIV"=>Opcode.DIV,
            "FABS"=>Opcode.ABS,"FNEG"=>Opcode.NEG,"FCMP"=>Opcode.CMP,
            "FTOI"=>Opcode.FTI,"ITOF"=>Opcode.ITF,"FCLASS"=>Opcode.CLASS,"FSWAP"=>Opcode.SWAP,
            "FSQRT"=>Opcode.SQRT,"FLW"=>Opcode.LD,"FSW"=>Opcode.ST,"FMOV"=>Opcode.MOV,
            "FZERO"=>Opcode.MOV,
            "TRITAND"=>Opcode.AND,"TRITOR"=>Opcode.OR,"TRITXOR"=>Opcode.XOR,
            _=>throw new Exception($"Unknown mnemonic: {m}")
        };

        /// <summary>Checks if a mnemonic is an F-prefixed instruction (RegGroup=-1)</summary>
        protected bool IsFPU(string m) => m.StartsWith("F.",StringComparison.OrdinalIgnoreCase);

        /// <summary>Checks if a mnemonic is an S-prefixed instruction (RegGroup=+1)</summary>
        protected bool IsSpecial(string m) => m.StartsWith("S.",StringComparison.OrdinalIgnoreCase);

        /// <summary>Returns the mnemonic without prefix for lookup</summary>
        protected string StripPrefix(string m) {
            if(m.Length>2 && (m[0]=='F'||m[0]=='S'||m[0]=='f'||m[0]=='s') && m[1]=='.')
                return m[2..];
            return m;
        }
    }
}