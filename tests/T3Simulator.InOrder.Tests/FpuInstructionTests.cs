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
        /// LI is always I-type: base opcode 68 (= 4 + 64)
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
        /// Encode STORE (R-type base opcode 2): STORE op1, op2 => mem[op2] = op1
        /// </summary>
        private Word18 EncodeStore(int srcReg, int addrReg)
        {
            // STORE = 2, R-type: [opcode=2 (6)] [srcReg (3)] [addrReg (3)] [op3=0 (3)] [res=0 (3)]
            string sOp = ToBalancedTernary(2, 6);
            string sOp1 = ToBalancedTernary(srcReg, 3);
            string sOp2 = ToBalancedTernary(addrReg, 3);
            string sOp3 = "000";
            string sRes = "000";
            return Word18.FromLong(BalancedTernary.ParseToLong(sOp + sOp1 + sOp2 + sOp3 + sRes));
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

        // --- FADD ---

        [TestMethod]
        [Timeout(30000)]
        public void FADD_AddsTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(10.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(20.0); // FY
            var program = new List<Word18>
            {
                EncodeFpu(92, 0, 1, 2), // FADD FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(30.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FSUB ---

        [TestMethod]
        [Timeout(30000)]
        public void FSUB_SubtractsTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(100.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(35.0);  // FY
            var program = new List<Word18>
            {
                EncodeFpu(93, 0, 1, 2), // FSUB FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(65.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FMUL ---

        [TestMethod]
        [Timeout(30000)]
        public void FMUL_MultipliesTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(7.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(6.0); // FY
            var program = new List<Word18>
            {
                EncodeFpu(94, 0, 1, 2), // FMUL FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(42.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FDIV ---

        [TestMethod]
        [Timeout(30000)]
        public void FDIV_DividesTwoFloats()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(100.0); // FX
            proc.FRegisters[2] = T3Float.FromDouble(4.0);   // FY
            var program = new List<Word18>
            {
                EncodeFpu(95, 0, 1, 2), // FDIV FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(25.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FSQRT ---

        [TestMethod]
        [Timeout(30000)]
        public void FSQRT_ComputesSquareRoot()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(16.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(96, 0, 1, 0), // FSQRT FW, FX (op3 ignored)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(4.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FABS ---

        [TestMethod]
        [Timeout(30000)]
        public void FABS_AbsoluteValue()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(-42.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(97, 0, 1, 0), // FABS FW, FX (op3 ignored)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(42.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FNEG ---

        [TestMethod]
        [Timeout(30000)]
        public void FNEG_Negates()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(5.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(98, 0, 1, 0), // FNEG FW, FX (op3 ignored)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(-5.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FZERO ---

        [TestMethod]
        [Timeout(30000)]
        public void FZERO_SetsToZero()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(99.0); // FW (overwritten)
            var program = new List<Word18>
            {
                EncodeFpu(108, 0, 0, 0), // FZERO FW
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(0.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FCMP ---

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_ComparesAndSetsCond()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(10.0); // FW
            proc.FRegisters[1] = T3Float.FromDouble(5.0);  // FX
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
        public void FCMP_LessThan_SetsNegCond()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(3.0); // FW
            proc.FRegisters[1] = T3Float.FromDouble(7.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(99, 0, 1, 0), // FCMP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(-1, proc.Cond);
        }

        // --- FTOI ---

        [TestMethod]
        [Timeout(30000)]
        public void FTOI_ConvertsFloatToInt()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(42.7); // FX
            var program = new List<Word18>
            {
                EncodeFpu(100, 0, 1, 0, 0), // FTOI RW, FX (func=0: 18-trit)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(42, proc.Registers[0].ToLong());
        }

        // --- ITOF ---

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

            Assert.AreEqual(100.0, proc.FRegisters[1].ToDouble(), 0.01);
        }

        // --- FLW / FSW ---

        [TestMethod]
        [Timeout(30000)]
        public void FSW_FLW_StoreLoadRoundTrip()
        {
            var proc = CreateProcessor();
            long addr = 100;

            // Step 1: LI RW, 100  (RW = address)
            // Step 2: STORE float via FSW: FSW FW, RW+0
            // Step 3: FLW FY, RW+0  (load float back into FY)
            // Step 4: HALT
            proc.FRegisters[0] = T3Float.FromDouble(3.14); // FW = 3.14

            var program = new List<Word18>
            {
                EncodeLi(0, addr),                // LI RW, addr
                EncodeFpuI(104, 0, 0, 0),          // FSW FW, RW+0  (store to mem[addr])
                EncodeFpuI(103, 2, 0, 0),          // FLW FY, RW+0  (load from mem[addr])
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(3.14, proc.FRegisters[2].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FSW_StoresFloatToMemory()
        {
            var proc = CreateProcessor();
            long addr = 200;

            proc.FRegisters[0] = T3Float.FromDouble(2.718); // FW = 2.718
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
            Assert.AreEqual(2.718, loaded.ToDouble(), 0.01);
        }

        // --- FMOV ---

        [TestMethod]
        [Timeout(30000)]
        public void FMOV_FtoF_MovesBetweenFRegisters()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(99.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(105, 0, 1, 0, 0), // FMOV FW, FX (func=0: F→F)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(99.0, proc.FRegisters[0].ToDouble(), 0.01);
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

            Assert.AreEqual(50.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // --- FSWAP ---

        [TestMethod]
        [Timeout(30000)]
        public void FSWAP_SwapsTwoFRegisters()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(100.0);  // FW
            proc.FRegisters[1] = T3Float.FromDouble(-100.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(107, 0, 1, 0), // FSWAP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(-100.0, proc.FRegisters[0].ToDouble(), 0.01);
            Assert.AreEqual(100.0, proc.FRegisters[1].ToDouble(), 0.01);
        }

        // --- FCLASS ---

        [TestMethod]
        [Timeout(30000)]
        public void FCLASS_ClassifiesNormal()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.14); // FX
            var program = new List<Word18>
            {
                EncodeFpu(106, 0, 1, 0), // FCLASS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(4.0, proc.FRegisters[0].ToDouble(), 0.01); // normal = 4
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCLASS_ClassifiesZero()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(0.0); // FX
            var program = new List<Word18>
            {
                EncodeFpu(106, 0, 1, 0), // FCLASS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(0.0, proc.FRegisters[0].ToDouble(), 0.01); // zero = 0
        }
    }
}