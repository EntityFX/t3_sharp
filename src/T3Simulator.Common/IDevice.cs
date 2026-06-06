namespace T3Simulator.Common
{
    /// <summary>
    /// Interface for T3 processor I/O devices.
    /// </summary>
    public interface IDevice<TWord>
    {
        /// <summary>
        /// Read a value from the device.
        /// </summary>
        TWord Read();

        /// <summary>
        /// Write a value to the device.
        /// </summary>
        void Write(TWord value);

        /// <summary>
        /// Check if the device has data ready to be read.
        /// If false, the processor should stall.
        /// </summary>
        bool DataReady { get; }
    }
}