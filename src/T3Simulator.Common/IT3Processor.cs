using System.Collections.Generic;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// Core interface for T3 Ternary Processor simulators.
    /// </summary>
    public interface IT3Processor<TWord> where TWord : IT3Word<TWord>
    {
        /// <summary>
        /// Loads a program into processor memory.
        /// </summary>
        void LoadProgram(IEnumerable<TWord> code);

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

        void SetInputDevice(long port, IDevice<TWord> dev);
        void SetOutputDevice(long port, IDevice<TWord> dev);

        ProcessorState<TWord> GetState();

        /// <summary>
        /// Reads a word from the processor's memory.
        /// </summary>
        TWord ReadWord(long address);
    }
}
