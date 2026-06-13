using System;
using System.Collections.Generic;
using System.Text;
using T3Compiler.Parser;
using T3Simulator.Common;
using TritTypes;

namespace T3Compiler.CodeGen
{
    /// <summary>
    /// Generates T3 assembly code from the parsed AST.
    /// Uses 9 GP registers (R0-R8) mapped as:
    ///   R0 = RW (workspace/accumulator), R1..R5 = temporaries, R6-R8 = saved
    /// Functions: params passed via stack (PUSH before CALL, POP inside callee)
    /// </summary>
    public class CodeGenerator
    {
        private readonly AstProgram _program;
        private readonly StringBuilder _output;
        private int _labelCounter;
        private readonly Dictionary<string, int> _varSlots; // variable name → stack offset
        private int _nextStackSlot;

        public CodeGenerator(AstProgram program)
        {
            _program = program;
            _output = new StringBuilder();
            _labelCounter = 0;
            _varSlots = new Dictionary<string, int>();
            _nextStackSlot = 0;
        }

        public string Generate()
        {
            EmitLine("; T-lang compiled output → T3 assembly");
            EmitLine("; ====================================");
            EmitLine();

            // Entry point at address 0: CALL main so RET works properly
            EmitLine("__entry:");
            EmitLine("    LI R0, main");
            EmitLine("    CALL R0");
            EmitLine("    HALT");       // if main returns, halt cleanly
            EmitLine();

            // Generate all functions (including main)
            foreach (var func in _program.Functions)
            {
                GenerateFunction(func);
            }

            // If no main, halt
            EmitLine("    HALT");
            EmitLine();

            return _output.ToString();
        }

        private void GenerateFunction(FunctionDef func)
        {
            _varSlots.Clear();
            _nextStackSlot = 0;

            EmitLine();
            EmitLine($"; === Function: {func.Name} ===");
            EmitLine($"{func.Name}:");

            // Allocate stack frame
            if (_nextStackSlot > 0)
                EmitLine($"    ; stack frame: {_nextStackSlot} slots");

            // Generate body statements
            foreach (var stmt in func.Body.Body)
            {
                GenerateStatement(stmt);
            }

            EmitLine("    RET");
            EmitLine();
        }

        private void GenerateStatement(Statement stmt)
        {
            switch (stmt)
            {
                case ExpressionStmt es:
                    if (es.Expression != null)
                        GenerateExpr(es.Expression, 0);
                    break;

                case VarDeclaration vd:
                    AllocLocal(vd.Name);
                    if (vd.Initializer != null)
                    {
                        int reg = GenerateExpr(vd.Initializer, 0);
                        StoreLocal(vd.Name, reg);
                    }
                    break;

                case ReturnStmt rs:
                    if (rs.Value != null)
                    {
                        int r = GenerateExpr(rs.Value, 0);
                        EmitLine($"    MOV R2, R{r}    ; return value");
                    }
                    EmitLine("    RET");
                    break;

                case CompoundStmt cs:
                    foreach (var s in cs.Body)
                        GenerateStatement(s);
                    break;

                case IfStmt ifs:
                    {
                        int condReg = GenerateExpr(ifs.Condition, 0);
                        string labelElse = NewLabel("else");
                        string labelEnd = NewLabel("endif");
                        // if cond ≈ true (+), jump to then; else jump to maybe/else
                        EmitLine($"    ; if R{condReg}");
                        EmitLine($"    LI R1, 0");
                        EmitLine($"    CMP R{condReg}, R1");
                        EmitLine($"    LI R2, {labelElse}");
                        EmitLine($"    JL R2          ; false => else");
                        EmitLine($"    JE R2          ; maybe (0) => else");
                        GenerateStatement(ifs.ThenBody);
                        EmitLine($"    LI R2, {labelEnd}");
                        EmitLine($"    JMP R2");
                        EmitLine($"{labelElse}:");
                        if (ifs.ElseBody != null) GenerateStatement(ifs.ElseBody);
                        EmitLine($"{labelEnd}:");
                    }
                    break;

                case WhileStmt ws:
                    {
                        string labelLoop = NewLabel("loop");
                        string labelEnd = NewLabel("endloop");
                        EmitLine($"{labelLoop}:");
                        int condReg = GenerateExpr(ws.Condition, 0);
                        EmitLine($"    LI R1, 0");
                        EmitLine($"    CMP R{condReg}, R1");
                        EmitLine($"    LI R2, {labelEnd}");
                        EmitLine($"    JL R2");
                        EmitLine($"    JE R2");
                        GenerateStatement(ws.Body);
                        EmitLine($"    LI R2, {labelLoop}");
                        EmitLine($"    JMP R2");
                        EmitLine($"{labelEnd}:");
                    }
                    break;

                case ForStmt fs:
                    {
                        if (fs.Init != null) GenerateExpr(fs.Init, 0);
                        string labelLoop = NewLabel("forloop");
                        string labelEnd = NewLabel("forend");
                        EmitLine($"{labelLoop}:");
                        if (fs.Condition != null)
                        {
                            int cr = GenerateExpr(fs.Condition, 0);
                            EmitLine($"    LI R1, 0");
                            EmitLine($"    CMP R{cr}, R1");
                            EmitLine($"    LI R2, {labelEnd}");
                            EmitLine($"    JL R2");
                            EmitLine($"    JE R2");
                        }
                        GenerateStatement(fs.Body);
                        if (fs.Step != null) GenerateExpr(fs.Step, 0);
                        EmitLine($"    LI R2, {labelLoop}");
                        EmitLine($"    JMP R2");
                        EmitLine($"{labelEnd}:");
                    }
                    break;
            }
        }

