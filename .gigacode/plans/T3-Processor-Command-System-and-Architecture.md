# System of Commands and Architecture of the T3 Ternary Processor

## 1. Introduction

The T3 (Ternary) processor is a ternary (balanced ternary) computer architecture simulator implemented in C#. The processor uses balanced ternary digits (trits) with values -1, 0, and +1, represented as '-', '0', and '+' respectively.

**Key Features:**
- Two word sizes: **T3-18** (18 trits) and **T3-54** (54 trits for VLIW)
- Register windowing for efficient subroutine calls
- Predicate-based conditional execution
- VLIW (Very Long Instruction Word) support for parallel execution
- Word-addressable memory (1M words)
- Device I/O with stall handling

---

## 2. Architecture Overview

### 2.1 Project Structure

```
T3Sharp.sln
├── src/
│   ├── TritTypes/               # Base ternary types (Trit, Tryte, Word18, Word54)
│   ├── T3Simulator.Common/      # Interfaces, base classes, common infrastructure
│   ├── T3Simulator.InOrder/     # Sequential processor (T3-18, T3-54)
│   └── T3Simulator.VLIW/        # VLIW processor (T3-54 only)
└── tests/
    ├── TritTypes.Tests/
    ├── T3Simulator.Common.Tests/
    ├── T3Simulator.InOrder.Tests/
    └── T3Simulator.VLIW.Tests/
```

### 2.2 Core Abstraction

#### IT3Processor<TWord> Interface

The main interface for all T3 processor implementations:

```csharp
void LoadProgram(IEnumerable<TWord> code);
void Reset();
bool Step();              // Execute single cycle
void Run();               // Execute until HALT

long CycleCount { get; }
long InstructionCount { get; }
long StallCount { get; }

void SetInputDevice(long port, IDevice<TWord> dev);
void SetOutputDevice(long port, IDevice<TWord> dev);

ProcessorState<TWord> GetState();
TWord ReadWord(long address);
```

#### ProcessorBase<TWord>

Abstract base class providing common functionality:
- 27 physical registers (R0-R26)
- Register windowing (9 logical registers mapped to 27 physical)
- Memory management (1M words)
- Device manager for I/O
- Cycle/instruction/stall counters

---

## 3. Processor State

### 3.1 Registers

| Register | Index | Description |
|----------|-------|-------------|
| **RW** | 0 | Working register A |
| **RX** | 1 | Working register B |
| **RY** | 2 | Working register C |
| **RZ** | 3 | Working register D |
| **R0** | 4 | General-purpose register 0 |
| **R1** | 5 | General-purpose register 1 |
| **R2** | 6 | General-purpose register 2 |
| **R3** | 7 | General-purpose register 3 |
| **R4** | 8 | General-purpose register 4 |
| **SP** | - | Stack pointer |
| **PC** | - | Program counter |
| **WP** | - | Window pointer (0-26) |
| **Cond** | - | Comparison result (-1, 0, 1) |
| **PR** | - | Predicate register (9 trits for T3-18) |

### 3.2 Register Window Mapping

The processor uses a rotating register window:
- **27 physical registers**: R0-R26
- **9 logical registers**: RW, RX, RY, RZ, R0-R4

**Mapping Formula:**
```
physical_index = (WP + logical_index) % 27
```

**CALL Window Change:**
```
WP_new = (WP_old + 23) % 27
```
(Old A (logical 0) becomes New E (logical 4))

### 3.3 Special Registers

#### Condition Register (Cond)
- Stores comparison result: -1 (less), 0 (equal), 1 (greater)
- Used by conditional jump instructions (JE, JNE, JL, JG)

#### Predicate Register (PR)
- **T3-18**: 9 trits (3 flags × 3 trits each)
- **T3-54**: 27 trits (9 flags × 3 trits each)
- Each flag can be -1, 0, or +1
- Predicate index 0 = unconditional execution
- Predicate index 1-3 (T3-18) or 1-9 (T3-54) = conditional execution

---

## 4. Memory Architecture

### 4.1 Memory Organization

