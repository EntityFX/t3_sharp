using System.Diagnostics;
using System;
using System.Numerics;

namespace TritTypes
{
    /// <summary>
    /// Represents a 54-trit word for the T3-54 processor.
    /// Range: ±(3⁵⁴−1)/2 ≈ ±2.9·10²⁵
    /// Stored as Int128.
    /// </summary>
    [DebuggerDisplay("{ToTritString()}")]
    public readonly struct Word54 : IEquatable<Word54>, IT3Word<Word54>
    {
        private readonly Int128 _value;

        private const int TritCount = 54;
        private static readonly Int128 MaxValue = (Pow3(54) - 1) / 2;
        private static readonly Int128 MinValue = -MaxValue;

        public Word54(Int128 value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Value must be between {MinValue} and {MaxValue}");
            _value = value;
        }

        private Word54(Int128 value, bool raw)
        {
            _value = value;
        }

        private static Word54 FromWrapped(Int128 value) => new Word54(Wrap(value), true);

        private static Int128 Wrap(Int128 value)
        {
            Int128 range = Pow3(54);
            Int128 offset = (range - 1) / 2;
            Int128 normalized = (value + offset) % range;
            if (normalized < 0) normalized += range;
            return normalized - offset;
        }

        public Int128 ToInt128() => _value;
        public int ToInt() => (int)(long)_value;
        public long ToLong() => (long)_value;

        /// <summary>
        /// Returns the value as a long, but ensures it fits within the 18-trit range.
        /// Useful for memory addressing and instruction extraction.
        /// </summary>
        public long ToAddress()
        {
            // We use the modulo of 3^18 to extract the lower 18 trits
            Int128 range = TernaryMath.Pow3(18);
            Int128 offset = (range - 1) / 2;
            Int128 val = _value;
            
            Int128 normalized = (val + offset) % range;
            if (normalized < 0) normalized += range;
            
            return (long)(normalized - offset);
        }

        public static Word54 FromLong(long value) => new Word54((Int128)value);
        public static Word54 FromInt128(Int128 value) => new Word54(value);

        static Word54 IT3Word<Word54>.FromLong(long value) => FromLong(value);
        static Word54 IT3Word<Word54>.FromInt128(Int128 value) => FromInt128(value);

        static Word54 IT3Word<Word54>.FromTritString(string tritString) => Parse(tritString);
        public static Word54 Zero => new Word54(0);

        /// <summary>
        /// Returns the balanced ternary string representation (54 characters: '-', '0', '+').
        /// </summary>
        public string ToTritString()
        {
            char[] chars = new char[TritCount];
            for (int i = 0; i < TritCount; i++)
            {
                int trit = TernaryMath.ExtractBalancedTrit(_value, i);
                chars[i] = trit switch { -1 => '-', 0 => '0', 1 => '+', _ => '?' };
            }
            Array.Reverse(chars);
            return new string(chars);
        }

        public override string ToString() => ToTritString();

        public int GetTrit(int index)
        {
            if (index < 0 || index >= TritCount) throw new ArgumentOutOfRangeException(nameof(index));
            return TernaryMath.ExtractBalancedTrit(_value, index);
        }

        /// <summary>
        /// Parse a balanced ternary string into a Word54.
        /// </summary>
        public static Word54 Parse(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != TritCount)
                throw new ArgumentException($"Word54 string must be exactly {TritCount} characters");

            Int128 value = 0;
            Int128 power = 1;
            for (int i = TritCount - 1; i >= 0; i--)
            {
                value += s[i] switch
                {
                    '-' => -power,
                    '0' => 0,
                    '+' => power,
                    _ => throw new FormatException($"Invalid trit character: '{s[i]}'")
                };
                power *= 3;
            }
            return new Word54(value);
        }

