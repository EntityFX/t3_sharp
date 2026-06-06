using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class TritArrayTests
    {
        [TestMethod]
        public void And_ReturnsMinimum()
        {
            Trit[] a = { Trit.MinusOne, Trit.Zero, Trit.PlusOne };
            Trit[] b = { Trit.PlusOne, Trit.PlusOne, Trit.PlusOne };
            Trit[] result = TritArray.And(a, b);
            Assert.AreEqual(Trit.MinusOne, result[0]);
            Assert.AreEqual(Trit.Zero, result[1]);
            Assert.AreEqual(Trit.PlusOne, result[2]);
        }

        [TestMethod]
        public void Or_ReturnsMaximum()
        {
            Trit[] a = { Trit.MinusOne, Trit.Zero, Trit.PlusOne };
            Trit[] b = { Trit.Zero, Trit.Zero, Trit.Zero };
            Trit[] result = TritArray.Or(a, b);
            Assert.AreEqual(Trit.Zero, result[0]);
            Assert.AreEqual(Trit.Zero, result[1]);
            Assert.AreEqual(Trit.PlusOne, result[2]);
        }

        [TestMethod]
        public void Xor_ReturnsSumMod3()
        {
            Trit[] a = { Trit.PlusOne, Trit.PlusOne, Trit.MinusOne };
            Trit[] b = { Trit.MinusOne, Trit.PlusOne, Trit.MinusOne };
            Trit[] result = TritArray.Xor(a, b);
            Assert.AreEqual(Trit.Zero, result[0]);
            Assert.AreEqual(Trit.MinusOne, result[1]);
            Assert.AreEqual(Trit.PlusOne, result[2]);
        }

        [TestMethod]
        public void And_DifferentLengths_Throws()
        {
            try { TritArray.And(new Trit[2], new Trit[3]); Assert.Fail("Expected exception"); }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void ShiftLeft_InsertsZeros()
        {
            Trit[] a = { Trit.PlusOne, Trit.Zero, Trit.MinusOne };
            Trit[] result = TritArray.ShiftLeft(a, 1);
            Assert.AreEqual(Trit.Zero, result[0]);
            Assert.AreEqual(Trit.PlusOne, result[1]);
            Assert.AreEqual(Trit.Zero, result[2]);
        }

        [TestMethod]
        public void ShiftLeft_ByZero_ReturnsClone()
        {
            Trit[] a = { Trit.PlusOne, Trit.Zero, Trit.MinusOne };
            Trit[] result = TritArray.ShiftLeft(a, 0);
            Assert.AreEqual(a[0], result[0]);
            Assert.AreEqual(a[1], result[1]);
            Assert.AreEqual(a[2], result[2]);
            Assert.AreNotSame(a, result);
        }

        [TestMethod]
        public void ShiftRight_SignExtends()
        {
            Trit[] a = { Trit.PlusOne, Trit.Zero, Trit.MinusOne };
            Trit[] result = TritArray.ShiftRight(a, 1);
            Assert.AreEqual(Trit.Zero, result[0]);
            Assert.AreEqual(Trit.MinusOne, result[1]);
            Assert.AreEqual(Trit.MinusOne, result[2]);
        }

        [TestMethod]
        public void ShiftRight_PositiveSign_ExtendsZero()
        {
            Trit[] a = { Trit.PlusOne, Trit.Zero, Trit.PlusOne };
            Trit[] result = TritArray.ShiftRight(a, 1);
            Assert.AreEqual(Trit.Zero, result[0]);
            Assert.AreEqual(Trit.PlusOne, result[1]);
            Assert.AreEqual(Trit.PlusOne, result[2]);
        }

        [TestMethod]
        public void ToString_ReturnsCorrect()
        {
            Trit[] a = { Trit.MinusOne, Trit.Zero, Trit.PlusOne };
            Assert.AreEqual("-0+", TritArray.ToString(a));
        }

        [TestMethod]
        public void FromString_ParsesCorrectly()
        {
            Trit[] result = TritArray.FromString("-0+");
            Assert.AreEqual(Trit.MinusOne, result[0]);
            Assert.AreEqual(Trit.Zero, result[1]);
            Assert.AreEqual(Trit.PlusOne, result[2]);
        }

        [TestMethod]
        public void RoundTrip_StringConversion()
        {
            Trit[] original = { Trit.MinusOne, Trit.Zero, Trit.PlusOne, Trit.Zero, Trit.MinusOne };
            string s = TritArray.ToString(original);
            Trit[] parsed = TritArray.FromString(s);
            CollectionAssert.AreEqual(original, parsed);
        }
    }
}