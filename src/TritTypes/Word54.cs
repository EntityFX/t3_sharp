using System.Diagnostics;
using System;

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
                throw new ArgumentOutOfRangeException(nameof(value), $"Word54 value must be between {MinValue} and {MaxValue}");
            _value = value;
        }

        public Int128 ToInt128() => _value;

        public static Word54 FromLong(long value) => new Word54((Int128)value);
        public static Word54 FromInt128(Int128 value) => new Word54(value);

        static Word54 IT3Word<Word54>.FromLong(long value) => FromLong(value);
        static Word54 IT3Word<Word54>.FromInt128(Int128 value) => FromInt128(value);
        public static Word54 Zero => new Word54(0);

        /// <summary>
        /// Returns the balanced ternary string representation (54 characters: '-', '0', '+').
        /// </summary>
        public string ToTritString()
        {
            char[] chars = new char[TritCount];
            Int128 remaining = _value;
            for (int i = TritCount - 1; i >= 0; i--)
            {
                Int128 rem = remaining % 3;
                int remInt = (int)rem;
                if (remInt == 2) { chars[i] = '-'; remaining = (remaining + 1) / 3; }
                else if (remInt == -2) { chars[i] = '+'; remaining = (remaining - 1) / 3; }
                else if (remInt == 1) { chars[i] = '+'; remaining = (remaining - 1) / 3; }
                else if (remInt == -1) { chars[i] = '-'; remaining = (remaining + 1) / 3; }
                else { chars[i] = '0'; remaining /= 3; }
            }
            return new string(chars);
        }

        public override string ToString() => ToTritString();

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
        public static Word54 operator +(Word54 a, Word54 b) => new Word54(a._value + b._value);
        public static Word54 operator -(Word54 a, Word54 b) => new Word54(a._value - b._value);
        public static Word54 operator *(Word54 a, Word54 b) => new Word54(a._value * b._value);
        public static Word54 operator /(Word54 a, Word54 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            Int128 result = a._value / b._value;
            Int128 rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                result--;
            return new Word54(result);
        }
        public static Word54 operator %(Word54 a, Word54 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            Int128 result = a._value % b._value;
            Int128 rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                result += b._value;
            return new Word54(result);
        }
        public static Word54 operator -(Word54 t) => new Word54(-t._value);

        public Word54 Negate() => -this;

        // Shift operators
        public static Word54 operator <<(Word54 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            return new Word54(t._value * Pow3(shift));
        }
        public static Word54 operator >>(Word54 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            Int128 divisor = Pow3(shift);
            Int128 result = t._value / divisor;
            Int128 rem = t._value % divisor;
            if (rem != 0 && ((divisor < 0) != (rem < 0)))
                result--;
            return new Word54(result);
        }

        // Tritwise logical operations
        private static int GetTrit(Int128 n, int pos)
        {
            Int128 power = Pow3(pos);
            Int128 digit = (n / power) % 3;
            int d = (int)digit;
            if (d == 2) return -1;
            if (d == -2) return 1;
            return d;
        }

        private static Int128 SetTrit(Int128 n, int pos, int tritValue)
        {
            Int128 power = Pow3(pos);
            Int128 current = (n / power) % 3;
            int cur = (int)current;
            if (cur == 2) cur = -1;
            else if (cur == -2) cur = 1;
            return n + (tritValue - cur) * power;
        }

        public static Word54 TritAnd(Word54 a, Word54 b)
        {
            Int128 result = 0;
            for (int i = 0; i < TritCount; i++)
            {
                int ta = GetTrit(a._value, i);
                int tb = GetTrit(b._value, i);
                result = SetTrit(result, i, Math.Min(ta, tb));
            }
            return new Word54(result);
        }

        public static Word54 TritOr(Word54 a, Word54 b)
        {
            Int128 result = 0;
            for (int i = 0; i < TritCount; i++)
            {
                int ta = GetTrit(a._value, i);
                int tb = GetTrit(b._value, i);
                result = SetTrit(result, i, Math.Max(ta, tb));
            }
            return new Word54(result);
        }

        public static Word54 TritXor(Word54 a, Word54 b)
        {
            Int128 result = 0;
            for (int i = 0; i < TritCount; i++)
            {
                int ta = GetTrit(a._value, i);
                int tb = GetTrit(b._value, i);
                int sum = ta + tb;
                if (sum > 1) sum -= 3;
                if (sum < -1) sum += 3;
                result = SetTrit(result, i, sum);
            }
            return new Word54(result);
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

        private static Int128 Pow3(int exp)
        {
            Int128 result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }
    }
}