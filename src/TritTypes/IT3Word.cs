using System;

namespace TritTypes
{
    /// <summary>
    /// Interface for T3 processor machine words (Word18, Word54).
    /// Provides a common set of operations for ternary arithmetic and conversion.
    /// </summary>
    public interface IT3Word<TSelf> : IEquatable<TSelf> where TSelf : IT3Word<TSelf>
    {
        // Basic conversions
        Int128 ToInt128();

        int ToInt();

        long ToLong();
        
        string ToTritString();

        int GetTrit(int index);
        
        // Unary operations
        TSelf Negate();

        // Static factory methods for generic instantiation
        static abstract TSelf FromLong(long value);
        static abstract TSelf FromInt128(Int128 value);
        static abstract TSelf FromTritString(string tritString);
        
        static abstract TSelf Zero { get; }
    }
}