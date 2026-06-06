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
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: T3Assembler <input.asm> <output_file> [format]");
                Console.WriteLine("Formats: text (default), binary, ninary, tryx");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];
            string format = args.Length > 2 ? args[2].ToLower() : "text";

            try
            {
                string source = File.ReadAllText(inputFile);
                T3AssemblerCore assembler = new T3AssemblerCore(T3Config.T3_27);
                List<long> machineCode = assembler.Assemble(source);

                string outputContent = Export(machineCode, format);
                File.WriteAllText(outputFile, outputContent);
                
                if (format == "binary")
                {
                    // For binary, we actually need to write bytes
                    byte[] binaryData = ConvertToBinaryBytes(machineCode);
                    File.WriteAllBytes(outputFile, binaryData);
                }

                Console.WriteLine($"Successfully assembled {inputFile} to {outputFile} (format: {format})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static string Export(List<long> code, string format)
        {
            // Convert each word to a list of trits
            List<int> allTrits = new List<int>();
            foreach (var word in code)
            {
                allTrits.AddRange(BalancedTernary.ToTritArray(word, 27));
            }

            return format switch
            {
                "text" => TritEncoding.ToSimpleText(allTrits),
                "ninary" => TritEncoding.ToNinary(allTrits),
                "tryx" => TritEncoding.ToTryx(allTrits),
                "binary" => "[Binary data written to file]",
                _ => throw new Exception($"Unsupported format: {format}")
            };
        }

        static byte[] ConvertToBinaryBytes(List<long> code)
        {
            List<int> allTrits = new List<int>();
            foreach (var word in code)
            {
                allTrits.AddRange(BalancedTernary.ToTritArray(word, 27));
            }
            return TritEncoding.ToBinary(allTrits);
        }
    }
}