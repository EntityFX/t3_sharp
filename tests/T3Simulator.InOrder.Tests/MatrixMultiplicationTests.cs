using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using TritTypes;
using T3Simulator.Common;
using T3Simulator.InOrder;
using T3Assembler;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MatrixMultiplicationTests
    {
        [TestMethod]
        public void MatrixMultiplication_IntegrationTest()
        {
            // 1. Setup Assembler and Processor
            var assembler = new T3AssemblerCore(T3Config.T3_27);
            var processor = new T3InOrderProcessor<long>(T3Config.T3_27);

            // 2. Load the matrix multiplication assembly code
            // We use a more robust path discovery to find the src directory from the test execution folder
            string asmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src", "T3Assembler", "examples", "matrix_mul.asm");
            
            if (!File.Exists(asmPath))
            {
                // Try to climb up from bin/Debug/net8.0/
                string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
                while (currentDir != null && !Directory.Exists(Path.Combine(currentDir, "src")))
                {
                    currentDir = Directory.GetParent(currentDir)?.FullName;
                }

                if (currentDir == null)
                {
                    Assert.Fail("Could not find project root directory containing 'src' folder.");
                }
                asmPath = Path.Combine(currentDir, "src", "T3Assembler", "examples", "matrix_mul.asm");
            }

            if (!File.Exists(asmPath))
            {
                Assert.Fail($"Assembly file not found at: {asmPath}");
            }

            string source = File.ReadAllText(asmPath);
            List<long> machineCode = assembler.Assemble(source);
            processor.LoadProgram(machineCode);

            // 3. Execute the program
            processor.Run();

            // 4. Verify results
            // Based on the assembly:
            // A = [1, 2; 3, 4], B = [5, 6; 7, 8]
            // C[0][0] = 1*5 + 2*7 = 5 + 14 = 19
            // C[0][1] = 1*6 + 2*8 = 6 + 16 = 22
            // C[1][0] = 3*5 + 4*7 = 15 + 28 = 43
            // C[1][1] = 3*6 + 4*8 = 18 + 32 = 50

            // The data section: addr_A (4), addr_B (4), addr_C (4).
            // Since the data is at the end of the program, addr_C starts at machineCode.Length - 4.
            long addrC = machineCode.Count - 4;
            long[] expectedC = { 19, 22, 43, 50 };
            
            for (int i = 0; i < 4; i++)
            {
                long actual = processor.ReadWord(addrC + i);
                Assert.AreEqual(expectedC[i], actual, $"Value at memory index {addrC + i} should be {expectedC[i]}, but was {actual}");
            }

            Assert.IsFalse(processor.Step(), "Processor should have halted.");
        }
    }
}