- **Word-addressable**: Each address references one complete word
- **Memory size**: 1,048,576 words (1M)
- **Word sizes**: 18 trits (T3-18) or 54 trits (T3-54)
- **Address range**: 0 to 1,048,575 (18-trit address space)

### 4.2 Memory-Mapped I/O

| Address (hex) | Address (decimal) | Description |
|---------------|-------------------|-------------|
| 0xFFFFFF00 | 16,777,215 | CYCLE_LOW (lower 32 bits) |
| 0xFFFFFF01 | 16,777,216 | CYCLE_HIGH (upper 32 bits) |
| 0xFFFFFF02 | 16,777,217 | INST_COUNT (instruction count) |
| 0xFFFFFF03 | 16,777,218 | STALL_COUNT (stall count) |
| 0xFFFFFF10 | 16,777,232 | TIMER_CTRL |
| 0xFFFFFF11 | 16,777,233 | TIMER_CMP |

**Note**: Writing to CYCLE_LOW resets all counters.

### 4.3 Stack Organization

The stack grows downward from high memory:

```
High Memory (1M-1)
    ┌─────────────┐
    │   Stack     │ ← SP points here (grows down)
    │   Data      │
    └─────────────┘
    └─────────────┘
    └─────────────┘
    └─────────────┘
Low Memory (0)
```

**Stack Operations:**
- `PUSH`: Decrement SP, write value to mem[SP]
- `POP`: Read value from mem[SP], increment SP

**CALL Convention:**
```
SP -= 2
mem[SP] = return_address
mem[SP+1] = saved_WP
WP = (WP + 23) % 27
PC = target_address
```

**RET Convention:**
```
PC = mem[SP]
WP = mem[SP+1]
SP += 2
```

---

## 5. Instruction Encoding

### 5.1 Instruction Format (18-trit word)

All instructions are encoded in exactly one 18-trit word.

#### R-Type (Register-Register) Format

```
[ opcode+pred (6) | op1 (3) | op2 (3) | op3 (3) | reserve (3) ]
   0-5           6-8      9-11     12-14     15-17
```

#### I-Type (Immediate) Format

```
[ opcode+pred (6) | op1 (3) | op2 (3) | imm6 (6) ]
   0-5           6-8      9-11     12-17
```

### 5.2 Opcode Encoding

The opcode field combines the base opcode and predicate index:

```
full_opcode = base_opcode + pred_index * 28
```

**Decoding:**
```
pred_index = full_opcode / 28
base_opcode = full_opcode % 28
```

### 5.3 Opcode Ranges

| Range | Purpose |
|-------|---------|
| 0-27 | Base R-type instructions |
| 41-44 | I/O instructions (IN, OUT, INI, OUTI) |
| 63 | NOP (reserved) |
| 65-81 | I-type variants (LOADI...CMPI) |
| 28-30 | VLIW speculation (SPEK, COMMIT, ROLLBACK) |
| 31-40 | VLIW SIMD operations (VADD3...VSHR3) |
| 64, 69, 82-91 | Reserved for future extensions |

---

## 6. Instruction Set Reference

### 6.1 Data Movement Instructions

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 0 | `HALT` | - | Stop processor | 1 |
| 1 | `LOAD` | R | `op1 = mem[op2]` | 2 |
| 2 | `STORE` | R | `mem[op2] = op1` | 2 |
| 3 | `MOV` | R | `op1 = op2` | 1 |
| 4 | `LI` | I | `op1 = imm6` | 1 |
| 5 | `LIMM` | R | `op1 = mem[PC]; PC++` | 2 |
| 41 | `IN` | R | `op1 = port[op2]` | 2 |
| 42 | `OUT` | R | `port[op2] = op1` | 2 |
| 43 | `INI` | I | `op1 = port[imm6]` | 2 |
| 44 | `OUTI` | I | `port[imm6] = op1` | 2 |

### 6.2 Arithmetic Instructions

