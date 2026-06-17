using System;using System.Collections.Generic;using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;using T3Simulator.Common;using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class T3RoundTripTests
    {
        readonly T3InOrderAssembler _asm = new(T3Config.T3_18);

        [TestMethod][DataTestMethod]
        [DataRow("LI R0, 10")][DataRow("LI R1, -100")][DataRow("MOV R0, R1")]
        [DataRow("LI RW, 10")][DataRow("LI RX, -5")][DataRow("MOV RY, RZ")]
        [DataRow("ADD R0, R1, R2")][DataRow("SUB R0, R1, R2")][DataRow("MUL R0, R1, R2")]
        [DataRow("DIV R0, R1, R2")][DataRow("MOD R0, R1, R2")][DataRow("NEG R0")]
        [DataRow("AND R0, R1, R2")][DataRow("OR R0, R1, R2")][DataRow("XOR R0, R1, R2")]
        [DataRow("SHL R0, R1, R2")][DataRow("SHR R0, R1, R2")][DataRow("CMP R0, R1")]
        [DataRow("LOAD R0, R1")][DataRow("STORE R0, R1")]
        [DataRow("JMP R0")][DataRow("JMP 10")][DataRow("JE R0")][DataRow("JE 20")]
        [DataRow("JNE R0")][DataRow("JNE 30")][DataRow("JL R0")][DataRow("JL 40")]
        [DataRow("JG R0")][DataRow("JG 50")][DataRow("JM R0")][DataRow("JM 60")]
        [DataRow("JLE R0")][DataRow("JGE R0")][DataRow("CALL R0")][DataRow("CALL 70")]
        [DataRow("RET")][DataRow("PUSH R0")][DataRow("POP R0")]
        [DataRow("IN R0, R1")][DataRow("OUT R0, R1")]
        [DataRow("INI R0, 5")][DataRow("OUTI R0, -10")][DataRow("LIMM R0, 12345")]
        [DataRow("(p1) ADD R0, R1, R2")][DataRow("(p2) MOV R0, R1")]
        [DataRow("(p3) LI R0, 10")][DataRow("(p1) JMP R0")][DataRow("(p2) JMP 100")]
        public void TestRoundTrip(string src) => VerifyRoundTrip(src);

        [TestMethod] public void TestRoundTrip_ComplexProgram() => VerifyRoundTrip("LI R0,10\nLI R1,20\nADD R2,R0,R1\nLIMM R3,500\nMOV R4,R2\nCMP R4,R3\nHALT");

        void VerifyRoundTrip(string src)
        {
            var bin = _asm.Assemble(src);
            var words = bin.Select(Word18.FromInt128).ToList();
            var disasm = T3Disassembler.Disassemble(words);
            var exp = src.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries).Select(l=>l.Trim()).Where(l=>l.Length>0&&!l.EndsWith(":")).ToList();
            var act = new List<string>();
            foreach(var l in disasm){int i=l.IndexOf(':');if(i>=0)act.Add(l[(i+1)..].Trim());}
            if(exp.Count!=act.Count)throw new Exception($"Line count mismatch. Exp:{exp.Count} Act:{act.Count}\nExp:{string.Join("|",exp)}\nAct:{string.Join("|",act)}");
            for(int j=0;j<exp.Count;j++){
                var en = Norm(exp[j]); var an = Norm(act[j]);
                if(en!=an)throw new Exception($"Line {j+1}: Exp:{en} Act:{an}");
            }
        }
        static string Norm(string s){
            var p=s.Split(' ');if(p.Length<2)return s;
            string m=p[0].ToUpper(),o=p[1];
            o=o.Replace("RW","R0").Replace("RX","R1").Replace("RY","R2").Replace("RZ","R3");
            if(new[]{ "ADD","SUB","MUL","DIV","MOD","AND","OR","XOR","SHL","SHR"}.Contains(m)){
                var a=o.Split(',',StringSplitOptions.TrimEntries);
                if(a.Length==2)return $"{m} {a[0]}, {a[0]}, {a[1]}";
            }
            return $"{m} {o}";
        }
    }
}