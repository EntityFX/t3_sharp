using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TritTypes;
using T3Simulator.Common;
using T3Simulator.VLIW;
using T3Assembler;

namespace T3Simulator.VLIW.Tests
{
    [TestClass]
    public class MatrixMul6x6VliwTests
    {
        [TestMethod]
        [Timeout(120000)] 
        public void MatrixMultiplication_6x6_VLIW_Test()
        {
            // 1. Setup
            var assembler = new T3VliwAssembler(T3Config.T3_54);
            var processor = new T3VliwProcessor<Word54>(T3Config.T3_54);

            string asmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "T3Assembler", "examples", "matrix_mul_6x6_vliw.asm");
            
            if (!File.Exists(asmPath))
            {
                string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
                while (currentDir != null && !Directory.Exists(Path.Combine(currentDir, "src")))
                {
                    currentDir = Directory.GetParent(currentDir)?.FullName;
                }
                if (currentDir != null)
                {
                    asmPath = Path.Combine(currentDir, "src", "T3Assembler", "examples", "matrix_mul_6x6_vliw.asm");
                }
            }

            if (!File.Exists(asmPath))
            {
                Assert.Fail($"Assembly file not found at: {asmPath}");
            }

            string source = File.ReadAllText(asmPath);
            var rawCode = assembler.Assemble(source);
            var machineCode = rawCode.Select(x => Word54.FromInt128(x)).ToList();
            processor.LoadProgram(machineCode);

            // 2. Execute
            processor.Run();

            // 3. Verify
            // Matrix A (6x6, all 1s) * Matrix B (6x6, all 1s) = Matrix C (6x6, all 6s)
            // Data section has A(36), B(36), C(36)
            long addrC = (long)machineCode.Count - 36; 
            
            for (int i = 0; i < 36; i++)
            {
                Word54 actual = processor.ReadWord(addrC + i);
                Assert.AreEqual((Int128)6, actual.ToInt128(), $"Value at memory index {addrC + i} should be 6, but was {actual}");
            }

            Assert.IsFalse(processor.Step(), "Processor should have halted.");
        }
    }
}