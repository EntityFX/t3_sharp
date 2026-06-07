using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace TritTypes
{
    /// <summary>
    /// T-SCII (Ternary Standard Code for Information Interchange) encoding support.
    /// </summary>
    public static class TScii
    {
        private const int Offset = 364;

        // Tables for digit representations
        private static readonly char[] NineDigits = { 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4' };
        private static readonly string NineDigitChars = new string(NineDigits);
        private static readonly char[] T27Digits = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
        private static readonly string T27DigitChars = new string(T27Digits);

        // Full character map (U 0..728)
        private static readonly char[] symbols = new char[729];
        private static readonly Dictionary<char, int> charToU = new Dictionary<char, int>();

        static TScii()
        {
            // 1. Additional symbols (U = 0 … 363)
            char[] additional1 = new char[]
            {
                'Α','Β','Γ','Δ','Ε','Ζ','Η','Θ','Ι','Κ','Λ','Μ','Ν','Ξ','Ο','Π','Ρ','Σ','Τ','Υ','Φ','Χ','Ψ','Ω','α','β','γ',
                'δ','ε','ζ','η','θ','ι','κ','λ','μ','ν','ξ','ο','π','ρ','ς','σ','τ','υ','φ','χ','ψ','ω','ϑ','ϒ','ϖ','Ϙ','Ϟ',
                'Ϡ','Ϣ','←','↑','→','↓','↔','↕','↖','↗','↘','↙','↚','↛','↜','↝','↞','↟','↠','↡','↢','↣','↤','↥','↦','↧','↨',
                '↩','↪','↫','↬','↭','↮','↯','↰','↱','↲','↳','↴','↵','↶','↷','↸','↹','↺','↻','↼','↽','↾','↿',
                '⇀','⇁','⇂','⇃','⇄','⇅','⇆','⇇','⇈','⇉','⇊','⇋','⇌','⇍','⇎','⇏','⇐','⇑','⇒','⇓','⇔','⇕','⇖','⇗','⇘','⇙','⇚','⇛','⇜','⇝','⇞','⇟',
                '⇠','⇡','⇢','⇣','⇤','⇥','⇦','⇧','⇨','⇩','⇪','⇫','⇬','⇭','⇮','⇯','⇰','⇱','⇲','⇳','⇴','⇵','⇶','⇷','⇸','⇹',
                '⇺','⇻','⇼','⇽','⇾','⇿','∀','∁','∂','∃','∄','∅','∆','∇','∈','∉','∊','∋','∌','∍','∎','∏','∐','∑','−','∓','∔',
                '∕','∖','∗','∘','∙','√','∛','∜','∝','∞','∟','∠','∡','∢','∣','∤','∥','∦',
                '∧','∨','∩','∪','∫','∬','∭','∮','∯','∰','∱','∲','∳','∴','∵','∶','∷','∸','∹','∺','∻','∼','∽','∾','∿',
                '≀','≁','≂','≃','≄','≅','≆','≇','≈','≉','≊',
                '≋','≌','≍','≎','≏','≐','≑','≒','≓','≔','≕','≖','≗','≘','≙','≚','≛','≜','≝','≞','≟','≠','≡','≢','≣','≤','≥',
                '≦','≧','≨','≩','≪','≫','≬','≭','≮','≯',
                '≰','≱','≲','≳','≴','≵','≶','≷','≸','≹','≺','≻','≼','≽','≾','≿',
                '⊀','⊁','⊂','⊃','⊄','⊅','⊆','⊇','⊈','⊉','⊊','⊋','⊌','⊍','⊎','⊏','⊐','⊑','⊒','⊓','⊔','⊕','⊖','⊗','⊘','⊙','⊚','⊛',
                '⊜','⊝','⊞','⊟','⊠','⊡','⊢','⊣','⊤','⊥','⊦','⊧','⊨','⊩','⊪','⊫','⊬','⊭','⊮','⊯',
                '⊰','⊱','⊲','⊳','⊴','⊵','⊶','⊷','⊸','⊹','⊺','⊻','⊼','⊽','⊾','⊿',
                '⋀','⋁','⋂','⋃','⋄','⋅','⋆','⋇','⋈','⋉','⋊','⋋','⋌','⋍','⋎','⋏','⋐','⋑','⋒','⋓','⋔','⋕','⋖','⋗','⋘','⋙','⋚','⋛','⋜','⋝','⋞','⋟',
                '⋠','⋡','⋢','⋣','⋤','⋥','⋦','⋧','⋨','⋩','⋪','⋫','⋬','⋭','⋮','⋯','⋰','⋱','⋲','⋳','⋴','⋵','⋶','⋷','⋸','⋹','⋺','⋻','⋼','⋽','⋾','⋿',
                '⌀','⌁','⌂','⌃','⌄','⌅','⌆','⌇','⌈','⌉','⌊','⌋','⌌','⌍','⌎','⌏','⌐','⌑','⌒','⌓','⌔','⌕','⌖','⌗','⌘','⌙','⌚','⌛','⌜','⌝','⌞','⌟',
                '⌠','⌡','⌢','⌣','⌤','⌥','⌦','⌧','⌨','〈','⌫','⌬','⌭','⌮','⌯',
                '⌰','⌱','⌲','⌳','⌴','⌵','⌶','⌷','⌸','⌹','⌺','⌻','⌼','⌽','⌾','⌿',
                '⍀','⍁','⍂','⍃','⍄','⍅','⍆','⍇','⍈','⍉','⍊','⍋','⍌','⍍','⍎','⍏','⍐','⍑','⍒','⍓','⍔','⍕','⍖','⍗','⍘','⍙','⍚','⍛','⍜','⍝','⍞','⍟',
                '⍠','⍡','⍢','⍣','⍤','⍥','⍦','⍧','⍨','⍩','⍪','⍫','⍬','⍭','⍮','⍯',
                '⍰','⍱','⍲','⍳','⍴','⍵','⍶','⍷','⍸','⍹','⍺','⍻','⍼','⍽','⍾','⍿',
                '⎀','⎁','⎂','⎃','⎄','⎅','⎆','⎇','⎈','⎉','⎊','⎋','⎌','⎍','⎎','⎏','⎐','⎑','⎒','⎓','⎔','⎕','⎖','⎗','⎘','⎙','⎚','⎛','⎜','⎝','⎞','⎟',
                '⎠','⎡','⎢','⎣','⎤','⎥','⎦','⎧','⎨','⎩','⎪','⎫','⎬','⎭','⎮','⎯'
            };
            for (int i = 0; i < 364; i++)
                symbols[i] = additional1[i];

            // 2. CP1251 (U = 364 … 619)
            char[] cp1251 = new char[256]
            {
                '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
                '\b',   '\t',   '\n',   '\v',   '\f',   '\r',   '\u000E', '\u000F',
                '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
                '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
                ' ', '!', '"', '#', '$', '%', '&', '\'',
                '(', ')', '*', '+', ',', '-', '.', '/',
                '0', '1', '2', '3', '4', '5', '6', '7',
                '8', '9', ':', ';', '<', '=', '>', '?',
                '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
                'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W',
                'X', 'Y', 'Z', '[', '\\', ']', '^', '_',
                '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
                'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
                'x', 'y', 'z', '{', '|', '}', '~', '\u007F',
                'Ђ', 'Ѓ', '‚', 'ѓ', '„', '…', '†', '‡',
                '€', '‰', 'Љ', '‹', 'Њ', 'Ќ', 'Ћ', 'Џ',
                'ђ', '‘', '’', '“', '”', '•', '–', '—',
                '˜', '™', 'љ', '›', 'њ', 'ќ', 'ћ', 'џ',
                '\u00A0', 'Ў', 'ў', 'Ј', '¤', 'Ґ', '¦', '§',
                'Ё', '©', 'Є', '«', '¬', '\u00AD', '®', 'Ї',
                '°', '±', 'І', 'і', 'ґ', 'µ', '¶', '·',
                'ё', '№', 'є', '»', 'ј', 'Ѕ', 'ѕ', 'ї',
                'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З',
                'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П',
                'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч',
                'Ш', 'Щ', 'Ъ', 'Ы', 'Ь', 'Э', 'Ю', 'Я',
                'а', 'б', 'в', 'г', 'д', 'е', 'ж', 'з',
                'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п',
                'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч',
                'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я'
            };
            for (int code = 0; code < 256; code++)
                symbols[364 + code] = cp1251[code];

            // 3. Additional symbols (U = 620 … 728)
            char[] additional2 = new char[]
            {
                '░','▒','▓','█','▀','▄','▌','▐','░','▒','▓','█','▀','▄','▌','▐','░','▒','▓','█','␀',
                '■','□','▪','▫','▬','▭','▮','▯','▰','▱','▲','△','▴','▵','▶','▷','▸','▹','►','▻','▼','▽','▾','▿',
                '◀','◁','◂','◃','◄','◅','◆','◇','◈','◉','◊','○','◌','◎','●','◐','◑','◒','◓','◔','◕','◖','◗','◘','◙','◚','◛','◜','◝','◞','◟',
                '◠','◡','◢','◣','◤','◥','◦','◧','◨','◩','◪','◫','◬','◭','◮','◯',
                '◰','◱','◲','◳','◴','◵','◶','◷','◸','◹','◺','◻','◼','◽','◾','◿'
            };
            for (int i = 0; i < 108; i++)
                symbols[620 + i] = additional2[i];

            // 4. Inverse dictionary
            for (int u = 0; u < 729; u++)
            {
                char c = symbols[u];
                if (c <= '\u001F' || (c >= '\u007F' && c <= '\u009F'))
                    continue;
                if (!charToU.ContainsKey(c))
                    charToU[c] = u;
            }
        }

        public static char ToChar(Int128 value)
        {
            int v = (int)value;
            if (v < -364 || v > 364)
                throw new ArgumentOutOfRangeException(nameof(value), "Value outside T-SCII range (-364 to 364).");

            return symbols[v + Offset];
        }

        public static Int128 FromChar(char c)
        {
            if (charToU.TryGetValue(c, out int u))
            {
                return (Int128)(u - Offset);
            }
            
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i] == c) return (Int128)(i - Offset);
            }
            
            throw new NotSupportedException($"Character '{c}' is not supported by the current T-SCII implementation.");
        }

        public static string ToNinary(Int128 value)
        {
            int u = (int)value + Offset;
            int d1 = u / 81, d2 = (u / 9) % 9, d3 = u % 9;
            return $"0n{NineDigits[d1]}{NineDigits[d2]}{NineDigits[d3]}";
        }

        public static string ToTryx(Int128 value)
        {
            int u = (int)value + Offset;
            int h = u / 27, l = u % 27;
            return $"0y{T27Digits[h]}{T27Digits[l]}";
        }

        public static string ToTritString(Int128 value)
        {
            int u = (int)value + Offset;
            char[] trits = new char[6];
            for (int i = 5; i >= 0; i--)
            {
                int rem = u % 3;
                trits[i] = rem == 0 ? '-' : (rem == 1 ? '0' : '+');
                u /= 3;
            }
            return "0t" + new string(trits);
        }

        public static int ParseLiteral(string literal)
        {
            if (string.IsNullOrEmpty(literal)) throw new ArgumentException("Пустая строка");
            literal = literal.Trim();
            if (literal.StartsWith("0t"))
            {
                string s = literal[2..].Replace("_", "");
                if (s.Length != 6) throw new FormatException("Троичный литерал должен содержать ровно 6 тритов");
                int u = 0;
                foreach (char c in s)
                {
                    u = u * 3 + (c == '-' ? 0 : c == '0' ? 1 : c == '+' ? 2 :
                        throw new FormatException($"Недопустимый символ '{c}'"));
                }
                return u - Offset;
            }
            if (literal.StartsWith("0n"))
            {
                string s = literal[2..].ToUpperInvariant();
                if (s.Length != 3) throw new FormatException("9‑ричный литерал должен содержать ровно 3 цифры");
                int u = 0;
                foreach (char c in s)
                {
                    int idx = NineDigitChars.IndexOf(c);
                    if (idx < 0) throw new FormatException($"Недопустимая 9‑ричная цифра '{c}'");
                    u = u * 9 + idx;
                }
                return u - Offset;
            }
            if (literal.StartsWith("0y"))
            {
                string s = literal[2..].ToUpperInvariant();
                if (s.Length != 2) throw new FormatException("27‑ричный литерал должен содержать ровно 2 цифры");
                int u = 0;
                foreach (char c in s)
                {
                    int idx = T27DigitChars.IndexOf(c);
                    if (idx < 0) throw new FormatException($"Недопустимая 27‑ричная цифра '{c}'");
                    u = u * 27 + idx;
                }
                return u - Offset;
            }
            if (literal.EndsWith("t"))
            {
                string num = literal[..^1];
                if (!int.TryParse(num, out int v) || v < -364 || v > 364)
                    throw new FormatException("Десятичный литерал должен быть целым числом от -364 до 364");
                return v;
            }
            throw new FormatException("Неизвестный формат литерала T‑SCII");
        }

        public static Int128[] FromString(string text)
        {
            Int128[] values = new Int128[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                values[i] = FromChar(text[i]);
            }
            return values;
        }
    }
}