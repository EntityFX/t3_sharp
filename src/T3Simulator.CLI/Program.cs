/// <summary>
/// T3 Ternary Processor Simulator CLI - Enhanced Version
/// Полная поддержка троичной арифметики с множественными форматами вывода
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TritTypes;
using T3Simulator.Common;
using T3Simulator.InOrder;

namespace T3Simulator.CLI
{
    /// <summary>
    /// Улучшенный CLI симулятор троичной машины
    /// </summary>
    class Program
    {
        // Константы для форматов вывода
        private const string FORMAT_TRINARY = "trinary";     // Троичный: -, 0, +
        private const string FORMAT_NONARY = "nonary";       // 9-ричный: W,X,Y,Z,0,1,2,3,4
        private const string FORMAT_27ARY = "27ary";         // 27-ричный: NOPQRSTUVWXYZ0123456789ABCD

        private T3InOrderProcessor<Word18> _processor;
        private List<Word18> _programWords = new List<Word18>();
        private HashSet<long> _breakpoints = new HashSet<long>();
        private bool _traceEnabled = false;
        private Queue<string> _traceBuffer = new Queue<string>();
        private const int MAX_TRACE_SIZE = 1000;

        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        void Run(string[] args)
        {
            Console.WriteLine("T3 Ternary Processor Simulator CLI (Enhanced)");
            Console.WriteLine("==============================================");
            Console.WriteLine("Type 'help' for commands list");
            Console.WriteLine();

            // Для простоты используем T3-18 In-Order Processor
            _processor = new T3InOrderProcessor<Word18>(T3Config.T3_18);

            // Attach T-SCII output device to port 0 for text output
            _processor.SetOutputDevice(0, new TsciiOutputDevice<Word18>());

            // Обработка аргументов командной строки
            if (args.Length > 0)
            {
                HandleCommandLine(args);
            }

            // Интерактивный цикл
            bool running = true;
            while (running)
            {
                Console.Write("T3> ");
                string input = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(input)) continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                string command = parts[0].ToLower();
                string[] arguments = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

                try
                {
                    switch (command)
                    {
                        case "help":
                        case "?":
                            ShowHelp();
                            break;

                        case "load":
                            LoadProgram(arguments);
                            break;

                        case "run":
                            RunProgram(arguments);
                            break;

                        case "step":
                            StepProgram(arguments);
                            break;

                        case "dump":
                            DumpState(arguments);
                            break;

                        case "trace":
                            TraceControl(arguments);
                            break;

                        case "breakpoint":
                        case "bp":
                            BreakpointControl(arguments);
                            break;

                        case "disassemble":
                        case "disasm":
                            DisassembleProgram(arguments);
                            break;

                        case "stack":
                            ShowStack(arguments);
                            break;

                        case "memory":
                            ShowMemory(arguments);
                            break;

                        case "exit":
                        case "quit":
                        case "q":
                            running = false;
                            break;

                        default:
                            Console.WriteLine($"Unknown command: {command}");
                            Console.WriteLine("Type 'help' for available commands");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    if (Debugger.IsAttached) Console.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
        }

        void HandleCommandLine(string[] args)
        {
            if (args.Length >= 2 && args[0].Equals("load", StringComparison.OrdinalIgnoreCase))
            {
                string filePath = args[1];
                string format = args.Length > 2 ? args[2].ToLower() : "text";

                try
                {
                    LoadProgramInternal(filePath, format);
                    Console.WriteLine($"Program loaded from {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load program: {ex.Message}");
                }
            }
        }

        void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine();
            Console.WriteLine("  load <file> [format]     - Load program from file");
            Console.WriteLine("                           Formats: text, ninary, tryx, binary");
            Console.WriteLine();
            Console.WriteLine("  run [max_steps]          - Run until halt or max steps");
            Console.WriteLine("  step [n]                 - Execute n steps (default: 1)");
            Console.WriteLine();
            Console.WriteLine("  dump [registers|memory|all]");
            Console.WriteLine("                           - Dump processor state");
            Console.WriteLine("    Options:");
            Console.WriteLine("      registers [format]   - Show registers (format: trinary, nonary, 27ary)");
            Console.WriteLine("      memory [start] [count]");
            Console.WriteLine("                           - Show memory (default: start=0, count=16)");
            Console.WriteLine("      all                  - Show everything");
            Console.WriteLine();
            Console.WriteLine("  trace [on|off|view]      - Control trace buffer");
            Console.WriteLine("                           view - show last 20 trace entries");
            Console.WriteLine();
            Console.WriteLine("  breakpoint [addr]        - Add breakpoint");
            Console.WriteLine("  bp list                  - List breakpoints");
            Console.WriteLine("  bp clear                 - Clear all breakpoints");
            Console.WriteLine();
            Console.WriteLine("  disassemble [start] [count]");
            Console.WriteLine("                           - Disassemble program");
            Console.WriteLine("                           (default: start=0, count=16)");
            Console.WriteLine();
            Console.WriteLine("  stack [count]            - Show stack (default: count=16)");
            Console.WriteLine();
            Console.WriteLine("  memory <start> <count>   - Show memory range");
            Console.WriteLine();
            Console.WriteLine("  help, ?                  - Show this help");
            Console.WriteLine("  exit, quit, q            - Exit simulator");
        }

        void LoadProgram(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: load <file> [format]");
                Console.WriteLine("Formats: text, ninary, tryx, binary");
                return;
            }

            string filePath = args[0];
            string format = args.Length > 1 ? args[1].ToLower() : "text";

            try
            {
                LoadProgramInternal(filePath, format);
                Console.WriteLine($"Program loaded from {filePath} (format: {format})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading program: {ex.Message}");
            }
        }

