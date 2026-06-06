using System.Numerics;

namespace T3Simulator.Common
{
    /// <summary>
    /// Interface for T3 processor I/O devices.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Read a value from the device.
        /// </summary>
        BigInteger Read();

        /// <summary>
        /// Write a value to the device.
        /// </summary>
        void Write(BigInteger value);

        /// <summary>
        /// Check if the device has data ready to be read.
        /// If false, the processor should stall.
        /// </summary>
        bool DataReady { get; }
    }
}