| Opcode | Mnemonic | Format | Description | T3-18 | T3-54 |
|--------|----------|--------|-------------|-------|-------|
| 6 | `ADD` | R | `op1 = op2 + op3` | 1 | 1 |
| 7 | `SUB` | R | `op1 = op2 - op3` | 1 | 1 |
| 8 | `MUL` | R | `op1 = op2 * op3` | 5 | 8 |
| 9 | `DIV` | R | `op1 = op2 / op3` | 10 | 15 |
| 10 | `MOD` | R | `op1 = op2 % op3` | 10 | 15 |
| 11 | `NEG` | R | `op1 = -op2` | 1 | 1 |

### 6.3 I-Type Arithmetic Instructions

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 70 | `ADDI` | `op1 = op2 + imm6` |
| 71 | `SUBI` | `op1 = op2 - imm6` |
| 72 | `MULI` | `op1 = op2 * imm6` |
| 73 | `DIVI` | `op1 = op2 / imm6` |
| 74 | `MODI` | `op1 = op2 % imm6` |
| 75 | `NEGI` | `op1 = -imm6` |

### 6.4 Logical Instructions (Tritwise)

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 12 | `TRITAND` | R | `op1 = min(op2, op3)` | 1 |
| 13 | `TRITOR` | R | `op1 = max(op2, op3)` | 1 |
| 14 | `TRITXOR` | R | `op1 = (op2 + op3) mod 3` | 1 |

### 6.5 I-Type Logical Instructions

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 76 | `TRITANDI` | `op1 = op2 & imm6` |
| 77 | `TRITORI` | `op1 = op2 | imm6` |
| 78 | `TRITXORI` | `op1 = op2 ^ imm6` |

### 6.6 Shift Instructions

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 15 | `SHL` | R | `op1 = op2 << op3` (×3^op3) | 1 |
| 16 | `SHR` | R | `op1 = op2 >> op3` (÷3^op3) | 1 |

### 6.7 I-Type Shift Instructions

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 79 | `SHLI` | `op1 = op2 << imm6` |
| 80 | `SHRI` | `op1 = op2 >> imm6` |

### 6.8 Comparison Instructions

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 17 | `CMP` | R | `Cond = sign(op1 - op2)` | 1 |

### 6.9 I-Type Comparison Instructions

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 81 | `CMPI` | `Cond = sign(op1 - imm6)` |

### 6.10 Control Flow Instructions

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 18 | `JMP` | R | `PC = op1` | 1 |
| 19 | `JE` | R | `if (Cond == 0) PC = op1` | 1/2 |
| 20 | `JNE` | R | `if (Cond != 0) PC = op1` | 1/2 |
| 21 | `JL` | R | `if (Cond < 0) PC = op1` | 1/2 |
| 22 | `JG` | R | `if (Cond > 0) PC = op1` | 1/2 |
| 23 | `JM` | R | `if (Cond == 0) PC = op1` | 1/2 |

**Note**: JE/JNE/JL/JG/JM take 1 cycle if condition is false (not taken), 2 cycles if true (taken).

### 6.11 Subroutine Instructions

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 24 | `CALL` | R | Save return address and WP, jump | 2 |
| 25 | `RET` | R | Restore PC and WP from stack | 2 |

### 6.12 Stack Instructions

| Opcode | Mnemonic | Format | Description | Cycles |
|--------|----------|--------|-------------|--------|
| 26 | `PUSH` | R | `SP--; mem[SP] = op1` | 2 |
| 27 | `POP` | R | `op1 = mem[SP]; SP++` | 2 |

### 6.13 VLIW-Specific Instructions

| Opcode | Mnemonic | Description |
|--------|----------|-------------|
| 28 | `SPEK` | Begin speculation (save registers) |
| 29 | `COMMIT` | Commit speculation (apply buffered writes) |
| 30 | `ROLLBACK` | Rollback speculation (restore registers) |
| 31 | `VADD3` | SIMD add (3 segments) |
| 32 | `VSUB3` | SIMD subtract (3 segments) |
| 33 | `VMUL3` | SIMD multiply (3 segments) |
| 34 | `VDOT3` | SIMD dot product |
| 35 | `VCMP` | SIMD compare → PR flags |
| 36 | `VTRITAND3` | SIMD trit AND |
| 37 | `VTRITOR3` | SIMD trit OR |
| 38 | `VTRITXOR3` | SIMD trit XOR |
| 39 | `VSHL3` | SIMD shift left |
| 40 | `VSHR3` | SIMD shift right |

