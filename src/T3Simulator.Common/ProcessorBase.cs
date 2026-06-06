using System;
using System.Collections.Generic;

namespace T3Simulator.Common
{
    /// <summary>
    /// Abstract base class for T3 processors.
    /// </summary>
    public abstract class ProcessorBase<TWord> : IT3Processor<TWord>
    {
        protected TWord[] Registers = new TWord[27];
        protected TWord PR;
        protected Memory<TWord> Memory;
        protected DeviceManager<TWord> DeviceManager;
        
        public T3Config Config { get; }
        public long PC { get; set; }
        public long SP { get; set; }
        public int WP { get; set; }
        public int Cond { get; set; }
        public bool IsHalted { get; set; }
        
        protected long _cycleCount;
        protected long _instructionCount;
        protected long _stallCount;

        public long CycleCount => _cycleCount;
        public long InstructionCount => _instructionCount;
        public long StallCount => _stallCount;

        protected ProcessorBase(T3Config config)
        {
            Config = config;
            WP = 0;
            PC = 0;
            IsHalted = false;
            
            long memSize = 1048576; // 1M words
            Memory = new Memory<TWord>(memSize);
            DeviceManager = new DeviceManager<TWord>();
            
            SP = memSize - 1;
        }

        public virtual void Reset()
        {
            PC = 0;
            WP = 0;
            IsHalted = false;
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;
        }

        public virtual void LoadProgram(IEnumerable<TWord> code)
        {
            Memory.LoadProgram(code);
        }

        public abstract bool Step();

        public virtual void Run()
        {
            while (!IsHalted && Step())
            {
            }
        }

        public virtual void SetInputDevice(long port, IDevice<TWord> dev)
        {
            DeviceManager.RegisterDevice(port, dev);
        }

        public virtual void SetOutputDevice(long port, IDevice<TWord> dev)
        {
            DeviceManager.RegisterDevice(port, dev);
        }

        public virtual ProcessorState<TWord> GetState()
        {
            return new ProcessorState<TWord>(
                PR,
                (TWord[])Registers.Clone(),
                _cycleCount,
                _instructionCount,
                _stallCount,
                PC,
                SP,
                WP,
                Cond
            );
        }

        protected void IncrementCycles(long cycles) => _cycleCount += cycles;
        protected void IncrementInstructions() => _instructionCount++;
        protected void IncrementStalls() => _stallCount++;

        protected TWord ReadWord(long address)
        {
            if (address == Memory<TWord>.ADDR_CYCLE_LOW) return (TWord)Convert.ChangeType((long)(_cycleCount & 0xFFFFFFFF), typeof(TWord));
            if (address == Memory<TWord>.ADDR_CYCLE_HIGH) return (TWord)Convert.ChangeType((long)(_cycleCount >> 32), typeof(TWord));
            if (address == Memory<TWord>.ADDR_INST_COUNT) return (TWord)Convert.ChangeType(_instructionCount, typeof(TWord));
            if (address == Memory<TWord>.ADDR_STALL_COUNT) return (TWord)Convert.ChangeType(_stallCount, typeof(TWord));

            return Memory.Read(address);
        }

        protected void WriteWord(long address, TWord value)
        {
            if (address >= Memory<TWord>.ADDR_CYCLE_LOW)
            {
                return;
            }
            Memory.Write(address, value);
        }
    }
}