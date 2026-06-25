using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Compiler;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;

namespace T3Interpreter.Tests
{
    [TestClass]
    public class Tests
    {
        static long I(string s) { var p = new T3Preprocessor().Process(s); var a = new Parser(new Tokenizer(p).Tokenize()).ParseProgram(); return new global::T3Interpreter.T3Interpreter(a).Run(); }
        [TestMethod] public void Add() => Assert.AreEqual(42, I("tint main(){return 40+2;}"));
        [TestMethod] public void WhileSum() => Assert.AreEqual(15, I("tint main(){tint s=0;tint i=1;while(i<=5){s=s+i;i=i+1;}return s;}"));
        [TestMethod] public void RecursiveFact() => Assert.AreEqual(5040, I("tint fact(tint n){if(n<=1){return 1;}return n*fact(n-1);}tint main(){return fact(7);}"));
        [TestMethod] public void NestedWhile() => Assert.AreEqual(9, I("tint main(){tint s=0;tint i=1;while(i<=3){tint j=1;while(j<=3){s=s+1;j=j+1;}i=i+1;}return s;}"));
        [TestMethod] public void IfElseTrue() => Assert.AreEqual(1, I("tint main(){tint x=10;if(x>5){return 1;}else{return -1;}}"));
        [TestMethod] public void IfElseFalse() => Assert.AreEqual(-1, I("tint main(){tint x=3;if(x>5){return 1;}else{return -1;}}"));
        [TestMethod] public void Fibonacci() => Assert.AreEqual(55, I("tint main(){tint n=10;tint a=0;tint b=1;tint i=0;while(i<n){tint t=a+b;a=b;b=t;i=i+1;}return a;}"));
        [TestMethod] public void NegNumbers() => Assert.AreEqual(-20, I("tint main(){tint a=-5;tint b=3;tint c=-10;tint d=2;return a*b+c/d;}"));
        [TestMethod] public void PreprocDefine() => Assert.AreEqual(42, I("#define ANSWER 42\ntint main(){return ANSWER;}"));
        [TestMethod] public void BreakContinue() => Assert.AreEqual(31, I("tint main(){tint s=0;tint i=1;while(i<=10){if(i==5){i=i+1;continue;}if(i>8){break;}s=s+i;i=i+1;}return s;}"));
        [TestMethod] public void ForLoop() => Assert.AreEqual(55, I("tint main(){tint s=0;tint i=1;for(;i<=10;i=i+1){s=s+i;}return s;}"));
        [TestMethod] public void DoWhile() => Assert.AreEqual(55, I("tint main(){tint s=0;tint i=1;do{s=s+i;i=i+1;}while(i<=10);return s;}"));
        [TestMethod] public void SwitchCase() => Assert.AreEqual(2, I("tint main(){tint x=2;switch(x){case 1:return 10;case 2:return 2;default:return -1;}}"));
        [TestMethod] public void ComplexExpr() => Assert.AreEqual(27, I("tint main(){return (2+3)*4-6/2+10;}"));
        [TestMethod] public void DoubleRecursion() => Assert.AreEqual(8, I("tint fib(tint n){if(n<=1){return n;}return fib(n-1)+fib(n-2);}tint main(){return fib(6);}"));
        // Literal format tests
        [TestMethod] public void TernaryLiteral() => Assert.AreEqual(5, I("tint main(){return 0t+--;}"));
        [TestMethod] public void TernaryLiteral11() => Assert.AreEqual(11, I("tint main(){return 0t++-;}"));
        [TestMethod] public void TernaryLiteralMinus40() => Assert.AreEqual(-40, I("tint main(){return 0t----;}"));
        [TestMethod] public void Base9Literal() => Assert.AreEqual(4, I("tint main(){return 0n4;}"));
        [TestMethod] public void Base9LiteralNeg4() => Assert.AreEqual(-4, I("tint main(){return 0nW;}"));
        [TestMethod] public void Base27Literal0() => Assert.AreEqual(0, I("tint main(){return 0y0;}"));
        [TestMethod] public void Base27LiteralNeg1() => Assert.AreEqual(-1, I("tint main(){return 0yZ;}"));
        [TestMethod] public void DecimalLiteral() => Assert.AreEqual(42, I("tint main(){return 42;}"));
        [TestMethod] public void DecimalLiteralNeg() => Assert.AreEqual(-255, I("tint main(){return -255;}"));
    }
}
