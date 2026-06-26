using System;
using System.Collections.Generic;
using TritTypes;

namespace T3Assembler
{
    /// <summary>Handles .string directive parsing in the T3 assembler.</summary>
    public static class StringLiteralParser
    {
        /// <summary>Resolves a .string or inline string literal into T-SCII Int128 values, null-terminated.</summary>
        public static List<Int128> Resolve(string content)
        {
            content = content.Trim('"');
            var result = new List<Int128>();
            foreach (char c in content)
                result.Add(TScii.FromChar(c));
            result.Add(0); // null terminator
            return result;
        }

        /// <summary>Extracts the string content between quotes in a line like .string label "text" or label .string "text".</summary>
        public static string ExtractContent(string line)
        {
            int firstQuote = line.IndexOf('"');
            int lastQuote = line.LastIndexOf('"');
            if (firstQuote == -1 || lastQuote == -1 || firstQuote == lastQuote)
                throw new Exception("Invalid string literal");
            return line.Substring(firstQuote, lastQuote - firstQuote + 1);
        }

        /// <summary>Returns the word count for a .string line (chars + null terminator).</summary>
        public static int CalculateSize(string content) => content.Length - 2 + 1; // strip quotes, add null

        /// <summary>Tries to extract a label from a .string line ("label .string ..." or ".string label ...")</summary>
        public static string? ExtractLabel(string line)
        {
            var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2) return null;
            if (words[1] == ".string" || words[1] == ".word")
                return words[0];
            if (words[0] == ".string")
                return words[1];
            return null;
        }
    }
}