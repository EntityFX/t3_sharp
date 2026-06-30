namespace T3Simulator.Common
{
    /// <summary>
    /// Opcodes for the T3 ternary processor.
    /// Redesigned for a fixed-field format: [Pred(3)][Opcode(6)][Args(9)].
    /// Opcode values are within the range [0, 728] (3^6).
    /// </summary>
    public enum Opcode
    {
        // System
        HALT = 0,
        NOP = 1,

        // Data Movement
        MOV = 2,
        MOVI = 3,
        LI = 4,
        LIMM = 5,

        // Arithmetic (R-type)
        ADD = 10,
        SUB = 11,
        MUL = 12,
        DIV = 13,
        MOD = 14,
        NEG = 15,

        // Arithmetic (I-type)
        ADDI = 20,
        SUBI = 21,
        MULI = 22,
        DIVI = 23,
        MODI = 24,
        NEGI = 25,

        // Logical
        AND = 30,
        OR = 31,
        XOR = 32,
        ANDI = 33,
        ORI = 34,
        XORI = 35,

        // Shifts
        SHL = 40,
        SHR = 41,
        SHLI = 42,
        SHRI = 43,

        // Memory & Stack
        LOAD = 50,
        LOADI = 51,
        STORE = 52,
        STOREI = 53,
        PUSH = 54,
        POP = 55,

        // Control Flow
        CMP = 60,
        CMPI = 61,
        JMP = 62,
        JE = 63,
        JNE = 64,
        JL = 65,
        JG = 66,
        JM = 67,   // Jump Maybe: conditional jump when Cond == 0 (ternary maybe/unstable state)
        JLE = 68,
        JGE = 69,
        CALL = 70,
        RET = 71,

        // I/O
        IN = 80,
        OUT = 81,
        INI = 82,
        OUTI = 83,

        // FPU Instructions (mapped to a separate block)
        FADD = 100,
        FSUB = 101,
        FMUL = 102,
        FDIV = 103,
        FSQRT = 104,
        FABS = 105,
        FNEG = 106,
        FCMP = 107,
        FTOI = 108,
        ITOF = 109,
        FTOF = 110,
        FLW = 111,
        FSW = 112,
        FMOV = 113,
        FCLASS = 114,
        FSWAP = 115,
        FZERO = 116,

        // Stack immediate
        PUSHI = 117,
        POPI = 118,

    }
}