---

## 7. Predicate System

### 7.1 Predicate Encoding

Predicates enable conditional execution without branching:

| Predicate Index | Behavior |
|-----------------|----------|
| 0 | Unconditional (always execute) |
| 1-3 (T3-18) | Conditional on PR[p-1] == +1 |
| 1-9 (T3-54) | Conditional on PR[p-1] == +1 |

### 7.2 Predicate Register Layout

**T3-18 (9 trits total):**
```
PR bits: [p2(7-9) p1(4-6) p0(1-3)] reserve(0)
         [trit2 trit1 trit0] for each flag
```

**T3-54 (27 trits total):**
```
PR bits: [p8(26-28) ... p0(1-3)] reserve(0)
```

### 7.3 Predicate Evaluation

- If predicate index = 0: instruction always executes
- If predicate index > 0: instruction executes only if corresponding PR flag = +1
- If flag = -1 or 0: instruction is skipped (acts as NOP)

---

## 8. VLIW Architecture

### 8.1 Bundle Format

A VLIW bundle is a 54-trit word containing 3 independent 18-trit slots:

```
[ Slot0 (18) | Slot1 (18) | Slot2 (18) ]
   0-17       18-35        36-53
```

Each slot has the same 18-trit format as T3-18 instructions.

### 8.2 Execution Rules

1. **No register write conflicts**: Two slots cannot write to the same register
2. **Single memory operation**: Only one LOAD/STORE per bundle
3. **Single branch**: Only one branch instruction per bundle
4. **Parallel execution**: All valid slots execute simultaneously

### 8.3 Priority Rules

- **Memory conflicts**: Slot 0 > Slot 1 > Slot 2
- **Branch conflicts**: Slot 0 > Slot 1 > Slot 2
- **Register conflicts**: Detected before execution, error if conflict exists

### 8.4 SIMD Operations

VLIW SIMD instructions process 3 segments independently:

- **Word54** = 3 segments of 18 trits each
- Operations apply to each segment in parallel
- Example: `VADD3` adds 3 pairs of 18-trit values simultaneously

---

## 9. Execution Models

### 9.1 In-Order Processor Pipeline

```
Fetch → Decode → Predicate Eval → Execute → Update
```

**Stages:**
1. **Fetch**: Read instruction from mem[PC]
2. **Decode**: Parse opcode, operands, predicate
3. **Predicate Eval**: Check if instruction should execute
4. **Execute**: Perform operation
5. **Update**: Increment PC and counters

### 9.2 VLIW Processor Pipeline

```
Fetch Bundle → Decode 3 Slots → Conflict Check → Execute → Update
```

**Additional Stages:**
- **Conflict Detection**: Check register/memory/branch conflicts
- **Parallel Execution**: Execute all valid slots simultaneously

### 9.3 Speculative Execution (VLIW)

```
SPEK → Save Registers → Buffer Writes → Execute → COMMIT/ROLLBACK
```

**Operations:**
- **SPEK**: Save register state, enable write buffering
- **COMMIT**: Apply buffered writes, discard shadow state
- **ROLLBACK**: Restore saved state, discard buffered writes

---

## 10. I/O System

### 10.1 Device Interface

```csharp
public interface IDevice<TWord>
{
    TWord Read();
    void Write(TWord value);
    bool DataReady { get; }
}
```

### 10.2 Device Manager

| Port | Device |
|------|--------|
| 0 | stdout (output) |
| 1 | stdin (input) |
| 2 | stderr (output) |
| 3-7 | User-defined |
| 0x10-0x1F | Timer/MMIO |

### 10.3 Device Stall

When reading from an unready device:
1. `DeviceStallException` is thrown
2. Instruction is retried on next cycle
3. Stall counter is incremented

---

## 11. Performance Considerations

### 11.1 Cycle Counts Summary

