namespace T3Simulator.Common
{
    /// <summary>
    /// Decoded T3 instruction.
    /// </summary>
    public struct Instruction<TWord>
    {
        public readonly Opcode Opcode;
        public readonly int PredicateIndex; // 0: unconditional, 1-8: p1-p8
        public readonly TWord Operand1;     // Register index or immediate value
        public readonly TWord Operand2;     // Register index or immediate value

        public Instruction(Opcode opcode, int predicateIndex, TWord operand1, TWord operand2)
        {
            Opcode = opcode;
            PredicateIndex = predicateIndex;
            Operand1 = operand1;
            Operand2 = operand2;
        }
    }
}