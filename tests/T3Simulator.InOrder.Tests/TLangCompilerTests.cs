using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Compiler;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;
using T3Compiler.CodeGen;
using System;
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

        [TestMethod] [Timeout(5000)]public void Compile_SimpleArithmetic_Returns42() => Assert.AreEqual(42, CompileAndRun("tint main(){tint x=40;tint y=2;return x+y;}"));
        [TestMethod] [Timeout(5000)]public void Compile_While_SumTo5() => Assert.AreEqual(15, CompileAndRun("tint main(){tint s=0;tint i=1;while(i<=5){s=s+i;i=i+1;}return s;}"));
        [TestMethod] [Timeout(5000)]public void Compile_For_Factorial() => Assert.AreEqual(120, CompileAndRun("tint main(){tint r=1;tint i=1;while(i<=5){r=r*i;i=i+1;}return r;}"));
        [TestMethod][Timeout(5000)] public void Compile_NestedWhile_SumProd() => Assert.AreEqual(36, CompileAndRun("tint main(){tint s=0;tint i=1;while(i<=3){tint j=1;while(j<=3){s=s+i*j;j=j+1;}i=i+1;}return s;}"));
        [TestMethod][Timeout(5000)] public void Compile_IfElse_Branch() { Assert.AreEqual(1, CompileAndRun("tint main(){tint x=10;if(x>5){return 1;}else{return -1;}}")); Assert.AreEqual(-1, CompileAndRun("tint main(){tint x=3;if(x>5){return 1;}else{return -1;}}")); }
        [TestMethod][Timeout(5000)] public void Compile_Fibonacci_Returns55() => Assert.AreEqual(55, CompileAndRun("tint main(){tint n=10;tint a=0;tint b=1;tint i=0;while(i<n){tint t=a+b;a=b;b=t;i=i+1;}return a;}"));
        [TestMethod][Timeout(5000)] public void Compile_Array_Sum() => Assert.AreEqual(15, CompileAndRun("tint main(){tint arr[5];arr[0]=1;arr[1]=2;arr[2]=3;arr[3]=4;arr[4]=5;return arr[0]+arr[1]+arr[2]+arr[3]+arr[4];}"));
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
    }
}