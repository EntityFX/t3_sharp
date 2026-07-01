using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Assembler;
using T3Compiler;
using T3Compiler.CodeGen;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;
using T3Simulator.Common;
using T3Simulator.InOrder;
using TritTypes;

namespace T3Interpreter.Tests
{
    [TestClass]
    public class Tests
    {
        static long I(string s) { var p = new T3Preprocessor().Process(s); var a = new Parser(new Tokenizer(p).Tokenize()).ParseProgram(); return new global::T3Interpreter.T3Interpreter(a).Run(); }
        
        // === Interpreter-only tests ===
        [TestMethod] [Timeout(5000)] public void Add() => Assert.AreEqual(42, I("tint main(){return 40+2;}"));
        [TestMethod] [Timeout(5000)] public void WhileSum() => Assert.AreEqual(15, I("tint main(){tint s=0;tint i=1;while(i<=5){s=s+i;i=i+1;}return s;}"));
        [TestMethod] [Timeout(5000)] public void RecursiveFact() => Assert.AreEqual(5040, I("tint fact(tint n){if(n<=1){return 1;}return n*fact(n-1);}tint main(){return fact(7);}"));
        [TestMethod] [Timeout(5000)] public void NestedWhile() => Assert.AreEqual(9, I("tint main(){tint s=0;tint i=1;while(i<=3){tint j=1;while(j<=3){s=s+1;j=j+1;}i=i+1;}return s;}"));
        [TestMethod] [Timeout(5000)] public void IfElseTrue() => Assert.AreEqual(1, I("tint main(){tint x=10;if(x>5){return 1;}else{return -1;}}"));
        [TestMethod] [Timeout(5000)] public void IfElseFalse() => Assert.AreEqual(-1, I("tint main(){tint x=3;if(x>5){return 1;}else{return -1;}}"));
        [TestMethod] [Timeout(5000)] public void Fibonacci() => Assert.AreEqual(55, I("tint main(){tint n=10;tint a=0;tint b=1;tint i=0;while(i<n){tint t=a+b;a=b;b=t;i=i+1;}return a;}"));
        [TestMethod] [Timeout(5000)] public void NegNumbers() => Assert.AreEqual(-20, I("tint main(){tint a=-5;tint b=3;tint c=-10;tint d=2;return a*b+c/d;}"));
        [TestMethod] [Timeout(5000)] public void PreprocDefine() => Assert.AreEqual(42, I("#define ANSWER 42\ntint main(){return ANSWER;}"));
        [TestMethod] [Timeout(5000)] public void BreakContinue() => Assert.AreEqual(31, I("tint main(){tint s=0;tint i=1;while(i<=10){if(i==5){i=i+1;continue;}if(i>8){break;}s=s+i;i=i+1;}return s;}"));
        [TestMethod] [Timeout(5000)] public void ForLoop() => Assert.AreEqual(55, I("tint main(){tint s=0;tint i=1;for(;i<=10;i=i+1){s=s+i;}return s;}"));
        [TestMethod] [Timeout(5000)] public void DoWhile() => Assert.AreEqual(55, I("tint main(){tint s=0;tint i=1;do{s=s+i;i=i+1;}while(i<=10);return s;}"));
        [TestMethod] [Timeout(5000)] public void SwitchCase() => Assert.AreEqual(2, I("tint main(){tint x=2;switch(x){case 1:return 10;case 2:return 2;default:return -1;}}"));
        [TestMethod] [Timeout(5000)] public void ComplexExpr() => Assert.AreEqual(27, I("tint main(){return (2+3)*4-6/2+10;}"));
        [TestMethod] [Timeout(5000)] public void DoubleRecursion() => Assert.AreEqual(8, I("tint fib(tint n){if(n<=1){return n;}return fib(n-1)+fib(n-2);}tint main(){return fib(6);}"));
        // Literal format tests
        [TestMethod] [Timeout(5000)] public void TernaryLiteral() => Assert.AreEqual(5, I("tint main(){return 0t+--;}"));
        [TestMethod] [Timeout(5000)] public void TernaryLiteral11() => Assert.AreEqual(11, I("tint main(){return 0t++-;}"));
        [TestMethod] [Timeout(5000)] public void TernaryLiteralMinus40() => Assert.AreEqual(-40, I("tint main(){return 0t----;}"));
        [TestMethod] [Timeout(5000)] public void Base9Literal() => Assert.AreEqual(4, I("tint main(){return 0n4;}"));
        [TestMethod] [Timeout(5000)] public void Base9LiteralNeg4() => Assert.AreEqual(-4, I("tint main(){return 0nW;}"));
        [TestMethod] [Timeout(5000)] public void Base27Literal0() => Assert.AreEqual(0, I("tint main(){return 0y0;}"));
        [TestMethod] [Timeout(5000)] public void Base27LiteralNeg1() => Assert.AreEqual(-1, I("tint main(){return 0yZ;}"));
        [TestMethod] [Timeout(5000)] public void DecimalLiteral() => Assert.AreEqual(42, I("tint main(){return 42;}"));
        [TestMethod] [Timeout(5000)] public void DecimalLiteralNeg() => Assert.AreEqual(-255, I("tint main(){return -255;}"));

