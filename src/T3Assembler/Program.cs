using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TritTypes;
using T3Simulator.Common;

namespace T3Assembler
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            try
            {
                // Linker mode: t3ld -o output.bin input1.o input2.o
                if (args[0] == "--link" || args[0] == "-link")
                {
                    return RunLinker(args.Skip(1).ToArray());
                }

                // Assemble to object file: --emit-obj input.asm -o output.o
                if (args.Contains("--emit-obj"))
                {
                    return RunAssembleToObj(args);
                }

                // Default: assemble to text/binary
                return RunAssemble(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int RunAssemble(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: T3Assembler <input.asm> <output_file> [format]");
                Console.WriteLine("Formats: text (default), binary, ninary, tryx");
                return 1;
            }

            string inputFile = args[0];
            string outputFile = args[1];
            string format = args.Length > 2 ? args[2].ToLower() : "text";

            string source = File.ReadAllText(inputFile);
            T3AssemblerBase assembler = new T3InOrderAssembler(T3Config.T3_18);
            List<Int128> machineCode = assembler.Assemble(source);

            string outputContent = Export(machineCode, format, 18);
            File.WriteAllText(outputFile, outputContent);

            if (format == "binary")
            {
                byte[] binaryData = ConvertToBinaryBytes(machineCode, 18);
                File.WriteAllBytes(outputFile, binaryData);
            }

            Console.WriteLine($"Successfully assembled {inputFile} to {outputFile} (format: {format})");
            return 0;
        }

        static int RunAssembleToObj(string[] args)
        {
            string? inputFile = null;
            string? outputFile = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--emit-obj") continue;
                if (args[i] == "-o" && i + 1 < args.Length) { outputFile = args[++i]; continue; }
                if (!args[i].StartsWith("-")) inputFile = args[i];
            }

            if (inputFile == null)
            {
                Console.Error.WriteLine("Error: no input file specified");
                return 1;
            }

            if (outputFile == null)
                outputFile = Path.ChangeExtension(inputFile, ".o");

            string source = File.ReadAllText(inputFile);
            var assembler = new T3InOrderAssembler(T3Config.T3_18);
            var obj = assembler.AssembleToObject(source, Path.GetDirectoryName(Path.GetFullPath(inputFile))!);
            obj.Write(outputFile);

            Console.WriteLine($"Assembled {inputFile} → {outputFile} ({obj.TextSection.Count} text words, {obj.DataSection.Count} data words, {obj.Symbols.Count} symbols, {obj.Relocations.Count} relocations)");
            return 0;
        }

        static int RunLinker(string[] args)
        {
            string? outputFile = null;
            var inputFiles = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length) { outputFile = args[++i]; continue; }
                if (!args[i].StartsWith("-")) inputFiles.Add(args[i]);
            }

            if (inputFiles.Count == 0)
            {
                Console.Error.WriteLine("Error: no input files specified");
                return 1;
            }

            if (outputFile == null)
                outputFile = "a.bin";

            var objects = new List<T3ObjectFile>();
            foreach (var f in inputFiles)
            {
                objects.Add(T3ObjectFile.Read(f));
            }

            var linker = new T3Linker();
            var image = linker.Link(objects);

            // Write binary as text (trit strings)
            using (var sw = new StreamWriter(outputFile))
            {
                foreach (var word in image)
                    sw.WriteLine(BalancedTernary.ToTernaryString((long)word));
            }

            Console.WriteLine($"Linked {inputFiles.Count} files → {outputFile} ({image.Count} words)");
            return 0;
        }

        static string Export(List<Int128> code, string format, int wordSize)
        {
            List<int> allTrits = new List<int>();
            foreach (var word in code)
                allTrits.AddRange(BalancedTernary.ToTritArray((long)word, wordSize));

            return format switch
            {
                "text" => TritEncoding.ToSimpleText(allTrits),
                "ninary" => TritEncoding.ToNinary(allTrits),
                "tryx" => TritEncoding.ToTryx(allTrits),
                "binary" => "[Binary data written to file]",
                _ => throw new Exception($"Unsupported format: {format}")
            };
        }

        static byte[] ConvertToBinaryBytes(List<Int128> code, int wordSize)
        {
            List<int> allTrits = new List<int>();
            foreach (var word in code)
                allTrits.AddRange(BalancedTernary.ToTritArray((long)word, wordSize));
            return TritEncoding.ToBinary(allTrits);
        }

        static void PrintUsage()
        {
            Console.WriteLine("T3Assembler — T3 Ternary Assembler & Linker");
            Console.WriteLine();
            Console.WriteLine("Assemble to text/binary:");
            Console.WriteLine("  T3Assembler <input.asm> <output> [format]");
            Console.WriteLine();
            Console.WriteLine("Assemble to object file:");
            Console.WriteLine("  T3Assembler --emit-obj <input.asm> -o <output.o>");
            Console.WriteLine();
            Console.WriteLine("Link object files:");
            Console.WriteLine("  T3Assembler --link -o <output.bin> <input1.o> [input2.o ...]");
        }
    }
}