        /// <summary>
        /// Generate code for an expression, returns the register containing the result.
        /// targetReg: 0 means auto-allocate, otherwise try to place result there.
        /// </summary>
        private int GenerateExpr(AstNode node, int targetReg)
        {
            if (targetReg == 0) targetReg = AllocReg();

            switch (node)
            {
                case IntegerLiteral il:
                    // Parse t-lang literal to decimal integer for LI
                    long intVal = ParseIntegerLiteral(il);
                    EmitLine($"    LI R{targetReg}, {intVal}   ; {il.Value}{il.Suffix ?? ""}");
                    return targetReg;

                case FloatLiteral fl:
                    // Store float in memory, load via FLW
                    EmitLine($"    LI R{targetReg}, 0    ; float literal (simplified)");
                    return targetReg;

                case Identifier id:
                    LoadLocal(id.Name, targetReg);
                    return targetReg;

                case BooleanLiteral bl:
                    int bv = bl.Value == true ? 1 : (bl.Value == false ? -1 : 0);
                    EmitLine($"    LI R{targetReg}, {bv}   ; boolean");
                    return targetReg;

                case BinaryOp bo:
                    int left = GenerateExpr(bo.Left, 0);
                    int right = GenerateExpr(bo.Right, 0);
                    string op = bo.Operator switch
                    {
                        "+" => "ADD", "-" => "SUB", "*" => "MUL", "/" => "DIV", "%" => "MOD",
                        "&" => "AND", "|" => "OR", "^" => "XOR",
                        "<<" => "SHL", ">>" => "SHR",
                        "==" or "!=" or "<" or ">" or "<=" or ">=" => "CMP",
                        _ => "ADD"
                    };
                    if (op == "CMP")
                    {
                        EmitLine($"    CMP R{left}, R{right}");
                        EmitLine($"    ; condition result in Cond (R{targetReg} set after)");
                        // Store Cond in register via branching
                        string labelT = NewLabel("cmpT"), labelF = NewLabel("cmpF"), labelD = NewLabel("cmpD");
                        EmitLine($"    LI R1, {labelT}");
                        EmitLine($"    {GetJumpCond(bo.Operator)} R1");
                        EmitLine($"    LI R{targetReg}, -1   ; false");
                        EmitLine($"    LI R1, {labelD}");
                        EmitLine($"    JMP R1");
                        EmitLine($"{labelT}:");
                        EmitLine($"    LI R{targetReg}, 1    ; true");
                        EmitLine($"{labelD}:");
                    }
                    else
                    {
                        EmitLine($"    {op} R{targetReg}, R{left}, R{right}");
                    }
                    return targetReg;

                case UnaryOp uo:
                    int opReg = GenerateExpr(uo.Operand, 0);
                    string unOp = uo.Operator switch { "-" => "NEG", "!" => "NEG", "~" => "NEG", _ => "MOV" };
                    EmitLine($"    {unOp} R{targetReg}, R{opReg}");
                    return targetReg;

                case Assignment ass:
                    int valReg = GenerateExpr(ass.Value, 0);
                    if (ass.Target is Identifier id2)
                    {
                        StoreLocal(id2.Name, valReg);
                    }
                    EmitLine($"    MOV R{targetReg}, R{valReg}   ; assign");
                    return targetReg;

                case FunctionCall fc:
                    // Push args right-to-left, CALL, pop result
                    for (int i = fc.Arguments.Count - 1; i >= 0; i--)
                    {
                        int argReg = GenerateExpr(fc.Arguments[i], 0);
                        EmitLine($"    PUSH R{argReg}     ; arg {i}");
                    }
                    EmitLine($"    LI R1, {fc.FunctionName}");
                    EmitLine($"    CALL R1");
                    for (int i = 0; i < fc.Arguments.Count; i++)
                        EmitLine($"    POP R0             ; discard arg"); // simplified
                    EmitLine($"    MOV R{targetReg}, R2   ; return value");
                    return targetReg;

                default:
                    EmitLine($"    LI R{targetReg}, 0    ; unhandled node");
                    return targetReg;
            }
        }

