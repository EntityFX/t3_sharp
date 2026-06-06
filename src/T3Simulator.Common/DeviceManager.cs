using System;
using System.Collections.Generic;
using System.Numerics;

namespace T3Simulator.Common
{
    /// <summary>
    /// Manages I/O devices mapped to ports.
    /// </summary>
    public class DeviceManager
    {
        private readonly Dictionary<BigInteger, IDevice> _devices = new Dictionary<BigInteger, IDevice>();

        public void RegisterDevice(BigInteger port, IDevice device)
        {
            _devices[port] = device;
        }

        public BigInteger Read(BigInteger port)
        {
            if (_devices.TryGetValue(port, out var device))
            {
                if (!device.DataReady)
                {
                    throw new DeviceStallException(port);
                }
                return device.Read();
            }
            return 0; // Default value for unmapped ports
        }

        public void Write(BigInteger port, BigInteger value)
        {
            if (_devices.TryGetValue(port, out var device))
            {
                device.Write(value);
            }
            // Writes to unmapped ports are silently ignored
        }

        public bool IsDeviceReady(BigInteger port)
        {
            return _devices.TryGetValue(port, out var device) && device.DataReady;
        }
    }

    /// <summary>
    /// Exception thrown when an I/O operation is attempted on a device that is not ready.
    /// This should be caught by the processor to trigger a stall.
    /// </summary>
    public class DeviceStallException : Exception
    {
        public BigInteger Port { get; }
        public DeviceStallException(BigInteger port) : base($"Device at port {port} is not ready.")
        {
            Port = port;
        }
    }
}