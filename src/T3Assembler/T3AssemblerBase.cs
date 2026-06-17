using System;using System.Collections.Generic;using System.Globalization;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    /// <summary>9 regs: RW(0) RX(1) RY(2) RZ(3) R0(4) R1(5) R2(6) R3(7) R4(8). A-I aliases kept.</summary>
    public abstract class T3AssemblerBase
    {
        protected readonly T3Config _config;
        protected readonly Dictionary<string,int> _labels=new();
        protected readonly List<string> _lines=new();
        public T3AssemblerBase(T3Config c){_config=c;}
        public abstract List<Int128> Assemble(string src);
        protected string CleanLine(string l){int c=l.IndexOf(';');if(c>=0)l=l[..c];return l.Trim();}
        protected int ResolveOperand(string t){
            if(IsRegister(t))return GetRegisterIndex(t);
            if(long.TryParse(t,out long v))return(int)v;
            if(_labels.TryGetValue(t,out int a))return a;
            throw new Exception($"Unknown: {t}");
        }
        protected List<Int128> ResolveString(string t){string s=t[1..^1];var r=new List<Int128>();foreach(char c in s)r.Add(TScii.FromChar(c));r.Add(0);return r;}
        protected Int128 ResolveOperandValue(string t){
            if(IsRegister(t))return GetRegisterIndex(t);
            if(long.TryParse(t,out long v))return v;
            if(_labels.TryGetValue(t,out int a))return a;
            if(t.StartsWith("t",StringComparison.OrdinalIgnoreCase))return BalancedTernary.ParseToInt128(t[1..]);
            if(t.StartsWith("0n",StringComparison.OrdinalIgnoreCase))return P9(t[2..]);
            if(t.StartsWith("0y",StringComparison.OrdinalIgnoreCase))return P27(t[2..]);
            throw new Exception($"Unknown: {t}");
        }
        Int128 P9(string t){string r="";foreach(char c in t.ToUpper())r+=c switch{'W'=>"--",'X'=>"-0",'Y'=>"-+",'Z'=>"0-",'0'=>"00",'1'=>"0+",'2'=>"+-",'3'=>"+0",'4'=>"++",_=>""};return BalancedTernary.ParseToInt128(r);}
        Int128 P27(string t){string r="";foreach(char c in t.ToUpper())r+=c switch{'N'=>"---",'O'=>"--0",'P'=>"--+",'Q'=>"-0-",'R'=>"-00",'S'=>"-0+",'T'=>"-+-",'U'=>"-+0",'V'=>"-++",'5'=>"+--",'6'=>"+-0",'7'=>"+-+",'8'=>"+0-",'9'=>"+00",'A'=>"+0+",'B'=>"++-",'C'=>"++0",'D'=>"+++",_=>""};return BalancedTernary.ParseToInt128(r);}

        protected bool IsRegister(string t){
            string u=t.ToUpper();
            if(u.Length==1&&u[0]>='A'&&u[0]<='I')return true;
            return u is"RW"or"RX"or"RY"or"RZ"or"FW"or"FX"or"FY"or"FZ"
                ||(u.StartsWith('R')&&int.TryParse(u[1..],out int i)&&i>=0&&i<=4)
                ||(u.StartsWith('F')&&int.TryParse(u[1..],out int j)&&j>=0&&j<=4);
        }

        protected int GetRegisterIndex(string t){
            string u=t.ToUpper();
            if(u.StartsWith('R')&&int.TryParse(u[1..],out int i)&&i>=0&&i<=4)return i+4;
            if(u.StartsWith('F')&&int.TryParse(u[1..],out int j)&&j>=0&&j<=4)return j+4;
            if(u.Length==1&&u[0]>='A'&&u[0]<='I')return (""+u[0]) switch{"A"=>0,"B"=>1,"C"=>2,"D"=>3,"E"=>4,"F"=>5,"G"=>6,"H"=>7,"I"=>8,_=>0};
            return u switch{"RW"or"FW"=>0,"RX"or"FX"=>1,"RY"or"FY"=>2,"RZ"or"FZ"=>3,_=>throw new Exception($"Unknown register: {t}")};
        }

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