        // Arithmetic operators
        public static Word54 operator +(Word54 a, Word54 b) => FromWrapped(a._value + b._value);
        public static Word54 operator -(Word54 a, Word54 b) => FromWrapped(a._value - b._value);
        public static Word54 operator *(Word54 a, Word54 b) => FromWrapped((Int128)((BigInteger)a._value * (BigInteger)b._value));
        public static Word54 operator /(Word54 a, Word54 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            Int128 result = a._value / b._value;
            Int128 rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                result--;
            return FromWrapped(result);
        }
        public static Word54 operator %(Word54 a, Word54 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            Int128 result = a._value % b._value;
            Int128 rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                result += b._value;
            return FromWrapped(result);
        }
        public static Word54 operator -(Word54 t) => FromWrapped(-t._value);

        public Word54 Negate() => -this;

        // Shift operators
        public static Word54 operator <<(Word54 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            return FromWrapped((Int128)((BigInteger)t._value * (BigInteger)Pow3(shift)));
        }
        public static Word54 operator >>(Word54 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            Int128 divisor = Pow3(shift);
            Int128 result = t._value / divisor;
            Int128 rem = t._value % divisor;
            if (rem != 0 && ((divisor < 0) != (rem < 0)))
                result--;
            return FromWrapped(result);
        }

        // Tritwise logical operations
        // Deprecated: use TernaryMath.ExtractBalancedTrit
        [Obsolete("Use TernaryMath.ExtractBalancedTrit instead")]
        private static int GetTrit(Int128 n, int pos)
        {
            return TernaryMath.ExtractBalancedTrit(n, pos);
        }

        private static Int128 SetTrit(Int128 n, int pos, int tritValue)
        {
            Int128 power = TernaryMath.Pow3(pos);
            int currentTrit = TernaryMath.ExtractBalancedTrit(n, pos);
            return n + (tritValue - currentTrit) * power;
        }

        public static Word54 TritAnd(Word54 a, Word54 b)
        {
            Int128 result = 0;
            for (int i = 0; i < TritCount; i++)
            {
                int ta = TernaryMath.ExtractBalancedTrit(a._value, i);
                int tb = TernaryMath.ExtractBalancedTrit(b._value, i);
                result = SetTrit(result, i, Math.Min(ta, tb));
            }
            return FromWrapped(result);
        }

        public static Word54 TritOr(Word54 a, Word54 b)
        {
            Int128 result = 0;
            for (int i = 0; i < TritCount; i++)
            {
                int ta = TernaryMath.ExtractBalancedTrit(a._value, i);
                int tb = TernaryMath.ExtractBalancedTrit(b._value, i);
                result = SetTrit(result, i, Math.Max(ta, tb));
            }
            return FromWrapped(result);
        }

        public static Word54 TritXor(Word54 a, Word54 b)
        {
            Int128 result = 0;
            for (int i = 0; i < TritCount; i++)
            {
                int ta = TernaryMath.ExtractBalancedTrit(a._value, i);
                int tb = TernaryMath.ExtractBalancedTrit(b._value, i);
                int resTrit = TernaryMath.BalancedTernaryXor(ta, tb);
                result = SetTrit(result, i, resTrit);
            }
            return FromWrapped(result);
        }

        // Comparison
        public override bool Equals(object? obj) => obj is Word54 other && ToInt128() == other.ToInt128();
        public bool Equals(Word54 other) => _value == other._value;
        public bool Equals(IT3Word<Word54> other) => ToInt128() == other.ToInt128();
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Word54 left, Word54 right) => left._value == right._value;
        public static bool operator !=(Word54 left, Word54 right) => left._value != right._value;
        public static bool operator <(Word54 left, Word54 right) => left._value < right._value;
        public static bool operator >(Word54 left, Word54 right) => left._value > right._value;
        public static bool operator <=(Word54 left, Word54 right) => left._value <= right._value;
        public static bool operator >=(Word54 left, Word54 right) => left._value >= right._value;

        public static implicit operator Word54(long value) => new Word54((Int128)value);
        public static explicit operator long(Word54 w) => (long)w._value;
        public static explicit operator Int128(Word54 w) => w._value;

        private static Int128 Pow3(int exp) => TernaryMath.Pow3(exp);
    }
}