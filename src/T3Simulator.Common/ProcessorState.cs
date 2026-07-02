using System;

namespace T3Simulator.Common
{
    public class ProcessorState<TWord>
    {
        public TWord PR { get; set; }
        public TWord[] Registers { get; set; }
        public long CycleCount { get; set; }
        public long InstructionCount { get; set; }
        public long StallCount { get; set; }
        public long PC { get; set; }
        public long SP { get; set; }
        public int WD { get; set; }
        public int CD { get; set; }

        public ProcessorState(TWord pr, TWord[] registers, long cycleCount, long instructionCount,
            long stallCount, long pc, long sp, int wd, int cd)
        {
            PR = pr;
            Registers = registers;
            CycleCount = cycleCount;
            InstructionCount = instructionCount;
            StallCount = stallCount;
            PC = pc;
            SP = sp;
            WD = wd;
            CD = cd;
        }

        public ProcessorState<TWord> Clone()
        {
            return new ProcessorState<TWord>(
                PR, (TWord[])Registers.Clone(),
                CycleCount, InstructionCount, StallCount,
                PC, SP, WD, CD);
        }
    }
}