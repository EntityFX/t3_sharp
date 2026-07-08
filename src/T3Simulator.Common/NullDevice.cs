namespace T3Simulator.Common
{
    /// <summary>
    /// Silent /dev/null device — accepts writes silently, reads return 0.
    /// </summary>
    public class NullDevice<TWord> : IDevice<TWord>
    {
        public TWord Read() => default!;
        public void Write(TWord value) { }
        public bool DataReady => true;
    }
}