using System.Linq;
using System.Numerics;
using TritTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TritTypes.Tests
{
    [TestClass]
    public class Word54Tests
    {
        [TestMethod]
        public void Constructor_Zero_Works()
        {
            Word54 w = new Word54(0);
            Assert.AreEqual(0, w.ToInt128());
        }

        [TestMethod]
        public void Constructor_MaxValue_Works()
        {
            Int128 p3_54 = 1;
            for(int i=0; i<54; i++) p3_54 *= 3;
            Int128 maxVal = (p3_54 - 1) / 2;
            
            Word54 w = new Word54(maxVal);
            Assert.AreEqual(maxVal, w.ToInt128());
        }

        [TestMethod]
        public void Constructor_MinValue_Works()
        {
            Int128 p3_54 = 1;
            for(int i=0; i<54; i++) p3_54 *= 3;
            Int128 minVal = -(p3_54 - 1) / 2;
            
            Word54 w = new Word54(minVal);
            Assert.AreEqual(minVal, w.ToInt128());
        }

        [TestMethod]
        public void Constructor_TooLarge_Throws()
        {
            Int128 p3_54 = 1;
            for(int i=0; i<54; i++) p3_54 *= 3;
            Int128 maxVal = (p3_54 - 1) / 2;
            try { new Word54(maxVal + 1); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void Addition_Works()
        {
            Word54 a = new Word54(1000);
            Word54 b = new Word54(2000);
            Assert.AreEqual(3000, (a + b).ToInt128());
        }

        [TestMethod]
        public void Subtraction_Works()
        {
            Word54 a = new Word54(2000);
            Word54 b = new Word54(1000);
            Assert.AreEqual(1000, (a - b).ToInt128());
        }

        [TestMethod]
        public void Multiplication_Works()
        {
            Word54 a = new Word54(100);
            Word54 b = new Word54(200);
            Assert.AreEqual(20000, (a * b).ToInt128());
        }

        [TestMethod]
        public void Division_Floor_Works()
        {
            Word54 a = new Word54(10);
            Word54 b = new Word54(3);
            Assert.AreEqual(3, (a / b).ToInt128());

            a = new Word54(-10);
            Assert.AreEqual(-4, (a / b).ToInt128());
        }

        [TestMethod]
        public void Modulus_Works()
        {
            Word54 a = new Word54(10);
            Word54 b = new Word54(3);
            Assert.AreEqual(1, (a % b).ToInt128());

            a = new Word54(-10);
            Assert.AreEqual(2, (a % b).ToInt128());
        }

        [TestMethod]
        public void Division_ByZero_Throws()
        {
            Word54 a = new Word54(10);
            Word54 b = new Word54(0);
            try { var _ = a / b; Assert.Fail("Expected exception"); }
            catch (DivideByZeroException) { }
        }

        [TestMethod]
        public void Negation_Works()
        {
            Word54 a = new Word54(1000);
            Assert.AreEqual(-1000, (-a).ToInt128());
        }

        [TestMethod]
        public void ShiftLeft_MultiplyBy3()
        {
            Word54 a = new Word54(10);
            Word54 shifted = a << 1;
            Assert.AreEqual(30, shifted.ToInt128());

            shifted = a << 2;
            Assert.AreEqual(90, shifted.ToInt128());
        }

        [TestMethod]
        public void ShiftRight_DivideBy3()
        {
            Word54 a = new Word54(30);
            Word54 shifted = a >> 1;
            Assert.AreEqual(10, shifted.ToInt128());

            a = new Word54(10);
            shifted = a >> 1;
            Assert.AreEqual(3, shifted.ToInt128());
        }

        [TestMethod]
        public void TritAnd_Works()
        {
            Word54 a = new Word54(0);
            Word54 b = new Word54(1);
            Word54 result = Word54.TritAnd(a, b);
            Assert.AreEqual(0, result.ToInt128());

            a = new Word54(1);
            b = new Word54(1);
            result = Word54.TritAnd(a, b);
            Assert.AreEqual(1, result.ToInt128());
        }

        [TestMethod]
        public void TritOr_Works()
        {
            Word54 a = new Word54(0);
            Word54 b = new Word54(1);
            Word54 result = Word54.TritOr(a, b);
            Assert.AreEqual(1, result.ToInt128());
        }

        [TestMethod]
        public void TritXor_Works()
        {
            Word54 a = new Word54(1);
            Word54 b = new Word54(1);
            Word54 result = Word54.TritXor(a, b);
            Assert.AreEqual(-1, result.ToInt128());

            a = new Word54(1);
            b = new Word54(-1);
            result = Word54.TritXor(a, b);
            Assert.AreEqual(0, result.ToInt128());
        }

        [TestMethod]
        public void Comparison_Works()
        {
            Word54 a = new Word54(100);
            Word54 b = new Word54(200);
            Assert.IsTrue(a < b);
            Assert.IsTrue(b > a);
            Assert.IsTrue(a != b);
            Assert.IsTrue(a == new Word54(100));
        }

        [TestMethod]
        public void ToTritString_Zero_Returns54Zeros()
        {
            Word54 w = new Word54(0);
            string s = w.ToTritString();
            Assert.AreEqual(54, s.Length);
            Assert.IsTrue(s.All(c => c == '0'));
        }

        [TestMethod]
        public void Parse_RoundTrip_Works()
        {
            Int128[] testValues = { 0, 1, -1, 100, -100, 1000000, -1000000 };
            foreach (Int128 val in testValues)
            {
                Word54 w = new Word54(val);
                string s = w.ToTritString();
                Word54 parsed = Word54.Parse(s);
                Assert.AreEqual(w, parsed, $"Round-trip failed for {val}");
            }
        }

        [TestMethod]
        public void ImplicitConversion_FromLong_Works()
        {
            Word54 w = 42L;
            Assert.AreEqual(42, w.ToInt128());
        }

        [TestMethod]
        public void ExplicitConversion_ToInt128_Works()
        {
            Word54 w = new Word54(42);
            Assert.AreEqual(42, (Int128)w);
        }
    }
}