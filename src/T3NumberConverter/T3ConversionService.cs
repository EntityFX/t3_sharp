using System;
using System.Globalization;
using System.Text;
using TritTypes;

namespace T3NumberConverter
{
    public class ConversionResult
    {
        public long DecimalValue { get; set; }
        public string Binary { get; set; }
        public string Hex { get; set; }
        public string Ternary { get; set; }
        public string Nonary { get; set; }
        public string TwentySevenAry { get; set; }
        public string Octal { get; set; }
    }

    public class T3ConversionService
    {
        public ConversionResult Convert(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string trimmed = input.Trim();
            long value;

            try
            {
                if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    value = long.Parse(trimmed.Substring(2), NumberStyles.HexNumber);
                }
                else if (trimmed.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                {
                    value = Convert.ToInt64(trimmed.Substring(2), 2);
                }
                else if (trimmed.StartsWith("0t", StringComparison.OrdinalIgnoreCase))
                {
                    value = BalancedTernary.ParseToLong(trimmed.Substring(2));
                }
                else if (trimmed.StartsWith("0n", StringComparison.OrdinalIgnoreCase))
                {
                    value = BalancedTernary.ParseToLong(AryToTrits(trimmed.Substring(2), 9));
                }
                else if (trimmed.StartsWith("0y", StringComparison.OrdinalIgnoreCase))
                {
                    value = BalancedTernary.ParseToLong(AryToTrits(trimmed.Substring(2), 27));
                }
                else if (trimmed.StartsWith("00", StringComparison.OrdinalIgnoreCase))
                {
                    value = Convert.ToInt64(trimmed.Substring(2), 8);
                }
                else
                {
                    value = long.Parse(trimmed);
                }
            }
            catch
            {
                throw new FormatException("Invalid input format for the detected base.");
            }

            string ternary = BalancedTernary.ToTernaryString(value);

            return new ConversionResult
            {
                DecimalValue = value,
                Binary = "0b" + Convert.ToString(value, 2),
                Hex = "0x" + value.ToString("X"),
                Octal = "00" + Convert.ToString(value, 8),
                Ternary = "0t" + ternary,
                Nonary = "0n" + TritTo9Ary(ternary),
                TwentySevenAry = "0y" + TritTo27Ary(ternary)
            };
        }

        private string TritTo9Ary(string ternary)
        {
            if (ternary.Length % 2 != 0)
                ternary = "0" + ternary;

            var result = new StringBuilder();
            for (int i = 0; i < ternary.Length; i += 2)
            {
                string pair = ternary.Substring(i, 2);
                result.Append(pair switch
                {
                    "--" => 'W', "-0" => 'X', "-+" => 'Y',
                    "0-" => 'Z', "00" => '0', "0+" => '1',
                    "+-" => '2', "+0" => '3', "++" => '4',
                    _ => '?'
                });
            }
            return result.ToString();
        }

        private string TritTo27Ary(string ternary)
        {
            char[] alphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
            while (ternary.Length % 3 != 0)
                ternary = "0" + ternary;

            var result = new StringBuilder();
            for (int i = 0; i < ternary.Length; i += 3)
            {
                int t1 = ternary[i] == '-' ? -1 : (ternary[i] == '+' ? 1 : 0);
                int t2 = ternary[i + 1] == '-' ? -1 : (ternary[i + 1] == '+' ? 1 : 0);
                int t3 = ternary[i + 2] == '-' ? -1 : (ternary[i + 2] == '+' ? 1 : 0);
                int index = (t1 + 1) * 9 + (t2 + 1) * 3 + (t3 + 1);
                result.Append(alphabet[index]);
            }
            return result.ToString();
        }

        private string AryToTrits(string input, int baseSystem)
        {
            string upper = input.ToUpper().Trim();
            var result = new StringBuilder();

            if (baseSystem == 9)
            {
                foreach (char c in upper)
                {
                    result.Append(c switch
                    {
                        'W' => "--", 'X' => "-0", 'Y' => "-+",
                        'Z' => "0-", '0' => "00", '1' => "0+",
                        '2' => "+-", '3' => "+0", '4' => "++",
                        _ => throw new Exception($"Invalid 9-ary character: {c}")
                    });
                }
            }
            else
            {
                char[] alphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
                foreach (char c in upper)
                {
                    int index = Array.IndexOf(alphabet, c);
                    if (index < 0) throw new Exception($"Invalid 27-ary character: {c}");

                    int t1 = (index / 9) % 3 - 1;
                    int t2 = (index / 3) % 3 - 1;
                    int t3 = index % 3 - 1;

                    result.Append(TritChar(t1));
                    result.Append(TritChar(t2));
                    result.Append(TritChar(t3));
                }
            }
            return result.ToString();
        }

        private char TritChar(int t) => t == -1 ? '-' : (t == 1 ? '+' : '0');
    }
}