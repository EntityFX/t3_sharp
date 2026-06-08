namespace T3Simulator.Common
{
    /// <summary>
    /// Decoded T3 instruction.
    /// </summary>
    public struct Instruction<TWord>
    {
        public readonly Opcode Opcode;
        public readonly int PredicateIndex; // 0: unconditional, 1-3: p0-p2
        public readonly int Op1;            // Register index
        public readonly int Op2;            // Register index
        public readonly int Op3;            // Register index (for R-type)
        public readonly long Immediate;     // Immediate value (for I-type)

        // Aliases for compatibility
        public int Operand1 => Op1;
        public int Operand2 => Op2;

        public Instruction(Opcode opcode, int predicateIndex, int op1, int op2, int op3, long immediate)
        {
            Opcode = opcode;
            PredicateIndex = predicateIndex;
            Op1 = op1;
            Op2 = op2;
            Op3 = op3;
            Immediate = immediate;
        }

        // Helper for R-type
        public Instruction(Opcode opcode, int predicateIndex, int op1, int op2, int op3) 
            : this(opcode, predicateIndex, op1, op2, op3, 0) { }

        // Helper for I-type
        public Instruction(Opcode opcode, int predicateIndex, int op1, int op2, long immediate) 
            : this(opcode, predicateIndex, op1, op2, 0, immediate) { }
    }
}
