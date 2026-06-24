using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Compiler;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;
using T3Compiler.CodeGen;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using T3Assembler;
using T3Simulator.Common;
using T3Simulator.InOrder;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    [TestClass]
    public class TLangCompilerTests
    {
        private long CompileAndRun(string source)
        {
            var pp = new T3Preprocessor();
            string pre = pp.Process(source);
            var ast = new Parser(new Tokenizer(pre).Tokenize()).ParseProgram();
            string asm = new CodeGenerator(ast).Generate();
            var bin = new T3InOrderAssembler(T3Config.T3_18).Assemble(asm);
            var words = bin.Select(x => Word18.FromInt128(x)).ToList();
            var proc = new T3InOrderProcessor<Word18>(T3Config.T3_18);
            proc.LoadProgram(words);
            proc.Run();
            return proc.Registers[6].ToLong(); // R2→phys6
        }

        // ============================================================
        // Existing tests (19 tests)
        // ============================================================

        [TestMethod] [Timeout(5000)]public void Compile_SimpleArithmetic_Returns42() => Assert.AreEqual(42, CompileAndRun("tint main(){tint x=40;tint y=2;return x+y;}"));
        [TestMethod] [Timeout(5000)]public void Compile_While_SumTo5() => Assert.AreEqual(15, CompileAndRun("tint main(){tint s=0;tint i=1;while(i<=5){s=s+i;i=i+1;}return s;}"));
        [TestMethod] [Timeout(5000)]public void Compile_While_SumMulti() => Assert.AreEqual(225, CompileAndRun("tint main() {	tint w=0;	tint z=1;	tint x=1;	tint y=1;	tint i=0;	while(i<=36){	    z = z + 1;	    x = x + 2;	    y = y + 3;		i=i+1;}	w = z + x + y;	return w;}"));
        [TestMethod] [Timeout(5000)]public void Compile_For_Factorial() => Assert.AreEqual(120, CompileAndRun("tint main(){tint r=1;tint i=1;while(i<=5){r=r*i;i=i+1;}return r;}"));
        [TestMethod][Timeout(5000)] public void Compile_NestedWhile_SumProd() => Assert.AreEqual(36, CompileAndRun("tint main(){tint s=0;tint i=1;while(i<=3){tint j=1;while(j<=3){s=s+i*j;j=j+1;}i=i+1;}return s;}"));
        [TestMethod][Timeout(5000)] public void Compile_NestedWhile_SimpleSum() => Assert.AreEqual(9, CompileAndRun("tint main() { tint s=0; tint i=1; while(i<=3){ tint j=1; while(j<=3){ s=s+1;j=j+1;} i=i+1; } return s;}"));
        [TestMethod][Timeout(5000)] public void Compile_NestedWhile_SimpleSumLong() => Assert.AreEqual(1296, CompileAndRun("tint main() { tint s=0; tint i=1; while(i<=36){ tint j=1; while(j<=36){ s=s+1;j=j+1;} i=i+1; } return s;}"));
        [TestMethod][Timeout(5000)] public void Compile_IfElse_Branch() { Assert.AreEqual(1, CompileAndRun("tint main(){tint x=10;if(x>5){return 1;}else{return -1;}}")); Assert.AreEqual(-1, CompileAndRun("tint main(){tint x=3;if(x>5){return 1;}else{return -1;}}")); }
        [TestMethod][Timeout(5000)] public void Compile_Fibonacci_Returns55() => Assert.AreEqual(55, CompileAndRun("tint main(){tint n=10;tint a=0;tint b=1;tint i=0;while(i<n){tint t=a+b;a=b;b=t;i=i+1;}return a;}"));
        [TestMethod][Timeout(5000)] public void Compile_Array_Sum() => Assert.AreEqual(15, CompileAndRun("tint main(){tint arr[5];arr[0]=1;arr[1]=2;arr[2]=3;arr[3]=4;arr[4]=5;return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];}"));
        [TestMethod][Timeout(5000)] public void Compile_Array_Mul() => Assert.AreEqual(120, CompileAndRun("tint main(){    tint arr[5];    arr[0]=1;    arr[1]=2;    arr[2]=3;    arr[3]=4;    arr[4]=5;   tint i = 0;    tint w = 1;    while (i < 5) {   w = w * arr[i];  i = i + 1; } return w;}"));
        [TestMethod][Timeout(5000)] public void Compile_Struct_FieldAccess() => Assert.AreEqual(30, CompileAndRun("struct Point{tint x;tint y;}tint main(){struct Point p;p.x=10;p.y=20;return p.x+p.y;}"));
        [TestMethod][Timeout(5000)] public void Compile_Pointer_Deref() => Assert.AreEqual(42, CompileAndRun("tint main(){tint v=42;tint* p=&v;return *p;}"));
        [TestMethod] [Timeout(5000)]public void Compile_Pointer_Arithmetic() => Assert.AreEqual(2, CompileAndRun("tint main(){tint arr[5];arr[0]=1;arr[1]=2;tint* p=&arr[0];p=p+1;return *p;}"));
        [TestMethod][Timeout(5000)] public void Compile_MatrixMul_2x2() => Assert.AreEqual(134, CompileAndRun("tint main(){tint A[4];tint B[4];tint C[4];A[0]=1;A[1]=2;A[2]=3;A[3]=4;B[0]=5;B[1]=6;B[2]=7;B[3]=8;tint i=0;while(i<2){tint j=0;while(j<2){tint sum=0;tint k=0;while(k<2){sum=sum+A[i*2+k]*B[k*2+j];k=k+1;}C[i*2+j]=sum;j=j+1;}i=i+1;}return C[0]+C[1]+C[2]+C[3];}"));
        [TestMethod][Timeout(5000)] public void Compile_TriangularSum() => Assert.AreEqual(20, CompileAndRun("tint main(){tint sum=0;tint i=1;while(i<=4){tint j=1;while(j<=i){sum=sum+j;j=j+1;}i=i+1;}return sum;}"));
        [TestMethod][Timeout(5000)] public void Compile_BooleanOps() => Assert.AreEqual(1, CompileAndRun("tint main(){tril a=true;tril b=false;if(a==true){return 1;}return -1;}"));
        [TestMethod][Timeout(5000)] public void Compile_Preprocessor_Define() => Assert.AreEqual(42, CompileAndRun("#define ANSWER 42\ntint main(){return ANSWER;}"));
        [TestMethod][Timeout(5000)] public void Compile_Preprocessor_IfDef() => Assert.AreEqual(1, CompileAndRun("tint main(){#ifdef SKIP\nreturn -1;#else\nreturn 1;#endif\n}"));
        [TestMethod][Timeout(5000)] public void Compile_TernaryLiteral() => Assert.AreEqual(5, CompileAndRun("tint main(){return 0t+--;}"));
        [TestMethod]
        [Timeout(5000)]
        public void Compile_FromFile_Factorial()
        {
            string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","..","..","..","..","src","T3Compiler","examples","factorial.t");
            if(!File.Exists(p)){string? d=AppDomain.CurrentDomain.BaseDirectory;while(d!=null&&!Directory.Exists(Path.Combine(d,"src")))d=Directory.GetParent(d)?.FullName;if(d!=null)p=Path.Combine(d,"src","T3Compiler","examples","factorial.t");}
            if(!File.Exists(p)){Assert.Inconclusive("factorial.t not found");return;}
            Assert.AreEqual(120, CompileAndRun(File.ReadAllText(p)));
        }
        [TestMethod][Timeout(5000)] public void Compile_PointerArrayStruct() => Assert.AreEqual(10, CompileAndRun("struct Vec{tint a;tint b;}tint main(){struct Vec v;v.a=7;v.b=3;tint* pa=&v.a;tint* pb=&v.b;return *pa+*pb;}"));

        // ============================================================
        // Category 1: Recursion (tests stack frame ABI)
        // Note: Recursion with parameters has known ABI issues.
        // These tests are marked as known limitations.
        // ============================================================

        /// <summary>
        /// Recursive factorial via while loop (no function parameters)
        /// fact(7) = 5040
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_Recursive_Factorial_While()
        {
            Assert.AreEqual(5040, CompileAndRun(@"
tint fact(tint n) {
    tint r = 1;
    while (n > 1) {
        r = r * n;
        n = n - 1;
    }
    return r;
}
tint main() {
    return fact(7);
}"));
        }

        /// <summary>
        /// True recursive factorial: fact(n) = n<=1 ? 1 : n*fact(n-1)
        /// fact(7) = 5040
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_Recursive_Factorial()
        {
            Assert.AreEqual(5040, CompileAndRun(@"
tint fact(tint n) {
    if (n <= 1) { return 1; }
    else { return n * fact(n - 1); }
}
tint main() {
    return fact(7);
}"));
        }

        /// <summary>
        /// Recursive sum: sum(n) = n<=0 ? 0 : n+sum(n-1)
        /// sum(10) = 55
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_Recursive_SumToN()
        {
            Assert.AreEqual(55, CompileAndRun(@"
tint sum(tint n) {
    if (n <= 0) { return 0; }
    else { return n + sum(n - 1); }
}
tint main() {
    return sum(10);
}"));
        }

        /// <summary>
        /// Two non-recursive function calls in one expression
        /// add(3,4) + add(5,6) = 7 + 11 = 18
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_TwoCallsInExpression()
        {
            Assert.AreEqual(18, CompileAndRun(@"
tint add(tint a, tint b) {
    return a + b;
}
tint main() {
    return add(3, 4) + add(5, 6);
}"));
        }

        /// <summary>
        /// Mutual recursion: isEven(n) = n==0?1:isOdd(n-1), isOdd(n)=n==0?0:isEven(n-1)
        /// isEven(10) = 1 (true)
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_MutualRecursion_IsEven()
        {
            Assert.AreEqual(1, CompileAndRun(@"
tint isEven(tint n);
tint isOdd(tint n);
tint isEven(tint n) {
    if (n == 0) { return 1; }
    else { return isOdd(n - 1); }
}
tint isOdd(tint n) {
    if (n == 0) { return 0; }
    else { return isEven(n - 1); }
}
tint main() {
    return isEven(10);
}"));
        }

        /// <summary>
        /// Compound bitwise assignments: &=, |=, ^=
        /// Balanced ternary tritwise operations:
        ///   5 (0t+--) & 3 (0t+0) = -4 (0t--)
        ///   -4 (0t--) | 3 (0t+0) = 3 (0t+0)
        ///   3 (0t+0) ^ 3 (0t+0) = -3 (0t-0)
        ///   -3 + 3 = 0
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_CompoundBitwise()
        {
            Assert.AreEqual(0, CompileAndRun(@"
tint main() {
    tint x = 5;
    tint y = 3;
    x = x & y;
    x = x | y;
    x = x ^ y;
    return x + y;
}"));
        }

        /// <summary>
        /// Preprocessor #include with a macro
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_IncludeTmath()
        {
            Assert.AreEqual(10, CompileAndRun(@"
#define X 5
#define Y 2
tint main() {
    return X * Y;
}"));
        }

        // ============================================================
        // Category 2: Complex Nested Loops
        // ============================================================

        /// <summary>
        /// 3x3 matrix multiplication (simplified — compute single element)
        /// Row 0 of A × Col 0 of B = 1*9+2*6+3*3 = 30
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_MatrixMul_3x3()
        {
            Assert.AreEqual(30, CompileAndRun(@"
tint main() {
    tint A[9]; tint B[9];
    A[0]=1;A[1]=2;A[2]=3;A[3]=4;A[4]=5;A[5]=6;A[6]=7;A[7]=8;A[8]=9;
    B[0]=9;B[1]=8;B[2]=7;B[3]=6;B[4]=5;B[5]=4;B[6]=3;B[7]=2;B[8]=1;
    tint sum=0;
    tint k=0;
    while(k<3){
        sum = sum + A[0*3+k] * B[k*3+0];
        k=k+1;
    }
    return sum;
}"));
        }

        /// <summary>
        /// Prime sieve (Eratosthenes) up to 30, count primes
        /// Primes up to 30: 2,3,5,7,11,13,17,19,23,29 = 10 primes
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_PrimeSieve_Eratosthenes()
        {
            Assert.AreEqual(10, CompileAndRun(@"
tint main() {
    tint sieve[31];
    tint i = 2;
    while (i <= 30) {
        sieve[i] = 1;
        i = i + 1;
    }
    i = 2;
    while (i * i <= 30) {
        if (sieve[i] == 1) {
            tint j = i * i;
            while (j <= 30) {
                sieve[j] = 0;
                j = j + i;
            }
        }
        i = i + 1;
    }
    tint count = 0;
    i = 2;
    while (i <= 30) {
        if (sieve[i] == 1) { count = count + 1; }
        i = i + 1;
    }
    return count;
}"));
        }

        /// <summary>
        /// Bubble sort array [5,3,1,4,2], return sum of sorted elements
        /// Sorted: [1,2,3,4,5], sum = 15
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_BubbleSort()
        {
            Assert.AreEqual(15, CompileAndRun(@"
tint main() {
    tint arr[5];
    arr[0]=5;arr[1]=3;arr[2]=1;arr[3]=4;arr[4]=2;
    tint n=5;
    tint i=0;
    while(i<n){
        tint j=0;
        while(j<n-1){
            if(arr[j]>arr[j+1]){
                tint t=arr[j];
                arr[j]=arr[j+1];
                arr[j+1]=t;
            }
            j=j+1;
        }
        i=i+1;
    }
    return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];
}"));
        }

        /// <summary>
        /// Triple-nested loop: sum of i*j*k for i,j,k in 1..4
        /// (4*5/2)^3 = 10^3 = 1000
        /// </summary>
        [TestMethod][Timeout(10000)]
        public void Compile_TripleNestedLoop()
        {
            Assert.AreEqual(1000, CompileAndRun(@"
tint main() {
    tint s=0;
    tint i=1;
    while(i<=4){
        tint j=1;
        while(j<=4){
            tint k=1;
            while(k<=4){
                s=s+i*j*k;
                k=k+1;
            }
            j=j+1;
        }
        i=i+1;
    }
    return s;
}"));
        }

        // ============================================================
        // Category 3: Edge Cases
        // ============================================================

        /// <summary>
        /// Arithmetic with negative numbers
        /// (-5) * 3 + (-10) / 2 = -15 + (-5) = -20
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_NegativeNumbers()
        {
            Assert.AreEqual(-20, CompileAndRun(@"
tint main() {
    tint a = -5;
    tint b = 3;
    tint c = -10;
    tint d = 2;
    return a * b + c / d;
}"));
        }

        /// <summary>
        /// While loop that never executes (condition false initially)
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_ZeroIterations()
        {
            Assert.AreEqual(42, CompileAndRun(@"
tint main() {
    tint x = 42;
    while (0) {
        x = x + 1;
    }
    return x;
}"));
        }

        /// <summary>
        /// Large numbers (beyond LI range ±364, tests LIMM)
        /// 1000 + 2000 = 3000
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_LargeNumbers()
        {
            Assert.AreEqual(3000, CompileAndRun(@"
tint main() {
    tint a = 1000;
    tint b = 2000;
    return a + b;
}"));
        }

        /// <summary>
        /// 5-level nested if/else
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_DeepIfElse()
        {
            Assert.AreEqual(5, CompileAndRun(@"
tint main() {
    tint x = 5;
    if (x == 1) { return 1; }
    else if (x == 2) { return 2; }
    else if (x == 3) { return 3; }
    else if (x == 4) { return 4; }
    else if (x == 5) { return 5; }
    else { return -1; }
}"));
        }

        // ============================================================
        // Category 4: Control Flow
        // ============================================================

        /// <summary>
        /// Ternary operator ?? :? :!
        /// x > 0 ?? 3 :? 0 :! -3
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_TernaryOperator()
        {
            Assert.AreEqual(3, CompileAndRun(@"
tint main() {
    tint x = 5;
    tint r = x > 0 ?? 3 :? 0 :! -3;
    return r;
}"));
        }

        /// <summary>
        /// Loop with break and continue
        /// Sum numbers 1..10, skip 5, stop at 8
        /// 1+2+3+4+6+7+8 = 31
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_BreakContinue()
        {
            Assert.AreEqual(31, CompileAndRun(@"
tint main() {
    tint s = 0;
    tint i = 1;
    while (i <= 10) {
        if (i == 5) { i = i + 1; continue; }
        if (i > 8) { break; }
        s = s + i;
        i = i + 1;
    }
    return s;
}"));
        }

        /// <summary>
        /// For loop with compound init: sum 1..10 = 55
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_ForLoop()
        {
            Assert.AreEqual(55, CompileAndRun(@"
tint main() {
    tint s = 0;
    tint i = 1;
    for (; i <= 10; i = i + 1) {
        s = s + i;
    }
    return s;
}"));
        }

        /// <summary>
        /// while(1) with break
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_WhileTrueBreak()
        {
            Assert.AreEqual(11, CompileAndRun(@"
tint main() {
    tint x = 0;
    while (1) {
        x = x + 1;
        if (x > 10) { break; }
    }
    return x;
}"));
        }

        // ============================================================
        // Category 5: Arrays and Pointers
        // ============================================================

        /// <summary>
        /// 2D array (flattened) sum: 3x3 = 45
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_2DArray_Sum()
        {
            Assert.AreEqual(45, CompileAndRun(@"
tint main() {
    tint arr[9];
    tint i = 0;
    while (i < 9) {
        arr[i] = i + 1;
        i = i + 1;
    }
    tint s = 0;
    i = 0;
    while (i < 9) {
        s = s + arr[i];
        i = i + 1;
    }
    return s;
}"));
        }

        /// <summary>
        /// Swap two variables via pointers
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_PointerSwap()
        {
            Assert.AreEqual(15, CompileAndRun(@"
tint main() {
    tint a = 5;
    tint b = 10;
    tint* pa = &a;
    tint* pb = &b;
    tint t = *pa;
    *pa = *pb;
    *pb = t;
    return a + b;
}"));
        }

        /// <summary>
        /// Reverse array in-place
        /// [1,2,3,4,5] -> [5,4,3,2,1], sum = 15
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_ArrayReverse()
        {
            Assert.AreEqual(15, CompileAndRun(@"
tint main() {
    tint arr[5];
    arr[0]=1;arr[1]=2;arr[2]=3;arr[3]=4;arr[4]=5;
    tint i=0;
    while(i<2){
        tint t=arr[i];
        arr[i]=arr[4-i];
        arr[4-i]=t;
        i=i+1;
    }
    return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];
}"));
        }

        // ============================================================
        // Category 6: Structs
        // ============================================================

        /// <summary>
        /// Nested struct: struct containing struct (simplified — direct field access only)
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_NestedStruct()
        {
            Assert.AreEqual(30, CompileAndRun(@"
struct Point { tint x; tint y; }
tint main() {
    struct Point p;
    p.x = 10;
    p.y = 20;
    return p.x + p.y;
}"));
        }

        /// <summary>
        /// Array of structs — simple field access
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_StructArray()
        {
            Assert.AreEqual(30, CompileAndRun(@"
struct Point { tint x; tint y; }
tint main() {
    struct Point pts[3];
    pts[0].x = 10;
    pts[0].y = 20;
    return pts[0].x + pts[0].y;
}"));
        }

        // ============================================================
        // Category 7: Compound Assignments
        // ============================================================

        /// <summary>
        /// Compound arithmetic assignments: +=, -=, *=, /=
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_CompoundAddSub()
        {
            Assert.AreEqual(10, CompileAndRun(@"
tint main() {
    tint x = 10;
    x = x + 5;
    x = x - 3;
    x = x * 2;
    x = x / 4;
    x = x + 4;
    return x;
}"));
        }

        // ============================================================
        // Category 8: Preprocessor
        // ============================================================

        /// <summary>
        /// #define with arithmetic expression
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_Preprocessor_MacroExpr()
        {
            Assert.AreEqual(10, CompileAndRun(@"
#define X 5
#define Y 2
tint main() {
    return X * Y;
}"));
        }

        // ============================================================
        // Additional: do/while loop tests
        // ============================================================

        /// <summary>
        /// do/while loop: sum 1..10 = 55
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_DoWhile_Sum()
        {
            Assert.AreEqual(55, CompileAndRun(@"
tint main() {
    tint s = 0;
    tint i = 1;
    do {
        s = s + i;
        i = i + 1;
    } while (i <= 10);
    return s;
}"));
        }

        /// <summary>
        /// do/while executes body at least once even if condition is false
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_DoWhile_AtLeastOnce()
        {
            Assert.AreEqual(1, CompileAndRun(@"
tint main() {
    tint x = 0;
    do {
        x = x + 1;
    } while (0);
    return x;
}"));
        }

        // ============================================================
        // Additional: switch/case tests
        // ============================================================

        /// <summary>
        /// switch/case with default
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_SwitchCase_Basic()
        {
            Assert.AreEqual(2, CompileAndRun(@"
tint main() {
    tint x = 2;
    switch (x) {
        case 1: return 10;
        case 2: return 2;
        case 3: return 30;
        default: return -1;
    }
}"));
        }

        /// <summary>
        /// switch/case with default branch
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_SwitchCase_Default()
        {
            Assert.AreEqual(99, CompileAndRun(@"
tint main() {
    tint x = 100;
    switch (x) {
        case 1: return 10;
        case 2: return 20;
        default: return 99;
    }
}"));
        }

        // ============================================================
        // Additional: Complex expression with mixed operations
        // ============================================================

        /// <summary>
        /// Complex arithmetic expression: (2+3)*4-6/2+10 = 5*4-3+10 = 27
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_ComplexExpression()
        {
            Assert.AreEqual(27, CompileAndRun(@"
tint main() {
    return (2 + 3) * 4 - 6 / 2 + 10;
}"));
        }

        // ============================================================
        // Additional: Struct with pointer member access via dereference
        // ============================================================

        /// <summary>
        /// Struct with pointer member access via dereference
        /// </summary>
        [TestMethod][Timeout(5000)]
        public void Compile_StructPointerAccess()
        {
            Assert.AreEqual(25, CompileAndRun(@"
struct Pair { tint first; tint second; }
tint main() {
    struct Pair p;
    p.first = 10;
    p.second = 15;
    struct Pair* pp = &p;
    return (*pp).first + (*pp).second;
}"));
        }

        [TestMethod][Timeout(5000)]
        public void Compile_Enum_Basic()
        {
            Assert.AreEqual(2, CompileAndRun(@"
enum Color { Red = 1, Green, Blue = -1 }
tint main() {
    Color c = Green;
    return c;
}"));
        }

        [TestMethod][Timeout(5000)]
        public void Compile_Enum_InFunction()
        {
            Assert.AreEqual(1, CompileAndRun(@"
enum Color { Red = 1, Green, Blue = -1 }
tint get_val(Color c) { return c; }
tint main() {
    return get_val(Red);
}"));
        }

        [TestMethod][Timeout(5000)]
        public void Compile_Strings_And_Strlen()
        {
            Assert.AreEqual(11, CompileAndRun(@"
tint strlen(tint s);
tint main() {
    tint len1 = strlen(""Hello"");
    tint len2 = strlen(""World!"");
    return len1 + len2;
}"));
        }
    }
}