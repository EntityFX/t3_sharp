using System;

namespace T3Simulator.Common
{
    /// <summary>
    /// Exception thrown when an I/O device is not ready for a read/write operation.
    /// </summary>
    public class DeviceStallException : Exception
    {
        public long Port { get; }

        public DeviceStallException(long port) 
            : base($"Device at port {port} is not ready.")
        {
            Port = port;
        }
    }
}