using System;
using System.Collections.Generic;
using System.Text;

namespace TritTypes
{
    /// <summary>
    /// Service for generating T-SCII encoding data based on the T-SCII specification.
    /// </summary>
    public class T3SciiService
    {
        public struct TSciiEntry
        {
            public char Symbol { get; set; }
            public int Value { get; set; }
            public string Ternary { get; set; }
            public string Base9 { get; set; }
            public string Base27 { get; set; }
        }

        /// <summary>
        /// Generates a list of all characters in the T-SCII range (-364 to 364).
        /// </summary>
        public List<TSciiEntry> GenerateSciiTable()
        {
            var table = new List<TSciiEntry>();
            for (int v = -364; v <= 364; v++)
            {
                table.Add(new TSciiEntry
                {
                    Symbol = GetSymbolForValue(v),
                    Value = v,
                    Ternary = ToBalancedTernary(v, 6),
                    Base9 = ToBase9(v),
                    Base27 = ToBase27(v)
                });
            }
            return table;
        }

        private char GetSymbolForValue(int v)
        {
            // Standard part: V = 0..255 is compatible with CP-1251
            if (v >= 0 && v <= 255)
            {
                return (char)v;
            }
            
            // Other ranges are currently unmapped to standard chars
            return ' ';
        }

        private string ToBalancedTernary(int v, int length)
        {
            // Use existing BalancedTernary utility
            return BalancedTernary.ToTernaryString(v, length);
        }

        private string ToBase9(int v)
        {
            // Based on plans/t-scii.md: Pair of trits
            // --=W, -0=X, -+=Y, 0-=Z, 00=0, 0+=1, +-=2, +0=3, ++=4
            string ternary = ToBalancedTernary(v, 6);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ternary.Length; i += 2)
            {
                string pair = ternary.Substring(i, 2);
                sb.Append(pair switch
                {
                    "--" => 'W', "-0" => 'X', "-+" => 'Y',
                    "0-" => 'Z', "00" => '0', "0+" => '1',
                    "+-" => '2', "+0" => '3', "++" => '4',
                    _ => '?'
                });
            }
            return sb.ToString();
        }

        private string ToBase27(int v)
        {
            // Based on plans/t-scii.md: Triple of trits
            string ternary = ToBalancedTernary(v, 6);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ternary.Length; i += 3)
            {
                string triple = ternary.Substring(i, 3);
                sb.Append(triple switch
                {
                    "---" => 'N', "--0" => 'O', "--+" => 'P', "-0-" => 'Q', "-00" => 'R', "-0+" => 'S', "-+-" => 'T', "-+0" => 'U', "-++" => 'V',
                    "0--" => 'W', "0-0" => 'X', "0-+" => 'Y', "00-" => 'Z', "000" => '0', "00+" => '1', "0+-" => '2', "0+0" => '3', "0++" => '4',
                    "+--" => '5', "+-0" => '6', "+-+" => '7', "+0-" => '8', "+00" => '9', "+0+" => 'A', "++-" => 'B', "++0" => 'C', "+++" => 'D',
                    _ => '?'
                });
            }
            return sb.ToString();
        }
    }
}