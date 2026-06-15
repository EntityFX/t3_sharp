using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TritTypes;
using T3Simulator.Common;
using T3Simulator.InOrder;
using T3Assembler;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MatrixMul6x6Tests
    {
        [TestMethod]
        [Timeout(30000)]
        public void MatrixMultiplication_6x6_InOrder_Test()
        {
            // 1. Setup
            var assembler = new T3InOrderAssembler(T3Config.T3_18);
            var processor = new T3InOrderProcessor<Word18>(T3Config.T3_18);

            string asmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "T3Assembler", "examples", "matrix_mul_6x6_inorder.asm");
            
            // Path discovery for local development
            if (!File.Exists(asmPath))
            {
                string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
                while (currentDir != null && !Directory.Exists(Path.Combine(currentDir, "src")))
                {
                    currentDir = Directory.GetParent(currentDir)?.FullName;
                }
                if (currentDir != null)
                {
                    asmPath = Path.Combine(currentDir, "src", "T3Assembler", "examples", "matrix_mul_6x6_inorder.asm");
                }
            }

            if (!File.Exists(asmPath))
            {
                Assert.Fail($"Assembly file not found at: {asmPath}");
            }

            string source = File.ReadAllText(asmPath);
            var machineCode = assembler.Assemble(source).Select(x => Word18.FromInt128(x)).ToList();
            processor.LoadProgram(machineCode);

            // 2. Execute
            processor.Run();

            // 3. Verify
            // Matrix A (6x6, all 1s) * Matrix B (6x6, all 1s) = Matrix C (6x6, all 6s)
            long addrC = machineCode.Count - 36; 
            
            for (int i = 0; i < 36; i++)
            {
                Word18 actual = processor.ReadWord(addrC + i);
                Assert.AreEqual(6, actual.ToLong(), $"Value at memory index {addrC + i} should be 6, but was {actual}");
            }

            Assert.IsFalse(processor.Step(), "Processor should have halted.");
        }
    }
}