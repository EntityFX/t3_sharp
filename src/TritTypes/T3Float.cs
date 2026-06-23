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
            // T3Float format: exponent in upper 6 trits (positions 12-17), mantissa in lower 12 trits (positions 0-11).
            // The combined value = exponent * 3^12 + mantissa (direct linear encoding).
            // Division and modulo correctly separate the fields because the encoding is linear.
            long pow12 = (long)TernaryMath.Pow3(12);
            long raw = word.ToLong();
            long exponent = raw / pow12;
            long mantissa = raw % pow12;
            return new T3Float(exponent, mantissa);
        }

        public Word18 ToWord18()
        {
            // T3Float format: exponent in upper 6 trits (positions 12-17), mantissa in lower 12 trits (positions 0-11).
            // Direct linear encoding: value = exponent * 3^12 + mantissa.
            long encoded = Exponent * (long)TernaryMath.Pow3(12) + Mantissa;
            return Word18.FromLong(encoded);
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
            // For integer values within Word18 range, use exact exponent=182
            if (value == Math.Floor(value) && value >= -193710244 && value <= 193710244)
                return new T3Float(182, (long)value);
            // For other values, use logarithmic decomposition
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