using System.Numerics;
using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class Word27Tests
    {
        [TestMethod]
        public void Constructor_Zero_Works()
        {
            Word27 w = new Word27(0);
            Assert.AreEqual(0, w.ToLong());
        }

        [TestMethod]
        public void Constructor_MaxValue_Works()
        {
            long max = (Pow3(27) - 1) / 2;
            Word27 w = new Word27(max);
            Assert.AreEqual(max, w.ToLong());
        }

        [TestMethod]
        public void Constructor_MinValue_Works()
        {
            long min = -(Pow3(27) - 1) / 2;
            Word27 w = new Word27(min);
            Assert.AreEqual(min, w.ToLong());
        }

        [TestMethod]
        public void Constructor_TooLarge_Throws()
        {
            long max = (Pow3(27) - 1) / 2;
            try { new Word27(max + 1); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void Addition_Works()
        {
            Word27 a = new Word27(1000);
            Word27 b = new Word27(2000);
            Assert.AreEqual(3000, (a + b).ToLong());
        }

        [TestMethod]
        public void Subtraction_Works()
        {
            Word27 a = new Word27(2000);
            Word27 b = new Word27(1000);
            Assert.AreEqual(1000, (a - b).ToLong());
        }

        [TestMethod]
        public void Multiplication_Works()
        {
            Word27 a = new Word27(100);
            Word27 b = new Word27(200);
            Assert.AreEqual(20000, (a * b).ToLong());
        }

        [TestMethod]
        public void Division_Floor_Works()
        {
            Word27 a = new Word27(10);
            Word27 b = new Word27(3);
            Assert.AreEqual(3, (a / b).ToLong());

            a = new Word27(-10);
            Assert.AreEqual(-4, (a / b).ToLong());
        }

        [TestMethod]
        public void Modulus_Works()
        {
            Word27 a = new Word27(10);
            Word27 b = new Word27(3);
            Assert.AreEqual(1, (a % b).ToLong());

            a = new Word27(-10);
            Assert.AreEqual(2, (a % b).ToLong());
        }

        [TestMethod]
        public void Division_ByZero_Throws()
        {
            Word27 a = new Word27(10);
            Word27 b = new Word27(0);
            try { var _ = a / b; Assert.Fail("Expected exception"); }
            catch (DivideByZeroException) { }
        }

        [TestMethod]
        public void Negation_Works()
        {
            Word27 a = new Word27(1000);
            Assert.AreEqual(-1000, (-a).ToLong());
        }

        [TestMethod]
        public void ShiftLeft_MultiplyBy3()
        {
            Word27 a = new Word27(10);
            Word27 shifted = a << 1;
            Assert.AreEqual(30, shifted.ToLong());

            shifted = a << 2;
            Assert.AreEqual(90, shifted.ToLong());
        }

        [TestMethod]
        public void ShiftRight_DivideBy3()
        {
            Word27 a = new Word27(30);
            Word27 shifted = a >> 1;
            Assert.AreEqual(10, shifted.ToLong());

            a = new Word27(10);
            shifted = a >> 1;
            Assert.AreEqual(3, shifted.ToLong());
        }

        [TestMethod]
        public void TritAnd_Works()
        {
            Word27 a = Word27.FromLong(0);
            Word27 b = Word27.FromLong(1);
            Word27 result = Word27.TritAnd(a, b);
            Assert.AreEqual(0, result.ToLong());

            a = Word27.FromLong(1);
            b = Word27.FromLong(1);
            result = Word27.TritAnd(a, b);
            Assert.AreEqual(1, result.ToLong());
        }

        [TestMethod]
        public void TritOr_Works()
        {
            Word27 a = Word27.FromLong(0);
            Word27 b = Word27.FromLong(1);
            Word27 result = Word27.TritOr(a, b);
            Assert.AreEqual(1, result.ToLong());
        }

        [TestMethod]
        public void TritXor_Works()
        {
            Word27 a = Word27.FromLong(1);
            Word27 b = Word27.FromLong(1);
            Word27 result = Word27.TritXor(a, b);
            Assert.AreEqual(-1, result.ToLong());

            a = Word27.FromLong(1);
            b = Word27.FromLong(-1);
            result = Word27.TritXor(a, b);
            Assert.AreEqual(0, result.ToLong());
        }

        [TestMethod]
        public void Comparison_Works()
        {
            Word27 a = new Word27(100);
            Word27 b = new Word27(200);
            Assert.IsTrue(a < b);
            Assert.IsTrue(b > a);
            Assert.IsTrue(a <= b);
            Assert.IsTrue(b >= a);
            Assert.IsTrue(a != b);
            Assert.IsTrue(a == new Word27(100));
        }

        [TestMethod]
        public void ToTritString_Zero_Returns27Zeros()
        {
            Word27 w = new Word27(0);
            string s = w.ToTritString();
            Assert.AreEqual(27, s.Length);
            Assert.IsTrue(s.All(c => c == '0'));
        }

        [TestMethod]
        public void ToTritString_One_ReturnsCorrect()
        {
            Word27 w = new Word27(1);
            string s = w.ToTritString();
            Assert.AreEqual(27, s.Length);
            Assert.AreEqual('+', s[26]);
        }

        [TestMethod]
        public void Parse_RoundTrip_Works()
        {
            long[] testValues = { 0, 1, -1, 100, -100, 1000000, -1000000 };
            foreach (long val in testValues)
            {
                Word27 w = new Word27(val);
                string s = w.ToTritString();
                Word27 parsed = Word27.Parse(s);
                Assert.AreEqual(w, parsed, $"Round-trip failed for {val}");
            }
        }

        [TestMethod]
        public void ImplicitConversion_FromLong_Works()
        {
            Word27 w = 42L;
            Assert.AreEqual(42, w.ToLong());
        }

        [TestMethod]
        public void ExplicitConversion_ToLong_Works()
        {
            Word27 w = new Word27(42);
            Assert.AreEqual(42, (BigInteger)w);
        }

        private static long Pow3(int exp)
        {
            long result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }
    }
}