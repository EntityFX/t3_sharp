namespace T3Simulator.Common
{
    /// <summary>
    /// Configuration for the T3 processor variants.
    /// </summary>
    public enum T3Config
    {
        T3_27,
        T3_54
    }

    public static class T3ConfigExtensions
    {
        public static int GetWordSize(this T3Config config) => config switch
        {
            T3Config.T3_27 => 27,
            T3Config.T3_54 => 54,
            _ => throw new System.ArgumentOutOfRangeException(nameof(config))
        };

        public static long GetMemorySize(this T3Config config) => 1048576; // 1M words
    }
}