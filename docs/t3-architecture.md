# T3 Processor Architecture — v2 (Research Prototype)

## Word Types

- **Word18**: 18 trits, stored as `long` (64 bits). Range: ±193,710,244
- **Word54**: 54 trits, stored as `Int128`. Range: ±2.9×10²⁵
- **T3Float**: 18-trit float (6 exponent + 12 mantissa). Bias = 182. Linear encoding: `value = exponent * 3^12 + mantissa`
- **T3Double**: 36-trit float (8 exponent + 28 mantissa). Bias = 3280.

## Register File (9 registers)

| Name | Trit val | Phys | FPU | Назначение | Caller-saved |
|------|----------|------|-----|-----------|--------------|
| RW | -4 | 0 | FW | Temporary | Yes |
| RX | -3 | 1 | FX | Temporary | Yes |
| RY | -2 | 2 | FY | Temporary | Yes |
| RZ | -1 | 3 | FZ | Frame pointer / Temporary | Yes |
| R0 | 0 | 4 | F0 | Temporary | Yes |
| R1 | +1 | 5 | F1 | Call temp | Yes |
| R2 | +2 | 6 | F2 | Return value | No |
| R3 | +3 | 7 | F3 | Temporary | Yes |
| R4 | +4 | 8 | F4 | Address register | Yes |

Special registers: SP (stack pointer), PC (program counter), Cond (1 trit: -1/0/+1), PR (9 trits = 3×3 predicate flags).

*Note: Register windowing is currently a planned architectural feature and not yet integrated into the execution model.*

## Instruction Format

```
[Pred (3)] [Opcode (6)] [Args (9)]
```

- R-type: Args = `[Op1(3)] [Op2(3)] [Op3(3)]`
- I-type: Args = `[Op1(3)] [Imm(6)]`
- J-type: Args = `[Reg(3)] [000000]`

Registers encoded by trit value (-4..+4). Phys index = trit + 4.

### Encoding Details

**Pred field** (positions 15-17, LSB-first): Raw unsigned, 0..13. If pred > 0, instruction executes only if PR[pred-1] == +1.

**Opcode field** (positions 9-14): Raw unsigned, 0..364.

**Args field** (positions 0-8): Raw unsigned, then sub-fields extracted and converted to balanced by subtracting offset:
- 3-trit sub-field: offset = 13, range ±13
- 6-trit sub-field (I-type imm): offset = 364, range ±364 (including LI)

**J-type**: Reg in Op1 position (3 trits). Op2 and Op3 are padding (6 trits, always 0). Imm is explicitly set to 0 by the decoder.

**LOADI/STOREI**: Use S-type encoding via `EncodeS(pred, opcode, op1, op2, imm3)` with 3-trit immediate field (±13 range). Decoder treats them as I-type with 3-trit imm field.

### Encoder (InstructionEncoder)

```csharp
long EncodeR(int pred, int opcode, int op1, int op2, int op3)
long EncodeI(int pred, int opcode, int op1, long imm)
long EncodeJ(int pred, int opcode, int reg)
long EncodeS(int pred, int opcode, int op1, int op2, long imm3)  // LOADI/STOREI
```

All signed operands are converted to unsigned via `ToUnsignedField(value, range, offset)`:
- `unsigned = value + offset` where `offset = (3^width - 1) / 2`

### Decoder (InstructionDecoder)

```csharp
DecodedInstruction Decode(Word18 word)
DecodedInstruction Decode(Word54 word)  // uses Word18.FromWrappedLong()
```

Decoding process:
1. Extract Pred: `ExtractRawField(word, 15, 3)` — raw unsigned
2. Extract Opcode: `ExtractRawField(word, 9, 6)` — raw unsigned
3. Extract Args: `ExtractRawField(word, 0, 9)` — raw unsigned
4. Based on type (R/I/J):
    - R-type: Extract Op1/Op2/Op3 from Args (3 trits each), convert to balanced: `value - 13`
    - I-type: Extract Op1 (3 trits) and Imm (6 trits) from Args, convert to balanced: Op1 -= 13, Imm -= 364
    - J-type: Extract Reg (3 trits) from Args, convert to balanced: Reg -= 13. Imm = 0
    - LOADI/STOREI: Extract Op1 (3 trits), Op2 (3 trits), Imm (3 trits), convert to balanced

## Memory

- 1M words (1,048,576) of Word18/Word54
- Stack grows downward, SP starts at MemSize-1
- PUSH: `SP--; Memory[SP] = value`
- POP: `value = Memory[SP]; SP++`
- CALL: `SP--; Memory[SP] = PC + 1; PC = target`
- RET: `PC = Memory[SP]; SP++`
- MMIO: CYCLE_LOW, CYCLE_HIGH, INST_COUNT, STALL_COUNT

## Execution Model (In-Order Prototype)

The current implementation is a functional interpreter rather than a cycle-accurate pipeline.

