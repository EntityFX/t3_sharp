using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using System.Collections.Generic;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void InstructionDecoder_Decode27_ValidInstruction()
        {
            // Test LI A, 10
            // Opcode LI = 4: "0000++" (6 trits)
            // Op1 A = 0: "000000000" (9 trits)
            // Op2 10 = 10: "000000+0+" (9 trits)
            // Total = 24 trits. Add 3 trailing zeros to make it a 27-trit word.
            string instrTritString = "0000++" + "000000000" + "000000+0+" + "000";
            long word = TritTypes.BalancedTernary.ParseToLong(instrTritString);
            
            var instr = InstructionDecoder.Decode27(word);
            
            Assert.AreEqual(Opcode.LI, instr.Opcode);
            Assert.AreEqual(0, instr.PredicateIndex);
            Assert.AreEqual(0, instr.Operand1);
            Assert.AreEqual(10, instr.Operand2);
        }

        [TestMethod]
        public void RegisterWindow_GetPhysicalIndex_CorrectMapping()
        {
            // WP = 0, Logical A (0) -> Physical 0
            Assert.AreEqual(0, RegisterWindow.GetPhysicalIndex(0, 0));
            // WP = 0, Logical I (8) -> Physical 8
            Assert.AreEqual(8, RegisterWindow.GetPhysicalIndex(8, 0));
            // WP = 5, Logical A (0) -> Physical 5
            Assert.AreEqual(5, RegisterWindow.GetPhysicalIndex(0, 5));
            // WP = 20, Logical I (8) -> Physical (20+8)%27 = 1
            Assert.AreEqual(1, RegisterWindow.GetPhysicalIndex(8, 20));
        }

        [TestMethod]
        public void RegisterWindow_CalculateNextWp_CorrectShift()
        {
            Assert.AreEqual(4, RegisterWindow.CalculateNextWp(0));
            Assert.AreEqual(8, RegisterWindow.CalculateNextWp(4));
            Assert.AreEqual(0, RegisterWindow.CalculateNextWp(23)); // (23+4)%27 = 0
        }

        [TestMethod]
        public void Memory_ReadWrite_CorrectValue()
        {
            var mem = new Memory(1024);
            mem.Write(10, 12345);
            Assert.AreEqual(12345, mem.Read(10));
        }
    }
}