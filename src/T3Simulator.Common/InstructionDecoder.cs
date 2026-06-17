using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes 18-trit instructions into opcode and operands.
    /// Format: [Pred(3)] [Opcode(6)] [Args(9)]
    /// No string operations. Uses arithmetic division/modulo by powers of 3.
    /// </summary>
    public static class InstructionDecoder
    {
        private const long P3_15 = 14348907L;
        private const long P3_12 = 531441L;
        private const long P3_9  = 19683L;
        private const long P3_6  = 729L;
        private const long P3_3  = 27L;

        public static DecodedInstruction Decode(Word18 word)
        {
            long val = word.ToLong();
            int pred = (int)(val / P3_15);
            val -= pred * P3_15;
            int opcode = (int)(val / P3_9);
            long args = val % P3_9;

            var op = (Opcode)opcode;
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            if (IsRType(op))
            {
                op1 = FromTernary((int)(args / P3_6)); args -= (args / P3_6) * P3_6;
                op2 = FromTernary((int)(args / P3_3));
                op3 = FromTernary((int)(args % P3_3));
            }
            else if (IsIType(op))
            {
                op1 = FromTernary((int)(args / P3_6));
                imm = FromTernary6((int)(args % P3_6));
            }
            else if (IsJType(op))
            {
                op1 = FromTernary((int)(args / P3_6));
                op2 = FromTernary((int)(args / P3_6));
            }
            else
            {
                op1 = FromTernary((int)(args / P3_6)); args -= (args / P3_6) * P3_6;
                op2 = FromTernary((int)(args / P3_3));
                op3 = FromTernary((int)(args % P3_3));
            }

            return new DecodedInstruction(op, pred, op1, op2, op3, imm);
        }

        // For Word54: extract last 18 trits
        public static DecodedInstruction Decode(Word54 word)
        {
            long val = word.ToLong();
            return Decode(Word18.FromLong(val % P3_15));
        }

        public static DecodedInstruction Decode<TWord>(TWord word) where TWord : IT3Word<TWord>
        {
            return Decode(Word18.FromLong(word.ToLong()));
        }

        private static bool IsRType(Opcode op) => op switch
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

        private static bool IsIType(Opcode op) => op switch
        {
            Opcode.MOVI or Opcode.LI or Opcode.LIMM or
            Opcode.ADDI or Opcode.SUBI or Opcode.MULI or Opcode.DIVI or Opcode.MODI or Opcode.NEGI or
            Opcode.ANDI or Opcode.ORI or Opcode.XORI or Opcode.SHLI or Opcode.SHRI or
            Opcode.LOADI or Opcode.STOREI or Opcode.CMPI or Opcode.INI or Opcode.OUTI or Opcode.FZERO => true,
            _ => false
        };

        private static bool IsJType(Opcode op) => op switch
        {
            Opcode.JMP or Opcode.JE or Opcode.JNE or Opcode.JL or Opcode.JG or Opcode.JM or
            Opcode.JLE or Opcode.JGE or Opcode.CALL => true,
            _ => false
        };

        public static int FromTernary(int value) => value - 4;
        private static long FromTernary6(int unsigned) => unsigned - 364;
    }

    public struct DecodedInstruction
    {
        public Opcode Opcode;
        public int Predicate;
        public int Op1;
        public int Op2;
        public int Op3;
        public long Immediate;

        public DecodedInstruction(Opcode op, int pred, int op1, int op2, int op3, long imm)
        {
            Opcode = op; Predicate = pred;
            Op1 = op1; Op2 = op2; Op3 = op3; Immediate = imm;
        }

        public int PhysOp1 => Op1 + 4;
        public int PhysOp2 => Op2 + 4;
        public int PhysOp3 => Op3 + 4;
    }
}