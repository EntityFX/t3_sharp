using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using System.Numerics;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class InOrderProcessorTests
    {
        [TestMethod]
        public void SimpleArithmeticTest()
        {
            // Test sequence:
            // LI A, 10
            // LI B, 20
            // ADD A, B
            // HALT
            
            var proc = new T3InOrderProcessor<Word27>(T3Config.T3_27);
            
            List<Word27> program = new List<Word27>
            {
                Encode(4, 0, 10), // LI A, 10
                Encode(4, 1, 20), // LI B, 20
                Encode(6, 0, 1),  // ADD A, B
                Encode(0, 0, 0)   // HALT
            };
            
            proc.LoadProgram(program);
            proc.Run();
            
            var state = proc.GetState();
            // WP=0, Logical A -> Physical 0.
            Assert.AreEqual(30, state.Registers[0].ToLong());
        }

        private Word27 Encode(int opcode, int op1, long op2)
        {
            // Simple encoder for test purposes
            // opcode(6), op1(9), op2(9). Total 24. 
            // Must pad to 27 to ensure opcode is at the start of the Word27 string.
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(op1, 9);
            string sOp2 = ToBalancedTernary(op2, 9);
            return Word27.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + "000"));
        }

        private string ToBalancedTernary(long value, int digits)
        {
            string s = TritTypes.BalancedTernary.ToTernaryString(value);
            if (s.Length > digits) s = s.Substring(s.Length - digits);
            if (s.Length < digits) s = s.PadLeft(digits, '0');
            return s;
        }
    }
}