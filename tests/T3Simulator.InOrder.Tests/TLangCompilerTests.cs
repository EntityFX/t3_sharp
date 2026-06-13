using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Compiler;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;
using T3Compiler.CodeGen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using T3Assembler;
using T3Simulator.Common;
using T3Simulator.InOrder;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class TLangCompilerTests
    {
        /// <summary>
        /// End-to-end: compile T source → assemble → run on simulator, verify registers.
        /// </summary>
        private long CompileAndRun(string source, int expectedResult)
        {
            // 1. Preprocess
            var pp = new T3Preprocessor();
            string preprocessed = pp.Process(source);

            // 2. Lex
            var tokenizer = new Tokenizer(preprocessed);
            var tokens = tokenizer.Tokenize();

            // 3. Parse
            var parser = new Parser(tokens);
            var astProgram = parser.ParseProgram();

            // 4. Generate code
            var gen = new CodeGenerator(astProgram);
            string asmCode = gen.Generate();

            // 5. Assemble
            var asm = new T3InOrderAssembler(T3Config.T3_18);
            var binary = asm.Assemble(asmCode);
            var words = binary.Select(x => Word18.FromInt128(x)).ToList();

            // 6. Run on simulator
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();

            // Return value is in R2 per calling convention
            return proc.Registers[2].ToLong();
        }

        [TestMethod]
        public void Compile_Factorial_Returns120()
        {
            string source = @"
                tint main() {
                    tint n = 5;
                    tint result = 1;
                    while (n > 1) {
                        result = result * n;
                        n = n - 1;
                    }
                    return result;
                }
            ";
            long result = CompileAndRun(source, 120);
            Assert.AreEqual(120, result, "5! should be 120");
        }

        [TestMethod]
        public void Compile_Fibonacci_Returns55()
        {
            string source = @"
                tint main() {
                    tint n = 10;
                    tint a = 0;
                    tint b = 1;
                    tint i = 0;
                    while (i < n) {
                        tint tmp = a + b;
                        a = b;
                        b = tmp;
                        i = i + 1;
                    }
                    return a;
                }
            ";
            long result = CompileAndRun(source, 55);
            Assert.AreEqual(55, result, "fib(10) should be 55");
        }

        [TestMethod]
        public void Compile_SimpleArithmetic_Returns42()
        {
            string source = @"
                tint main() {
                    tint x = 40;
                    tint y = 2;
                    return x + y;
                }
            ";
            long result = CompileAndRun(source, 42);
            Assert.AreEqual(42, result, "40 + 2 should be 42");
        }

        [TestMethod]
        public void Compile_IfElse_ReturnsCorrectBranch()
        {
            string source = @"
                tint main() {
                    tint x = 10;
                    if (x > 5) {
                        return 1;
                    } else {
                        return -1;
                    }
                }
            ";
            long result = CompileAndRun(source, 1);
            Assert.AreEqual(1, result, "if x>5 should return 1");
        }

        [TestMethod]
        public void Compile_ForLoop_SumTo5()
        {
            string source = @"
                tint main() {
                    tint sum = 0;
                    tint i = 0;
                    for (i = 1; i <= 5; i = i + 1) {
                        sum = sum + i;
                    }
                    return sum;
                }
            ";
            long result = CompileAndRun(source, 15);
            Assert.AreEqual(15, result, "sum 1..5 should be 15");
        }

        [TestMethod]
        public void Compile_FromFile_Factorial()
        {
            string asmPath = Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "src", "T3Compiler", "examples", "factorial.t");

            if (!File.Exists(asmPath))
            {
                string? dir = System.AppDomain.CurrentDomain.BaseDirectory;
                while (dir != null && !Directory.Exists(Path.Combine(dir, "src")))
                    dir = Directory.GetParent(dir)?.FullName;
                if (dir != null)
                    asmPath = Path.Combine(dir, "src", "T3Compiler", "examples", "factorial.t");
            }

            if (!File.Exists(asmPath))
            {
                Assert.Inconclusive("factorial.t not found");
                return;
            }

            string source = File.ReadAllText(asmPath);
            long result = CompileAndRun(source, 120);
            Assert.AreEqual(120, result, "factorial.t should compile and return 120");
        }

        [TestMethod]
        public void Preprocessor_DefineMacro_ExpandsCorrectly()
        {
            string source = @"
                #define ANSWER 42
                tint main() {
                    return ANSWER;
                }
            ";
            long result = CompileAndRun(source, 42);
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void Preprocessor_IfDef_SkipsBlock()
        {
            string source = @"
                tint main() {
                    #ifdef SKIP_THIS
                    return -1;
                    #else
                    return 1;
                    #endif
                }
            ";
            long result = CompileAndRun(source, 1);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Tokenizer_BalancedTernaryLiteral_ParsesCorrectly()
        {
            // 0t+-- = 9 + (-3) + (-1) = 5
            string source = @"
                tint main() {
                    return 0t+--;
                }
            ";
            long result = CompileAndRun(source, 5);
            Assert.AreEqual(5, result, "0t+-- = 5 in balanced ternary");
        }
    }
}