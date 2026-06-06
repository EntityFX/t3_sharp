namespace T3Simulator.Common
{
    /// <summary>
    /// Opcodes for the T3 ternary processor.
    /// Values 0-27 are basic instructions.
    /// Values 28-44 are VLIW/SIMD instructions.
    /// </summary>
    public enum Opcode
    {
        HALT = 0,
        LOAD = 1,
        STORE = 2,
        MOV = 3,
        LI = 4,
        LIMM = 5,
        ADD = 6,
        SUB = 7,
        MUL = 8,
        DIV = 9,
        MOD = 10,
        NEG = 11,
        TRITAND = 12,
        TRITOR = 13,
        TRITXOR = 14,
        SHL = 15,
        SHR = 16,
        CMP = 17,
        JMP = 18,
        JE = 19,
        JNE = 20,
        JL = 21,
        JG = 22,
        JM = 23,
        CALL = 24,
        RET = 25,
        PUSH = 26,
        POP = 27,
        
        // VLIW / SIMD / Speculation
        SPEK = 28,
        COMMIT = 29,
        ROLLBACK = 30,
        VADD3 = 31,
        VSUB3 = 32,
        VMUL3 = 33,
        VDOT3 = 34,
        VCMP = 35,
        VTRITAND3 = 36,
        VTRITOR3 = 37,
        VTRITXOR3 = 38,
        VSHL3 = 39,
        VSHR3 = 40,
        IN = 41,
        OUT = 42,
        INI = 43,
        OUTI = 44
    }
}