| Operation | T3-18 | T3-54 |
|-----------|-------|-------|
| ADD/SUB | 1 | 1 |
| MUL | 5 | 8 |
| DIV/MOD | 10 | 15 |
| LOAD/STORE | 2 | 2 |
| Branch (not taken) | 1 | 1 |
| Branch (taken) | 2 | 2 |
| CALL/RET | 2 | 2 |
| PUSH/POP | 2 | 2 |
| VLIW bundle | 1 | 1 |

### 11.2 Optimization Tips

1. **Use VLIW for parallel workloads**: SIMD processes 3 segments simultaneously
2. **Minimize branches**: Predicate execution avoids branch penalties
3. **Leverage register windowing**: Reduces memory traffic for function calls
4. **Batch I/O operations**: Device access has 2-cycle latency
"\n---\n\n## 12. Number Base Encodings\n\nThe T3 processor supports three different encoding formats for representing ternary values:\n\n### 12.1 Ternary (Base-3) Encoding\n\n**Prefix**: `0t`\n\nThe most direct representation using the ternary digits themselves:\n- **Trit characters**: `-` (minus one), `0` (zero), `+` (plus one)\n- **Tryte (6 trits)**: 6 characters (e.g., `0t+0-0+0`)\n- **Word18 (18 trits)**: 18 characters\n- **Word54 (54 trits)**: 54 characters\n\n**Format**: `0t[trits]`\n\n**Example**:\n```csharp\n// Tryte: +0-0+0 = 1*3^5 + 0*3^4 + (-1)*3^3 + 0*3^2 + 1*3^1 + 0*3^0\n//        = 243 + 0 - 27 + 0 + 3 + 0 = 219\n0t+0-0+0  // Value: 219\n\n// Word18 example\n0t+0-0+0-0+0-0+0-0+0-0+0-0+0-0+0-0+0\n```\n\n**Conversion**:\n```csharp\n// From ternary string to value\npublic static long ParseToLong(string s)\n{\n    long value = 0;\n    long power = 1;\n    for (int i = s.Length - 1; i >= 0; i--)\n    {\n        value += s[i] switch\n        {\n            '-' => -power,\n            '0' => 0,\n            '+' => power,\n            _ => throw new FormatException($\"Invalid trit character: '{s[i]}'\")\n        };\n        power *= 3;\n    }\n    return value;\n}\n```\n\n### 12.2 Ninary (Base-9) Encoding\n\n**Prefix**: `0n`\n\nA compact encoding where each digit represents 2 trits (since 3² = 9):\n- **Digits**: `W, X, Y, Z, 0, 1, 2, 3, 4` (representing values 0-8)\n- **Tryte (6 trits)**: 3 digits (e.g., `0nY12`)\n- **Efficiency**: 50% reduction in character count vs ternary\n\n**Trit-to-Digit Mapping**:\n\n| Trit Pair | Value | Digit |\n|-----------|-------|-------|\n| `00` | 0 | `W` |\n| `0+` | 1 | `X` |\n| `0-` | 2 | `Y` |\n| `+0` | 3 | `Z` |\n| `++` | 4 | `0` |\n| `+-` | 5 | `1` |\n| `-0` | 6 | `2` |\n| `-+` | 7 | `3` |\n| `--` | 8 | `4` |\n\n**Format**: `0n[digits]`\n\n**Example**:\n```csharp\n// Tryte: +0-0+0\n// Pairs: +0 | -0 | +0\n// Values: 3 | 6 | 3\n// Digits: Z | 2 | Z\n0nZ2Z  // Same value as 0t+0-0+0\n\n// Example with all digits\n0nY12  // Y=2, 1=5, 2=6 → 2*81 + 5*9 + 6 = 219\n```\n\n### 12.3 Tryx (Base-27) Encoding\n\n**Prefix**: `0y`\n\nA more compact encoding where each digit represents 3 trits (since 3³ = 27):\n- **Digits**: `N, O, P, Q, R, S, T, U, V, W, X, Y, Z, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, A, B, C, D`\n- **Tryte (6 trits)**: 2 digits (e.g., `0y2B`)\n- **Efficiency**: 67% reduction in character count vs ternary\n\n**Trit-to-Digit Mapping**:\n\n| Trit Triplet | Value | Digit |\n|--------------|-------|-------|\n| `000` | 0 | `N` |\n| `00+` | 1 | `O` |\n| `00-` | 2 | `P` |\n| `0+0` | 3 | `Q` |\n| `0++` | 4 | `R` |\n| `0+-` | 5 | `S` |\n| `0-0` | 6 | `T` |\n| `0-+` | 7 | `U` |\n| `0--` | 8 | `V` |\n| `+00` | 9 | `W` |\n| `+0+` | 10 | `X` |\n| `+0-` | 11 | `Y` |\n| `++0` | 12 | `Z` |\n| `+++` | 13 | `0` |\n| `++-` | 14 | `1` |\n| `+-0` | 15 | `2` |\n| `+-+` | 16 | `3` |\n| `+--` | 17 | `4` |\n| `-00` | 18 | `5` |\n| `-0+` | 19 | `6` |\n| `-0-` | 20 | `7` |\n| `-+0` | 21 | `8` |\n| `-++` | 22 | `9` |\n| `-+-` | 23 | `A` |\n| `--0` | 24 | `B` |\n| `--+` | 25 | `C` |\n| `---` | 26 | `D` |\n\n**Format**: `0y[digits]`\n\n**Example**:\n```csharp\n// Tryte: +0-0+0\n// Triplets: +0- | 0+0\n// Values: 11 | 3\n// Digits: Y | Q\n0yYQ  // Same value as 0t+0-0+0\n\n// Example with all digits\n0y2B  // 2=11, B=24 → 11*27 + 24 = 321\n```\n\n### 12.4 Encoding Comparison\n\n| Format | Tryte Length | Efficiency | Use Case |\n|--------|--------------|------------|----------|\n| **Ternary (0t)** | 6 chars | 100% | Human-readable, educational |\n| **Ninary (0n)** | 3 chars | 50% | Compact display, assembly |\n| **Tryx (0y)** | 2 chars | 33% | Maximum compactness |\n\n### 12.5 Conversion Functions\n\n```csharp\n// T-SCII class provides encoding/decoding utilities\n\n// Convert value to different formats\npublic static string ToTritString(Int128 value)\n{\n    int u = (int)value + Offset;\n    char[] trits = new char[6];\n    for (int i = 5; i >= 0; i--)\n    {\n        int rem = u % 3;\n        trits[i] = rem == 0 ? '-' : (rem == 1 ? '0' : '+');\n        u /= 3;\n    }\n    return \"0t\" + new string(trits);\n}\n\npublic static string ToNinary(Int128 value)\n{\n    int u = (int)value + Offset;\n    int d1 = u / 81, d2 = (u / 9) % 9, d3 = u % 9;\n    char[] digits = { 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4' };\n    return $\"0n{digits[d1]}{digits[d2]}{digits[d3]}\";\n}\n\npublic static string ToTryx(Int128 value)\n{\n    int u = (int)value + Offset;\n    int h = u / 27, l = u % 27;\n    char[] digits = \"NOPQRSTUVWXYZ0123456789ABCD\".ToCharArray();\n    return $\"0y{digits[h]}{digits[l]}\";\n}\n\n// Parse literal from any format\npublic static int ParseLiteral(string literal)\n{\n    literal = literal.Trim();\n    \n    if (literal.StartsWith(\"0t\"))\n    {\n        // Parse ternary: 0t+0-0+0\n        string s = literal[2..].Replace(\"_\", \"\");\n        if (s.Length != 6) throw new FormatException(\"Ternary literal must have 6 trits\");\n        int u = 0;\n        foreach (char c in s)\n        {\n            u = u * 3 + (c == '-' ? 0 : c == '0' ? 1 : c == '+' ? 2 : throw new FormatException(...));\n        }\n        return u - Offset;\n    }\n    else if (literal.StartsWith(\"0n\"))\n    {\n        // Parse ninary: 0nY12\n        string s = literal[2..].ToUpperInvariant();\n        if (s.Length != 3) throw new FormatException(\"Ninary literal must have 3 digits\");\n        int u = 0;\n        foreach (char c in s)\n        {\n            int idx = \"WXYZ01234\".IndexOf(c);\n            if (idx < 0) throw new FormatException($\"Invalid ninary digit '{c}'\");\n            u = u * 9 + idx;\n        }\n        return u - Offset;\n    }\n    else if (literal.StartsWith(\"0y\"))\n    {\n        // Parse tryx: 0y2B\n        string s = literal[2..].ToUpperInvariant();\n        if (s.Length != 2) throw new FormatException(\"Tryx literal must have 2 digits\");\n        int u = 0;\n        foreach (char c in s)\n        {\n            int idx = \"NOPQRSTUVWXYZ0123456789ABCD\".IndexOf(c);\n            if (idx < 0) throw new FormatException($\"Invalid tryx digit '{c}'\");\n            u = u * 27 + idx;\n        }\n        return u - Offset;\n    }\n    else if (literal.EndsWith(\"t\"))\n    {\n        // Parse decimal: 123t\n        if (!int.TryParse(literal[..^1], out int v) || v < -364 || v > 364)\n            throw new FormatException(\"Decimal literal must be -364 to 364\");\n        return v;\n    }\n    \n    throw new FormatException(\"Unknown T-SCII literal format\");\n}\n```\n\n### 12.6 Practical Examples\n\n```csharp\n// All representations of the same value (219):\n0t+0-0+0   // Ternary - direct trit representation\n0nZ2Z      // Ninary - 3 characters\n0yYQ       // Tryx - 2 characters\n\n// All representations of value 0:\n0t000000   // Ternary\n0nWWW      // Ninary\n0yNN       // Tryx\n\n// All representations of value 364 (maximum for tryte):\n0t++++++   // Ternary\n0n444      // Ninary\n0yDD       // Tryx\n\n// All representations of value -364 (minimum for tryte):\n0t-------  // Ternary\n0n000      // Ninary (0 in ninary = value 0, but offset applies)\n0yNN       // Tryx (same as 0, offset handling)\n```\n\n### 12.7 Usage in Assembly\n\nIn assembly language, you can use any of the three formats:\n\n```asm\n; Using ternary (most explicit)\nLI RW, 0t+0-0+0\n\n; Using ninary (compact)\nLI RW, 0nZ2Z\n\n; Using tryx (most compact)\nLI RW, 0yYQ\n\n; Using decimal\nLI RW, 219t\n```\n\nAll three formats represent the same value and are converted to the internal word representation by the assembler.\n\n---\n\n## 13. Usage Examples"}

