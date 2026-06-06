namespace TritTypes
{
    /// <summary>
    /// Utility class for trit-level array operations.
    /// </summary>
    public static class TritArray
    {
        /// <summary>
        /// Tritwise AND (minimum) of two trit arrays.
        /// </summary>
        public static Trit[] And(Trit[] a, Trit[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Arrays must have the same length");
            Trit[] result = new Trit[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = Trit.TritAnd(a[i], b[i]);
            return result;
        }

        /// <summary>
        /// Tritwise OR (maximum) of two trit arrays.
        /// </summary>
        public static Trit[] Or(Trit[] a, Trit[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Arrays must have the same length");
            Trit[] result = new Trit[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = Trit.TritOr(a[i], b[i]);
            return result;
        }

        /// <summary>
        /// Tritwise XOR (sum mod 3) of two trit arrays.
        /// </summary>
        public static Trit[] Xor(Trit[] a, Trit[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Arrays must have the same length");
            Trit[] result = new Trit[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = Trit.TritXor(a[i], b[i]);
            return result;
        }

        /// <summary>
        /// Shift left (multiply by 3^shift) — inserts zero trits at the least significant end.
        /// </summary>
        public static Trit[] ShiftLeft(Trit[] a, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            if (shift == 0) return (Trit[])a.Clone();
            Trit[] result = new Trit[a.Length];
            for (int i = shift; i < a.Length; i++)
                result[i] = a[i - shift];
            for (int i = 0; i < shift && i < a.Length; i++)
                result[i] = Trit.Zero;
            return result;
        }

        /// <summary>
        /// Arithmetic shift right (divide by 3^shift) — sign-extends the most significant trit.
        /// </summary>
        public static Trit[] ShiftRight(Trit[] a, int shift)
        {
            if (shift < 0) throw new ArgumentOutOfRangeException(nameof(shift));
            if (shift == 0) return (Trit[])a.Clone();
            Trit sign = a[^1]; // arithmetic shift: sign-extend
            Trit[] result = new Trit[a.Length];
            for (int i = 0; i < a.Length - shift; i++)
                result[i] = a[i + shift];
            for (int i = a.Length - shift; i < a.Length; i++)
                result[i] = sign;
            return result;
        }

        /// <summary>
        /// Convert a trit array to its string representation.
        /// </summary>
        public static string ToString(Trit[] a)
        {
            char[] chars = new char[a.Length];
            for (int i = 0; i < a.Length; i++)
                chars[i] = a[i].ToChar();
            return new string(chars);
        }

        /// <summary>
        /// Parse a string into a trit array.
        /// </summary>
        public static Trit[] FromString(string s)
        {
            Trit[] result = new Trit[s.Length];
            for (int i = 0; i < s.Length; i++)
                result[i] = Trit.FromChar(s[i]);
            return result;
        }
    }
}