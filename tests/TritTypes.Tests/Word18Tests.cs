using Microsoft.VisualStudio.TestTools.UnitTesting;
using TritTypes;
using System;
using System.Linq;

namespace TritTypes.Tests
{
    [TestClass]
    public class Word18Tests
    {
        private static long Pow3(int exp) { long r = 1; for (int i = 0; i < exp; i++) r *= 3; return r; }

        [TestMethod] public void Constructor_MaxValue_Works() { int max = (int)((Pow3(18) - 1) / 2); Word18 w = new Word18(max); Assert.AreEqual(max, w.ToLong()); }
        [TestMethod] public void Constructor_MinValue_Works() { int min = -(int)((Pow3(18) - 1) / 2); Word18 w = new Word18(min); Assert.AreEqual(min, w.ToLong()); }
        [TestMethod] public void Constructor_TooLarge_Throws() { int max = 193710244; try { new Word18(max + 1); Assert.Fail("Expected exception"); } catch (ArgumentOutOfRangeException) { } }
        [TestMethod] public void Addition_Works() { Word18 a = new(1000), b = new(2000); Assert.AreEqual(3000, (a + b).ToLong()); }
        [TestMethod] public void Subtraction_Works() { Word18 a = new(2000), b = new(1000); Assert.AreEqual(1000, (a - b).ToLong()); }
        [TestMethod] public void Multiplication_Works() { Word18 a = new(100), b = new(200); Assert.AreEqual(20000, (a * b).ToLong()); }
        [TestMethod] public void Division_Floor_Works() { Word18 a = new(10), b = new(3); Assert.AreEqual(3, (a / b).ToLong()); a = new Word18(-10); Assert.AreEqual(-4, (a / b).ToLong()); }
        [TestMethod] public void Modulus_Works() { Word18 a = new(10), b = new(3); Assert.AreEqual(1, (a % b).ToLong()); a = new Word18(-10); Assert.AreEqual(2, (a % b).ToLong()); }
        [TestMethod] public void Division_ByZero_Throws() { try { var _ = new Word18(10) / new Word18(0); Assert.Fail("Expected exception"); } catch (DivideByZeroException) { } }
        [TestMethod] public void Negation_Works() { Word18 a = new(1000); Assert.AreEqual(-1000, (-a).ToLong()); }
        [TestMethod] public void ShiftLeft_MultiplyBy3() { Word18 a = new(10); Assert.AreEqual(30, (a<<1).ToLong()); Assert.AreEqual(90, (a<<2).ToLong()); }
        [TestMethod] public void ShiftRight_DivideBy3() { Word18 a = new(30); Assert.AreEqual(10, (a>>1).ToLong()); a = new Word18(10); Assert.AreEqual(3, (a>>1).ToLong()); }
        [TestMethod] public void TritAnd_Works() { Assert.AreEqual(0, Word18.TritAnd(Word18.FromLong(0),Word18.FromLong(1)).ToLong()); Assert.AreEqual(1, Word18.TritAnd(Word18.FromLong(1),Word18.FromLong(1)).ToLong()); }
        [TestMethod] public void TritOr_Works() { Assert.AreEqual(1, Word18.TritOr(Word18.FromLong(0),Word18.FromLong(1)).ToLong()); }
        [TestMethod] public void TritXor_Works() { Assert.AreEqual(-1, Word18.TritXor(Word18.FromLong(1),Word18.FromLong(1)).ToLong()); Assert.AreEqual(0, Word18.TritXor(Word18.FromLong(1),Word18.FromLong(-1)).ToLong()); }
        [TestMethod] public void Comparison_Works() { Word18 a = new(100), b = new(200); Assert.IsTrue(a<b); Assert.IsTrue(b>a); Assert.IsTrue(a<=b); Assert.IsTrue(b>=a); Assert.IsTrue(a!=b); Assert.IsTrue(a==new Word18(100)); }
        [TestMethod] public void ToTritString_Zero_Returns18Zeros() { var s = new Word18(0).ToTritString(); Assert.AreEqual(18,s.Length); Assert.IsTrue(s.All(c=>c=='0')); }
        [TestMethod] public void ToTritString_One_ReturnsCorrect() { var s = new Word18(1).ToTritString(); Assert.AreEqual(18,s.Length); Assert.AreEqual('+',s[17]); }
        [TestMethod] public void Parse_RoundTrip_Works() { int[] vals = {0,1,-1,100,-100,1000000,-1000000}; foreach(var v in vals) { Word18 w = new(v); var s = w.ToTritString(); Assert.AreEqual(w,Word18.Parse(s),$"Round-trip failed for {v}"); } }
        [TestMethod] public void ImplicitConversion_FromInt_Works() { Word18 w = 42; Assert.AreEqual(42,w.ToLong()); }
        [TestMethod] public void TritMinOperators_Work() { Word18 a = Word18.FromLong(0); Word18 b = Word18.FromLong(1); Assert.AreEqual(0, Word18.TritAnd(a,b).ToLong()); Assert.AreEqual(1, Word18.TritOr(a,b).ToLong()); }
    }
}