        private string GetJumpCond(string op) => op switch
        {
            "==" => "JE", "!=" => "JNE", "<" => "JL", ">" => "JG", "<=" => "JL", ">=" => "JG",
            _ => "JE"
        };

        private long ParseIntegerLiteral(IntegerLiteral il)
        {
            string val = il.Value;
            // 0t+-0 style
            if (val.StartsWith("0t"))
                return BalancedTernary.ParseToLong(val.Substring(2).Replace("_", ""));
            // 0y style (27-ary)
            if (val.StartsWith("0y"))
                return ParseTryxToLong(val.Substring(2));
            // 0n style (9-ary)
            if (val.StartsWith("0n"))
                return ParseNinaryToLong(val.Substring(2));
            // Decimal with optional sign
            return long.TryParse(val, out long n) ? n : 0;
        }

        private long ParseTryxToLong(string s)
        {
            char[] alpha = "NOPQRSTUVWXYZ0123456789ABCD".ToCharArray();
            string trits = "";
            foreach (char c in s.ToUpper())
            {
                int idx = Array.IndexOf(alpha, c);
                if (idx < 0) continue;
                int t1 = idx / 9 - 1, t2 = (idx / 3) % 3 - 1, t3 = idx % 3 - 1;
                trits += TritChar(t1) + TritChar(t2) + TritChar(t3);
            }
            return BalancedTernary.ParseToLong(trits);
        }

        private long ParseNinaryToLong(string s)
        {
            string trits = "";
            foreach (char c in s.ToUpper())
            {
                trits += c switch
                {
                    'W' => "--", 'X' => "-0", 'Y' => "-+", 'Z' => "0-",
                    '0' => "00", '1' => "0+", '2' => "+-", '3' => "+0", '4' => "++",
                    _ => "00"
                };
            }
            return BalancedTernary.ParseToLong(trits);
        }

        private string TritChar(int t) => t == -1 ? "-" : (t == 1 ? "+" : "0");

        // === Register allocation (simplified: round-robin R0-R5) ===
        private int _nextReg = 0;
        private int AllocReg() { int r = _nextReg; _nextReg = (_nextReg + 1) % 6; return r; }

        // === Local variable slots (stack-based) ===
        private void AllocLocal(string name)
        {
            if (!_varSlots.ContainsKey(name))
            {
                _varSlots[name] = _nextStackSlot++;
            }
        }

        private void StoreLocal(string name, int reg)
        {
            if (_varSlots.TryGetValue(name, out int slot))
            {
                EmitLine($"    ; store local '{name}' (slot {slot}) — simplified: kept in reg");
                EmitLine($"    MOV R{reg+1}, R{reg}  ; preserve in next reg");
            }
        }

        private void LoadLocal(string name, int targetReg)
        {
            if (_varSlots.TryGetValue(name, out int slot))
            {
                EmitLine($"    ; load local '{name}' (slot {slot}) — simplified");
                EmitLine($"    MOV R{targetReg}, R{targetReg}  ; placeholder");
            }
        }

        // === Helpers ===
        private string NewLabel(string prefix) => $"{prefix}_{_labelCounter++}";
        private void EmitLine(string line = "") => _output.AppendLine(line);
    }
}