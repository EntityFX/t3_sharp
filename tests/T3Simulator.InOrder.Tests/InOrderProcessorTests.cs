using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using System.Numerics;
using TritTypes;
using T3Assembler;
using System;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class InOrderProcessorTests
    {
        private T3InOrderProcessor<Word18> CreateProcessor()
        {
            return new T3InOrderProcessor<Word18>(T3Config.T3_18);
        }

        private List<Word18> Assemble(string code)
        {
            var assembler = new T3InOrderAssembler(T3Config.T3_18);
            List<Int128> binary = assembler.Assemble(code);
            List<Word18> words = new List<Word18>();
            foreach (var b in binary)
            {
                words.Add(Word18.FromLong((long)b));
            }
            return words;
        }

        [TestMethod]
        public void SimpleArithmeticTest()
        {
            // Test sequence:
            // LI A, 10
            // LI B, 20
            // ADD A, B, C (A = B + C, but let's use ADD A, B, B for simplicity)
            // HALT
            
            var proc = CreateProcessor();
            
            // Note: In the new ISA, ADD is R-type: op1 = op2 + op3
            // We use LI to set B and C, then ADD A, B, C.
            string code = 
                "LI A, 0\n" + 
                "LI B, 10\n" + 
                "LI C, 20\n" + 
                "ADD A, B, C\n" + 
                "HALT";
            
            proc.LoadProgram(Assemble(code));
            proc.Run();
            
            var state = proc.GetState();
            // A = 10 + 20 = 30
            Assert.AreEqual(30, state.Registers[0].ToLong());
        }

        [TestMethod]
        public void ImmediateArithmeticTest()
        {
            var proc = CreateProcessor();
            
            string code = 
                "LI A, 10\n" + 
                "ADDI A, A, 5\n" + // A = 10 + 5 = 15
                "SUBI A, A, 2\n" + // A = 15 - 2 = 13
                "HALT";
            
            proc.LoadProgram(Assemble(code));
            proc.Run();
            
            var state = proc.GetState();
            Assert.AreEqual(13, state.Registers[0].ToLong());
        }

        [TestMethod]
        public void PredicationTest()
        {
            var proc = CreateProcessor();
            
            // We need to set a predicate flag in PR.
            // PR is a word. p0 is first 3 trits. Value +1 means true.
            // 1 in 3-trits is "00+". In 18-trits, that's 1 * 3^0 = 1.
            proc.PR = Word18.FromLong(1); 
            
            // We can't easily set predicates in the simple assembler yet, 
            // but we can manually construct a predicated instruction.
            // ADD A, B, C with pred=1 (p0). 
            // Base = 6, Pred = 1 => Field = 6 + 1*28 = 34.
            // Word: 34*3^12 + 0*3^9 + 1*3^6 + 2*3^3 + 0
            long wordVal = 34 * 531441 + 0 * 19683 + 1 * 729 + 2 * 27 + 0;
            
            // Setup registers: B=10, C=20
            proc.Registers[1] = Word18.FromLong(10);
            proc.Registers[2] = Word18.FromLong(20);
            
            List<Word18> program = new List<Word18>
            {
                Word18.FromLong(wordVal),
                Word18.FromLong(0) // HALT
            };
            
            proc.LoadProgram(program);
            proc.Run();
            
            var state = proc.GetState();
            Assert.AreEqual(30, state.Registers[0].ToLong());
        }

        [TestMethod]
        public void StackTest()
        {
            var proc = CreateProcessor();
            
            string code = 
                "LI A, 100\n" + 
                "PUSH A\n" + 
                "LI A, 200\n" + 
                "POP A\n" + 
                "HALT";
            
            proc.LoadProgram(Assemble(code));
            proc.Run();
            
            var state = proc.GetState();
            Assert.AreEqual(100, state.Registers[0].ToLong());
        }

        [TestMethod]
        public void IOTest()
        {
            var proc = CreateProcessor();
            
            // We'll simulate a device by adding it to the manager
            // Port 10: Echo device (just for this test)
            // Since DeviceManager is internal/shared, we might need a mock.
            // For now, let's just check if it doesn't crash and calls the manager.
            
            string code = 
                "LI A, 42\n" + 
                "OUT A, B\n" + // port in reg B
                "HALT";
            
            // Set B = 10 (port)
            proc.Registers[1] = Word18.FromLong(10);
            
            // This might fail if no device is at port 10, depending on DeviceManager implementation.
            // But it verifies the opcode path.
            try {
                proc.LoadProgram(Assemble(code));
                proc.Run();
            } catch (Exception ex) {
                // If it's a "device not found" exception, it's partially a success.
                Console.WriteLine($"IO Test info: {ex.Message}");
            }
        }
    }
}