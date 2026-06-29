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

        // Offsets for balanced ternary fields
        private const long OFFSET_3 = 13L;    // (27-1)/2
        private const long OFFSET_6 = 364L;   // (729-1)/2

        public static DecodedInstruction Decode(Word18 word)
        {
            Int128 val = word.ToInt128();
            
            // Pred and opcode are stored as raw non-negative integers (not balanced ternary)
            int pred = (int)TritTypes.TernaryMath.ExtractRawField(val, 15, 3);
            int opcodeVal = (int)TritTypes.TernaryMath.ExtractRawField(val, 9, 6);
            // Extract the raw args field (unsigned balanced ternary encoding: value + offset)
            Int128 rawArgs = TritTypes.TernaryMath.ExtractRawField(val, 0, 9);
            
            var op = (Opcode)opcodeVal;
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            if (op.IsRType())
            {
                // Extract sub-fields from raw args and convert to balanced by subtracting offset
                op1 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 6, 3) - OFFSET_3);
                op2 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 3, 3) - OFFSET_3);
                op3 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 0, 3) - OFFSET_3);
            }
            else if (op == Opcode.LOADI || op == Opcode.STOREI)
            {
                op1 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 6, 3) - OFFSET_3);
                op2 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 3, 3) - OFFSET_3);
                imm = (long)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 0, 3) - OFFSET_3);
            }
            else if (op.IsIType())
            {
                op1 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 6, 3) - OFFSET_3);
                imm = (long)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 0, 6) - OFFSET_6);
            }
            else if (op.IsJType())
            {
                // J-type: [Pred(3)][Opcode(6)][Reg(3)][000000]
                // The lower 6 trits are padding (all zeros), so imm is always 0.
                op1 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 6, 3) - OFFSET_3);
                imm = 0;
            }
            else
            {
                op1 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 6, 3) - OFFSET_3);
                op2 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 3, 3) - OFFSET_3);
                op3 = (int)(TritTypes.TernaryMath.ExtractRawField(rawArgs, 0, 3) - OFFSET_3);
            }

            return new DecodedInstruction(op, pred, op1, op2, op3, imm);
        }

        // For Word54: extract lower 18 trits as a basic instruction using wrap semantics
        public static DecodedInstruction Decode(Word54 word)
        {
            return Decode(Word18.FromWrappedLong(word.ToAddress()));
        }

        public static DecodedInstruction Decode<TWord>(TWord word) where TWord : IT3Word<TWord>
        {
            if (word is Word54 w54) return Decode(w54);
            if (word is Word18 w18) return Decode(w18);
            return Decode(Word18.FromLong(word.ToLong()));
        }

        private const int REGISTER_OFFSET = 4;
        private const long IMM_OFFSET = 364;
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