using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// A snapshot of the T3 processor state.
    /// </summary>
    public class ProcessorState
    {
        public long PC { get; set; }
        public long WP { get; set; }
        public long SP { get; set; }
        public int Cond { get; set; }
        public System.Numerics.BigInteger PR { get; set; } // Predicate register as a word
        public System.Numerics.BigInteger[] Registers { get; set; } = new System.Numerics.BigInteger[27];
        public long CycleCount { get; set; }
        public long InstructionCount { get; set; }
        public long StallCount { get; set; }

        public ProcessorState Clone()
        {
            return new ProcessorState
            {
                PC = this.PC,
                WP = this.WP,
                SP = this.SP,
                Cond = this.Cond,
                PR = this.PR,
                Registers = (System.Numerics.BigInteger[])this.Registers.Clone(),
                CycleCount = this.CycleCount,
                InstructionCount = this.InstructionCount,
                StallCount = this.StallCount
            };
        }
    }
}