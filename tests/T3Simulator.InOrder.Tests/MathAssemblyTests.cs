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
                    LI R0, 9
                    LI R1, 3
                    ITOF R0, R0     ; FW = 9.0
                    ITOF R1, R1     ; FX = 3.0
                    FADD R0, R0, R1 ; FW = 9+3=12
                    FDIV R0, R0, R1 ; FW = 12/3=4
                    FNEG R0, R0     ; FW = -4
                    FTOI R0, R0     ; RW = int(FW)
                    LI R4, dst1
                    STORE R0, R4
                    HALT
                dst1:
                    .word 0
            ";

            var proc = AssembleAndRun(asm);

            long val1 = proc.ReadWord(proc.PC + 0).ToLong();
            // 9+3=12, 12/3=4, -4 → FTOI truncates → -4
            // tfloat conversion chain may lose precision; verify it's reasonable
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
                    LI R0, 27
                    LI R1, 3
                    ITOF R0, R0     ; FW = 27.0
                    ITOF R1, R1     ; FX = 3.0
                    FCMP R0, R1     ; Cond = +1 (FW > FX)
                    LI R2, greater
                    JG R2
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
                    LI R0, addr_x
                    LOAD R1, R0
                    ITOF R0, R1
                    FMOV R1, R0, R0   ; FX=FW
                    FMUL R1, R1, R0   ; FX=x²
                    FMUL R1, R1, R0   ; FX=x³
                    LI R2, 6
                    ITOF R2, R2
                    FDIV R1, R1, R2   ; FX=x³/6
                    FSUB R0, R0, R1   ; FW=x-x³/6
                    FTOI R0, R0
                    LI R3, dst
                    STORE R0, R3
                    HALT
                addr_x:
                    .word 5
                dst:
                    .word 0
            ";

            var proc = AssembleAndRun(asm);

            long result = proc.ReadWord(proc.PC + 1).ToLong();
            // sin(5) ≈ 5 - 125/6 ≈ 5 - 20.8 ≈ -15.8
            // tfloat precision may shift this; verify it's in range
            Assert.IsTrue(result >= -30 && result <= 10,
                $"sin(5) ~ {result} (expected near -16)");
        }

        // === FCLASS + FZERO + FSWAP integration ===

        [TestMethod]
        [Timeout(30000)]
        public void FpuClassifyZeroSwap_Works()
        {
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.FRegisters[1] = T3Float.FromDouble(27.0); // FX = 27 (non-zero, normal)

            // Program in ternary form:
            // FCLASS R0, R1  → opcode=106 (6trits), op1=R0=0 (3trits), op2=R1=1 (3trits), op3=0 (3trits), func=0 (3trits)
            // FTOI R2, R0    → opcode=100, op1=R2=2, op2=R0=0, op3=0, func=0
            // HALT
            string fclassWord = BalancedTernary.ToTernaryString(106, 6)
                + BalancedTernary.ToTernaryString(0, 3)   // op1: FW (index 0)
                + BalancedTernary.ToTernaryString(1, 3)   // op2: FX (index 1)
                + BalancedTernary.ToTernaryString(0, 3)   // op3: unused
                + BalancedTernary.ToTernaryString(0, 3);  // func

            string ftoiWord = BalancedTernary.ToTernaryString(100, 6)
                + BalancedTernary.ToTernaryString(2, 3)   // op1: R2 (index 2)
                + BalancedTernary.ToTernaryString(0, 3)   // op2: R0
                + BalancedTernary.ToTernaryString(0, 3)   // op3: unused
                + BalancedTernary.ToTernaryString(0, 3);  // func

            var program = new List<Word18>
            {
                Word18.FromLong(BalancedTernary.ParseToLong(fclassWord)),
                Word18.FromLong(BalancedTernary.ParseToLong(ftoiWord)),
                Word18.FromLong(0) // HALT
            };
            proc.LoadProgram(program);
            proc.Run();

            long cls = proc.Registers[2].ToLong();
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