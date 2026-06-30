using T3Simulator.Common;

namespace T3Simulator.Common
{
    /// <summary>Extension methods for Opcode classification — shared between Encoder and Decoder.</summary>
    public static class OpcodeExtensions
    {
        public static bool IsRType(this Opcode op) => op switch
        {
            Opcode.ADD or Opcode.SUB or Opcode.MUL or Opcode.DIV or Opcode.MOD or Opcode.NEG or
            Opcode.AND or Opcode.OR or Opcode.XOR or Opcode.SHL or Opcode.SHR or
            Opcode.MOV or Opcode.CMP or Opcode.LOAD or Opcode.STORE or Opcode.PUSH or Opcode.POP or
            Opcode.IN or Opcode.OUT or
            Opcode.FADD or Opcode.FSUB or Opcode.FMUL or Opcode.FDIV or Opcode.FSQRT or
            Opcode.FABS or Opcode.FNEG or Opcode.FCMP or Opcode.FTOF or Opcode.FSWAP or
            Opcode.FMOV or Opcode.FTOI or Opcode.FCLASS or Opcode.ITOF or Opcode.FLW or Opcode.FSW => true,
            _ => false
        };

        public static bool IsIType(this Opcode op) => op switch
        {
            Opcode.MOVI or Opcode.LI or Opcode.LIMM or
            Opcode.ADDI or Opcode.SUBI or Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or
            Opcode.ANDI or Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or
            Opcode.LOADI or Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.FZERO or
            Opcode.PUSHI or Opcode.POPI => true,
            _ => false
        };

        public static bool IsJType(this Opcode op) => op switch
        {
            Opcode.JMP or Opcode.JE or Opcode.JNE or Opcode.JL or Opcode.JG or Opcode.JM or
            Opcode.JLE or Opcode.JGE or Opcode.CALL => true,
            _ => false
        };
    }
}