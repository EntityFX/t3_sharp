using T3Simulator.Common;

namespace T3Simulator.VLIW
{
    /// <summary>
    /// Represents a single 18-trit slot within a VLIW bundle.
    /// Format: opcode(6), op1(6), op2(6)
    /// </summary>
    public readonly struct VliwSlot
    {
        public readonly Instruction Instruction;
        public readonly bool IsNoOp;

        public VliwSlot(Instruction instruction)
        {
            Instruction = instruction;
            IsNoOp = instruction.Opcode == Opcode.HALT; // Simplified: treating HALT in a slot as NOP unless it's the only one or specific logic
        }

        public VliwSlot(bool isNoOp)
        {
            IsNoOp = isNoOp;
            Instruction = default;
        }
    }
}