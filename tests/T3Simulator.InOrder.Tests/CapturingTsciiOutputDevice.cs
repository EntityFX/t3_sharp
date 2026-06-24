using System;
using System.Text;
using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.InOrder.Tests
{
    /// <summary>
    /// A T-SCII output device that captures written characters into a buffer for testing purposes.
    /// </summary>
    public class CapturingTsciiOutputDevice<TWord> : TsciiOutputDevice<TWord> where TWord : IT3Word<TWord>
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        public override void Write(TWord value)
        {
            // We don't call base.Write because we don't want to pollute the console during tests
            try
            {
                Int128 numericValue = value.ToInt128();
                char c = TScii.ToChar(numericValue);
                _buffer.Append(c);
            }
            catch (Exception ex)
            {
                _buffer.Append($"[T-SCII Error: {ex.Message}]");
            }
        }

        public string GetCapturedText()
        {
            return _buffer.ToString();
        }

        public void Clear()
        {
            _buffer.Clear();
        }
    }
}