using System;
using System.Collections.Generic;

namespace T3Simulator.Common
{
    /// <summary>
    /// Word-addressable memory for the T3 processor.
    /// MMIO region is placed immediately after physical memory (addresses [size, size+MMIO_REGION_SIZE-1]).
    /// This keeps physical memory and MMIO disjoint while computing addresses dynamically.
    /// </summary>
    public class Memory<TWord>
    {
        private readonly TWord[] _data;
        private readonly long _size;

        /// <summary>Number of words reserved for MMIO (beyond physical memory).</summary>
        public const long MMIO_REGION_SIZE = 0x100; // 256 words

        /// <summary>Base address of the MMIO region (= physical size, just past physical memory).</summary>
        public long MmioBase { get; }

        /// <summary>End (exclusive) of the MMIO region.</summary>
        public long MmioEnd => MmioBase + MMIO_REGION_SIZE;

        // MMIO offsets within the region
        public long AddrCycleLow   => MmioBase + 0x00;
        public long AddrCycleHigh  => MmioBase + 0x01;
        public long AddrInstCount  => MmioBase + 0x02;
        public long AddrStallCount => MmioBase + 0x03;
        public long AddrTimerCtrl  => MmioBase + 0x10;
        public long AddrTimerCmp   => MmioBase + 0x11;

        /// <summary>Returns true if the address falls within the MMIO region (beyond physical memory).</summary>
        public bool IsMmioAddress(long address) => address >= MmioBase && address < MmioEnd;

        public Memory(long size)
        {
            _size = size;
            _data = new TWord[size];
            MmioBase = size; // MMIO starts right after physical memory
        }

        public TWord Read(long address)
        {
            if (address < 0)
                throw new IndexOutOfRangeException($"Memory read out of bounds: address {address}");
            if (address >= _size)
            {
                if (IsMmioAddress(address))
                    return default(TWord); // MMIO access — handled by ProcessorBase.ReadWord
                throw new IndexOutOfRangeException(
                    $"Memory read out of bounds: address 0x{address:X} (memory size 0x{_size:X})");
            }
            return _data[address];
        }

        public void Write(long address, TWord value)
        {
            if (address < 0)
                throw new IndexOutOfRangeException($"Memory write out of bounds: address {address}");
            if (address >= _size)
            {
                if (IsMmioAddress(address))
                    return; // MMIO writes handled by ProcessorBase.WriteWord
                throw new IndexOutOfRangeException(
                    $"Memory write out of bounds: address 0x{address:X} (memory size 0x{_size:X})");
            }
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