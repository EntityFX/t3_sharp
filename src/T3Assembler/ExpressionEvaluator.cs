using System;
using System.Collections.Generic;
using TritTypes;

namespace T3Assembler
{
    /// <summary>Evaluates arithmetic expressions in assembler operands (supports +, -, *, /, labels, constants, and literal formats).</summary>
    public class ExpressionEvaluator
    {
        readonly Dictionary<string, Int128> _constants;
        readonly Dictionary<string, int> _labels;

        public ExpressionEvaluator(Dictionary<string, Int128> constants, Dictionary<string, int> labels)
        {
            _constants = constants;
            _labels = labels;
        }

        public Int128 Evaluate(string expr)
        {
            expr = expr.Replace(" ", "").Replace("\t", "");
            char[] ops = { '+', '-', '*', '/' };
            for (int i = 1; i < expr.Length - 1; i++)
            {
                char c = expr[i];
                if (Array.IndexOf(ops, c) >= 0)
                {
                    Int128 lv = ResolveSimple(expr[..i]);
                    Int128 rv = ResolveSimple(expr[(i + 1)..]);
                    return c switch
                    {
                        '+' => lv + rv,
                        '-' => lv - rv,
                        '*' => lv * rv,
                        '/' => (rv == 0 ? throw new DivideByZeroException() : lv / rv),
                        _ => Int128.Zero
                    };
                }
            }
            return ResolveSimple(expr);
        }

        Int128 ResolveSimple(string t)
        {
            if (long.TryParse(t, out long v)) return v;
            if (_constants.TryGetValue(t, out var cv)) return cv;
            if (_labels.TryGetValue(t, out int a)) return a;
            if (t.StartsWith("0t", StringComparison.OrdinalIgnoreCase)) return BalancedTernary.ParseToInt128(t[2..]);
            if (t.StartsWith("0n", StringComparison.OrdinalIgnoreCase)) return ParseBase9(t[2..]);
            if (t.StartsWith("0y", StringComparison.OrdinalIgnoreCase)) return ParseBase27(t[2..]);
            return Int128.Zero;
        }

        static Int128 ParseBase9(string t)
        {
            string r = "";
            foreach (char c in t.ToUpper())
                r += c switch { 'W' => "--", 'X' => "-0", 'Y' => "-+", 'Z' => "0-", '0' => "00", '1' => "0+", '2' => "+-", '3' => "+0", '4' => "++", _ => throw new FormatException($"Unknown 0n character: {c}") };
            return BalancedTernary.ParseToInt128(r);
        }

        static Int128 ParseBase27(string t)
        {
            string r = "";
            foreach (char c in t.ToUpper())
                r += c switch { 'N' => "---", 'O' => "--0", 'P' => "--+", 'Q' => "-0-", 'R' => "-00", 'S' => "-0+", 'T' => "-+-", 'U' => "-+0", 'V' => "-++", '5' => "+--", '6' => "+-0", '7' => "+-+", '8' => "+0-", '9' => "+00", 'A' => "+0+", 'B' => "++-", 'C' => "++0", 'D' => "+++", _ => throw new FormatException($"Unknown 0y character: {c}") };
            return BalancedTernary.ParseToInt128(r);
        }
    }
}