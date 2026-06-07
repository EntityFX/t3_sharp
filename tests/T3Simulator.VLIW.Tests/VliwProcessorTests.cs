using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.VLIW;
using System.Collections.Generic;
using System.Linq;
using TritTypes;
using System;
using T3Assembler;

namespace T3Simulator.VLIW.Tests
{
    [TestClass]
    public class VliwProcessorTests
    {
        private T3VliwProcessor CreateProcessor()
        {
            return new T3VliwProcessor();
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_BasicVLIW_Parallelism()
        {
            var proc = CreateProcessor();
            var assembler = new T3AssemblerCore(T3Config.T3_54);
            
            // In VLIW, we can put 3 instructions in one word.
            // We'll test if 3 LI instructions execute in parallel.
            // Note: T3AssemblerCore doesn't explicitly support VLIW bundles in a high-level way,
            // but for T3-54 it emits Word54.
            
            string asm = @"
                LI A, 10
                LI B, 20
                LI C, 30
                HALT
            ";
            
            var program = assembler.Assemble(asm);
            proc.LoadProgram(program.Select(x => (Word54)x).ToList());
            proc.Run();
            
            Assert.AreEqual(10, proc.GetState().Registers[0]);
            Assert.AreEqual(20, proc.GetState().Registers[1]);
            Assert.AreEqual(30, proc.GetState().Registers[2]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_VLIW_SIMD_VADD3()
        {
            var proc = CreateProcessor();
            var assembler = new T3AssemblerCore(T3Config.T3_54);
            
            // Vector ADD: A = A + B
            // Assuming A and B are initialized with some values
            string asm = @"
                LI A, 10
                LI B, 20
                VADD3 A, A, B
                HALT
            ";
            
            var program = assembler.Assemble(asm);
            proc.LoadProgram(program.Select(x => (Word54)x).ToList());
            proc.Run();
            
            // VADD3 for T3-54 works on 3 segments of 18 trits.
            // Since we only set the whole word to 10 and 20, the result should be 30.
            Assert.AreEqual(30, proc.GetState().Registers[0]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_VLIW_Speculation()
        {
            var proc = CreateProcessor();
            var assembler = new T3AssemblerCore(T3Config.T3_54);
            
            string asm = @"
                LI A, 10
                SPEK
                LI A, 20
                ROLLBACK
                HALT
            ";
            
            var program = assembler.Assemble(asm);
            proc.LoadProgram(program.Select(x => (Word54)x).ToList());
            proc.Run();
            
            // After ROLLBACK, A should be 10 again
            Assert.AreEqual(10, proc.GetState().Registers[0]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_VLIW_Speculation_Commit()
        {
            var proc = CreateProcessor();
            var assembler = new T3AssemblerCore(T3Config.T3_54);
            
            string asm = @"
                LI A, 10
                SPEK
                LI A, 20
                COMMIT
                HALT
            ";
            
            var program = assembler.Assemble(asm);
            proc.LoadProgram(program.Select(x => (Word54)x).ToList());
            proc.Run();
            
            // After COMMIT, A should be 20
            Assert.AreEqual(20, proc.GetState().Registers[0]);
        }
    }
}