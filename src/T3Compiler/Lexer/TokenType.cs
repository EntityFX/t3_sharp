namespace T3Compiler.Lexer
{
    public enum TokenType
    {
        // Literals
        IntegerLiteral,     // 42y, 0t+-0, 0yNN, 0nW04
        FloatLiteral,       // 3.14f, 1.0e+2d
        CharLiteral,        // '+', 'A'
        StringLiteral,      // "text"

        // Identifiers
        Identifier,

        // Keywords (all bilingual)
        KwVoid, KwTrit, KwTril, KwTryte, KwTshort, KwTint, KwTlong, KwTlongLong,
        KwTfloat, KwTdouble,
        KwIf, KwMaybe, KwElse,
        KwSwitch, KwCase, KwDefault,
        KwWhile, KwDo, KwFor,
        KwBreak, KwContinue, KwReturn, KwGoto,
        KwStruct, KwUnion, KwEnum, KwTypedef,
        KwConst, KwVolatile,
        KwSigned, KwUnsigned,
        KwTrue, KwFalse, KwNull,
        KwExtern, KwStatic, KwAuto, KwRegister, KwInline,
        KwSizeof,

        // Operators
        OpPlus, OpMinus, OpStar, OpSlash, OpPercent,
        OpAmpersand, OpPipe, OpCaret, OpTilde,
        OpExclamation, OpQuestion, OpColon,
        OpEq, OpPlusEq, OpMinusEq, OpStarEq, OpSlashEq, OpPercentEq,
        OpAmpEq, OpPipeEq, OpCaretEq,
        OpLShift, OpRShift, OpLShiftEq, OpRShiftEq,
        OpLt, OpGt, OpLtEq, OpGtEq, OpEqEq, OpNeq,
        OpPlusPlus, OpMinusMinus,
        OpAndAnd, OpPipePipe,
        OpTernaryQuestion,  // ??
        OpTernaryMaybe,     // :?
        OpTernaryFalse,     // :!
        OpArrow, OpDot,

        // Delimiters
        LParen, RParen, LBrace, RBrace, LBracket, RBracket,
        Semicolon, Comma,

        // Preprocessor
        Hash,               // # at start of line

        // Special
        EndOfFile, Unknown
    }
}