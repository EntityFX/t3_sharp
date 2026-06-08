using System;
using System.Collections.Generic;

namespace T3Simulator.Common
{
    /// <summary>
    /// Manages I/O devices for the T3 processor.
    /// </summary>
    public class DeviceManager<TWord>
    {
        private readonly Dictionary<long, IDevice<TWord>> _devices = new Dictionary<long, IDevice<TWord>>();

        public void RegisterDevice(long port, IDevice<TWord> device)
        {
            _devices[port] = device;
        }

        public TWord Read(long port)
        {
            if (_devices.TryGetValue(port, out var device))
            {
                if (device.DataReady)
                {
                    return device.Read();
                }
                throw new DeviceStallException(port);
            }
            return default!;
        }

        public void Write(long port, TWord value)
        {
            if (_devices.TryGetValue(port, out var device))
            {
                device.Write(value);
            }
        }

        public bool IsDeviceReady(long port)
        {
            return _devices.TryGetValue(port, out var device) && device.DataReady;
        }
    }
}
