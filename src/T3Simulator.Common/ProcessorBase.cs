using System;
using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Abstract base class for T3 processors — ISA v5.
    /// Special registers: FP, HP, SP, CD, PR, WD.
    /// </summary>
    public abstract class ProcessorBase<TWord> : IT3Processor<TWord> where TWord : IT3Word<TWord>
    {
        public TWord[] Registers = new TWord[9];
        public TWord PR;
        public T3Float[] FRegisters = new T3Float[9];
        public Memory<TWord> Memory;
        public DeviceManager<TWord> DeviceManager;

        public T3Config Config { get; }
        public long PC { get; set; }
        public long SP { get; set; }

        // Special registers (accessed via RegGroup=+1)
        public long FP { get; set; }
        public long HP { get; set; }
        public int CD { get; set; }   // Condition flag (-1/0/+1), was Cond
        public int WD { get; set; }   // Window pointer, was WP

        public bool IsHalted { get; set; }

        protected long _cycleCount;
        protected long _instructionCount;
        protected long _stallCount;

        public const int HeapFractionNumerator = 2;
        public const int HeapFractionDenominator = 3;
        
        public readonly long HeapStart;
        private readonly long _initialSp;

        public long CycleCount => _cycleCount;
        public long InstructionCount => _instructionCount;
        public long StallCount => _stallCount;

        protected ProcessorBase(T3Config config)
        {
            Config = config;
            WD = 0;
            PC = 0;
            IsHalted = false;

            long memSize = 1048576;
            HeapStart = memSize * HeapFractionNumerator / HeapFractionDenominator;
            Memory = new Memory<TWord>(memSize);
            DeviceManager = new DeviceManager<TWord>();

            _initialSp = memSize - 1;
            SP = _initialSp;
            FP = _initialSp;
            HP = HeapStart;
            CD = 0;

            for (int i = 0; i < Registers.Length; i++)
                Registers[i] = TWord.FromLong(0);
            PR = TWord.FromLong(0);
        }

        public virtual void Reset()
        {
            PC = 0;
            WD = 0;
            SP = _initialSp;
            FP = _initialSp;
            HP = HeapStart;
            CD = 0;
            IsHalted = false;
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;

            for (int i = 0; i < Registers.Length; i++)
                Registers[i] = TWord.FromLong(0);
            PR = TWord.FromLong(0);
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
            while (!IsHalted && Step()) { }
        }

        public virtual void RunIterations(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                PC = 0;
                IsHalted = false;
                SP = _initialSp;
                FP = _initialSp;
                HP = HeapStart;
                CD = 0;
                WD = 0;

                for (int j = 0; j < Registers.Length; j++)
                    Registers[j] = TWord.FromLong(0);
                PR = TWord.FromLong(0);
                for (int j = 0; j < FRegisters.Length; j++)
                    FRegisters[j] = T3Float.FromDouble(0);

                while (!IsHalted && Step()) { }
            }
        }

        public virtual void ResetCounters()
        {
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;
        }

        public virtual void SetInputDevice(long port, IDevice<TWord> dev)  => DeviceManager.RegisterDevice(port, dev);
        public virtual void SetOutputDevice(long port, IDevice<TWord> dev) => DeviceManager.RegisterDevice(port, dev);

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
                WD,
                CD
            );
        }

        protected void IncrementCycles(long cycles) => _cycleCount += cycles;
        protected void IncrementInstructions() => _instructionCount++;
        protected void IncrementStalls() => _stallCount++;

        protected TWord FromLong(long value) => (TWord)TWord.FromLong(value);
        protected long ToLong(TWord value) => (long)value.ToInt128();

        public TWord ReadWord(long address)
        {
            if (address == Memory.AddrCycleLow) return FromLong(_cycleCount & 0xFFFFFFFF);
            if (address == Memory.AddrCycleHigh) return FromLong(_cycleCount >> 32);
            if (address == Memory.AddrInstCount) return FromLong(_instructionCount);
            if (address == Memory.AddrStallCount) return FromLong(_stallCount);
            if (address == Memory.AddrTimerCtrl) return FromLong(0);
            if (address == Memory.AddrTimerCmp) return FromLong(0);
            return Memory.Read(address);
        }

        protected void WriteWord(long address, TWord value)
        {
            if (address == Memory.AddrCycleLow)
            {
                _cycleCount = 0;
                _instructionCount = 0;
                _stallCount = 0;
                return;
            }
            if (Memory.IsMmioAddress(address)) return;
            Memory.Write(address, value);
        }

        public long GetMemoryValue(long address)
        {
            var word = ReadWord(address);
            return ToLong(word);
        }
    }
}