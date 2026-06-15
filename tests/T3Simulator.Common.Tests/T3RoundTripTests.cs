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

        [TestMethod]
        [Timeout(30000)]
        [DataTestMethod]
        [DataRow("LI R1, 10")]
        [DataRow("LI R10, -100")]
        [DataRow("MOV R5, R10")]
        [DataRow("ADD R1, R2, R3")]
        [DataRow("SUB R1, R1, R2")]
        [DataRow("MUL R1, R2, R3")]
        [DataRow("DIV R1, R2, R3")]
        [DataRow("MOD R1, R2, R3")]
        [DataRow("NEG R1")]
        [DataRow("AND R1, R2, R3")]
        [DataRow("OR R1, R2, R3")]
        [DataRow("XOR R1, R2, R3")]
        [DataRow("SHL R1, R2, R3")]
        [DataRow("SHR R1, R2, R3")]
        [DataRow("CMP R1, R2")]
        [DataRow("LOAD R1, R2")]
        [DataRow("STORE R1, R2")]
        [DataRow("JMP R1")]
        [DataRow("JMP 10")]
        [DataRow("JE R1")]
        [DataRow("JE 20")]
        [DataRow("JNE R1")]
        [DataRow("JNE 30")]
        [DataRow("JL R1")]
        [DataRow("JL 40")]
        [DataRow("JG R1")]
        [DataRow("JG 50")]
        [DataRow("JM R1")]
        [DataRow("JM 60")]
        [DataRow("CALL R1")]
        [DataRow("CALL 70")]
        [DataRow("RET")]
        [DataRow("PUSH R1")]
        [DataRow("POP R1")]
        [DataRow("IN R1, R2")]
        [DataRow("OUT R1, R2")]
        [DataRow("INI R1, 5")]
        [DataRow("OUTI R1, -10")]
        [DataRow("LIMM R1, 12345")]
        [DataRow("(p1) ADD R1, R2, R3")]
        [DataRow("(p2) MOV R1, R2")]
        [DataRow("(p3) LI R1, 10")]
        [DataRow("(p1) JMP R1")]
        [DataRow("(p2) JMP 100")]
        public void TestRoundTrip_StandardRegisters(string sourceCode)
        {
            VerifyRoundTrip(sourceCode);
        }

        [TestMethod]
        [Timeout(30000)]
        [DataTestMethod]
        [DataRow("LI A, 10")]
        [DataRow("MOV B, C")]
        [DataRow("ADD D, A, B")]
        [DataRow("SUB E, D, A")]
        [DataRow("MUL F, E, B")]
        [DataRow("DIV G, F, C")]
        [DataRow("MOD H, G, A")]
        [DataRow("NEG I")]
        [DataRow("AND A, B, C")]
        [DataRow("OR B, C, D")]
        [DataRow("XOR C, D, E")]
        [DataRow("SHL D, E, F")]
        [DataRow("SHR E, F, G")]
        [DataRow("CMP F, G")]
        [DataRow("LOAD G, H")]
        [DataRow("STORE H, I")]
                [DataRow("JMP R0")]
                [DataRow("JE R1")]
                [DataRow("JNE R2")]
                [DataRow("JL R3")]
                [DataRow("JG R4")]
                [DataRow("JM R5")]
                [DataRow("CALL R6")]
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
        [Timeout(30000)]
        public void TestRoundTrip_ComplexProgram()
        {
            // Removed labels to ensure strict string symmetry, as disassembler 
            // only knows absolute addresses.
            string sourceCode = @"
                LI A, 10
                LI B, 20
                ADD C, A, B
                LIMM D, 500
                MOV E, C
                CMP E, D
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
                string normExpected = NormalizeShorthand(NormalizeRegisters(expectedLines[i]));
                string normActual = NormalizeShorthand(NormalizeRegisters(actualLines[i]));

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

        private string NormalizeShorthand(string input)
        {
            // Converts "OP R1, R2" to "OP R1, R1, R2" for symmetry check.
            string[] parts = input.Split(new[] { ' ' }, 2);
            if (parts.Length < 2) return input;

            string mnemonic = parts[0].ToUpper();
            string operandsPart = parts[1];
            
            string[] arithmetic = { "ADD", "SUB", "MUL", "DIV", "MOD", "AND", "OR", "XOR", "SHL", "SHR" };
            if (arithmetic.Contains(mnemonic))
            {
                string[] ops = operandsPart.Split(new[] { ',' }, StringSplitOptions.TrimEntries);
                if (ops.Length == 2)
                {
                    return $"{mnemonic} {ops[0]}, {ops[0]}, {ops[1]}";
                }
            }
            return input;
        }

        private string NormalizeRegisters(string input)
        {
            // Maps any register name to the canonical name used by the T3Disassembler
            string[] names = { "RW", "RX", "RY", "RZ", "R0", "R1", "R2", "R3", "R4" };
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (char.ToUpper(input[i]) == 'R' && i + 1 < input.Length && char.IsDigit(input[i+1]))
                {
                    // Handle R0-R26
                    int start = i + 1;
                    int end = start;
                    while (end < input.Length && char.IsDigit(input[end])) end++;
                    
                    string numStr = input.Substring(start, end - start);
                    if (int.TryParse(numStr, out int idx))
                    {
                        if (idx >= 0 && idx <= 4) sb.Append(names[idx + 4]);
                        else if (idx >= 5 && idx <= 8) sb.Append(names[idx]); // This case is actually redundant if we use a fixed map, but for safety
                        else sb.Append($"R{idx}");
                    }
                    else sb.Append('R');
                    
                    i = end - 1;
                }
                else if (char.ToUpper(input[i]) >= 'A' && char.ToUpper(input[i]) <= 'I')
                {
                    // Handle A-I
                    bool prevIsSep = (i == 0 || !char.IsLetterOrDigit(input[i - 1]));
                    bool nextIsSep = (i == input.Length - 1 || !char.IsLetterOrDigit(input[i + 1]));
                    
                    if (prevIsSep && nextIsSep)
                    {
                        int idx = char.ToUpper(input[i]) - 'A';
                        sb.Append(idx < names.Length ? names[idx] : $"R{idx}");
                    }
                    else sb.Append(input[i]);
                }
                else
                {
                    sb.Append(input[i]);
                }
            }
            
            // Special case for R0-R4 that might have been passed as R0-R4 but should be mapped to indices 4-8
            // Wait, the logic above handles R(0-4) -> names[4-8].
            // Let's refine the R-prefix handling to be more precise.
            return sb.ToString();
        }
    }
}