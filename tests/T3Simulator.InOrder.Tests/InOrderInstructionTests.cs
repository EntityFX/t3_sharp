using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using TritTypes;
using System;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class InOrderInstructionTests
    {
        private long Encode(int opcode, int op1, long op2, int pred = 0)
        {
            long fullOpcode = pred * 45 + opcode;
            string sOp = ToBalancedTernary(fullOpcode, 6);
            string sOp1 = ToBalancedTernary(op1, 9);
            string sOp2 = ToBalancedTernary(op2, 9);
            return BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + "000");
        }

        private string ToBalancedTernary(long value, int digits)
        {
            string s = TritTypes.BalancedTernary.ToTernaryString(value);
            if (s.Length > digits) s = s.Substring(s.Length - digits);
            if (s.Length < digits) s = s.PadLeft(digits, '0');
            return s;
        }

        private T3InOrderProcessor<long> CreateProcessor()
        {
            return new T3InOrderProcessor<long>(T3Config.T3_27);
        }

        [TestMethod]
        public void Test_ADD()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 10), // LI A, 10
                Encode(4, 1, 20), // LI B, 20
                Encode(6, 0, 1),  // ADD A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(30, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_SUB()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 50), // LI A, 50
                Encode(4, 1, 20), // LI B, 20
                Encode(7, 0, 1),  // SUB A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(30, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_MUL()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 5),  // LI A, 5
                Encode(4, 1, 6),  // LI B, 6
                Encode(8, 0, 1),  // MUL A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(30, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_DIV()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 30), // LI A, 30
                Encode(4, 1, 4),  // LI B, 4
                Encode(9, 0, 1),  // DIV A, B -> floor(30/4) = 7
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(7, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_MOD()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 30), // LI A, 30
                Encode(4, 1, 4),  // LI B, 4
                Encode(10, 0, 1), // MOD A, B -> 30 % 4 = 2
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(2, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_NEG()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 15), // LI A, 15
                Encode(11, 0, 0), // NEG A
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(-15, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_TRITAND()
        {
            var proc = CreateProcessor();
            // 1 in balanced ternary is "0...0+"
            // 0 is "0...00"
            // TritAnd(1, 0) = 0
            var program = new List<long>
            {
                Encode(4, 0, 1),  // LI A, 1
                Encode(4, 1, 0),  // LI B, 0
                Encode(12, 0, 1), // TRITAND A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(0, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_TRITOR()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 1),  // LI A, 1
                Encode(4, 1, 0),  // LI B, 0
                Encode(13, 0, 1), // TRITOR A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(1, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_TRITXOR()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 1),  // LI A, 1
                Encode(4, 1, 1),  // LI B, 1
                Encode(14, 0, 1), // TRITXOR A, B -> 1+1=2 -> -1 (mod 3)
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(-1, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_SHL()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 2),  // LI A, 2
                Encode(4, 1, 2),  // LI B, 2 (shift by 2: * 3^2 = * 9)
                Encode(15, 0, 1), // SHL A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(18, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_SHR()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 18), // LI A, 18
                Encode(4, 1, 2),  // LI B, 2 (shift by 2: / 9)
                Encode(16, 0, 1), // SHR A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(2, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_CMP_And_Branches()
        {
            var proc = CreateProcessor();
            // Test: if (A > B) A = 1 else A = -1
            // LI A, 10
            // LI B, 20
            // CMP A, B -> Cond = -1 (A < B)
            // JG target_gt
            // LI A, -1
            // JMP end
            // target_gt: LI A, 1
            // end: HALT
            
            List<long> program = new List<long>();
            program.Add(Encode(4, 0, 10)); // A=10
            program.Add(Encode(4, 1, 20)); // B=20
            program.Add(Encode(17, 0, 1)); // CMP A, B -> Cond=-1
            
            // JG (jump if Cond > 0)
            // We need to jump to LI A, 1. 
            // Current PC: 3. target_gt is at index 6.
            program.Add(Encode(22, 0, 6)); // JG to index 6 (logical register 6 is used here as absolute address for simplicity in this test)
            // Note: In a real processor, JMP/JE etc use the value in a register.
            // Let's fix the program to use a register for the address.
            
            // Fixed program:
            program.Clear();
            program.Add(Encode(4, 2, 6));  // R2 = 6 (target address)
            program.Add(Encode(4, 0, 10)); // A=10
            program.Add(Encode(4, 1, 20)); // B=20
            program.Add(Encode(17, 0, 1)); // CMP A, B
            program.Add(Encode(22, 2, 0)); // JG R2
            program.Add(Encode(4, 0, -1)); // A = -1
            program.Add(Encode(18, 2, 0)); // JMP R2 (wait, need to jump to end)
            
            // Let's rethink. 
            // PC 0: LI R2, 7 (address of HALT)
            // PC 1: LI A, 10
            // PC 2: LI B, 20
            // PC 3: CMP A, B
            // PC 4: JG R2 (jump to HALT)
            // PC 5: LI A, 1
            // PC 6: HALT
            
            program.Clear();
            program.Add(Encode(4, 2, 6)); // PC 0: R2 = 6
            program.Add(Encode(4, 0, 10)); // PC 1: A = 10
            program.Add(Encode(4, 1, 20)); // PC 2: B = 20
            program.Add(Encode(17, 0, 1)); // PC 3: CMP A, B -> Cond = -1
            program.Add(Encode(22, 2, 0)); // PC 4: JG R2 (Condition -1, so NO jump)
            program.Add(Encode(4, 0, 1));  // PC 5: A = 1
            program.Add(Encode(0, 0, 0));  // PC 6: HALT
            
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(1, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_Loop_Sum()
        {
            var proc = CreateProcessor();
            // Calculate sum of 1..5
            // R0: sum = 0
            // R1: i = 1
            // R2: limit = 5
            // R3: constant 1
            // R4: target address for loop start
            
            // PC 0: LI R0, 0
            // PC 1: LI R1, 1
            // PC 2: LI R2, 5
            // PC 3: LI R3, 1
            // PC 4: LI R4, 5 (loop start)
            // PC 5: ADD R0, R1
            // PC 6: ADD R1, R3
            // PC 7: CMP R1, R2 (is i <= limit?)
            // PC 8: JE end (if i == 6, stop) - Wait, the spec says JE is Cond == 0.
            // Let's use: if (R1 == R2+1) break.
            
            // Revised:
            // PC 0: LI R0, 0
            // PC 1: LI R1, 1
            // PC 2: LI R2, 6 (break when i == 6)
            // PC 3: LI R3, 1
            // PC 4: LI R4, 5 (loop start)
            // PC 5: ADD R0, R1
            // PC 6: ADD R1, R3
            // PC 7: CMP R1, R2
            // PC 8: JE R5 (address of HALT)
            // PC 9: JMP R4 (loop start)
            // PC 10: HALT
            
            List<long> program = new List<long>();
            program.Add(Encode(4, 0, 0)); // PC 0: sum = 0
            program.Add(Encode(4, 1, 1)); // PC 1: i = 1
            program.Add(Encode(4, 2, 6)); // PC 2: limit = 6
            program.Add(Encode(4, 3, 1)); // PC 3: const 1
            program.Add(Encode(4, 4, 5)); // PC 4: loop_start_addr = 5
            program.Add(Encode(6, 0, 1)); // PC 5: sum += i
            program.Add(Encode(6, 1, 3)); // PC 6: i += 1
            program.Add(Encode(17, 1, 2)); // PC 7: CMP i, limit
            program.Add(Encode(19, 5, 0)); // PC 8: JE R5 (R5 is address of HALT)
            // Wait, I need to set R5 = 10.
            
            program.Clear();
            program.Add(Encode(4, 0, 0)); // PC 0: sum = 0
            program.Add(Encode(4, 1, 1)); // PC 1: i = 1
            program.Add(Encode(4, 2, 6)); // PC 2: limit = 6
            program.Add(Encode(4, 3, 1)); // PC 3: const 1
            program.Add(Encode(4, 4, 5)); // PC 4: loop_start = 5
            program.Add(Encode(4, 5, 10)); // PC 5: end_addr = 10 (Wait, I'm using R5 for end_addr)
            
            // Shift things:
            // PC 0: sum=0, PC 1: i=1, PC 2: limit=6, PC 3: c1=1, PC 4: loop=6, PC 5: end=11
            // PC 6: sum+=i, PC 7: i+=1, PC 8: CMP i, limit, PC 9: JE end, PC 10: JMP loop, PC 11: HALT
            
            program.Clear();
            program.Add(Encode(4, 0, 0)); // 0: R0 = 0
            program.Add(Encode(4, 1, 1)); // 1: R1 = 1
            program.Add(Encode(4, 2, 6)); // 2: R2 = 6
            program.Add(Encode(4, 3, 1)); // 3: R3 = 1
            program.Add(Encode(4, 4, 6)); // 4: R4 = 6 (loop)
            program.Add(Encode(4, 5, 11)); // 5: R5 = 11 (end)
            program.Add(Encode(6, 0, 1)); // 6: R0 += R1
            program.Add(Encode(6, 1, 3)); // 7: R1 += R3
            program.Add(Encode(17, 1, 2)); // 8: CMP R1, R2
            program.Add(Encode(19, 5, 0)); // 9: JE R5
            program.Add(Encode(18, 4, 0)); // 10: JMP R4
            program.Add(Encode(0, 0, 0));  // 11: HALT
            
            proc.LoadProgram(program);
            proc.Run();
            // Sum of 1..5 = 15
            Assert.AreEqual(15, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_Call_Ret()
        {
            var proc = CreateProcessor();
            // Simple function that adds 1 to the first parameter (A in main -> E in func)
            // main:
            //   LI A, 10
            //   CALL func
            //   HALT
            // func:
            //   LI B, 1
            //   ADD E, B
            //   RET
            
            List<long> program = new List<long>();
            // main:
            program.Add(Encode(4, 0, 10)); // PC 0: A = 10
            program.Add(Encode(4, 1, 4));  // PC 1: R1 = 4 (addr of func)
            program.Add(Encode(24, 1, 0)); // PC 2: CALL R1
            program.Add(Encode(0, 0, 0));  // PC 3: HALT
            
            // func:
            program.Add(Encode(4, 1, 1));  // PC 4: B = 1 (Logical B = index 1)
            program.Add(Encode(6, 4, 1));  // PC 5: E += B (Logical E = index 4, which is main's A)
            program.Add(Encode(25, 0, 0)); // PC 6: RET
            
            proc.LoadProgram(program);
            proc.Run();
            // After RET, WP is restored to 0, so we check main's A (Physical 0)
            Assert.AreEqual(11, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_Push_Pop()
        {
            var proc = CreateProcessor();
            // LI A, 10
            // PUSH A
            // LI A, 20
            // POP A
            // HALT
            var program = new List<long>
            {
                Encode(4, 0, 10), // A=10
                Encode(26, 0, 0), // PUSH A
                Encode(4, 0, 20), // A=20
                Encode(27, 0, 0), // POP A
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(10, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_LIMM()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(5, 0, 0), // LIMM A, [next]
                12345,           // Immediate value
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(12345, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_LOAD_STORE()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 100), // LI A, 100
                Encode(4, 1, 50),  // LI B, 50
                Encode(2, 0, 1),   // STORE A, B (mem[50] = 100)
                Encode(4, 0, 0),   // LI A, 0 (clear A)
                Encode(1, 0, 1),   // LOAD A, B (A = mem[50])
                Encode(0, 0, 0)    // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(100, proc.GetState().Registers[0]);
        }

        [TestMethod]
        public void Test_IO_Basic()
        {
            var proc = CreateProcessor();
            var program = new List<long>
            {
                Encode(4, 0, 5),    // LI A, 5 (port)
                Encode(4, 1, 42),   // LI B, 42 (value)
                Encode(42, 1, 0),   // OUT B, A (port 5 = 42)
                Encode(0, 0, 0)     // HALT
            };
            
            // Setup a mock device
            var mockDevice = new MockDevice(42); 
            proc.SetOutputDevice(5, mockDevice);

            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(42, mockDevice.LastWrittenValue);
        }

        [TestMethod]
        public void Test_T3_54_Int128()
        {
            // Test with T3-54 and Int128
            var proc = new T3InOrderProcessor<Int128>(T3Config.T3_54);
            
            List<Int128> program = new List<Int128>();
            
            // Use LI instead of LIMM to isolate the problem
            program.Add(EncodeInt128(4, 0, 100)); // LI A, 100
            program.Add(EncodeInt128(4, 1, 2));   // LI B, 2
            program.Add(EncodeInt128(8, 0, 1));   // MUL A, B -> 200
            program.Add(EncodeInt128(0, 0, 0));   // HALT
            
            proc.LoadProgram(program);
            proc.Run();
            
            Assert.AreEqual((Int128)200, proc.GetState().Registers[0]);
        }

        private Int128 EncodeInt128(int opcode, int op1, Int128 op2, int pred = 0)
        {
            long fullOpcode = pred * 45 + opcode;
            string sOp = ToBalancedTernary(fullOpcode, 6);
            string sOp1 = ToBalancedTernary(op1, 9);
            string sOp2 = TritTypes.BalancedTernary.ToTernaryString(op2, 9);
            // For T3-54, the word is 54 trits. We pad the 27-trit instruction to 54.
            string instruction = sOp + sOp1 + sOp2 + "000";
            string word = instruction.PadLeft(54, '0');
            return TritTypes.BalancedTernary.ParseToInt128(word);
        }

        private class MockDevice : IDevice<long>
        {
            public long LastWrittenValue { get; private set; }
            private readonly long _initialValue;
            public MockDevice(long initialValue) => _initialValue = initialValue;
            public long Read() => _initialValue;
            public void Write(long value) => LastWrittenValue = value;
            public bool DataReady => true;
        }
    }
}
