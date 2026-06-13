using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;
using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class T3RoundTripTests
    {
        private readonly T3Config _config = new T3Config();
        private readonly T3InOrderAssembler _assembler;

        public T3RoundTripTests()
        {
            _assembler = new T3InOrderAssembler(_config);
        }

        [DataTestMethod]
        [DataRow("LI R1, 10")]
        [DataRow("LI R15, -100")]
        [DataRow("MOV R5, R10")]
        [DataRow("ADD R1, R2, R3")]
        [DataRow("SUB R1, R1, R2")]
        [DataRow("MUL R1, R2, R3")]
        [DataRow("DIV R1, R2, R3")]
        [DataRow("MOD R1, R2, R3")]
        [DataRow("NEG R1")]
        [DataRow("TRITAND R1, R2, R3")]
        [DataRow("TRITOR R1, R2, R3")]
        [DataRow("TRITXOR R1, R2, R3")]
        [DataRow("SHL R1, R2, R3")]
        [DataRow("SHR R1, R2, R3")]
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
        [DataRow("OUTI R1, -10")]
        [DataRow("LIMM R1, 12345")]
        public void TestRoundTrip_StandardRegisters(string sourceCode)
        {
            VerifyRoundTrip(sourceCode);
        }

        [DataTestMethod]
        [DataRow("LI A, 10")]
        [DataRow("MOV B, C")]
        [DataRow("ADD D, A, B")]
        [DataRow("SUB E, D, A")]
        [DataRow("MUL F, E, B")]
        [DataRow("DIV G, F, C")]
        [DataRow("MOD H, G, A")]
        [DataRow("NEG I")]
        [DataRow("TRITAND A, B, C")]
        [DataRow("TRITOR B, C, D")]
        [DataRow("TRITXOR C, D, E")]
        [DataRow("SHL D, E, F")]
        [DataRow("SHR E, F, G")]
        [DataRow("CMP F, G")]
        [DataRow("LOAD G, H")]
        [DataRow("STORE H, I")]
        [DataRow("JMP A")]
        [DataRow("JE B")]
        [DataRow("JNE C")]
        [DataRow("JL D")]
        [DataRow("JG E")]
        [DataRow("JM F")]
        [DataRow("CALL G")]
        [DataRow("RET")]
        [DataRow("PUSH H")]
        [DataRow("POP I")]
        [DataRow("IN A, B")]
        [DataRow("OUT B, C")]
        [DataRow("INI C, 5")]
        [DataRow("OUTI D, -10")]
        [DataRow("LIMM E, 12345")]
        public void TestRoundTrip_NamedRegisters(string sourceCode)
        {
            // This test might fail if the disassembler doesn't use A-I names.
            // We use it to verify if the mapping is consistent.
            VerifyRoundTrip(sourceCode);
        }

        [TestMethod]
        public void TestRoundTrip_ComplexProgram()
        {
            string sourceCode = @"
                LI A, 10
                LI B, 20
                ADD C, A, B
                LIMM D, 500
                MOV E, C
                CMP E, D
                JG label_true
                LI F, 0
                JMP label_end
                label_true:
                LI F, 1
                label_end:
                HALT
            ";
            
            VerifyRoundTrip(sourceCode);
        }

        private void VerifyRoundTrip(string sourceCode)
        {
            // 1. Assemble
            List<Int128> binary = _assembler.Assemble(sourceCode);
            
            // 2. Disassemble
            List<Word18> words = binary.Select(Word18.FromInt128).ToList();
            List<string> disassembled = T3Disassembler.Disassemble(words);
            
            // 3. Normalize and Compare
            string normalized = NormalizeDisassembly(disassembled);
            
            // For complex programs, we compare line by line
            var expectedLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                           .Select(l => l.Trim())
                                           .Where(l => !string.IsNullOrEmpty(l) && !l.EndsWith(':'))
                                           .ToList();
            
            var actualLines = normalized.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(l => l.Trim()).ToList();

            // We can't use simple equality if we have labels or different register names.
            // But if the requirement is "correct in both directions", we should expect symmetry.
            
            // To make the test robust against label definitions, we filter them out of expected.
            // We also need to handle the case where 'A' becomes 'R0'.
            
            if (expectedLines.Count != actualLines.Count)
            {
                throw new Exception($"Instruction count mismatch. Expected {expectedLines.Count}, got {actualLines.Count}.\n" +
                                    $"Expected:\n{string.Join("\n", expectedLines)}\n" +
                                    $"Actual:\n{string.Join("\n", actualLines)}");
            }

            for (int i = 0; i < expectedLines.Count; i++)
            {
                string normExpected = NormalizeRegisters(expectedLines[i]);
                string normActual = NormalizeRegisters(actualLines[i]);

                if (normExpected != normActual)
                {
                    throw new Exception($"Mismatch at line {i + 1}.\n" +
                                        $"Expected (normalized): {normExpected}\n" +
                                        $"Actual (normalized):   {normActual}\n" +
                                        $"Original Expected: {expectedLines[i]}\n" +
                                        $"Original Actual:   {actualLines[i]}");
                }
            }
        }

        private string NormalizeDisassembly(List<string> lines)
        {
            List<string> normalizedLines = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains(": "))
                {
                    string cleaned = line.Substring(line.IndexOf(':') + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(cleaned))
                    {
                        normalizedLines.Add(cleaned);
                    }
                }
            }
            return string.Join("\n", normalizedLines);
        }

        private string NormalizeRegisters(string input)
        {
            // Simple replacement for A-I to R0-R8. 
            // We use a map to ensure we don't replace letters inside other mnemonics.
            // Since registers are usually separated by spaces or commas, we can do a basic replacement
            // if we are careful, but a more robust way is to split by tokens.
            
            string[] tokens = input.Split(new[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                string t = tokens[i].ToUpper();
                if (t.Length == 1 && t[0] >= 'A' && t[0] <= 'I')
                {
                    tokens[i] = $"R{t[0] - 'A'}";
                }
            }
            
            // This is tricky because we lost the delimiters. 
            // Let's use a simpler approach: replace using regex or a character-by-character pass.
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.ToUpper(c) >= 'A' && char.ToUpper(c) <= 'I')
                {
                    // Check if it's a standalone register
                    bool prevIsSeparator = (i == 0 || !char.IsLetterOrDigit(input[i - 1]));
                    bool nextIsSeparator = (i == input.Length - 1 || !char.IsLetterOrDigit(input[i + 1]));
                    
                    if (prevIsSeparator && nextIsSeparator)
                    {
                        sb.Append($"R{char.ToUpper(c) - 'A'}");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}