using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class TritTests
    {
        [TestMethod]
        public void Constructor_ValidValues_Succeeds()
        {
            Assert.AreEqual(-1, Trit.MinusOne.Value);
            Assert.AreEqual(0, Trit.Zero.Value);
            Assert.AreEqual(1, Trit.PlusOne.Value);
        }

        [TestMethod]
        public void Constructor_InvalidValue_Throws()
        {
            try { Trit.FromInt(2); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void Constructor_InvalidNegative_Throws()
        {
            try { Trit.FromInt(-2); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void FromChar_ValidChars_Succeeds()
        {
            Assert.AreEqual(Trit.MinusOne, Trit.FromChar('-'));
            Assert.AreEqual(Trit.Zero, Trit.FromChar('0'));
            Assert.AreEqual(Trit.PlusOne, Trit.FromChar('+'));
        }

        [TestMethod]
        public void FromChar_InvalidChar_Throws()
        {
            try { Trit.FromChar('x'); Assert.Fail("Expected exception"); }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void ToChar_ReturnsCorrectChar()
        {
            Assert.AreEqual('-', Trit.MinusOne.ToChar());
            Assert.AreEqual('0', Trit.Zero.ToChar());
            Assert.AreEqual('+', Trit.PlusOne.ToChar());
        }

        [TestMethod]
        public void ToString_ReturnsCorrectString()
        {
            Assert.AreEqual("-", Trit.MinusOne.ToString());
            Assert.AreEqual("0", Trit.Zero.ToString());
            Assert.AreEqual("+", Trit.PlusOne.ToString());
        }

        [TestMethod]
        public void Equality_IdenticalValues_AreEqual()
        {
            Assert.AreEqual(Trit.MinusOne, Trit.FromInt(-1));
            Assert.AreEqual(Trit.Zero, Trit.FromInt(0));
            Assert.AreEqual(Trit.PlusOne, Trit.FromInt(1));
        }

        [TestMethod]
        public void Inequality_DifferentValues_AreNotEqual()
        {
            Assert.AreNotEqual(Trit.MinusOne, Trit.Zero);
            Assert.AreNotEqual(Trit.Zero, Trit.PlusOne);
        }

        [TestMethod]
        public void Addition_Wrapping_Works()
        {
            Assert.AreEqual(Trit.MinusOne, Trit.PlusOne + Trit.PlusOne);
            Assert.AreEqual(Trit.PlusOne, Trit.MinusOne + Trit.MinusOne);
            Assert.AreEqual(Trit.PlusOne, Trit.PlusOne + Trit.Zero);
            Assert.AreEqual(Trit.MinusOne, Trit.MinusOne + Trit.Zero);
            Assert.AreEqual(Trit.Zero, Trit.PlusOne + Trit.MinusOne);
        }

        [TestMethod]
        public void Subtraction_Wrapping_Works()
        {
            Assert.AreEqual(Trit.Zero, Trit.PlusOne - Trit.PlusOne);
            Assert.AreEqual(Trit.MinusOne, Trit.Zero - Trit.PlusOne);
            Assert.AreEqual(Trit.PlusOne, Trit.MinusOne - Trit.PlusOne);
        }

        [TestMethod]
        public void Multiplication_Works()
        {
            Assert.AreEqual(Trit.PlusOne, Trit.PlusOne * Trit.PlusOne);
            Assert.AreEqual(Trit.Zero, Trit.PlusOne * Trit.Zero);
            Assert.AreEqual(Trit.MinusOne, Trit.PlusOne * Trit.MinusOne);
            Assert.AreEqual(Trit.PlusOne, Trit.MinusOne * Trit.MinusOne);
        }

        [TestMethod]
        public void Negation_Works()
        {
            Assert.AreEqual(Trit.MinusOne, -Trit.PlusOne);
            Assert.AreEqual(Trit.Zero, -Trit.Zero);
            Assert.AreEqual(Trit.PlusOne, -Trit.MinusOne);
        }

        [TestMethod]
        public void TritAnd_ReturnsMinimum()
        {
            Assert.AreEqual(Trit.MinusOne, Trit.TritAnd(Trit.MinusOne, Trit.PlusOne));
            Assert.AreEqual(Trit.Zero, Trit.TritAnd(Trit.Zero, Trit.PlusOne));
            Assert.AreEqual(Trit.PlusOne, Trit.TritAnd(Trit.PlusOne, Trit.PlusOne));
            Assert.AreEqual(Trit.MinusOne, Trit.TritAnd(Trit.MinusOne, Trit.MinusOne));
        }

        [TestMethod]
        public void TritOr_ReturnsMaximum()
        {
            Assert.AreEqual(Trit.PlusOne, Trit.TritOr(Trit.MinusOne, Trit.PlusOne));
            Assert.AreEqual(Trit.PlusOne, Trit.TritOr(Trit.Zero, Trit.PlusOne));
            Assert.AreEqual(Trit.Zero, Trit.TritOr(Trit.MinusOne, Trit.Zero));
            Assert.AreEqual(Trit.MinusOne, Trit.TritOr(Trit.MinusOne, Trit.MinusOne));
        }

        [TestMethod]
        public void TritXor_ReturnsSumMod3()
        {
            Assert.AreEqual(Trit.Zero, Trit.TritXor(Trit.PlusOne, Trit.MinusOne));
            Assert.AreEqual(Trit.MinusOne, Trit.TritXor(Trit.PlusOne, Trit.PlusOne));
            Assert.AreEqual(Trit.PlusOne, Trit.TritXor(Trit.MinusOne, Trit.MinusOne));
            Assert.AreEqual(Trit.PlusOne, Trit.TritXor(Trit.PlusOne, Trit.Zero));
        }

        [TestMethod]
        public void ImplicitConversion_FromInt_Works()
        {
            Trit t = 1;
            Assert.AreEqual(Trit.PlusOne, t);
            t = 0;
            Assert.AreEqual(Trit.Zero, t);
            t = -1;
            Assert.AreEqual(Trit.MinusOne, t);
        }

        [TestMethod]
        public void ImplicitConversion_FromInvalidInt_Throws()
        {
            try { Trit t = 2; Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void ExplicitConversion_ToInt_Works()
        {
            Assert.AreEqual(-1, (int)Trit.MinusOne);
            Assert.AreEqual(0, (int)Trit.Zero);
            Assert.AreEqual(1, (int)Trit.PlusOne);
        }

        [TestMethod]
        public void CompareTo_OrdersCorrectly()
        {
            Assert.IsLessThan(0, Trit.MinusOne.CompareTo(Trit.Zero));
            Assert.IsLessThan(0, Trit.Zero.CompareTo(Trit.PlusOne));
            Assert.IsGreaterThan(0, Trit.PlusOne.CompareTo(Trit.MinusOne));
            Assert.AreEqual(0, Trit.Zero.CompareTo(Trit.Zero));
        }
    }
}