1. **Fetch**: `word = Memory[PC]`
2. **Decode**: extract Pred, Opcode, Args via `InstructionDecoder.Decode()`
3. **Predicate**: skip if predicate condition is not met (`GetPredicateFlag` checks PR trits 0,1,2)
4. **Execute**: switch on Opcode via `T3Alu` (integer) or `T3Fpu` (floating-point)
5. **PC advance** (unless branch/jump)

### Predication Details

- `GetPredicateFlag(predIndex)` reads `PR.GetTrit(predIndex - 1)`:
  - predIndex=1 → PR.GetTrit(0) (LSB, 3^0)
  - predIndex=2 → PR.GetTrit(1) (3^1)
  - predIndex=3 → PR.GetTrit(2) (3^2)
- If trit value == +1, predicate is true, instruction executes
- If trit value == 0 or -1, instruction is skipped (PC advances normally)

**Setting PR**:
- Predicate 1 = true → `PR = Word18.FromLong(1)` (3^0)
- Predicate 2 = true → `PR = Word18.FromLong(3)` (3^1)
- Predicate 3 = true → `PR = Word18.FromLong(9)` (3^2)

## T3Float Format

**Structure**: 6 trits exponent (biased by 182) + 12 trits mantissa = 18 trits total.

- Exponent range: ±364 (signed balanced)
- Mantissa range: ±88,573 (signed balanced)
- Value: `mantissa * 3^(exponent - 182)`
- Encoding: `value = exponent * 3^12 + mantissa` (direct linear arithmetic)

**ToWord18()**:
```csharp
long encoded = Exponent * (long)TernaryMath.Pow3(12) + Mantissa;
return Word18.FromLong(encoded);
```

**FromWord18()**:
```csharp
long pow12 = (long)TernaryMath.Pow3(12);
long raw = word.ToLong();
long exponent = raw / pow12;
long mantissa = raw % pow12;
return new T3Float(exponent, mantissa);
```

**CRITICAL**: Do NOT use string-based conversion (`ToTritString()`/`FromTritString()`) for T3Float encoding/decoding. The `ExtractBalancedTrit` function used by `ToTritString()` has a fundamental issue with carry propagation between the exponent and mantissa fields. The linear arithmetic encoding is correct because division and modulo operate on integer values, and carries in the balanced ternary representation do not affect integer division.

## FPU Operations

FPU registers (FW..F4) map to the same physical slots as integer registers (RW..R4) but store `T3Float` values.

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 100 | FADD | Fop1 = Fop2 + Fop3 |
| 101 | FSUB | Fop1 = Fop2 - Fop3 |
| 102 | FMUL | Fop1 = Fop2 * Fop3 |
| 103 | FDIV | Fop1 = Fop2 / Fop3 |
| 104 | FSQRT | Fop1 = sqrt(Fop2) |
| 105 | FABS | Fop1 = abs(Fop2) |
| 106 | FNEG | Fop1 = -Fop2 |
| 107 | FCMP | Cond = sign(Fop1 - Fop2) |
| 108 | FTOI | Rop1 = int(Fop2) |
| 109 | ITOF | Fop1 = float(Rop2) |
| 110 | FTOF | tfloat ↔ tdouble |
| 111 | FLW | Fop1 = mem[Rop2 + op3] |
| 112 | FSW | mem[Rop2 + op3] = Fop1 |
| 113 | FMOV | func:0=F→F,1=R→F,2=F→R |
| 114 | FCLASS | Classify Fop2 |
| 115 | FSWAP | Swap Fop1↔Fop2 |
| 116 | FZERO | Fop1 = 0.0 |

## LIMM (Large Immediate)

2-word instruction for loading values outside ±364 range.

```
Word 1: [Pred(3)] [Opcode=LIMM(5)] [Reg(3)] [000000]
Word 2: [data (18 trits)]
```

Processor: `Register[reg] = Memory[PC]; PC++`

## Compiler ABI v4 (T-lang)

### Register Model

| Register | Phys | Purpose |
|----------|------|---------|
| RW (0) | 0 | Temporary, arg 0 |
| RX (1) | 1 | Temporary, arg 1 |
| RY (2) | 2 | Temporary, arg 2 |
| RZ (3) | 3 | **Frame Pointer** (FP) |
| R0 (4) | 4 | Temporary, arg 3 |
| R1 (5) | 5 | Call temp register |
| R2 (6) | 6 | Return value (NOT saved/restored) |
| R3 (7) | 7 | Callee-saved |
| R4 (8) | 8 | Address register (callee-saved) |

### Prologue

```asm
PUSH RZ          ; save old FP
PUSH R3          ; callee-saved
PUSH R4          ; callee-saved
MOV RZ, SP       ; RZ = FP
SUBI SP, SP, N   ; allocate local frame (N words)
```

### Epilogue

```asm
ADDI SP, SP, N   ; deallocate local frame
POP R4
POP R3
POP RZ
RET
```

### Calling Convention

1. Caller pushes arguments onto stack
2. `LIMM R1, function` + `CALL R1`
3. Callee prologue: PUSH RZ/R3/R4, set FP, allocate locals
4. Callee saves register args 0..3 to local slots
5. Callee body
6. Return value in R2
7. Callee epilogue: deallocate locals, POP R4/R3/RZ, RET

