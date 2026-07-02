namespace T3Simulator.Common
{
    /// <summary>Extension methods for Opcode classification.</summary>
    public static class OpcodeExtensions
    {
        /// <summary>Opcode supports R-type encoding (Fmt=0, three register args).</summary>
        public static bool SupportsRType(this Opcode op) => op switch
        {
            Opcode.MOV or Opcode.PUSH or Opcode.POP or
            Opcode.ADD or Opcode.SUB or Opcode.MUL or Opcode.DIV or Opcode.MOD or
            Opcode.NEG or Opcode.ABS or Opcode.CMP or
            Opcode.AND or Opcode.OR or Opcode.XOR or
            Opcode.SHL or Opcode.SHR or
            Opcode.IN or Opcode.OUT or
            Opcode.SQRT or Opcode.FTI or Opcode.ITF or
            Opcode.CLASS or Opcode.SWAP => true,
            _ => false
        };

        /// <summary>Opcode supports I-type encoding (Fmt=+1, reg + 6-trit imm).</summary>
        public static bool SupportsIType(this Opcode op) => op switch
        {
            Opcode.LIMM or
            Opcode.MOV or Opcode.PUSH or Opcode.POP or
            Opcode.ADD or Opcode.SUB or Opcode.MUL or Opcode.DIV or Opcode.MOD or
            Opcode.NEG or Opcode.CMP or
            Opcode.AND or Opcode.OR or Opcode.XOR or
            Opcode.SHL or Opcode.SHR or
            Opcode.IN or Opcode.OUT => true,
            _ => false
        };

        /// <summary>Opcode supports J-type encoding (Fmt=-1, register jump target).</summary>
        public static bool SupportsJType(this Opcode op) => op switch
        {
            Opcode.JMP or Opcode.JE or Opcode.JNE or Opcode.JL or Opcode.JG or
            Opcode.JM or Opcode.JLE or Opcode.JGE or Opcode.CALL => true,
            _ => false
        };

        /// <summary>Opcode uses S-type encoding (Fmt=0, Op1=reg, Op2=base, Imm3=offset).</summary>
        public static bool IsSType(this Opcode op) => op is Opcode.LD or Opcode.ST;

        /// <summary>Opcode is a control-flow instruction (J-type or RET).</summary>
        public static bool IsControlFlow(this Opcode op) => op.SupportsJType() || op == Opcode.RET;

        /// <summary>Opcode uses no operands at all.</summary>
        public static bool IsZeroOperand(this Opcode op) => op is Opcode.HALT or Opcode.NOP or Opcode.RET;
    }
}