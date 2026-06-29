using System;

namespace TritTypes
{
    /// <summary>
    /// Centralized utility for balanced ternary arithmetic and field extraction.
    /// Ensures consistent trit order and indexing across different word sizes.
    /// </summary>
    public static class TernaryMath
    {
        /// <summary>
        /// Extracts a single balanced trit from a value at the given position.
        /// Position 0 is the least significant trit.
        /// </summary>
        /// <summary>
        /// Extracts a single balanced trit from a value at the given position.
        /// Position 0 is the least significant trit.
        /// Uses the standard balanced ternary carry algorithm to correctly handle
        /// values where carries from lower positions affect higher positions.
        /// </summary>
        public static int ExtractBalancedTrit(Int128 value, int position)
        {
            // To correctly extract a balanced trit, we need to process all trits
            // from position 0 up to the requested position, handling carries.
            // We use the same algorithm as ToTernaryString: for each position,
            // compute rem = remaining % 3, map 2->-1 (with carry +1), -2->1 (with carry -1).
            Int128 remaining = value;
            for (int i = 0; i <= position; i++)
            {
                Int128 rem = remaining % 3;
                int remInt = (int)rem;
                
                if (i == position)
                {
                    // This is the trit we want
                    if (remInt == 2) return -1;
                    if (remInt == -2) return 1;
                    if (remInt == 1) return 1;
                    if (remInt == -1) return -1;
                    return 0;
                }
                
                // Update remaining for next position (carry propagation)
                if (remInt == 2) remaining = (remaining + 1) / 3;
                else if (remInt == -2) remaining = (remaining - 1) / 3;
                else if (remInt == 1) remaining = (remaining - 1) / 3;
                else if (remInt == -1) remaining = (remaining + 1) / 3;
                else remaining /= 3;
            }
            
            return 0; // Should never reach here
        }

        /// <summary>
        /// Extracts a range of trits and converts them to a balanced ternary integer.
        /// This is the only reliable way to extract fields from a balanced ternary value.
        /// </summary>
        public static Int128 ExtractFieldAsBalanced(Int128 value, int startPos, int width)
        {
            Int128 result = 0;
            Int128 power = 1;
            for (int i = 0; i < width; i++)
            {
                result += ExtractBalancedTrit(value, startPos + i) * power;
                power *= 3;
            }
            return result;
        }

        /// <summary>
        /// Extracts a range of trits and converts them to an unsigned ternary integer.
        /// Formula: sum((trit + 1) * 3^i)
        /// </summary>
        public static Int128 ExtractFieldAsUnsigned(Int128 value, int startPos, int width)
        {
            Int128 result = 0;
            Int128 power = 1;
            for (int i = 0; i < width; i++)
            {
                result += (ExtractBalancedTrit(value, startPos + i) + 1) * power;
                power *= 3;
            }
            return result;
        }

        [Obsolete("Use ExtractFieldAsUnsigned for raw values and ExtractFieldAsBalanced for balanced values.")]
        public static Int128 ExtractRawField(Int128 value, int startPos, int width)
        {
            return ExtractFieldAsUnsigned(value, startPos, width);
        }

        [Obsolete("Use ExtractFieldAsBalanced.")]
        public static Int128 ExtractBalancedField(Int128 value, int startPos, int width)
        {
            return ExtractFieldAsBalanced(value, startPos, width);
        }

        /// <summary>
        /// Computes 3^exp.
        /// </summary>
        public static Int128 Pow3(int exp)
        {
            if (exp < 0) throw new ArgumentOutOfRangeException(nameof(exp), "Exponent must be non-negative");
            Int128 result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }

        /// <summary>
        /// Performs a balanced ternary XOR (sum modulo 3 with balanced mapping).
        /// Truth table: 
        /// -1, -1 -> -2 -> 1
        /// -1,  0 -> -1 -> -1
        /// -1,  1 ->  0 -> 0
        ///  0,  0 ->  0 -> 0
        ///  0,  1 ->  1 -> 1
        ///  1,  1 ->  2 -> -1
        /// </summary>
        public static int BalancedTernaryXor(int a, int b)
        {
            int sum = a + b;
            int res = sum % 3;
            if (res == 2) return -1;
            if (res == -2) return 1;
            return res;
        }
    }
}