using System;
using System.Diagnostics;

namespace TritTypes
{
    /// <summary>
    /// Represents an 18-trit word for the T3-18 processor.
    /// Stored internally as a signed int (32-bit). 3^18 = 387,420,489 fits in int.
    /// Range: ±(3^18 - 1)/2 = ±193,710,244
    /// </summary>
    [DebuggerDisplay("{ToLong()} ({ToTritString()})")]
    public readonly struct Word18 : IEquatable<Word18>, IT3Word<Word18>
    {
        private readonly int _value;

        public Int128 ToInt128() => _value;

        public long ToLong() => _value;

        public int ToInt() => _value;

        public Word18(int value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Value must be between {MinValue} and {MaxValue}"
                );
            _value = value;
        }

        private Word18(int value, bool raw)
        {
            _value = value;
        }

        private static Word18 FromWrapped(long value) => new Word18(Wrap(value), true);

        private static int Wrap(long value)
        {
            const long range = 387420489;
            const long offset = 193710244;
            long normalized = (value + offset) % range;
            if (normalized < 0)
                normalized += range;
            return (int)(normalized - offset);
        }

        /// <summary>
        /// Constructs a Word18 from a value. Throws exception if value is out of balanced range.
        /// </summary>
        public static Word18 FromExactLong(long value) => new Word18((int)value);

        /// <summary>
        /// Constructs a Word18 from a value with wrap-around semantics.
        /// </summary>
        public static Word18 FromWrappedLong(long value) => FromWrapped(value);

        public static Word18 FromLong(long value) => FromExactLong(value);

        public static Word18 FromInt128(Int128 value) => FromExactLong((long)value);

        static Word18 IT3Word<Word18>.FromLong(long value) => FromLong(value);

        static Word18 IT3Word<Word18>.FromInt128(Int128 value) => FromInt128(value);

        static Word18 IT3Word<Word18>.FromTritString(string s) => Parse(s);

        public static Word18 FromTritString(string s) => Parse(s);

        public static Word18 Zero => new(0);

        private const int TritCount = 18;
        private const int MaxValue = 193710244;
        private const int MinValue = -193710244;

        public string ToTritString()
        {
            var chars = new char[TritCount];
            for (int i = 0; i < TritCount; i++)
            {
                int trit = TernaryMath.ExtractBalancedTrit(_value, i);
                chars[i] = trit switch
                {
                    -1 => '-',
                    0 => '0',
                    1 => '+',
                    _ => '?',
                };
            }
            // We need to return trits from MSB to LSB for the string representation
            Array.Reverse(chars);
            return new string(chars);
        }

        public int GetTrit(int index)
        {
            if (index < 0 || index >= TritCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return TernaryMath.ExtractBalancedTrit(_value, index);
        }

        public override string ToString() => ToTritString();

        public static Word18 Parse(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != TritCount)
                throw new ArgumentException(
                    $"Word18 string must be exactly {TritCount} characters"
                );
            int v = 0,
                p = 1;
            for (int i = TritCount - 1; i >= 0; i--)
            {
                v += s[i] switch
                {
                    '-' => -p,
                    '0' => 0,
                    '+' => p,
                    _ => throw new FormatException($"Invalid trit: '{s[i]}'"),
                };
                p *= 3;
            }
            return new(v);
        }

        // Arithmetic
        public static Word18 operator +(Word18 a, Word18 b) => FromWrapped(a._value + b._value);

        public static Word18 operator -(Word18 a, Word18 b) => FromWrapped(a._value - b._value);

        public static Word18 operator *(Word18 a, Word18 b) =>
            FromWrapped((long)a._value * b._value);

        public static Word18 operator /(Word18 a, Word18 b)
        {
            if (b._value == 0)
                throw new DivideByZeroException();
            int r = a._value / b._value;
            int rem = a._value % b._value;
            if (rem != 0 && ((b._value < 0) != (rem < 0)))
                r--;
            return FromWrapped(r);
        }

        public static Word18 operator %(Word18 a, Word18 b)
        {
            if (b._value == 0)
                throw new DivideByZeroException();
            int r = a._value % b._value;
            if (r != 0 && ((b._value < 0) != (r < 0)))
                r += b._value;
            return FromWrapped(r);
        }

        public static Word18 operator -(Word18 t) => FromWrapped(-t._value);

        public Word18 Negate() => -this;

        // Shifts
        public static Word18 operator <<(Word18 t, int shift)
        {
            if (shift < 0)
                throw new ArgumentOutOfRangeException(nameof(shift));
            return FromWrapped((long)t._value * Pow3(shift));
        }

        public static Word18 operator >>(Word18 t, int shift)
        {
            if (shift < 0)
                throw new ArgumentOutOfRangeException(nameof(shift));
            long d = Pow3(shift);
            int r = (int)(t._value / d);
            long rem = t._value % d;
            if (rem != 0 && ((d < 0) != (rem < 0)))
                r--;
            return FromWrapped(r);
        }

        // Tritwise
        public static Word18 TritAnd(Word18 a, Word18 b) => TritOp(a, b, (x, y) => Math.Min(x, y));

        public static Word18 TritOr(Word18 a, Word18 b) => TritOp(a, b, (x, y) => Math.Max(x, y));

        public static Word18 TritXor(Word18 a, Word18 b) =>
            TritOp(a, b, TernaryMath.BalancedTernaryXor);

        private static Word18 TritOp(Word18 a, Word18 b, Func<int, int, int> op)
        {
            int result = 0,
                power = 1,
                ta = a._value,
                tb = b._value;
            for (int i = 0; i < TritCount; i++)
            {
                int tA = ExtractTrit(ref ta);
                int tB = ExtractTrit(ref tb);
                result += op(tA, tB) * power;
                power *= 3;
            }
            return FromWrapped(result);
        }

        private static int ExtractTrit(ref int v)
        {
            int currentV = v;
            int trit = TernaryMath.ExtractBalancedTrit(currentV, 0);

            // Update v for the next iteration
            int rem = v % 3;
            if (rem == 2 || rem == -1)
                v = (v + 1) / 3;
            else if (rem == -2 || rem == 1)
                v = (v - 1) / 3;
            else
                v /= 3;

            return trit;
        }

        // Equality & comparison
        public override bool Equals(object? obj) => obj is Word18 o && _value == o._value;

        public bool Equals(Word18 other) => _value == other._value;

        public bool Equals(IT3Word<Word18> other) => ToInt128() == other.ToInt128();

        public override int GetHashCode() => _value;

        public static bool operator ==(Word18 l, Word18 r) => l._value == r._value;

        public static bool operator !=(Word18 l, Word18 r) => l._value != r._value;

        public static bool operator <(Word18 l, Word18 r) => l._value < r._value;

        public static bool operator >(Word18 l, Word18 r) => l._value > r._value;

        public static bool operator <=(Word18 l, Word18 r) => l._value <= r._value;

        public static bool operator >=(Word18 l, Word18 r) => l._value >= r._value;

        public static implicit operator Word18(int v) => new(v);

        public static explicit operator int(Word18 w) => w._value;

        private static long Pow3(int exp)
        {
            long r = 1;
            for (int i = 0; i < exp; i++)
                r *= 3;
            return r;
        }
    }
}
