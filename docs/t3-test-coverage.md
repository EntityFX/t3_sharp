# T3 Test Coverage Report

**Version**: 2.1 | **Date**: 2026-06-30 | **Total Tests**: 412 (Passed: 412, Failed: 0, Skipped: 1)

---

## 1. Test Project Structure

| # | Project | Tests | Files | Description |
|---|---------|-------|-------|-------------|
| 1 | `TritTypes.Tests` | 126 | 7 | Word18/Word54, T3Float, BalancedTernary, T3Int, Trit, Tryte, TScii |
| 2 | `T3Simulator.Common.Tests` | 71 | 5 | Assembler/disassembler, ALU, FPU, Memory, Roundtrip |
| 3 | `T3Simulator.InOrder.Tests` | 147 | 10 | ISA instructions, FPU, T-lang compiler, ABI v3/v4, Matrix, Advanced |
| 4 | `T3Interpreter.Tests` | 68 | 1 | T-lang interpreter + compiler equivalence tests |
| | **Total** | **412** | **23** | |

---

## 2. ISA Opcode Coverage

### 2.1 Integer ALU — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 10 | ADD | R | ✅ | BenchmarkTests.ADD, InOrderProcessorTests.SimpleArithmeticTest, TLangCompilerTests (via compiler) |
| 11 | SUB | R | ✅ | BenchmarkTests.SUB, InOrderProcessorTests.SimpleArithmeticTest |
| 12 | MUL | R | ✅ | BenchmarkTests.MUL, InOrderProcessorTests.SimpleArithmeticTest |
| 13 | DIV | R | ✅ | BenchmarkTests.DIV, InOrderProcessorTests.SimpleArithmeticTest |
| 14 | MOD | R | ✅ | BenchmarkTests.MOD |
| 15 | NEG | R | ✅ | BenchmarkTests.NEG |
| 20 | ADDI | I | ✅ | BenchmarkTests.ADDI, InOrderProcessorTests.ImmediateArithmeticTest |
| 21 | SUBI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest, TLangCompilerTests (frame alloc) |
| 22 | MULI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |
| 23 | DIVI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |
| 24 | MODI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |
| 25 | NEGI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |

### 2.2 Logical — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 30 | AND | R | ✅ | BenchmarkTests.AND, InOrderProcessorTests |
| 31 | OR | R | ✅ | BenchmarkTests.OR |
| 32 | XOR | R | ✅ | BenchmarkTests.XOR |
| 33 | ANDI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |
| 34 | ORI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |
| 35 | XORI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |

### 2.3 Shifts — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 40 | SHL | R | ✅ | BenchmarkTests.SHL |
| 41 | SHR | R | ✅ | BenchmarkTests.SHR |
| 42 | SHLI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |
| 43 | SHRI | I | ✅ | InOrderProcessorTests.ImmediateArithmeticTest |

### 2.4 Memory & Stack — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 50 | LOAD | R | ✅ | BenchmarkTests.LOAD_STORE, InOrderProcessorTests |
| 51 | LOADI | I | ✅ | AssemblerTests, TLangCompilerTests (all array/struct tests) |
| 52 | STORE | R | ✅ | BenchmarkTests.LOAD_STORE |
| 53 | STOREI | I | ✅ | AssemblerTests, TLangCompilerTests (all array/struct tests) |
| 54 | PUSH | R | ✅ | BenchmarkTests.PUSH_POP, InOrderProcessorTests.StackTest |
| 55 | POP | R | ✅ | BenchmarkTests.PUSH_POP, InOrderProcessorTests.StackTest |
| — | PUSHI | I | ✅ | InOrderProcessorTests.PUSHI_POPI_Immediate |
| — | POPI | I | ✅ | InOrderProcessorTests.PUSHI_POPI_Immediate |

