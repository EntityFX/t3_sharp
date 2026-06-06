using System.Diagnostics;

namespace TritTypes
{
    /// <summary>
    /// Represents a tryte — 6 trits, the minimum addressable memory cell.
    /// Range: -364 to +364 in balanced ternary.
    /// </summary>
    [DebuggerDisplay("{ToLong()} ({ToString()})")]
    public readonly struct Tryte : IEquatable<Tryte>
    {
        private readonly short _value; // stored as signed integer for efficiency

        private const int MinValue = -364;
        private const int MaxValue = 364;

        public Tryte(short value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Tryte value must be between {MinValue} and {MaxValue}");
            _value = value;
        }

        public Tryte(int value) : this((short)value) { }

        public long ToLong() => _value;

        public static Tryte FromLong(long value) => new Tryte((short)value);

        public override string ToString()
        {
            char[] chars = new char[6];
            long remaining = _value;
            for (int i = 5; i >= 0; i--)
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

        public static Tryte Parse(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 6)
                throw new ArgumentException("Tryte string must be exactly 6 characters");

            long value = 0;
            long power = 1;
            for (int i = 5; i >= 0; i--)
            {
                value += s[i] switch
                {
                    '-' => -power,
                    '0' => 0,
                    '+' => power,
                    _ => throw new FormatException($"Invalid tryte character: '{s[i]}'")
                };
                power *= 3;
            }
            return new Tryte((short)value);
        }

        public override bool Equals(object? obj) => obj is Tryte other && _value == other._value;
        public bool Equals(Tryte other) => _value == other._value;
        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Tryte left, Tryte right) => left._value == right._value;
        public static bool operator !=(Tryte left, Tryte right) => left._value != right._value;

        public static Tryte operator +(Tryte a, Tryte b) => new Tryte((short)(a._value + b._value));
        public static Tryte operator -(Tryte a, Tryte b) => new Tryte((short)(a._value - b._value));
        public static Tryte operator *(Tryte a, Tryte b) => new Tryte((short)(a._value * b._value));
        public static Tryte operator -(Tryte t) => new Tryte((short)(-t._value));

        public static implicit operator Tryte(int value) => new Tryte(value);
        public static explicit operator int(Tryte t) => t._value;
        public static explicit operator short(Tryte t) => t._value;
    }
}