        void LoadProgramInternal(string filePath, string format)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File not found: {filePath}");

            string content = File.ReadAllText(filePath);
            List<int> trits = new List<int>();

            // Detect format by extension or content
            if (filePath.EndsWith(".bin") || format == "binary")
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                trits = TritEncoding.FromBinary(bytes, 1000000 * 27);
            }
            else if (content.StartsWith("0n") || format == "ninary")
            {
                trits = TritEncoding.FromNinary(content);
            }
            else if (content.StartsWith("0y") || format == "tryx")
            {
                trits = TritEncoding.FromTryx(content);
            }
            else
            {
                trits = TritEncoding.FromSimpleText(content);
            }

            // Convert trits to words
            _programWords.Clear();
            for (int i = 0; i + 26 < trits.Count; i += 27)
            {
                int[] wordTrits = trits.GetRange(i, 27).ToArray();
                _programWords.Add(BalancedTernary.ParseFromTritArray(wordTrits));
            }

            _processor.LoadProgram(_programWords);
        }

        void RunProgram(string[] args)
        {
            long maxSteps = long.MaxValue;
            if (args.Length > 0 && long.TryParse(args[0], out long parsed))
            {
                maxSteps = parsed;
            }

            Console.WriteLine("Running program...");
            Console.WriteLine($"Breakpoints: {string.Join(", ", _breakpoints.Select(b => $"0x{b:X8}"))}");

            long stepCount = 0;
            while (stepCount < maxSteps)
            {
                // Check breakpoint
                long pc = _processor.GetState().PC;
                if (_breakpoints.Contains(pc))
                {
                    Console.WriteLine($"*** Breakpoint at 0x{pc:X8} ***");
                    break;
                }

                if (!_processor.Step())
                {
                    Console.WriteLine("Execution finished or halted.");
                    break;
                }

                stepCount++;

                // Trace
                if (_traceEnabled)
                {
                    AddTraceEntry($"Step {stepCount}: PC=0x{pc:X8}");
                }
            }

            Console.WriteLine($"Executed {stepCount} steps");
            DumpStateInternal();
        }

        void StepProgram(string[] args)
        {
            int steps = 1;
            if (args.Length > 0 && int.TryParse(args[0], out int parsed))
            {
                steps = parsed;
            }

            for (int i = 0; i < steps; i++)
            {
                long pc = _processor.GetState().PC;

                // Check breakpoint
                if (_breakpoints.Contains(pc))
                {
                    Console.WriteLine($"*** Breakpoint at 0x{pc:X8} ***");
                    DumpStateInternal();
                    return;
                }

                if (!_processor.Step())
                {
                    Console.WriteLine($"Step {i + 1}/{steps}: Processor halted.");
                    DumpStateInternal();
                    return;
                }

                // Trace
                if (_traceEnabled)
                {
                    AddTraceEntry($"Step {i + 1}: PC=0x{pc:X8}");
                }

                Console.WriteLine($"Step {i + 1}/{steps} executed (PC=0x{pc:X8})");
            }

            if (_traceEnabled && steps > 1)
            {
                Console.WriteLine($"Trace: {Math.Min(steps, _traceBuffer.Count)} entries recorded");
            }
        }

        void DumpState(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("all"))
            {
                DumpStateInternal();
            }
            else if (args[0].Equals("registers"))
            {
                string format = args.Length > 1 ? args[1].ToLower() : FORMAT_TRINARY;
                DumpRegisters(format);
            }
            else if (args[0].Equals("memory"))
            {
                long start = args.Length > 1 ? ParseAddress(args[1]) : 0;
                int count = args.Length > 2 ? int.Parse(args[2]) : 16;
                DumpMemory(start, count);
            }
            else
            {
                Console.WriteLine("Unknown dump option. Use: dump [registers|memory|all]");
            }
        }

        void DumpStateInternal()
        {
            var state = _processor.GetState();

            Console.WriteLine();
            Console.WriteLine("--- Processor State ---");
            Console.WriteLine($"PC: 0x{state.PC:X8} | SP: 0x{state.SP:X8} | WP: {state.WP}");
            Console.WriteLine($"Cond: {state.Cond} | Cycles: {state.CycleCount}");
            Console.WriteLine($"Instructions: {state.InstructionCount} | Stalls: {state.StallCount}");
            Console.WriteLine();

            DumpRegisters(FORMAT_TRINARY);

            Console.WriteLine();
            Console.WriteLine($"PR (Predicate): {state.PR}");
            Console.WriteLine("----------------------");
            Console.WriteLine();
        }

        void DumpRegisters(string format)
        {
            var state = _processor.GetState();

            Console.WriteLine("Registers:");
            for (int i = 0; i < state.Registers.Length; i++)
            {
                string regName = GetRegName(i);
                string valueStr = FormatValue(state.Registers[i], format);

                // Highlight active window registers
                string prefix = (i >= state.WP && i < state.WP + 9) ? "*" : " ";
                Console.WriteLine($"  {prefix}{regName} (R{i:D2}): {valueStr}");
            }
        }

        void DumpMemory(long start, int count)
        {
            Console.WriteLine($"Memory [0x{start:X8} .. 0x{start + count - 1:X8}]:");
            Console.WriteLine();

            for (long addr = start; addr < start + count; addr++)
            {
                try
                {
                    long value = _processor.GetMemoryValue(addr);
                    string formatted = FormatValue(value, FORMAT_TRINARY);
                    Console.WriteLine($"  0x{addr:X8}: {value,15} = {formatted}");
                }
                catch
                {
                    Console.WriteLine($"  0x{addr:X8}: [ACCESS ERROR]");
                }
            }
        }

        void TraceControl(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("view"))
            {
                Console.WriteLine("Trace buffer:");
                if (_traceBuffer.Count == 0)
                {
                    Console.WriteLine("  (empty)");
                }
                else
                {
                    int count = Math.Min(20, _traceBuffer.Count);
                    var entries = _traceBuffer.Skip(Math.Max(0, _traceBuffer.Count - count)).ToList();
                    foreach (var entry in entries)
                    {
                        Console.WriteLine($"  {entry}");
                    }
                }
            }
            else if (args[0].Equals("on"))
            {
                _traceEnabled = true;
                Console.WriteLine("Trace enabled");
            }
            else if (args[0].Equals("off"))
            {
                _traceEnabled = false;
                Console.WriteLine("Trace disabled");
            }
            else
            {
                Console.WriteLine("Usage: trace [on|off|view]");
            }
        }

        void BreakpointControl(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: breakpoint <address>");
                return;
            }

            if (args[0].Equals("list"))
            {
                if (_breakpoints.Count == 0)
                {
                    Console.WriteLine("No breakpoints set");
                }
                else
                {
                    Console.WriteLine("Breakpoints:");
                    foreach (var bp in _breakpoints.OrderBy(b => b))
                    {
                        Console.WriteLine($"  0x{bp:X8}");
                    }
                }
                return;
            }

            if (args[0].Equals("clear"))
            {
                _breakpoints.Clear();
                Console.WriteLine("All breakpoints cleared");
                return;
            }

            long addr = ParseAddress(args[0]);
            _breakpoints.Add(addr);
            Console.WriteLine($"Breakpoint set at 0x{addr:X8}");
        }

        void DisassembleProgram(string[] args)
        {
            long start = args.Length > 0 ? ParseAddress(args[0]) : 0;
            int count = args.Length > 1 ? int.Parse(args[1]) : 16;

            var words = new List<Word18>();
            for (long i = start; i < start + count && i < _programWords.Count; i++)
            {
                words.Add(_programWords[(int)i]);
            }

            var state = _processor.GetState();
            Console.WriteLine($"Disassembly [0x{start:X8} .. 0x{start + count - 1:X8}]:");
            Console.WriteLine();

            var lines = T3Disassembler.Disassemble(words);

            foreach (var line in lines)
            {
                // Highlight current instruction
                long pc = state.PC;
                if (line.StartsWith($"0x{pc:X8}:"))
                {
                    Console.WriteLine($"> {line}");
                }
                else
                {
                    Console.WriteLine($"  {line}");
                }
            }
        }

        void ShowStack(string[] args)
        {
            int count = args.Length > 0 ? int.Parse(args[0]) : 16;
            var state = _processor.GetState();

            Console.WriteLine($"Stack (from SP=0x{state.SP:X8}):");
            Console.WriteLine();

            for (int i = 0; i < count; i++)
            {
                long addr = state.SP + i;
                try
                {
                    long value = _processor.GetMemoryValue(addr);
                    string formatted = FormatValue(value, FORMAT_TRINARY);
                    Console.WriteLine($"  0x{addr:X8}: {value,15} = {formatted}");
                }
                catch
                {
                    Console.WriteLine($"  0x{addr:X8}: [ACCESS ERROR]");
                }
            }
        }

        void ShowMemory(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: memory <start> <count>");
                return;
            }

            long start = ParseAddress(args[0]);
            int count = int.Parse(args[1]);

            DumpMemory(start, count);
        }

        // Вспомогательные методы

        void AddTraceEntry(string entry)
        {
            _traceBuffer.Enqueue(entry);
            if (_traceBuffer.Count > MAX_TRACE_SIZE)
            {
                _traceBuffer.Dequeue();
            }
        }

        string FormatValue(Word18 value, string format)
        {
            long valLong = value.ToLong();
            return format switch
            {
                FORMAT_NONARY => FormatAsNonary(valLong),
                FORMAT_27ARY => FormatAs27ary(valLong),
                _ => FormatAsTrinary(valLong)
            };
        }

        string FormatAsTrinary(long value)
        {
            string ternary = BalancedTernary.ToTernaryString(value, 27);
            return ternary;
        }

        string FormatAsNonary(long value)
        {
            string trinary = FormatAsTrinary(value);
            // Group by 2 trits
            var sb = new StringBuilder();
            for (int i = 0; i < trinary.Length; i += 2)
            {
                string pair = trinary.Substring(i, Math.Min(2, trinary.Length - i));
                char c = pair switch
                {
                    "--" => 'W',
                    "-0" => 'X',
                    "-+" => 'Y',
                    "0-" => 'Z',
                    "00" => '0',
                    "0+" => '1',
                    "+-" => '2',
                    "+0" => '3',
                    "++" => '4',
                    _ => '?'
                };
                sb.Append(c);
            }
            return sb.ToString();
        }

        string FormatAs27ary(long value)
        {
            string trinary = FormatAsTrinary(value);
            // Group by 3 trits
            var sb = new StringBuilder();
            char[] alphabet = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
            for (int i = 0; i < trinary.Length; i += 3)
            {
                string triple = trinary.Substring(i, Math.Min(3, trinary.Length - i));
                // Pad with zeros
                while (triple.Length < 3)
                {
                    triple += "0";
                }

                int t1 = triple[0] == '-' ? -1 : (triple[0] == '+' ? 1 : 0);
                int t2 = triple[1] == '-' ? -1 : (triple[1] == '+' ? 1 : 0);
                int t3 = triple[2] == '-' ? -1 : (triple[2] == '+' ? 1 : 0);

                int index = (t1 + 1) * 9 + (t2 + 1) * 3 + (t3 + 1);
                sb.Append(alphabet[index]);
            }
            return sb.ToString();
        }

        string GetRegName(int index)
        {
            if (index >= 0 && index < 9)
            {
                return "ABCDEFGHI"[index].ToString();
            }
            return $"R{index}";
        }

        long ParseAddress(string addrStr)
        {
            if (addrStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return long.Parse(addrStr.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }
            return long.Parse(addrStr);
        }
    }
}
