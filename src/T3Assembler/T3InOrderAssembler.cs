using System;using System.Collections.Generic;using System.Linq;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    public class T3InOrderAssembler : T3AssemblerBase
    {
        public T3InOrderAssembler(T3Config c):base(c){}
        public override List<Int128> Assemble(string src){
            _labels.Clear();
            string[] raw=src.Split(new[]{"\r\n","\r","\n"},StringSplitOptions.None);
            int addr=0;
            foreach(var l in raw){string cl=CleanLine(l);if(string.IsNullOrWhiteSpace(cl))continue;int ci=cl.IndexOf(':');if(ci!=-1&&ci>0&&cl[..ci].All(c=>char.IsLetterOrDigit(c)||c=='_')){string lb=cl[..ci];if(!_labels.ContainsKey(lb))_labels[lb]=addr;string rest=cl[(ci+1)..].Trim();if(!string.IsNullOrWhiteSpace(rest))addr+=CountWords(rest);}else addr+=CountWords(cl);}
            var bin=new List<Int128>();
            foreach(var l in raw){string cl=CleanLine(l);if(string.IsNullOrWhiteSpace(cl))continue;int ci=cl.IndexOf(':');string instr=ci!=-1?cl[(ci+1)..].Trim():cl;if(!string.IsNullOrWhiteSpace(instr)){var r=AssembleLine(instr,bin.Count);bin.AddRange(r);}}
            return bin;
        }
        int CountWords(string l){if(l.StartsWith("\""))return l.Length-2+1;if(l.StartsWith(".word")){var p=l.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(p.Length<2||!p[1].StartsWith("\""))return 1;return p[1].Length-2+1;}var x=l.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(x.Length==0)return 0;if(x[0].ToUpper()=="LIMM")return 2;return 1;}
        List<Int128> AssembleLine(string line,int pc){
            if(line.StartsWith("\""))return ResolveString(line);
            if(line.StartsWith(".word")){var p=line.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(p.Length<2)throw new Exception("Invalid .word");if(p[1].StartsWith("\""))return ResolveString(p[1]);return new List<Int128>{ResolveOperandValue(p[1])};}
            int pred=0;string pl=line;
            if(line.StartsWith("(")){int cp=line.IndexOf(')');if(cp!=-1){string pp=line[1..cp].ToLower();if(pp.StartsWith("p")&&int.TryParse(pp[1..],out int pi)){pred=pi;pl=line[(cp+1)..].Trim();}}}
            var ip=pl.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(ip.Length==0)throw new Exception("Empty");
            string mn=ip[0].ToUpper();Opcode op=GetOpcode(mn);
            int op1=0,op2=0,op3=0;long imm=0;
            if(ip.Length>1)op1=IsRegister(ip[1])?GetRegisterTrit(ip[1]):0;
            if(ip.Length>2)op2=IsRegister(ip[2])?GetRegisterTrit(ip[2]):0;
            if(ip.Length>3)op3=IsRegister(ip[3])?GetRegisterTrit(ip[3]):0;

            if(IsJumpMnemonic(mn)){
                string opn=ip.Length>1?ip[1]:"0";
                if(IsRegister(opn)){long enc=InstructionEncoder.EncodeJ(pred,(int)op,GetRegisterTrit(opn));return new List<Int128>{enc};}
                else if(_labels.ContainsKey(opn)){long tgt=(long)ResolveOperandValue(opn);imm=tgt-pc;long enc=InstructionEncoder.EncodeI(pred,(int)op,0,imm);return new List<Int128>{enc};}
                else{imm=(long)ResolveOperandValue(opn);long enc=InstructionEncoder.EncodeI(pred,(int)op,0,imm);return new List<Int128>{enc};}
            }
            else if(mn=="LI"){long rv=(long)ResolveOperandValue(ip.Length>2?ip[2]:"0");if(rv>364||rv<-364)return new List<Int128>{InstructionEncoder.EncodeR(pred,(int)Opcode.LIMM,op1,0,0),ResolveOperandValue(ip[2])};return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)Opcode.LI,op1,rv)};}
            else if(mn=="LIMM")return new List<Int128>{InstructionEncoder.EncodeR(pred,(int)Opcode.LIMM,op1,0,0),ResolveOperandValue(ip[2])};
            else if(mn=="INI"||mn=="OUTI"){if(ip.Length>2)imm=(long)ResolveOperandValue(ip[2]);return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)op,op1,imm)};}
            else if(IsIType(op)){if(ip.Length>2)imm=(long)ResolveOperandValue(ip[2]);return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)op,op1,imm)};}
            else return new List<Int128>{InstructionEncoder.EncodeR(pred,(int)op,op1,op2,op3)};
        }
        bool IsJumpMnemonic(string m)=>m is"JMP"or"JE"or"JNE"or"JL"or"JG"or"JM"or"JLE"or"JGE"or"CALL";
        bool IsIType(Opcode op)=>op switch{Opcode.MOVI or Opcode.LI or Opcode.LIMM or Opcode.ADDI or Opcode.SUBI or Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or Opcode.ANDI or Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or Opcode.LOADI or Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.FZERO=>true,_=>false};
    }
}