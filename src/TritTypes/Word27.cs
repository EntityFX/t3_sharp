using System.Diagnostics;
using System.Numerics;

namespace TritTypes
{
    /// <summary>
    /// Represents a 27-trit word for the T3-27 processor.
    /// Stored internally as a signed long (64-bit, enough for 27 trits).
    /// Range: ±(3²⁷−1)/2 ≈ ±3.8·10¹²
    /// </summary>
    [DebuggerDisplay("{ToLong()} ({ToTritString()})")]
    public readonly struct Word27 : IEquatable<Word27>
    {
        private readonly BigInteger _value;

        private const int TritCount = 27;
        private static readonly BigInteger MaxValue = (Pow3(27) - 1) / 2;
        private static readonly BigInteger MinValue = -MaxValue;

        public Word27(BigInteger value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Word27 value must be between {MinValue} and {MaxValue}");
            _value = value;
        }

        public BigInteger ToLong() => _value;

        public static Word27 FromLong(BigInteger value) => new Word27(value);

        /// <summary>
        /// Returns the balanced ternary string representation (27 characters: '-', '0', '+').
        /// </summary>
        public string ToTritString()
        {
            char[] chars = new char[TritCount];
            var remaining = _value;
            for (int i = TritCount - 1; i >= 0; i--)
            {
                var rem = remaining % 3;
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
        /// Parse a balanced ternary string into a Word27.
        /// </summary>
        public static Word27 Parse(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != TritCount)
                throw new ArgumentException($"Word27 string must be exactly {TritCount} characters");

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
            return new Word27(value);
        }

        // Arithmetic operators
        public static Word27 operator +(Word27 a, Word27 b) => new Word27(a._value + b._value);
        public static Word27 operator -(Word27 a, Word27 b) => new Word27(a._value - b._value);
        public static Word27 operator *(Word27 a, Word27 b) => new Word27(a._value * b._value);
        public static Word27 operator /(Word27 a, Word27 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            // Floor division (round toward -inf) for balanced ternary
            var result = a._value / b._value;
            var rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                result--;
            return new Word27(result);
        }
        public static Word27 operator %(Word27 a, Word27 b)
        {
            if (b._value == 0) throw new DivideByZeroException();
            var result = a._value % b._value;
            if (result != 0 && ((b._value < 0) != (result < 0)))
                result += b._value;
            return new Word27(result);
        }
        public static Word27 operator -(Word27 t) => new Word27(-t._value);

        // Shift operators (balanced ternary shifts: multiply/divide by 3^shift)
        public static Word27 operator <<(Word27 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            return new Word27(t._value * Pow3(shift));
        }
        public static Word27 operator >>(Word27 t, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            // Arithmetic right shift in balanced ternary
            var divisor = Pow3(shift);
            var result = t._value / divisor;
            var rem = t._value % divisor;
            if (rem != 0 && ((divisor < 0) != (rem < 0)))
                result--;
            return new Word27(result);
        }

        // Tritwise logical operations
        public static Word27 TritAnd(Word27 a, Word27 b)
        {
            BigInteger result = 0;
            BigInteger power = 1;
            BigInteger ta = a._value, tb = b._value;
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
            return new Word27(result);
        }

        public static Word27 TritOr(Word27 a, Word27 b)
        {
            BigInteger result = 0;
            BigInteger power = 1;
            BigInteger ta = a._value, tb = b._value;
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
            return new Word27(result);
        }

        public static Word27 TritXor(Word27 a, Word27 b)
        {
            BigInteger result = 0;
            BigInteger power = 1;
            BigInteger ta = a._value, tb = b._value;
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
            return new Word27(result);
        }

        // Comparison
        public override bool Equals(object? obj) => obj is Word27 other && _value == other._value;
        public bool Equals(Word27 other) => _value == other._value;
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Word27 left, Word27 right) => left._value == right._value;
        public static bool operator !=(Word27 left, Word27 right) => left._value != right._value;
        public static bool operator <(Word27 left, Word27 right) => left._value < right._value;
        public static bool operator >(Word27 left, Word27 right) => left._value > right._value;
        public static bool operator <=(Word27 left, Word27 right) => left._value <= right._value;
        public static bool operator >=(Word27 left, Word27 right) => left._value >= right._value;

        public static implicit operator Word27(long value) => new Word27(value);
        public static explicit operator BigInteger(Word27 w) => w._value;

        private static long Pow3(int exp)
        {
            long result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }
    }
}