using System;

namespace TritTypes
{
    /// <summary>
    /// Represents a ternary floating point number (tfloat - 18 trits).
    /// Format: 6 trits exponent, 12 trits mantissa. Bias = 182.
    /// </summary>
    public struct T3Float
    {
        public long Exponent { get; } // Biased exponent
        public long Mantissa { get; } // Normalized mantissa

        public T3Float(long exponent, long mantissa)
        {
            Exponent = exponent;
            Mantissa = mantissa;
        }

        public static T3Float FromWord18(Word18 word)
        {
            string s = word.ToTritString();
            string expPart = s.Substring(0, 6);
            string mantPart = s.Substring(6, 12);
            
            return new T3Float(
                BalancedTernary.ParseToLong(expPart),
                BalancedTernary.ParseToLong(mantPart)
            );
        }

        public Word18 ToWord18()
        {
            string s = BalancedTernary.ToTernaryString(Exponent, 6) + 
                       BalancedTernary.ToTernaryString(Mantissa, 12);
            return Word18.FromTritString(s);
        }

        public double ToDouble()
        {
            if (Mantissa == 0) return 0.0;
            
            // Value = Mantissa * 3^(Exponent - Bias)
            // Note: This is a simplified conversion. 
            // Real ternary float normalization usually involves the first trit of mantissa being +1.
            double exp = Exponent - 182;
            return Mantissa * Math.Pow(3, exp);
        }

        public static T3Float FromDouble(double value)
        {
            if (value == 0) return new T3Float(182, 0);
            
            // Extremely simplified conversion for simulation purposes
            // In a real FPU, this would involve normalization and rounding.
            double log3 = Math.Log(3);
            double exp = Math.Round(Math.Log(Math.Abs(value), 3));
            long mantissa = (long)Math.Round(value / Math.Pow(3, exp));
            
            return new T3Float((long)exp + 182, mantissa);
        }
    }

    /// <summary>
    /// Represents a ternary double precision floating point number (tdouble - 36 trits).
    /// Format: 8 trits exponent, 28 trits mantissa. Bias = 3280.
    /// </summary>
    public struct T3Double
    {
        public long Exponent { get; } // Biased exponent
        public Int128 Mantissa { get; } // Normalized mantissa

        public T3Double(long exponent, Int128 mantissa)
        {
            Exponent = exponent;
            Mantissa = mantissa;
        }

        public static T3Double FromTritString(string s)
        {
            if (s.Length != 36) throw new ArgumentException("tdouble requires 36 trits");
            return new T3Double(
                BalancedTernary.ParseToLong(s.Substring(0, 8)),
                BalancedTernary.ParseToInt128(s.Substring(8, 28))
            );
        }

        public string ToTritString()
        {
            return BalancedTernary.ToTernaryString(Exponent, 8) + 
                   BalancedTernary.ToTernaryString(Mantissa, 28);
        }

        public double ToDouble()
        {
            if (Mantissa == 0) return 0.0;
            double exp = Exponent - 3280;
            return (double)Mantissa * Math.Pow(3, exp);
        }

        public static T3Double FromDouble(double value)
        {
            if (value == 0) return new T3Double(3280, 0);
            double exp = Math.Round(Math.Log(Math.Abs(value), 3));
            Int128 mantissa = (Int128)Math.Round(value / Math.Pow(3, exp));
            return new T3Double((long)exp + 3280, mantissa);
        }
    }
}