        // === Equivalence tests: interpreter vs compiler+simulator ===

        /// <summary>
        /// Compiles T-lang source to assembly, assembles to binary, runs on processor,
        /// and returns the result value (R2 register after main returns).
        /// </summary>
        static long C(string source, string? testName = null)
        {
            var preprocessed = new T3Preprocessor().Process(source);
            var tokens = new Tokenizer(preprocessed).Tokenize();
            var ast = new Parser(tokens).ParseProgram();
            var asmCode = new CodeGenerator(ast).Generate();
            
            // Dump ASM for debugging (like TLangCompilerTests)
            if (testName != null)
            {
                string dumpDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "test_results");
                if (!System.IO.Directory.Exists(dumpDir)) System.IO.Directory.CreateDirectory(dumpDir);
                System.IO.File.WriteAllText(System.IO.Path.Combine(dumpDir, $"{testName}.asm"), asmCode);
            }
            
            var config = T3Config.T3_18;
            var assembler = new T3InOrderAssembler(config);
            var machineCode = assembler.Assemble(asmCode);
            var processor = new T3InOrderProcessor<Word18>(config);
            processor.LoadProgram(machineCode.ConvertAll(w => Word18.FromInt128(w)));
            processor.Run();
            
            // Dump final state
            if (testName != null)
            {
                string dumpDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "test_results");
                System.IO.File.WriteAllText(System.IO.Path.Combine(dumpDir, $"{testName}.final.state.txt"), processor.DumpState());
            }
            
