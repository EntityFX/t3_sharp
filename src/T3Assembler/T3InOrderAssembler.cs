using System;using System.Collections.Generic;using System.Linq;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    public class T3InOrderAssembler : T3AssemblerBase
    {
        public T3InOrderAssembler(T3Config c):base(c){}
        public override List<Int128> Assemble(string src){
            _labels.Clear();
            _constants.Clear();
            string[] raw = src.Split(new[]{"\r\n", "\r", "\n"}, StringSplitOptions.None);
            
            // First Pass: Calculate Labels and Constants
            int addr = 0;
            foreach (var l in raw) {
                string cl = CleanLine(l);
                if (string.IsNullOrWhiteSpace(cl)) continue;
                
                // Handle Label at the start of the line
                int ci = cl.IndexOf(':');
                if (ci != -1) {
                    string lb = cl[..ci].Trim();
                    if (!string.IsNullOrWhiteSpace(lb) && lb.All(c => char.IsLetterOrDigit(c) || c == '_')) {
                        if (!_labels.ContainsKey(lb)) _labels[lb] = addr;
                        cl = cl[(ci + 1)..].Trim();
                    } else {
                        // If the label is invalid, we treat it as a regular line or ignore it
                        // but for this assembler, we assume labels are well-formed.
                    }
                }

                if (string.IsNullOrWhiteSpace(cl)) continue;

                // Handle Constants (equ)
                if (cl.StartsWith("equ ") || cl.Contains(" equ ")) {
                    string content = cl;
                    if (cl.StartsWith("equ ")) content = cl[4..].Trim();
                    
                    var parts = content.Split(new[] { " equ " }, StringSplitOptions.RemoveEmptyEntries);
                    // This is a bit simplified, let's just use a regex-like approach
                    if (cl.Contains(" equ ")) {
                        var p = cl.Split(new[] { " equ " }, 2, StringSplitOptions.None);
                        if (p.Length == 2) {
                            _constants[p[0].Trim()] = ResolveOperandValue(p[1].Trim());
                            continue;
                        }
                    }
                }
                
                // Special handling for .string/.word labels in first pass
                if (cl.Contains(".string") || cl.Contains(".word")) {
                    var words = cl.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length >= 2) {
                        // Format 1: label .string "..."
                        if (words[1] == ".string" || words[1] == ".word") {
                            string lb = words[0];
                            if (lb.All(c => char.IsLetterOrDigit(c) || c == '_')) {
                                if (!_labels.ContainsKey(lb)) _labels[lb] = addr;
                            }
                        }
                        // Format 2: .string label "..."
                        else if (words[0] == ".string" || words[0] == ".word") {
                            string lb = words[1];
                            if (lb.All(c => char.IsLetterOrDigit(c) || c == '_')) {
                                if (!_labels.ContainsKey(lb)) _labels[lb] = addr;
                            }
                        }
                    }
                }

                addr += CalculateLineSize(cl, addr);
            }

            // Second Pass: Generate Binary
            var bin = new List<Int128>();
            foreach (var l in raw) {
                string cl = CleanLine(l);
                if (string.IsNullOrWhiteSpace(cl)) continue;
                
                // Skip label definition part
                int ci2 = cl.IndexOf(':');
                string instr = (ci2 != -1) ? cl[(ci2 + 1)..].Trim() : cl;
                
                if (!string.IsNullOrWhiteSpace(instr)) {
                    var r = AssembleLine(instr, bin.Count);
                    bin.AddRange(r);
                }
            }
            return bin;
        }

        int CalculateLineSize(string line, int pc) {
            if (line.Contains(" equ ")) return 0;
            
            // Handle strings: can be ".string [label] \"...\"" or "label .string \"...\"" or just "\"...\""
            if (line.Contains(".string") || line.StartsWith("\"")) {
                int firstQuote = line.IndexOf('"');
                int lastQuote = line.LastIndexOf('"');
                if (firstQuote != -1 && lastQuote != -1 && firstQuote != lastQuote) {
                    return (lastQuote - firstQuote - 1) + 1;
                }
            }
            
            // Handle words: can be ".word [label] value" or "label .word value"
            if (line.Contains(".word")) {
                var p = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                // Find where the actual value is. It's usually the last part.
                if (p.Length > 0) {
                    string last = p[p.Length - 1];
                    if (last.StartsWith("\"")) return last.Length - 2 + 1;
                    return 1;
                }
            }
            
            var ip = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (ip.Length == 0) return 0;
            string mn = ip[0].ToUpper();
            if (IsJumpMnemonic(mn) && ip.Length > 1 && !IsRegister(ip[1])) {
                // All label-based jumps: LIMM R1, addr; Jxx R1 = 3 words
                return 3;
            }
            if (mn == "LI") {
                string valStr = ip.Length > 2 ? ip[2] : "0";
                // Labels/names always use LIMM (2 words) — match AssembleLine behavior
                if (!long.TryParse(valStr, out _) && !valStr.StartsWith("0t") && !valStr.StartsWith("0y") && !valStr.StartsWith("0n"))
                    return 2;
                try {
                    long rv = (long)ResolveOperandValue(valStr);
                    if (rv > 364 || rv < -364) return 2;
                } catch { return 2; }
                return 1;
            }
            if (mn == "LIMM") return 2;
            return 1;
        }
        List<Int128> AssembleLine(string line,int pc){
            if (line.Contains(" equ ")) return new List<Int128>();
            if (line.Contains(".string")) {
                int firstQuote = line.IndexOf('"');
                int lastQuote = line.LastIndexOf('"');
                if (firstQuote == -1 || lastQuote == -1 || firstQuote == lastQuote) throw new Exception("Invalid string literal");
                string content = line.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                
                var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2) {
                    string label = words[0] == ".string" ? words[1] : words[0];
                    if (label.All(c => char.IsLetterOrDigit(c) || c == '_')) {
                        if (!_labels.ContainsKey(label)) _labels[label] = pc;
                    }
                }
                return ResolveString($"\"{content}\"");
            }
            if(line.StartsWith("\""))return ResolveString(line);
            if(line.StartsWith(".word")){var p=line.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(p.Length<2)throw new Exception("Invalid .word");if(p[1].StartsWith("\""))return ResolveString(p[1]);return new List<Int128>{ResolveOperandValue(p[1])};}
            int pred=0;string pl=line;
            if(line.StartsWith("(")){int cp=line.IndexOf(')');if(cp!=-1){string pp=line[1..cp].ToLower();if(pp.StartsWith("p")&&int.TryParse(pp[1..],out int pi)){pred=pi;pl=line[(cp+1)..].Trim();}}}
            var ip=pl.Split(new[]{' ','\t',','},StringSplitOptions.RemoveEmptyEntries);if(ip.Length==0)throw new Exception("Empty");
            string mn=ip[0].ToUpper();Opcode op=GetOpcode(mn);
            int op1=0,op2=0,op3=0;long imm=0;
            if(ip.Length>1){if(IsRegister(ip[1]))op1=GetRegisterTrit(ip[1]);else if(int.TryParse(ip[1],out int i1))op1=i1;}
            if(ip.Length>2){if(IsRegister(ip[2]))op2=GetRegisterTrit(ip[2]);else if(int.TryParse(ip[2],out int i2))op2=i2;}
            if(ip.Length>3){if(IsRegister(ip[3]))op3=GetRegisterTrit(ip[3]);else if(int.TryParse(ip[3],out int i3))op3=i3;}

            if(IsJumpMnemonic(mn)){
                string opn=ip.Length>1?ip[1]:"0";
                // Label-based jumps: LIMM R1, target; Jxx R1 (register-indirect, 3 words)
                if(!IsRegister(opn)){
                    long tgtAddr = _labels.TryGetValue(opn,out int laddr)?laddr:(long)ResolveOperandValue(opn);
                    return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)Opcode.LIMM,1,0),(Int128)tgtAddr,InstructionEncoder.EncodeJ(pred,(int)op,1)};
                }
                // Register-indirect: single word J-type
                long enc=InstructionEncoder.EncodeJ(pred,(int)op,GetRegisterTrit(opn));return new List<Int128>{enc};
            }
            // LI with label in first operand position: always LIMM (2 words) for consistency
            else if(mn=="LI"){
                string valStr = ip.Length>2?ip[2]:"0";
                // If value is a label or constant name → always LIMM
                if(!long.TryParse(valStr,out _)&&!valStr.StartsWith("0t")&&!valStr.StartsWith("0y")&&!valStr.StartsWith("0n"))
                    return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)Opcode.LIMM,op1,0),(Int128)ResolveOperandValue(valStr)};
                long rv=(long)ResolveOperandValue(valStr);
                if(rv>364||rv<-364)return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)Opcode.LIMM,op1,0),(Int128)rv};
                return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)Opcode.LI,op1,rv)};
            }
            else if(mn=="LIMM")return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)Opcode.LIMM,op1,0),(Int128)ResolveOperandValue(ip[2])};
            else if(mn=="INI"||mn=="OUTI"){if(ip.Length>2)imm=(long)ResolveOperandValue(ip[2]);return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)op,op1,imm)};}
            else if(IsIType(op)){if(ip.Length>2)imm=(long)ResolveOperandValue(ip[2]);return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)op,op1,imm)};}
            else return new List<Int128>{InstructionEncoder.EncodeR(pred,(int)op,op1,op2,op3)};
        }
        bool IsJumpMnemonic(string m)=>m is"JMP"or"JE"or"JNE"or"JL"or"JG"or"JM"or"JLE"or"JGE"or"CALL";
        bool IsIType(Opcode op)=>op switch{Opcode.MOVI or Opcode.LI or Opcode.LIMM or Opcode.ADDI or Opcode.SUBI or Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or Opcode.ANDI or Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or Opcode.LOADI or Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.FZERO=>true,_=>false};
    }
}