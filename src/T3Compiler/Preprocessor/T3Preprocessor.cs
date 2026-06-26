using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace T3Compiler.Preprocessor
{
    /// <summary>
    /// T3 C-compatible preprocessor. Handles #include, #define, #ifdef, #ifndef, #if, #else, #endif.
    /// In conditional expressions: true→1, false→0, maybe→error.
    /// </summary>
    public class T3Preprocessor
    {
        private readonly List<string> _includePaths;
        private readonly Dictionary<string, string> _macros;          // #define NAME value
        private readonly Dictionary<string, string[]> _funcMacros;    // #define NAME(args) body
        private readonly Stack<bool> _conditionalStack;               // true=active, false=skipped
        private bool _branchTaken;                                     // true once any branch of #if/#elif/#else is taken

        public T3Preprocessor(List<string>? includePaths = null)
        {
            _includePaths = includePaths ?? new List<string>();
            _macros = new Dictionary<string, string>();
            _funcMacros = new Dictionary<string, string[]>();
            _conditionalStack = new Stack<bool>();
            _conditionalStack.Push(true); // root level is active
        }

        /// <summary>
        /// Preprocess source code: expand includes, macros, handle conditionals.
        /// </summary>
        public string Process(string source, string? currentFile = null)
        {
            var output = new StringBuilder();
            var lines = source.Split('\n');
            int lineNum = 0;
            bool skipToEndif = false;
            int skipDepth = 0;

            foreach (var rawLine in lines)
            {
                lineNum++;
                string line = rawLine.TrimStart();

                // Handle #directives (anywhere in line)
                int hashIdx = line.IndexOf('#');
                if (hashIdx >= 0)
                {
                    string beforeHash = line.Substring(0, hashIdx).TrimEnd();
                    if (beforeHash.Length > 0 && _conditionalStack.Peek())
                    {
                        // Output text before #
                        string expanded = ExpandMacros(beforeHash);
                        output.AppendLine(expanded);
                    }
                    string directive = line.Substring(hashIdx + 1).Trim();
                    string dirName = directive.Split(' ')[0].ToLower();
                    string dirArgs = directive.Length > dirName.Length ? directive.Substring(dirName.Length).Trim() : "";

                    switch (dirName)
                    {
                        case "include":
                            if (_conditionalStack.Peek())
                            {
                                string incFile = dirArgs.Trim('"', '<', '>', ' ');
                                string? resolved = ResolveInclude(incFile, currentFile);
                                if (resolved != null)
                                {
                                    string content = File.ReadAllText(resolved);
                                    output.AppendLine($"// #include \"{incFile}\"");
                                    output.AppendLine(Process(content, resolved));
                                }
                                else
                                {
                                    throw new Exception($"Preprocessor error: cannot find include file '{incFile}'");
                                }
                            }
                            break;

                        case "define":
                            if (_conditionalStack.Peek())
                            {
                                int parenIdx = dirArgs.IndexOf('(');
                                if (parenIdx >= 0)
                                {
                                    // Function-like macro: #define NAME(args) body
                                    string name = dirArgs.Substring(0, parenIdx).Trim();
                                    int closeParen = dirArgs.IndexOf(')');
                                    string argsStr = dirArgs.Substring(parenIdx + 1, closeParen - parenIdx - 1);
                                    string body = dirArgs.Substring(closeParen + 1).Trim();
                                    _funcMacros[name] = new[] { argsStr, body };
                                }
                                else
                                {
                                    // Simple macro
                                    int spaceIdx = dirArgs.IndexOf(' ');
                                    if (spaceIdx >= 0)
                                    {
                                        string name = dirArgs.Substring(0, spaceIdx).Trim();
                                        string value = dirArgs.Substring(spaceIdx + 1).Trim();
                                        _macros[name] = value;
                                    }
                                    else
                                    {
                                        _macros[dirArgs.Trim()] = "1";
                                    }
                                }
                            }
                            break;

                        case "undef":
                            if (_conditionalStack.Peek())
                            {
                                string name = dirArgs.Trim();
                                _macros.Remove(name);
                                _funcMacros.Remove(name);
                            }
                            break;

                        case "ifdef":
                            {
                                string name = dirArgs.Trim();
                                bool cond = _macros.ContainsKey(name) || _funcMacros.ContainsKey(name);
                                if (!_conditionalStack.Peek()) cond = false;
                                _conditionalStack.Push(cond);
                            }
                            break;

                        case "ifndef":
                            {
                                string name = dirArgs.Trim();
                                bool cond = !(_macros.ContainsKey(name) || _funcMacros.ContainsKey(name));
                                if (!_conditionalStack.Peek()) cond = false;
                                _conditionalStack.Push(cond);
                            }
                            break;

                        case "if":
                            if (_conditionalStack.Peek())
                            {
                                bool cond = EvaluateConditional(dirArgs);
                                _conditionalStack.Push(cond);
                            }
                            else
                            {
                                _conditionalStack.Push(false);
                            }
                            break;

                        case "elif":
                            if (_conditionalStack.Count > 1)
                            {
                                _conditionalStack.Pop();
                                bool parentActive = _conditionalStack.Peek();
                                bool cond = parentActive && !_branchTaken && EvaluateConditional(dirArgs);
                                _conditionalStack.Push(cond);
                                if (cond) _branchTaken = true;
                            }
                            break;

                        case "else":
                            if (_conditionalStack.Count > 1)
                            {
                                _conditionalStack.Pop();
                                bool parentActive = _conditionalStack.Peek();
                                bool cond = parentActive && !_branchTaken;
                                _conditionalStack.Push(cond);
                                if (cond) _branchTaken = true;
                            }
                            break;

                        case "endif":
                            if (_conditionalStack.Count > 1)
                            {
                                _conditionalStack.Pop();
                                _branchTaken = false;
                            }
                            break;

                        case "error":
                            if (_conditionalStack.Peek())
                                throw new Exception($"Preprocessor #error: {dirArgs}");
                            break;

                        case "line":
                        case "pragma":
                            // Ignored
                            break;
                    }
                }
                else
                {
                    // Regular line: expand macros and output if active
                    if (_conditionalStack.Peek())
                    {
                        string expanded = ExpandMacros(line);
                        output.AppendLine(expanded);
                    }
                }
            }

            if (_conditionalStack.Count > 1)
                throw new Exception("Preprocessor error: unmatched #if/#ifdef at end of file");

            return output.ToString();
        }

        private bool EvaluateConditional(string expr)
        {
            expr = expr.Trim();
            // true→1, false→0, maybe→error
            if (expr == "1" || expr.Equals("true", StringComparison.OrdinalIgnoreCase)
                          || expr.Equals("истина", StringComparison.OrdinalIgnoreCase))
                return true;
            if (expr == "0" || expr.Equals("false", StringComparison.OrdinalIgnoreCase)
                          || expr.Equals("ложь", StringComparison.OrdinalIgnoreCase))
                return false;
            if (expr.Equals("maybe", StringComparison.OrdinalIgnoreCase)
             || expr.Equals("может", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Preprocessor error: maybe in #if requires explicit cast");
            // Check macro
            if (_macros.TryGetValue(expr, out var val))
            {
                if (val == "0") return false;
                return true;
            }
            // Try parse as integer
            if (long.TryParse(expr, out long n)) return n != 0;
            // Default: undefined macro → false
            return false;
        }

        private string ExpandMacros(string line)
        {
            // Expand function-like macros first: NAME(args)
            line = ExpandFuncMacros(line);
            // Then simple macros
            foreach (var kv in _macros)
            {
                line = ReplaceWord(line, kv.Key, kv.Value);
            }
            return line;
        }

        private string ExpandFuncMacros(string line)
        {
            foreach (var kv in _funcMacros)
            {
                string name = kv.Key;
                string argsDef = kv.Value[0]; // "arg1, arg2, ..."
                string body = kv.Value[1];
                // Find calls: NAME(...)
                int pos = 0;
                while (pos < line.Length)
                {
                    int nameIdx = line.IndexOf(name, pos, StringComparison.Ordinal);
                    if (nameIdx < 0) break;
                    // Check word boundary before name
                    if (nameIdx > 0 && IsIdentChar(line[nameIdx - 1]))
                    {
                        pos = nameIdx + name.Length;
                        continue;
                    }
                    // Check for '(' after name
                    int parenIdx = nameIdx + name.Length;
                    if (parenIdx >= line.Length || line[parenIdx] != '(')
                    {
                        pos = parenIdx;
                        continue;
                    }
                    // Find closing ')'
                    int closeIdx = line.IndexOf(')', parenIdx + 1);
                    if (closeIdx < 0) break;
                    // Extract call arguments
                    string callArgs = line.Substring(parenIdx + 1, closeIdx - parenIdx - 1);
                    // Split call arguments
                    var callArgList = callArgs.Split(',').Select(a => a.Trim()).ToArray();
                    // Split definition arguments
                    var defArgNames = argsDef.Split(',').Select(a => a.Trim()).ToArray();
                    // Build replacement
                    string replacement = body;
                    for (int i = 0; i < defArgNames.Length && i < callArgList.Length; i++)
                    {
                        replacement = ReplaceWord(replacement, defArgNames[i], callArgList[i]);
                    }
                    // Replace the macro call
                    line = line.Substring(0, nameIdx) + replacement + line.Substring(closeIdx + 1);
                    pos = nameIdx + replacement.Length;
                }
            }
            return line;
        }

        private static string ReplaceWord(string text, string word, string replacement)
        {
            var sb = new StringBuilder();
            int i = 0;
            while (i < text.Length)
            {
                int idx = text.IndexOf(word, i, StringComparison.Ordinal);
                if (idx < 0) { sb.Append(text.Substring(i)); break; }
                // Check word boundaries
                bool leftOk = idx == 0 || !IsIdentChar(text[idx - 1]);
                bool rightOk = idx + word.Length >= text.Length || !IsIdentChar(text[idx + word.Length]);
                if (leftOk && rightOk)
                {
                    sb.Append(text.Substring(i, idx - i));
                    sb.Append(replacement);
                    i = idx + word.Length;
                }
                else
                {
                    sb.Append(text.Substring(i, idx + word.Length - i));
                    i = idx + word.Length;
                }
            }
            return sb.ToString();
        }

        private static bool IsIdentChar(char c) =>
            char.IsLetterOrDigit(c) || c == '_';

        private string? ResolveInclude(string fileName, string? currentFile)
        {
            // Try relative to current file
            if (currentFile != null)
            {
                string dir = Path.GetDirectoryName(currentFile) ?? ".";
                string relPath = Path.Combine(dir, fileName);
                if (File.Exists(relPath)) return relPath;
            }
            // Try relative to include paths
            foreach (var incPath in _includePaths)
            {
                string fullPath = Path.Combine(incPath, fileName);
                if (File.Exists(fullPath)) return fullPath;
            }
            // Try as absolute
            if (File.Exists(fileName)) return fileName;
            return null;
        }
    }
}