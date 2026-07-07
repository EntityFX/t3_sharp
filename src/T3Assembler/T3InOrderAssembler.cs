using System;using System.Collections.Generic;using System.Linq;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    public class T3InOrderAssembler : T3AssemblerBase
    {
        readonly List<(int offset,string symbolName,T3ObjectFile.RelocationType type,T3ObjectFile.SectionType section)> _relocations=new();
        T3ObjectFile.SectionType _currentSection=T3ObjectFile.SectionType.TEXT;

        public T3InOrderAssembler(T3Config c):base(c){}
        public override List<Int128> Assemble(string src)=>Assemble(src,".");
        public T3ObjectFile AssembleToObject(string src,string baseDir){
            _labels.Clear();_constants.Clear();_globals.Clear();_externs.Clear();_relocations.Clear();
            _currentSection=T3ObjectFile.SectionType.TEXT;
            src=ProcessIncludes(src,baseDir);
            string[] raw=src.Split(new[]{"\r\n","\r","\n"},System.StringSplitOptions.None);
            int addr=0;
            foreach(var l in raw){
                string cl=CleanLine(l);if(string.IsNullOrWhiteSpace(cl))continue;
                if(cl.StartsWith(".globl",StringComparison.OrdinalIgnoreCase)){var p=cl.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(p.Length>=2)_globals.Add(p[1]);continue;}
                if(cl.StartsWith(".extern",StringComparison.OrdinalIgnoreCase)){var p=cl.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(p.Length>=2)_externs.Add(p[1]);continue;}
                if(cl.Equals(".text",StringComparison.OrdinalIgnoreCase)){_currentSection=T3ObjectFile.SectionType.TEXT;continue;}
                if(cl.Equals(".data",StringComparison.OrdinalIgnoreCase)){_currentSection=T3ObjectFile.SectionType.DATA;continue;}
                if(cl.Equals(".bss",StringComparison.OrdinalIgnoreCase)){_currentSection=T3ObjectFile.SectionType.BSS;continue;}
                int ci=cl.IndexOf(':');if(ci!=-1){string lb=cl[..ci].Trim();if(!string.IsNullOrWhiteSpace(lb)&&lb.All(c=>char.IsLetterOrDigit(c)||c=='_')){if(!_labels.ContainsKey(lb))_labels[lb]=addr;cl=cl[(ci+1)..].Trim();}}
                if(string.IsNullOrWhiteSpace(cl))continue;
                if(cl.StartsWith("equ ")||cl.Contains(" equ ")){if(cl.Contains(" equ ")){var x=cl.Split(new[]{" equ "},2,StringSplitOptions.None);if(x.Length==2)_constants[x[0].Trim()]=ResolveOperandValue(x[1].Trim());continue;}}
                if(cl.Contains(".string")||cl.Contains(".word")){var w=cl.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(w.Length>=2){if(w[1]==".string"||w[1]==".word"){string lb=w[0];if(lb.All(c=>char.IsLetterOrDigit(c)||c=='_')){if(!_labels.ContainsKey(lb))_labels[lb]=addr;}}else if(w[0]==".string"||w[0]==".word"){string lb=w[1];if(lb.All(c=>char.IsLetterOrDigit(c)||c=='_')){if(!_labels.ContainsKey(lb))_labels[lb]=addr;}}}}
                addr+=CalcSize(cl,addr);
            }
            var obj=new T3ObjectFile();int pc=0;_currentSection=T3ObjectFile.SectionType.TEXT;
            foreach(var l in raw){
                string cl=CleanLine(l);if(string.IsNullOrWhiteSpace(cl))continue;
                if(cl.StartsWith(".globl",StringComparison.OrdinalIgnoreCase))continue;
                if(cl.StartsWith(".extern",StringComparison.OrdinalIgnoreCase))continue;
                if(cl.Equals(".text",StringComparison.OrdinalIgnoreCase)){_currentSection=T3ObjectFile.SectionType.TEXT;continue;}
                if(cl.Equals(".data",StringComparison.OrdinalIgnoreCase)){_currentSection=T3ObjectFile.SectionType.DATA;continue;}
                if(cl.Equals(".bss",StringComparison.OrdinalIgnoreCase)){_currentSection=T3ObjectFile.SectionType.BSS;continue;}
                int ci2=cl.IndexOf(':');string instr=(ci2!=-1)?cl[(ci2+1)..].Trim():cl;
                if(string.IsNullOrWhiteSpace(instr))continue;
                var words=AssembleReloc(instr,pc,obj);
                foreach(var w in words){if(_currentSection==T3ObjectFile.SectionType.TEXT)obj.TextSection.Add(w);else obj.DataSection.Add(w);pc++;}
            }
            foreach(var g in _globals)if(_labels.TryGetValue(g,out int off))obj.Symbols.Add(new T3ObjectFile.Symbol{Name=g,Type=T3ObjectFile.SymbolType.GLOBAL,Section=_currentSection,Offset=off});
            foreach(var kv in _labels)if(!_globals.Contains(kv.Key)&&!_externs.Contains(kv.Key))obj.Symbols.Add(new T3ObjectFile.Symbol{Name=kv.Key,Type=T3ObjectFile.SymbolType.LOCAL,Section=_currentSection,Offset=kv.Value});
            return obj;
        }
        public List<Int128> Assemble(string src,string baseDir){
            _labels.Clear();_constants.Clear();src=ProcessIncludes(src,baseDir);
            string[] raw=src.Split(new[]{"\r\n","\r","\n"},System.StringSplitOptions.None);
            int addr=0;
            foreach(var l in raw){string cl=CleanLine(l);if(string.IsNullOrWhiteSpace(cl))continue;
                int ci=cl.IndexOf(':');if(ci!=-1){string lb=cl[..ci].Trim();if(!string.IsNullOrWhiteSpace(lb)&&lb.All(c=>char.IsLetterOrDigit(c)||c=='_')){if(!_labels.ContainsKey(lb))_labels[lb]=addr;cl=cl[(ci+1)..].Trim();}}
                if(string.IsNullOrWhiteSpace(cl))continue;
                if(cl.StartsWith("equ ")||cl.Contains(" equ ")){if(cl.Contains(" equ ")){var x=cl.Split(new[]{" equ "},2,StringSplitOptions.None);if(x.Length==2)_constants[x[0].Trim()]=ResolveOperandValue(x[1].Trim());continue;}}
                if(cl.Contains(".string")||cl.Contains(".word")){var w=cl.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(w.Length>=2){if(w[1]==".string"||w[1]==".word"){string lb=w[0];if(lb.All(c=>char.IsLetterOrDigit(c)||c=='_')){if(!_labels.ContainsKey(lb))_labels[lb]=addr;}}else if(w[0]==".string"||w[0]==".word"){string lb=w[1];if(lb.All(c=>char.IsLetterOrDigit(c)||c=='_')){if(!_labels.ContainsKey(lb))_labels[lb]=addr;}}}}
                addr+=CalcSize(cl,addr);
            }
            var bin=new List<Int128>();
            foreach(var l in raw){string cl=CleanLine(l);if(string.IsNullOrWhiteSpace(cl))continue;
                int ci2=cl.IndexOf(':');string instr=(ci2!=-1)?cl[(ci2+1)..].Trim():cl;
                if(!string.IsNullOrWhiteSpace(instr)){var r=AssembleLine(instr,bin.Count);bin.AddRange(r);}
            }
            return bin;
        }

        int CalcSize(string line,int pc){
            if(line.Contains(" equ "))return 0;
            if(line.Contains(".string")||line.StartsWith("\"")){int fq=line.IndexOf('"');int lq=line.LastIndexOf('"');if(fq!=-1&&lq!=-1&&fq!=lq)return(lq-fq-1)+1;}
            if(line.Contains(".word")){var p=line.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(p.Length>0){string last=p[p.Length-1];if(last.StartsWith("\""))return last.Length-2+1;return 1;}}
            var ip=line.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(ip.Length==0)return 0;
            string rawMn=ip[0];string mn=StripPrefix(rawMn).ToUpper();
            
            long? imm = null;
            if(ip.Length > 0) {
                string last = ip[ip.Length-1];
                if(!IsRegister(last)) {
                    try { imm = (long)ResolveOperandValue(last); } catch { }
                }
            }
            return InstructionMeta.GetSize(mn, ip, imm);
        }

        List<Int128> AssembleLine(string line,int pc){
            if(line.Contains(" equ "))return new List<Int128>();
            if(line.Contains(".string")){int fq=line.IndexOf('"');int lq=line.LastIndexOf('"');if(fq==-1||lq==-1||fq==lq)throw new Exception("Invalid string");string c=line.Substring(fq+1,lq-fq-1);return ResolveString($"\"{c}\"");}
            if(line.StartsWith("\""))return ResolveString(line);
            if(line.StartsWith(".align")){var p=line.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(p.Length<2)throw new Exception(".align needs arg");int al=int.Parse(p[1]);int pad=al-(pc%al);if(pad==al)pad=0;var r=new List<Int128>();for(int i=0;i<pad;i++)r.Add(0);return r;}
            if(line.StartsWith(".word")){var p=line.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(p.Length<2)throw new Exception("Invalid .word");if(p[1].StartsWith("\""))return ResolveString(p[1]);return new List<Int128>{ResolveOperandValue(p[1])};}
            return AssembleCore(line,pc,null);
        }
        List<Int128> AssembleReloc(string line,int pc,T3ObjectFile obj)=>AssembleCore(line,pc,obj);

        List<Int128> AssembleCore(string line,int pc,T3ObjectFile? obj){
            int pred=0;string pl=line;
            if(line.StartsWith("(")){int cp=line.IndexOf(')');if(cp!=-1){string pp=line[1..cp].ToLower();if(pp.StartsWith("p")&&int.TryParse(pp[1..],out int pi)){pred=pi;pl=line[(cp+1)..].Trim();}}}
            var ip=pl.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(ip.Length==0)throw new Exception("Empty");

            string rawMn=ip[0];string mn=StripPrefix(rawMn).ToUpper();
            Opcode op=GetOpcode(mn);

            int regGroup=IsFPU(rawMn)?-1:IsSpecial(rawMn)?+1:0;
            // MOV without prefix uses destination-driven RegGroup (avoids Special promotion when dest is GP)
            bool hasExplicitPrefix = IsFPU(rawMn) || IsSpecial(rawMn);
            if (!hasExplicitPrefix && op == Opcode.MOV && ip.Length >= 2 && IsRegister(ip[1]))
            {
                regGroup = GetRegGroup(ip[1]);
            }
            else
            {
                for(int k=1; k<ip.Length; k++){
                    if(IsRegister(ip[k])){
                        string rn=ip[k].ToUpper();
                        int rg=GetRegGroup(rn);
                        if(regGroup != 0 && rg != 0 && rg != regGroup)
                            throw new Exception($"Architecture Error: Mixing registers from different groups in instruction {mn}. Operand {ip[k]} (Group {rg}) conflicts with detected Group {regGroup}.");
                        if(rg==+1 && regGroup!=+1){regGroup=+1;}
                        else if(rg==-1 && regGroup==0){regGroup=-1;}
                    }
                }
            }

            int op1=0,op2=0,op3=0;long imm=0;bool isImm=false;
            if(ip.Length>1){if(IsRegister(ip[1]))op1=GetRegisterRaw(ip[1]);else if(long.TryParse(ip[1],out long i1))op1=(int)i1;}
            if(ip.Length>2){if(IsRegister(ip[2]))op2=GetRegisterRaw(ip[2]);else if(long.TryParse(ip[2],out long i2))op2=(int)i2;}
            if(ip.Length>3){if(IsRegister(ip[3]))op3=GetRegisterRaw(ip[3]);else if(long.TryParse(ip[3],out long i3))op3=(int)i3;}

            if(ip.Length>=2){
                string last=ip[ip.Length-1];isImm=last.StartsWith("#");if(isImm)imm=(long)ResolveOperandValue(last[1..]);
            }

            if(mn=="LIMM"){
                string valStr=ip[2];
                if(obj!=null&&(!long.TryParse(valStr,out _)&&!valStr.StartsWith("0t")&&!valStr.StartsWith("0y")&&!valStr.StartsWith("0n"))){
                    if(_externs.Contains(valStr)||(!_labels.ContainsKey(valStr)&&_globals.Contains(valStr))){
                        int si=obj.Symbols.Count;obj.Symbols.Add(new T3ObjectFile.Symbol{Name=valStr,Type=T3ObjectFile.SymbolType.EXTERN,Section=T3ObjectFile.SectionType.ABS,Offset=0});
                        obj.Relocations.Add(new T3ObjectFile.Relocation{SectionOffset=pc,SymbolIndex=si,Type=T3ObjectFile.RelocationType.LIMM_ABSOLUTE,Section=_currentSection});
                        return new List<Int128>{EncI(pred,regGroup,Opcode.LIMM,op1,0),(Int128)0};
                    }
                }
                return new List<Int128>{EncI(pred,regGroup,Opcode.LIMM,op1,0),(Int128)ResolveOperandValue(valStr)};
            }

            if(IsJumpMnemonic(mn)){
                string opn=ip.Length>1?ip[1]:"0";
                if(!IsRegister(opn)){
                    long tgt=_labels.TryGetValue(opn,out int la)?la:(long)ResolveOperandValue(opn);
                    if(obj!=null&&(!_labels.ContainsKey(opn)&&(_externs.Contains(opn)||_globals.Contains(opn)))){
                        int si=obj.Symbols.Count;obj.Symbols.Add(new T3ObjectFile.Symbol{Name=opn,Type=T3ObjectFile.SymbolType.EXTERN,Section=T3ObjectFile.SectionType.ABS,Offset=0});
                        obj.Relocations.Add(new T3ObjectFile.Relocation{SectionOffset=pc,SymbolIndex=si,Type=T3ObjectFile.RelocationType.LIMM_ABSOLUTE,Section=_currentSection});
                        return new List<Int128>{EncI(0,0,Opcode.LIMM,1,0),(Int128)0,EncJ(0,0,op,1)};
                    }
                    return new List<Int128>{EncI(0,0,Opcode.LIMM,1,0),(Int128)tgt,EncJ(0,0,op,1)};
                }
                return new List<Int128>{EncJ(pred,regGroup,op,GetRegisterRaw(opn))};
            }

            if(op==Opcode.MOV && ip.Length>=2 && !IsRegister(ip[1])){
                string valStr=ip[1];Int128 v=ResolveOperandValue(valStr);
                if((long)v>364||(long)v<-364)return new List<Int128>{EncI(pred,regGroup,Opcode.LIMM,op1,0),(Int128)v};
                return new List<Int128>{EncI(pred,regGroup,Opcode.MOV,op1,(long)v)};
            }
            if((mn=="LI"||mn=="MOV")&&ip.Length>=3&&!IsRegister(ip[2])){
                string valStr=ip[2];Int128 v=ResolveOperandValue(valStr);
                if((long)v>364||(long)v<-364)return new List<Int128>{EncI(pred,regGroup,Opcode.LIMM,op1,0),(Int128)v};
                return new List<Int128>{EncI(pred,regGroup,Opcode.MOV,op1,(long)v)};
            }

            if(op==Opcode.LD||op==Opcode.ST){
                int baseR=op2!=0?op2:(op1!=0?3:0);
                long off=ip.Length>3&&!IsRegister(ip[ip.Length-1])?(long)ResolveOperandValue(ip[ip.Length-1]):0;
                if(ip.Length>=3){string last=ip[ip.Length-1];if(!IsRegister(last)&&!last.StartsWith("#"))off=(long)ResolveOperandValue(last);}
                return new List<Int128>{EncS(pred,regGroup,op,op1,baseR,off)};
            }

            if(isImm || (ip.Length>=3 && !IsRegister(ip[ip.Length-1])))
                return new List<Int128>{EncI(pred,regGroup,op,op1,(long)ResolveOperandValue(ip[ip.Length-1]))};

            return new List<Int128>{EncR(pred,regGroup,op,op1,op2,op3)};
        }

        bool IsJumpMnemonic(string m)=>m is "JMP" or "JE" or "JNE" or "JL" or "JG" or "JM" or "JLE" or "JGE" or "CALL";

        static Int128 EncR(int pred,int rg,Opcode op,int o1,int o2,int o3)=>InstructionEncoder.EncodeR(pred,rg,0,(int)op,o1,o2,o3);
        static Int128 EncI(int pred,int rg,Opcode op,int o1,long imm)=>InstructionEncoder.EncodeI(pred,rg,+1,(int)op,o1,imm);
        static Int128 EncJ(int pred,int rg,Opcode op,int reg)=>InstructionEncoder.EncodeJ(pred,rg,-1,(int)op,reg);
        static Int128 EncS(int pred,int rg,Opcode op,int o1,int o2,long imm)=>InstructionEncoder.EncodeS(pred,rg,0,(int)op,o1,o2,imm);
    }
}