### Register Allocation

Round-robin `AllocR()` / `FreeR()` allocator with reserved registers:
- R1 (phys 5) — call target address
- R2 (phys 6) — return value
- R4 (phys 8) — address register
- RZ (phys 3) — frame pointer

Available temp registers: RW(0), RX(1), RY(2), R0(4), R3(7). With only 5 temp registers, complex expressions use **PUSH/POP to stack** to protect live values from register reuse.

### FlatIdx — Multidimensional Array Indexing

For arrays declared as `tint a[D1][D2]...[Dn]`:

```
FlatIdx(row, col, ...) = (...rows) * D2 * D3 * ... * Dn + col * D3 * ... * Dn + ...
```

Generated code uses PUSH/POP to preserve accumulator across stride multiplications.

### Register Clobber Protection

The `EmitArrStore`, `EmitArrAccess`, `EmitMemStore`, and `EmitMemAccess` methods use **PUSH/POP** to protect value/index registers from being overwritten by the register allocator during address computation (FlatIdx, LabelAddr, Imm calls).

### Memory Layout

```
Stack (grows downward, SP starts at MemSize-1):
┌─────────────────────┐ ← initial SP (1048575)
│   ...               │
├─────────────────────┤
│  saved R4           │ ← RZ (FP points here after MOV RZ, SP)
├─────────────────────┤
│  saved R3           │
├─────────────────────┤
│  saved RZ (old FP)  │
├─────────────────────┤
│  return address     │
├─────────────────────┤
│  stack args (4+)    │
├─────────────────────┤
│  local variables    │ ← SP after frame allocation
└─────────────────────┘

Data section starts at address 300.
Code section starts at address 0.
```

## Debug Infrastructure

### Compiler Debug Config

`CompilerDebugConfig.EnableDumps` (in `src/T3Compiler/CompilerDebugConfig.cs`) controls dump generation:

- **AST dump** (`.ast.txt`): Serialized AST tree via `AstDumper.Dump()`
- **ASM dump** (`.asm`): Generated assembly code
- **Binary dump** (`.bin.txt`): Ternary binary representation (18-trit strings per word)
- **Final state dump** (`.final.state.txt`): Register and memory state after execution
- **Crash state dump** (`.crash.state.txt`): State on exception
- **Trace** (`.trace.txt`): Instruction-by-instruction execution log

Dumps are written to `test_results/` directory.

## Opcode Table

See [t3-isa-reference.md](t3-isa-reference.md) for full ISA.

## Key Source Files

| File | Description |
|------|-------------|
| `src/TritTypes/Word18.cs` | 18-trit word type with arithmetic, shifts, tritwise ops |
| `src/TritTypes/Word54.cs` | 54-trit word type |
| `src/TritTypes/T3Float.cs` | Floating-point type (18 trits) |
| `src/TritTypes/TernaryMath.cs` | Field extraction, balanced ternary utilities |
| `src/TritTypes/BalancedTernary.cs` | String conversion for balanced ternary |
| `src/T3Simulator.Common/InstructionEncoder.cs` | Instruction encoding |
| `src/T3Simulator.Common/InstructionDecoder.cs` | Instruction decoding |
| `src/T3Simulator.Common/T3Disassembler.cs` | Disassembler |
| `src/T3Simulator.Common/T3Alu.cs` | Integer ALU operations |
| `src/T3Simulator.Common/T3Fpu.cs` | FPU operations |
| `src/T3Simulator.Common/ProcessorBase.cs` | Base processor class |
| `src/T3Simulator.InOrder/T3InOrderProcessor.cs` | In-order processor implementation |
| `src/T3Assembler/T3InOrderAssembler.cs` | Assembler for InOrder |
| `src/T3Compiler/CodeGen/CodeGenerator.cs` | T-lang compiler code generator |
| `src/T3Compiler/CompilerDebugConfig.cs` | Debug dump configuration |
| `src/T3Compiler/CodeGen/AstDumper.cs` | AST serialization for debugging |
| `src/T3Interpreter/T3Interpreter.cs` | T-lang interpreter (reference implementation) |
| `src/TritTypes/T3ConversionService.cs` | Number format conversion service |

## Test Results (v2.1)

| Test Project | Passed | Failed | Skipped | Description |
|---|---|---|---|---|
| TritTypes.Tests | 126 | 0 | 0 | Word18/Word54, T3Float, balanced ternary, TScii |
| T3Simulator.Common.Tests | 71 | 0 | 0 | Assembler/disassembler roundtrip, ALU, FPU, Memory |
| T3Simulator.InOrder.Tests | 147 | 0 | 0 | ISA instructions, FPU, T-lang compiler, ABI v3/v4 |
| T3Interpreter.Tests | 68 | 0 | 1 | Interpreter + equivalence tests |
| **Total** | **412** | **0** | **1** | |

> **Note**: `Equiv_NestedFunctionCalls_Compiler` is `[Ignore]`d — tracked for future ABI fix (nested calls lose registers).

See [t3-test-coverage.md](t3-test-coverage.md) for detailed test coverage analysis.