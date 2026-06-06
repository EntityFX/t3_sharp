using System.Numerics;
using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class BalancedTernaryTests
    {
        [TestMethod]
        public void ToTernaryString_Zero_Returns0()
        {
            Assert.AreEqual("0", BalancedTernary.ToTernaryString(0));
        }

        [TestMethod]
        public void ToTernaryString_One_ReturnsPlus()
        {
            Assert.AreEqual("+", BalancedTernary.ToTernaryString(1));
        }

        [TestMethod]
        public void ToTernaryString_MinusOne_ReturnsMinus()
        {
            Assert.AreEqual("-", BalancedTernary.ToTernaryString(-1));
        }

        [TestMethod]
        public void ToTernaryString_Three_ReturnsPlus0()
        {
            Assert.AreEqual("+0", BalancedTernary.ToTernaryString(3));
        }

        [TestMethod]
        public void ToTernaryString_Four_ReturnsPlusPlus()
        {
            Assert.AreEqual("++", BalancedTernary.ToTernaryString(4));
        }

        [TestMethod]
        public void ToTernaryString_Two_ReturnsPlusMinus()
        {
            Assert.AreEqual("+-", BalancedTernary.ToTernaryString(2));
        }

        [TestMethod]
        public void ToTernaryString_MinDigits_Pads()
        {
            string s = BalancedTernary.ToTernaryString(1, 5);
            Assert.AreEqual(5, s.Length);
            Assert.AreEqual("0000+", s);
        }

        [TestMethod]
        public void ParseToLong_RoundTrip()
        {
            long[] values = { 0, 1, -1, 2, -2, 10, -10, 100, -100, 1000000, -1000000 };
            foreach (long val in values)
            {
                string s = BalancedTernary.ToTernaryString(val);
                long parsed = BalancedTernary.ParseToLong(s);
                Assert.AreEqual(val, parsed, $"Round-trip failed for {val} (string: {s})");
            }
        }

        [TestMethod]
        public void ParseToInt128_RoundTrip()
        {
            Int128[] values = {
                Int128.Zero, Int128.One, -Int128.One,
                (Int128)100, (Int128)(-100),
                (Int128)Math.Pow(3, 20), (Int128)(-Math.Pow(3, 20))
            };
            foreach (Int128 val in values)
            {
                string s = BalancedTernary.ToTernaryString(val);
                Int128 parsed = BalancedTernary.ParseToInt128(s);
                Assert.AreEqual(val, parsed, $"Round-trip failed for {val} (string: {s})");
            }
        }

        [TestMethod]
        public void ToTernaryString_Int128_Works()
        {
            Int128 val = (Int128)Math.Pow(3, 10);
            string s = BalancedTernary.ToTernaryString(val);
            Assert.AreEqual("+0000000000", s);
            Int128 parsed = BalancedTernary.ParseToInt128(s);
            Assert.AreEqual(val, parsed);
        }

        [TestMethod]
        public void ParseToLong_InvalidChar_Throws()
        {
            try { BalancedTernary.ParseToLong("12+"); Assert.Fail("Expected exception"); }
            catch (FormatException) { }
        }

        [TestMethod]
        public void KnownValues_AreCorrect()
        {
            var known = new (long value, string ternary)[]
            {
                (0, "0"),
                (1, "+"),
                (-1, "-"),
                (2, "+-"),
                (-2, "-+"),
                (3, "+0"),
                (-3, "-0"),
                (4, "++"),
                (-4, "--"),
                (5, "+--"),
                (-5, "-++"),
                (6, "+-0"),
                (-6, "-+0"),
                (7, "+-+"),
                (-7, "-+-"),
                (8, "+0-"),
                (-8, "-0+"),
                (9, "+00"),
                (-9, "-00"),
                (10, "+0+"),
                (-10, "-0-"),
            };

            foreach (var (value, ternary) in known)
            {
                string s = BalancedTernary.ToTernaryString(value);
                Assert.AreEqual(ternary, s, $"Value {value} should be '{ternary}' but got '{s}'");
                long parsed = BalancedTernary.ParseToLong(ternary);
                Assert.AreEqual(value, parsed, $"String '{ternary}' should parse to {value} but got {parsed}");
            }
        }
    }
}