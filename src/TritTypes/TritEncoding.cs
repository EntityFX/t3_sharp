using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TritTypes
{
    /// <summary>
    /// Provides methods for converting between ternary values and various encoding formats.
    /// </summary>
    public static class TritEncoding
    {
        // Ninary Alphabet: 2 trits per character
        // W (--)  X (-0)  Y (-+)  Z (0-)  0 (00)  1 (0+)  2 (+-)  3 (+0)  4 (++)
        private static readonly Dictionary<char, int[]> NinaryAlphabet = new Dictionary<char, int[]>
        {
            { 'W', new[] { -1, -1 } }, { 'X', new[] { -1, 0 } }, { 'Y', new[] { -1, 1 } }, { 'Z', new[] { 0, -1 } },
            { '0', new[] { 0, 0 } }, { '1', new[] { 0, 1 } }, { '2', new[] { 1, -1 } }, { '3', new[] { 1, 0 } }, { '4', new[] { 1, 1 } }
        };

        private static readonly Dictionary<string, char> NinaryReverseAlphabet = NinaryAlphabet
            .Select(kvp => new { Key = string.Join("", kvp.Value), Value = kvp.Key })
            .ToDictionary(x => x.Key, x => x.Value);

        // Tryx Alphabet: 3 trits per character
        // N O P Q R S T U V    (--- … -++)
        // W X Y Z 0 1 2 3 4    (0-- … 0++)
        // 5 6 7 8 9 A B C D    (+-- … +++)
        private static readonly char[] TryxAlphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();

        /// <summary>
        /// Encodes a list of trits into a simple text string: - => -1, 0 => 0, + => 1.
        /// </summary>
        public static string ToSimpleText(IEnumerable<int> trits)
        {
            var sb = new StringBuilder();
            foreach (var t in trits)
            {
                sb.Append(t == -1 ? '-' : (t == 1 ? '+' : '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decodes a simple text string into a list of trits.
        /// </summary>
        public static List<int> FromSimpleText(string text)
        {
            return text.Select(c => c == '-' ? -1 : (c == '+' ? 1 : 0)).ToList();
        }

        /// <summary>
        /// Encodes trits into a binary representation (2 bits per trit: 10 -> -1, 00 -> 0, 01 -> 1).
        /// </summary>
        public static byte[] ToBinary(IEnumerable<int> trits)
        {
            var bits = new List<int>();
            foreach (var t in trits)
            {
                if (t == -1) { bits.Add(1); bits.Add(0); }
                else if (t == 0) { bits.Add(0); bits.Add(0); }
                else { bits.Add(0); bits.Add(1); }
            }

            var bytes = new byte[(bits.Count + 7) / 8];
            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i] == 1)
                {
                    bytes[i / 8] |= (byte)(1 << (7 - (i % 8)));
                }
            }
            return bytes;
        }

        /// <summary>
        /// Decodes a binary representation into trits.
        /// </summary>
        public static List<int> FromBinary(byte[] data, int expectedTrits)
        {
            var trits = new List<int>();
            int bitIdx = 0;
            while (trits.Count < expectedTrits)
            {
                int bIdx = bitIdx / 8;
                if (bIdx >= data.Length) break;

                int bit1 = (data[bIdx] >> (7 - (bitIdx % 8))) & 1;
                int bit2 = (data[bIdx] >> (7 - ((bitIdx + 1) % 8))) & 1;
                // Note: This simplified version assumes bitIdx+1 is in the same byte or handled correctly
                // For a robust implementation, we need to handle byte boundaries for bit2.
                
                // Correcting bit2 extraction:
                int actualBit2 = 0;
                if ((bitIdx + 1) % 8 == 0)
                {
                    if (bIdx + 1 < data.Length)
                        actualBit2 = (data[bIdx + 1] >> 7) & 1;
                    else
                        actualBit2 = 0;
                }
                else
                {
                    actualBit2 = (data[bIdx] >> (7 - ((bitIdx + 1) % 8))) & 1;
                }

                if (bit1 == 1 && actualBit2 == 0) trits.Add(-1);
                else if (bit1 == 0 && actualBit2 == 0) trits.Add(0);
                else if (bit1 == 0 && actualBit2 == 1) trits.Add(1);
                else trits.Add(0); // Default/Invalid

                bitIdx += 2;
            }
            return trits;
        }

        /// <summary>
        /// Encodes trits into Ninary (0n) format.
        /// </summary>
        public static string ToNinary(IEnumerable<int> trits)
        {
            var tritList = trits.ToList();
            var sb = new StringBuilder("0n");
            for (int i = 0; i < tritList.Count; i += 2)
            {
                int t1 = tritList[i];
                int t2 = (i + 1 < tritList.Count) ? tritList[i + 1] : 0;
                string key = t1 + "," + t2;
                
                // Find char in NinaryAlphabet
                char found = ' ';
                foreach(var kvp in NinaryAlphabet)
                    if(kvp.Value[0] == t1 && kvp.Value[1] == t2) { found = kvp.Key; break; }
                
                sb.Append(found);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decodes Ninary (0n) format.
        /// </summary>
        public static List<int> FromNinary(string ninary)
        {
            if (ninary.StartsWith("0n")) ninary = ninary.Substring(2);
            var trits = new List<int>();
            foreach (var c in ninary)
            {
                if (NinaryAlphabet.TryGetValue(c, out var pair))
                {
                    trits.AddRange(pair);
                }
            }
            return trits;
        }

        /// <summary>
        /// Encodes trits into Tryx (0y) format.
        /// </summary>
        public static string ToTryx(IEnumerable<int> trits)
        {
            var tritList = trits.ToList();
            var sb = new StringBuilder("0y");
            for (int i = 0; i < tritList.Count; i += 3)
            {
                int t1 = tritList[i];
                int t2 = (i + 1 < tritList.Count) ? tritList[i + 1] : 0;
                int t3 = (i + 2 < tritList.Count) ? tritList[i + 2] : 0;
                
                // Find index in TryxAlphabet
                // Range is -1,0,1 for each. Total 3^3 = 27.
                // Index = (t1+1)*9 + (t2+1)*3 + (t3+1)
                int index = (t1 + 1) * 9 + (t2 + 1) * 3 + (t3 + 1);
                sb.Append(TryxAlphabet[index]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decodes Tryx (0y) format.
        /// </summary>
        public static List<int> FromTryx(string tryx)
        {
            if (tryx.StartsWith("0y")) tryx = tryx.Substring(2);
            var trits = new List<int>();
            foreach (var c in tryx)
            {
                int index = Array.IndexOf(TryxAlphabet, c);
                if (index == -1) continue;

                trits.Add((index / 9) - 1);
                trits.Add(((index / 3) % 3) - 1);
                trits.Add((index % 3) - 1);
            }
            return trits;
        }
    }
}