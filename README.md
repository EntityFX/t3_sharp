# T3 Sharp — Ternary Computer Simulator & Toolchain

T3 Sharp is a full-stack simulator, assembler, compiler (T-lang), and runtime for a balanced ternary computer (18-trit word, 1M words of memory).

## Project Status

**ISA v5** — orthogonal instruction set with RegGroup flags (July 2026).

## Architecture

### Instruction format (ISA v5)

```
[Pred(3)] [RegGroup(1)] [Fmt(1)] [Opcode(4)] [Args(9)]
```

| Field      | Width | Description                              |
|------------|-------|------------------------------------------|
| Pred       | 3     | Predicate (0 = always execute)           |
| RegGroup   | 1     | -1=FPU (F.), 0=GP, +1=Special (S.)      |
| Fmt        | 1     | -1=J-type, 0=R/S-type, +1=I-type        |
| Opcode     | 4     | 4-trit opcode (30 values)               |
| Args       | 9     | Operands (balanced ternary fields)       |

### Register files

| RegGroup | Prefix | Registers                              |
|----------|--------|----------------------------------------|
| **GP**   | (none) | RW, RX, RY, RZ, R0, R1, R2, R3, R4    |
| **FPU**  | `F.`   | FW, FX, FY, FZ, F0, F1, F2, F3, F4    |
| **Special** | `S.` | FP, HP, SP, CD, PR, WD              |

### 30 opcodes

System: `HALT`, `NOP`, `LIMM`  
Data: `MOV`, `LD`, `ST`  
Stack: `PUSH`, `POP`  
ALU: `ADD`, `SUB`, `MUL`, `DIV`, `MOD`, `NEG`, `ABS`, `CMP`  
Logic: `AND`, `OR`, `XOR`, `SHL`, `SHR`  
Control: `JMP`, `JE`, `JNE`, `JL`, `JG`, `JLE`, `JGE`, `JM`, `CALL`, `RET`  
I/O: `IN`, `OUT`  
Float: `SQRT`, `FTI`, `ITF`, `CLASS`, `SWAP`

### Example (orthogonal syntax)

```
       MOV  R0, #42          ; GP immediate
       F.ADD F0, F1, F2      ; FPU arithmetic
       S.MOV RZ, FP           ; Special → GP
       S.SUB SP, SP, #16      ; Stack allocation
```

## Project Structure

```
src/
├── T3Simulator.Common/     # ISA definitions, encoder, decoder, ALU, state
├── T3Simulator.InOrder/    # Sequential processor implementation
├── T3Simulator.CLI/        # Command-line simulator
├── T3Simulator.GUI/        # GUI simulator (WPF/Avalonia)
├── T3Assembler/            # Two-pass assembler (supports F./S. prefixes)
├── T3Compiler/             # T-lang → ASM code generator
├── T3Interpreter/          # T-lang interpreter (reference)
├── T3Interpreter.CLI/      # T-lang REPL
├── TritTypes/              # Word18/Word54/T3Float balanced ternary types
└── T3NumberConverter/      # Ternary ↔ decimal converter
tests/
├── T3Simulator.Common.Tests/
├── T3Simulator.InOrder.Tests/
└── TritTypes.Tests/
docs/
├── t3-isa-v5-specification.md   # ISA v5 spec
├── t3-abi-v4-specification.md   # ABI v4 spec (obsoleted by v5)
└── t3-architecture.md           # Architecture overview
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Windows 10+ / Linux / macOS

### Build

```bash
dotnet build T3Sharp.sln
```

### Run tests

```bash
dotnet test T3Sharp.sln
```

### Use CLI simulator

```bash
dotnet run --project src/T3Simulator.CLI
> load test_fib.asm
> run
> dump registers
```

### Compile & run T-lang

```bash
dotnet run --project src/T3Interpreter.CLI
> tint main() { return 42; }
```

## License

MIT License. Copyright (c) 2024-2026 EntityFX