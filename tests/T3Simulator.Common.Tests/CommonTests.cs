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
            // MOV RW, #10  =  I-type (Fmt=+1), RegGroup=0, Opcode=MOV(5), op1=RW(-4)
            Word18 word = Word18.FromLong(InstructionEncoder.EncodeI(0, 0, +1, (int)Opcode.MOV, -4, 10));
            var instr = InstructionDecoder.Decode(word);

            Assert.AreEqual(Opcode.MOV, instr.Opcode);
            Assert.AreEqual(0, instr.Predicate);
            Assert.AreEqual(0, instr.RegGroup);
            Assert.AreEqual(+1, instr.Fmt);
            Assert.AreEqual(-4, instr.Op1);
            Assert.AreEqual(10, instr.Immediate);
        }

        [TestMethod]
        public void RegisterWindow_GetPhysicalIndex_CorrectMapping()
        {
            Assert.AreEqual(0, RegisterWindow.GetPhysicalIndex(0, 0));
            Assert.AreEqual(8, RegisterWindow.GetPhysicalIndex(8, 0));
            Assert.AreEqual(5, RegisterWindow.GetPhysicalIndex(0, 5));
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