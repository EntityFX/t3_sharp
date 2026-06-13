using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class FpuInstructionTests
    {
        /// <summary>
        /// Encode an FPU R-type instruction: [Opcode (6)] [Op1 (3)] [Op2 (3)] [Op3 (3)] [Func (3)]
        /// </summary>
        private Word18 EncodeFpu(int opcode, int op1, int op2, int op3, int func = 0)
        {
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(op2, 3);
            string sOp3 = ToBalancedTernary(op3, 3);
            string sFunc = ToBalancedTernary(func, 3);
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sOp3 + sFunc));
        }

        /// <summary>
        /// Encode an FPU I-type instruction (FLW/FSW): [Opcode (6)] [Op1 (3)] [Op2 (3)] [Imm6 (6)]
        /// </summary>
        private Word18 EncodeFpuI(int opcode, int op1, int op2, long imm)
        {
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(op2, 3);
            string sImm = ToBalancedTernary(imm, 6);
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sImm));
        }

        /// <summary>
        /// Encode LI (Load Immediate) for integer register: LI op1, imm6
        /// </summary>
        private Word18 EncodeLi(int reg, long value)
        {
            int field = 68; // LI_I = 68
            string sOp = ToBalancedTernary(field, 6);
            string sOp1 = ToBalancedTernary(reg, 3);
            string sOp2 = "000";
            string sImm = ToBalancedTernary(value, 6);
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sImm));
        }

        /// <summary>
        /// Encode HALT (opcode 0)
        /// </summary>
        private Word18 EncodeHalt()
        {
            return Word18.FromLong(0);
        }

        private string ToBalancedTernary(long value, int digits)
        {
            string s = BalancedTernary.ToTernaryString(value);
            if (s.Length > digits) s = s.Substring(s.Length - digits);
            if (s.Length < digits) s = s.PadLeft(digits, '0');
            return s;
        }

        private T3InOrderProcessor<Word18> CreateProcessor()
        {
            return new T3InOrderProcessor<Word18>(T3Config.T3_18);
        }

        /// <summary>
        /// Helper: compute the expected result of an FPU operation given tfloat inputs.
        /// Uses the same double-based chain as T3Fpu.
        /// </summary>
        private static double ExpectedFpuResult(double a, double b, string op)
        {
            var tfA = T3Float.FromDouble(a);
            var tfB = T3Float.FromDouble(b);
            T3Float result = op switch
            {
                "ADD" => T3Fpu.Add(tfA, tfB),
                "SUB" => T3Fpu.Sub(tfA, tfB),
                "MUL" => T3Fpu.Mul(tfA, tfB),
                "DIV" => T3Fpu.Div(tfA, tfB),
                _ => throw new System.Exception("Unknown op")
            };
            return result.ToDouble();
        }

        // === FADD ===

        [TestMethod]
        [Timeout(30000)]
        public void FADD_AddsTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.0);  // FX = 3 (round-trips exactly)
            proc.FRegisters[2] = T3Float.FromDouble(9.0);  // FY = 9 (round-trips exactly)
            var program = new List<Word18>
            {
                EncodeFpu(92, 0, 1, 2), // FADD FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = ExpectedFpuResult(3.0, 9.0, "ADD");
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FSUB ===

        [TestMethod]
        [Timeout(30000)]
        public void FSUB_SubtractsTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(9.0);  // FY
            var program = new List<Word18>
            {
                EncodeFpu(93, 0, 1, 2), // FSUB FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = ExpectedFpuResult(27.0, 9.0, "SUB");
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FMUL ===

        [TestMethod]
        [Timeout(30000)]
        public void FMUL_MultipliesTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(9.0); // FY
            var program = new List<Word18>
            {
                EncodeFpu(94, 0, 1, 2), // FMUL FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = ExpectedFpuResult(3.0, 9.0, "MUL");
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FDIV ===

        [TestMethod]
        [Timeout(30000)]
        public void FDIV_DividesTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(9.0);  // FY
            var program = new List<Word18>
            {
                EncodeFpu(95, 0, 1, 2), // FDIV FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = ExpectedFpuResult(27.0, 9.0, "DIV");
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FSQRT ===

        [TestMethod]
        [Timeout(30000)]
        public void FSQRT_ComputesSquareRoot()
        {
            var proc = CreateProcessor();
            // 81 = 3^4, sqrt(81) = 9 = 3^2 (both exact in tfloat)
            proc.FRegisters[1] = T3Float.FromDouble(81.0);
            var program = new List<Word18>
            {
                EncodeFpu(96, 0, 1, 0), // FSQRT FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.Sqrt(T3Float.FromDouble(81.0)).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FABS ===

        [TestMethod]
        [Timeout(30000)]
        public void FABS_AbsoluteValueOfNegative()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(-27.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(97, 0, 1, 0), // FABS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.Abs(T3Float.FromDouble(-27.0)).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FABS_AbsoluteValueOfPositive()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(97, 0, 1, 0), // FABS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.Abs(T3Float.FromDouble(27.0)).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FNEG ===

        [TestMethod]
        [Timeout(30000)]
        public void FNEG_NegatesPositive()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(98, 0, 1, 0), // FNEG FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.Neg(T3Float.FromDouble(3.0)).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FNEG_NegatesNegative()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(-3.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(98, 0, 1, 0), // FNEG FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.Neg(T3Float.FromDouble(-3.0)).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FZERO ===

        [TestMethod]
        [Timeout(30000)]
        public void FZERO_SetsToZero()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(27.0); // FW (should be overwritten)
            var program = new List<Word18>
            {
                EncodeFpu(108, 0, 0, 0), // FZERO FW
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(0.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FCMP ===

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_Greater_SetsPositiveCond()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(27.0); // FW = 27
            proc.FRegisters[1] = T3Float.FromDouble(3.0);  // FX = 3
            var program = new List<Word18>
            {
                EncodeFpu(99, 0, 1, 0), // FCMP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(1, proc.Cond);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_Less_SetsNegCond()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(3.0);  // FW = 3
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX = 27
            var program = new List<Word18>
            {
                EncodeFpu(99, 0, 1, 0), // FCMP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(-1, proc.Cond);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_Equal_SetsZeroCond()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(9.0); // FW = 9
            proc.FRegisters[1] = T3Float.FromDouble(9.0); // FX = 9
            var program = new List<Word18>
            {
                EncodeFpu(99, 0, 1, 0), // FCMP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(0, proc.Cond);
        }

        // === FTOI ===

        [TestMethod]
        [Timeout(30000)]
        public void FTOI_ConvertsFloatToInt()
        {
            var proc = CreateProcessor();
            // 9.0 -> ToDouble = 9, truncate = 9
            proc.FRegisters[1] = T3Float.FromDouble(9.0);
            var program = new List<Word18>
            {
                EncodeFpu(100, 0, 1, 0, 0), // FTOI RW, FX (func=0: 18-trit)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            long expected = T3Fpu.ToInt(T3Float.FromDouble(9.0), 0);
            Assert.AreEqual(expected, proc.Registers[0].ToLong());
        }

        // === ITOF ===

        [TestMethod]
        [Timeout(30000)]
        public void ITOF_ConvertsIntToFloat()
        {
            var proc = CreateProcessor();
            proc.Registers[0] = Word18.FromLong(100); // RW = 100
            var program = new List<Word18>
            {
                EncodeFpu(101, 1, 0, 0), // ITOF FX, RW
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.FromInt(100).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[1].ToDouble(), 0.01);
        }

        // === FLW / FSW ===

        [TestMethod]
        [Timeout(30000)]
        public void FSW_FLW_StoreLoadRoundTrip()
        {
            var proc = CreateProcessor();
            long addr = 100;

            // Use a value that survives tfloat: 9.0
            proc.FRegisters[0] = T3Float.FromDouble(9.0); // FW = 9.0

            var program = new List<Word18>
            {
                EncodeLi(0, addr),                // LI RW, addr
                EncodeFpuI(104, 0, 0, 0),          // FSW FW, RW+0  (store to mem[addr])
                EncodeFpuI(103, 2, 0, 0),          // FLW FY, RW+0  (load from mem[addr])
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(9.0, proc.FRegisters[2].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FSW_StoresFloatToMemory()
        {
            var proc = CreateProcessor();
            long addr = 200;

            proc.FRegisters[0] = T3Float.FromDouble(3.0); // FW = 3.0
            var program = new List<Word18>
            {
                EncodeLi(1, addr),                // LI RX, addr
                EncodeFpuI(104, 0, 1, 0),          // FSW FW, RX+0  (mem[addr] = FW)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            // Verify memory holds the float word
            Word18 rawWord = proc.ReadWord(addr);
            T3Float loaded = T3Float.FromWord18(rawWord);
            Assert.AreEqual(3.0, loaded.ToDouble(), 0.01);
        }

        // === FMOV ===

        [TestMethod]
        [Timeout(30000)]
        public void FMOV_FtoF_MovesBetweenFRegisters()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(105, 0, 1, 0, 0), // FMOV FW, FX (func=0: F→F)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(27.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FMOV_RtoF_MovesFromRegToFreg()
        {
            var proc = CreateProcessor();
            proc.Registers[1] = Word18.FromLong(50); // RX = 50
            var program = new List<Word18>
            {
                EncodeFpu(105, 0, 1, 0, 2), // FMOV FW, RX (func=2: R→F)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = T3Fpu.FromInt(50).ToDouble();
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FSWAP ===

        [TestMethod]
        [Timeout(30000)]
        public void FSWAP_SwapsTwoFRegisters()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(81.0);  // FW = 81
            proc.FRegisters[1] = T3Float.FromDouble(3.0);   // FX = 3
            var program = new List<Word18>
            {
                EncodeFpu(107, 0, 1, 0), // FSWAP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(3.0, proc.FRegisters[0].ToDouble(), 0.01);
            Assert.AreEqual(81.0, proc.FRegisters[1].ToDouble(), 0.01);
        }

        // === FCLASS ===

        [TestMethod]
        [Timeout(30000)]
        public void FCLASS_ClassifiesNormal()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.0); // FX = normal number
            var program = new List<Word18>
            {
                EncodeFpu(106, 0, 1, 0), // FCLASS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            // Classify returns 4 for normal; stored via FromInt(4) which compresses to ~3.0 in tfloat
            // Verify FW is non-zero (not the zero classification value of 0)
            Assert.IsTrue(proc.FRegisters[0].ToDouble() > 0.0,
                "FCLASS for normal number should return a positive value (classification code > 0)");
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCLASS_ClassifiesZero()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(0.0); // FX = zero
            var program = new List<Word18>
            {
                EncodeFpu(106, 0, 1, 0), // FCLASS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            int cls = T3Fpu.Classify(T3Float.FromDouble(0.0));
            Assert.AreEqual((double)cls, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === Additional: FPU opcodes used with cond branch ===

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_WithBranch_JumpIfGreater()
        {
            var proc = CreateProcessor();
            // FW=27, FX=3 → FCMP sets Cond=1 → JG should jump
            proc.FRegisters[0] = T3Float.FromDouble(27.0);
            proc.FRegisters[1] = T3Float.FromDouble(3.0);

            // Set up branch target in RX: if FW > FX (true), jump to PC=4, else fall through to HALT at PC=3
            // Program:
            // 0: FCMP FW, FX
            // 1: JG R1  (R1 = 4 = target address)
            // 2: LI RW, -1  (fall-through — should NOT execute)
            // 3: HALT     (fall-through halt)
            // 4: LI RW, 1  (target)
            // 5: HALT
            proc.Registers[1] = Word18.FromLong(4); // R1 = target address

            var program = new List<Word18>
            {
                EncodeFpu(99, 0, 1, 0),  // 0: FCMP FW, FX
                EncodeJg(1),              // 1: JG R1
                EncodeLi(0, -1),          // 2: LI RW, -1 (should be skipped)
                EncodeHalt(),             // 3: HALT (fall-through)
                EncodeLi(0, 1),           // 4: LI RW, 1 (target)
                EncodeHalt()              // 5: HALT
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(1, proc.Registers[0].ToLong());
        }

        private Word18 EncodeJg(int reg)
        {
            // JG = 22, R-type: [opcode=22 (6)] [reg (3)] [0 (3)] [0 (3)] [0 (3)]
            string sOp = ToBalancedTernary(22, 6);
            string sOp1 = ToBalancedTernary(reg, 3);
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + "000" + "000" + "000"));
        }
    }
}