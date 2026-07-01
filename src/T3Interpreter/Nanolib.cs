using System;
using System.Collections.Generic;
using System.Numerics;
using T3Compiler.Parser;
using TritTypes;

namespace T3Interpreter
{
    /// <summary>
    /// Nanolib — встроенная стандартная библиотека для интерпретатора T-lang.
    /// Содержит функции из <tio.h>, <tstring.h>, <tmath.h> и базовые builtins.
    /// </summary>
    public static class Nanolib
    {
        /// <summary>
        /// Пытается выполнить builtin-функцию. Возвращает true, если функция найдена и выполнена.
        /// </summary>
        public static bool TryCall(string fn, FunctionCall fc, Func<AstNode, T3Value> eval, out T3Value result)
        {
            result = T3Value.Void;

            // === <tio.h> — I/O functions ===
            if (fn == "putchar" || fn == "print_char")
            {
                Console.Write((char)eval(fc.Arguments[0]).AsInt());
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "printint" || fn == "print_int")
            {
                Console.Write(eval(fc.Arguments[0]).AsInt());
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "print_long")
            {
                Console.Write(eval(fc.Arguments[0]).AsLong());
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "printfloat" || fn == "print_float")
            {
                Console.Write(eval(fc.Arguments[0]).AsFloat().ToString(System.Globalization.CultureInfo.InvariantCulture));
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "print_double")
            {
                Console.Write(eval(fc.Arguments[0]).AsFloat().ToString(System.Globalization.CultureInfo.InvariantCulture));
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "print_tryte")
            {
                Console.Write((char)eval(fc.Arguments[0]).AsInt());
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "print_bal")
            {
                long v = eval(fc.Arguments[0]).AsInt();
                Console.Write(BalancedTernary.ToTernaryString(v));
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "print_str" || fn == "puts")
            {
                var str = eval(fc.Arguments[0]);
                if (str.Kind == T3Value.ValueKind.Array)
                {
                    int len = (int)str.GetElement(0).AsInt();
                    for (int i = 1; i <= len; i++)
                        Console.Write((char)str.GetElement(i).AsInt());
                }
                Console.WriteLine();
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "get_char")
            {
                int c = Console.Read();
                result = T3Value.FromInt(c >= 0 ? c : -1);
                return true;
            }
            if (fn == "scan_int")
            {
                string? line = Console.ReadLine();
                result = T3Value.FromInt(line != null && long.TryParse(line.Trim(), out long n) ? n : 0);
                return true;
            }
            if (fn == "scan_long")
            {
                string? line = Console.ReadLine();
                result = line != null && Int128.TryParse(line.Trim(), out var n)
                    ? T3Value.FromLong(n)
                    : T3Value.FromLong(0);
                return true;
            }
            if (fn == "scan_float")
            {
                string? line = Console.ReadLine();
                result = line != null && double.TryParse(line.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d)
                    ? T3Value.FromFloat(d)
                    : T3Value.FromFloat(0);
                return true;
            }
            if (fn == "scan_double")
            {
                string? line = Console.ReadLine();
                result = line != null && double.TryParse(line.Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d)
                    ? T3Value.FromFloat(d)
                    : T3Value.FromFloat(0);
                return true;
            }

            // === <tstring.h> — String functions ===
            if (fn == "strlen" || fn == "t_strlen")
            {
                var str = eval(fc.Arguments[0]);
                result = str.Kind == T3Value.ValueKind.Array && str.ArrayLength > 0
                    ? str.GetElement(0)
                    : T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_strcmp")
            {
                string a = GetString(eval(fc.Arguments[0]));
                string b = GetString(eval(fc.Arguments[1]));
                int cmp = string.Compare(a, b, StringComparison.Ordinal);
                result = T3Value.FromInt(cmp < 0 ? -1 : (cmp > 0 ? 1 : 0));
                return true;
            }
            if (fn == "t_strncmp")
            {
                string a = GetString(eval(fc.Arguments[0]));
                string b = GetString(eval(fc.Arguments[1]));
                int n = (int)eval(fc.Arguments[2]).AsInt();
                if (n < 0) n = 0;
                int minLen = Math.Min(Math.Min(a.Length, b.Length), n);
                for (int i = 0; i < minLen; i++)
                {
                    if (a[i] < b[i]) { result = T3Value.FromInt(-1); return true; }
                    if (a[i] > b[i]) { result = T3Value.FromInt(1); return true; }
                }
                if (minLen < n)
                {
                    if (a.Length < b.Length) { result = T3Value.FromInt(-1); return true; }
                    if (a.Length > b.Length) { result = T3Value.FromInt(1); return true; }
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_strcpy")
            {
                var dest = eval(fc.Arguments[0]);
                var src = eval(fc.Arguments[1]);
                if (dest.Kind == T3Value.ValueKind.Array && src.Kind == T3Value.ValueKind.Array)
                {
                    int srcLen = (int)src.GetElement(0).AsInt();
                    int copyLen = Math.Min(srcLen, dest.ArrayLength - 1);
                    dest.SetElement(0, T3Value.FromInt(copyLen));
                    for (int i = 0; i < copyLen; i++)
                        dest.SetElement(i + 1, src.GetElement(i + 1));
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_strncpy")
            {
                var dest = eval(fc.Arguments[0]);
                var src = eval(fc.Arguments[1]);
                int n = (int)eval(fc.Arguments[2]).AsInt();
                if (dest.Kind == T3Value.ValueKind.Array && src.Kind == T3Value.ValueKind.Array)
                {
                    int srcLen = (int)src.GetElement(0).AsInt();
                    int copyLen = Math.Min(Math.Min(srcLen, n), dest.ArrayLength - 1);
                    dest.SetElement(0, T3Value.FromInt(copyLen));
                    for (int i = 0; i < copyLen; i++)
                        dest.SetElement(i + 1, src.GetElement(i + 1));
                    for (int i = copyLen; i < n && i + 1 < dest.ArrayLength; i++)
                        dest.SetElement(i + 1, T3Value.FromInt(0));
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_strcat")
            {
                var dest = eval(fc.Arguments[0]);
                var src = eval(fc.Arguments[1]);
                if (dest.Kind == T3Value.ValueKind.Array && src.Kind == T3Value.ValueKind.Array)
                {
                    int destLen = (int)dest.GetElement(0).AsInt();
                    int srcLen = (int)src.GetElement(0).AsInt();
                    int maxCopy = Math.Min(srcLen, dest.ArrayLength - 1 - destLen);
                    for (int i = 0; i < maxCopy; i++)
                        dest.SetElement(destLen + 1 + i, src.GetElement(i + 1));
                    dest.SetElement(0, T3Value.FromInt(destLen + maxCopy));
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_strchr")
            {
                string str = GetString(eval(fc.Arguments[0]));
                char c = (char)eval(fc.Arguments[1]).AsInt();
                int idx = str.IndexOf(c);
                result = T3Value.FromInt(idx >= 0 ? idx + 1 : -1);
                return true;
            }
            if (fn == "t_strrchr")
            {
                string str = GetString(eval(fc.Arguments[0]));
                char c = (char)eval(fc.Arguments[1]).AsInt();
                int idx = str.LastIndexOf(c);
                result = T3Value.FromInt(idx >= 0 ? idx + 1 : -1);
                return true;
            }
            if (fn == "t_strstr")
            {
                string haystack = GetString(eval(fc.Arguments[0]));
                string needle = GetString(eval(fc.Arguments[1]));
                int idx = haystack.IndexOf(needle, StringComparison.Ordinal);
                result = T3Value.FromInt(idx >= 0 ? idx + 1 : -1);
                return true;
            }
            if (fn == "atoi" || fn == "t_atoi")
            {
                var str = eval(fc.Arguments[0]);
                if (str.Kind == T3Value.ValueKind.Array)
                {
                    int len = (int)str.GetElement(0).AsInt();
                    if (len == 0) { result = T3Value.FromInt(0); return true; }
                    bool neg = str.GetElement(1).AsInt() == '-';
                    long val = 0;
                    for (int i = neg ? 2 : 1; i <= len; i++)
                        val = val * 10 + (int)(str.GetElement(i).AsInt() - '0');
                    result = T3Value.FromInt(neg ? -val : val);
                    return true;
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_atol")
            {
                string s = GetString(eval(fc.Arguments[0]));
                if (s.Length == 0) { result = T3Value.FromLong(0); return true; }
                result = Int128.TryParse(s, out var n)
                    ? T3Value.FromLong(n)
                    : T3Value.FromLong(0);
                return true;
            }
            if (fn == "t_atof")
            {
                string s = GetString(eval(fc.Arguments[0]));
                if (s.Length == 0) { result = T3Value.FromFloat(0); return true; }
                result = double.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double d)
                    ? T3Value.FromFloat(d)
                    : T3Value.FromFloat(0);
                return true;
            }
            if (fn == "t_itoa")
            {
                long val = eval(fc.Arguments[0]).AsInt();
                var buf = eval(fc.Arguments[1]);
                string s = val.ToString();
                if (buf.Kind == T3Value.ValueKind.Array)
                {
                    int maxLen = Math.Min(s.Length, buf.ArrayLength - 1);
                    buf.SetElement(0, T3Value.FromInt(maxLen));
                    for (int i = 0; i < maxLen; i++)
                        buf.SetElement(i + 1, T3Value.FromInt((int)s[i]));
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_ltoa")
            {
                Int128 val = eval(fc.Arguments[0]).AsLong();
                var buf = eval(fc.Arguments[1]);
                string s = val.ToString();
                if (buf.Kind == T3Value.ValueKind.Array)
                {
                    int maxLen = Math.Min(s.Length, buf.ArrayLength - 1);
                    buf.SetElement(0, T3Value.FromInt(maxLen));
                    for (int i = 0; i < maxLen; i++)
                        buf.SetElement(i + 1, T3Value.FromInt((int)s[i]));
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_baltoa")
            {
                long val = eval(fc.Arguments[0]).AsInt();
                var buf = eval(fc.Arguments[1]);
                string s = BalancedTernary.ToTernaryString(val);
                if (buf.Kind == T3Value.ValueKind.Array)
                {
                    int maxLen = Math.Min(s.Length, buf.ArrayLength - 1);
                    buf.SetElement(0, T3Value.FromInt(maxLen));
                    for (int i = 0; i < maxLen; i++)
                        buf.SetElement(i + 1, T3Value.FromInt((int)s[i]));
                }
                result = T3Value.FromInt(0);
                return true;
            }
            if (fn == "t_ftoa")
            {
                double val = eval(fc.Arguments[0]).AsFloat();
                var buf = eval(fc.Arguments[1]);
                string s = val.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (buf.Kind == T3Value.ValueKind.Array)
                {
                    int maxLen = Math.Min(s.Length, buf.ArrayLength - 1);
                    buf.SetElement(0, T3Value.FromInt(maxLen));
                    for (int i = 0; i < maxLen; i++)
                        buf.SetElement(i + 1, T3Value.FromInt((int)s[i]));
                }
                result = T3Value.FromInt(0);
                return true;
            }

            // === <tmath.h> — Math functions ===
            if (fn == "abs" || fn == "t_abs")
            {
                long v = eval(fc.Arguments[0]).AsInt();
                result = T3Value.FromInt(v < 0 ? -v : v);
                return true;
            }
            if (fn == "min" || fn == "t_min")
            {
                long a = eval(fc.Arguments[0]).AsInt();
                long b = eval(fc.Arguments[1]).AsInt();
                result = T3Value.FromInt(a < b ? a : b);
                return true;
            }
            if (fn == "max" || fn == "t_max")
            {
                long a = eval(fc.Arguments[0]).AsInt();
                long b = eval(fc.Arguments[1]).AsInt();
                result = T3Value.FromInt(a > b ? a : b);
                return true;
            }
            if (fn == "clamp")
            {
                long v = eval(fc.Arguments[0]).AsInt();
                long lo = eval(fc.Arguments[1]).AsInt();
                long hi = eval(fc.Arguments[2]).AsInt();
                result = v < lo ? T3Value.FromInt(lo) : (v > hi ? T3Value.FromInt(hi) : T3Value.FromInt(v));
                return true;
            }
            if (fn == "t_sqrt")
            {
                double x = eval(fc.Arguments[0]).AsFloat();
                if (x < 0) throw new ArithmeticException("sqrt of negative number");
                result = T3Value.FromFloat(Math.Sqrt(x));
                return true;
            }
            if (fn == "t_floor")
            {
                result = T3Value.FromFloat(Math.Floor(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_ceil")
            {
                result = T3Value.FromFloat(Math.Ceiling(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_sin")
            {
                result = T3Value.FromFloat(Math.Sin(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_cos")
            {
                result = T3Value.FromFloat(Math.Cos(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_tan")
            {
                result = T3Value.FromFloat(Math.Tan(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_asin")
            {
                result = T3Value.FromFloat(Math.Asin(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_acos")
            {
                result = T3Value.FromFloat(Math.Acos(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_atan")
            {
                result = T3Value.FromFloat(Math.Atan(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_exp")
            {
                result = T3Value.FromFloat(Math.Exp(eval(fc.Arguments[0]).AsFloat()));
                return true;
            }
            if (fn == "t_log")
            {
                double x = eval(fc.Arguments[0]).AsFloat();
                if (x <= 0) throw new ArithmeticException("log of non-positive number");
                result = T3Value.FromFloat(Math.Log(x));
                return true;
            }
            if (fn == "t_log3")
            {
                double x = eval(fc.Arguments[0]).AsFloat();
                if (x <= 0) throw new ArithmeticException("log3 of non-positive number");
                result = T3Value.FromFloat(Math.Log(x, 3));
                return true;
            }
            if (fn == "t_pow")
            {
                double b = eval(fc.Arguments[0]).AsFloat();
                double e = eval(fc.Arguments[1]).AsFloat();
                result = T3Value.FromFloat(Math.Pow(b, e));
                return true;
            }

            // === Heap allocation ===
            if (fn == "malloc")
            {
                int n = (int)eval(fc.Arguments[0]).AsInt();
                // Note: heap state is managed externally via _heap list
                result = T3Value.FromInt(n); // placeholder, real impl in interpreter
                return true;
            }
            if (fn == "free")
            {
                result = T3Value.Void;
                return true;
            }

            return false; // not a nanolib function
        }

        /// <summary>Extract string from T3Value array (element 0 = length).</summary>
        static string GetString(T3Value v)
        {
            if (v.Kind == T3Value.ValueKind.Array)
            {
                int len = (int)v.GetElement(0).AsInt();
                char[] chars = new char[len];
                for (int i = 0; i < len; i++)
                    chars[i] = (char)v.GetElement(i + 1).AsInt();
                return new string(chars);
            }
            return "";
        }
    }
}