# T3 Processor Architecture — v2 (Research Prototype)

## Word Types

- **Word18**: 18 trits, stored as `int` (32 bits). Range: ±193,710,244
- **Word54**: 54 trits, stored as `Int128`. Range: ±2.9×10²⁵
- **T3Float**: 18-trit float (6 exponent + 12 mantissa). Bias = 182. Linear encoding: `value = exponent * 3^12 + mantissa`
- **T3Double**: 36-trit float (8 exponent + 28 mantissa). Bias = 3280.

## Register File (9 registers)

| Name | Trit val | Phys | FPU | Назначение | Caller-saved |
|------|----------|------|-----|-----------|--------------|
| RW | -4 | 0 | FW | Temporary | Yes |
| RX | -3 | 1 | FX | Temporary | Yes |
| RY | -2 | 2 | FY | Temporary | Yes |
| RZ | -1 | 3 | FZ | Temporary | Yes |
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

### Encoder (InstructionEncoder)

```csharp
long EncodeR(int pred, int opcode, int op1, int op2, int op3)
long EncodeI(int pred, int opcode, int op1, long imm)
long EncodeJ(int pred, int opcode, int reg)
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
   - R-type: Extract Op1/Op2/Op3 from Args (3 trits each), convert to balanced: `value - 4`
   - I-type: Extract Op1 (3 trits) and Imm (6 trits) from Args, convert to balanced: Op1 -= 4, Imm -= 13
   - J-type: Extract Reg (3 trits) from Args, convert to balanced: Reg -= 4. Imm = 0

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

## Compiler ABI (T-lang)

### Calling Convention

**Caller**:
1. PUSH caller-saved registers (RW, RX, RY, RZ, R0, R1, R3, R4)
2. PUSH arguments in reverse order
3. LIMM R1, function_address
4. CALL R1
5. POP restore caller-saved registers
6. MOV result_reg, R2

**Callee (prologue)**:
1. Function label
2. If arguments exist:
   - POP return address into temporary register
   - POP arguments and store in local memory
   - PUSH return address back to stack
3. PUSH R4, R3, R1, R0, RZ, RY, RX, RW (all except R2)

**Callee (epilogue)**:
1. POP RW, RX, RY, RZ, R0, R1, R3, R4
2. RET

**return statement**: Generates LIMM + JMP to epilogue label (not RET directly).

**R2 (return value)**: NOT saved/restored in prologue/epilogue. Caller copies value from R2 after return.

### Register Allocation

Round-robin allocator skips:
- R1 (phys 5) — reserved for call target address
- R2 (phys 6) — reserved for return value
- R4 (phys 8) — reserved for address register

### Memory Layout

```
Stack (grows downward, SP starts at MemSize-1):
┌─────────────────────┐ ← initial SP (1048575)
│   ...               │
├─────────────────────┤
│  return address     │ ← SP after CALL
├─────────────────────┤
│  saved registers    │ ← SP after prologue PUSH
├─────────────────────┤
│  local variables    │ ← SP after frame allocation
└─────────────────────┘

Data section starts at address 300.
Code section starts at address 0.
```

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
| `src/TritTypes/T3ConversionService.cs` | Number format conversion service |

## Test Results (v2.0)

| Test Project | Passed | Failed |
|---|---|---|
| TritTypes.Tests | 123 | 0 |
| T3Simulator.Common.Tests | 71 | 0 |
| T3Simulator.InOrder.Tests | 79 | 0 |
| **Total** | **273** | **0** |