### 12.1 Basic Arithmetic

```asm
; Calculate (10 + 20) * 3
LI RW, 10
LI RX, 20
ADD RY, RW, RX    ; RY = 30
LI RZ, 3
MUL R0, RY, RZ    ; R0 = 90
HALT
```

### 12.2 Predicate-Based Execution

```asm
; Conditional increment
MOV RW, RX        ; RW = RX
TRITOR RW, RW, +1 ; RW = RW + 1 if predicate true
; Only executes if PR[0] == +1
```

### 12.3 VLIW Parallel Execution

```asm
; Process 3 data elements in parallel
; Slot 0: LOAD R0, [addr0]
; Slot 1: LOAD R1, [addr1]
; Slot 2: LOAD R2, [addr2]
; All execute simultaneously in 1 cycle
```

---

## 13. References

- **Project Repository**: e:\Projects\t3_sharp
- **Core Implementation**: `src/T3Simulator.*`
- **Base Types**: `src/TritTypes`
- **Tests**: `tests/T3Simulator.*.Tests`
- **Implementation Plan**: `.gigacode/plans/t3-processor-implementation-plan.md`
- **Opcode Specification**: `.gigacode/plans/opcodes.md`

---

## 14. Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-06-08 | Initial documentation |

---

*This documentation is generated from the T3 processor implementation and is provided for educational and research purposes.*