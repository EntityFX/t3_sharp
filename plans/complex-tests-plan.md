# Plan: Comprehensive T-lang Compiler Tests + Missing Features

## Overview

Two phases:
1. **Phase A**: Implement missing compiler features (function parameters, do/while, switch/case, etc.)
2. **Phase B**: Add 24+ comprehensive test cases

---

## Phase A: Missing Compiler Features

### A1. Function Parameters (P1 — needed for recursion with arguments)

**Current state**: Parameters are parsed into `FunctionDef.Parameters` but `CodeGenerator.GenFunc` ignores them.

**Implementation**:
In `GenFunc`, after prologue PUSH, add code to POP parameters from stack into local variable slots:

```csharp
void GenFunc(FunctionDef f) {
    _varSlots.Clear(); _varSizes.Clear(); _arrDims.Clear(); _structFields.Clear(); _nextReg = 3;
    _epilogueLabel = Lbl("epilogue");
    Emit($"{f.Name}:");
    // Prologue: save caller-saved registers
    Emit("    PUSH RW"); Emit("    PUSH RX"); Emit("    PUSH RY"); Emit("    PUSH RZ");
    Emit("    PUSH R0"); Emit("    PUSH R1"); Emit("    PUSH R3"); Emit("    PUSH R4");
    
    // Pop parameters from stack into local slots
    // Parameters are pushed in reverse order by caller, so we pop in forward order
    foreach (var param in f.Parameters) {
        Alloc(param.Name, param.Type);  // allocate slot
        Emit("    POP R4");
        Store(param.Name, 4, 0);  // store param value into its slot (R4 = phys 8 = AddrReg)
    }
    
    foreach(var s in f.Body.Body) GenStmt(s);
    // Epilogue...
}
```

**Files**: `src/T3Compiler/CodeGen/CodeGenerator.cs`

### A2. do/while loop

**Current state**: `DoWhileStmt` not defined in AST, not parsed.

**Implementation**:
1. Add `DoWhileStmt` class to `Ast.cs`
2. Add parsing in `Parser.cs` (already has `do_kw` token type)
3. Add codegen in `CodeGenerator.cs`

```csharp
// Ast.cs
public class DoWhileStmt : Statement {
    public AstNode Condition;
    public Statement Body;
}

// Parser.cs — in ParseStmt():
if (t == TokenType.KwDo) return ParseDoWhile();

DoWhileStmt ParseDoWhile() {
    Expect(TokenType.KwDo);
    var body = ParseStmt();
    Expect(TokenType.KwWhile);
    Expect(TokenType.LParen);
    var cond = ParseExpr();
    Expect(TokenType.RParen);
    Expect(TokenType.Semicolon);
    return new DoWhileStmt { Body = body, Condition = cond };
}

// CodeGenerator.cs — in GenStmt():
case DoWhileStmt dws: GenDoWhile(dws); break;

void GenDoWhile(DoWhileStmt s) {
    string ll = Lbl("loop"), le = Lbl("wend");
    _loopStack.Push((le, ll));
    Emit($"{ll}:");
    GenStmt(s.Body);
    if (s.Condition is BinaryOp bo) {
        int a = GenExpr(bo.Left); int b = GenExpr(bo.Right);
        Emit($"    CMP {RegName(a)},{RegName(b)}");
        JumpCond(bo.Operator, ll);
    } else {
        int c = GenExpr(s.Condition);
        Emit($"    LI R0,0");
        Emit($"    CMP {RegName(c)},R0");
        JumpReg("JNE", ll);
    }
    Emit($"{le}:");
    _loopStack.Pop();
}
```

### A3. switch/case

**Current state**: Not implemented.

**Implementation**: Add `SwitchStmt`, `CaseStmt` to AST, parser, codegen.

```csharp
// Ast.cs
public class SwitchStmt : Statement {
    public AstNode Expression;
    public List<CaseStmt> Cases = new();
}
public class CaseStmt : Statement {
    public AstNode? Value;  // null = default
    public List<Statement> Body = new();
}

// Parser.cs
if (t == TokenType.KwSwitch) return ParseSwitch();

SwitchStmt ParseSwitch() {
    Expect(TokenType.KwSwitch); Expect(TokenType.LParen);
    var expr = ParseExpr(); Expect(TokenType.RParen);
    Expect(TokenType.LBrace);
    var cases = new List<CaseStmt>();
    while (Peek().Type is TokenType.KwCase or TokenType.KwDefault) {
        AstNode? val = null;
        if (Match(TokenType.KwCase)) val = ParseExpr();
        else Expect(TokenType.KwDefault);
        Expect(TokenType.Colon);
        var body = new List<Statement>();
        while (Peek().Type is not (TokenType.KwCase or TokenType.KwDefault or TokenType.RBrace or TokenType.EndOfFile))
            body.Add(ParseStmt());
        cases.Add(new CaseStmt { Value = val, Body = body });
    }
    Expect(TokenType.RBrace);
    return new SwitchStmt { Expression = expr, Cases = cases };
}

// CodeGenerator.cs
case SwitchStmt ss: GenSwitch(ss); break;

void GenSwitch(SwitchStmt s) {
    int exprReg = GenExpr(s.Expression);
    string end = Lbl("swend");
    var labels = new List<string>();
    for (int i = 0; i < s.Cases.Count; i++)
        labels.Add(Lbl("scase"));
    for (int i = 0; i < s.Cases.Count; i++) {
        if (s.Cases[i].Value != null) {
            int caseVal = GenExpr(s.Cases[i].Value);
            Emit($"    CMP {RegName(exprReg)},{RegName(caseVal)}");
            JumpReg("JE", labels[i]);
        }
    }
    // Default case or end
    var defaultCase = s.Cases.FindIndex(c => c.Value == null);
    if (defaultCase >= 0)
        Jmp(labels[defaultCase]);
    else
        Jmp(end);
    // Generate case bodies
    for (int i = 0; i < s.Cases.Count; i++) {
        Emit($"{labels[i]}:");
        foreach (var stmt in s.Cases[i].Body)
            GenStmt(stmt);
        Jmp(end);  // break
    }
    Emit($"{end}:");
}
```

