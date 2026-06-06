using System;
using System.Collections.Generic;

namespace T3Simulator.Common
{
    /// <summary>
    /// Word-addressable memory for the T3 processor.
    /// </summary>
    public class Memory<TWord>
    {
        private readonly TWord[] _data;
        private readonly long _size;

        public const long ADDR_CYCLE_LOW = 0xFFFFFF00;
        public const long ADDR_CYCLE_HIGH = 0xFFFFFF01;
        public const long ADDR_INST_COUNT = 0xFFFFFF02;
        public const long ADDR_STALL_COUNT = 0xFFFFFF03;
        public const long ADDR_TIMER_CTRL = 0xFFFFFF10;
        public const long ADDR_TIMER_CMP = 0xFFFFFF11;

        public Memory(long size)
        {
            _size = size;
            _data = new TWord[size];
        }

        public TWord Read(long address)
        {
            if (address < 0 || address >= _size)
                throw new IndexOutOfRangeException($"Memory read out of bounds: address {address}");
            return _data[address];
        }

        public void Write(long address, TWord value)
        {
            if (address < 0 || address >= _size)
                throw new IndexOutOfRangeException($"Memory write out of bounds: address {address}");
            _data[address] = value;
        }

        // Helpers for program loading
        public void LoadProgram(IEnumerable<TWord> code, long startAddress = 0)
        {
            int i = 0;
            foreach (var word in code)
            {
                Write(startAddress + i, word);
                i++;
            }
        }
    }
}