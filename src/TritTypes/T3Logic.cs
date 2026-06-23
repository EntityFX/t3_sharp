using System;
using System.Collections.Generic;

namespace TritTypes
{
    /// <summary>
    /// Implements balanced ternary logic operations based on the T3 architecture documentation.
    /// Values: -1 (False/Low), 0 (Unknown/Neutral), +1 (True/High).
    /// </summary>
    public static class T3Logic
    {
        public enum TernaryValue
        {
            Minus = -1,
            Zero = 0,
            Plus = 1
        }

        // --- Basic Binary Operations ---

        public static int And(int a, int b) => Math.Min(a, b);
        public static int Or(int a, int b) => Math.Max(a, b);
        
        public static int Xor(int a, int b)
        {
            // Sum mod 3 adjusted for balanced ternary
            // -1, 0, 1
            int sum = a + b;
            if (sum == -2) return 1;  // -1 + -1 = -2 => +1 (mod 3)
            if (sum == 2) return -1;  // 1 + 1 = 2 => -1 (mod 3)
            return sum;
        }

        public static int Implies(int a, int b)
        {
            // T3 Implies: a -> b
            // -1 -> -1 = 1, -1 -> 0 = 1, -1 -> 1 = 1
            // 0 -> -1 = -1, 0 -> 0 = 0, 0 -> 1 = 1
            // 1 -> -1 = -1, 1 -> 0 = 0, 1 -> 1 = 1
            if (a == -1) return 1;
            if (a == 0) return b;
            return b; 
            // Simplified: if a is not -1, result is b. If a is -1, result is 1.
            // Wait, looking at doc Table 5:
            // A \ B | - | 0 | +
            // -     | + | + | +
            // 0     | - | 0 | +
            // +     | - | 0 | +
            // This matches my logic.
        }

        public static int Xnor(int a, int b)
        {
            // Table 6: Equivalence
            // - -: + | - 0: 0 | - +: -
            // 0 -: 0 | 0 0: 0 | 0 +: 0
            // + -: - | + 0: 0 | + +: +
            if (a == b) return 1;
            if (a == 0 || b == 0) return 0;
            return -1;
        }

        // --- Unary Operations ---

        public static int Not(int a) => -a; // Central complement

        public static int NotMinus(int a) => a == 1 ? 1 : -1; // Negative saturation
        public static int NotPlus(int a) => a == -1 ? -1 : 1; // Positive saturation

        public static int Inc(int a)
        {
            // -1 -> 0, 0 -> 1, 1 -> -1
            return (a + 1 == 2) ? -1 : a + 1;
        }

        public static int Dec(int a)
        {
            // 1 -> 0, 0 -> -1, -1 -> 1
            return (a - 1 == -2) ? 1 : a - 1;
        }

        // --- Advanced / NERC ITMO Operations ---

        public static int Up(int a) => a == 1 ? 0 : 1; // f(-)=+, f(0)=+, f(+)=0
        public static int Down(int a) => a == -1 ? 0 : -1; // f(-)=0, f(0)=-, f(+)=-

        /// <summary>
        /// Generates a truth table for a given binary operation.
        /// </summary>
        public static List<(int A, int B, int Result)> GenerateTruthTable(Func<int, int, int> op)
        {
            var table = new List<(int, int, int)>();
            int[] values = { -1, 0, 1 };
            foreach (var a in values)
            {
                foreach (var b in values)
                {
                    table.Add((a, b, op(a, b)));
                }
            }
            return table;
        }
    }
}