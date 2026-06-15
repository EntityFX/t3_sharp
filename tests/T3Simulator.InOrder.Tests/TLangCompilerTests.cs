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
        /// End-to-end: compile T source → assemble → run → verify R2.
        /// </summary>
        private long CompileAndRun(string source)
        {
            var pp = new T3Preprocessor();
            string preprocessed = pp.Process(source);
            var tokens = new Tokenizer(preprocessed).Tokenize();
            var ast = new T3Compiler.Parser.Parser(tokens).ParseProgram();
            string asmCode = new T3Compiler.CodeGen.CodeGenerator(ast).Generate();
            var binary = new T3InOrderAssembler(T3Config.T3_18).Assemble(asmCode);
            var words = binary.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();
            return proc.Registers[2].ToLong();
        }

        // === Arithmetic ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_SimpleArithmetic_Returns42()
        {
            Assert.AreEqual(42, CompileAndRun("tint main() { tint x = 40; tint y = 2; return x + y; }"), "40+2=42");
        }

        // === While / For ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_While_SumTo5()
        {
            string s = "tint main() { tint sum = 0; tint i = 1; while (i <= 5) { sum = sum + i; i = i + 1; } return sum; }";
            Assert.AreEqual(15, CompileAndRun(s), "sum 1..5=15");
        }

        [TestMethod]
        [Timeout(30000)]
        public void Compile_For_Factorial()
        {
            string s = "tint main() { tint r = 1; tint i = 1; while (i <= 5) { r = r * i; i = i + 1; } return r; }";
            Assert.AreEqual(120, CompileAndRun(s), "5!=120");
        }

        [TestMethod]
        [Timeout(30000)]
        public void Compile_NestedWhile_SumProd()
        {
            string s = @"tint main() { tint s=0; tint i=1; while(i<=3){tint j=1; while(j<=3){s=s+i*j; j=j+1;} i=i+1;} return s;}";
            Assert.AreEqual(36, CompileAndRun(s), "sum i*j = 36");
        }

        // === If/Else ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_IfElse_Branch()
        {
            Assert.AreEqual(1, CompileAndRun("tint main() { tint x = 10; if (x > 5) { return 1; } else { return -1; } }"));
            Assert.AreEqual(-1, CompileAndRun("tint main() { tint x = 3; if (x > 5) { return 1; } else { return -1; } }"));
        }

        // === Fibonacci ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_Fibonacci_Returns55()
        {
            string s = "tint main() { tint n=10; tint a=0; tint b=1; tint i=0; while(i<n){tint t=a+b; a=b; b=t; i=i+1;} return a; }";
            Assert.AreEqual(55, CompileAndRun(s), "fib(10)=55");
        }

        // === Arrays ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_Array_Sum()
        {
            string s = @"tint main() { tint arr[5]; arr[0]=1; arr[1]=2; arr[2]=3; arr[3]=4; arr[4]=5;
                return arr[0]+arr[1]+arr[2]+arr[3]+arr[4]; }";
            Assert.AreEqual(15, CompileAndRun(s), "array sum=15");
        }

        // === Struct ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_Struct_FieldAccess()
        {
            string s = @"struct Point { tint x; tint y; }
                tint main() { struct Point p; p.x = 10; p.y = 20; return p.x + p.y; }";
            long r = CompileAndRun(s);
            Assert.AreEqual(30, r, "struct field sum = 30");
        }

        // === Pointers ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_Pointer_Deref()
        {
            string s = @"tint main() { tint v = 42; tint* ptr = &v; return *ptr; }";
            long r = CompileAndRun(s);
            Assert.AreEqual(42, r, "pointer deref = 42");
        }

        [TestMethod]
        [Timeout(30000)]
        public void Compile_Pointer_Arithmetic()
        {
            string s = @"tint main() { tint arr[5]; arr[0]=1; arr[1]=2; tint* p = &arr[0];
                p = p + 1; return *p; }";
            long r = CompileAndRun(s);
            Assert.AreEqual(2, r, "ptr+1 = arr[1] = 2");
        }

        // === Matrix multiply ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_MatrixMul_2x2()
        {
            string s = @"
                tint main() {
                    tint A[4]; tint B[4]; tint C[4];
                    A[0]=1; A[1]=2; A[2]=3; A[3]=4;
                    B[0]=5; B[1]=6; B[2]=7; B[3]=8;
                    tint i=0;
                    while(i<2) {
                        tint j=0;
                        while(j<2) {
                            tint sum=0;
                            tint k=0;
                            while(k<2){
                                sum=sum + A[i*2+k]*B[k*2+j];
                                k=k+1;
                            }
                            C[i*2+j]=sum;
                            j=j+1;
                        }
                        i=i+1;
                    }
                    return C[0]+C[1]+C[2]+C[3];
                }";
            // C = [1*5+2*7, 1*6+2*8; 3*5+4*7, 3*6+4*8] = [19,22;43,50]
            // sum = 19+22+43+50 = 134
            long r = CompileAndRun(s);
            Assert.AreEqual(134, r, "matrix C sum = 134");
        }

        // === Nested for (via while) with triangular numbers ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_TriangularSum()
        {
            string s = @"
                tint main() {
                    tint sum = 0;
                    tint i = 1;
                    while (i <= 4) {
                        tint j = 1;
                        while (j <= i) {
                            sum = sum + j;
                            j = j + 1;
                        }
                        i = i + 1;
                    }
                    return sum;
                }";
            // 1 + (1+2) + (1+2+3) + (1+2+3+4) = 1+3+6+10 = 20
            Assert.AreEqual(20, CompileAndRun(s), "triangular sum=20");
        }

        // === Boolean (tril) logic ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_BooleanOps()
        {
            // true=1, false=-1
            string s = @"
                tint main() {
                    tril a = true;
                    tril b = false;
                    if (a == true) { return 1; }
                    return -1;
                }";
            Assert.AreEqual(1, CompileAndRun(s), "true==true → return 1");
        }

        // === Preprocessor ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_Preprocessor_Define()
        {
            string s = "#define ANSWER 42\ntint main() { return ANSWER; }";
            Assert.AreEqual(42, CompileAndRun(s));
        }

        [TestMethod]
        [Timeout(30000)]
        public void Compile_Preprocessor_IfDef()
        {
            string s = "tint main() { #ifdef SKIP\nreturn -1; #else\nreturn 1; #endif\n}";
            Assert.AreEqual(1, CompileAndRun(s));
        }

        // === Ternary literal ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_TernaryLiteral()
        {
            // 0t+-- = 9 + (-3) + (-1) = 5
            Assert.AreEqual(5, CompileAndRun("tint main() { return 0t+--; }"), "0t+-- = 5");
        }

        // === File-based factorial ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_FromFile_Factorial()
        {
            string asmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..", "src", "T3Compiler", "examples", "factorial.t");
            if (!File.Exists(asmPath))
            {
                string? d = AppDomain.CurrentDomain.BaseDirectory;
                while (d != null && !Directory.Exists(Path.Combine(d, "src"))) d = Directory.GetParent(d)?.FullName;
                if (d != null) asmPath = Path.Combine(d, "src", "T3Compiler", "examples", "factorial.t");
            }
            if (!File.Exists(asmPath)) { Assert.Inconclusive("factorial.t not found"); return; }
            Assert.AreEqual(120, CompileAndRun(File.ReadAllText(asmPath)));
        }

        // === Pointer + array + struct combo ===
        [TestMethod]
        [Timeout(30000)]
        public void Compile_PointerArrayStruct()
        {
            string s = @"
                struct Vec { tint a; tint b; }
                tint main() {
                    struct Vec v;
                    v.a = 7;
                    v.b = 3;
                    tint* pa = &v.a;
                    tint* pb = &v.b;
                    return *pa + *pb;
                }";
            Assert.AreEqual(10, CompileAndRun(s), "struct pointer fields sum = 10");
        }
    }
}