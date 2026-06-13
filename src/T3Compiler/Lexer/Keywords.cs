using System.Collections.Generic;

namespace T3Compiler.Lexer
{
    public static class Keywords
    {
        private static readonly Dictionary<string, TokenType> _map = new()
        {
            // English keywords
            { "void", TokenType.KwVoid },
            { "trit", TokenType.KwTrit },
            { "tril", TokenType.KwTril },
            { "tryte", TokenType.KwTryte },
            { "tshort", TokenType.KwTshort },
            { "tint", TokenType.KwTint },
            { "tlong", TokenType.KwTlong },
            { "tfloat", TokenType.KwTfloat },
            { "tdouble", TokenType.KwTdouble },
            { "if", TokenType.KwIf },
            { "maybe", TokenType.KwMaybe },
            { "else", TokenType.KwElse },
            { "switch", TokenType.KwSwitch },
            { "case", TokenType.KwCase },
            { "default", TokenType.KwDefault },
            { "while", TokenType.KwWhile },
            { "do", TokenType.KwDo },
            { "for", TokenType.KwFor },
            { "break", TokenType.KwBreak },
            { "continue", TokenType.KwContinue },
            { "return", TokenType.KwReturn },
            { "goto", TokenType.KwGoto },
            { "struct", TokenType.KwStruct },
            { "union", TokenType.KwUnion },
            { "enum", TokenType.KwEnum },
            { "typedef", TokenType.KwTypedef },
            { "const", TokenType.KwConst },
            { "volatile", TokenType.KwVolatile },
            { "signed", TokenType.KwSigned },
            { "unsigned", TokenType.KwUnsigned },
            { "true", TokenType.KwTrue },
            { "false", TokenType.KwFalse },
            { "extern", TokenType.KwExtern },
            { "static", TokenType.KwStatic },
            { "auto", TokenType.KwAuto },
            { "register", TokenType.KwRegister },
            { "inline", TokenType.KwInline },
            { "sizeof", TokenType.KwSizeof },

            // Russian keywords
            { "пусто", TokenType.KwVoid },
            { "трит", TokenType.KwTrit },
            { "трил", TokenType.KwTril },
            { "трайт", TokenType.KwTryte },
            { "тшорт", TokenType.KwTshort },
            { "ткоротк", TokenType.KwTshort },
            { "тцел", TokenType.KwTint },
            { "тдлин", TokenType.KwTlong },
            { "тдлинлонг", TokenType.KwTlongLong },
            { "тдлиндлин", TokenType.KwTlongLong },
            { "твещ", TokenType.KwTfloat },
            { "тдвойн", TokenType.KwTdouble },
            { "если", TokenType.KwIf },
            { "может", TokenType.KwMaybe },
            { "иначе", TokenType.KwElse },
            { "выбор", TokenType.KwSwitch },
            { "случай", TokenType.KwCase },
            { "умолчание", TokenType.KwDefault },
            { "пока", TokenType.KwWhile },
            { "делать", TokenType.KwDo },
            { "для", TokenType.KwFor },
            { "прервать", TokenType.KwBreak },
            { "продолжить", TokenType.KwContinue },
            { "возврат", TokenType.KwReturn },
            { "перейти", TokenType.KwGoto },
            { "структура", TokenType.KwStruct },
            { "объединение", TokenType.KwUnion },
            { "перечисление", TokenType.KwEnum },
            { "типрег", TokenType.KwTypedef },
            { "конст", TokenType.KwConst },
            { "изменч", TokenType.KwVolatile },
            { "знак", TokenType.KwSigned },
            { "беззнак", TokenType.KwUnsigned },
            { "истина", TokenType.KwTrue },
            { "ложь", TokenType.KwFalse },
            { "внеш", TokenType.KwExtern },
            { "статич", TokenType.KwStatic },
            { "авто", TokenType.KwAuto },
            { "регистр", TokenType.KwRegister },
            { "встроен", TokenType.KwInline },
            { "размер", TokenType.KwSizeof },

            // Reserved but not used
            { "short", TokenType.KwTshort },
            { "int", TokenType.KwTint },
            { "long", TokenType.KwTlong },
            { "char", TokenType.KwTryte },
            { "float", TokenType.KwTfloat },
            { "double", TokenType.KwTdouble },
        };

        public static TokenType? TryGetKeyword(string identifier)
        {
            if (_map.TryGetValue(identifier, out var kw))
                return kw;
            return null;
        }

        public static bool IsTypeSpecifier(TokenType t) => t switch
        {
            TokenType.KwVoid or TokenType.KwTrit or TokenType.KwTril
         or TokenType.KwTryte or TokenType.KwTshort or TokenType.KwTint
         or TokenType.KwTlong or TokenType.KwTlongLong
         or TokenType.KwTfloat or TokenType.KwTdouble => true,
            _ => false
        };
    }
}