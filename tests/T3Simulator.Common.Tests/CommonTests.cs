using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void InstructionDecoder_Decode18_ValidInstruction()
        {
            // Test LI A, 10
            // Opcode LI = 4: "0000++" (6 trits)
            // Op1 A = 0: "000" (3 trits)
            // Op2 10 = 10: "+0+" (3 trits)
            // Op3 = 0: "000" (3 trits)
            // Reserve = "000" (3 trits)
            // Total = 18 trits.
            string instrTritString = "000" + "0000++" + "000" + "000+0+";
            Word18 word = Word18.Parse(instrTritString);
            
            var instr = InstructionDecoder.Decode(word);
            
            Assert.AreEqual(Opcode.LI, instr.Opcode);
            Assert.AreEqual(0, instr.PredicateIndex);
            Assert.AreEqual(0, instr.Operand1);
            Assert.AreEqual(10, instr.Immediate);
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
            long wp = 0;
            long nextWp = RegisterWindow.CalculateNextWp(wp);
            Assert.AreEqual(23, nextWp);
        }

        [TestMethod]
        public void Memory_ReadWrite_CorrectValue()
        {
            var mem = new Memory<long>(1024);
            mem.Write(10, 12345);
            Assert.AreEqual(12345, mem.Read(10));
        }
    }
}
