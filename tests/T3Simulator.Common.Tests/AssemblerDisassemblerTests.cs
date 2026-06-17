using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;
using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class AssemblerDisassemblerTests
    {
        private readonly T3InOrderAssembler _assembler = new(T3Config.T3_18);

        [DataTestMethod]
        [DataRow("LI R0, 10")]
        [DataRow("LI R1, -5")]
        [DataRow("MOV R0, R1")]
        [DataRow("ADD R0, R1")]
        [DataRow("SUB R0, R1")]
        [DataRow("MUL R0, R1")]
        [DataRow("DIV R0, R1")]
        [DataRow("MOD R0, R1")]
        [DataRow("NEG R0")]
        [DataRow("AND R0, R1")]
        [DataRow("OR R0, R1")]
        [DataRow("XOR R0, R1")]
        [DataRow("SHL R0, R1")]
        [DataRow("SHR R0, R1")]
        [DataRow("CMP R0, R1")]
        [DataRow("LOAD R0, R1")]
        [DataRow("STORE R0, R1")]
        [DataRow("JMP R0")]
        [DataRow("JE R0")]
        [DataRow("JNE R0")]
        [DataRow("JL R0")]
        [DataRow("JG R0")]
        [DataRow("JM R0")]
        [DataRow("CALL R0")]
        [DataRow("RET")]
        [DataRow("PUSH R0")]
        [DataRow("POP R0")]
        [DataRow("IN R0, R1")]
        [DataRow("OUT R0, R1")]
        [DataRow("INI R0, 5")]
        [DataRow("OUTI R0, 10")]
        [DataRow("LIMM R0, 12345")]
        public void TestRoundTrip_AssembleThenDisassemble(string sourceCode)
        {
            List<Int128> binary = _assembler.Assemble(sourceCode);
            List<Word18> words = binary.Select(Word18.FromInt128).ToList();
            List<string> disassembled = T3Disassembler.Disassemble(words);
            string normalized = NormalizeDisassembly(disassembled);
            Assert.AreEqual(sourceCode.Trim(), normalized);
        }

        [TestMethod]
        public void TestRoundTrip_ComplexSequence()
        {
            string sourceCode = @"LI R0, 10
LI R1, 20
ADD R2, R0, R1
LIMM R3, 500
MOV R4, R2
HALT";
            List<Int128> binary = _assembler.Assemble(sourceCode);
            List<Word18> words = binary.Select(Word18.FromInt128).ToList();
            List<string> disassembled = T3Disassembler.Disassemble(words);
            string normalized = NormalizeDisassembly(disassembled);
            var expected = sourceCode.Split(new[]{"\r\n","\r","\n"},StringSplitOptions.None).Select(l=>l.Trim()).Where(l=>!string.IsNullOrEmpty(l)).ToList();
            var actual = normalized.Split(new[]{'\n','\r'},StringSplitOptions.RemoveEmptyEntries).Select(l=>l.Trim()).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestRoundTrip_DisassembleThenAssemble()
        {
            string originalSource = "ADD R0, R1";
            List<Int128> originalBinary = _assembler.Assemble(originalSource);
            List<Word18> words = originalBinary.Select(Word18.FromInt128).ToList();
            List<string> disassembled = T3Disassembler.Disassemble(words);
            string normalizedSource = NormalizeDisassembly(disassembled);
            List<Int128> roundTripBinary = _assembler.Assemble(normalizedSource);
            CollectionAssert.AreEqual(originalBinary, roundTripBinary);
        }

        private string NormalizeDisassembly(List<string> lines)
        {
            var result = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains(": "))
                {
                    string cleaned = line.Substring(line.IndexOf(':') + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(cleaned)) result.Add(cleaned);
                }
            }
            return string.Join("\n", result);
        }
    }
}