namespace T3Compiler
{
    public static class CompilerDebugConfig
    {
        /// <summary>
        /// If true, the compiler and simulator will dump AST, ASM, Bin, and Trace files 
        /// to the test_results directory for every test case.
        /// </summary>
        public static bool EnableDumps = true;

        /// <summary>
        /// If true, the CodeGenerator will emit [TRACE] comments into the ASM output
        /// showing register allocation, StoreV/LoadV operations, and call details.
        /// Default false to keep ASM clean.
        /// </summary>
        public static bool EnableCodeGenTrace = true;
    }
}