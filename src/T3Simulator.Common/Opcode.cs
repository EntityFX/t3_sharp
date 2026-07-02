namespace T3Simulator.Common
{
    /// <summary>
    /// Opcodes for the T3 ternary processor — ISA v5 (orthogonal with RegGroup + Fmt).
    /// Format: [Pred(3)] [RegGroup(1)] [Fmt(1)] [Opcode(4)] [Args(9)]
    /// Opcode values are within the range [0, 80] (3^4 = 81 values).
    /// No R/I duplicates — Fmt distinguishes argument layout.
    /// </summary>
    public enum Opcode
    {
        // System
        HALT = 0,
        NOP = 1,

        // Special (always I-type for LIMM)
        LIMM = 2,

        // Data movement (Fmt=0=R-type, Fmt=+1=I-type)
        MOV = 5,

        // Memory (Fmt=0, S-layout: Op1=reg, Op2=base, Imm3=offset)
        LD  = 6,
        ST  = 7,

        // Stack (Fmt=0=R-type, Fmt=+1=I-type)
        PUSH = 8,
        POP  = 9,

        // Arithmetic (Fmt=0=R-type, Fmt=+1=I-type)
        ADD = 10,
        SUB = 11,
        MUL = 12,
        DIV = 13,
        MOD = 14,
        NEG = 15,
        ABS = 16,   // R-type only

        // Comparison (Fmt=0=R-type, Fmt=+1=I-type)
        CMP = 17,

        // Logical (Fmt=0=R-type, Fmt=+1=I-type)
        AND = 18,
        OR  = 19,
        XOR = 20,

        // Shifts (Fmt=0=R-type, Fmt=+1=I-type)
        SHL = 21,
        SHR = 22,

        // I/O (Fmt=0=R-type, Fmt=+1=I-type)
        IN  = 23,
        OUT = 24,

        // FPU-specific (Fmt=0, R-type, only with RegGroup=-1)
        SQRT = 25,

        // Cross-group conversions (Fmt=0, R-type)
        FTI   = 26,   // Float→Int (was FTOI)
        ITF   = 27,   // Int→Float (was ITOF)
        CLASS = 28,   // Float classify (was FCLASS)
        SWAP  = 29,   // Exchange registers (was FSWAP)

        // === Control Flow (Fmt=-1, J-type) ===
        JMP  = 40,
        JE   = 41,
        JNE  = 42,
        JL   = 43,
        JG   = 44,
        JLE  = 45,
        JGE  = 46,
        JM   = 47,   // Jump Maybe: when CD == 0
        CALL = 48,
        RET  = 49,
    }
}