### 2.5 Control Flow — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 60 | CMP | R | ✅ | BenchmarkTests.CMP, TLangCompilerTests |
| 61 | CMPI | I | ✅ | InOrderProcessorTests, AssemblerTests |
| 62 | JMP | J | ✅ | BenchmarkTests.Branch, AssemblerTests |
| 63 | JE | J | ✅ | BenchmarkTests.Branch, TLangCompilerTests |
| 64 | JNE | J | ✅ | BenchmarkTests.Branch, TLangCompilerTests |
| 65 | JL | J | ✅ | BenchmarkTests.Branch, TLangCompilerTests |
| 66 | JG | J | ✅ | BenchmarkTests.Branch, TLangCompilerTests |
| 67 | JM | J | ✅ | AssemblerTests (JM alias for JE) |
| 68 | JLE | J | ✅ | AssemblerTests |
| 69 | JGE | J | ✅ | AssemblerTests |
| 70 | CALL | J | ✅ | BenchmarkTests.CALL_RET, AssemblerTests, TLangCompilerTests (all function tests) |
| 71 | RET | – | ✅ | BenchmarkTests.CALL_RET, TLangCompilerTests (all function tests) |

### 2.6 I/O — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 80 | IN | R | ✅ | InOrderProcessorTests.IOTest |
| 81 | OUT | R | ✅ | InOrderProcessorTests.IOTest |
| 82 | INI | I | ✅ | InOrderProcessorTests.IOTest |
| 83 | OUTI | I | ✅ | InOrderProcessorTests.IOTest, T3LibraryTests.Test_PutChar/Test_PrintInt/Test_PrintFloat/Test_PrintString |

### 2.7 System — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 0 | HALT | – | ✅ | Implicit in all processor tests |
| 1 | NOP | – | ✅ | InOrderProcessorTests |

### 2.8 FPU — 100% Covered (17/17 opcodes)

| Op | Mnemonic | Tested | Test Locations |
|----|----------|--------|----------------|
| 100 | FADD | ✅ | FpuInstructionTests.FADD_Works, BenchmarkTests.FADD_Works |
| 101 | FSUB | ✅ | FpuInstructionTests.FSUB_Works |
| 102 | FMUL | ✅ | FpuInstructionTests.FMUL_Works |
| 103 | FDIV | ✅ | FpuInstructionTests.FDIV_Works |
| 104 | FSQRT | ✅ | FpuInstructionTests.FSQRT_Works |
| 105 | FABS | ✅ | FpuInstructionTests.FABS_Works |
| 106 | FNEG | ✅ | FpuInstructionTests.FNEG_Works |
| 107 | FCMP | ✅ | FpuInstructionTests.FCMP_Greater/Less/Equal, FpuCompareAndBranch_CorrectlyJumps |
| 108 | FTOI | ✅ | FpuInstructionTests.FTOI_Works |
| 109 | ITOF | ✅ | FpuInstructionTests.ITOF_Works |
| 110 | FTOF | ✅ | FpuInstructionTests (implicit) |
| 111 | FLW | ✅ | FpuInstructionTests.FSW_FLW_RoundTrip |
| 112 | FSW | ✅ | FpuInstructionTests.FSW_StoresFloatToMemory, FSW_FLW_RoundTrip |
| 113 | FMOV | ✅ | FpuInstructionTests.FMOV_FtoF/FMOV_RtoF/FMOV_FtoR |
| 114 | FCLASS | ✅ | FpuInstructionTests.FCLASS_Classifies |
| 115 | FSWAP | ✅ | FpuInstructionTests.FSWAP_Works |
| 116 | FZERO | ✅ | FpuInstructionTests.FZERO_Works |

### 2.9 Data Movement — 100% Covered

| Op | Mnemonic | Type | Tested | Test Locations |
|----|----------|------|--------|----------------|
| 2 | MOV | R | ✅ | AssemblerTests, TLangCompilerTests |
| 3 | MOVI | I | ✅ | InOrderProcessorTests |
| 4 | LI | I | ✅ | AssemblerTests, InOrderProcessorTests, TLangCompilerTests |
| 5 | LIMM | I | ✅ | AssemblerTests, T3AssemblerTests.Assemble_LIMM_OffsetCorrectness |
| — | GETSP | I | ✅ | InOrderProcessorTests (implicit in StackTest) |

