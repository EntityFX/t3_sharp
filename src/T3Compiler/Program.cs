using System;
using System.Collections.Generic;
using System.IO;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;
using T3Compiler.CodeGen;

namespace T3Compiler
{
    /// <summary>
    /// T3-lang Compiler CLI — compiles T source to T3 assembly.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            string? inputFile = null;
            string? outputFile = null;
            var includePaths = new List<string>();
            var defines = new Dictionary<string, string>();
            bool assemble = false;

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-I" && i + 1 < args.Length)
                    includePaths.Add(args[++i]);
                else if (arg == "-D" && i + 1 < args.Length)
                {
                    string def = args[++i];
                    int eq = def.IndexOf('=');
                    if (eq >= 0)
                        defines[def.Substring(0, eq)] = def.Substring(eq + 1);
                    else
                        defines[def] = "1";
                }
                else if (arg == "-o" && i + 1 < args.Length)
                    outputFile = args[++i];
                else if (arg == "--assemble")
                    assemble = true;
                else if (!arg.StartsWith("-"))
                    inputFile = arg;
            }

            if (inputFile == null)
            {
                Console.Error.WriteLine("Error: no input file specified");
                PrintUsage();
                return 1;
            }

            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine($"Error: file not found: {inputFile}");
                return 1;
            }

            if (outputFile == null)
                outputFile = Path.ChangeExtension(inputFile, ".asm");

            try
            {
                // Read source
                string source = File.ReadAllText(inputFile);

                // 1. Preprocess
                var pp = new T3Preprocessor(includePaths);
                foreach (var d in defines)
                    pp.GetType().GetField("_macros", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                      ?.SetValue(pp, new Dictionary<string, string>(defines));
                // Workaround: add defines via Process directive injection
                string defineSource = "";
                foreach (var d in defines)
                    defineSource += $"#define {d.Key} {d.Value}\n";
                source = defineSource + source;

                string preprocessed = pp.Process(source, inputFile);

                // 2. Tokenize
                var tokenizer = new Tokenizer(preprocessed);
                var tokens = tokenizer.Tokenize();

                // 3. Parse
                var parser = new Parser.Parser(tokens);
                var program = parser.ParseProgram();

                // 4. Generate code
                var gen = new CodeGenerator(program);
                string asmCode = gen.Generate();

                // 5. Output
                File.WriteAllText(outputFile, asmCode);
                Console.WriteLine($"Compiled {inputFile} → {outputFile}");

                // 6. Assemble (optional)
                if (assemble)
                {
                    var assembler = new T3Assembler.T3InOrderAssembler(T3Simulator.Common.T3Config.T3_18);
                    var binary = assembler.Assemble(asmCode);
                    string binFile = Path.ChangeExtension(outputFile, ".bin");
                    // Write binary as text (trit strings)
                    using (var sw = new StreamWriter(binFile))
                    {
                        foreach (var word in binary)
                        {
                            sw.WriteLine(TritTypes.BalancedTernary.ToTernaryString((long)word));
                        }
                    }
                    Console.WriteLine($"Assembled → {binFile}");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Compilation error: {ex.Message}");
                return 1;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("t3cc — T-lang Compiler for T3 Ternary Processor");
            Console.WriteLine();
            Console.WriteLine("Usage: t3cc [options] <source.t>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -I <path>     Add include path");
            Console.WriteLine("  -D <name>[=value]  Define preprocessor macro");
            Console.WriteLine("  -o <file>     Output assembly file (default: source.asm)");
            Console.WriteLine("  --assemble    Also assemble to binary (.bin)");
        }
    }
}