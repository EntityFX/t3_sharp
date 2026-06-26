using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.Common.Tests
{
    [TestClass]
    public class T3FpuTests
    {
        // T3Float (tfloat) uses 6-trit exponent and 12-trit mantissa,
        // giving approximately 2.5 decimal digits of precision.
        // Tolerance is set conservatively to account for tfloat ↔ double round-trip error.
        private const double TOL = 1.5;

        // === Add ===

        [TestMethod]
        public void Add_TwoPositiveNumbers_ReturnsSum()
        {
            double actual = T3Fpu.Add(T3Float.FromDouble(3.0), T3Float.FromDouble(9.0)).ToDouble();
            Assert.AreEqual(12.0, actual, TOL);
        }

        [TestMethod]
        public void Add_NegativeAndPositive_ReturnsCorrectSum()
        {
            double actual = T3Fpu.Add(T3Float.FromDouble(-9.0), T3Float.FromDouble(27.0)).ToDouble();
            Assert.AreEqual(18.0, actual, TOL);
        }

        // === Sub ===

        [TestMethod]
        public void Sub_TwoNumbers_ReturnsDifference()
        {
            double actual = T3Fpu.Sub(T3Float.FromDouble(27.0), T3Float.FromDouble(9.0)).ToDouble();
            Assert.AreEqual(18.0, actual, TOL);
        }

        [TestMethod]
        public void Sub_ResultNegative_ReturnsNegative()
        {
            double actual = T3Fpu.Sub(T3Float.FromDouble(9.0), T3Float.FromDouble(27.0)).ToDouble();
            Assert.AreEqual(-18.0, actual, TOL);
        }

        // === Mul ===

        [TestMethod]
        public void Mul_TwoNumbers_ReturnsProduct()
        {
            double actual = T3Fpu.Mul(T3Float.FromDouble(3.0), T3Float.FromDouble(9.0)).ToDouble();
            Assert.AreEqual(27.0, actual, TOL);
        }

        [TestMethod]
        public void Mul_ByZero_ReturnsZero()
        {
            double actual = T3Fpu.Mul(T3Float.FromDouble(81.0), T3Float.FromDouble(0.0)).ToDouble();
            Assert.AreEqual(0.0, actual, 0.01);
        }

        // === Div ===

        [TestMethod]
        public void Div_TwoNumbers_ReturnsQuotient()
        {
            double actual = T3Fpu.Div(T3Float.FromDouble(27.0), T3Float.FromDouble(9.0)).ToDouble();
            Assert.AreEqual(3.0, actual, TOL);
        }

        [TestMethod]
        public void Div_ByZero_ThrowsException()
        {
            var a = T3Float.FromDouble(9.0);
            var b = T3Float.FromDouble(0.0);
            Assert.ThrowsException<DivideByZeroException>(() => T3Fpu.Div(a, b));
        }

        // === Sqrt ===

        [TestMethod]
        public void Sqrt_PositiveNumber_ReturnsRoot()
        {
            double actual = T3Fpu.Sqrt(T3Float.FromDouble(81.0)).ToDouble();
            Assert.AreEqual(9.0, actual, 1.0);
        }

        [TestMethod]
        public void Sqrt_NegativeNumber_ThrowsException()
        {
            Assert.ThrowsException<ArithmeticException>(() => T3Fpu.Sqrt(T3Float.FromDouble(-1.0)));
        }

        [TestMethod]
        public void Sqrt_Zero_ReturnsZero()
        {
            double actual = T3Fpu.Sqrt(T3Float.FromDouble(0.0)).ToDouble();
            Assert.AreEqual(0.0, actual, 0.01);
        }

        // === Abs ===

        [TestMethod]
        public void Abs_Negative_ReturnsPositive()
        {
            double actual = T3Fpu.Abs(T3Float.FromDouble(-27.0)).ToDouble();
            Assert.AreEqual(27.0, actual, TOL);
        }

        [TestMethod]
        public void Abs_Positive_ReturnsSame()
        {
            double actual = T3Fpu.Abs(T3Float.FromDouble(27.0)).ToDouble();
            Assert.AreEqual(27.0, actual, TOL);
        }

        [TestMethod]
        public void Abs_Zero_ReturnsZero()
        {
            double actual = T3Fpu.Abs(T3Float.FromDouble(0.0)).ToDouble();
            Assert.AreEqual(0.0, actual, 0.01);
        }

        // === Neg ===

        [TestMethod]
        public void Neg_Positive_ReturnsNegative()
        {
            double actual = T3Fpu.Neg(T3Float.FromDouble(3.0)).ToDouble();
            Assert.AreEqual(-3.0, actual, TOL);
        }

        [TestMethod]
        public void Neg_Negative_ReturnsPositive()
        {
            double actual = T3Fpu.Neg(T3Float.FromDouble(-3.0)).ToDouble();
            Assert.AreEqual(3.0, actual, TOL);
        }

        // === Compare ===

        [TestMethod]
        public void Compare_Greater_ReturnsPositive()
        {
            Assert.AreEqual(1, T3Fpu.Compare(T3Float.FromDouble(27.0), T3Float.FromDouble(3.0)));
        }

        [TestMethod]
        public void Compare_Less_ReturnsNegative()
        {
            Assert.AreEqual(-1, T3Fpu.Compare(T3Float.FromDouble(3.0), T3Float.FromDouble(27.0)));
        }

        [TestMethod]
        public void Compare_Equal_ReturnsZero()
        {
            Assert.AreEqual(0, T3Fpu.Compare(T3Float.FromDouble(9.0), T3Float.FromDouble(9.0)));
        }

        // === ToInt / FromInt ===

        [TestMethod]
        public void ToInt_FromInt_RoundTrip()
        {
            long intVal = T3Fpu.ToInt(T3Float.FromDouble(9.0), 0);
            double back = T3Fpu.FromInt(intVal).ToDouble();
            Assert.AreEqual(9.0, back, 0.01);
        }

        [TestMethod]
        public void ToInt_TruncatesTowardZero()
        {
            long intVal = T3Fpu.ToInt(T3Float.FromDouble(9.9), 0);
            Assert.AreEqual(9, intVal);
        }

        [TestMethod]
        public void ToInt_Negative_TruncatesTowardZero()
        {
            long intVal = T3Fpu.ToInt(T3Float.FromDouble(-9.9), 0);
            Assert.AreEqual(-9, intVal);
        }

        [TestMethod]
        public void FromInt_Zero_ReturnsZero()
        {
            Assert.AreEqual(0.0, T3Fpu.FromInt(0).ToDouble(), 0.01);
        }

        [TestMethod]
        public void FromInt_ReturnsCorrectValue()
        {
            double result = T3Fpu.FromInt(27).ToDouble();
            Assert.AreEqual(27.0, result, TOL);
        }

        // === ToDoublePrecision / FromDoublePrecision ===

        [TestMethod]
        public void ToDoublePrecision_RoundTrip_ReturnsEquivalent()
        {
            var a = T3Float.FromDouble(9.0);
            var d = T3Fpu.ToDoublePrecision(a);
            var back = T3Fpu.FromDoublePrecision(d);
            Assert.AreEqual(a.ToDouble(), back.ToDouble(), 0.01);
        }

        // === Classify ===

        [TestMethod]
        public void Classify_Zero_ReturnsZero()
        {
            Assert.AreEqual(0, T3Fpu.Classify(T3Float.FromDouble(0.0)));
        }

        [TestMethod]
        public void Classify_Normal_Returns4()
        {
            // 3.0 classifies as normal (not zero, not NaN, not infinity)
            Assert.AreEqual(4, T3Fpu.Classify(T3Float.FromDouble(3.0)));
        }

        // === Zero ===

        [TestMethod]
        public void Zero_ReturnsZero()
        {
            Assert.AreEqual(0.0, T3Fpu.Zero().ToDouble(), 0.01);
        }
    }
}