---

## 3. T-lang Compiler Coverage

### 3.1 Arithmetic & Expressions

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| Integer arithmetic | 8+ | ✅ | Compile_SimpleArithmetic_Returns42, Compile_ComplexExpression, Compile_CompoundAddSub |
| Negative numbers | 1 | ✅ | Compile_NegativeNumbers |
| Large numbers | 1 | ✅ | Compile_LargeNumbers |
| Compound assignments | 1 | ✅ | Compile_CompoundAddSub |
| Bitwise operations | 1 | ✅ | Compile_CompoundBitwise |
| Boolean ops | 1 | ✅ | Compile_BooleanOps |
| Ternary operator | 1 | ✅ | Compile_TernaryOperator |
| Constant folding | 4 | ✅ | Compile_ConstantFolding_Add/Mul/Complex/WithVar |

### 3.2 Control Flow

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| if/else | 2+ | ✅ | Compile_IfElse_Branch, Compile_DeepIfElse, all Equiv_If* tests |
| while | 3+ | ✅ | Compile_While_SumTo5, Compile_While_SumMulti, Compile_WhileTrueBreak |
| do-while | 2 | ✅ | Compile_DoWhile_Sum, Compile_DoWhile_AtLeastOnce |
| for | 1 | ✅ | Compile_ForLoop |
| break/continue | 1 | ✅ | Compile_BreakContinue |
| switch/case | 2 | ✅ | Compile_SwitchCase_Basic, Compile_SwitchCase_Default |
| nested loops | 3 | ✅ | Compile_NestedWhile_SumProd/SimpleSum/SimpleSumLong, Compile_TripleNestedLoop |

### 3.3 Arrays

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| 1D array | 4 | ✅ | Compile_Array_Sum, Compile_Array_Mul, Compile_2DArray_Sum, Compile_ArrayReverse |
| Multidimensional | 2+ | ✅ | Compile_MatrixMul_2x2/3x3, Equiv_MultidimArray_2x3 |
| Sorting | 1 | ✅ | Compile_BubbleSort |
| Sieve | 1 | ✅ | Compile_PrimeSieve_Eratosthenes |

### 3.4 Pointers

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| Dereference | 1 | ✅ | Compile_Pointer_Deref |
| Arithmetic | 1 | ✅ | Compile_Pointer_Arithmetic |
| Swap via pointers | 1 | ✅ | Compile_PointerSwap |
| Struct via pointer | 1 | ✅ | Compile_StructPointerAccess |
| Struct array + pointer | 1 | ✅ | Compile_PointerArrayStruct |

### 3.5 Structs

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| Field access | 3 | ✅ | Compile_Struct_FieldAccess, Compile_NestedStruct, Equiv_StructWrite/Sum |
| Struct array | 1 | ✅ | Compile_StructArray |
| Struct pointer | 1 | ✅ | Compile_StructPointerAccess |

### 3.6 Functions & Recursion

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| Simple function call | 2+ | ✅ | Equiv_FunctionCall, Equiv_FunctionWithParam |
| Recursion | 5 | ✅ | Compile_Recursive_Factorial, Compile_Recursive_SumToN, Compile_Recursive_Factorial_While |
| Double recursion (Fibonacci) | 2 | ✅ | ABIv3_DoubleRecursion, Equiv_DoubleRecursion |
| Mutual recursion | 2 | ✅ | Compile_MutualRecursion_IsEven, ABIv3_MutualRecursionWithLocal |
| Two calls in expression | 1 | ✅ | Compile_TwoCallsInExpression |
| Nested calls (ABI) | 1 | ⚠️ | Equiv_NestedFunctionCalls_Compiler [**Ignored** — ABI limitation] |

### 3.7 Other Language Features

