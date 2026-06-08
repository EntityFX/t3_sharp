using System.Diagnostics;
using System;

namespace TritTypes
{
    /// <summary>
    /// Represents an 18-trit word for the T3-18 processor.
    /// Stored internally as a signed long (64-bit).
    /// Range: ±(3¹⁸−1)/2 = ±193,710,244
    /// </summary>
    [DebuggerDisplay("{ToLong()} ({ToTritString()})")]
    public readonly struct Word18 : IEquatable<Word18>, IT3Word<Word18>
    {
        private readonly long _value;

        public Int128 ToInt128() => _value;
        public long ToLong() => _value;

        public Word18(long value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Word18 value must be between {MinValue} and {MaxValue}");
            _value = value;
        }

        public static Word18 FromLong(long value) => new Word18(value);
        public static Word18 FromInt128(Int128 value) => new Word18((long)value);
        
        static Word18 IT3Word<Word18>.FromLong(long value) => Word18.FromLong(value);
        static Word18 IT3Word<Word18>.FromInt128(Int128 value) => Word18.FromInt128(value);

        static Word18 IT3Word<Word18>.FromTritString(string tritString) => Parse(tritString);


        public static Word18 FromTritString(string tritString) => Parse(tritString);

        public static Word18 Zero => new Word18(0);

        private const int TritCount = 18;
        private static readonly long MaxValue = 193710244;
        private static readonly long MinValue = -193710244;

        /// <summary>
        /// Returns the balanced ternary string representation (18 characters: '-', '0', '+').
        /// </summary>
        public string ToTritString()
        {
            char[] chars = new char[TritCount];
            long remaining = _value;
            for (int i = TritCount - 1; i >= 0; i--)
            {
                long rem = remaining % 3;
                if (rem == 2) { chars[i] = '-'; remaining = (remaining + 1) / 3; }
                else if (rem == -2) { chars[i] = '+'; remaining = (remaining - 1) / 3; }
                else if (rem == 1) { chars[i] = '+'; remaining = (remaining - 1) / 3; }
                else if (rem == -1) { chars[i] = '-'; remaining = (remaining + 1) / 3; }
                else { chars[i] = '0'; remaining /= 3; }
            }
            return new string(chars);
        }

        public override string ToString() => ToTritString();

        /// <summary>
        /// Parse a balanced ternary string into a Word18.
        /// </summary>
        public static Word18 Parse(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != TritCount)
                throw new ArgumentException($"Word18 string must be exactly {TritCount} characters");

            long value = 0;
            long power = 1;
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
            return new Word18(value);
        }

        // Arithmetic operators
        public static Word18 operator +(Word18 a, Word18 b) => new Word18(a._value + b._value);
        public static Word18 operator -(Word18 a, Word18 b) => new Word18(a._value - b._value);
        public static Word18 operator *(Word18 a, Word18 b) => new Word18(a._value * b._value);
        public static Word18 operator /(Word18 a, Word18 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            long result = a._value / b._value;
            long rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                result--;
            return new Word18(result);
        }
        public static Word18 operator %(Word18 a, Word18 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            long result = a._value % b._value;
            if (result != 0 && ((b._value < 0) != (result < 0)))
                result += b._value;
            return new Word18(result);
        }
        public static Word18 operator -(Word18 t) => new Word18(-t._value);

        public Word18 Negate() => -this;

        // Shift operators
        public static Word18 operator <<(Word18 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            return new Word18(t._value * Pow3(shift));
        }
        public static Word18 operator >>(Word18 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            long divisor = Pow3(shift);
            long result = t._value / divisor;
            long rem = t._value % divisor;
            if (rem != 0 && ((divisor < 0) != (rem < 0)))
                result--;
            return new Word18(result);
        }

        // Tritwise logical operations
        public static Word18 TritAnd(Word18 a, Word18 b)
        {
            long result = 0;
            long power = 1;
            long ta = a._value, tb = b._value;
            for (int i = 0; i < TritCount; i++)
            {
                int tritA = (int)(ta % 3); if (tritA == 2) tritA = -1; else if (tritA == -2) tritA = 1;
                int tritB = (int)(tb % 3); if (tritB == 2) tritB = -1; else if (tritB == -2) tritB = 1;
                int min = Math.Min(tritA, tritB);
                result += min * power;
                ta = (ta - (ta % 3)) / 3;
                tb = (tb - (tb % 3)) / 3;
                power *= 3;
            }
            return new Word18(result);
        }

        public static Word18 TritOr(Word18 a, Word18 b)
        {
            long result = 0;
            long power = 1;
            long ta = a._value, tb = b._value;
            for (int i = 0; i < TritCount; i++)
            {
                int tritA = (int)(ta % 3); if (tritA == 2) tritA = -1; else if (tritA == -2) tritA = 1;
                int tritB = (int)(tb % 3); if (tritB == 2) tritB = -1; else if (tritB == -2) tritB = 1;
                int max = Math.Max(tritA, tritB);
                result += max * power;
                ta = (ta - (ta % 3)) / 3;
                tb = (tb - (tb % 3)) / 3;
                power *= 3;
            }
            return new Word18(result);
        }

        public static Word18 TritXor(Word18 a, Word18 b)
        {
            long result = 0;
            long power = 1;
            long ta = a._value, tb = b._value;
            for (int i = 0; i < TritCount; i++)
            {
                int tritA = (int)(ta % 3); if (tritA == 2) tritA = -1; else if (tritA == -2) tritA = 1;
                int tritB = (int)(tb % 3); if (tritB == 2) tritB = -1; else if (tritB == -2) tritB = 1;
                int sum = tritA + tritB;
                if (sum > 1) sum -= 3;
                if (sum < -1) sum += 3;
                result += sum * power;
                ta = (ta - (ta % 3)) / 3;
                tb = (tb - (tb % 3)) / 3;
                power *= 3;
            }
            return new Word18(result);
        }

        public override bool Equals(object? obj) => obj is Word18 other && ToInt128() == other.ToInt128();
        public bool Equals(Word18 other) => _value == other._value;
        public bool Equals(IT3Word<Word18> other) => ToInt128() == other.ToInt128();
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Word18 left, Word18 right) => left._value == right._value;
        public static bool operator !=(Word18 left, Word18 right) => left._value != right._value;
        public static bool operator <(Word18 left, Word18 right) => left._value < right._value;
        public static bool operator >(Word18 left, Word18 right) => left._value > right._value;
        public static bool operator <=(Word18 left, Word18 right) => left._value <= right._value;
        public static bool operator >=(Word18 left, Word18 right) => left._value >= right._value;

        public static implicit operator Word18(long value) => new Word18(value);
        public static explicit operator long(Word18 w) => w._value;

        private static long Pow3(int exp)
        {
            long result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }
    }
}