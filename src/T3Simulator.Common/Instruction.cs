using System.Numerics;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// A decoded instruction for the T3 processor.
    /// </summary>
    public readonly struct Instruction
    {
        public readonly Opcode Opcode;
        public readonly int PredicateIndex; // 0: unconditional, 1-8: p1-p8
        public readonly BigInteger Operand1;     // Register index or immediate value
        public readonly BigInteger Operand2;     // Register index or immediate value

        public Instruction(Opcode opcode, int predicateIndex, BigInteger operand1, BigInteger operand2)
        {
            Opcode = opcode;
            PredicateIndex = predicateIndex;
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public bool IsUnconditional => PredicateIndex == 0;

        public override string ToString()
        {
            string predStr = IsUnconditional ? "" : $" (p{PredicateIndex})";
            return $"{Opcode}{predStr} op1={Operand1}, op2={Operand2}";
        }
    }
}