| Construct | Tests | Covered | Test Names |
|-----------|-------|---------|------------|
| Enums | 2 | ✅ | Compile_Enum_Basic, Compile_Enum_InFunction |
| Typedef | 2 | ✅ | Compile_Typedef_TIntAlias, Compile_Typedef_WithFunction |
| Preprocessor (#define) | 2 | ✅ | Compile_Preprocessor_Define, Compile_Preprocessor_MacroExpr |
| Preprocessor (#ifdef) | 1 | ✅ | Compile_Preprocessor_IfDef |
| Ternary literal | 1 | ✅ | Compile_TernaryLiteral |
| Strings | 1 | ✅ | Compile_Strings_And_Strlen |
| Global variables | 1+ | ✅ | Equiv_GlobalVar, ComplexIntegrationTest_GlobalVarsAndIncludes |
| Includes | 1+ | ✅ | Compile_IncludeTmath, ComplexIntegrationTest_GlobalVarsAndIncludes |
| Source errors | 2 | ✅ | SourceError_MissingSemicolon, SourceError_UnexpectedToken |

---

## 4. Processor Feature Coverage

| Feature | Tested | Test Locations |
|---------|--------|----------------|
| **Predication** | ✅ | BenchmarkTests.Predication, InOrderProcessorTests.PredicationTest, FADD_Predicated_Honored |
| **Stack** | ✅ | BenchmarkTests.PUSH_POP/CALL_RET, StackTest, StackOverflowGuard |
| **I/O** | ✅ | IOTest, Test_PutChar/Test_PrintInt/Test_PrintFloat/Test_PrintString |
| **FPU** | ✅ | All 17 FPU opcodes (see §2.8) |
| **Matrix multiplication** | ✅ | MatrixMul6x6Tests, MatrixMultiplication_IntegrationTest, MatrixMultiplication_6x6_InOrder_Test |
| **Taylor sin** | ✅ | TaylorSin_ApproximatesSinX |
| **Quadratic discriminant** | ✅ | Quadratic_Discriminant_Integer, CLI_Quadratic_Verification |
| **LIMM** | ✅ | AssemblerTests, T3AssemblerTests |
| **Register windowing** | ❌ | Planned feature, not yet integrated |
| **VLIW** | ❌ | T3VliwAssembler exists but no tests |
| **T3-54 mode** | ✅ | Test_T3_54_Int128, Word54 tests in TritTypes.Tests |

---

## 5. Interpreted T-lang Coverage (T3Interpreter.Tests)

The T3Interpreter provides a reference implementation of the T-lang language, used to validate compiler output via equivalence tests (`EE` method).

| Category | Tests | Notes |
|----------|-------|-------|
| Arithmetic | 3 | Add, NegNumbers, ComplexExpr |
| Loops | 4 | WhileSum, ForLoop, DoWhile, NestedWhile |
| Conditionals | 3 | IfElseTrue/False, SwitchCase |
| Recursion | 3 | RecursiveFact, Fibonacci, DoubleRecursion |
| Preprocessor | 1 | PreprocDefine |
| Break/Continue | 1 | BreakContinue |
| Literals | 8 | Ternary (3), Base9 (2), Base27 (2), Decimal (2) |

### Equivalence Tests (EE)

| Test | Expected | Status |
|------|----------|--------|
| Equiv_Add | 42 | ✅ |
| Equiv_WhileSum | 15 | ✅ |
| Equiv_ComplexExpr | 27 | ✅ |
| Equiv_NegNumbers | -20 | ✅ |
| Equiv_ForLoop | 55 | ✅ |
| Equiv_DoWhile | 55 | ✅ |
| Equiv_SwitchCase | 2 | ✅ |
| Equiv_TernaryLiteral | 5 | ✅ |
| Equiv_Base9Literal | 4 | ✅ |
| Equiv_Base27Literal | -1 | ✅ |
| Equiv_DecimalLiteral | 42 | ✅ |
| Equiv_IfElseTrue | 1 | ✅ |
| Equiv_IfElseFalse | -1 | ✅ |
| Equiv_FunctionCall | 15 | ✅ |
| Equiv_SimpleReturn | 99 | ✅ |
| Equiv_FunctionWithParam | 10 | ✅ |
| Equiv_LogicalAnd_True | 1 | ✅ |
| Equiv_LogicalAnd_False | -1 | ✅ |
| Equiv_LogicalOr_True | 1 | ✅ |
| Equiv_LogicalOr_False | -1 | ✅ |
| Equiv_UnaryNot_True | 1 | ✅ |
| Equiv_UnaryNot_False | -1 | ✅ |
| Equiv_StructWrite | 5 | ✅ |
| Equiv_StructWrite_Sum | 30 | ✅ |
| Equiv_MultidimArray_2x3 | 36 | ✅ |
| Equiv_GlobalVar | 7 | ✅ |
| Equiv_IfMaybeTrue | 10 | ✅ |
| Equiv_IfMaybeNeutral | 0 | ✅ |
| Equiv_IfMaybeFalse | -10 | ✅ |
| Equiv_BreakContinue | 31 | ✅ |
| Equiv_NestedWhile | 9 | ✅ |
| Equiv_PreprocDefine | 42 | ✅ |
| Equiv_Fibonacci | 55 | ✅ |
| Equiv_RecursiveFact | 5040 | ✅ |
| Equiv_DoubleRecursion | 8 | ✅ |
| Equiv_NestedFunctionCalls_Compiler | 12 | ⚠️ [Ignore] |

---

## 6. Known Gaps & Limitations

| # | Area | Description | Severity |
|---|------|-------------|----------|
| 1 | **Nested function calls ABI** | `Equiv_NestedFunctionCalls_Compiler` returns 6 instead of 12 due to register loss in nested call ABI. Tracked with `[Ignore]`. | Medium |
| 2 | **Register windowing** | Planned architectural feature not yet integrated into execution model. No tests. | Future |
| 3 | **VLIW mode** | `T3VliwAssembler` exists but has no dedicated test suite. | Low |
| 4 | **T3-54 full coverage** | Only basic `Test_T3_54_Int128` and Word54 tests. No 54-trit compiler pipeline tests. | Low |
| 5 | **Stack overflow recovery** | `StackOverflowGuard_DetectsOverflow` tests detection but not recovery/mitigation. | Low |
| 6 | **Floating-point edge cases** | NaN, infinity, and denormalized values in T3Float not exhaustively tested. | Low |
| 7 | **LIMM offset correctness** | Single test (`Assemble_LIMM_OffsetCorrectness`); LIMM used extensively via compiler but not directly tested for all register/offset combinations. | Low |

---

## 7. Test Infrastructure

### Dump Generation (Debug Mode)

When `CompilerDebugConfig.EnableDumps = true`, each `TLangCompilerTests` test generates:
- `.ast.txt` — AST tree dump
- `.asm` — Generated assembly
- `.bin.txt` — Ternary binary (18-trit strings per word)
- `.trace.txt` — Instruction-by-instruction execution trace (on failure)
- `.final.state.txt` — Final register + memory state
- `.crash.state.txt` — State at crash (on exception)

Dumps written to `test_results/` directory.

### Test Runner

```
dotnet test T3Sharp.sln --configuration Debug
```

Run specific test suite:
```
dotnet test tests/T3Simulator.InOrder.Tests
```

Filter specific tests:
```
dotnet test tests/T3Simulator.InOrder.Tests --filter "Compile_Array"
```

---

## 8. Summary

| Metric | Value |
|--------|-------|
| **Total tests** | 412 |
| **Passed** | 412 (100%) |
| **Failed** | 0 |
| **Skipped** | 1 (known ABI limitation) |
| **Test files** | 23 across 4 projects |
| **ISA opcodes tested** | All 83+ opcodes (100%) |
| **FPU opcodes tested** | 17/17 (100%) |
| **T-lang constructs tested** | All major constructs |
| **Equivalence tests** | 35 (34 passed, 1 skipped) |
| **Known gaps** | 7 (see §6) |