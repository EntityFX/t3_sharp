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
            long pow12 = (long)TernaryMath.Pow3(12);
            long raw = word.ToLong();
            long exponent = raw / pow12;
            long mantissa = raw % pow12;

            // Correct for C# truncated division to keep mantissa within [-maxMant, maxMant]
            // where maxMant = (3^12 - 1) / 2 = 265720.
            const long maxMant = 265720;
            if (mantissa > maxMant)
            {
                exponent++;
                mantissa -= pow12;
            }
            else if (mantissa < -maxMant)
            {
                exponent--;
                mantissa += pow12;
            }

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
            
            // Normalize for maximum precision:
            // Start with |mant| ≈ 1, then decrease exponent while |mant| ≤ maxMant.
            // This uses all 12 trits of mantissa for maximum precision.
            const long maxMant = 265720; // (3^12 - 1) / 2
            double absVal = Math.Abs(value);
            int sign = value < 0 ? -1 : 1;
            
            // Start with exponent that gives |mant| ≈ 1
            int exp = (int)Math.Round(Math.Log(absVal, 3));
            
            // Clamp exponent to 6-trit range [-182, +182]
            if (exp < -182) exp = -182;
            if (exp > 182) exp = 182;
            
            // Decrease exponent while mantissa fits in 12 trits (max precision)
            long mantissa;
            while (true)
            {
                mantissa = (long)Math.Round(absVal / Math.Pow(3, exp), MidpointRounding.AwayFromZero);
                if (mantissa > maxMant || mantissa < -maxMant)
                {
                    // Overflow: increase exponent
                    exp++;
                    mantissa = (long)Math.Round(absVal / Math.Pow(3, exp), MidpointRounding.AwayFromZero);
                    break;
                }
                if (exp <= -182) break; // Hit minimum exponent
                // Check if next lower exponent would overflow
                long nextMant = (long)Math.Round(absVal / Math.Pow(3, exp - 1), MidpointRounding.AwayFromZero);
                if (nextMant > maxMant || nextMant < -maxMant) break;
                exp--;
            }
            
            return new T3Float(exp + 182, mantissa * sign);
        }

        public static T3Float FromInt(int value)
        {
            return FromDouble(value);
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
            
            // Normalize for maximum precision:
            // Start with |mant| ≈ 1, then decrease exponent while |mant| ≤ maxMant.
            // This uses all 28 trits of mantissa for maximum precision.
            Int128 maxMant = (Int128)(TernaryMath.Pow3(28) - 1) / 2;
            double absVal = Math.Abs(value);
            int sign = value < 0 ? -1 : 1;
            
            // Start with exponent that gives |mant| ≈ 1
            int exp = (int)Math.Round(Math.Log(absVal, 3));
            
            // Clamp exponent to 8-trit range
            if (exp < -3280) exp = -3280;
            if (exp > 3280) exp = 3280;
            
            // Decrease exponent while mantissa fits in 28 trits (max precision)
            Int128 mantissa;
            while (true)
            {
                mantissa = (Int128)Math.Round(absVal / Math.Pow(3, exp), MidpointRounding.AwayFromZero);
                if (mantissa > maxMant || mantissa < -maxMant)
                {
                    // Overflow: increase exponent
                    exp++;
                    mantissa = (Int128)Math.Round(absVal / Math.Pow(3, exp), MidpointRounding.AwayFromZero);
                    break;
                }
                if (exp <= -3280) break; // Hit minimum exponent
                // Check if next lower exponent would overflow
                Int128 nextMant = (Int128)Math.Round(absVal / Math.Pow(3, exp - 1), MidpointRounding.AwayFromZero);
                if (nextMant > maxMant || nextMant < -maxMant) break;
                exp--;
            }
            
            return new T3Double(exp + 3280, mantissa * sign);
        }
    }
}