using System;
using System.Text;
using TritTypes;

namespace T3Simulator.GUI.Utils;

public static class T3Formatter
{
    public const string FORMAT_TRINARY = "trinary";
    public const string FORMAT_NONARY = "nonary";
    public const string FORMAT_27ARY = "27ary";
    public const string FORMAT_DECIMAL = "decimal";

    public static string FormatValue(long value, string format)
    {
        return format switch
        {
            FORMAT_NONARY => FormatAsNonary(value),
            FORMAT_27ARY => FormatAs27ary(value),
            FORMAT_DECIMAL => value.ToString(),
            _ => FormatAsTrinary(value)
        };
    }

    public static string FormatAsTrinary(long value)
    {
        return BalancedTernary.ToTernaryString(value, 27);
    }

    public static string FormatAsNonary(long value)
    {
        string trinary = FormatAsTrinary(value);
        var sb = new StringBuilder();
        for (int i = 0; i < trinary.Length; i += 2)
        {
            string pair = trinary.Substring(i, Math.Min(2, trinary.Length - i));
            char c = pair switch
            {
                "--" => 'W',
                "-0" => 'X',
                "-+" => 'Y',
                "0-" => 'Z',
                "00" => '0',
                "0+" => '1',
                "+-" => '2',
                "+0" => '3',
                "++" => '4',
                _ => '?'
            };
            sb.Append(c);
        }
        return sb.ToString();
    }

    public static string FormatAs27ary(long value)
    {
        string trinary = FormatAsTrinary(value);
        var sb = new StringBuilder();
        char[] alphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
        for (int i = 0; i < trinary.Length; i += 3)
        {
            string triple = trinary.Substring(i, Math.Min(3, trinary.Length - i));
            while (triple.Length < 3) triple += "0";

            int t1 = triple[0] == '-' ? -1 : (triple[0] == '+' ? 1 : 0);
            int t2 = triple[1] == '-' ? -1 : (triple[1] == '+' ? 1 : 0);
            int t3 = triple[2] == '-' ? -1 : (triple[2] == '+' ? 1 : 0);

            int index = (t1 + 1) * 9 + (t2 + 1) * 3 + (t3 + 1);
            sb.Append(alphabet[index]);
        }
        return sb.ToString();
    }
}