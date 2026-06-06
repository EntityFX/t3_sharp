using System;

namespace T3Simulator.Common
{
    /// <summary>
    /// Manages the mapping between logical registers (A-I) and physical registers (R0-R26).
    /// </summary>
    public static class RegisterWindow
    {
        public const int PhysicalRegisterCount = 27;
        public const int LogicalRegisterCount = 9;

        /// <summary>
        /// Maps a logical register index (0=A, 8=I) to a physical register index based on the Window Pointer (WP).
        /// </summary>
        public static int GetPhysicalIndex(int logicalIndex, long wp)
        {
            if (logicalIndex < 0 || logicalIndex >= LogicalRegisterCount)
                throw new ArgumentOutOfRangeException(nameof(logicalIndex), $"Logical register index must be between 0 and {LogicalRegisterCount - 1}");

            return (int)((wp + logicalIndex) % PhysicalRegisterCount);
        }

        /// <summary>
        /// Calculates the new Window Pointer after a CALL.
        /// </summary>
        public static long CalculateNextWp(long currentWp)
        {
            // Old A (logical 0) should become New E (logical 4).
            // WP_old + 0 = WP_new + 4 (mod 27)
            // WP_new = WP_old - 4 = WP_old + 23 (mod 27)
            return (currentWp + 23) % PhysicalRegisterCount;
        }
    }
}