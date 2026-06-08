using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// T3 Floating Point Unit (FPU) core logic.
    /// Handles operations on tfloat (18-trit) and tdouble (36-trit) numbers.
    /// </summary>
    public static class T3Fpu
    {
        public static T3Float Add(T3Float a, T3Float b)
        {
            double res = a.ToDouble() + b.ToDouble();
            return T3Float.FromDouble(res);
        }

        public static T3Float Sub(T3Float a, T3Float b)
        {
            double res = a.ToDouble() - b.ToDouble();
            return T3Float.FromDouble(res);
        }

        public static T3Float Mul(T3Float a, T3Float b)
        {
            double res = a.ToDouble() * b.ToDouble();
            return T3Float.FromDouble(res);
        }

        public static T3Float Div(T3Float a, T3Float b)
        {
            if (b.ToDouble() == 0) throw new DivideByZeroException("FPU Division by zero");
            double res = a.ToDouble() / b.ToDouble();
            return T3Float.FromDouble(res);
        }

        public static T3Float Sqrt(T3Float a)
        {
            double val = a.ToDouble();
            if (val < 0) throw new ArithmeticException("FPU Sqrt of negative number");
            return T3Float.FromDouble(Math.Sqrt(val));
        }

        public static T3Float Abs(T3Float a)
        {
            return T3Float.FromDouble(Math.Abs(a.ToDouble()));
        }

        public static T3Float Neg(T3Float a)
        {
            return T3Float.FromDouble(-a.ToDouble());
        }

        public static int Compare(T3Float a, T3Float b)
        {
            double va = a.ToDouble();
            double vb = b.ToDouble();
            return va > vb ? 1 : (va < vb ? -1 : 0);
        }

        public static long ToInt(T3Float a, int precision)
        {
            // precision 0: 18-trit, 1: 54-trit
            // Simple truncation towards zero as per spec
            return (long)Math.Truncate(a.ToDouble());
        }

        public static T3Float FromInt(long value)
        {
            return T3Float.FromDouble(value);
        }

        public static T3Double ToDoublePrecision(T3Float a)
        {
            return T3Double.FromDouble(a.ToDouble());
        }

        public static T3Float FromDoublePrecision(T3Double a)
        {
            return T3Float.FromDouble(a.ToDouble());
        }

        public static int Classify(T3Float a)
        {
            double val = a.ToDouble();
            if (double.IsNaN(val)) return 2;
            if (double.IsPositiveInfinity(val) || double.IsNegativeInfinity(val)) return 1;
            if (val == 0) return 0;
            return 4; // Normal
        }

        public static T3Float Zero()
        {
            return new T3Float(182, 0);
        }
    }
}