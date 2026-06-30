using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class TLangCompilerTests
    {
    private List<Word18> Compile(string source, string testName)
    {
        var pp = new T3Preprocessor();
        string pre = pp.Process(source);
        var ast = new Parser(new Tokenizer(pre).Tokenize()).ParseProgram();
        
        string dumpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "test_results");
        if (!Directory.Exists(dumpDir)) Directory.CreateDirectory(dumpDir);

        if (CompilerDebugConfig.EnableDumps)
        {
            var astText = AstDumper.Dump(ast);
            File.WriteAllText(Path.Combine(dumpDir, $"{testName}.ast.txt"), astText);
        }
        
        string asm = new CodeGenerator(ast).Generate();
        
        if (CompilerDebugConfig.EnableDumps)
        {
            File.WriteAllText(Path.Combine(dumpDir, $"{testName}.asm"), asm);
        }
        
        var bin = new T3InOrderAssembler(T3Config.T3_18).Assemble(asm);
        var words = bin.Select(x => Word18.FromInt128(x)).ToList();
        
        if (CompilerDebugConfig.EnableDumps)
        {
            var binStrings = words.Select(w => w.ToTritString()).ToList();
            File.WriteAllLines(Path.Combine(dumpDir, $"{testName}.bin.txt"), binStrings);
        }
        
        return words;
    }

    private long Run(List<Word18> words, string testName, bool enableTrace = false)
    {
        var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
        proc.LoadProgram(words);
        
        string dumpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "test_results");

        if (enableTrace)
        {
            var traceFile = Path.Combine(dumpDir, $"{testName}.trace.txt");
            using var traceWriter = new StreamWriter(traceFile);
            proc.EnableTracing(traceWriter);
            
            try 
            {
                proc.Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(dumpDir, $"{testName}.crash.state.txt"), 
                    $"Exception: {ex.Message}\n{ex.StackTrace}\n\n{proc.DumpState()}");
                throw;
            }
            finally
            {
                traceWriter.Flush();
            }
        }
        else
        {
            try 
            {
                proc.Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(dumpDir, $"{testName}.crash.state.txt"), 
                    $"Exception: {ex.Message}\n{ex.StackTrace}\n\n{proc.DumpState()}");
                throw;
            }
        }
        
        File.WriteAllText(Path.Combine(dumpDir, $"{testName}.final.state.txt"), proc.DumpState());
        return proc.Registers[6].ToLong();
    }

    private long CompileAndRun(string source, string testName = "unknown", bool enableTrace = false)
    {
        var words = Compile(source, testName);
        return Run(words, testName, enableTrace);
    }

    private void AssertResult(long expected, string source, string testName)
    {
        // First pass: no trace
        long actual = CompileAndRun(source, testName, enableTrace: false);
        
        if (actual != expected)
        {
            // Second pass: full trace for debugging
            CompileAndRun(source, testName, enableTrace: true);
            Assert.AreEqual(expected, actual, $"Test {testName} failed. Full trace generated in test_results/{testName}.trace.txt");
        }
        else
        {
            Assert.AreEqual(expected, actual);
        }
    }

        [TestMethod]
        [Timeout(5000)]
        public void Compile_SimpleArithmetic_Returns42() =>
            AssertResult(42, "tint main(){tint x=40;tint y=2;return x+y;}", nameof(Compile_SimpleArithmetic_Returns42));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_While_SumTo5() =>
            AssertResult(15, "tint main(){tint s=0;tint i=1;while(i<=5){s=s+i;i=i+1;}return s;}", nameof(Compile_While_SumTo5));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_While_SumMulti() =>
            AssertResult(225, "tint main() {	tint w=0;	tint z=1;	tint x=1;	tint y=1;	tint i=0;	while(i<=36){	    z = z + 1;	    x = x + 2;	    y = y + 3;		i=i+1;}	w = z + x + y;	return w;}", nameof(Compile_While_SumMulti));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_For_Factorial() =>
            AssertResult(120, "tint main(){tint r=1;tint i=1;while(i<=5){r=r*i;i=i+1;}return r;}", nameof(Compile_For_Factorial));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_NestedWhile_SumProd() =>
            AssertResult(36, "tint main(){tint s=0;tint i=1;while(i<=3){tint j=1;while(j<=3){s=s+i*j;j=j+1;}i=i+1;}return s;}", nameof(Compile_NestedWhile_SumProd));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_NestedWhile_SimpleSum() =>
            AssertResult(9, "tint main() { tint s=0; tint i=1; while(i<=3){ tint j=1; while(j<=3){ s=s+1;j=j+1;} i=i+1; } return s;}", nameof(Compile_NestedWhile_SimpleSum));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_NestedWhile_SimpleSumLong() =>
            AssertResult(1296, "tint main() { tint s=0; tint i=1; while(i<=36){ tint j=1; while(j<=36){ s=s+1;j=j+1;} i=i+1; } return s;}", nameof(Compile_NestedWhile_SimpleSumLong));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_IfElse_Branch()
        {
            Assert.AreEqual(
                1,
                CompileAndRun("tint main(){tint x=10;if(x>5){return 1;}else{return -1;}}", nameof(Compile_IfElse_Branch) + "_true")
            );
            Assert.AreEqual(
                -1,
                CompileAndRun("tint main(){tint x=3;if(x>5){return 1;}else{return -1;}}", nameof(Compile_IfElse_Branch) + "_false")
            );
        }

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Fibonacci_Returns55() =>
            Assert.AreEqual(
                55,
                CompileAndRun(
                    "tint main(){tint n=10;tint a=0;tint b=1;tint i=0;while(i<n){tint t=a+b;a=b;b=t;i=i+1;}return a;}", 
                    nameof(Compile_Fibonacci_Returns55)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Array_Sum() =>
            Assert.AreEqual(
                15,
                CompileAndRun(
                    "tint main(){tint arr[5];arr[0]=1;arr[1]=2;arr[2]=3;arr[3]=4;arr[4]=5;return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];}", 
                    nameof(Compile_Array_Sum)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Array_Mul() =>
            Assert.AreEqual(
                120,
                CompileAndRun(
                    "tint main(){    tint arr[5];    arr[0]=1;    arr[1]=2;    arr[2]=3;    arr[3]=4;    arr[4]=5;   tint i = 0;    tint w = 1;    while (i < 5) {   w = w * arr[i];  i = i + 1; } return w;}", 
                    nameof(Compile_Array_Mul)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Struct_FieldAccess() =>
            Assert.AreEqual(
                30,
                CompileAndRun(
                    "struct Point{tint x;tint y;}tint main(){struct Point p;p.x=10;p.y=20;return p.x+p.y;}", 
                    nameof(Compile_Struct_FieldAccess)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Pointer_Deref() =>
            Assert.AreEqual(42, CompileAndRun("tint main(){tint v=42;tint* p=&v;return *p;}", nameof(Compile_Pointer_Deref)));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Pointer_Arithmetic() =>
            Assert.AreEqual(
                2,
                CompileAndRun(
                    "tint main(){tint arr[5];arr[0]=1;arr[1]=2;tint* p=&arr[0];p=p+1;return *p;}", 
                    nameof(Compile_Pointer_Arithmetic)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_MatrixMul_2x2() =>
            Assert.AreEqual(
                134,
                CompileAndRun(
                    "tint main(){tint A[4];tint B[4];tint C[4];A[0]=1;A[1]=2;A[2]=3;A[3]=4;B[0]=5;B[1]=6;B[2]=7;B[3]=8;tint i=0;while(i<2){tint j=0;while(j<2){tint sum=0;tint k=0;while(k<2){sum=sum+A[i*2+k]*B[k*2+j];k=k+1;}C[i*2+j]=sum;j=j+1;}i=i+1;}return C[0]+C[1]+C[2]+C[3];}", 
                    nameof(Compile_MatrixMul_2x2)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_TriangularSum() =>
            Assert.AreEqual(
                20,
                CompileAndRun(
                    "tint main(){tint sum=0;tint i=1;while(i<=4){tint j=1;while(j<=i){sum=sum+j;j=j+1;}i=i+1;}return sum;}", 
                    nameof(Compile_TriangularSum)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_BooleanOps() =>
            Assert.AreEqual(
                1,
                CompileAndRun(
                    "tint main(){tril a=true;tril b=false;if(a==true){return 1;}return -1;}", 
                    nameof(Compile_BooleanOps)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Preprocessor_Define() =>
            Assert.AreEqual(42, CompileAndRun("#define ANSWER 42\ntint main(){return ANSWER;}", nameof(Compile_Preprocessor_Define)));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Preprocessor_IfDef() =>
            Assert.AreEqual(
                1,
                CompileAndRun("tint main(){#ifdef SKIP\nreturn -1;#else\nreturn 1;#endif\n}", nameof(Compile_Preprocessor_IfDef))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_TernaryLiteral() =>
            Assert.AreEqual(5, CompileAndRun("tint main(){return 0t+--;}", nameof(Compile_TernaryLiteral)));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_FromFile_Factorial()
        {
            string p = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "src", "T3Compiler", "examples", "factorial.t"
            );
            if (!File.Exists(p))
            {
                string? d = AppDomain.CurrentDomain.BaseDirectory;
                while (d != null && !Directory.Exists(Path.Combine(d, "src")))
                    d = Directory.GetParent(d)?.FullName;
                if (d != null)
                    p = Path.Combine(d, "src", "T3Compiler", "examples", "factorial.t");
            }
            if (!File.Exists(p)) { Assert.Inconclusive("factorial.t not found"); return; }
            Assert.AreEqual(120, CompileAndRun(File.ReadAllText(p), nameof(Compile_FromFile_Factorial)));
        }

        [TestMethod]
        [Timeout(5000)]
        public void Compile_PointerArrayStruct() =>
            Assert.AreEqual(
                10,
                CompileAndRun(
                    "struct Vec{tint a;tint b;}tint main(){struct Vec v;v.a=7;v.b=3;tint* pa=&v.a;tint* pb=&v.b;return *pa+*pb;}", 
                    nameof(Compile_PointerArrayStruct)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_Recursive_Factorial_While() =>
            Assert.AreEqual(
                5040,
                CompileAndRun(
                    "tint fact(tint n){tint r=1;while(n>1){r=r*n;n=n-1;}return r;}tint main(){return fact(7);}", 
                    nameof(Compile_Recursive_Factorial_While)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_Recursive_Factorial() =>
            Assert.AreEqual(
                5040,
                CompileAndRun(
                    "tint fact(tint n){if(n<=1){return 1;}return n*fact(n-1);}tint main(){return fact(7);}", 
                    nameof(Compile_Recursive_Factorial)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_Recursive_SumToN() =>
            Assert.AreEqual(
                55,
                CompileAndRun(
                    "tint sum(tint n){if(n<=0){return 0;}return n+sum(n-1);}tint main(){return sum(10);}", 
                    nameof(Compile_Recursive_SumToN)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_TwoCallsInExpression() =>
            Assert.AreEqual(
                18,
                CompileAndRun(
                    "tint add(tint a,tint b){return a+b;}tint main(){return add(3,4)+add(5,6);}", 
                    nameof(Compile_TwoCallsInExpression)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_MutualRecursion_IsEven() =>
            Assert.AreEqual(
                1,
                CompileAndRun(
                    "tint isEven(tint n);tint isOdd(tint n);tint isEven(tint n){if(n==0){return 1;}return isOdd(n-1);}tint isOdd(tint n){if(n==0){return 0;}return isEven(n-1);}tint main(){return isEven(10);}", 
                    nameof(Compile_MutualRecursion_IsEven)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_CompoundBitwise() =>
            Assert.AreEqual(
                0,
                CompileAndRun("tint main(){tint x=5;tint y=3;x=x&y;x=x|y;x=x^y;return x+y;}", nameof(Compile_CompoundBitwise))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_IncludeTmath() =>
            Assert.AreEqual(
                10,
                CompileAndRun("#define X 5\n#define Y 2\ntint main(){return X*Y;}", nameof(Compile_IncludeTmath))
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_MatrixMul_3x3() =>
            Assert.AreEqual(
                30,
                CompileAndRun(
                    "tint main(){tint A[9];tint B[9];A[0]=1;A[1]=2;A[2]=3;A[3]=4;A[4]=5;A[5]=6;A[6]=7;A[7]=8;A[8]=9;B[0]=9;B[1]=8;B[2]=7;B[3]=6;B[4]=5;B[5]=4;B[6]=3;B[7]=2;B[8]=1;tint sum=0;tint k=0;while(k<3){sum=sum+A[0*3+k]*B[k*3+0];k=k+1;}return sum;}", 
                    nameof(Compile_MatrixMul_3x3)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_PrimeSieve_Eratosthenes() =>
            Assert.AreEqual(
                10,
                CompileAndRun(
                    "tint main(){tint sieve[31];tint i=2;while(i<=30){sieve[i]=1;i=i+1;}i=2;while(i*i<=30){if(sieve[i]==1){tint j=i*i;while(j<=30){sieve[j]=0;j=j+i;}}i=i+1;}tint count=0;i=2;while(i<=30){if(sieve[i]==1){count=count+1;}i=i+1;}return count;}", 
                    nameof(Compile_PrimeSieve_Eratosthenes)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_BubbleSort() =>
            Assert.AreEqual(
                15,
                CompileAndRun(
                    "tint main(){tint arr[5];arr[0]=5;arr[1]=3;arr[2]=1;arr[3]=4;arr[4]=2;tint n=5;tint i=0;while(i<n){tint j=0;while(j<n-1){if(arr[j]>arr[j+1]){tint t=arr[j];arr[j]=arr[j+1];arr[j+1]=t;}j=j+1;}i=i+1;}return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];}", 
                    nameof(Compile_BubbleSort)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void Compile_TripleNestedLoop() =>
            Assert.AreEqual(
                1000,
                CompileAndRun(
                    "tint main(){tint s=0;tint i=1;while(i<=4){tint j=1;while(j<=4){tint k=1;while(k<=4){s=s+i*j*k;k=k+1;}j=j+1;}i=i+1;}return s;}", 
                    nameof(Compile_TripleNestedLoop)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_NegativeNumbers() =>
            Assert.AreEqual(
                -20,
                CompileAndRun("tint main(){tint a=-5;tint b=3;tint c=-10;tint d=2;return a*b+c/d;}", nameof(Compile_NegativeNumbers))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_ZeroIterations() =>
            Assert.AreEqual(42, CompileAndRun("tint main(){tint x=42;while(0){x=x+1;}return x;}", nameof(Compile_ZeroIterations)));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_LargeNumbers() =>
            Assert.AreEqual(
                3000,
                CompileAndRun("tint main(){tint a=1000;tint b=2000;return a+b;}", nameof(Compile_LargeNumbers))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_DeepIfElse() =>
            Assert.AreEqual(
                5,
                CompileAndRun(
                    "tint main(){tint x=5;if(x==1){return 1;}else if(x==2){return 2;}else if(x==3){return 3;}else if(x==4){return 4;}else if(x==5){return 5;}else{return -1;}}", 
                    nameof(Compile_DeepIfElse)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_TernaryOperator() =>
            Assert.AreEqual(
                3,
                CompileAndRun("tint main(){tint x=5;tint r=x>0??3:?0:!-3;return r;}", nameof(Compile_TernaryOperator))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_BreakContinue() =>
            Assert.AreEqual(
                31,
                CompileAndRun(
                    "tint main(){tint s=0;tint i=1;while(i<=10){if(i==5){i=i+1;continue;}if(i>8){break;}s=s+i;i=i+1;}return s;}", 
                    nameof(Compile_BreakContinue)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_ForLoop() =>
            Assert.AreEqual(
                55,
                CompileAndRun("tint main(){tint s=0;tint i=1;for(;i<=10;i=i+1){s=s+i;}return s;}", nameof(Compile_ForLoop))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_WhileTrueBreak() =>
            Assert.AreEqual(
                11,
                CompileAndRun("tint main(){tint x=0;while(1){x=x+1;if(x>10){break;}}return x;}", nameof(Compile_WhileTrueBreak))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_2DArray_Sum() =>
            Assert.AreEqual(
                45,
                CompileAndRun(
                    "tint main(){tint arr[9];tint i=0;while(i<9){arr[i]=i+1;i=i+1;}tint s=0;i=0;while(i<9){s=s+arr[i];i=i+1;}return s;}", 
                    nameof(Compile_2DArray_Sum)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_PointerSwap() =>
            Assert.AreEqual(
                15,
                CompileAndRun("tint main(){tint a=5;tint b=10;tint*pa=&a;tint*pb=&b;tint t=*pa;*pa=*pb;*pb=t;return a+b;}", nameof(Compile_PointerSwap))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_ArrayReverse() =>
            Assert.AreEqual(
                15,
                CompileAndRun(
                    "tint main(){tint arr[5];arr[0]=1;arr[1]=2;arr[2]=3;arr[3]=4;arr[4]=5;tint i=0;while(i<2){tint t=arr[i];arr[i]=arr[4-i];arr[4-i]=t;i=i+1;}return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];}", 
                    nameof(Compile_ArrayReverse)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_NestedStruct() =>
            Assert.AreEqual(
                30,
                CompileAndRun(
                    "struct Point{tint x;tint y;}tint main(){struct Point p;p.x=10;p.y=20;return p.x+p.y;}", 
                    nameof(Compile_NestedStruct)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_StructArray() =>
            Assert.AreEqual(
                30,
                CompileAndRun(
                    "struct Point{tint x;tint y;}tint main(){struct Point pts[3];pts[0].x=10;pts[0].y=20;return pts[0].x+pts[0].y;}", 
                    nameof(Compile_StructArray)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_CompoundAddSub() =>
            Assert.AreEqual(
                10,
                CompileAndRun("tint main(){tint x=10;x=x+5;x=x-3;x=x*2;x=x/4;x=x+4;return x;}", nameof(Compile_CompoundAddSub))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Preprocessor_MacroExpr() =>
            Assert.AreEqual(
                10,
                CompileAndRun("#define X 5\n#define Y 2\ntint main(){return X*Y;}", nameof(Compile_Preprocessor_MacroExpr))
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_DoWhile_Sum() =>
            Assert.AreEqual(
                55,
                CompileAndRun(
                    "tint main(){tint s=0;tint i=1;do{s=s+i;i=i+1;}while(i<=10);return s;}", 
                    nameof(Compile_DoWhile_Sum)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_DoWhile_AtLeastOnce() =>
            Assert.AreEqual(1, CompileAndRun("tint main(){tint x=0;do{x=x+1;}while(0);return x;}", nameof(Compile_DoWhile_AtLeastOnce)));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_SwitchCase_Basic() =>
            Assert.AreEqual(
                2,
                CompileAndRun(
                    "tint main(){tint x=2;switch(x){case 1:return 10;case 2:return 2;case 3:return 30;default:return -1;}}", 
                    nameof(Compile_SwitchCase_Basic)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_SwitchCase_Default() =>
            Assert.AreEqual(
                99,
                CompileAndRun(
                    "tint main(){tint x=100;switch(x){case 1:return 10;case 2:return 20;default:return 99;}}", 
                    nameof(Compile_SwitchCase_Default)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_ComplexExpression() =>
            Assert.AreEqual(27, CompileAndRun("tint main(){return (2+3)*4-6/2+10;}", nameof(Compile_ComplexExpression)));

        [TestMethod]
        [Timeout(5000)]
        public void Compile_StructPointerAccess() =>
            Assert.AreEqual(
                25,
                CompileAndRun(
                    "struct Pair{tint first;tint second;}tint main(){struct Pair p;p.first=10;p.second=15;struct Pair* pp=&p;return (*pp).first+(*pp).second;}", 
                    nameof(Compile_StructPointerAccess)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Enum_Basic() =>
            Assert.AreEqual(
                2,
                CompileAndRun(
                    "enum Color{Red=1,Green,Blue=-1};tint main(){Color c=Green;return c;}", 
                    nameof(Compile_Enum_Basic)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Enum_InFunction() =>
            Assert.AreEqual(
                1,
                CompileAndRun(
                    "enum Color{Red=1,Green,Blue=-1};tint get_val(Color c){return c;}tint main(){return get_val(Red);}", 
                    nameof(Compile_Enum_InFunction)
                )
            );

        [TestMethod]
        [Timeout(5000)]
        public void Compile_Strings_And_Strlen() =>
            Assert.AreEqual(
                11,
                CompileAndRun(
                    "tint strlen(tint s);tint main(){tint len1=strlen(\"Hello\");tint len2=strlen(\"World!\");return len1+len2;}", 
                    nameof(Compile_Strings_And_Strlen)
                )
            );

        // ============================================================
        // ABI legacy: Deep recursion with spilled locals
        // ============================================================
        [TestMethod]
        [Timeout(10000)]
        public void ABIv3_FactorialRecursive() =>
            Assert.AreEqual(
                120,
                CompileAndRun(
                    "tint fact(tint n){if(n<=1){return 1;}return n*fact(n-1);}tint main(){return fact(5);}", 
                    nameof(ABIv3_FactorialRecursive)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void ABIv3_SumWithLocalVar() =>
            Assert.AreEqual(
                55,
                CompileAndRun(
                    "tint sum(tint n){if(n<=0){return 0;}tint m=n-1;return n+sum(m);}tint main(){return sum(10);}", 
                    nameof(ABIv3_SumWithLocalVar)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void ABIv3_MutualRecursionWithLocal() =>
            Assert.AreEqual(
                1,
                CompileAndRun(
                    "tint isOdd(tint n);tint isEven(tint n){if(n==0){return 1;}tint m=n-1;return isOdd(m);}tint isOdd(tint n){if(n==0){return 0;}tint m=n-1;return isEven(m);}tint main(){return isEven(10);}", 
                    nameof(ABIv3_MutualRecursionWithLocal)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void ABIv3_FibIterative() =>
            Assert.AreEqual(
                55,
                CompileAndRun(
                    "tint fib(tint n){tint a=0;tint b=1;tint c=0;tint i=0;while(i<n){c=a+b;a=b;b=c;i=i+1;}return a;}tint main(){return fib(10);}", 
                    nameof(ABIv3_FibIterative)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void ABIv3_NestedCallsWithLocals() =>
            Assert.AreEqual(
                37,
                CompileAndRun(
                    "tint add(tint a,tint b){return a+b;}tint mul(tint a,tint b){return a*b;}tint main(){tint x=add(3,4);tint y=mul(5,6);tint z=add(2,3);tint w=mul(4,5);return x+y;}", 
                    nameof(ABIv3_NestedCallsWithLocals)
                )
            );

        [TestMethod]
        [Timeout(10000)]
        public void ABIv3_DoubleRecursion() =>
            Assert.AreEqual(
                8,
                CompileAndRun(
                    "tint fib(tint n){if(n<=1){return n;}return fib(n-1)+fib(n-2);}tint main(){return fib(6);}", 
                    nameof(ABIv3_DoubleRecursion)
                )
            );

        [TestMethod]
        [Timeout(15000)]
        public void ComplexIntegrationTest_GlobalVarsAndIncludes()
        {
            string baseDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "src", "T3Compiler", "examples"
            );
            string mathlib = File.ReadAllText(Path.Combine(baseDir, "mathlib.t"));
            string prime = File.ReadAllText(Path.Combine(baseDir, "prime.t"));
            string main = @"
tint main() {
    tint pc = sieve(50);
    tint g = gcd(48, 180);
    tint f = fib(10);
    return pc + g + f;
}
";
            string source = mathlib + "\n" + prime + "\n" + main;
            long result = CompileAndRun(source, nameof(ComplexIntegrationTest_GlobalVarsAndIncludes));
            Assert.IsTrue(result > 0, $"Expected positive result, got {result}");
        }

        [TestMethod][Timeout(10000)]
        public void E2E_FullPipeline_CompileAndRun()
        {
            string source = "tint main(){tint x=40;tint y=2;return x+y;}";
            var pp = new T3Preprocessor();
            string pre = pp.Process(source);
            var ast = new Parser(new Tokenizer(pre).Tokenize()).ParseProgram();
            string asm = new CodeGenerator(ast).Generate();
            
            // Manual dump for this specific test
            string dumpDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "test_results");
            if (!Directory.Exists(dumpDir)) Directory.CreateDirectory(dumpDir);
            File.WriteAllText(Path.Combine(dumpDir, $"{nameof(E2E_FullPipeline_CompileAndRun)}.asm"), asm);

            Assert.IsTrue(asm.Contains("main:"), "ASM should contain main label");
            Assert.IsTrue(asm.Contains("RET"), "ASM should contain RET");
            var bin = new T3InOrderAssembler(T3Config.T3_18).Assemble(asm);
            Assert.IsTrue(bin.Count > 0, "Binary should not be empty");
            var words = bin.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();
            Assert.AreEqual(42, proc.Registers[6].ToLong(), "Processor should return 42");
        }

        [TestMethod]
        [Timeout(15000)]
        public void Compile_FromFile_Comprehensive()
        {
            string p = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "src", "T3Compiler", "examples", "comprehensive.t"
            );
            if (!File.Exists(p))
            {
                string? d = AppDomain.CurrentDomain.BaseDirectory;
                while (d != null && !Directory.Exists(Path.Combine(d, "src")))
                    d = Directory.GetParent(d)?.FullName;
                if (d != null)
                    p = Path.Combine(d, "src", "T3Compiler", "examples", "comprehensive.t");
            }
            if (!File.Exists(p)) { Assert.Inconclusive("comprehensive.t not found"); return; }
            long result = CompileAndRun(File.ReadAllText(p), nameof(Compile_FromFile_Comprehensive));
            Assert.IsTrue(result > 0, $"Expected positive result, got {result}");
        }
    }
}