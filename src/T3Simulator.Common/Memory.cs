using System;
using System.Collections.Generic;

namespace T3Simulator.Common
{
    /// <summary>
    /// Word-addressable memory for the T3 processor.
    /// Supports MMIO for cycle counters.
    /// </summary>
    public class Memory
    {
        private readonly System.Numerics.BigInteger[] _data;
        private readonly long _size;

        // MMIO Addresses
        public const long ADDR_CYCLE_LOW = 0xFFFFFF00;
        public const long ADDR_CYCLE_HIGH = 0xFFFFFF01;
        public const long ADDR_INST_COUNT = 0xFFFFFF02;
        public const long ADDR_STALL_COUNT = 0xFFFFFF03;
        public const long ADDR_TIMER_CTRL = 0xFFFFFF10;
        public const long ADDR_TIMER_CMP = 0xFFFFFF11;

        public Memory(long size)
        {
            _size = size;
            _data = new System.Numerics.BigInteger[size];
        }

        public System.Numerics.BigInteger Read(long address)
        {
            if (address < 0 || address >= _size)
            {
                throw new IndexOutOfRangeException($"Memory read out of bounds: {address}");
            }
            return _data[address];
        }

        public void Write(long address, System.Numerics.BigInteger value)
        {
            if (address < 0 || address >= _size)
            {
                throw new IndexOutOfRangeException($"Memory write out of bounds: {address}");
            }
            _data[address] = value;
        }

        public long Size => _size;

        // Helpers for program loading
        public void LoadProgram(IEnumerable<System.Numerics.BigInteger> code, long startAddress = 0)
        {
            long currentAddr = startAddress;
            foreach (var word in code)
            {
                if (currentAddr >= _size) break;
                _data[currentAddr++] = word;
            }
        }
    }
}