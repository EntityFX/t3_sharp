using Microsoft.VisualStudio.TestTools.UnitTesting;
using TritTypes;
using System;

namespace TritTypes.Tests
{
    [TestClass]
    public class T3ConversionServiceTests
    {
        private T3ConversionService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new T3ConversionService();
        }

        [TestMethod]
        public void Convert_Decimal_ShouldWork()
        {
            var result = _service.Convert("42");
            Assert.AreEqual(42, result.DecimalValue);
            Assert.AreEqual("0x2A", result.Hex);
            Assert.AreEqual("0b101010", result.Binary);
            Assert.AreEqual("0052", result.Octal);
        }

        [TestMethod]
        public void Convert_Hex_ShouldWork()
        {
            var result = _service.Convert("0x2A");
            Assert.AreEqual(42, result.DecimalValue);
        }

        [TestMethod]
        public void Convert_Binary_ShouldWork()
        {
            var result = _service.Convert("0b101010");
            Assert.AreEqual(42, result.DecimalValue);
        }

        [TestMethod]
        public void Convert_Octal_ShouldWork()
        {
            var result = _service.Convert("0052");
            Assert.AreEqual(42, result.DecimalValue);
        }

        [TestMethod]
        public void Convert_Ternary_ShouldWork()
        {
            // 42 in balanced ternary:
            // 42 / 3 = 14 R 0
            // 14 / 3 = 5 R -1
            // 5 / 3 = 2 R -1
            // 2 / 3 = 1 R -1
            // 1 / 3 = 0 R 1
            // Result: +---0
            var result = _service.Convert("0t+---0");
            Assert.AreEqual(42, result.DecimalValue);
        }

        [TestMethod]
        public void Convert_Nonary_ShouldWork()
        {
            // 42 = 4*9 + 6. Nonary digits for TritTypes: 0-4 (WXYZ01234)
            // Wait, 42 = 4*9 + 6. 6 is not in 0-4.
            // In TritTypes, Nonary is based on pairs of trits.
            // 42 = 0t1---0. Pairs: (1-), (--), (-0)
            // (1-) = 2, (--) = W, (-0) = X? No, let's check T3ConversionService logic.
            // TritTo9Ary: "--" => 'W', "-0" => 'X', "-+" => 'Y', "0-" => 'Z', "00" => '0', "0+" => '1', "+-" => '2', "+0" => '3', "++" => '4'
            // 42 = 0t1---0. Padded: 0t01-- -0. 
            // Pairs: (01) -> '1', (--) -> 'W', (-0) -> 'X'. Result: 0n1WX
            var result = _service.Convert("0n1WX");
            Assert.AreEqual(42, result.DecimalValue);
        }

        [TestMethod]
        public void Convert_TwentySevenAry_ShouldWork()
        {
            // 0 in balanced ternary is 0,0,0.
            // index = (0+1)*9 + (0+1)*3 + (0+1) = 13.
            // alphabet[13] = '0'
            var result = _service.Convert("0y0");
            Assert.AreEqual(0, result.DecimalValue);
        }

        [TestMethod]
        public void Convert_InvalidFormat_ShouldThrow()
        {
            try
            {
                _service.Convert("0xZZZ");
                Assert.Fail("Should have thrown FormatException");
            }
            catch (FormatException)
            {
                // Success
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected FormatException, but caught {ex.GetType().Name}");
            }
        }
    }
}