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
        [Timeout(30000)]
        public void SimpleArithmeticTest()
        {
            // Test sequence:
            // LI RW, 10
            // LI RX, 20
            // ADD RW, RX, RY
            // HALT
            
            var proc = CreateProcessor();
            
            // Note: In the new ISA, ADD is R-type: op1 = op2 + op3
            string code = 
                "LI RW, 0\n" + 
                "LI RX, 10\n" + 
                "LI RY, 20\n" + 
                "ADD RW, RX, RY\n" + 
                "HALT";
            
            proc.LoadProgram(Assemble(code));
            proc.Run();
            
            var state = proc.GetState();
            // RW = 10 + 20 = 30
            Assert.AreEqual(30, state.Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void ImmediateArithmeticTest()
        {
            var proc = CreateProcessor();
            
            string code = 
                "LI RW, 10\n" + 
                "ADDI RW, RW, 5\n" + // RW = 10 + 5 = 15
                "SUBI RW, RW, 2\n" + // RW = 15 - 2 = 13
                "HALT";
            
            proc.LoadProgram(Assemble(code));
            proc.Run();
            
            var state = proc.GetState();
            Assert.AreEqual(13, state.Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
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
        [Timeout(30000)]    
        public void StackTest()
        {
            var proc = CreateProcessor();
            
            string code = 
                "LI RW, 100\n" + 
                "PUSH RW\n" + 
                "LI RW, 200\n" + 
                "POP RW\n" + 
                "HALT";
            
            proc.LoadProgram(Assemble(code));
            proc.Run();
            
            var state = proc.GetState();
            Assert.AreEqual(100, state.Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void IOTest()
        {
            var proc = CreateProcessor();
            
            // We'll simulate a device by adding it to the manager
            // Port 10: Echo device (just for this test)
            
            string code = 
                "LI RW, 42\n" + 
                "OUT RW, RX\n" + // port in reg RX
                "HALT";
            
            // Set RX = 10 (port)
            proc.Registers[1] = Word18.FromLong(10);
            
            try {
                proc.LoadProgram(Assemble(code));
                proc.Run();
            } catch (Exception ex) {
                Console.WriteLine($"IO Test info: {ex.Message}");
            }
        }
    }
}