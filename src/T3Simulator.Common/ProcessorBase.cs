using System;
using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Abstract base class for T3 processors.
    /// </summary>
    public abstract class ProcessorBase<TWord> : IT3Processor<TWord> where TWord : IT3Word<TWord>
    {
        public TWord[] Registers = new TWord[27];
        public TWord PR;
        public T3Float[] FRegisters = new T3Float[9];
        protected Memory<TWord> Memory;
        public DeviceManager<TWord> DeviceManager;

        public T3Config Config { get; }
        public long PC { get; set; }
        public long SP { get; set; }
        public int WP { get; set; }
        public int Cond { get; set; }
        public bool IsHalted { get; set; }

        protected long _cycleCount;
        protected long _instructionCount;
        protected long _stallCount;

        private readonly long _initialSp;

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

            _initialSp = memSize - 1;
            SP = _initialSp;

            // Initialize registers to default (0)
            for (int i = 0; i < Registers.Length; i++)
                Registers[i] = TWord.FromLong(0);

            PR = TWord.FromLong(0);
        }

        public virtual void Reset()
        {
            PC = 0;
            WP = 0;
            SP = _initialSp;
            IsHalted = false;
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;

            // Reset all registers to 0
            for (int i = 0; i < Registers.Length; i++)
                Registers[i] = TWord.FromLong(0);

            PR = TWord.FromLong(0);

            // Reset FRegisters
            for (int i = 0; i < FRegisters.Length; i++)
                FRegisters[i] = T3Float.FromDouble(0);
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

        /// <summary>
        /// Run the program N iterations. After each iteration, resets PC but preserves counters.
        /// Returns true if all iterations completed without error.
        /// </summary>
        public virtual void RunIterations(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                // Reset PC and halt flag for next iteration (preserve counters)
                PC = 0;
                IsHalted = false;
                SP = _initialSp;
                WP = 0;

                // Reset registers
                for (int j = 0; j < Registers.Length; j++)
                    Registers[j] = TWord.FromLong(0);
                PR = TWord.FromLong(0);
                for (int j = 0; j < FRegisters.Length; j++)
                    FRegisters[j] = T3Float.FromDouble(0);

                while (!IsHalted && Step())
                {
                }
            }
        }

        /// <summary>
        /// Reset all counters to zero (cycle, instruction, stall).
        /// </summary>
        public virtual void ResetCounters()
        {
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;
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

        protected TWord FromLong(long value)
        {
            return (TWord)TWord.FromLong(value);
        }

        protected long ToLong(TWord value)
        {
            return (long)value.ToInt128();
        }

        public TWord ReadWord(long address)
        {
            if (address == Memory<TWord>.ADDR_CYCLE_LOW) return FromLong(_cycleCount & 0xFFFFFFFF);
            if (address == Memory<TWord>.ADDR_CYCLE_HIGH) return FromLong(_cycleCount >> 32);
            if (address == Memory<TWord>.ADDR_INST_COUNT) return FromLong(_instructionCount);
            if (address == Memory<TWord>.ADDR_STALL_COUNT) return FromLong(_stallCount);

            return Memory.Read(address);
        }

        protected void WriteWord(long address, TWord value)
        {
            // Writing to CYCLE_LOW resets all counters (per spec)
            if (address == Memory<TWord>.ADDR_CYCLE_LOW)
            {
                _cycleCount = 0;
                _instructionCount = 0;
                _stallCount = 0;
                return;
            }
            if (address >= Memory<TWord>.ADDR_CYCLE_HIGH)
            {
                return;
            }
            Memory.Write(address, value);
        }

        /// <summary>
        /// Read memory value as long for CLI display purposes
        /// </summary>
        public long GetMemoryValue(long address)
        {
            var word = ReadWord(address);
            return ToLong(word);
        }
    }
}