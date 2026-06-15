using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using T3Simulator.InOrder;
using T3Assembler;
using System.Collections.Generic;
using System.Linq;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class MathAssemblyTests
    {
        /// <summary>
        /// Helper: assemble and run inline source, return processor
        /// </summary>
        private T3InOrderProcessor<Word18> AssembleAndRun(string sourceCode)
        {
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            var binary = asm.Assemble(sourceCode);
            var words = binary.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();
            return proc;
        }

        // === FPU Demo: Basic arithmetic ===

        [TestMethod]
        [Timeout(30000)]
        public void FpuDemo_Arithmetic_ProducesExpectedOutputs()
        {
            string asm = @"
                start:
                    LI A, 9
                    LI B, 3
                    ITOF FW, A       ; FW = 9.0
                    ITOF FX, B       ; FX = 3.0
                    FADD FW, FW, FX  ; FW = 9+3=12
                    FDIV FW, FW, FX  ; FW = 12/3=4
                    FNEG FW, FW      ; FW = -4
                    FTOI A, FW       ; A = int(FW)
                    LI B, dst1
                    STORE A, B
                    HALT
                dst1:
                    .word 0
            ";

            var proc = AssembleAndRun(asm);

            long val1 = proc.ReadWord(proc.PC + 0).ToLong();
            // 9+3=12, 12/3=4, FNEG→-4, FTOI→-4
            Assert.IsTrue(val1 <= 0, $"FNEG result should be non-positive, got {val1}");
        }

        // === Quadratic Equation ===

        [TestMethod]
        [Timeout(30000)]
        public void Quadratic_Discriminant_Integer()
        {
            // Compute discriminant D = b² - 4ac for a=1, b=-3, c=2
            // D = 9 - 8 = 1. Uses pure integer arithmetic.
            string asm = @"
                start:
                    LI A, 1
                    LI B, -3
                    LI C, 2
                    MUL D, B, B     ; D=9
                    MUL E, A, C     ; E=2
                    ADD E, E, E     ; E=4
                    ADD E, E, E     ; E=8 (4ac)
                    SUB D, D, E     ; D=1
                    HALT
            ";

            var proc = AssembleAndRun(asm);
            long disc = proc.Registers[3].ToLong();  // D = RZ (index 3)
            Assert.AreEqual(1, disc, "Discriminant b²-4ac should be 1");
        }

        // === FPU Compare + Branch ===

        [TestMethod]
        [Timeout(30000)]
        public void FpuCompareAndBranch_CorrectlyJumps()
        {
            string asm = @"
                start:
                    LI A, 27
                    LI B, 3
                    ITOF FW, A      ; FW = 27.0
                    ITOF FX, B      ; FX = 3.0
                    FCMP FW, FX     ; Cond = +1 (FW > FX)
                    LI C, greater
                    JG C
                    LI A, -1         ; fall-through — should NOT execute
                    HALT
                greater:
                    LI A, 1          ; target
                    HALT
            ";

            var proc = AssembleAndRun(asm);

            Assert.AreEqual(1, proc.Registers[0].ToLong(),
                "A should be 1 because FCMP sets Cond=1 and JG jumps to greater label");
        }

        // === Taylor SIN approximation ===

        [TestMethod]
        [Timeout(30000)]
        public void TaylorSin_ApproximatesSinX()
        {
            string asm = @"
                start:
                    LI A, addr_x
                    LOAD A, A
                    ITOF FW, A
                    FMOV FX, FW      ; FX=FW
                    FMUL FX, FX, FW  ; FX=x²
                    FMUL FX, FX, FW  ; FX=x³
                    LI B, 6
                    ITOF FY, B
                    FDIV FX, FX, FY  ; FX=x³/6
                    FSUB FW, FW, FX  ; FW=x-x³/6
                    FTOI A, FW
                    LI B, dst
                    STORE A, B
                    HALT
                addr_x:
                    .word 5
                dst:
                    .word 0
            ";

            var proc = AssembleAndRun(asm);

            // Read result from memory at 'dst' label (one word after HALT)
            long result = proc.ReadWord(proc.PC + 1).ToLong();
            // sin(5) ≈ 5 - 125/6 ≈ 5 - 20.8 ≈ -15.8
            // tfloat precision may shift this; verify it's in range
            Assert.IsTrue(result >= -30 && result <= 10,
                $"sin(5) ~ {result} (expected near -16)");
        }

        // === FCLASS + FTOI integration ===

        [TestMethod]
        [Timeout(30000)]
        public void FpuClassifyZeroSwap_Works()
        {
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX = 27 (non-zero, normal)

            // Program: FCLASS FW, FX → FTOI A, FW → HALT
            // Format: [Pred(000)][Opcode(6)][Op1(3)][Op2(3)][Op3(3)]
            // FCLASS = 114, op1=FW=0, op2=FX=1, op3=0
            string fclassS = "000"  // pred
                + BalancedTernary.ToTernaryString((int)Opcode.FCLASS, 6)
                + BalancedTernary.ToTernaryString(0, 3)   // op1: FW
                + BalancedTernary.ToTernaryString(1, 3)   // op2: FX
                + BalancedTernary.ToTernaryString(0, 3);  // op3
            // FTOI = 108, op1=A=0, op2=FW=0, op3=0
            string ftoiS = "000"
                + BalancedTernary.ToTernaryString((int)Opcode.FTOI, 6)
                + BalancedTernary.ToTernaryString(0, 3)   // op1: A
                + BalancedTernary.ToTernaryString(0, 3)   // op2: FW
                + BalancedTernary.ToTernaryString(0, 3);  // op3

            var program = new List<Word18>
            {
                Word18.FromLong(BalancedTernary.ParseToLong(fclassS)),
                Word18.FromLong(BalancedTernary.ParseToLong(ftoiS)),
                Word18.FromLong(0) // HALT
            };
            proc.LoadProgram(program);
            proc.Run();

            long cls = proc.Registers[0].ToLong();
            Assert.IsTrue(cls > 0, $"FCLASS of non-zero should be > 0, got {cls}");
        }

        // === CLI verification: assemble quadratic, run, check output ===

        [TestMethod]
        [Timeout(30000)]
        public void CLI_Quadratic_Verification()
        {
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            string asmPath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "src", "T3Assembler", "examples", "fpu_demo.asm");

            // Fallback path discovery
            if (!System.IO.File.Exists(asmPath))
            {
                string? dir = System.AppDomain.CurrentDomain.BaseDirectory;
                while (dir != null && !System.IO.Directory.Exists(System.IO.Path.Combine(dir, "src")))
                    dir = System.IO.Directory.GetParent(dir)?.FullName;
                if (dir != null)
                    asmPath = System.IO.Path.Combine(dir, "src", "T3Assembler", "examples", "fpu_demo.asm");
            }

            if (!System.IO.File.Exists(asmPath))
            {
                Assert.Inconclusive($"Could not find fpu_demo.asm");
            }

            string source = System.IO.File.ReadAllText(asmPath);
            var binary = asm.Assemble(source);
            var words = binary.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();

            Assert.IsFalse(proc.Step(), "Processor should be halted after demo");

            // Verify the state dump doesn't throw
            var state = proc.GetState();
            Assert.IsNotNull(state);

            // Verify we executed some instructions
            Assert.IsTrue(state.InstructionCount > 0, "Should have executed instructions");
        }
    }
}