using System;
using TritTypes;

namespace T3Simulator.Common
{
    /// <summary>
    /// I/O device that interprets written values as T-SCII characters and prints them to the console.
    /// </summary>
    public class TsciiOutputDevice<TWord> : IDevice<TWord> where TWord : IT3Word<TWord>
    {
        public bool DataReady => true;

        public TWord Read()
        {
            throw new NotSupportedException("TsciiOutputDevice is an output-only device.");
        }

            public virtual void Write(TWord value)
        {
            try
            {
                // Convert word to numeric value
                Int128 numericValue = value.ToInt128();
                
                // Convert numeric value to T-SCII character
                char c = TScii.ToChar(numericValue);
                
                // Output to console
                Console.Write(c);
            }
            catch (Exception ex)
            {
                Console.Write($"[T-SCII Error: {ex.Message}]");
            }
        }
    }
}