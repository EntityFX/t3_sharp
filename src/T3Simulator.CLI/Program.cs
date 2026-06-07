using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TritTypes;
using T3Simulator.Common;
using T3Simulator.InOrder;

namespace T3Simulator.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("T3 Ternary Processor Simulator CLI");
            Console.WriteLine("----------------------------------");
            
            // For simplicity, we use T3-27 In-Order Processor
            var processor = new T3InOrderProcessor<long>(T3Config.T3_27);
            
            // Attach T-SCII output device to port 0 for text output
            processor.SetOutputDevice(0, new TsciiOutputDevice<long>());
            
            bool running = true;

            while (running)
            {
                Console.Write("\nT3> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                try
                {
                    switch (command)
                    {
                        case "load":
                            if (parts.Length < 2) { Console.WriteLine("Usage: load <file>"); break; }
                            LoadProgram(processor, parts[1]);
                            break;
                        case "run":
                            processor.Run();
                            Console.WriteLine("Execution finished or halted.");
                            break;
                        case "step":
                            if (processor.Step())
                                Console.WriteLine("Step executed.");
                            else
                                Console.WriteLine("Processor halted.");
                            break;
                        case "dump":
                            DumpState(processor);
                            break;
                        case "exit":
                            running = false;
                            break;
                        default:
                            Console.WriteLine($"Unknown command: {command}");
                            Console.WriteLine("Commands: load <file>, run, step, dump, exit");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void LoadProgram(T3InOrderProcessor<long> proc, string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");

            string content = File.ReadAllText(filePath);
            List<int> trits = new List<int>();

            // Detect format by extension or content
            if (filePath.EndsWith(".bin"))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                trits = TritEncoding.FromBinary(bytes, 1000000 * 27); // Large buffer
            }
            else if (content.StartsWith("0n"))
            {
                trits = TritEncoding.FromNinary(content);
            }
            else if (content.StartsWith("0y"))
            {
                trits = TritEncoding.FromTryx(content);
            }
            else
            {
                trits = TritEncoding.FromSimpleText(content);
            }

            // Convert trits to words
            List<long> words = new List<long>();
            for (int i = 0; i + 26 < trits.Count; i += 27)
            {
                int[] wordTrits = trits.GetRange(i, 27).ToArray();
                words.Add(BalancedTernary.ParseFromTritArray(wordTrits));
            }

            proc.LoadProgram(words);
            Console.WriteLine($"Loaded {words.Count} words from {filePath}");
        }

        static void DumpState(T3InOrderProcessor<long> proc)
        {
            var state = proc.GetState();
            Console.WriteLine($"--- Processor State ---");
            Console.WriteLine($"PC: {state.PC} | WP: {state.WP} | Cond: {state.Cond}");
            Console.WriteLine($"Cycles: {state.CycleCount} | Inst: {state.InstructionCount} | Stalls: {state.StallCount}");
            Console.WriteLine("Registers:");
            for (int i = 0; i < 27; i++)
            {
                Console.Write($"R{i:D2}: {state.Registers[i],15} ");
                if ((i + 1) % 6 == 0) Console.WriteLine();
            }
            Console.WriteLine("\n----------------------");
        }
    }
}