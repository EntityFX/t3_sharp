using System;
using System.Collections.Generic;
using System.Text;

namespace TritTypes
{
    /// <summary>
    /// Utility class for converting between balanced ternary and binary representations.
    /// </summary>
    public static class BalancedTernary
    {
        /// <summary>
        /// Convert a long (binary) to a balanced ternary string.
        /// </summary>
        public static string ToTernaryString(long value, int minDigits = 1)
        {
            if (value == 0) return minDigits <= 1 ? "0" : new string('0', minDigits);

            int maxLen = Math.Max(minDigits, 1);
            // Estimate needed digits: log_3(2^63) ≈ 40, so 54 is safe
            char[] chars = new char[Math.Max(maxLen, 54)];
            int pos = chars.Length - 1;
            long remaining = value;

            while (remaining != 0 && pos >= 0)
            {
                long rem = remaining % 3;
                if (rem == 2) { chars[pos] = '-'; remaining = (remaining + 1) / 3; }
                else if (rem == -2) { chars[pos] = '+'; remaining = (remaining - 1) / 3; }
                else if (rem == 1) { chars[pos] = '+'; remaining = (remaining - 1) / 3; }
                else if (rem == -1) { chars[pos] = '-'; remaining = (remaining + 1) / 3; }
                else { chars[pos] = '0'; remaining /= 3; }
                pos--;
            }

            // Pad with leading zeros
            while (pos >= 0 && (chars.Length - 1 - pos) < minDigits)
            {
                chars[pos] = '0';
                pos--;
            }

            int start = pos + 1;
            int len = chars.Length - start;
            return new string(chars, start, len);
        }

        /// <summary>
        /// Convert an Int128 (binary) to a balanced ternary string.
        /// </summary>
        public static string ToTernaryString(Int128 value, int minDigits = 1)
        {
            if (value == 0) return minDigits <= 1 ? "0" : new string('0', minDigits);
 
            int maxLen = Math.Max(minDigits, 64);
            char[] chars = new char[maxLen];
            int pos = chars.Length - 1;
            Int128 remaining = value;
 
            while (remaining != 0 && pos >= 0)
            {
                Int128 rem = remaining % 3;
                int remInt = (int)rem;
                if (remInt == 2) { chars[pos] = '-'; remaining = (remaining + 1) / 3; }
                else if (remInt == -2) { chars[pos] = '+'; remaining = (remaining - 1) / 3; }
                else if (remInt == 1) { chars[pos] = '+'; remaining = (remaining - 1) / 3; }
                else if (remInt == -1) { chars[pos] = '-'; remaining = (remaining + 1) / 3; }
                else { chars[pos] = '0'; remaining /= 3; }
                pos--;
            }
 
            while (pos >= 0 && (chars.Length - 1 - pos) < minDigits)
            {
                chars[pos] = '0';
                pos--;
            }
 
            int start = pos + 1;
            int len = chars.Length - start;
            return new string(chars, start, len);
        }

        /// <summary>
        /// Parse a balanced ternary string to a long.
        /// </summary>
        public static long ParseToLong(string s)
        {
            long value = 0;
            long power = 1;
            for (int i = s.Length - 1; i >= 0; i--)
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
            return value;
        }

        /// <summary>
        /// Parse a balanced ternary string to an Int128.
        /// </summary>
        public static Int128 ParseToInt128(string s)
        {
            Int128 value = 0;
            Int128 power = 1;
            for (int i = s.Length - 1; i >= 0; i--)
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
            return value;
        }

        public static int[] ToTritArray(long value, int length)
        {
            int[] trits = new int[length];
            long remaining = value;
            for (int i = length - 1; i >= 0; i--)
            {
                long rem = remaining % 3;
                if (rem == 2) { trits[i] = -1; remaining = (remaining + 1) / 3; }
                else if (rem == -2) { trits[i] = 1; remaining = (remaining - 1) / 3; }
                else if (rem == 1) { trits[i] = 1; remaining = (remaining - 1) / 3; }
                else if (rem == -1) { trits[i] = -1; remaining = (remaining + 1) / 3; }
                else { trits[i] = 0; remaining /= 3; }
            }
            return trits;
        }

        public static long ParseFromTritArray(int[] trits)
        {
            long value = 0;
            long power = 1;
            for (int i = trits.Length - 1; i >= 0; i--)
            {
                value += trits[i] * power;
                power *= 3;
            }
            return value;
        }

        /// <summary>
        /// Generates a visual representation of balanced ternary addition.
        /// </summary>
        public static string GetAdditionVisual(string aStr, string bStr)
        {
            // Normalize lengths
            int maxLen = Math.Max(aStr.Length, bStr.Length);
            string a = aStr.PadLeft(maxLen, '0');
            string b = bStr.PadLeft(maxLen, '0');

            StringBuilder carryLine = new StringBuilder();
            StringBuilder aLine = new StringBuilder("  ");
            StringBuilder bLine = new StringBuilder(" + ");
            StringBuilder resLine = new StringBuilder();

            int carry = 0;
            StringBuilder resBuilder = new StringBuilder();

            for (int i = maxLen - 1; i >= 0; i--)
            {
                int valA = a[i] == '+' ? 1 : (a[i] == '-' ? -1 : 0);
                int valB = b[i] == '+' ? 1 : (b[i] == '-' ? -1 : 0);
                int sum = valA + valB + carry;

                int resultTrit;
                int nextCarry;

                switch (sum)
                {
                    case -2: resultTrit = 1; nextCarry = -1; break;
                    case -1: resultTrit = -1; nextCarry = 0; break;
                    case 0: resultTrit = 0; nextCarry = 0; break;
                    case 1: resultTrit = 1; nextCarry = 0; break;
                    case 2: resultTrit = -1; nextCarry = 1; break;
                    default: throw new Exception("Invalid sum");
                }

                resBuilder.Insert(0, resultTrit == 1 ? '+' : (resultTrit == -1 ? '-' : '0'));
                carryLine.Insert(0, nextCarry == 1 ? '+' : (nextCarry == -1 ? '-' : '0'));
                
                carry = nextCarry;
            }
            
            // The final carry is the most significant digit
            char finalCarry = carry == 1 ? '+' : (carry == -1 ? '-' : '0');
            string finalRes = finalCarry + resBuilder.ToString();
            
            // Formatting
            StringBuilder output = new StringBuilder();
            output.AppendLine($"  {carryLine}");
            output.AppendLine($"    {a}");
            output.AppendLine($" + {b}");
            output.AppendLine(new string('-', a.Length + 3));
            output.AppendLine($"  {finalRes}");

            return output.ToString();
        }
    }
}