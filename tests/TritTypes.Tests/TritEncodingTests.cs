using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using TritTypes;

namespace TritTypes.Tests
{
    [TestClass]
    public class TritEncodingTests
    {
        [TestMethod]
        public void SimpleText_RoundTrip_Success()
        {
            int[] original = { -1, 0, 1, 1, -1, 0 };
            string encoded = TritEncoding.ToSimpleText(original);
            Assert.AreEqual("-0++-0", encoded);

            List<int> decoded = TritEncoding.FromSimpleText(encoded);
            CollectionAssert.AreEqual(original, decoded);
        }

        [TestMethod]
        public void Binary_RoundTrip_Success()
        {
            int[] original = { -1, 0, 1, -1, 0, 1, 1, 0 }; // 8 trits = 16 bits = 2 bytes
            byte[] encoded = TritEncoding.ToBinary(original);
            
            // Binary: -1=10, 0=00, 1=01
            // Sequence: 10 00 01 10 00 01 01 00
            // Byte 1: 10000110 (0x86)
            // Byte 2: 00010100 (0x14)
            Assert.HasCount(2, encoded);
            Assert.AreEqual(0x86, encoded[0]);
            Assert.AreEqual(0x14, encoded[1]);

            List<int> decoded = TritEncoding.FromBinary(encoded, original.Length);
            CollectionAssert.AreEqual(original, decoded);
        }

        [TestMethod]
        public void Ninary_RoundTrip_Success()
        {
            int[] original = { -1, -1, 0, 1, 1, 0, -1, 1 }; // 8 trits = 4 ninary chars
            // -1,-1 -> W
            // 0,1   -> 1
            // 1,0   -> 3
            // -1,1  -> Y
            string encoded = TritEncoding.ToNinary(original);
            Assert.AreEqual("0nW13Y", encoded);

            List<int> decoded = TritEncoding.FromNinary(encoded);
            CollectionAssert.AreEqual(original, decoded);
        }

        [TestMethod]
        public void Tryx_RoundTrip_Success()
        {
            int[] original = { -1, -1, -1, 0, 0, 0, 1, 1, 1 }; // 9 trits = 3 tryx chars
            // -1,-1,-1 -> N
            // 0,0,0    -> 0
            // 1,1,1    -> D
            string encoded = TritEncoding.ToTryx(original);
            Assert.AreEqual("0yN0D", encoded);

            List<int> decoded = TritEncoding.FromTryx(encoded);
            CollectionAssert.AreEqual(original, decoded);
        }

        [TestMethod]
        public void Ninary_InvalidChar_Ignored()
        {
            string input = "0nZ?1"; // '?' is invalid
            List<int> decoded = TritEncoding.FromNinary(input);
            // Z -> 0,-1; 1 -> 0,1
            int[] expected = { 0, -1, 0, 1 };
            CollectionAssert.AreEqual(expected, decoded);
        }

        [TestMethod]
        public void Tryx_InvalidChar_Ignored()
        {
            string input = "0yN!D"; // '!' is invalid
            List<int> decoded = TritEncoding.FromTryx(input);
            // N -> -1,-1,-1; D -> 1,1,1
            int[] expected = { -1, -1, -1, 1, 1, 1 };
            CollectionAssert.AreEqual(expected, decoded);
        }
    }
}