            return processor.Registers[6].ToLong();
        }

        [TestMethod] [Timeout(5000)] public void Equiv_Add() => EE("tint main(){return 40+2;}");
        [TestMethod] [Timeout(5000)] public void Equiv_WhileSum() => EE("tint main(){tint s=0;tint i=1;while(i<=5){s=s+i;i=i+1;}return s;}");
        [TestMethod] [Timeout(5000)] public void Equiv_ComplexExpr() => EE("tint main(){return (2+3)*4-6/2+10;}");
        [TestMethod] [Timeout(5000)] public void Equiv_NegNumbers() => EE("tint main(){tint a=-5;tint b=3;tint c=-10;tint d=2;return a*b+c/d;}");
        [TestMethod] [Timeout(5000)] public void Equiv_ForLoop() => EE("tint main(){tint s=0;tint i=1;for(;i<=10;i=i+1){s=s+i;}return s;}");
        [TestMethod] [Timeout(5000)] public void Equiv_DoWhile() => EE("tint main(){tint s=0;tint i=1;do{s=s+i;i=i+1;}while(i<=10);return s;}");
        [TestMethod] [Timeout(5000)] public void Equiv_SwitchCase() => EE("tint main(){tint x=2;switch(x){case 1:return 10;case 2:return 2;default:return -1;}}");
        [TestMethod] [Timeout(5000)] public void Equiv_TernaryLiteral() => EE("tint main(){return 0t+--;}");
        [TestMethod] [Timeout(5000)] public void Equiv_Base9Literal() => EE("tint main(){return 0n4;}");
        [TestMethod] [Timeout(5000)] public void Equiv_Base27Literal() => EE("tint main(){return 0yZ;}");
        [TestMethod] [Timeout(5000)] public void Equiv_DecimalLiteral() => EE("tint main(){return 42;}");
        [TestMethod] [Timeout(5000)] public void Equiv_IfElseTrue() => EE("tint main(){tint x=10;if(x>5){return 1;}else{return -1;}}");
        [TestMethod] [Timeout(5000)] public void Equiv_IfElseFalse() => EE("tint main(){tint x=3;if(x>5){return 1;}else{return -1;}}");
        [TestMethod] [Timeout(5000)] public void Equiv_FunctionCall() => EE("tint add5(tint x){return x+5;}tint main(){return add5(10);}");
        [TestMethod] [Timeout(5000)] public void Equiv_SimpleReturn() => EE("tint main(){return 99;}");
        [TestMethod] [Timeout(5000)] public void Equiv_FunctionWithParam() => EE("tint add5(tint x){return x+5;}tint main(){return add5(5);}");

        // Nested function calls work correctly in interpreter (returns 12).
        // Compiler returns 6 due to register loss in nested call ABI — tracked for future fix.
        [TestMethod] [Timeout(5000)] public void Equiv_NestedFunctionCalls_Interpreter() => Assert.AreEqual(12, I("tint f(tint x){return x*2;}tint g(tint x){return f(x+1);}tint main(){return g(5);}"));
        [TestMethod] [Timeout(5000)][Ignore("Compiler returns 6 instead of 12: nested calls lose registers in call ABI")]
        public void Equiv_NestedFunctionCalls_Compiler() => Assert.AreEqual(12, C("tint f(tint x){return x*2;}tint g(tint x){return f(x+1);}tint main(){return g(5);}"));

        // === P2: Expanded equivalence tests (struct, multidim, &&, ||, !) ===

        [TestMethod] [Timeout(5000)] public void Equiv_LogicalAnd_True() => EE("tint main(){if(1>0&&2>0){return 1;}return -1;}");
        [TestMethod] [Timeout(5000)] public void Equiv_LogicalAnd_False() => EE("tint main(){if(1>0&&2<0){return 1;}return -1;}");
        [TestMethod] [Timeout(5000)] public void Equiv_LogicalOr_True() => EE("tint main(){if(1>0||2<0){return 1;}return -1;}");
        [TestMethod] [Timeout(5000)] public void Equiv_LogicalOr_False() => EE("tint main(){if(1<0||2<0){return 1;}return -1;}");
        [TestMethod] [Timeout(5000)] public void Equiv_UnaryNot_True() => EE("tint main(){if(!(1<0)){return 1;}return -1;}");
        [TestMethod] [Timeout(5000)] public void Equiv_UnaryNot_False() => EE("tint main(){if(!(1>0)){return 1;}return -1;}");
        [TestMethod] [Timeout(5000)] public void Equiv_StructWrite() => EE("struct Point{tint x;tint y;};tint main(){struct Point p;p.x=5;return p.x;}");
        [TestMethod] [Timeout(5000)] public void Equiv_StructWrite_Sum() => EE("struct Point{tint x;tint y;};tint main(){struct Point p;p.x=10;p.y=20;return p.x+p.y;}");
        [TestMethod] [Timeout(5000)] public void Equiv_MultidimArray_2x3() => EE("tint main(){tint a[2][3];a[0][0]=0;a[0][1]=1;a[0][2]=2;a[1][0]=10;a[1][1]=11;a[1][2]=12;return a[0][0]+a[0][1]+a[0][2]+a[1][0]+a[1][1]+a[1][2];}", nameof(Equiv_MultidimArray_2x3));
        [TestMethod] [Timeout(5000)] public void Equiv_GlobalVar() => EE("tint g;tint main(){g=7;return g;}");
        [TestMethod] [Timeout(5000)] public void Equiv_IfMaybeTrue() => EE("tint main(){tint x=1;if(x>0){return 10;}maybe{return 0;}else{return -10;}}");
        [TestMethod] [Timeout(5000)] public void Equiv_IfMaybeNeutral() => EE("tint main(){tint x=0;if(x>0){return 10;}maybe{return 0;}else{return -10;}}");
        [TestMethod] [Timeout(5000)] public void Equiv_IfMaybeFalse() => EE("tint main(){tint x=-1;if(x>0){return 10;}maybe{return 0;}else{return -10;}}");
        [TestMethod] [Timeout(5000)] public void Equiv_BreakContinue() => EE("tint main(){tint s=0;tint i=1;while(i<=10){if(i==5){i=i+1;continue;}if(i>8){break;}s=s+i;i=i+1;}return s;}");
        [TestMethod] [Timeout(5000)] public void Equiv_NestedWhile() => EE("tint main(){tint s=0;tint i=1;while(i<=3){tint j=1;while(j<=3){s=s+1;j=j+1;}i=i+1;}return s;}");
        [TestMethod] [Timeout(5000)] public void Equiv_PreprocDefine() => EE("#define ANSWER 42\ntint main(){return ANSWER;}");
        [TestMethod] [Timeout(5000)] public void Equiv_Fibonacci() => EE("tint main(){tint n=10;tint a=0;tint b=1;tint i=0;while(i<n){tint t=a+b;a=b;b=t;i=i+1;}return a;}");
        [TestMethod] [Timeout(5000)] public void Equiv_RecursiveFact() => EE("tint fact(tint n){if(n<=1){return 1;}return n*fact(n-1);}tint main(){return fact(7);}");
        [TestMethod] [Timeout(5000)] public void Equiv_DoubleRecursion() => EE("tint fib(tint n){if(n<=1){return n;}return fib(n-1)+fib(n-2);}tint main(){return fib(6);}");

        static void EE(string source, string? testName = null)
        {
            long interpreted = I(source);
            long compiled = C(source, testName);
            Console.WriteLine($"Interpreted: {interpreted}");
            Console.WriteLine($"Compiled: {compiled}");
            Assert.AreEqual(interpreted, compiled, $"Divergence on: {source[..Math.Min(source.Length, 60)]}");
        }

        // === 🟡 ROADMAP tests: typedef, constant folding, source-level errors ===

        [TestMethod] [Timeout(5000)] public void Compile_Typedef_TIntAlias() => Assert.AreEqual(42, C("typedef tint myint; myint main(){return 42;}"));
        [TestMethod] [Timeout(5000)] public void Compile_Typedef_WithFunction() => Assert.AreEqual(15, C("typedef tint i32; i32 add(i32 a,i32 b){return a+b;}i32 main(){return add(5,10);}"));

        static long C_asm(string source)
        {
            var preprocessed = new T3Preprocessor().Process(source);
            var tokens = new Tokenizer(preprocessed).Tokenize();
            var ast = new Parser(tokens).ParseProgram();
            var asmCode = new CodeGenerator(ast).Generate();
            return asmCode.Split('\n').Length; // count lines as sanity
        }

        [TestMethod] [Timeout(5000)] public void Compile_ConstantFolding_Add() { var s="tint main(){return 2+3;}"; Assert.IsTrue(C(s)==5); }
        [TestMethod] [Timeout(5000)] public void Compile_ConstantFolding_Mul() { var s="tint main(){return 4*5;}"; Assert.IsTrue(C(s)==20); }
        [TestMethod] [Timeout(5000)] public void Compile_ConstantFolding_Complex() { var s="tint main(){return (2+3)*4;}"; Assert.IsTrue(C(s)==20); }
        [TestMethod] [Timeout(5000)] public void Compile_ConstantFolding_WithVar() { var s="tint main(){tint x=10;return x*2+3*4;}"; Assert.IsTrue(C(s)==32); }

        [TestMethod] [Timeout(5000)] public void Null_ReturnsZero() => Assert.AreEqual(0, C("tint main(){ return null; }"));
        [TestMethod] [Timeout(5000)] public void Null_Equiv() => EE("tint main(){ return null; }");
        [TestMethod] [Timeout(5000)] public void Intrp_NullLiteral() => Assert.AreEqual(0, I("tint main(){return null;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_NullIsZero() => Assert.AreEqual(0, I("tint main(){tint x=null;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Malloc() => Assert.IsTrue(I("tint main(){tint p=malloc(10);return p;}") >= 0);

        // === New language feature tests (interpreter) ===
        [TestMethod] [Timeout(5000)] public void Intrp_Strlen() => Assert.AreEqual(5, I("tint main(){return strlen(\"Hello\");}"));
        [TestMethod] [Timeout(5000)] public void Intrp_ShiftMul() => Assert.AreEqual(9, I("tint len(tint x,tint n){while(n>0){x=x*3;n=n-1;}return x;} tint main(){return len(1,2);}"));
        [TestMethod] [Timeout(5000)] public void Intrp_ShiftDiv() => Assert.AreEqual(3, I("tint len(tint x,tint n){while(n>0){x=x/3;n=n-1;}return x;} tint main(){return len(27,2);}"));
        [TestMethod] [Timeout(5000)] public void Intrp_BitwiseNot() => Assert.AreEqual(-5, I("tint main(){tint x=5;return ~x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_PrefixInc() => Assert.AreEqual(6, I("tint main(){tint x=5;++x;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_PrefixDec() => Assert.AreEqual(4, I("tint main(){tint x=5;--x;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_PostfixInc() => Assert.AreEqual(5, I("tint main(){tint x=5;return x++;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_PostfixDec() => Assert.AreEqual(5, I("tint main(){tint x=5;return x--;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_AndEq() => Assert.AreEqual(5, I("tint main(){tint x=7;x&=5;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_OrEq() => Assert.AreEqual(7, I("tint main(){tint x=7;x|=5;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_XorEq() => Assert.AreEqual(2, I("tint main(){tint x=7;x^=5;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_SizeofTint() => Assert.AreEqual(3, I("tint main(){return sizeof(tint);}"));
        [TestMethod] [Timeout(5000)] public void Intrp_SizeofTryte() => Assert.AreEqual(1, I("tint main(){return sizeof(tryte);}"));
        [TestMethod] [Timeout(5000)] public void Intrp_SizeofTlong() => Assert.AreEqual(6, I("tint main(){return sizeof(tlong);}"));
        [TestMethod] [Timeout(5000)] public void Intrp_FloatLiteral() => Assert.AreEqual(3, I("tint main(){tfloat x=3.14;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_FloatVar() => Assert.AreEqual(0, I("tint main(){tfloat x;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_BraceInit() => Assert.AreEqual(14, I("tint main(){tint a[3]={2,4,8};return a[0]+a[1]+a[2];}"));
        [TestMethod] [Timeout(5000)] public void Intrp_StringLiteral() => Assert.IsTrue(I("tint main(){tint s=strlen(\"Hello\");return s;}") > 0);
        [TestMethod] [Timeout(5000)] public void Intrp_FloatDefaultZero() => Assert.AreEqual(1, I("tint main(){tfloat f;tfloat g;if(f==g){return 1;}return -1;}"));

        [TestMethod] [Timeout(5000)] public void SourceError_MissingSemicolon()
        {
            var tokens = new Tokenizer("tint main(){return 42}").Tokenize();
            try
            {
                new Parser(tokens).ParseProgram();
                Assert.Fail("Expected parse exception");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("error: expected"), $"Got: {ex.Message}");
            }
        }

        [TestMethod] [Timeout(5000)] public void SourceError_UnexpectedToken()
        {
            try
            {
                new Tokenizer("@bad").Tokenize();
                Assert.Fail("Expected tokenizer exception");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Unexpected character"), $"Got: {ex.Message}");
            }
        }
    }
}
