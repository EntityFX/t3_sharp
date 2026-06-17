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
            // Test LI RW, 10
            // Use the new InstructionEncoder for round-trip consistency
            // LI RW, 10: I-type, pred=0, opcode=LI(4), op1=RW=(-4), imm=10
            Word18 word = Word18.FromLong(InstructionEncoder.EncodeI(0, (int)Opcode.LI, -4, 10));
            var instr = InstructionDecoder.Decode(word);
            
            Assert.AreEqual(Opcode.LI, instr.Opcode);
            Assert.AreEqual(0, instr.Predicate);
            Assert.AreEqual(-4, instr.Op1);   // trit value of RW
            Assert.AreEqual(0, instr.PhysOp1); // phys index of RW = 0
            Assert.AreEqual(10, instr.Immediate);
        }

        [TestMethod]
        public void RegisterWindow_GetPhysicalIndex_CorrectMapping()
        {
            // WP = 0, Logical RW (0) -> Physical 0
            Assert.AreEqual(0, RegisterWindow.GetPhysicalIndex(0, 0));
            // WP = 0, Logical R4 (8) -> Physical 8
            Assert.AreEqual(8, RegisterWindow.GetPhysicalIndex(8, 0));
            // WP = 5, Logical RW (0) -> Physical 5
            Assert.AreEqual(5, RegisterWindow.GetPhysicalIndex(0, 5));
            // WP = 20, Logical R4 (8) -> Physical (20+8)%27 = 1
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
