using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class T3FpuTests
    {
        [TestMethod]
        public void Add_TwoPositiveNumbers_ReturnsSum()
        {
            var a = T3Float.FromDouble(10.0);
            var b = T3Float.FromDouble(20.0);
            var result = T3Fpu.Add(a, b);
            Assert.AreEqual(30.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Add_NegativeAndPositive_ReturnsCorrectSum()
        {
            var a = T3Float.FromDouble(-5.0);
            var b = T3Float.FromDouble(15.0);
            var result = T3Fpu.Add(a, b);
            Assert.AreEqual(10.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Sub_TwoNumbers_ReturnsDifference()
        {
            var a = T3Float.FromDouble(100.0);
            var b = T3Float.FromDouble(35.0);
            var result = T3Fpu.Sub(a, b);
            Assert.AreEqual(65.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Sub_ResultNegative_ReturnsNegative()
        {
            var a = T3Float.FromDouble(10.0);
            var b = T3Float.FromDouble(30.0);
            var result = T3Fpu.Sub(a, b);
            Assert.AreEqual(-20.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Mul_TwoNumbers_ReturnsProduct()
        {
            var a = T3Float.FromDouble(7.0);
            var b = T3Float.FromDouble(6.0);
            var result = T3Fpu.Mul(a, b);
            Assert.AreEqual(42.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Mul_ByZero_ReturnsZero()
        {
            var a = T3Float.FromDouble(42.0);
            var b = T3Float.FromDouble(0.0);
            var result = T3Fpu.Mul(a, b);
            Assert.AreEqual(0.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Div_TwoNumbers_ReturnsQuotient()
        {
            var a = T3Float.FromDouble(100.0);
            var b = T3Float.FromDouble(4.0);
            var result = T3Fpu.Div(a, b);
            Assert.AreEqual(25.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Div_ByZero_ThrowsException()
        {
            var a = T3Float.FromDouble(10.0);
            var b = T3Float.FromDouble(0.0);
            Assert.ThrowsException<DivideByZeroException>(() => T3Fpu.Div(a, b));
        }

        [TestMethod]
        public void Sqrt_PositiveNumber_ReturnsRoot()
        {
            var a = T3Float.FromDouble(16.0);
            var result = T3Fpu.Sqrt(a);
            Assert.AreEqual(4.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Sqrt_NegativeNumber_ThrowsException()
        {
            var a = T3Float.FromDouble(-1.0);
            Assert.ThrowsException<ArithmeticException>(() => T3Fpu.Sqrt(a));
        }

        [TestMethod]
        public void Sqrt_Zero_ReturnsZero()
        {
            var a = T3Float.FromDouble(0.0);
            var result = T3Fpu.Sqrt(a);
            Assert.AreEqual(0.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Abs_Negative_ReturnsPositive()
        {
            var a = T3Float.FromDouble(-42.0);
            var result = T3Fpu.Abs(a);
            Assert.AreEqual(42.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Abs_Positive_ReturnsSame()
        {
            var a = T3Float.FromDouble(42.0);
            var result = T3Fpu.Abs(a);
            Assert.AreEqual(42.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Abs_Zero_ReturnsZero()
        {
            var a = T3Float.FromDouble(0.0);
            var result = T3Fpu.Abs(a);
            Assert.AreEqual(0.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Neg_Positive_ReturnsNegative()
        {
            var a = T3Float.FromDouble(5.0);
            var result = T3Fpu.Neg(a);
            Assert.AreEqual(-5.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Neg_Negative_ReturnsPositive()
        {
            var a = T3Float.FromDouble(-5.0);
            var result = T3Fpu.Neg(a);
            Assert.AreEqual(5.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Compare_Greater_ReturnsPositive()
        {
            var a = T3Float.FromDouble(10.0);
            var b = T3Float.FromDouble(5.0);
            int result = T3Fpu.Compare(a, b);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Compare_Less_ReturnsNegative()
        {
            var a = T3Float.FromDouble(5.0);
            var b = T3Float.FromDouble(10.0);
            int result = T3Fpu.Compare(a, b);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Compare_Equal_ReturnsZero()
        {
            var a = T3Float.FromDouble(7.0);
            var b = T3Float.FromDouble(7.0);
            int result = T3Fpu.Compare(a, b);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ToInt_FromInt_RoundTrip_18Trit()
        {
            var a = T3Float.FromDouble(42.0);
            long intVal = T3Fpu.ToInt(a, 0);
            var back = T3Fpu.FromInt(intVal);
            Assert.AreEqual(42.0, back.ToDouble(), 0.01);
        }

        [TestMethod]
        public void ToInt_TruncatesTowardZero()
        {
            var a = T3Float.FromDouble(3.9);
            long intVal = T3Fpu.ToInt(a, 0);
            Assert.AreEqual(3, intVal);
        }

        [TestMethod]
        public void ToInt_Negative_TruncatesTowardZero()
        {
            var a = T3Float.FromDouble(-3.9);
            long intVal = T3Fpu.ToInt(a, 0);
            Assert.AreEqual(-3, intVal);
        }

        [TestMethod]
        public void FromInt_Zero_ReturnsZero()
        {
            var result = T3Fpu.FromInt(0);
            Assert.AreEqual(0.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void FromInt_Negative_ReturnsNegative()
        {
            var result = T3Fpu.FromInt(-100);
            Assert.AreEqual(-100.0, result.ToDouble(), 0.01);
        }

        [TestMethod]
        public void ToDoublePrecision_RoundTrip_ReturnsEquivalent()
        {
            var a = T3Float.FromDouble(3.14);
            var d = T3Fpu.ToDoublePrecision(a);
            var back = T3Fpu.FromDoublePrecision(d);
            Assert.AreEqual(a.ToDouble(), back.ToDouble(), 0.01);
        }

        [TestMethod]
        public void Classify_Zero_ReturnsZero()
        {
            var a = T3Float.FromDouble(0.0);
            int cls = T3Fpu.Classify(a);
            Assert.AreEqual(0, cls);
        }

        [TestMethod]
        public void Classify_Normal_Returns4()
        {
            var a = T3Float.FromDouble(3.14);
            int cls = T3Fpu.Classify(a);
            Assert.AreEqual(4, cls);
        }

        [TestMethod]
        public void Zero_ReturnsZero()
        {
            var result = T3Fpu.Zero();
            Assert.AreEqual(0.0, result.ToDouble(), 0.01);
        }
    }
}