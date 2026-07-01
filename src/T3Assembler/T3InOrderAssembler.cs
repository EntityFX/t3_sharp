using System;using System.Collections.Generic;using System.Linq;using TritTypes;using T3Simulator.Common;
namespace T3Assembler
{
    /// <summary>
    /// Two-pass in-order assembler for T3-18.
    /// Label-based jumps (JMP/JE/JNE/JL/JG/JM/JLE/JGE/CALL label) use
    /// R1 as a scratch register: LIMM R1, addr; Jxx R1 (3 words).
    /// Caller must not rely on R1 being preserved across such jumps.
    /// </summary>
    public class T3InOrderAssembler : T3AssemblerBase
    {
        // Relocation tracking for object file emission
        readonly List<(int offset, string symbolName, T3ObjectFile.RelocationType type, T3ObjectFile.SectionType section)> _relocations = new();
        T3ObjectFile.SectionType _currentSection = T3ObjectFile.SectionType.TEXT;

        public T3InOrderAssembler(T3Config c):base(c){}
        public override List<Int128> Assemble(string src){
            return Assemble(src, ".");
        }

        /// <summary>
        /// Assemble source to a T3ObjectFile (.o) with symbol and relocation tables.
        /// </summary>
        public T3ObjectFile AssembleToObject(string src, string baseDir)
        {
            _labels.Clear();
            _constants.Clear();
            _globals.Clear();
            _externs.Clear();
            _relocations.Clear();
            _currentSection = T3ObjectFile.SectionType.TEXT;

            src = ProcessIncludes(src, baseDir);
            string[] raw = src.Split(new[]{"\r\n", "\r", "\n"}, StringSplitOptions.None);

            // First Pass: Calculate Labels, Constants, .globl, .extern
            int addr = 0;
            foreach (var l in raw)
            {
                string cl = CleanLine(l);
                if (string.IsNullOrWhiteSpace(cl)) continue;

                // Handle .globl directive
                if (cl.StartsWith(".globl", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = cl.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2) _globals.Add(parts[1]);
                    continue;
                }

                // Handle .extern directive
                if (cl.StartsWith(".extern", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = cl.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2) _externs.Add(parts[1]);
                    continue;
                }

                // Handle .text / .data section switching
                if (cl.Equals(".text", StringComparison.OrdinalIgnoreCase))
                {
                    _currentSection = T3ObjectFile.SectionType.TEXT;
                    continue;
                }
                if (cl.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    _currentSection = T3ObjectFile.SectionType.DATA;
                    continue;
                }
                if (cl.Equals(".bss", StringComparison.OrdinalIgnoreCase))
                {
                    _currentSection = T3ObjectFile.SectionType.BSS;
                    continue;
                }

                // Handle Label at the start of the line
                int ci = cl.IndexOf(':');
                if (ci != -1) {
                    string lb = cl[..ci].Trim();
                    if (!string.IsNullOrWhiteSpace(lb) && lb.All(c => char.IsLetterOrDigit(c) || c == '_')) {
                        if (!_labels.ContainsKey(lb)) _labels[lb] = addr;
                        cl = cl[(ci + 1)..].Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(cl)) continue;

                // Handle Constants (equ)
                if (cl.StartsWith("equ ") || cl.Contains(" equ ")) {
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
                        if (words[1] == ".string" || words[1] == ".word") {
                            string lb = words[0];
                            if (lb.All(c => char.IsLetterOrDigit(c) || c == '_')) {
                                if (!_labels.ContainsKey(lb)) _labels[lb] = addr;
                            }
                        }
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

            // Second Pass: Generate Binary with relocation tracking
            var obj = new T3ObjectFile();
            int pc = 0;
            _currentSection = T3ObjectFile.SectionType.TEXT;

            foreach (var l in raw)
            {
                string cl = CleanLine(l);
                if (string.IsNullOrWhiteSpace(cl)) continue;

                // Handle directives that don't produce code
                if (cl.StartsWith(".globl", StringComparison.OrdinalIgnoreCase)) continue;
                if (cl.StartsWith(".extern", StringComparison.OrdinalIgnoreCase)) continue;
                if (cl.Equals(".text", StringComparison.OrdinalIgnoreCase))
                {
                    _currentSection = T3ObjectFile.SectionType.TEXT;
                    continue;
                }
                if (cl.Equals(".data", StringComparison.OrdinalIgnoreCase))
                {
                    _currentSection = T3ObjectFile.SectionType.DATA;
                    continue;
                }
                if (cl.Equals(".bss", StringComparison.OrdinalIgnoreCase))
                {
                    _currentSection = T3ObjectFile.SectionType.BSS;
                    continue;
                }

                // Skip label definition part
                int ci2 = cl.IndexOf(':');
                string instr = (ci2 != -1) ? cl[(ci2 + 1)..].Trim() : cl;

                if (string.IsNullOrWhiteSpace(instr)) continue;

                // Handle .word in data section
                if (instr.StartsWith(".word"))
                {
                    var p = instr.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length >= 2)
                    {
                        // Check if value is a label reference (for relocation)
                        string valStr = p[1];
                        if (!long.TryParse(valStr, out _) && !valStr.StartsWith("0t") && !valStr.StartsWith("0y") && !valStr.StartsWith("0n"))
                        {
                            if (_externs.Contains(valStr) || (!_labels.ContainsKey(valStr) && _globals.Contains(valStr)))
                            {
                                // External reference: emit 0, record relocation
                                obj.TextSection.Add(0);
                                int symIdx = obj.Symbols.Count;
                                obj.Symbols.Add(new T3ObjectFile.Symbol { Name = valStr, Type = T3ObjectFile.SymbolType.EXTERN, Section = T3ObjectFile.SectionType.ABS, Offset = 0 });
                                obj.Relocations.Add(new T3ObjectFile.Relocation { SectionOffset = pc, SymbolIndex = symIdx, Type = T3ObjectFile.RelocationType.LIMM_ABSOLUTE, Section = _currentSection });
                                pc++;
                                continue;
                            }
                        }
                    }
                }

                // Assemble the line with relocation tracking
                var words = AssembleLineWithRelocs(instr, pc, obj);
                foreach (var w in words)
                {
                    if (_currentSection == T3ObjectFile.SectionType.TEXT)
                        obj.TextSection.Add(w);
                    else
                        obj.DataSection.Add(w);
                    pc++;
                }
            }

            // Add GLOBAL symbols
            foreach (var g in _globals)
            {
                if (_labels.TryGetValue(g, out int offset))
                {
                    obj.Symbols.Add(new T3ObjectFile.Symbol { Name = g, Type = T3ObjectFile.SymbolType.GLOBAL, Section = _currentSection, Offset = offset });
                }
            }

            // Add LOCAL symbols (non-global, non-extern labels)
            foreach (var kv in _labels)
            {
                if (!_globals.Contains(kv.Key) && !_externs.Contains(kv.Key))
                {
                    obj.Symbols.Add(new T3ObjectFile.Symbol { Name = kv.Key, Type = T3ObjectFile.SymbolType.LOCAL, Section = _currentSection, Offset = kv.Value });
                }
            }

            return obj;
        }

        public List<Int128> Assemble(string src, string baseDir){
            _labels.Clear();
            _constants.Clear();
            
            // Pre-process #include directives
            src = ProcessIncludes(src, baseDir);
            
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
                    }
                }

                if (string.IsNullOrWhiteSpace(cl)) continue;

                // Handle Constants (equ)
                if (cl.StartsWith("equ ") || cl.Contains(" equ ")) {
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
                    if (label.All(c => char.IsLetterOrDigit(c) || c == '_') && !_labels.ContainsKey(label)) {
                        _labels[label] = pc;
                    }
                }
                return ResolveString($"\"{content}\"");
            }
            if(line.StartsWith("\""))return ResolveString(line);
            if(line.StartsWith(".align")){var p=line.Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(p.Length<2)throw new Exception(".align needs argument");int al=int.Parse(p[1]);int pad=al-(pc%al);if(pad==al)pad=0;var r=new List<Int128>();for(int i=0;i<pad;i++)r.Add(0);return r;}
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
            else if(op == Opcode.LOADI || op == Opcode.STOREI){
                int baseReg = 0;
                if(ip.Length > 2 && IsRegister(ip[2])){
                    baseReg = GetRegisterTrit(ip[2]);
                    imm = (ip.Length > 3) ? (long)ResolveOperandValue(ip[3]) : 0;
                } else if (ip.Length > 2){
                    baseReg = 3; // Default to RZ if second arg is not a register (it's the offset)
                    imm = (long)ResolveOperandValue(ip[2]);
                } else {
                    baseReg = 3;
                    imm = 0;
                }
                return new List<Int128>{InstructionEncoder.EncodeS(pred,(int)op,op1,baseReg,imm)};
            }
            else if(IsIType(op)){
                // Use last argument as immediate (handles 3-arg I-type like ADDI SP, SP, 7)
                if(ip.Length>2)imm=(long)ResolveOperandValue(ip[ip.Length - 1]);
                return new List<Int128>{InstructionEncoder.EncodeI(pred,(int)op,op1,imm)};
            }
            else return new List<Int128>{InstructionEncoder.EncodeR(pred,(int)op,op1,op2,op3)};
        }
        bool IsJumpMnemonic(string m)=>m is"JMP"or"JE"or"JNE"or"JL"or"JG"or"JM"or"JLE"or"JGE"or"CALL";
        bool IsIType(Opcode op)=>op switch{Opcode.MOVI or Opcode.LI or Opcode.LIMM or Opcode.ADDI or Opcode.SUBI or Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or Opcode.ANDI or Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or Opcode.LOADI or Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.FZERO or Opcode.PUSHI or Opcode.POPI=>true,_=>false};

