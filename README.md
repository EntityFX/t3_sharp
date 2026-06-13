# T3Sharp — Ternary Computer Simulator Suite

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Ternary](https://img.shields.io/badge/ternary-balanced-red.svg)](https://en.wikipedia.org/wiki/Balanced_ternary)

A comprehensive **.NET-based simulator suite** for ternary processors implementing a **balanced ternary** instruction set architecture (ISA) with multiple microarchitectures.

> **Документация на русском языке:** [README.ru.md](README.ru.md)

---

## Table of Contents

- [T3Sharp — Ternary Computer Simulator Suite](#t3sharp--ternary-computer-simulator-suite)
  - [Table of Contents](#table-of-contents)
  - [What is Balanced Ternary?](#what-is-balanced-ternary)
  - [Architecture Overview](#architecture-overview)
    - [Key Features](#key-features)
  - [Quick Start](#quick-start)
    - [Prerequisites](#prerequisites)
    - [Build](#build)
    - [Run the CLI Simulator](#run-the-cli-simulator)
    - [Run Tests](#run-tests)
  - [Processor Configurations](#processor-configurations)
    - [Word Types](#word-types)
    - [Register File](#register-file)
    - [Memory](#memory)
  - [Instruction Set](#instruction-set)
    - [Instruction Groups](#instruction-groups)
    - [Instruction Formats](#instruction-formats)
    - [Example Assembly](#example-assembly)
  - [Microarchitectures](#microarchitectures)
    - [In‑Order Processor](#inorder-processor)
    - [VLIW Processor (T3‑54 only)](#vliw-processor-t354-only)
  - [Floating-Point Unit (FPU)](#floating-point-unit-fpu)
  - [T‑SCII Character Encoding](#tscii-character-encoding)
    - [Output Format Examples](#output-format-examples)
  - [Input \& Output Formats](#input--output-formats)
  - [CLI Commands](#cli-commands)
  - [Project Structure](#project-structure)
  - [Building \& Testing](#building--testing)
  - [Documentation](#documentation)
  - [Contributing](#contributing)
    - [Adding a New Processor](#adding-a-new-processor)
  - [License \& Acknowledgments](#license--acknowledgments)
    - [Inspiration](#inspiration)
    - [Built With](#built-with)

---

## What is Balanced Ternary?

**Balanced ternary** is a numeral system that uses three digits: **−1**, **0**, and **+1** (represented as `-`, `0`, `+`). Unlike binary (base 2) or standard ternary (0, 1, 2), balanced ternary:

- Represents negative numbers **naturally** — no separate sign bit needed
- Symmetric range around zero: an *n*-trit number spans from −(3ⁿ−1)/2 to +(3ⁿ−1)/2
- Was used in the historic **Setun computer** (Moscow State University, 1958)
- Provides elegant arithmetic: negation is simply flipping each trit (`-` ↔ `+`)

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                      T3 Processor Family                     │
├───────────────────┬──────────────────────────────────────────┤
│     T3‑18         │              T3‑54                       │
│  18‑trit words    │        54‑trit words                     │ 
│  3 trytes/word    │      9 trytes/word                       │
│  9 GP registers   │    9 GP registers (+ windowing)          │
│  In‑Order only    │  In‑Order + VLIW + SIMD + Speculation    │
│  Range: ±193M     │  Range: ±2.9×10²⁵                        │
└───────────────────┴──────────────────────────────────────────┘
```

### Key Features

| Feature | Description |
|---------|-------------|
| **Unified ISA** | 28 base instructions + I/O + FPU + VLIW extensions |
| **Predication** | Conditional execution via predicate flags (PR register) |
| **Multiple output formats** | Ternary (`-0+`), 9‑ary, 27‑ary, binary |
| **Multiple input formats** | Text, Ninary (`0n`), Tryx (`0y`), binary |
| **Cycle-accurate simulation** | Latency tables per instruction type and configuration |
| **Hardware counters** | Cycle count, instruction count, stall count via MMIO |
| **CLI debugger** | Interactive REPL with breakpoints, trace, disassembly, memory dump |

---

## Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Build

```bash
dotnet build T3Sharp.slnx
```

### Run the CLI Simulator

```bash
dotnet run --project src/T3Simulator.CLI/T3Simulator.CLI.csproj
```

### Run Tests

```bash
dotnet test tests/
```

---

## Processor Configurations

### Word Types

| Type | Trits | Trytes | Range | Internal Storage (C#) |
|------|-------|--------|-------|-----------------------|
| **T3‑18** | 18 | 3 | ±193,710,244 | `long` / `int` |
| **T3‑54** | 54 | 9 | ±2.9 × 10²⁵ | `BigInteger` / `Int128` |

### Register File

Nine logical general-purpose registers:

| 0: RW | 1: RX | 2: RY | 3: RZ | 4: R0 | 5: R1 | 6: R2 | 7: R3 | 8: R4 |
|-------|-------|-------|-------|-------|-------|-------|-------|-------|

**Special registers:** `SP` (stack pointer), `PC` (program counter), `Cond` (condition flag, 1 trit), `PR` (predicate register, 9/27 trits).

### Memory

- **1M words** of physical memory (1,048,576 words)
- Word-addressed; stack grows downward
- MMIO-mapped hardware counters at `0x3FFFF00`–`0x3FFFF02`

---

## Instruction Set

### Instruction Groups

| Group | Mnemonics | Description |
|-------|-----------|-------------|
| **Arithmetic** | `ADD`, `SUB`, `MUL`, `DIV`, `MOD`, `NEG` | Balanced ternary arithmetic |
| **Logic** | `TRITAND`, `TRITOR`, `TRITXOR` | Per‑trit min, max, sum mod 3 |
| **Shifts** | `SHL`, `SHR` | Multiply/divide by powers of 3 |
| **Memory** | `LOAD`, `STORE`, `LI`, `LIMM` | Memory access and immediate loads |
| **Control** | `JMP`, `JE`, `JNE`, `JL`, `JG`, `JM` | Conditional/unconditional branches |
| **Subroutines** | `CALL`, `RET` | Stack-based calls with register windowing |
| **Stack** | `PUSH`, `POP` | Stack manipulation |
| **I/O** | `IN`, `OUT`, `INI`, `OUTI` | Port-mapped I/O |
| **FPU** | `FADD`, `FSUB`, `FMUL`, `FDIV`, … | Floating-point operations (17 instructions) |
| **VLIW** | `SPEK`, `COMMIT`, `VADD3`, `VCMP`, … | Speculation, SIMD (T3‑54 only) |

### Instruction Formats

**R‑type (Register–Register):**
```
[ opcode+pred (6) | op1 (3) | op2 (3) | op3 (3) | reserve/func (3) ]
```

**I‑type (Register–Immediate):**
```
[ opcode+pred (6) | op1 (3) | op2 (3) | imm6 (6) ]
```

- `opcode+pred = base_opcode + pred_index × 28`
- `imm6` range: −364 … +364

### Example Assembly

```asm
; Add two numbers
main:
    LI  RW, 10           ; RW = 10
    LI  RX, 20           ; RX = 20
    ADD RY, RW, RX       ; RY = RW + RX = 30
    HALT
```

> **Full ISA reference:** [docs/t3-isa-reference.md](docs/t3-isa-reference.md)

---

## Microarchitectures

### In‑Order Processor

- Single ALU, sequential execution
- No pipeline, no speculation
- Predication is emulated (NOP on predicate fail)
- Supported on both T3‑18 and T3‑54

**Instruction Latencies (cycles):**

| Instruction | T3‑18 | T3‑54 |
|-------------|-------|-------|
| ADD, SUB, MOV, LI, NEG, TRITAND, TRITOR, TRITXOR, CMP, SHL, SHR | 1 | 1 |
| LOAD, STORE | 2 | 2 |
| MUL | 5 | 8 |
| DIV, MOD | 10 | 15 |
| CALL, RET, PUSH, POP | 2 | 2 |
| Branches (not taken / taken) | 1 / 2 | 1 / 2 |

### VLIW Processor (T3‑54 only)

- Three identical ALUs executing in parallel
- One bundle = three 18‑trit slots in one 54‑trit word
- **Conflict detection**: register write conflicts, memory access priority, branch limits
- **Speculation**: `SPEK`/`COMMIT`/`ROLLBACK` with shadow register file
- **SIMD**: vector operations on three 18‑trit sub‑words (`VADD3`, `VMUL3`, `VDOT3`, etc.)

```asm
; VLIW bundle syntax
{ ADD R0, R1, R2 | MUL R3, R4, R5 | LOAD R6, R7 }
;  Slot 0           Slot 1           Slot 2
```

> **Full architecture reference:** [docs/t3-architecture.md](docs/t3-architecture.md)

---

## Floating-Point Unit (FPU)

| Format | Width | Exponent | Mantissa (hidden trit) | Bias |
|--------|-------|----------|------------------------|------|
| **tfloat** | 18 trits | 6 trits | 12 trits | 182 |
| **tdouble** | 36 trits | 8 trits | 28 trits | 3280 |

- 9 FPU data registers: **FW, FX, FY, FZ, F0–F4**
- Status register **FSR** accessible via port `0x20`
- Exceptions set flags in FSR but do **not** generate interrupts
- Supports: add, sub, mul, div, sqrt, abs, neg, cmp, conversions, classify

---

## T‑SCII Character Encoding

T‑SCII (Ternary Standard Code for Information Interchange) is a 6‑trit (tryte) encoding:

| Range | Content |
|-------|---------|
| −364 … −1 | Greek, pseudographics, arrows, math symbols |
| **0 … 255** | **CP‑1251 compatible (ASCII + Cyrillic + special chars)** |
| 256 … 364 | Mathematical & technical symbols |

### Output Format Examples

| Format | Example (`'A'`) | Alphabet |
|--------|-----------------|----------|
| **Ternary** | `0 + - + + -` | `- 0 +` |
| **9‑ary (Ninary)** | `0n1Y2` | `W X Y Z 0 1 2 3 4` (pairs) |
| **27‑ary (Tryx)** | `0y2B` | `N O P … A B C D` (triplets) |

---

## Input & Output Formats

Programs can be loaded in multiple representations:

1. **Text format** — direct `-0+` character sequences
2. **Binary format** — `.bin` files with raw 2‑bit‑per‑trit encoding
3. **Ninary format (`0n`)** — 9‑ary encoding (3 chars per tryte)
4. **Tryx format (`0y`)** — 27‑ary encoding (2 chars per tryte)

Output formats for register and memory dumps: ternary, 9‑ary, 27‑ary, binary.

---

## CLI Commands

```
T3> help                    Show available commands
T3> load program.tsc        Load program from file
T3> run                     Run until HALT
T3> run 100                 Run up to 100 instructions
T3> step                    Execute one instruction
T3> step 10                 Execute 10 instructions
T3> dump registers ternary  Show registers in ternary format
T3> dump registers nonary   Show registers in 9‑ary format
T3> dump registers 27ary    Show registers in 27‑ary format
T3> breakpoint 0x00000010   Set breakpoint
T3> breakpoint list         List all breakpoints
T3> trace on                Enable trace buffer
T3> trace off               Disable trace buffer
T3> disassemble             Show disassembled program
T3> stack                   Show stack contents
T3> memory 0x00000000 16    Show 16 memory locations
T3> reset                   Reset processor state
T3> exit                    Exit the simulator
```

---

## Project Structure

```
T3Sharp/
├── README.md                           # You are here
├── README.ru.md                        # Russian version
├── T3Sharp.slnx                        # Solution file
├── docs/
│   ├── t3-architecture.md              # Full architecture reference (EN)
│   ├── t3-architecture.ru.md           # Full architecture reference (RU)
│   ├── t3-isa-reference.md             # Instruction set reference (EN)
│   └── t3-isa-reference.ru.md          # Instruction set reference (RU)
├── plans/                              # Design documents & specifications
│   ├── plan.md                         # Core processor spec (RU)
│   ├── opcodes.md                      # Opcode table (RU)
│   ├── fpu.md                          # FPU spec (RU)
│   ├── t-scii.md                       # T‑SCII encoding spec (RU)
│   ├── t3-processor-implementation-plan.md
│   └── t3-simulator-cli-plan.md
├── reports/
│   └── development_report.md           # Development status report (RU)
├── src/
│   ├── TritTypes/                      # Core ternary types (Trit, Tryte, Word18, Word54)
│   ├── T3Assembler/                    # T3 assembly language assembler
│   ├── T3Simulator.Common/             # Shared infrastructure (ProcessorBase, Memory, Devices)
│   ├── T3Simulator.CLI/                # Interactive command-line simulator
│   ├── T3Simulator.GUI/                # Graphical UI (planned)
│   ├── T3Simulator.InOrder/            # In‑order processor implementation
│   └── T3Simulator.VLIW/               # VLIW processor implementation (T3‑54)
└── tests/
    ├── TritTypes.Tests/                # Unit tests for ternary types
    ├── T3Simulator.Common.Tests/       # Unit tests for common components
    └── T3Simulator.InOrder.Tests/      # Instruction & integration tests
```

---

## Building & Testing

```bash
# Restore dependencies
dotnet restore T3Sharp.slnx

# Build all projects
dotnet build T3Sharp.slnx

# Run all tests
dotnet test tests/

# Run a specific test project
dotnet test tests/T3Simulator.InOrder.Tests/T3Simulator.InOrder.Tests.csproj

# Build in Release mode
dotnet build -c Release T3Sharp.slnx
```

---

## Documentation

| Document | Language | Description |
|----------|----------|-------------|
| [docs/t3-architecture.md](docs/t3-architecture.md) | 🇬🇧 English | Full architecture reference: data types, registers, memory, instruction formats, microarchitectures, FPU, I/O, T‑SCII |
| [docs/t3-architecture.ru.md](docs/t3-architecture.ru.md) | 🇷🇺 Russian | Полный справочник по архитектуре |
| [docs/t3-isa-reference.md](docs/t3-isa-reference.md) | 🇬🇧 English | Complete instruction set reference with encoding examples |
| [docs/t3-isa-reference.ru.md](docs/t3-isa-reference.ru.md) | 🇷🇺 Russian | Полный справочник по системе команд |
| [plans/](plans/) | 🇷🇺 Russian | Design docs, opcode tables, FPU spec, T‑SCII spec, implementation plans |

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Adding a New Processor

1. Create a new processor class inheriting from `ProcessorBase<T>`
2. Implement abstract methods: `Step()`, `LoadProgram()`, `GetState()`
3. Add configuration to the processor config
4. Update CLI to support the new processor type

---

## License & Acknowledgments

This project is licensed under the **MIT License**.

### Inspiration

- **Thomas Fowler** (1840) — originator of the balanced ternary concept
- **Setun computer** — ternary computer developed at Moscow State University (1958) under Nikolay Brusentsov
- **Donald Knuth** — *The Art of Computer Programming*, Vol. 2: Seminumerical Algorithms

### Built With

- [.NET 8.0 SDK](https://dotnet.microsoft.com/)
- [MSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)

---

**Happy Ternary Computing!** 🌟