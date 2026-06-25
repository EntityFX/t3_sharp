using System;
using System.Collections.Generic;

namespace TritTypes
{
    /// <summary>Parses integer literals in binary, decimal, balanced ternary (0t), nonal (0n), and heptavigesimal (0y) formats.</summary>
    public static class LiteralParser
    {
        public static long ParseInt(string v)
        {
            if (v.StartsWith("0t")) return ParseBalancedTernary(v);
            if (v.StartsWith("0y")) return ParseBase27(v);
            if (v.StartsWith("0n")) return ParseBase9(v);
            if (v.StartsWith("0b")) return Convert.ToInt64(v[2..], 2);
            return long.TryParse(v, out long n) ? n : 0;
        }

        static long ParseBalancedTernary(string s)
        {
            var t = s[2..]; long r = 0, p = 1;
            for (int i = t.Length - 1; i >= 0; i--) { r += t[i] switch { '+' => p, '-' => -p, _ => 0 }; p *= 3; }
            return r;
        }

        static long ParseBase9(string s)
        {
            var t = s[2..].ToUpper();
            var m = new Dictionary<char, long> { ['W'] = -4, ['X'] = -3, ['Y'] = -2, ['Z'] = -1, ['0'] = 0, ['1'] = 1, ['2'] = 2, ['3'] = 3, ['4'] = 4 };
            long r = 0, p = 1;
            for (int i = t.Length - 1; i >= 0; i--) { r += m.GetValueOrDefault(t[i]) * p; p *= 9; }
            return r;
        }

        static long ParseBase27(string s)
        {
            var t = s[2..].ToUpper();
            var m = new Dictionary<char, long> { ['N'] = -13, ['O'] = -12, ['P'] = -11, ['Q'] = -10, ['R'] = -9, ['S'] = -8, ['T'] = -7, ['U'] = -6, ['V'] = -5, ['W'] = -4, ['X'] = -3, ['Y'] = -2, ['Z'] = -1, ['0'] = 0, ['1'] = 1, ['2'] = 2, ['3'] = 3, ['4'] = 4, ['5'] = 5, ['6'] = 6, ['7'] = 7, ['8'] = 8, ['9'] = 9, ['A'] = 10, ['B'] = 11, ['C'] = 12, ['D'] = 13 };
            long r = 0, p = 1;
            for (int i = t.Length - 1; i >= 0; i--) { r += m.GetValueOrDefault(t[i]) * p; p *= 27; }
            return r;
        }
    }
}