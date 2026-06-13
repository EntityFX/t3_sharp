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
        private Word18 Encode(int opcode, int op1, long op2, int pred = 0)
        {
            // New ISA: Opcode+Pred (6), Op1 (3), Op2 (3), Op3/Imm6 (3/6), Reserve (3)
            int baseOp = opcode;
            int fieldOp = pred * 28 + baseOp;
            string sOp = ToBalancedTernary(fieldOp, 6);
            string sOp1 = ToBalancedTernary(op1, 3);

            if (opcode == 4) // LI: I-type
            {
                // LI Op1, Imm6
                // Format: [Opcode+Pred] [Op1] [Op2(res)] [Imm6]
                string sOp2 = "000";
                string sImm = ToBalancedTernary(op2, 6);
                return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sImm));
            }
            else if (opcode == 11) // NEG: R-type
            {
                // NEG Op1, Op2
                string sOp2 = ToBalancedTernary((int)op2, 3);
                string sOp3 = "000";
                string sRes = "000";
                return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sOp3 + sRes));
            }
            else if (opcode >= 6 && opcode <= 16) // Arithmetic/Logical R-type
            {
                // Assuming Op1 = Op1 <op> Op2 for these tests
                string sOp2 = ToBalancedTernary(op1, 3);
                string sOp3 = ToBalancedTernary((int)op2, 3);
                string sRes = "000";
                return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sOp3 + sRes));
            }
            else
            {
                // General R-type (JMP, CALL, etc.)
                string sOp2 = ToBalancedTernary((int)op2, 3);
                string sOp3 = "000";
                string sRes = "000";
                return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sOp3 + sRes));
            }
        }

        private string ToBalancedTernary(long value, int digits)
        {
            string s = TritTypes.BalancedTernary.ToTernaryString(value);
            if (s.Length > digits) s = s.Substring(s.Length - digits);
            if (s.Length < digits) s = s.PadLeft(digits, '0');
            return s;
        }

        private T3InOrderProcessor<Word18> CreateProcessor()
        {
            return new T3InOrderProcessor<Word18>(T3Config.T3_18);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_ADD()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 10), // LI A, 10
                Encode(4, 1, 20), // LI B, 20
                Encode(6, 0, 1),  // ADD A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(30, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_SUB()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 50), // LI A, 50
                Encode(4, 1, 20), // LI B, 20
                Encode(7, 0, 1),  // SUB A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(30, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_MUL()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 5),  // LI A, 5
                Encode(4, 1, 6),  // LI B, 6
                Encode(8, 0, 1),  // MUL A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(30, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_DIV()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 30), // LI A, 30
                Encode(4, 1, 4),  // LI B, 4
                Encode(9, 0, 1),  // DIV A, B -> floor(30/4) = 7
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(7, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_MOD()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 30), // LI A, 30
                Encode(4, 1, 4),  // LI B, 4
                Encode(10, 0, 1), // MOD A, B -> 30 % 4 = 2
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(2, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_NEG()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 15), // LI A, 15
                Encode(11, 0, 0), // NEG A
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(-15, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_AND()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 1),  // LI A, 1
                Encode(4, 1, 0),  // LI B, 0
                Encode(12, 0, 1), // AND A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(0, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_OR()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 1),  // LI A, 1
                Encode(4, 1, 0),  // LI B, 0
                Encode(13, 0, 1), // OR A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(1, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_XOR()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 1),  // LI A, 1
                Encode(4, 1, 1),  // LI B, 1
                Encode(14, 0, 1), // XOR A, B -> 1+1=2 -> -1 (mod 3)
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(-1, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_SHL()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 2),  // LI A, 2
                Encode(4, 1, 2),  // LI B, 2 (shift by 2: * 3^2 = * 9)
                Encode(15, 0, 1), // SHL A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(18, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_SHR()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 18), // LI A, 18
                Encode(4, 1, 2),  // LI B, 2 (shift by 2: / 9)
                Encode(16, 0, 1), // SHR A, B
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(2, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_CMP_And_Branches()
        {
            var proc = CreateProcessor();
            List<Word18> program = new List<Word18>();
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
        [Timeout(30000)]
        public void Test_Loop_Sum()
        {
            var proc = CreateProcessor();
            List<Word18> program = new List<Word18>();
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
            Assert.AreEqual(15, proc.GetState().Registers[0]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_Call_Ret()
        {
            var proc = CreateProcessor();
            List<Word18> program = new List<Word18>();
                program.Add(Encode(4, 0, 10)); // PC 0: A = 10
                program.Add(Encode(4, 1, 4));  // PC 1: R1 = 4 (addr of func)
                program.Add(Encode(24, 1, 0)); // PC 2: CALL R1
                program.Add(Encode(0, 0, 0));  // PC 3: HALT
                program.Add(Encode(4, 1, 1));  // PC 4: B = 1
                program.Add(Encode(6, 0, 1));  // PC 5: A += B
                program.Add(Encode(25, 0, 0)); // PC 6: RET
            
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(11, proc.GetState().Registers[0]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_Push_Pop()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 10), // A=10
                Encode(26, 0, 0), // PUSH A
                Encode(4, 0, 20), // A=20
                Encode(27, 0, 0), // POP A
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(10, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_LIMM()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(5, 0, 0), // LIMM A, [next]
                Word18.FromLong(12345),           // Immediate value
                Encode(0, 0, 0)   // HALT
            };
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(12345, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_LOAD_STORE()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
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
            Assert.AreEqual(100, proc.GetState().Registers[0].ToLong());
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_IO_Basic()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(4, 0, 5),    // LI A, 5 (port)
                Encode(4, 1, 42),   // LI B, 42 (value)
                Encode(42, 1, 0),   // OUT B, A (port 5 = 42)
                Encode(0, 0, 0)     // HALT
            };
            var mockDevice = new MockDevice(42); 
            proc.SetOutputDevice(5, mockDevice);
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(42, mockDevice.LastWrittenValue);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_INI_OUTI()
        {
            var proc = CreateProcessor();
            var program = new List<Word18>
            {
                Encode(43, 0, 7),   // INI A, 7 (port 7)
                Encode(44, 0, 8),   // OUTI A, 8 (port 8)
                Encode(0, 0, 0)     // HALT
            };
            var inputDev = new MockDevice(99);
            var outputDev = new MockDevice(0);
            proc.SetInputDevice(7, inputDev);
            proc.SetOutputDevice(8, outputDev);
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual(99, outputDev.LastWrittenValue);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_JNE_JM()
        {
            var proc = CreateProcessor();
            List<Word18> progJne = new List<Word18>();
            progJne.Add(Encode(4, 2, 6));  // R2 = 6 (target)
            progJne.Add(Encode(4, 0, 10)); // A = 10
            progJne.Add(Encode(4, 1, 20)); // B = 20
            progJne.Add(Encode(17, 0, 1)); // CMP A, B -> Cond = -1
            progJne.Add(Encode(20, 2, 0)); // JNE R2
            progJne.Add(Encode(4, 0, -1)); // A = -1 (should be skipped)
            progJne.Add(Encode(4, 0, 1));  // A = 1 (target)
            progJne.Add(Encode(0, 0, 0));  // HALT
            proc.LoadProgram(progJne);
            proc.Run();
            Assert.AreEqual(1, proc.GetState().Registers[0]);

            proc.Reset();
            List<Word18> progJm = new List<Word18>();
            progJm.Add(Encode(4, 2, 6));   // R2 = 6 (target)
            progJm.Add(Encode(4, 0, 10));  // A = 10
            progJm.Add(Encode(4, 1, 10));  // B = 10
            progJm.Add(Encode(17, 0, 1));  // CMP A, B -> Cond = 0
            progJm.Add(Encode(23, 2, 0));  // JM R2
            progJm.Add(Encode(4, 0, -1));  // A = -1 (should be skipped)
            progJm.Add(Encode(4, 0, 1));   // A = 1 (target)
            progJm.Add(Encode(0, 0, 0));   // HALT
            proc.LoadProgram(progJm);
            proc.Run();
            Assert.AreEqual(1, proc.GetState().Registers[0]);
        }

        [TestMethod]
        [Timeout(30000)]
        public void Test_T3_54_Int128()
        {
            var proc = new T3InOrderProcessor<Word54>(T3Config.T3_54);
            List<Word54> program = new List<Word54>();
            program.Add(EncodeInt128(4, 0, 100)); // LI A, 100
            program.Add(EncodeInt128(4, 1, 2));   // LI B, 2
            program.Add(EncodeInt128(8, 0, 1));   // MUL A, B -> 200
            program.Add(EncodeInt128(0, 0, 0));   // HALT
            proc.LoadProgram(program);
            proc.Run();
            Assert.AreEqual((Int128)200, proc.GetState().Registers[0].ToInt128());
        }

        private Word54 EncodeInt128(int opcode, int op1, Int128 op2, int pred = 0)
        {
            // Use the same encoding as Word18, then pad to 54 trits
            // Instruction is encoded in the first 18 trits for T3-54
            Word18 w18 = Encode(opcode, op1, (long)op2, pred);
            string s18 = w18.ToTritString();
            string s54 = s18.PadLeft(54, '0');
            return Word54.FromInt128(TritTypes.BalancedTernary.ParseToInt128(s54));
        }

        private class MockDevice : IDevice<Word18>
        {
            public long LastWrittenValue { get; private set; }
            private readonly long _initialValue;
            public MockDevice(long initialValue) => _initialValue = initialValue;
            public Word18 Read() => Word18.FromLong(_initialValue);
            public void Write(Word18 value) => LastWrittenValue = value.ToLong();
            public bool DataReady => true;
        }
    }
}