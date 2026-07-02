using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;using T3Simulator.Common;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class T3RoundTripTests
    {
        static List<Word18> Asm(string s){var a=new T3InOrderAssembler(T3Config.T3_18);var r=new List<Word18>();foreach(var v in a.Assemble(s))r.Add(Word18.FromLong((long)v));return r;}
        static string Dis(List<Word18> code)=>string.Join("\n",T3Disassembler.Disassemble(code));

        void Check(string asm){var bin=Asm(asm);var dis=Dis(bin);var reb=Asm(dis);Assert.AreEqual(bin.Count,reb.Count,$"Round-trip mismatch for: {asm}");}

        [TestMethod]public void Trip_SimpleArith(){Check("LI R0, 10\nLI R1, 20\nADD R2, R0, R1\nHALT");}
        [TestMethod]public void Trip_Jump(){Check("LI R0, 10\nCMP R0, R0\nJE R1\nHALT\nLI R0, 1\nHALT");}
        [TestMethod]public void Trip_Complex(){
            var asm="LI R0, 10\nLI R1, 20\nADD R2, R0, R1\nSUB R3, R2, R1\nCMP R3, R0\nJE R4\nHALT";
            var bin=Asm(asm);var dis=Dis(bin);var reb=Asm(dis);
            Assert.AreEqual(bin.Count,reb.Count);
            Assert.AreNotEqual(0,bin.Count);
        }
    }
}