using Microsoft.VisualStudio.TestTools.UnitTesting;
using TritTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TritTypes.Tests
{
    [TestClass]
    public class TSciiTests
    {
        [TestMethod]
        public void FromChar_ASCII_ReturnsCorrectValue()
        {
            // 'A' -> CP1251 65 -> V = 65
            Assert.AreEqual(65, TScii.FromChar('A'));
            // Space -> CP1251 32 -> V = 32
            Assert.AreEqual(32, TScii.FromChar(' '));
            // '!' -> CP1251 33 -> V = 33
            Assert.AreEqual(33, TScii.FromChar('!'));
        }

        [TestMethod]
        public void FromChar_Cyrillic_ReturnsCorrectValue()
        {
            // 'А' (Cyrillic A) -> CP1251 192 -> V = 192
            Assert.AreEqual(192, TScii.FromChar('А'));
            // 'я' (Cyrillic ya) -> CP1251 255 -> V = 255
            Assert.AreEqual(255, TScii.FromChar('я'));
        }

        [TestMethod]
        public void ToChar_CorrectValue_ReturnsCorrectChar()
        {
            Assert.AreEqual('A', TScii.ToChar(65));
            Assert.AreEqual(' ', TScii.ToChar(32));
            Assert.AreEqual('А', TScii.ToChar(192));
            Assert.AreEqual('я', TScii.ToChar(255));
        }

        [TestMethod]
        public void RoundTrip_PreservesCharacter()
        {
            string testString = "Hello T3! Привет!";
            foreach (char c in testString)
            {
                Int128 val = TScii.FromChar(c);
                Assert.AreEqual(c, TScii.ToChar(val));
            }
        }

        [TestMethod]
        public void FromChar_SpecialChars_ReturnsValues()
        {
            // Test a few boundaries if possible, though FromChar depends on CP1251
            // Just ensure it doesn't crash for common symbols
            Assert.IsNotNull(TScii.FromChar('©')); 
            Assert.IsNotNull(TScii.FromChar('®'));
        }

        [TestMethod]
        public void PrintTSciiTable()
        {
            Console.WriteLine("TScii Table:");
            Console.WriteLine("{0,5} | {1,8} | {2,6} | {3,6} | {4}", "V", "Ternary", "0n", "0y", "Char");
            Console.WriteLine(new string('-', 45));
            
            for (int v = -364; v <= 364; v++)
            {
                Int128 val = v;
                int u = v + 364;
                string ternary = TScii.ToTritString(val);
                string ninary = TScii.ToNinary(val);
                string tryx = TScii.ToTryx(val);
                char c = TScii.ToChar(val);

                Console.WriteLine("{0,5} | {1,8} | {2,6} | {3,6} | {4}", v, ternary, ninary, tryx, c);
            }
        }
    }
}