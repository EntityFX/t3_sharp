using System;

namespace T3Simulator.Common
{
    /// <summary>
    /// Snapshot of the processor state.
    /// </summary>
    public class ProcessorState<TWord>
    {
        public TWord PR { get; set; } // Predicate register as a word
        public TWord[] Registers { get; set; }
        public long CycleCount { get; set; }
        public long InstructionCount { get; set; }
        public long StallCount { get; set; }
        public long PC { get; set; }
        public long SP { get; set; }
        public int WP { get; set; }
        public int Cond { get; set; }

        public ProcessorState(TWord pr, TWord[] registers, long cycleCount, long instructionCount, long stallCount, long pc, long sp, int wp, int cond)
        {
            PR = pr;
            Registers = registers;
            CycleCount = cycleCount;
            InstructionCount = instructionCount;
            StallCount = stallCount;
            PC = pc;
            SP = sp;
            WP = wp;
            Cond = cond;
        }

        public ProcessorState<TWord> Clone()
        {
            return new ProcessorState<TWord>(
                PR,
                (TWord[])Registers.Clone(),
                CycleCount,
                InstructionCount,
                StallCount,
                PC,
                SP,
                WP,
                Cond
            );
        }
    }
}