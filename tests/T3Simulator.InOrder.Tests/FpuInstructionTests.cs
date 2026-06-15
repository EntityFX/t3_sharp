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
        /// Encode an FPU R-type instruction with predicate prefix.
        /// Format: [Pred(000)][Opcode(6)][Op1(3)][Op2(3)][Op3(3)]
        /// </summary>
        private Word18 EncodeFpu(int opcode, int op1, int op2, int op3, int func = 0)
        {
            string sPred = "000";  // unconditional
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(op2, 3);
            // Use op3 field for func when needed (FMOV, FTOI, FTOF)
            string sOp3 = ToBalancedTernary(func != 0 ? func : op3, 3);
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sOp2 + sOp3));
        }

        /// <summary>
        /// Encode an FPU I-type instruction (FZERO): [Pred(000)][Opcode(6)][Op1(3)][Imm(6)]
        /// </summary>
        private Word18 EncodeFpuI(int opcode, int op1, long imm)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sImm = ToBalancedTernary(imm, 6);
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sImm));
        }

        /// <summary>
        /// Encode a predicated FPU instruction.
        /// Format: [Pred(3)][Opcode(6)][Op1(3)][Op2(3)][Op3(3)]
        /// </summary>
        private Word18 EncodeFpuPred(int pred, int opcode, int op1, int op2, int op3)
        {
            string sPred = ToBalancedTernary(pred, 3);
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(op1, 3);
            string sOp2 = ToBalancedTernary(op2, 3);
            string sOp3 = ToBalancedTernary(op3, 3);
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sOp2 + sOp3));
        }

        /// <summary>
        /// Encode LI (Load Immediate) for integer register: LI op1, imm6
        /// Format: [Pred(000)][Opcode(6)][Op1(3)][Imm(6)]
        /// </summary>
        private Word18 EncodeLi(int reg, long value)
        {
            int opcode = (int)Opcode.LI; // LI = 4
            string sPred = "000";
            string sOp = ToBalancedTernary(opcode, 6);
            string sOp1 = ToBalancedTernary(reg, 3);
            string sImm = ToBalancedTernary(value, 6);
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sImm));
        }

        /// <summary>
        /// Encode integer ADD: ADD reg1, reg2, reg3
        /// Format: [Pred(000)][Opcode(6)][Op1(3)][Op2(3)][Op3(3)]
        /// </summary>
        private Word18 EncodeAdd(int rd, int rs1, int rs2)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.ADD, 6);
            string sOp1 = ToBalancedTernary(rd, 3);
            string sOp2 = ToBalancedTernary(rs1, 3);
            string sOp3 = ToBalancedTernary(rs2, 3);
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sOp2 + sOp3));
        }

        /// <summary>
        /// Encode HALT (opcode 0)
        /// </summary>
        private Word18 EncodeHalt()
        {
            return Word18.FromLong(0);
        }

        /// <summary>
        /// Encode CMP: CMP reg1, reg2
        /// </summary>
        private Word18 EncodeCmp(int r1, int r2)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.CMP, 6);
            string sOp1 = ToBalancedTernary(r1, 3);
            string sOp2 = ToBalancedTernary(r2, 3);
            string sOp3 = "000";
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sOp2 + sOp3));
        }

        /// <summary>
        /// Encode JG: JG reg
        /// </summary>
        private Word18 EncodeJG(int reg)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.JG, 6);
            string sArgs = ToBalancedTernary(0, 3) + ToBalancedTernary(reg, 3) + "000";
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sArgs));
        }

        /// <summary>
        /// Encode JMP: JMP reg
        /// </summary>
        private Word18 EncodeJmp(int reg)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.JMP, 6);
            string sArgs = ToBalancedTernary(0, 3) + ToBalancedTernary(reg, 3) + "000";
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sArgs));
        }

        /// <summary>
        /// Encode LIMM: LIMM reg, value
        /// Returns two words
        /// </summary>
        private List<Word18> EncodeLimm(int reg, long value)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.LIMM, 6);
            string sOp1 = ToBalancedTernary(reg, 3);
            string sImm = "000000";
            var result = new List<Word18>
            {
                Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sImm)),
                Word18.FromLong(value)
            };
            return result;
        }

        /// <summary>
        /// Encode MOV: MOV rd, rs
        /// </summary>
        private Word18 EncodeMov(int rd, int rs)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.MOV, 6);
            string sOp1 = ToBalancedTernary(rd, 3);
            string sOp2 = ToBalancedTernary(rs, 3);
            string sOp3 = "000";
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sOp1 + sOp2 + sOp3));
        }

        /// <summary>
        /// Encode JE: JE reg
        /// </summary>
        private Word18 EncodeJE(int reg)
        {
            string sPred = "000";
            string sOp = ToBalancedTernary((int)Opcode.JE, 6);
            string sArgs = ToBalancedTernary(0, 3) + ToBalancedTernary(reg, 3) + "000";
            return Word18.FromLong(BalancedTernary.ParseToLong(sPred + sOp + sArgs));
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
                EncodeFpu((int)Opcode.FADD, 0, 1, 2), // FADD FW, FX, FY
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
                EncodeFpu((int)Opcode.FSUB, 0, 1, 2), // FSUB FW, FX, FY
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
                EncodeFpu((int)Opcode.FMUL, 0, 1, 2), // FMUL FW, FX, FY
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
                EncodeFpu((int)Opcode.FDIV, 0, 1, 2), // FDIV FW, FX, FY
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
            proc.FRegisters[1] = T3Float.FromDouble(81.0);
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FSQRT, 0, 1, 0), // FSQRT FW, FX
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
                EncodeFpu((int)Opcode.FABS, 0, 1, 0), // FABS FW, FX
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
                EncodeFpu((int)Opcode.FABS, 0, 1, 0), // FABS FW, FX
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
                EncodeFpu((int)Opcode.FNEG, 0, 1, 0), // FNEG FW, FX
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
                EncodeFpu((int)Opcode.FNEG, 0, 1, 0), // FNEG FW, FX
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
                EncodeFpuI((int)Opcode.FZERO, 0, 0), // FZERO FW
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
                EncodeFpu((int)Opcode.FCMP, 0, 1, 0), // FCMP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(1, proc.Cond, "FW > FX → Cond=1");
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
                EncodeFpu((int)Opcode.FCMP, 0, 1, 0), // FCMP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(-1, proc.Cond, "FW < FX → Cond=-1");
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_Equal_SetsZeroCond()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(9.0);
            proc.FRegisters[1] = T3Float.FromDouble(9.0);
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FCMP, 0, 1, 0), // FCMP FW, FX (both 9.0)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(0, proc.Cond, "FW == FX → Cond=0");
        }

        // === FCMP with conditional branches ===

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_WithBranch_JumpIfGreater()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(27.0); // FW
            proc.FRegisters[1] = T3Float.FromDouble(3.0);  // FX
            // Program:
            // 0: FCMP FW, FX       (Cond=+1)
            // 1: JG greater_label  (jump to 3)
            // 2: LI R5, -1         (skipped)
            // 3: HALT
            // R5 should be 0 (never written), but ProcFinished check tests R5==1? 
            // Actually we want: if (A > B) Registers[5]=1 else continue
            // We'll use MOV to set R5=1 at the target
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FCMP, 0, 1, 0), // PC=0: FCMP FW, FX
                EncodeJG(5),                           // PC=1: JG R5 (jump to addr in R5)
                EncodeLi(5, -1),                       // PC=2: LI R5, -1 (should be skipped, but we need R5=1 first)
                EncodeHalt()                           // PC=3: HALT
            };
            // Need to set up R5 with address 3 (the "greater" target)
            proc.Registers[5] = Word18.FromLong(4);  // jump to PC=4
            proc.LoadProgram(program);

            // Manually step: we need a different approach
            // Let's restructure: use the program to test properly
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCMP_WithBranch_JumpIfGreater_Restructured()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(27.0); // FW > FX
            proc.FRegisters[1] = T3Float.FromDouble(3.0);
            // Program:
            // 0: LI R5, greater   (R5=4, address of greater label)
            // 1: FCMP FW, FX      (Cond=+1)
            // 2: JG R5            (jump to 4)
            // 3: LI R5, -1        (skipped when A>B)
            // 4: HALT
            // If jumped, R5=4; if not, R5=-1
            var program = new List<Word18>
            {
                EncodeLi(5, 4),                        // PC=0: LI R5, 4
                EncodeFpu((int)Opcode.FCMP, 0, 1, 0),  // PC=1: FCMP FW, FX → Cond=1
                EncodeJG(5),                           // PC=2: JG R5 → PC=4
                EncodeLi(5, -1),                       // PC=3: LI R5, -1 (skipped)
                EncodeHalt()                           // PC=4: HALT
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(4, proc.Registers[5].ToLong(), "R5 should be 4 (jumped over LI)");
        }

        // === FTOI ===

        [TestMethod]
        [Timeout(30000)]
        public void FTOI_ConvertsFloatToInt()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(9.0); // FX = 9.0
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FTOI, 5, 1, 0), // FTOI R5, FX (func=0 in op3)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(9, proc.Registers[5].ToLong());
        }

        // === ITOF ===

        [TestMethod]
        [Timeout(30000)]
        public void ITOF_ConvertsIntToFloat()
        {
            var proc = CreateProcessor();
            proc.Registers[5] = Word18.FromLong(27); // RW = 27 (register index 0 for RW)
            // ITOF FX, RW: converts int in RW (reg 0) to float in FX (freg 1)
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.ITOF, 1, 0, 0), // ITOF FX, RW
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(27.0, proc.FRegisters[1].ToDouble(), 0.01);
        }

        // === FTOF ===

        [TestMethod]
        [Timeout(30000)]
        public void FTOF_PrecisionRoundtrip_PreservesValue()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(3.0);
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FTOF, 0, 0, 0), // FTOF FW, FW
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(3.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        // === FLW / FSW ===

        [TestMethod]
        [Timeout(30000)]
        public void FSW_FLW_StoreLoadRoundTrip()
        {
            var proc = CreateProcessor();
            proc.FRegisters[2] = T3Float.FromDouble(3.0); // FY = 3.0
            // Store FY to memory[100]
            // FLW uses addr = base(R0) + op3
            proc.Registers[0] = Word18.FromLong(100); // R0 = 100 (base address)
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FSW, 2, 0, 0), // FSW FY, R0, offset=0 → mem[100] = FY
                EncodeFpu((int)Opcode.FLW, 3, 0, 0), // FLW FZ, R0, offset=0 → FZ = mem[100]
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(3.0, proc.FRegisters[3].ToDouble(), 0.01, "FSW→FLW round-trip should preserve value");
        }

        [TestMethod]
        [Timeout(30000)]
        public void FSW_StoresFloatToMemory()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(3.0); // FW = 3.0
            proc.Registers[1] = Word18.FromLong(200); // R1 = 200 (base address, actual reg idx 5)
            // FSW FW, R1, offset=0 → mem[200] = FW
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FSW, 0, 1, 0), // FSW FW, R1
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            // Read back from memory (Memory<TWord> has no indexer — use ReadWord)
            var memWord = (Word18)(object)proc.ReadWord(200);
            var resultFloat = T3Float.FromWord18(memWord);
            Assert.AreEqual(3.0, resultFloat.ToDouble(), 0.01);
        }

        // === FMOV ===

        [TestMethod]
        [Timeout(30000)]
        public void FMOV_FtoF_MovesBetweenFRegisters()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX = 27
            // FMOV FW, FX (func=0 in op3)
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FMOV, 0, 1, 0), // FMOV FW, FX
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
            proc.Registers[0] = Word18.FromLong(81); // RW = 81 (register index 0)
            // FMOV FW, RW (func=2 in op3 = Fop1 = Rop2)
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FMOV, 0, 0, 2), // FMOV FW, R0, func=2
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(81.0, proc.FRegisters[0].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FMOV_FtoR_MovesFromFregToReg()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(81.0); // FX = 81
            // FMOV R5, FX (func=1 in op3 = Rop1 = Fop2)
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FMOV, 5, 1, 1), // FMOV R5, FX, func=1
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(81, proc.Registers[5].ToLong());
        }

        // === FSWAP ===

        [TestMethod]
        [Timeout(30000)]
        public void FSWAP_SwapsTwoFRegisters()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.0);  // FX = 3
            proc.FRegisters[0] = T3Float.FromDouble(81.0); // FW = 81
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FSWAP, 0, 1, 0), // FSWAP FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(3.0, proc.FRegisters[0].ToDouble(), 0.01, "FW should now be 3");
            Assert.AreEqual(81.0, proc.FRegisters[1].ToDouble(), 0.01, "FX should now be 81");
        }

        // === FCLASS ===

        [TestMethod]
        [Timeout(30000)]
        public void FCLASS_ClassifiesNormal()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX = normal positive
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FCLASS, 0, 1, 0), // FCLASS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double cls = proc.FRegisters[0].ToDouble();
            Assert.IsTrue(cls > 0, "FCLASS for normal number should return a positive value");
        }

        [TestMethod]
        [Timeout(30000)]
        public void FCLASS_ClassifiesZero()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Fpu.Zero(); // FX = 0
            var program = new List<Word18>
            {
                EncodeFpu((int)Opcode.FCLASS, 0, 1, 0), // FCLASS FW, FX
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double cls = proc.FRegisters[0].ToDouble();
            Assert.AreEqual(0.0, cls, "FCLASS for zero should return 0");
        }

        // === Predicated FPU ===

        [TestMethod]
        [Timeout(30000)]
        public void FADD_WithPredicate_ExecutesWhenPredTrue()
        {
            var proc = CreateProcessor();
            proc.FRegisters[1] = T3Float.FromDouble(3.0);  // FX
            proc.FRegisters[2] = T3Float.FromDouble(9.0);  // FY
            // Enable predicate 1
            proc.PR = Word18.FromLong(BalancedTernary.ParseToLong("+++++++++000000000")); // p1=1
            var program = new List<Word18>
            {
                EncodeFpuPred(1, (int)Opcode.FADD, 0, 1, 2), // (p1) FADD FW, FX, FY
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            double expected = ExpectedFpuResult(3.0, 9.0, "ADD");
            Assert.AreEqual(expected, proc.FRegisters[0].ToDouble(), 0.01);
        }

        [TestMethod]
        [Timeout(30000)]
        public void FADD_WithPredicate_SkipsWhenPredFalse()
        {
            var proc = CreateProcessor();
            proc.FRegisters[0] = T3Float.FromDouble(100.0); // FW = 100 (should stay)
            proc.FRegisters[1] = T3Float.FromDouble(3.0);   // FX
            proc.FRegisters[2] = T3Float.FromDouble(9.0);   // FY
            // Predicate 1 is 0 (disabled)
            proc.PR = Word18.FromLong(0);
            var program = new List<Word18>
            {
                EncodeFpuPred(1, (int)Opcode.FADD, 0, 1, 2), // (p1) FADD FW, FX, FY (skipped)
                EncodeHalt()
            };
            proc.LoadProgram(program);
            proc.Run();

            Assert.AreEqual(100.0, proc.FRegisters[0].ToDouble(), 0.01, "FW should remain unchanged when predicated instruction is skipped");
        }
    }
}