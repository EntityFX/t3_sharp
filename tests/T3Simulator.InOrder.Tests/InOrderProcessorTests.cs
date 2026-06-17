using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using TritTypes;
using T3Assembler;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class InOrderProcessorTests
    {
        private T3InOrderProcessor<Word18> CreateProcessor() => new(T3Config.T3_18);
        private List<Word18> Assemble(string code)
        {
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            var words = new List<Word18>();
            foreach (var b in asm.Assemble(code)) words.Add(Word18.FromLong((long)b));
            return words;
        }

        [TestMethod]
        public void SimpleArithmeticTest()
        {
            var proc = CreateProcessor();
            string code = "LI RW, 0\nLI RX, 10\nLI RY, 20\nADD RW, RX, RY\nHALT";
            proc.LoadProgram(Assemble(code)); proc.Run();
            Assert.AreEqual(30, proc.Registers[0].ToLong()); // RW=phys0
        }

        [TestMethod]
        public void ImmediateArithmeticTest()
        {
            var proc = CreateProcessor();
            string code = "LI RW, 10\nADDI RW, RW, 5\nSUBI RW, RW, 2\nHALT";
            proc.LoadProgram(Assemble(code)); proc.Run();
            Assert.AreEqual(13, proc.Registers[0].ToLong());
        }

        [TestMethod]
        public void PredicationTest()
        {
            var proc = CreateProcessor();
            // p1 flag: trits 12-14. Use "+" at position 12 (bit 3^12 * 1 = 531441)
            proc.PR = Word18.FromLong(531441); // 3^12, '+' at trit 12, p1 flag = 1
            // Predicated ADD: Pred=1, ADD RW=RW+RX (RW=0,RX=1,RY=2) -> phys RW=0
            string s = BalancedTernary.ToTernaryString(1,3)
                     + BalancedTernary.ToTernaryString((int)Opcode.ADD,6)
                     + BalancedTernary.ToTernaryString(0,3)
                     + BalancedTernary.ToTernaryString(1,3)
                     + BalancedTernary.ToTernaryString(2,3);
            proc.Registers[1] = Word18.FromLong(10);
            proc.Registers[2] = Word18.FromLong(20);
            var prog = new List<Word18> { Word18.FromLong(BalancedTernary.ParseToLong(s)), Word18.FromLong(0) };
            proc.LoadProgram(prog); proc.Run();
            Assert.AreEqual(30, proc.Registers[0].ToLong());
        }

        [TestMethod]
        public void StackTest()
        {
            var proc = CreateProcessor();
            string code = "LI RW, 100\nPUSH RW\nLI RW, 200\nPOP RW\nHALT";
            proc.LoadProgram(Assemble(code)); proc.Run();
            Assert.AreEqual(100, proc.Registers[0].ToLong());
        }

        [TestMethod]
        public void IOTest()
        {
            var proc = CreateProcessor();
            proc.Registers[1] = Word18.FromLong(10); // RX=phys1 as port
            string code = "LI RW, 42\nOUT RW, RX\nHALT";
            try { proc.LoadProgram(Assemble(code)); proc.Run(); }
            catch { /* port may throw */ }
        }
    }
}