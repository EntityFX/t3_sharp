# Supported Components — T3Sharp

This document lists the implementation status of every major component in the T3Sharp project.  
Status categories:

| Status | Meaning |
|--------|---------|
| **Supported** | Implemented, tested in CI, semantic contract is stable. |
| **Experimental** | Functional prototype; may have known gaps; tests may be incomplete. |
| **Planned** | Design exists in specification/docs; no execution backend yet. |
| **Specification-only** | Described in architecture documents; no code exists. |

---

## Core Types

| Component | Status | Notes |
|-----------|--------|-------|
| `Trit`, `Tryte` | **Supported** | Core balanced-ternary primitives. |
| `Word18` | **Supported** | 18-trit word; arithmetic, comparison, shifts, tritwise ops. |
| `Word54` | **Experimental** | 54-trit word; arithmetic implemented, generic path is partial (lower-18 extraction in decoder). |
| `IT3Word<TSelf>` | **Supported** | Generic interface with static abstract members. |
| `T3ArithmeticEngine` | **Supported** | Expression evaluator for assembler `.equ` directives. |
| `TScii` | **Supported** | 6-trit T-SCII encoding table (CP1251-compatible zone at 0..255). |

---

## Simulator — Common Infrastructure

| Component | Status | Notes |
|-----------|--------|-------|
| `ProcessorBase<TWord>` | **Supported** | Abstract processor; register file, PC, SP, counters, MMIO. |
| `Memory<TWord>` | **Supported** | Word-addressed memory (1M words); MMIO region at top of address space. |
| `DeviceManager<TWord>` | **Experimental** | Port-mapped I/O; basic register/unregister. |
| `InstructionEncoder` | **Supported** | Strict encoding; throws on out-of-range fields. |
| `InstructionDecoder` | **Supported** | Full decode for 18-trit instructions; `Decode(Word54)` extracts lower 18 trits. |
| `T3Disassembler` | **Supported** | All 66 opcodes have mnemonics; I/R/J format printing. |
| `Opcode` enum | **Supported** | 69 opcodes defined. |
| `T3Config` | **Supported** | Processor configuration (word size, latencies). |

---

## Simulator — In-Order Processor

| Component | Status | Notes |
|-----------|--------|-------|
| `T3InOrderProcessor` | **Supported** | Full in-order execution for T3-18; all base + FPU instructions. |
| Predication (PR register) | **Supported** | NOP-on-fail emulation. |
| Stack instructions (PUSH/POP/PUSHI/POPI/CALL/RET) | **Supported** | Stack grows downward from top of memory. |
| Hardware counters (cycle/inst/stall) | **Supported** | Readable via MMIO; CYCLE_LOW write resets all counters. |
| MMIO timer interface | **Experimental** | Addresses defined; read returns 0; write is ignored. |

---

## Assembler

| Component | Status | Notes |
|-----------|--------|-------|
| `T3Assembler` | **Supported** | Two-pass assembler; labels, `.word`, `.string`, `.equ`. |
| VLIW Assembler (`T3VliwAssembler`) | **Experimental** | Assembles three 18-trit slots into Word54 bundle. |

---

## T-lang Toolchain

| Component | Status | Notes |
|-----------|--------|-------|
| Lexer / Tokenizer | **Supported** | Full tokenization for T-lang. |
| Parser / AST | **Supported** | `IfStmt` has `MaybeBody`; ternary expressions supported. |
| Preprocessor | **Experimental** | Basic `#include` support. |
| `T3Interpreter` (AST walker) | **Experimental** | Scalar subset: three-valued `if`/`maybe`/`else`, scopes (dynamic), functions. Arrays/structs/globals not yet initialized from declarations; string/float literals return 0. |
| `T3Compiler` (`CodeGenerator`) | **Experimental** | Generates T3 assembly; `MaybeBody` supported (2026-06-26); calling convention uses stack-based save/restore. |
| Interpreter ↔ Compiler equivalence | **Experimental** | Partial — not all language features verified across both backends. |

---

## FPU

| Component | Status | Notes |
|-----------|--------|-------|
| `T3Float` (tfloat) | **Experimental** | 6 trit exponent + 12 trit mantissa. Approximate model: `FromDouble` quantizes to integer/power-of-3 mantissa; full tfloat precision not yet achieved. |
| `T3Fpu` arithmetic (Add/Sub/Mul/Div/Sqrt) | **Experimental** | Integer-oriented approximation; `Div` uses integer division for exponent=182. Uses `double` intermediates; 12-trit mantissa range not fully utilized. |
| FSR / status flags | **Planned** | FSR port defined (`0x20`); flags not fully implemented. |
| Rounding modes | **Planned** | Not implemented. |
| FPU exception handling | **Experimental** | Division by zero / sqrt negative throw .NET exceptions; processor halts. |
| `LOADI`/`STOREI` | **Experimental** | Semantics: base register (op2) + immediate offset. Requires two registers (destination + base) in one I-type slot; full encoding may need I2-type expansion. |

---

## Microarchitectures

| Component | Status | Notes |
|-----------|--------|-------|
| T3-18 In-Order | **Supported** | Single-ALU, sequential, predication emulated. |
| T3-54 In-Order | **Experimental** | Generic path exists but `Decode(Word54)` extracts lower 18 trits; FPU ops cast via `Word18`. |
| VLIW Processor | **Planned** | Design in specification; no execution backend. |
| SIMD instructions (VADD3, VMUL3, …) | **Planned** | Defined in ISA; no implementation. |
| Speculation (SPEK/COMMIT/ROLLBACK) | **Planned** | Defined in ISA; no implementation. |
| Register Windowing | **Planned** | `RegisterWindow` helper and `WP` field exist; not integrated into register access. |

---

## Tools & GUI

| Component | Status | Notes |
|-----------|--------|-------|
| CLI Simulator (`T3Simulator.CLI`) | **Supported** | Interactive REPL with breakpoints, trace, disasm, memory dump. |
| Number Converter (`T3NumberConverter`) | **Supported** | Decimal ↔ ternary/9-ary/27-ary. |
| `T3Converter.GUI` | **Experimental** | GUI prototype. |
| `T3Calculator.GUI` | **Experimental** | GUI prototype. |
| `T3Interpreter.CLI` | **Experimental** | CLI runner for T-lang interpreter. |
| `T3Simulator.GUI` | **Experimental** | GUI prototype. |

---

## Testing & CI

| Component | Status | Notes |
|-----------|--------|-------|
| `TritTypes.Tests` | **Supported** | In CI. |
| `T3Simulator.Common.Tests` | **Supported** | In CI; FPU tests use independent expected values (2026-06-26). |
| `T3Simulator.InOrder.Tests` | **Supported** | In CI. |
| `T3Interpreter.Tests` | **Experimental** | In CI (2026-06-26); coverage being expanded. |
| Equivalence tests (interpreter vs compiler+simulator) | **Experimental** | 24 tests covering expressions, loops, if/maybe/else, functions, recursion, preprocessor; 1 known divergence (nested function calls, compiler call ABI). |

---

*Last updated: 2026-06-26*