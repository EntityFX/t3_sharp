using System.Diagnostics;
using System.Numerics;

namespace TritTypes
{
    /// <summary>
    /// Represents a 54-trit word for the T3-54 processor.
    /// Range: ±(3⁵⁴−1)/2 ≈ ±2.9·10²⁵
    /// Stored as BigInteger.
    /// </summary>
    [DebuggerDisplay("{ToTritString()}")]
    public readonly struct Word54 : IEquatable<Word54>
    {
        private readonly BigInteger _value;

        private const int TritCount = 54;
        private static readonly BigInteger MaxValue = (BigInteger.Pow(3, 54) - 1) / 2;
        private static readonly BigInteger MinValue = -MaxValue;

        public Word54(BigInteger value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Word54 value must be between {MinValue} and {MaxValue}");
            _value = value;
        }

        public BigInteger ToBigInteger() => _value;

        public static Word54 FromBigInteger(BigInteger value) => new Word54(value);

        /// <summary>
        /// Returns the balanced ternary string representation (54 characters: '-', '0', '+').
        /// </summary>
        public string ToTritString()
        {
            char[] chars = new char[TritCount];
            BigInteger remaining = _value;
            for (int i = TritCount - 1; i >= 0; i--)
            {
                BigInteger rem = remaining % 3;
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

            BigInteger value = 0;
            BigInteger power = 1;
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
            BigInteger result = BigInteger.Divide(a._value, b._value);
            BigInteger rem = a._value % b._value;
            if (rem != 0 && ((b._value.Sign < 0) != (rem.Sign < 0)))
                result--;
            return new Word54(result);
        }
        public static Word54 operator %(Word54 a, Word54 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            BigInteger result = a._value % b._value;
            if (result != 0 && ((b._value.Sign < 0) != (result.Sign < 0)))
                result += b._value;
            return new Word54(result);
        }
        public static Word54 operator -(Word54 t) => new Word54(-t._value);

        // Shift operators
        public static Word54 operator <<(Word54 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            return new Word54(t._value * BigInteger.Pow(3, shift));
        }
        public static Word54 operator >>(Word54 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            BigInteger divisor = BigInteger.Pow(3, shift);
            BigInteger result = BigInteger.Divide(t._value, divisor);
            BigInteger rem = t._value % divisor;
            if (rem != 0 && ((divisor.Sign < 0) != (rem.Sign < 0)))
                result--;
            return new Word54(result);
        }

        // Tritwise logical operations
        private static int GetTrit(BigInteger n, int pos)
        {
            BigInteger power = BigInteger.Pow(3, pos);
            BigInteger digit = (n / power) % 3;
            int d = (int)digit;
            if (d == 2) return -1;
            if (d == -2) return 1;
            return d;
        }

        private static BigInteger SetTrit(BigInteger n, int pos, int tritValue)
        {
            BigInteger power = BigInteger.Pow(3, pos);
            BigInteger current = (n / power) % 3;
            int cur = (int)current;
            if (cur == 2) cur = -1;
            else if (cur == -2) cur = 1;
            return n + (tritValue - cur) * power;
        }

        public static Word54 TritAnd(Word54 a, Word54 b)
        {
            BigInteger result = 0;
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
            BigInteger result = 0;
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
            BigInteger result = 0;
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
        public override bool Equals(object? obj) => obj is Word54 other && _value == other._value;
        public bool Equals(Word54 other) => _value == other._value;
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Word54 left, Word54 right) => left._value == right._value;
        public static bool operator !=(Word54 left, Word54 right) => left._value != right._value;
        public static bool operator <(Word54 left, Word54 right) => left._value < right._value;
        public static bool operator >(Word54 left, Word54 right) => left._value > right._value;
        public static bool operator <=(Word54 left, Word54 right) => left._value <= right._value;
        public static bool operator >=(Word54 left, Word54 right) => left._value >= right._value;

        public static implicit operator Word54(long value) => new Word54(value);
        public static explicit operator long(Word54 w) => (long)w._value;
        public static explicit operator BigInteger(Word54 w) => w._value;
    }
}