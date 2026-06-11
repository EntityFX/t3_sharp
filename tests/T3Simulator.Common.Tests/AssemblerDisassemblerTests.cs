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
        private readonly T3Config _config = new T3Config();
        private readonly T3InOrderAssembler _assembler;

        public AssemblerDisassemblerTests()
        {
            _assembler = new T3InOrderAssembler(_config);
        }

        [DataTestMethod]
        [DataRow("LI R1, 10")]
        [DataRow("LI R2, -5")]
        [DataRow("MOV R1, R2")]
        [DataRow("ADD R1, R2")]
        [DataRow("SUB R1, R2")]
        [DataRow("MUL R1, R2")]
        [DataRow("DIV R1, R2")]
        [DataRow("MOD R1, R2")]
        [DataRow("NEG R1")]
        [DataRow("TRITAND R1, R2")]
        [DataRow("TRITOR R1, R2")]
        [DataRow("TRITXOR R1, R2")]
        [DataRow("SHL R1, R2")]
        [DataRow("SHR R1, R2")]
        [DataRow("CMP R1, R2")]
        [DataRow("LOAD R1, R2")]
        [DataRow("STORE R1, R2")]
        [DataRow("JMP R1")]
        [DataRow("JE R1")]
        [DataRow("JNE R1")]
        [DataRow("JL R1")]
        [DataRow("JG R1")]
        [DataRow("JM R1")]
        [DataRow("CALL R1")]
        [DataRow("RET")]
        [DataRow("PUSH R1")]
        [DataRow("POP R1")]
        [DataRow("IN R1, R2")]
        [DataRow("OUT R1, R2")]
        [DataRow("INI R1, 5")]
        [DataRow("OUTI R1, 10")]
        [DataRow("LIMM R1, 12345")]
        public void TestRoundTrip_AssembleThenDisassemble(string sourceCode)
        {
            // Assemble
            List<Int128> binary = _assembler.Assemble(sourceCode);
            
            // Convert to Word18 for disassembler
            List<Word18> words = binary.Select(Word18.FromInt128).ToList();
            
            // Disassemble
            List<string> disassembled = T3Disassembler.Disassemble(words);
            
            // Normalize
            string normalized = NormalizeDisassembly(disassembled);
            
            // Compare
            Assert.AreEqual(sourceCode.Trim(), normalized);
        }

        [TestMethod]
        public void TestRoundTrip_ComplexSequence()
        {
            string sourceCode = @"
                LI R1, 10
                LI R2, 20
                ADD R3, R1, R2
                LIMM R4, 500
                MOV R5, R3
                HALT
            ";

            List<Int128> binary = _assembler.Assemble(sourceCode);
            List<Word18> words = binary.Select(Word18.FromInt128).ToList();
            List<string> disassembled = T3Disassembler.Disassemble(words);
            string normalized = NormalizeDisassembly(disassembled);

            // We compare line by line since sourceCode has whitespace/newlines
            var expectedLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(l => l.Trim()).ToList();
            var actualLines = normalized.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(l => l.Trim()).ToList();

            CollectionAssert.AreEqual(expectedLines, actualLines);
        }

        [TestMethod]
        public void TestRoundTrip_DisassembleThenAssemble()
        {
            // Pre-defined binary for "ADD R1, R2"
            string originalSource = "ADD R1, R2";
            List<Int128> originalBinary = _assembler.Assemble(originalSource);
            
            // Disassemble
            List<Word18> words = originalBinary.Select(Word18.FromInt128).ToList();
            List<string> disassembled = T3Disassembler.Disassemble(words);
            
            // Normalize to something the assembler can take
            string normalizedSource = NormalizeDisassembly(disassembled);
            
            // Assemble again
            List<Int128> roundTripBinary = _assembler.Assemble(normalizedSource);
            
            // Compare binary results
            CollectionAssert.AreEqual(originalBinary, roundTripBinary);
        }

        private string NormalizeDisassembly(List<string> lines)
        {
            List<string> normalizedLines = new List<string>();
            foreach (var line in lines)
            {
                // Remove address prefix "00000000: "
                string cleaned = line;
                if (cleaned.Contains(": "))
                {
                    cleaned = cleaned.Substring(cleaned.IndexOf(':') + 1).Trim();
                }
                else
                {
                    continue; // Skip "  -> Immediate: ..." lines
                }

                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    normalizedLines.Add(cleaned);
                }
            }
            return string.Join("\n", normalizedLines);
        }
    }
}