### A4. goto/labels

**Current state**: `goto` keyword exists in lexer but not in parser/codegen.

**Implementation**: Add `GotoStmt`, `LabelStmt` to AST, parser, codegen.

### A5. enum

**Current state**: `enum` keyword exists in lexer but not in parser.

**Implementation**: Parse enum definitions, store as integer constants.

### A6. sizeof

**Current state**: `sizeof` keyword exists in lexer but not in parser/codegen.

**Implementation**: Evaluate at compile time, return integer literal.

### A7. Float literals in codegen

**Current state**: `FloatLiteral` parsed but `GenExpr` doesn't handle it.

**Implementation**: Add `FloatLiteral` case to `GenExpr` — convert to integer representation or throw `NotSupportedException`.

### A8. String support in codegen

**Current state**: `StringLiteral` parsed but not handled in codegen.

**Implementation**: Store string in data section, return pointer to first character.

---

## Phase B: Test Cases

### Category 1: Recursion (tests stack frame ABI)

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 1 | `Compile_Recursive_Factorial` | `fact(n) = n<=1 ? 1 : n*fact(n-1)`, fact(7) | 5040 |
| 2 | `Compile_Recursive_Fibonacci` | `fib(n) = n<=1 ? n : fib(n-1)+fib(n-2)`, fib(12) | 144 |
| 3 | `Compile_Recursive_SumToN` | `sum(n) = n<=0 ? 0 : n+sum(n-1)`, sum(100) | 5050 |
| 4 | `Compile_MutualRecursion_IsEven` | `isEven(n) = n==0?1:isOdd(n-1)`, `isOdd(n)=n==0?0:isEven(n-1)`, isEven(10) | 1 |

### Category 2: Complex Nested Loops

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 5 | `Compile_MatrixMul_3x3` | 3x3 matrix multiplication | 285 |
| 6 | `Compile_PrimeSieve_Eratosthenes` | Find primes up to 30, count them | 10 |
| 7 | `Compile_BubbleSort` | Sort [5,3,1,4,2], return sum of sorted | 15 |
| 8 | `Compile_TripleNestedLoop` | 3-level nested loop sum | 216 |

### Category 3: Edge Cases

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 9 | `Compile_NegativeNumbers` | Arithmetic with negatives | -15 |
| 10 | `Compile_ZeroIterations` | While loop that never executes | 42 |
| 11 | `Compile_LargeNumbers` | Numbers near tint max (193710244) | 387420488 |
| 12 | `Compile_DeepIfElse` | 5-level nested if/else | 5 |

### Category 4: Control Flow

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 13 | `Compile_TernaryOperator` | Ternary `?? :? :!` with comparisons | 3 |
| 14 | `Compile_BreakContinue` | Loop with break and continue | 25 |
| 15 | `Compile_ForLoop` | For loop with compound init | 55 |
| 16 | `Compile_WhileTrueBreak` | `while(1){if(x>10)break;x++;}` | 11 |

### Category 5: Arrays and Pointers

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 17 | `Compile_2DArray_Sum` | 3x3 2D array sum | 45 |
| 18 | `Compile_PointerSwap` | Swap via pointers | 15 |
| 19 | `Compile_ArrayReverse` | Reverse array in-place | 15 |

### Category 6: Structs

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 20 | `Compile_NestedStruct` | Struct containing struct | 30 |
| 21 | `Compile_StructArray` | Array of structs | 60 |

### Category 7: Compound Assignments

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 22 | `Compile_CompoundAddSub` | `+=`, `-=`, `*=`, `/=` | 10 |
| 23 | `Compile_CompoundBitwise` | `&=`, `|=`, `^=` | 7 |

### Category 8: Preprocessor

| # | Test Name | Description | Expected |
|---|-----------|-------------|----------|
| 24 | `Compile_IncludeTmath` | `#include <tmath.h>` + `#define` | 10 |

---

## Implementation Order

1. **Phase A1**: Function parameters in codegen (needed for recursion tests)
2. **Phase A2**: do/while loop
3. **Phase A3**: switch/case
4. **Phase B**: All 24 test cases
5. Build and run all tests

## Risk Assessment

- **Recursion tests**: May hit stack/memory limits if recursion depth is too large. Use n=7 for factorial (depth 7), n=12 for fibonacci (exponential calls).
- **Large number tests**: Must stay within tint range ±193,710,244.
- **Multi-dimensional arrays**: Codegen for FlatIdx with multi-dim arrays may have register allocation issues with deep expressions.
- **Break/continue**: Must verify loop stack tracking works correctly with nested loops.
- **Function parameters**: The POP in prologue must match the PUSH order in EmitCall (reverse order).

## Verification

After implementation:
1. `dotnet build T3Sharp.sln` — must succeed
2. `dotnet test` — all tests must pass