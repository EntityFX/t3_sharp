using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace T3Assembler
{
    /// <summary>
    /// .macro processor for T3 assembler. Expands .macro/.endm definitions inline.
    /// </summary>
    public class MacroProcessor
    {
        readonly Dictionary<string, (string[] args, string body)> _macros = new();

        public string Process(string src)
        {
            // First pass: collect macro definitions
            var lines = src.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var output = new StringBuilder();
            string? currentMacro = null;
            var currentArgs = Array.Empty<string>();
            var macroBody = new StringBuilder();

            foreach (var line in lines)
            {
                string t = line.Trim();
                if (currentMacro != null)
                {
                    if (t.StartsWith(".endm"))
                    {
                        _macros[currentMacro] = (currentArgs, macroBody.ToString().Trim());
                        currentMacro = null;
                        macroBody.Clear();
                    }
                    else
                    {
                        macroBody.AppendLine(line);
                    }
                    continue;
                }

                if (t.StartsWith(".macro"))
                {
                    var parts = t.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        currentMacro = parts[1];
                        currentArgs = parts.Length > 2 ? parts[2..] : Array.Empty<string>();
                    }
                    continue;
                }

                output.AppendLine(line);
            }

            // Second pass: expand macro invocations
            string result = output.ToString();
            foreach (var kv in _macros)
            {
                // Replace CALL macroName with macro body
                // Simple approach: find lines that start with the macro name
                var sb = new StringBuilder();
                foreach (
                    var line in result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                )
                {
                    string lt = line.Trim();
                    if (lt.StartsWith(kv.Key + " ") || lt == kv.Key)
                    {
                        var parts = lt.Split(
                            new[] { ' ', '\t', ',' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        string body = kv.Value.body;
                        for (int i = 0; i < kv.Value.args.Length && i + 1 < parts.Length; i++)
                            body = body.Replace("\\" + kv.Value.args[i], parts[i + 1]);
                        sb.AppendLine(body);
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
                result = sb.ToString();
            }

            return result;
        }
    }
}
