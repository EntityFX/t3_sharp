using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Decodes 18-trit instructions into opcode and operands.
    /// Format: [Pred(3)] [Opcode(6)] [Args(9)]
    /// Uses integer division/modulo by powers of 3 — the exact inverse of
    /// InstructionEncoder which encodes as field * 3^position.
    /// </summary>
    public static class InstructionDecoder
    {
        private const long P3_15 = 14348907L; // 3^15
        private const long P3_12 = 531441L;   // 3^12
        private const long P3_9  = 19683L;    // 3^9
        private const long P3_6  = 729L;      // 3^6
        private const long P3_3  = 27L;       // 3^3

        // Offsets for balanced ternary fields (encoder adds offset, decoder subtracts)
        private const long OFFSET_3 = 13L;    // (27-1)/2
        private const long OFFSET_6 = 364L;   // (729-1)/2

        /// <summary>
        /// Extracts an unsigned integer field using standard arithmetic:
        /// (value / 3^startPos) % 3^width.
        /// This is the exact inverse of the encoder's field * 3^startPos.
        /// </summary>
        private static long ExtractUnsigned(Int128 value, int startPos, int width)
        {
            long divisor = Pow3(startPos);
            long modulo = Pow3(width);
            return (long)(value / divisor % modulo);
        }

        /// <summary>
        /// Extracts a balanced integer field: first extract unsigned, then subtract offset.
        /// Inverse of encoder's ToUnsignedField(value, range, offset).
        /// </summary>
        private static long ExtractBalanced(Int128 value, int startPos, int width, long offset)
        {
            return ExtractUnsigned(value, startPos, width) - offset;
        }

        private static long Pow3(int exp)
        {
            long result = 1;
            for (int i = 0; i < exp; i++) result *= 3;
            return result;
        }

        public static DecodedInstruction Decode(Word18 word)
        {
            Int128 val = word.ToInt128();
            
            // Pred and opcode are stored as raw unsigned integers
            int pred = (int)ExtractUnsigned(val, 15, 3);
            int opcodeVal = (int)ExtractUnsigned(val, 9, 6);
            
            var op = (Opcode)opcodeVal;
            int op1 = 0, op2 = 0, op3 = 0;
            long imm = 0;

            if (op.IsRType())
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                op2 = (int)ExtractBalanced(val, 3, 3, OFFSET_3);
                op3 = (int)ExtractBalanced(val, 0, 3, OFFSET_3);
            }
            else if (op == Opcode.LOADI || op == Opcode.STOREI)
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                op2 = (int)ExtractBalanced(val, 3, 3, OFFSET_3);
                imm = ExtractBalanced(val, 0, 3, OFFSET_3);
            }
            else if (op.IsIType())
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                imm = ExtractBalanced(val, 0, 6, OFFSET_6);
            }
            else if (op.IsJType())
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                imm = 0;
            }
            else
            {
                op1 = (int)ExtractBalanced(val, 6, 3, OFFSET_3);
                op2 = (int)ExtractBalanced(val, 3, 3, OFFSET_3);
                op3 = (int)ExtractBalanced(val, 0, 3, OFFSET_3);
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
        private const int REGISTER_OFFSET = 4;

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

        public int PhysOp1 => Op1 == 9 ? 9 : Op1 + REGISTER_OFFSET;
        public int PhysOp2 => Op2 == 9 ? 9 : Op2 + REGISTER_OFFSET;
        public int PhysOp3 => Op3 == 9 ? 9 : Op3 + REGISTER_OFFSET;
    }
}