using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Core interface for T3 Ternary Processor simulators.
    /// </summary>
    public interface IT3Processor
    {
        /// <summary>
        /// Loads a program into processor memory.
        /// </summary>
        void LoadProgram(IEnumerable<System.Numerics.BigInteger> code);

        /// <summary>
        /// Resets the processor to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Executes a single clock cycle/step.
        /// Returns true if the processor is still running, false if HALTED.
        /// </summary>
        bool Step();

        /// <summary>
        /// Runs the processor until it HALTs or an exception occurs.
        /// </summary>
        void Run();

        long CycleCount { get; }
        long InstructionCount { get; }
        long StallCount { get; }

        void SetInputDevice(long port, IDevice dev);
        void SetOutputDevice(long port, IDevice dev);

        ProcessorState GetState();
    }
}