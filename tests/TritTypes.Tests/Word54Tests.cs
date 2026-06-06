using System.Numerics;
using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class Word54Tests
    {
        [TestMethod]
        public void Constructor_Zero_Works()
        {
            Word54 w = new Word54(0);
            Assert.AreEqual(0, w.ToBigInteger());
        }

        [TestMethod]
        public void Constructor_MaxValue_Works()
        {
            BigInteger max = (BigInteger.Pow(3, 54) - 1) / 2;
            Word54 w = new Word54(max);
            Assert.AreEqual(max, w.ToBigInteger());
        }

        [TestMethod]
        public void Constructor_MinValue_Works()
        {
            BigInteger min = -(BigInteger.Pow(3, 54) - 1) / 2;
            Word54 w = new Word54(min);
            Assert.AreEqual(min, w.ToBigInteger());
        }

        [TestMethod]
        public void Constructor_TooLarge_Throws()
        {
            BigInteger max = (BigInteger.Pow(3, 54) - 1) / 2;
            try { new Word54(max + 1); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void Addition_Works()
        {
            Word54 a = new Word54(1000);
            Word54 b = new Word54(2000);
            Assert.AreEqual(3000, (a + b).ToBigInteger());
        }

        [TestMethod]
        public void Subtraction_Works()
        {
            Word54 a = new Word54(2000);
            Word54 b = new Word54(1000);
            Assert.AreEqual(1000, (a - b).ToBigInteger());
        }

        [TestMethod]
        public void Multiplication_Works()
        {
            Word54 a = new Word54(100);
            Word54 b = new Word54(200);
            Assert.AreEqual(20000, (a * b).ToBigInteger());
        }

        [TestMethod]
        public void Division_Floor_Works()
        {
            Word54 a = new Word54(10);
            Word54 b = new Word54(3);
            Assert.AreEqual(3, (a / b).ToBigInteger());

            a = new Word54(-10);
            Assert.AreEqual(-4, (a / b).ToBigInteger());
        }

        [TestMethod]
        public void Modulus_Works()
        {
            Word54 a = new Word54(10);
            Word54 b = new Word54(3);
            Assert.AreEqual(1, (a % b).ToBigInteger());

            a = new Word54(-10);
            Assert.AreEqual(2, (a % b).ToBigInteger());
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
            Assert.AreEqual(-1000, (-a).ToBigInteger());
        }

        [TestMethod]
        public void ShiftLeft_MultiplyBy3()
        {
            Word54 a = new Word54(10);
            Word54 shifted = a << 1;
            Assert.AreEqual(30, shifted.ToBigInteger());

            shifted = a << 2;
            Assert.AreEqual(90, shifted.ToBigInteger());
        }

        [TestMethod]
        public void ShiftRight_DivideBy3()
        {
            Word54 a = new Word54(30);
            Word54 shifted = a >> 1;
            Assert.AreEqual(10, shifted.ToBigInteger());

            a = new Word54(10);
            shifted = a >> 1;
            Assert.AreEqual(3, shifted.ToBigInteger());
        }

        [TestMethod]
        public void TritAnd_Works()
        {
            Word54 a = Word54.FromBigInteger(0);
            Word54 b = Word54.FromBigInteger(1);
            Word54 result = Word54.TritAnd(a, b);
            Assert.AreEqual(0, result.ToBigInteger());

            a = Word54.FromBigInteger(1);
            b = Word54.FromBigInteger(1);
            result = Word54.TritAnd(a, b);
            Assert.AreEqual(1, result.ToBigInteger());
        }

        [TestMethod]
        public void TritOr_Works()
        {
            Word54 a = Word54.FromBigInteger(0);
            Word54 b = Word54.FromBigInteger(1);
            Word54 result = Word54.TritOr(a, b);
            Assert.AreEqual(1, result.ToBigInteger());
        }

        [TestMethod]
        public void TritXor_Works()
        {
            Word54 a = Word54.FromBigInteger(1);
            Word54 b = Word54.FromBigInteger(1);
            Word54 result = Word54.TritXor(a, b);
            Assert.AreEqual(-1, result.ToBigInteger());

            a = Word54.FromBigInteger(1);
            b = Word54.FromBigInteger(-1);
            result = Word54.TritXor(a, b);
            Assert.AreEqual(0, result.ToBigInteger());
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
            BigInteger[] testValues = { 0, 1, -1, 100, -100, 1000000, -1000000 };
            foreach (BigInteger val in testValues)
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
            Assert.AreEqual(42, w.ToBigInteger());
        }

        [TestMethod]
        public void ExplicitConversion_ToBigInteger_Works()
        {
            Word54 w = new Word54(42);
            Assert.AreEqual(42, (BigInteger)w);
        }
    }
}