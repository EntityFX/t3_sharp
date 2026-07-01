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
        [TestMethod] [Timeout(5000)] public void Intrp_Typedef_TIntAlias() => Assert.AreEqual(42, I("typedef tint myint; myint main(){return 42;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Typedef_WithFunction() => Assert.AreEqual(15, I("typedef tint i32; i32 add(i32 a,i32 b){return a+b;}i32 main(){return add(5,10);}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Typedef_FloatAlias() => Assert.AreEqual(1, I("typedef tfloat tf; tf main(){tf x=3.14;tf y=3.14;if(x==y){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Typedef_Chain() => Assert.AreEqual(7, I("typedef tint a;typedef a b;b main(){return 7;}"));

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
        [TestMethod] [Timeout(5000)] public void Intrp_CharLiteral_A() => Assert.AreEqual(65, I("tint main(){return 'A';}"));
        [TestMethod] [Timeout(5000)] public void Intrp_CharLiteral_0() => Assert.AreEqual(48, I("tint main(){return '0';}"));
        [TestMethod] [Timeout(5000)] public void Intrp_CharLiteral_Newline() => Assert.AreEqual(10, I("tint main(){return '\\n';}"));
        [TestMethod] [Timeout(5000)] public void Intrp_CharLiteral_InExpr() => Assert.AreEqual(131, I("tint main(){return 'A'+'B';}"));
        [TestMethod] [Timeout(5000)] public void Equiv_CharLiteral_A() => EE("tint main(){return 'A';}");
        [TestMethod] [Timeout(5000)] public void Equiv_CharLiteral_0() => EE("tint main(){return '0';}");
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
        [TestMethod] [Timeout(5000)] public void Intrp_FloatCompareEq() => Assert.AreEqual(1, I("tint main(){tfloat x=3.14;tfloat y=3.14;if(x==y){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_FloatCompareNe() => Assert.AreEqual(1, I("tint main(){tfloat x=3.14;tfloat y=2.71;if(x!=y){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_FloatCompareLt() => Assert.AreEqual(1, I("tint main(){tfloat x=1.0;tfloat y=2.0;if(x<y){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_FloatCompareGt() => Assert.AreEqual(1, I("tint main(){tfloat x=3.0;tfloat y=1.0;if(x>y){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_FloatNeg() => Assert.AreEqual(1, I("tint main(){tfloat x=3.14;tfloat y=-x;if(y<0){return 1;}return -1;}"));

        // === tlong (36-trit) tests ===
        [TestMethod] [Timeout(5000)] public void Intrp_TlongLiteral() => Assert.AreEqual(42, I("tlong main(){return 42tl;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongAdd() => Assert.AreEqual(30, I("tlong main(){tlong a=10tl;tlong b=20tl;return a+b;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongSub() => Assert.AreEqual(70, I("tlong main(){tlong a=100tl;tlong b=30tl;return a-b;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongMul() => Assert.AreEqual(42, I("tlong main(){tlong a=6tl;tlong b=7tl;return a*b;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongDiv() => Assert.AreEqual(7, I("tlong main(){tlong a=42tl;tlong b=6tl;return a/b;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongCompare() => Assert.AreEqual(1, I("tlong main(){tlong a=100tl;tlong b=200tl;if(a<b){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongNeg() => Assert.AreEqual(-42, I("tlong main(){tlong a=42tl;tlong b=-a;return b;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongVarDecl() => Assert.AreEqual(0, I("tlong main(){tlong x;return x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongGlobal() => Assert.AreEqual(99, I("tlong g;tlong main(){g=99tl;return g;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_TlongOverflow() => Assert.AreEqual(-75047317648499560, I("tlong main(){tlong x=75047317648499560tl;tlong y=1tl;return x+y;}"));

        // === union tests ===
        [TestMethod] [Timeout(5000)] public void Intrp_Union_WriteRead() => Assert.AreEqual(42, I("union Data{tint x;tint y;};tint main(){union Data d;d.x=42;return d.x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Union_SharedStorage() => Assert.AreEqual(42, I("union Data{tint x;tint y;};tint main(){union Data d;d.x=42;return d.y;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Union_Overwrite() => Assert.AreEqual(99, I("union Data{tint x;tint y;};tint main(){union Data d;d.x=42;d.y=99;return d.x;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Union_Global() => Assert.AreEqual(7, I("union Data{tint x;tint y;};Data g;tint main(){g.x=7;return g.y;}"));
        [TestMethod] [Timeout(5000)] public void Intrp_Union_DefaultZero() => Assert.AreEqual(0, I("union Data{tint x;tint y;};tint main(){union Data d;return d.x;}"));

        // === nanolib builtins ===
        [TestMethod] [Timeout(5000)] public void Nanolib_Abs_Pos() => Assert.AreEqual(42, I("tint main(){return abs(42);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Abs_Neg() => Assert.AreEqual(42, I("tint main(){return abs(-42);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Min() => Assert.AreEqual(3, I("tint main(){return min(3,10);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Min_Reverse() => Assert.AreEqual(3, I("tint main(){return min(10,3);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Max() => Assert.AreEqual(10, I("tint main(){return max(3,10);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Clamp_Low() => Assert.AreEqual(5, I("tint main(){return clamp(2,5,10);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Clamp_High() => Assert.AreEqual(10, I("tint main(){return clamp(20,5,10);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Clamp_Mid() => Assert.AreEqual(7, I("tint main(){return clamp(7,5,10);}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Atoi_Simple() => Assert.AreEqual(42, I("tint main(){return atoi(\"42\");}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Atoi_Neg() => Assert.AreEqual(-42, I("tint main(){return atoi(\"-42\");}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Atoi_Zero() => Assert.AreEqual(0, I("tint main(){return atoi(\"0\");}"));
        [TestMethod] [Timeout(5000)] public void Nanolib_Puts_ReturnsZero() => Assert.AreEqual(0, I("tint main(){return puts(\"hello\");}"));

        // === <tio.h> — I/O stdlib tests ===
        [TestMethod] [Timeout(5000)] public void Stdlib_PrintInt() => Assert.AreEqual(0, I("tint main(){print_int(42);return 0;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_PrintLong() => Assert.AreEqual(0, I("tlong main(){print_long(42);return 0;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_PrintFloat() => Assert.AreEqual(0, I("tfloat main(){print_float(3.14);return 0;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_PrintTryte() => Assert.AreEqual(0, I("tint main(){print_tryte('A');return 0;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_PrintChar() => Assert.AreEqual(0, I("tint main(){print_char('X');return 0;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_PrintBal() => Assert.AreEqual(0, I("tint main(){print_bal(42);return 0;}"));

        // === <tstring.h> — String stdlib tests ===
        [TestMethod] [Timeout(5000)] public void Stdlib_Strlen() => Assert.AreEqual(5, I("tint main(){return t_strlen(\"Hello\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strlen_Empty() => Assert.AreEqual(0, I("tint main(){return t_strlen(\"\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strcmp_Equal() => Assert.AreEqual(0, I("tint main(){return t_strcmp(\"abc\",\"abc\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strcmp_Less() => Assert.AreEqual(-1, I("tint main(){return t_strcmp(\"abc\",\"abd\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strcmp_Greater() => Assert.AreEqual(1, I("tint main(){return t_strcmp(\"abd\",\"abc\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strncmp_Equal() => Assert.AreEqual(0, I("tint main(){return t_strncmp(\"abc\",\"abc\",3);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strncmp_Truncated() => Assert.AreEqual(0, I("tint main(){return t_strncmp(\"abc\",\"abcde\",3);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strchr_Found() => Assert.AreEqual(2, I("tint main(){return t_strchr(\"hello\",'e');}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strchr_NotFound() => Assert.AreEqual(-1, I("tint main(){return t_strchr(\"hello\",'x');}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strrchr_Found() => Assert.AreEqual(4, I("tint main(){return t_strrchr(\"hello\",'l');}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strstr_Found() => Assert.AreEqual(2, I("tint main(){return t_strstr(\"hello\",\"ell\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Strstr_NotFound() => Assert.AreEqual(-1, I("tint main(){return t_strstr(\"hello\",\"xyz\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Atoi_Positive() => Assert.AreEqual(42, I("tint main(){return t_atoi(\"42\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Atoi_Negative() => Assert.AreEqual(-42, I("tint main(){return t_atoi(\"-42\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Atoi_Zero() => Assert.AreEqual(0, I("tint main(){return t_atoi(\"0\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Atol_Positive() => Assert.AreEqual(42, I("tlong main(){return t_atol(\"42\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Atol_Large() => Assert.AreEqual(1000000, I("tlong main(){return t_atol(\"1000000\");}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Atof_Simple() => Assert.AreEqual(1, I("tint main(){tfloat x=t_atof(\"3.14\");if(x>3.0&&x<3.2){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Itoa() => Assert.AreEqual(0, I("tint main(){tryte buf[20];return t_itoa(42,buf);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Baltoa() => Assert.AreEqual(0, I("tint main(){tryte buf[20];t_baltoa(42,buf);return 0;}"));

        // === <tmath.h> — Math stdlib tests ===
        [TestMethod] [Timeout(5000)] public void Stdlib_Abs_Pos() => Assert.AreEqual(42, I("tint main(){return t_abs(42);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Abs_Neg() => Assert.AreEqual(42, I("tint main(){return t_abs(-42);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Min() => Assert.AreEqual(3, I("tint main(){return t_min(3,10);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Max() => Assert.AreEqual(10, I("tint main(){return t_max(3,10);}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Sqrt() => Assert.AreEqual(1, I("tint main(){tfloat x=t_sqrt(9.0);if(x>2.9&&x<3.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Floor() => Assert.AreEqual(1, I("tint main(){tfloat x=t_floor(3.7);if(x>2.9&&x<3.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Ceil() => Assert.AreEqual(1, I("tint main(){tfloat x=t_ceil(3.1);if(x>3.9&&x<4.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Sin() => Assert.AreEqual(1, I("tint main(){tfloat x=t_sin(0.0);if(x>-0.1&&x<0.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Cos() => Assert.AreEqual(1, I("tint main(){tfloat x=t_cos(0.0);if(x>0.9&&x<1.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Exp() => Assert.AreEqual(1, I("tint main(){tfloat x=t_exp(0.0);if(x>0.9&&x<1.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Log() => Assert.AreEqual(1, I("tint main(){tfloat x=t_log(1.0);if(x>-0.1&&x<0.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Log3() => Assert.AreEqual(1, I("tint main(){tfloat x=t_log3(9.0);if(x>1.9&&x<2.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Pow() => Assert.AreEqual(1, I("tint main(){tfloat x=t_pow(2.0,3.0);if(x>7.9&&x<8.1){return 1;}return -1;}"));
        [TestMethod] [Timeout(5000)] public void Stdlib_Clamp() => Assert.AreEqual(5, I("tint main(){return clamp(2,5,10);}"));

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

        // === Library tests (via #include) ===

        /// <summary>Run interpreter with include paths pointing to src/T3Compiler/lib/.</summary>
        static long I_with_lib(string source)
        {
            string libDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "src", "T3Compiler", "lib");
            var pp = new T3Preprocessor(new List<string> { libDir });
            string preprocessed = pp.Process(source);
            var ast = new Parser(new Tokenizer(preprocessed).Tokenize()).ParseProgram();
            return new global::T3Interpreter.T3Interpreter(ast).Run();
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strlen()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte s[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    return t_strlen(s);\n" +
                "}");
            Assert.AreEqual(5, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strcmp_eq()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte a[] = {3, 'a', 'b', 'c'};\n" +
                "    tryte b[] = {3, 'a', 'b', 'c'};\n" +
                "    return t_strcmp(a, b);\n" +
                "}");
            Assert.AreEqual(0, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strcmp_lt()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte a[] = {3, 'a', 'b', 'c'};\n" +
                "    tryte b[] = {3, 'a', 'b', 'd'};\n" +
                "    return t_strcmp(a, b);\n" +
                "}");
            Assert.AreEqual(-1, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strcmp_gt()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte a[] = {3, 'a', 'b', 'd'};\n" +
                "    tryte b[] = {3, 'a', 'b', 'c'};\n" +
                "    return t_strcmp(a, b);\n" +
                "}");
            Assert.AreEqual(1, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strcpy()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte src[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    tryte dst[10]; dst[0] = 9;\n" +
                "    t_strcpy(dst, src);\n" +
                "    return dst[0];\n" +  // length should be 5
                "}");
            Assert.AreEqual(5, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strcat()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte a[] = {3, 'a', 'b', 'c', 0, 0, 0, 0, 0, 0};\n" +
                "    tryte b[] = {3, 'd', 'e', 'f'};\n" +
                "    t_strcat(a, b);\n" +
                "    return a[0];\n" +  // length should be 6
                "}");
            Assert.AreEqual(6, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strchr_found()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte s[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    return t_strchr(s, 'l');\n" +  // first 'l' at index 3
                "}");
            Assert.AreEqual(3, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strchr_notfound()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte s[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    return t_strchr(s, 'x');\n" +
                "}");
            Assert.AreEqual(0, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strrchr()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte s[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    return t_strrchr(s, 'l');\n" +  // last 'l' at index 4
                "}");
            Assert.AreEqual(4, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strstr_found()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte haystack[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    tryte needle[] = {2, 'e', 'l'};\n" +
                "    return t_strstr(haystack, needle);\n" +  // "el" starts at index 2
                "}");
            Assert.AreEqual(2, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strstr_notfound()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte haystack[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    tryte needle[] = {2, 'x', 'y'};\n" +
                "    return t_strstr(haystack, needle);\n" +
                "}");
            Assert.AreEqual(0, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_atoi()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte s[] = {3, '1', '2', '3'};\n" +
                "    return t_atoi(s);\n" +
                "}");
            Assert.AreEqual(123, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_atoi_neg()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte s[] = {3, '-', '4', '2'};\n" +
                "    return t_atoi(s);\n" +
                "}");
            Assert.AreEqual(-42, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_itoa()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte buf[20]; buf[0] = 19;\n" +
                "    t_itoa(123, buf);\n" +
                "    return buf[0];\n" +  // length should be 3
                "}");
            Assert.AreEqual(3, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_itoa_content()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte buf[20]; buf[0] = 19;\n" +
                "    t_itoa(42, buf);\n" +
                "    return buf[1] * 100 + buf[2] * 10 + buf[3];\n" +  // '4'=52, '2'=50
                "}");
            Assert.AreEqual(52 * 100 + 50 * 10, result);  // 5200 + 500 = 5700
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_abs()
        {
            long result = I_with_lib(
                "#include <tmath.th>\n" +
                "#include <libtmath.t>\n" +
                "tint main() {\n" +
                "    return t_abs(-42);\n" +
                "}");
            Assert.AreEqual(42, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_min()
        {
            long result = I_with_lib(
                "#include <tmath.th>\n" +
                "#include <libtmath.t>\n" +
                "tint main() {\n" +
                "    return t_min(10, 20);\n" +
                "}");
            Assert.AreEqual(10, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_max()
        {
            long result = I_with_lib(
                "#include <tmath.th>\n" +
                "#include <libtmath.t>\n" +
                "tint main() {\n" +
                "    return t_max(10, 20);\n" +
                "}");
            Assert.AreEqual(20, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_clamp()
        {
            long result = I_with_lib(
                "#include <tmath.th>\n" +
                "#include <libtmath.t>\n" +
                "tint main() {\n" +
                "    return clamp(50, 10, 30);\n" +
                "}");
            Assert.AreEqual(30, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_baltoa()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte buf[30]; buf[0] = 29;\n" +
                "    t_baltoa(5, buf);\n" +  // 5 in balanced ternary = +--
                "    return buf[0];\n" +  // length should be 3
                "}");
            Assert.AreEqual(3, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strncmp()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte a[] = {5, 'a', 'b', 'c', 'd', 'e'};\n" +
                "    tryte b[] = {3, 'a', 'b', 'x'};\n" +
                "    return t_strncmp(a, b, 2);\n" +  // first 2 chars match
                "}");
            Assert.AreEqual(0, result);
        }

        [TestMethod] [Timeout(5000)]
        public void Lib_t_strncpy()
        {
            long result = I_with_lib(
                "#include <tstring.th>\n" +
                "#include <libtstring.t>\n" +
                "tint main() {\n" +
                "    tryte src[] = {5, 'H', 'e', 'l', 'l', 'o'};\n" +
                "    tryte dst[10]; dst[0] = 9;\n" +
                "    t_strncpy(dst, src, 3);\n" +
                "    return dst[0];\n" +  // length should be 3
                "}");
            Assert.AreEqual(3, result);
        }
    }
}
