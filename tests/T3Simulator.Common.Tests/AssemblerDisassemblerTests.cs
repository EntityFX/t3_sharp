using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;using T3Simulator.Common;using System.Collections.Generic;using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class AssemblerDisassemblerTests
    {
        static List<Word18> Asm(string src){var a=new T3InOrderAssembler(T3Config.T3_18);var r=new List<Word18>();foreach(var v in a.Assemble(src))r.Add(Word18.FromLong((long)v));return r;}
        static string Dis(List<Word18> code)=>string.Join("\n",T3Disassembler.Disassemble(code));

        [TestMethod]public void Rnd_LI_RW_10(){var c=Asm("LI RW, 10");var d=Dis(c);Assert.IsTrue(d.Contains("MOV RW, #10"));}
        [TestMethod]public void Rnd_LI_R1_neg5(){var c=Asm("LI R1, -5");var d=Dis(c);Assert.IsTrue(d.Contains("MOV R1, #-5"));}
        [TestMethod]public void Rnd_MOV_R0_R1(){var c=Asm("MOV R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("MOV R0, R1"));}
        [TestMethod]public void Rnd_ADD_R0_R1(){var c=Asm("ADD R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("ADD R0, R0, R1"));}
        [TestMethod]public void Rnd_SUB_R0_R1(){var c=Asm("SUB R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("SUB R0, R0, R1"));}
        [TestMethod]public void Rnd_MUL_R0_R1(){var c=Asm("MUL R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("MUL R0, R0, R1"));}
        [TestMethod]public void Rnd_DIV_R0_R1(){var c=Asm("DIV R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("DIV R0, R0, R1"));}
        [TestMethod]public void Rnd_MOD_R0_R1(){var c=Asm("MOD R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("MOD R0, R0, R1"));}
        [TestMethod]public void Rnd_NEG_R0(){var c=Asm("NEG R0");var d=Dis(c);Assert.IsTrue(d.Contains("NEG R0"));}
        [TestMethod]public void Rnd_AND_R0_R1(){var c=Asm("AND R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("AND R0, R0, R1"));}
        [TestMethod]public void Rnd_OR_R0_R1(){var c=Asm("OR R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("OR R0, R0, R1"));}
        [TestMethod]public void Rnd_XOR_R0_R1(){var c=Asm("XOR R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("XOR R0, R0, R1"));}
        [TestMethod]public void Rnd_SHL_R0_R1(){var c=Asm("SHL R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("SHL R0, R0, R1"));}
        [TestMethod]public void Rnd_SHR_R0_R1(){var c=Asm("SHR R0, R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("SHR R0, R0, R1"));}
        [TestMethod]public void Rnd_CMP_R0_R1(){var c=Asm("CMP R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("CMP R0, R1"));}
        [TestMethod]public void Rnd_LOAD_R0_R1(){var c=Asm("LD R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("LD R0, R1"));}
        [TestMethod]public void Rnd_STORE_R0_R1(){var c=Asm("ST R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("ST R0, R1"));}
        [TestMethod]public void Rnd_JMP_R0(){var c=Asm("JMP R0");var d=Dis(c);Assert.IsTrue(d.Contains("JMP R0"));}
        [TestMethod]public void Rnd_JE_R0(){var c=Asm("JE R0");var d=Dis(c);Assert.IsTrue(d.Contains("JE R0"));}
        [TestMethod]public void Rnd_JNE_R0(){var c=Asm("JNE R0");var d=Dis(c);Assert.IsTrue(d.Contains("JNE R0"));}
        [TestMethod]public void Rnd_JL_R0(){var c=Asm("JL R0");var d=Dis(c);Assert.IsTrue(d.Contains("JL R0"));}
        [TestMethod]public void Rnd_JG_R0(){var c=Asm("JG R0");var d=Dis(c);Assert.IsTrue(d.Contains("JG R0"));}
        [TestMethod]public void Rnd_JM_R0(){var c=Asm("JM R0");var d=Dis(c);Assert.IsTrue(d.Contains("JM R0"));}
        [TestMethod]public void Rnd_CALL_R0(){var c=Asm("CALL R0");var d=Dis(c);Assert.IsTrue(d.Contains("CALL R0"));}
        [TestMethod]public void Rnd_PUSH_R0(){var c=Asm("PUSH R0");var d=Dis(c);Assert.IsTrue(d.Contains("PUSH R0"));}
        [TestMethod]public void Rnd_POP_R0(){var c=Asm("POP R0");var d=Dis(c);Assert.IsTrue(d.Contains("POP R0"));}
        [TestMethod]public void Rnd_IN_R0_R1(){var c=Asm("IN R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("IN R0, R1"));}
        [TestMethod]public void Rnd_OUT_R0_R1(){var c=Asm("OUT R0, R1");var d=Dis(c);Assert.IsTrue(d.Contains("OUT R0, R1"));}
        [TestMethod]public void Rnd_INI_R0_5(){var c=Asm("IN R0, 5");var d=Dis(c);Assert.IsTrue(d.Contains("IN R0, #5"));}
        [TestMethod]public void Rnd_OUTI_R0_10(){var c=Asm("OUT R0, 10");var d=Dis(c);Assert.IsTrue(d.Contains("OUT R0, #10"));}
        [TestMethod]public void Rnd_LIMM_R0_12345(){var c=Asm("LIMM R0, 12345");var d=Dis(c);Assert.IsTrue(d.Contains("LIMM R0, 12345"));}
        [TestMethod]public void Rnd_Complex(){
            var c=Asm("LI R0, 42\nADD R0, R0, R1\nSUB R1, R1, R0\nHALT");
            var d=Dis(c);
            Assert.IsTrue(d.Contains("MOV R0, #42"));
            Assert.IsTrue(d.Contains("ADD R0, R0, R1"));
            Assert.IsTrue(d.Contains("HALT"));
        }
        void Check(string asm){var bin=Asm(asm);var dis=Dis(bin);var reb=Asm(dis);Assert.AreEqual(bin.Count,reb.Count,$"Round-trip: {asm}");}
        [TestMethod]public void Rnd_DisasmThenAsm(){Check("LI R0, 100\nLI R1, 200\nADD R0, R0, R1");}
        [TestMethod]public void Rnd_AllJumps(){Check("JMP R0\nJE R0\nJNE R0\nJL R0\nJG R0\nJM R0\nCALL R0\nRET\nHALT");}
        [TestMethod]public void Rnd_LIMM_full(){var asm="LIMM R0, 500000";var bin=Asm(asm);var dis=Dis(bin);Assert.IsTrue(dis.Contains("LIMM R0, 500000"));}
    }
}