        /// <summary>
        /// Assemble a single line with relocation tracking for object file emission.
        /// Returns the encoded words and records relocations in the provided T3ObjectFile.
        /// </summary>
        List<Int128> AssembleLineWithRelocs(string line, int pc, T3ObjectFile obj)
        {
            if (line.Contains(" equ ")) return new List<Int128>();
            if (line.Contains(".string"))
            {
                int firstQuote = line.IndexOf('"');
                int lastQuote = line.LastIndexOf('"');
                if (firstQuote == -1 || lastQuote == -1 || firstQuote == lastQuote) throw new Exception("Invalid string literal");
                string content = line.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
                return ResolveString($"\"{content}\"");
            }
            if (line.StartsWith("\"")) return ResolveString(line);
            if (line.StartsWith(".align"))
            {
                var p = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 2) throw new Exception(".align needs argument");
                int al = int.Parse(p[1]);
                int pad = al - (pc % al);
                if (pad == al) pad = 0;
                var r = new List<Int128>();
                for (int i = 0; i < pad; i++) r.Add(0);
                return r;
            }
            if (line.StartsWith(".word"))
            {
                var p = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 2) throw new Exception("Invalid .word");
                if (p[1].StartsWith("\"")) return ResolveString(p[1]);

                string valStr = p[1];
                if (!long.TryParse(valStr, out _) && !valStr.StartsWith("0t") && !valStr.StartsWith("0y") && !valStr.StartsWith("0n"))
                {
                    if (_externs.Contains(valStr) || (!_labels.ContainsKey(valStr) && _globals.Contains(valStr)))
                    {
                        int symIdx = obj.Symbols.Count;
                        obj.Symbols.Add(new T3ObjectFile.Symbol { Name = valStr, Type = T3ObjectFile.SymbolType.EXTERN, Section = T3ObjectFile.SectionType.ABS, Offset = 0 });
                        obj.Relocations.Add(new T3ObjectFile.Relocation { SectionOffset = pc, SymbolIndex = symIdx, Type = T3ObjectFile.RelocationType.LIMM_ABSOLUTE, Section = _currentSection });
                        return new List<Int128> { 0 };
                    }
                }
                return new List<Int128> { ResolveOperandValue(p[1]) };
            }

            int pred = 0;
            string pl = line;
            if (line.StartsWith("("))
            {
                int cp = line.IndexOf(')');
                if (cp != -1)
                {
                    string pp = line[1..cp].ToLower();
                    if (pp.StartsWith("p") && int.TryParse(pp[1..], out int pi)) { pred = pi; pl = line[(cp + 1)..].Trim(); }
                }
            }
            var ip = pl.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (ip.Length == 0) throw new Exception("Empty");
            string mn = ip[0].ToUpper();
            Opcode op = GetOpcode(mn);
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;
            if (ip.Length > 1) { if (IsRegister(ip[1])) op1 = GetRegisterTrit(ip[1]); else if (int.TryParse(ip[1], out int i1)) op1 = i1; }
            if (ip.Length > 2) { if (IsRegister(ip[2])) op2 = GetRegisterTrit(ip[2]); else if (int.TryParse(ip[2], out int i2)) op2 = i2; }
            if (ip.Length > 3) { if (IsRegister(ip[3])) op3 = GetRegisterTrit(ip[3]); else if (int.TryParse(ip[3], out int i3)) op3 = i3; }

            if (IsJumpMnemonic(mn))
            {
                string opn = ip.Length > 1 ? ip[1] : "0";
                if (!IsRegister(opn))
                {
                    if (_externs.Contains(opn) || (!_labels.ContainsKey(opn) && _globals.Contains(opn)))
                    {
                        int symIdx = obj.Symbols.Count;
                        obj.Symbols.Add(new T3ObjectFile.Symbol { Name = opn, Type = T3ObjectFile.SymbolType.EXTERN, Section = T3ObjectFile.SectionType.ABS, Offset = 0 });
                        obj.Relocations.Add(new T3ObjectFile.Relocation { SectionOffset = pc, SymbolIndex = symIdx, Type = T3ObjectFile.RelocationType.LIMM_ABSOLUTE, Section = _currentSection });
                        return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, 1, 0), (Int128)0, InstructionEncoder.EncodeJ(pred, (int)op, 1) };
                    }
                    long tgtAddr = _labels.TryGetValue(opn, out int laddr) ? laddr : (long)ResolveOperandValue(opn);
                    return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, 1, 0), (Int128)tgtAddr, InstructionEncoder.EncodeJ(pred, (int)op, 1) };
                }
                long enc = InstructionEncoder.EncodeJ(pred, (int)op, GetRegisterTrit(opn));
                return new List<Int128> { enc };
            }
            else if (mn == "LI")
            {
                string valStr = ip.Length > 2 ? ip[2] : "0";
                if (!long.TryParse(valStr, out _) && !valStr.StartsWith("0t") && !valStr.StartsWith("0y") && !valStr.StartsWith("0n"))
                {
                    if (_externs.Contains(valStr) || (!_labels.ContainsKey(valStr) && _globals.Contains(valStr)))
                    {
                        int symIdx = obj.Symbols.Count;
                        obj.Symbols.Add(new T3ObjectFile.Symbol { Name = valStr, Type = T3ObjectFile.SymbolType.EXTERN, Section = T3ObjectFile.SectionType.ABS, Offset = 0 });
                        obj.Relocations.Add(new T3ObjectFile.Relocation { SectionOffset = pc, SymbolIndex = symIdx, Type = T3ObjectFile.RelocationType.LIMM_ABSOLUTE, Section = _currentSection });
                        return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, op1, 0), (Int128)0 };
                    }
                    return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, op1, 0), (Int128)ResolveOperandValue(valStr) };
                }
                long rv = (long)ResolveOperandValue(valStr);
                if (rv > 364 || rv < -364) return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, op1, 0), (Int128)rv };
                return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LI, op1, rv) };
            }
            else if (mn == "LIMM")
            {
                string valStr = ip[2];
                if (!long.TryParse(valStr, out _) && !valStr.StartsWith("0t") && !valStr.StartsWith("0y") && !valStr.StartsWith("0n"))
                {
                    if (_externs.Contains(valStr) || (!_labels.ContainsKey(valStr) && _globals.Contains(valStr)))
                    {
                        int symIdx = obj.Symbols.Count;
                        obj.Symbols.Add(new T3ObjectFile.Symbol { Name = valStr, Type = T3ObjectFile.SymbolType.EXTERN, Section = T3ObjectFile.SectionType.ABS, Offset = 0 });
                        obj.Relocations.Add(new T3ObjectFile.Relocation { SectionOffset = pc, SymbolIndex = symIdx, Type = T3ObjectFile.RelocationType.LIMM_ABSOLUTE, Section = _currentSection });
                        return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, op1, 0), (Int128)0 };
                    }
                }
                return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)Opcode.LIMM, op1, 0), (Int128)ResolveOperandValue(ip[2]) };
            }
            else if (mn == "INI" || mn == "OUTI") { if (ip.Length > 2) imm = (long)ResolveOperandValue(ip[2]); return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)op, op1, imm) }; }
            else if (op == Opcode.LOADI || op == Opcode.STOREI)
            {
                int baseReg = 0;
                if (ip.Length > 2 && IsRegister(ip[2])) { baseReg = GetRegisterTrit(ip[2]); imm = (ip.Length > 3) ? (long)ResolveOperandValue(ip[3]) : 0; }
                else if (ip.Length > 2) { baseReg = 3; imm = (long)ResolveOperandValue(ip[2]); }
                else { baseReg = 3; imm = 0; }
                return new List<Int128> { InstructionEncoder.EncodeS(pred, (int)op, op1, baseReg, imm) };
            }
            else if (IsIType(op))
            {
                if (ip.Length > 2) imm = (long)ResolveOperandValue(ip[ip.Length - 1]);
                return new List<Int128> { InstructionEncoder.EncodeI(pred, (int)op, op1, imm) };
            }
            else return new List<Int128> { InstructionEncoder.EncodeR(pred, (int)op, op1, op2, op3) };
        }
    }
}