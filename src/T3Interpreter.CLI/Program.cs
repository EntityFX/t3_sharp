using System;
using System.IO;
using T3Compiler;
using T3Compiler.Lexer;
using T3Compiler.Parser;
using T3Compiler.Preprocessor;

namespace T3Interpreter.CLI;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length >= 1 && File.Exists(args[0]))
        {
            RunFile(args[0]);
            return;
        }
        RunRepl();
    }

    static void RunFile(string path)
    {
        string src = File.ReadAllText(path);
        Run(src);
    }

    static void RunRepl()
    {
        Console.WriteLine("T3 Interpreter REPL 1.0");
        Console.WriteLine("Enter T-lang statements or 'exit'. Supports: 0t+--, 0nZ, 0yC, 0b101");
        var buffer = "";
        while (true)
        {
            Console.Write(buffer.Length > 0 ? "..> " : "t3> ");
            var line = Console.ReadLine();
            if (line == null || line.Trim() == "exit") break;
            buffer += line + "\n";
            // If buffer contains a complete function or a return statement, execute it
            if (buffer.Contains("return ") || buffer.TrimEnd().EndsWith("}"))
            {
                // Wrap in main if not provided
                if (!buffer.Contains("tint main("))
                    buffer = "tint main(){" + buffer + "}";
                try { Run(buffer); } catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                buffer = "";
            }
        }
    }

    static void Run(string source)
    {
        var pp = new T3Preprocessor();
        var pre = pp.Process(source);
        var tok = new Tokenizer(pre).Tokenize();
        var ast = new Parser(tok).ParseProgram();
        var interpreter = new T3Interpreter(ast);
        long result = interpreter.Run();
        Console.WriteLine(result);
    }
}