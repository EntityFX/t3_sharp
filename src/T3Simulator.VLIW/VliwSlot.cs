using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.VLIW
{
    /// <summary>
    /// Represents a single 18-trit slot within a VLIW bundle.
    /// Format: opcode(6), op1(6), op2(6)
    /// </summary>
    public readonly struct VliwSlot<TWord> where TWord : IT3Word<TWord>
    {
        public readonly Instruction<TWord> Instruction;
        public readonly bool IsNoOp;

        public VliwSlot(Instruction<TWord> instruction)
        {
            Instruction = instruction;
            IsNoOp = false; // HALT is not a NOP, it's a control instruction
        }

        public VliwSlot(bool isNoOp)
        {
            IsNoOp = isNoOp;
            Instruction = default;
        }
    }
}