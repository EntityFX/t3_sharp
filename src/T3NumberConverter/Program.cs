using System;
using System.Globalization;
using TritTypes;

namespace T3NumberConverter
{
    /// <summary>
    /// T3 Number Converter — converts decimal integers to/from ternary, 9-ary, and 27-ary representations.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "dec2trit":
                    case "d2t":
                        if (args.Length < 2) { Console.WriteLine("Usage: d2t <decimal>"); return; }
                        Console.WriteLine(DecToTernary(args[1]));
                        break;

                    case "trit2dec":
                    case "t2d":
                        if (args.Length < 2) { Console.WriteLine("Usage: t2d <ternary>"); return; }
                        Console.WriteLine(TernaryToDec(args[1]));
                        break;

                    case "dec2n":
                    case "d2n":
                        if (args.Length < 2) { Console.WriteLine("Usage: d2n <decimal>"); return; }
                        Console.WriteLine(DecTo9Ary(args[1]));
                        break;

                    case "n2d":
                    case "n2dec":
                        if (args.Length < 2) { Console.WriteLine("Usage: n2d <9ary>"); return; }
                        Console.WriteLine(AryToDec(args[1], 9));
                        break;

                    case "dec2y":
                    case "d2y":
                        if (args.Length < 2) { Console.WriteLine("Usage: d2y <decimal>"); return; }
                        Console.WriteLine(DecTo27Ary(args[1]));
                        break;

                    case "y2d":
                    case "y2dec":
                        if (args.Length < 2) { Console.WriteLine("Usage: y2d <27ary>"); return; }
                        Console.WriteLine(AryToDec(args[1], 27));
                        break;

                    case "all":
                        if (args.Length < 2) { Console.WriteLine("Usage: all <decimal>"); return; }
                        PrintAll(args[1]);
                        break;

                    case "help":
                    case "--help":
                    case "-h":
                        PrintUsage();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static string DecToTernary(string input)
        {
            long value = long.Parse(input);
            return BalancedTernary.ToTernaryString(value);
        }

        static long TernaryToDec(string input)
        {
            return BalancedTernary.ParseToLong(input);
        }

        static string DecTo9Ary(string input)
        {
            long value = long.Parse(input);
            string ternary = BalancedTernary.ToTernaryString(value);
            return TritTo9Ary(ternary);
        }

        static string DecTo27Ary(string input)
        {
            long value = long.Parse(input);
            string ternary = BalancedTernary.ToTernaryString(value);
            return TritTo27Ary(ternary);
        }

        static long AryToDec(string input, int baseSystem)
        {
            if (baseSystem == 9)
                return BalancedTernary.ParseToLong(AryToTrits(input, 9));
            else
                return BalancedTernary.ParseToLong(AryToTrits(input, 27));
        }

        /// <summary>
        /// Converts a ternary string (+,0,-) to 9-ary representation.
        /// </summary>
        static string TritTo9Ary(string ternary)
        {
            // Pad to even length
            if (ternary.Length % 2 != 0)
                ternary = "0" + ternary;

            var result = new System.Text.StringBuilder();
            result.Append("0n");
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

        /// <summary>
        /// Converts a ternary string (+,0,-) to 27-ary representation.
        /// </summary>
        static string TritTo27Ary(string ternary)
        {
            char[] alphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();

            // Pad to multiple of 3
            while (ternary.Length % 3 != 0)
                ternary = "0" + ternary;

            var result = new System.Text.StringBuilder();
            result.Append("0y");
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

        /// <summary>
        /// Converts a 9-ary or 27-ary string back to ternary trit string.
        /// </summary>
        static string AryToTrits(string input, int baseSystem)
        {
            string upper = input.ToUpper().Trim();
            if (upper.StartsWith("0N")) upper = upper.Substring(2);
            if (upper.StartsWith("0Y")) upper = upper.Substring(2);

            var result = new System.Text.StringBuilder();

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

        static char TritChar(int t) => t == -1 ? '-' : (t == 1 ? '+' : '0');

        static void PrintAll(string input)
        {
            long value = long.Parse(input);
            string ternary = BalancedTernary.ToTernaryString(value);
            Console.WriteLine($"Decimal : {value}");
            Console.WriteLine($"Ternary : {ternary}");
            Console.WriteLine($"9-ary   : {TritTo9Ary(ternary)}");
            Console.WriteLine($"27-ary  : {TritTo27Ary(ternary)}");
        }

        static void PrintUsage()
        {
            Console.WriteLine("T3 Number Converter — Balanced Ternary Number System");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  T3NumberConverter <command> <value>");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  d2t, dec2trit   <decimal>   → ternary (+,0,-)");
            Console.WriteLine("  t2d, trit2dec   <ternary>   → decimal");
            Console.WriteLine("  d2n, dec2n      <decimal>   → 9-ary (WXYZ01234)");
            Console.WriteLine("  n2d, n2dec      <9ary>      → decimal");
            Console.WriteLine("  d2y, dec2y      <decimal>   → 27-ary (NOPQRSTUV...)");
            Console.WriteLine("  y2d, y2dec      <27ary>     → decimal");
            Console.WriteLine("  all             <decimal>   → all representations");
            Console.WriteLine("  help                        → this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  T3NumberConverter all 42");
            Console.WriteLine("  T3NumberConverter d2t -100");
            Console.WriteLine("  T3NumberConverter t2d +-0");
            Console.WriteLine("  T3NumberConverter d2n 364");
            Console.WriteLine("  T3NumberConverter y2d 0y1C");
        }
    }
}