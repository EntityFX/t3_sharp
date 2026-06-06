using System;
using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Abstract base class for T3 processors.
    /// Provides shared infrastructure: register file, memory, device management, and timing.
    /// </summary>
    public abstract class ProcessorBase : IT3Processor
    {
        protected readonly T3Config Config;
        protected readonly Memory Memory;
        protected readonly DeviceManager DeviceManager;
        
        // Processor State
        protected long PC;
        protected long WP;
        protected long SP;
        protected int Cond;
        protected System.Numerics.BigInteger PR;
        protected System.Numerics.BigInteger[] Registers = new System.Numerics.BigInteger[27];
        
        protected long _cycleCount;
        protected long _instructionCount;
        protected long _stallCount;

        protected bool IsHalted;

        protected ProcessorBase(T3Config config)
        {
            Config = config;
            Memory = new Memory(T3ConfigExtensions.GetMemorySize(config));
            DeviceManager = new DeviceManager();
            Reset();
        }

        public virtual void Reset()
        {
            PC = 0;
            WP = 0;
            SP = Memory.Size - 1;
            Cond = 0;
            PR = 0;
            Array.Clear(Registers, 0, Registers.Length);
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;
            IsHalted = false;
        }

        public abstract bool Step();

        public virtual void Run()
        {
            while (!IsHalted && Step())
            {
                // Continue execution
            }
        }

        public virtual void LoadProgram(IEnumerable<System.Numerics.BigInteger> code)
        {
            Memory.LoadProgram(code);
        }

        public long CycleCount => this._cycleCount;
        public long InstructionCount => this._instructionCount;
        public long StallCount => this._stallCount;

        public void SetInputDevice(long port, IDevice dev) => DeviceManager.RegisterDevice(port, dev);
        public void SetOutputDevice(long port, IDevice dev) => DeviceManager.RegisterDevice(port, dev);

        public virtual ProcessorState GetState()
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

        /// <summary>
        /// Helper to read from memory or MMIO.
        /// </summary>
        protected System.Numerics.BigInteger ReadWord(long address)
        {
            if (address == Memory.ADDR_CYCLE_LOW) return (System.Numerics.BigInteger)(_cycleCount & 0xFFFFFFFF); // Simplified for example
            if (address == Memory.ADDR_CYCLE_HIGH) return (System.Numerics.BigInteger)(_cycleCount >> 32);
            if (address == Memory.ADDR_INST_COUNT) return (System.Numerics.BigInteger)_instructionCount;
            if (address == Memory.ADDR_STALL_COUNT) return (System.Numerics.BigInteger)_stallCount;
            
            return Memory.Read(address);
        }

        /// <summary>
        /// Helper to write to memory or MMIO.
        /// </summary>
        protected void WriteWord(long address, System.Numerics.BigInteger value)
        {
            if (address == Memory.ADDR_CYCLE_LOW)
            {
                // Writing to CYCLE_LOW resets all counters as per spec
                ResetCounters();
                return;
            }
            // Other MMIO writes (TIMER_CMP etc) can be implemented here
            Memory.Write(address, value);
        }

        protected void ResetCounters()
        {
            _cycleCount = 0;
            _instructionCount = 0;
            _stallCount = 0;
        }

        protected void IncrementCycles(int count) => _cycleCount += count;
        protected void IncrementInstructions() => _instructionCount++;
        protected void IncrementStalls() => _stallCount++;
    }
}