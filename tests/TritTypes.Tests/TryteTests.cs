using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class TryteTests
    {
        [TestMethod]
        public void Constructor_ValidValues_Succeeds()
        {
            Tryte t = new Tryte(0);
            Assert.AreEqual(0, t.ToLong());

            t = new Tryte(364);
            Assert.AreEqual(364, t.ToLong());

            t = new Tryte(-364);
            Assert.AreEqual(-364, t.ToLong());
        }

        [TestMethod]
        public void Constructor_TooLarge_Throws()
        {
            try { new Tryte(365); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void Constructor_TooSmall_Throws()
        {
            try { new Tryte(-365); Assert.Fail("Expected exception"); }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void ToString_Zero_ReturnsCorrect()
        {
            Tryte t = new Tryte(0);
            Assert.AreEqual("000000", t.ToString());
        }

        [TestMethod]
        public void ToString_Positive_ReturnsCorrect()
        {
            Tryte t = new Tryte(1);
            Assert.AreEqual("00000+", t.ToString());
        }

        [TestMethod]
        public void ToString_Negative_ReturnsCorrect()
        {
            Tryte t = new Tryte(-1);
            Assert.AreEqual("00000-", t.ToString());
        }

        [TestMethod]
        public void ToString_MaxValue_ReturnsCorrect()
        {
            Tryte t = new Tryte(364);
            string s = t.ToString();
            Assert.AreEqual(6, s.Length);
            Tryte parsed = Tryte.Parse(s);
            Assert.AreEqual(t, parsed);
        }

        [TestMethod]
        public void Parse_ValidString_Works()
        {
            Tryte t = Tryte.Parse("00000+");
            Assert.AreEqual(1, t.ToLong());

            t = Tryte.Parse("00000-");
            Assert.AreEqual(-1, t.ToLong());

            t = Tryte.Parse("000000");
            Assert.AreEqual(0, t.ToLong());
        }

        [TestMethod]
        public void Parse_InvalidChar_Throws()
        {
            try { Tryte.Parse("00000x"); Assert.Fail("Expected exception"); }
            catch (FormatException) { }
        }

        [TestMethod]
        public void Parse_WrongLength_Throws()
        {
            try { Tryte.Parse("00+"); Assert.Fail("Expected exception"); }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void Addition_Works()
        {
            Tryte a = new Tryte(100);
            Tryte b = new Tryte(200);
            Tryte c = a + b;
            Assert.AreEqual(300, c.ToLong());
        }

        [TestMethod]
        public void Subtraction_Works()
        {
            Tryte a = new Tryte(200);
            Tryte b = new Tryte(100);
            Tryte c = a - b;
            Assert.AreEqual(100, c.ToLong());
        }

        [TestMethod]
        public void Multiplication_Works()
        {
            Tryte a = new Tryte(10);
            Tryte b = new Tryte(20);
            Tryte c = a * b;
            Assert.AreEqual(200, c.ToLong());
        }

        [TestMethod]
        public void Negation_Works()
        {
            Tryte a = new Tryte(100);
            Tryte neg = -a;
            Assert.AreEqual(-100, neg.ToLong());
        }

        [TestMethod]
        public void Equality_Works()
        {
            Tryte a = new Tryte(42);
            Tryte b = new Tryte(42);
            Tryte c = new Tryte(43);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
        }

        [TestMethod]
        public void ImplicitConversion_FromInt_Works()
        {
            Tryte t = 42;
            Assert.AreEqual(42, t.ToLong());
        }

        [TestMethod]
        public void ExplicitConversion_ToInt_Works()
        {
            Tryte t = new Tryte(42);
            Assert.AreEqual(42, (int)t);
        }

        [TestMethod]
        public void RoundTrip_StringConversion()
        {
            for (int v = -364; v <= 364; v += 73)
            {
                Tryte t = new Tryte(v);
                string s = t.ToString();
                Tryte parsed = Tryte.Parse(s);
                Assert.AreEqual(t, parsed, $"Round-trip failed for value {v}");
            }
        }
    }
}