using System.Diagnostics;

namespace TritTypes
{
    /// <summary>
    /// Represents a balanced ternary trit with values -1, 0, or +1.
    /// </summary>
    [DebuggerDisplay("{ToChar()}")]
    public readonly struct Trit : IEquatable<Trit>, IComparable<Trit>
    {
        private readonly sbyte _value;

        private Trit(sbyte value)
        {
            if (value < -1 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Trit value must be -1, 0, or +1");
            _value = value;
        }

        public static readonly Trit MinusOne = new Trit(-1);
        public static readonly Trit Zero = new Trit(0);
        public static readonly Trit PlusOne = new Trit(1);

        public int Value => _value;

        public static Trit FromInt(int value) => value switch
        {
            -1 => MinusOne,
            0 => Zero,
            1 => PlusOne,
            _ => throw new ArgumentOutOfRangeException(nameof(value), "Trit value must be -1, 0, or +1")
        };

        public static Trit FromChar(char c) => c switch
        {
            '-' => MinusOne,
            '0' => Zero,
            '+' => PlusOne,
            _ => throw new ArgumentException($"Invalid trit character: '{c}'. Expected '-', '0', or '+'.")
        };

        public char ToChar() => _value switch
        {
            -1 => '-',
            0 => '0',
            1 => '+',
            _ => '?'
        };

        public override string ToString() => ToChar().ToString();

        public override bool Equals(object? obj) => obj is Trit other && _value == other._value;
        public bool Equals(Trit other) => _value == other._value;
        public override int GetHashCode() => _value.GetHashCode();

        public int CompareTo(Trit other) => _value.CompareTo(other._value);

        public static bool operator ==(Trit left, Trit right) => left._value == right._value;
        public static bool operator !=(Trit left, Trit right) => left._value != right._value;

        // Arithmetic operators
        public static Trit operator +(Trit a, Trit b)
        {
            int sum = a._value + b._value;
            if (sum > 1) sum = -1; // wrap: 1+1 = -1 (balanced ternary wrap)
            if (sum < -1) sum = 1;  // wrap: -1+-1 = 1
            return FromInt(sum);
        }

        public static Trit operator -(Trit a, Trit b)
        {
            int diff = a._value - b._value;
            if (diff > 1) diff = -1;
            if (diff < -1) diff = 1;
            return FromInt(diff);
        }

        public static Trit operator *(Trit a, Trit b) => FromInt((sbyte)(a._value * b._value));

        public static Trit operator -(Trit t) => FromInt((sbyte)(-t._value));

        // Logical operators (tritwise min/max/sum mod 3)
        public static Trit TritAnd(Trit a, Trit b) => FromInt(Math.Min(a._value, b._value));
        public static Trit TritOr(Trit a, Trit b) => FromInt(Math.Max(a._value, b._value));
        public static Trit TritXor(Trit a, Trit b)
        {
            int sum = a._value + b._value;
            if (sum > 1) sum -= 3;
            if (sum < -1) sum += 3;
            return FromInt(sum);
        }

        // Implicit conversion from int (for convenience in tests)
        public static implicit operator Trit(int value) => FromInt(value);

        // Explicit conversion to int
        public static explicit operator int(Trit t) => t._value;
    }
}