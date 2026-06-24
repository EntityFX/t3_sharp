# T3 Ternary Processor — Deep Gap Analysis & Fix Specification

**Date:** 2026-06-14  
**Author:** T3 Research Team  
**Scope:** Processor (T3‑18/T3‑54), Assembler (T3Assembler), Language (T‑lang compiler)

---

## Table of Contents

1. [Summary Matrix](#1-summary-matrix)
2. [Processor-Level Gaps](#2-processor-level-gaps)
3. [Assembler-Level Gaps](#3-assembler-level-gaps)
4. [T-lang Compiler Gaps](#4-t-lang-compiler-gaps)
5. [Fix Signatures & Code Changes](#5-fix-signatures--code-changes)
6. [Test Impact Analysis](#6-test-impact-analysis)
7. [Roadmap](#7-roadmap)

---

## 1. Summary Matrix

| ID | Level | Gap | Priority | Effort (LOC) | Phase |
|----|-------|-----|----------|-------------|-------|
| CPU‑01 | Processor | No interrupt vectors / controller | 🔴 Critical | ~800 | 3 |
| CPU‑02 | Processor | No privilege modes (user/supervisor) | 🟡 High | ~1200 | 3 |
| CPU‑03 | Processor | Only 9 GP registers | 🟡 High | ~400 | 2 |
| CPU‑04 | Processor | No MMU / virtual memory | 🟡 High | ~2000 | 3 |
| CPU‑05 | Processor | Missing `JLE`/`JGE` instructions | ✅ Done | ~150 | 1 |
| CPU‑06 | Processor | `LI` limited to ±364 | 🟡 High | ~200 | 2 |
| CPU‑07 | Processor | No atomic operations (LL/SC/CAS) | 🟡 High | ~500 | 3 |
| CPU‑08 | Processor | No explicit `NOP` instruction | 🟢 Low | ~20 | 1 |
| CPU‑09 | Processor | No `PUSHI`/`POPI` immediate push/pop | 🟢 Low | ~100 | 1 |
| CPU‑10 | Processor | No shift-through-carry | 🟢 Low | ~200 | 2 |
| CPU‑11 | Processor | FPU exceptions don't generate interrupts | 🟡 High | ~300 | 2 |
| CPU‑12 | Processor | No FMA (fused multiply-add) | 🟢 Low | ~250 | 4 |
| CPU‑13 | Processor | No trig functions (sin/cos/tan) | 🟢 Low | ~600 | 4 |
| CPU‑14 | Processor | Single rounding mode only | 🟢 Low | ~150 | 2 |
| CPU‑15 | Processor | No pipeline (in-order only) | 🟡 High | ~1500 | 4 |
| ASM‑01 | Assembler | No macros (`.macro`/`.endm`) | 🟡 High | ~400 | 2 |
| ASM‑02 | Assembler | No linker (single-file only) | 🟡 High | ~1500 | 2 |
| ASM‑03 | Assembler | No assembly-time expressions | 🟡 High | ~200 | 1 |
| ASM‑04 | Assembler | No `.equ`/`.set` constants | 🟡 High | ~100 | 1 |
| ASM‑05 | Assembler | No `.align` directive | 🟡 High | ~80 | 1 |
| ASM‑06 | Assembler | No `.org` directive | 🟡 High | ~60 | 1 |
| ASM‑07 | Assembler | No debug info (`.loc`/`.file`) | 🟡 High | ~500 | 2 |
| ASM‑08 | Assembler | No listing output (`.list`) | 🟢 Low | ~150 | 1 |
| ASM‑09 | Assembler | No `.include` for .asm files | 🟢 Low | ~50 | 1 |
| ASM‑10 | Assembler | No conditional assembly (`.if`/`.else`) | 🟡 High | ~250 | 1 |
| ASM‑11 | Assembler | Labels not usable as immediates | 🔴 Critical | ~200 | 1 |
| TL‑01 | T‑lang | No standard library implementation | 🔴 Critical | ~800 | 1 |
| TL‑02 | T‑lang | Float literals → zero | 🔴 Critical | ~300 | 1 |
| TL‑03 | T‑lang | No string support | 🟡 High | ~400 | 1 |
| TL‑04 | T‑lang | No `switch`/`case` | ✅ Done | ~250 | 1 |
| TL‑05 | T‑lang | No `enum` | 🟡 High | ~150 | 2 |
| TL‑06 | T‑lang | No type casts `(type)expr` | 🟡 High | ~200 | 2 |
| TL‑07 | T‑lang | No `sizeof()` | 🟡 High | ~100 | 2 |
| TL‑08 | T‑lang | No `typedef` | 🟡 High | ~150 | 2 |
| TL‑09 | T‑lang | No array initialization `{1,2,3}` | ✅ Done | ~300 | 1 |
| TL‑10 | T‑lang | No `do`/`while` | ✅ Done | ~100 | 1 |
| TL‑11 | T‑lang | Ternary expression not codegen'd | ✅ Done | ~200 | 2 |
| TL‑12 | T‑lang | No `goto` / labels | 🟢 Low | ~100 | 2 |
| TL‑13 | T‑lang | Round-robin register allocator | 🟡 High | ~800 | 2 |
| TL‑14 | T‑lang | No constant folding / DCE / CSE | 🟢 Low | ~600 | 2 |
| TL‑15 | T‑lang | Caller-saved regs not preserved | ✅ Done | ~150 | 1 |
| TL‑16 | T‑lang | LIMM word reservation broken | ✅ Done | ~100 | 1 |
| TL‑17 | T‑lang | No `.map`/symbol table output | 🟢 Low | ~200 | 2 |
| TL‑18 | T‑lang | No source-location error messages | 🟡 High | ~300 | 2 |
| TL‑19 | T‑lang | No `-Werror`/`-Wall` diagnostics | 🟢 Low | ~200 | 2 |
| TL‑20 | T‑lang | No `-O0`/`-O1`/`-O2` flags | 🟢 Low | ~150 | 2 |

**Totals:** 46 gaps — 5 🔴 Critical, 26 🟡 High, 15 🟢 Low  

---

## 2. Processor-Level Gaps

### CPU‑01: No Interrupt Vectors / Controller

**Current State:** Exceptions (invalid opcode, div‑by‑zero, memory out‑of‑bounds) halt the processor immediately via `IsHalted = true`. No interrupt mechanism exists.

**Target State:** Add an interrupt controller with 16 vector entries at fixed address `0x00010`–`0x0001F`. Add `INT n` (trigger interrupt), `IRET` (return from interrupt) instructions.

**ISA Changes:**
```asm
; New instructions (codes 46, 47)
INT  op1    ; PC = vector_table[op1], push PC, push SR
IRET        ; pop SR, pop PC

; Interrupt vector table (16 entries)
; Address 0x00010: Entry 0  0x00011: Entry 1  ...  0x0001F: Entry 15

; Status Register (SR) — new special register
; Bits: [PRIV(1) | IE(1) | reserved(remainder)]
; PRIV = 1 => supervisor mode; IE = 1 => interrupts enabled
```

**Files Changed:**
- `src/T3Simulator.Common/T3Config.cs` — add `InterruptVectorsBase` constant
- `src/T3Simulator.Common/ProcessorBase.cs` — add `SR` register, `HandleInterrupt()` method
- `src/T3Simulator.InOrder/T3InOrderProcessor.cs` — implement INT/IRET, add `RaiseInterrupt(int vector)`
- `src/T3Simulator.Common/Opcode.cs` — add `INT=46, IRET=47`
- `src/T3Assembler/T3InOrderAssembler.cs` — encode INT/IRET

**Effort:** ~800 LOC, ~8 hours

---

### CPU‑02: No Privilege Modes

**Current State:** Flat memory model. All code runs with full access. No separation between kernel and user code.

**Target State:** Add Supervisor/User modes via `SR.PRIV` bit. Certain instructions (`IN`/`OUT`, `HALT`, writes to MMIO) are supervisor-only. Attempted execution in user mode raises privilege violation exception (interrupt vector 13).

**ISA Changes:**
```asm
; New supervisor-mode-only instructions
SVC  imm6   ; Supervisor Call: software interrupt, enters supervisor
RFE         ; Return From Exception (alias IRET for privilege transitions)
SETPRIV op1  ; Set privilege bit directly (supervisor only)
```

**Files Changed:**
- `ProcessorBase.cs` — add `PrivilegeLevel` property, guard check before MMIO/IO/halt
- `T3InOrderProcessor.cs` — implement SVC (code 48), SETPRIV (code 49)

**Effort:** ~1200 LOC, ~12 hours

---

### CPU‑03: Only 9 GP Registers

**Current State:** R0–R8 (9 registers). Register allocator in compiler runs round‑robin over R0–R5, R4 is hard‑dedicated to address calculations.

**Target State:** Expand to 27 GP registers (R0–R26) via register windowing. Add `WP` (Window Pointer) register. `CALL` saves WP to stack, allocates fresh window; `RET` restores. This mirrors SPARC / T3‑54 model but brings it to T3‑18 as well.

**ISA Changes:**
```asm
; New instruction
SAVE        ; WP -= 8, saves previous window registers to stack if needed
RESTORE     ; WP += 8, restores previous window
```

**Register File Changes:**
- `ProcessorBase.cs` — `Registers` array expands from 9 to 27
- `T3InOrderProcessor.cs` — SAVE/RESTORE opcodes (50, 51)
- `CodeGen/CodeGenerator.cs` — allocator uses 0‑25, drops round‑robin for graph‑coloring

**Effort:** ~400 LOC (register expansion) + ~800 LOC (allocator rewrite) = ~1200 LOC total, ~16 hours

---

### CPU‑04: No MMU / Virtual Memory

**Current State:** Physical addressing only. 1M words of directly‑addressable memory.

**Target State:** Add software‑managed TLB with 16 entries. Page size: 256 words (8 trits offset). Translation: `VA[19:8] → TLB lookup → PA[19:8] | VA[7:0]`.

**ISA Changes:**
```asm
; MMU control ports (0x30–0x37)
; 0x30: MMU_CTRL  — enable (bit 0), flush (bit 1)
; 0x31: MMU_PTE   — write page table entry
; 0x32: MMU_FAULT_ADDR — last fault address
; 0x33: MMU_FAULT_STATUS

; New instructions
TLBWR       ; Write TLB entry: op1 = VPN, op2 = PPN|flags
TLBRD       ; Read TLB entry
TLBFL       ; Flush TLB
```

**Files Changed:**
- `ProcessorBase.cs` — add `TLB` array (16 entries), `TranslateAddress()` method
- `T3InOrderProcessor.cs` — TLBWR/TLBRD/TLBFL instructions (52, 53, 54)
- `T3Config.cs` — `TLB_SIZE = 16`, `PAGE_SIZE = 256`

**Effort:** ~2000 LOC, ~24 hours

---

### CPU‑05: Missing JLE/JGE Instructions

**Current State:** Only JE(19), JNE(20), JL(21), JG(22), JM(23). Compiler generates 2‑jump patterns for ≤ and ≥.

**Target State:** Add dedicated jump instructions that test both less‑than and equal conditions.

**ISA Changes:**
```asm
; New instructions (codes 55, 56)
JLE  op1    ; Jump if Cond ≤ 0  (i.e. Cond is - or 0)
JGE  op1    ; Jump if Cond ≥ 0  (i.e. Cond is 0 or +)
```

**Implementation:**
```csharp
// In T3InOrderProcessor.Execute():
case Opcode.JLE:
    if (Cond <= 0) { PC = registers[op1]; CycleCount++; }
    break;
case Opcode.JGE:
    if (Cond >= 0) { PC = registers[op1]; CycleCount++; }
    break;
```

**Files Changed:**
- `Opcode.cs` — add `JLE=55, JGE=56`
- `T3InOrderProcessor.cs` — implement execution
- `T3AssemblerBase.cs` / `T3InOrderAssembler.cs` — encode
- `CodeGen/CodeGenerator.cs` — simplify `EmitCondJump` to use single‑jump for all ops

**Effort:** ~150 LOC, ~2 hours

---

### CPU‑06: LI Limited to ±364

**Current State:** `LI` instruction uses 6‑trit immediate (range −364…+364). Addresses outside this range require `LIMM` which reads next instruction word.

**Target State:** Extend I‑type to use 9‑trit immediate by stealing bits from op2 when op2 is unused (LI ignores op2). New encoding: `[opcode+pred(6) | op1(3) | imm9(9)]`.

**ISA Changes:**
```asm
; LI op1, imm9   — range: −9841 … +9841
; Encoding: op2 field is zero-extended into immediate
; If op2 != 0 in LI encoding, treated as old-style LI with imm6
```

**Files Changed:**
- `T3InOrderAssembler.cs` — detect when imm fits 9 trits, use extended encoding
- `T3InOrderProcessor.cs` — decode extended LI

**Effort:** ~200 LOC, ~3 hours

---

### CPU‑07: No Atomic Operations

**Current State:** No way to perform read‑modify‑write atomically. Multi‑threading impossible.

**Target State:** Add Load‑Linked / Store‑Conditional pair.

**ISA Changes:**
```asm
; New instructions (codes 57, 58)
LL  op1, op2   ; Load Linked: op1 = mem[op2]; set reservation flag
SC  op1, op2   ; Store Conditional: if reservation still valid, mem[op2] = op1, op1 = 1; else op1 = -1
```

**Implementation:**
```csharp
long? _reservationAddr;
Word18 _reservationData;

case Opcode.LL:
    registers[op1] = memory[registers[op2]];
    _reservationAddr = registers[op2];
    break;
case Opcode.SC:
    if (_reservationAddr == registers[op2] && memory[registers[op2]] == _reservationData) {
        memory[registers[op2]] = registers[op1];
        registers[op1] = Word18.FromLong(1);  // success
    } else {
        registers[op1] = Word18.FromLong(-1); // failure
    }
    _reservationAddr = null;
    break;
```

**Files Changed:**
- `Opcode.cs` — `LL=57, SC=58`
- `ProcessorBase.cs` — add `_reservationAddr`
- `T3InOrderProcessor.cs` — implement

**Effort:** ~500 LOC, ~6 hours

---

### CPU‑08: No Explicit NOP

**Current State:** Code 63 is reserved. Predication creates implicit NOPs.

**Target State:** Formalise code 63 as `NOP`. All operands ignored. `PC += 1`.

**Effort:** ~20 LOC (recognise opcode 63, skip), trivial

---

### CPU‑09: No PUSHI/POPI

**Current State:** Push/pop only register operands. To push a constant: `LI Rr, val; PUSH Rr` (2 cycles).

**Target State:** Add I‑type variants.

**ISA Changes:**
```asm
PUSHI imm6   ; SP -= 1; mem[SP] = imm6  (code 83)
POPI  op1    ; op1 = imm6  (code 84, same as MOVI but with stack semantics?)
; Better: add PUSHI (83), POP is always to register
```

**Effort:** ~100 LOC, ~1 hour

---

### CPU‑10: No Shift-Through-Carry

**Current State:** SHL/SHR discard overflow bits.

**Target State:** Add SHLC/SHRC that propagate trit through Cond.

**ISA Changes:**
```asm
SHLC op1, op2, op3   ; op1 = op2 << op3, carry trit → Cond  (code 59)
SHRC op1, op2, op3   ; op1 = op2 >> op3, carry trit → Cond  (code 60)
```

**Effort:** ~200 LOC, ~3 hours

---

### CPU‑11: FPU Exceptions Don't Generate Interrupts

**Current State:** FPU exceptions only set FSR flags. No interrupt.

**Target State:** On FADD/FSUB/FMUL/FDIV/FSQRT exception, if FSR.IE (interrupt enable) bit is set, raise interrupt vector 14.

**ISA Changes:**
```asm
; FSR bit 5: IE (Interrupt Enable)
; FSR bit 6: IRQ (Interrupt Request — read‑only)
```

**Files Changed:**
- `T3Fpu.cs` — on exception, set FSR.IRQ, check FSR.IE, call `proc.RaiseInterrupt(14)`
- `ProcessorBase.cs` — add `FpuInterruptVector = 14`

**Effort:** ~300 LOC, ~4 hours

---

### CPU‑12: No FMA (Fused Multiply-Add)

**Current State:** Separate FMUL + FADD: double rounding, 12 cycles.

**Target State:** Single‑cycle fused operation.

**ISA Changes:**
```asm
FMADD Fop1, Fop2, Fop3, Fop4   ; Fop1 = Fop2 * Fop3 + Fop4  (code 109)
FMSUB Fop1, Fop2, Fop3, Fop4   ; Fop1 = Fop2 * Fop3 - Fop4  (code 110)
```

**Effort:** ~250 LOC, ~4 hours

---

### CPU‑13: No Trig Functions

**Current State:** Only FSQRT. sin/cos/tan need software emulation (Taylor series ~100+ cycles).

**Target State:** Add CORDIC‑based hardware acceleration.

**ISA Changes:**
```asm
FSIN  Fop1, Fop2   ; Fop1 = sin(Fop2)  (code 111)
FCOS  Fop1, Fop2   ; Fop1 = cos(Fop2)  (code 112)
FTAN  Fop1, Fop2   ; Fop1 = tan(Fop2)  (code 113)
FEXP  Fop1, Fop2   ; Fop1 = exp(Fop2)  (code 114)
FLOG  Fop1, Fop2   ; Fop1 = ln(Fop2)   (code 115)
```

**Effort:** ~600 LOC + CORDIC LUT, ~10 hours

---

### CPU‑14: Single Rounding Mode

**Current State:** Only round‑to‑nearest ties‑to‑even.

**Target State:** Add round‑toward‑zero, round‑toward‑+∞, round‑toward‑−∞ controlled by FSR.RM field (bits 7‑8).

**Files Changed:**
- `T3Fpu.cs` — `Round()` method accepts `roundingMode` parameter
- `ProcessorBase.cs` — FSR.RM read/write

**Effort:** ~150 LOC, ~2 hours

---

### CPU‑15: No Pipeline

**Current State:** T3‑18 is strictly sequential: fetch → decode → execute → memory → writeback, one instruction per cycle.

**Target State:** Implement 3‑stage pipeline (Fetch, Decode, Execute+Memory+Writeback) with forwarding paths for data hazards and branch prediction (static always‑not‑taken).

**Files Changed:**
- New file: `src/T3Simulator.InOrder/T3PipelineProcessor.cs` (~800 LOC)
- `ProcessorBase.cs` — refactor to support pipeline stages

**Effort:** ~1500 LOC, ~20 hours

---

## 3. Assembler-Level Gaps

### ASM‑01: No Macros

**Current State:** No text‑substitution mechanism. Repeated code patterns are manually copied.

**Target State:**
```asm
.macro push_all
    PUSH RW
    PUSH RX
    PUSH RY
.endm

.macro mul_by Rdest, Rsrc, factor
    LI Rdest, factor
    MUL Rdest, Rsrc, Rdest
.endm

main:
    push_all
    mul_by R0, R1, 5
    ; expands to:  PUSH RW; PUSH RX; PUSH RY; LI R0, 5; MUL R0, R1, R0
```

**Files Changed:**
- `T3AssemblerBase.cs` — `PreprocessMacros()` pass before assembly: expand `.macro` bodies, substitute `\args`
- `T3InOrderAssembler.cs` — hook preprocessor

**Effort:** ~400 LOC, ~6 hours

---

### ASM‑02: No Linker

**Current State:** All code must be in a single file. `.extern` and cross‑file references unsupported.

**Target State:**
```asm
; file1.asm
.extern external_function
tint main:
    LI R0, external_function
    CALL R0
    HALT

; file2.asm
.global external_function
external_function:
    LI R2, 42
    RET
```

**New Tool:**
```bash
t3ld file1.o file2.o -o program.bin
```

**Files Changed:**
- New project: `src/T3Linker/T3Linker.csproj` — object file format (`.o`), symbol table, relocation pass
- `T3Assembler.cs` — emit `.o` with relocation entries
- `ObjectFormat.cs` — new file: defines `.o` layout

**Effort:** ~1500 LOC, ~20 hours

---

### ASM‑03: No Assembly-Time Expressions

**Current State:** `LI R0, 100+50` fails. Only raw numbers.

**Target State:**
```asm
LI R0, (5 * 10) + 2        ; → LI R0, 52
LI R1, sizeof(Point)        ; → compile‑time constant
.data
    .equ SIZE, 256
    LI R2, SIZE / 2          ; → LI R2, 128
```

**Files Changed:**
- `T3AssemblerBase.cs` — `ParseExpression()` method: evaluate `+-*/%<<>> && || & | ^` on integers
- `ExpressionEvaluator.cs` — new helper

**Effort:** ~200 LOC, ~3 hours

---

### ASM‑04: No .equ / .set

**Current State:** Only `.word` directive for data.

**Target State:**
```asm
.equ BUFFER_SIZE, 1024
.equ MAX_ITERS, 100

main:
    LI R0, BUFFER_SIZE       ; → LI R0, 1024
    LI R1, MAX_ITERS
```

**Files Changed:**
- `T3AssemblerBase.cs` — `ParseDirective()` handles `.equ name, value` — stores in symbol table

**Effort:** ~100 LOC, ~1 hour

---

### ASM‑05: No .align

**Current State:** No control over instruction alignment. VLIW bundles may misalign.

**Target State:**
```asm
.align 4        ; pad with NOP until PC % 4 == 0
main:
    ; guaranteed to be at word‑aligned boundary
```

**Effort:** ~80 LOC, ~1 hour

---

### ASM‑06: No .org

**Current State:** Programs always start at address 0.

**Target State:**
```asm
.org 0x100
interrupt_handler:
    IRET

.org 0x200
main:
    HALT
```

**Files Changed:**
- `T3AssemblerBase.cs` — `_currentAddress` variable, `.org` sets it, NOP‑fills gap

**Effort:** ~60 LOC, ~1 hour

---

### ASM‑07: No Debug Info

**Current State:** No mapping from instruction address to source line.

**Target State:** Emit DWARF‑like line‑number program. Assembly directives:
```asm
.file "main.asm"
.loc 42         ; line 42
    LI R0, 100
```

**Files Changed:**
- `T3AssemblerBase.cs` — `.file`/`.loc` directives
- `DebugInfo.cs` — new file: `List<(uint address, uint file, uint line)>`
- `T3Simulator.Common/T3Disassembler.cs` — show source lines in disassembly

**Effort:** ~500 LOC, ~8 hours

---

### ASM‑08: No Listing Output

**Current State:** No `.list` output showing addresses alongside instructions.

**Target State:**
```asm
; With -l flag, output:
; 0000: 4 * 3^12 + 0 * 3^9 + 0 * 3^6 + 374    LI R0, 10
; 0001: 24 * 3^12 + 0 * 3^9 + ...              CALL R0
```

**Effort:** ~150 LOC, ~2 hours

---

### ASM‑09: No .include for .asm Files

**Current State:** Only T‑lang preprocessor has `#include`.

**Target State:** Assembler supports `.include "macros.inc"` to inline assembly files.

**Effort:** ~50 LOC, ~1 hour

---

### ASM‑10: No Conditional Assembly

**Current State:** No `#ifdef`/`#if` in assembler.

**Target State:**
```asm
.ifdef DEBUG
    LI R0, 999
.endif
```

**Files Changed:**
- `T3AssemblerBase.cs` — `PreprocessConditionals()`: evaluate `.ifdef`/`.ifndef`/`.if`/`.else`/`.endif`

**Effort:** ~250 LOC, ~4 hours

---

### ASM‑11: Labels Not Usable as Immediates

**Current State:** `LI R0, main` fails with "Unable to resolve operand value: main" because assembler doesn't know label address until second pass.

**Target State:** Two‑pass assembly: pass 1 collects label addresses, pass 2 resolves them as immediates. Labels with addresses > 364 automatically use LIMM encoding.

**Files Changed:**
- `T3InOrderAssembler.cs` — refactor to two‑pass: `CollectLabels()` then `AssemblePass2()`

**Effort:** ~200 LOC, ~3 hours

---

## 4. T‑lang Compiler Gaps

### TL‑01: No Standard Library Implementation

**Current State:** `lib/tio.th` has `#defines` and function declarations but no bodies. `print_int(42)` compiles but does nothing.

**Target State:** Provide assembly implementations in T‑lang callable form:
```asm
; tio.asm — compiled into every program with -l tio
print_int:
    POP R7          ; save return
    POP R0          ; get int arg
    OUTI R0, 0      ; write to stdout (port 0 in decimal mode)
    PUSH R7         ; restore return
    RET

print_str:
    POP R7
    POP R1          ; arg = string address
    ; ... loop printing characters via OUTI ...
    PUSH R7
    RET
```

**Files Changed:**
- New: `src/T3Compiler/lib/tio.asm`, `tmath.asm`, `tstring.asm`
- `Program.cs` — auto‑link `.asm` libraries when `-l` flag
- `TLangCompilerTests.cs` — add tests for print_char, scan_int

**Effort:** ~800 LOC, ~10 hours

---

### TL‑02: Float Literals → Zero

**Current State:** `FloatLiteral fl => EmitImm(0)`.

**Target State:** Store float literal in memory after function, load via FLW:
```csharp
case FloatLiteral fl:
    long floatBits = ConvertFloatToTernary(fl.Value);
    int dataAddr = EmitDataWord(floatBits);
    int r = AllocReg();
    Emit($"    LI {RegName(r)}, {dataAddr}");
    Emit($"    FLW {RegName(r)}, {RegName(r)}");  // FPU load
    return r;
```

**Files Changed:**
- `CodeGen/CodeGenerator.cs` — `EmitDataWord()` method, float literal codegen
- `T3FloatConverter.cs` — new: decimal → tfloat/tryte bit‑pattern

**Effort:** ~300 LOC, ~4 hours

---

### TL‑03: No String Support

**Current State:** `StringLiteral` lexed, not codegen'd.

**Target State:** Store strings in a `.data` section after code, emit string address:
```csharp
case StringLiteral sl:
    int strAddr = EmitString(sl.Value);
    return EmitImm(strAddr);
```

**Files Changed:**
- `CodeGen/CodeGenerator.cs` — `EmitString()`: emits `.word` directives with T‑SCII encoded characters, null‑terminator

**Effort:** ~400 LOC, ~6 hours

---

### TL‑04: No switch/case

**Current State:** Keywords lexed, parser doesn't handle.

**Target State:** Parse `switch(expr) { case val: ... break; default: ... }` into a `SwitchStmt { AstNode Expr; List<CaseClause> Cases; }`. Codegen: emit jump table for dense cases, or if‑else chain for sparse.

**Files Changed:**
- `Ast.cs` — add `SwitchStmt`, `CaseClause`
- `Parser.cs` — `ParseSwitchStmt()`
- `CodeGen/CodeGenerator.cs` — `GenSwitch()`

**Effort:** ~250 LOC, ~4 hours

---

### TL‑05: No enum

**Current State:** `enum` keyword exists, not parsed.

**Target State:**
```c
enum Color { RED = 0, GREEN = 1, BLUE = 2 };
tint main() { tint c = RED; return c + GREEN; }
```

**Files Changed:**
- `Ast.cs` — add `EnumDef`
- `Parser.cs` — `ParseEnumDef()`
- `CodeGen/CodeGenerator.cs` — `#define RED 0` equivalent via preprocessor

**Effort:** ~150 LOC, ~2 hours

---

### TL‑06: No Type Casts

**Current State:** `(tint)expr` not parsed.

**Target State:** Add `CastExpr` AST node. Codegen: for `(tint)3.14`, use `FTOI`; for `(tfloat)42`, use `ITOF`.

**Files Changed:**
- `Ast.cs` — add `CastExpr { TypeSpec TargetType; AstNode Expr; }`
- `Parser.cs` — `ParseUnary()` handles `(` type `)` before expression
- `CodeGen/CodeGenerator.cs` — `GenCast()`

**Effort:** ~200 LOC, ~3 hours

---

### TL‑07: No sizeof()

**Current State:** Keyword exists, unimplemented.

**Target State:** `sizeof(tint) → 1`, `sizeof(struct Point) → field count`.

**Effort:** ~100 LOC, ~1 hour

---

### TL‑08: No typedef

**Current State:** Only `struct Name` / `union Name` syntax.

**Target State:** `typedef struct Point { tint x; tint y; } Point;` → register `Point` as alias.

**Effort:** ~150 LOC, ~2 hours

---

### TL‑09: No Array Init List

**Current State:** `tint arr[3] = {1, 2, 3}` not parsed.

**Target State:** Parser collects initializer list from `{ expr, expr, ... }`. Codegen emits sequence of `STORE` instructions.

**Files Changed:**
- `Parser.cs` — `ParseInitList()` after `=` in variable declaration
- `CodeGen/CodeGenerator.cs` — `GenInitList()`: loop over elements, compute offset, STORE

**Effort:** ~300 LOC, ~4 hours

---

### TL‑10: No do/while

**Current State:** Keyword exists, not implemented.

**Target State:** Parse `do { body } while (cond);`. Codegen: `loop: body; CMP a,b; JE loop`.

**Effort:** ~100 LOC, ~1 hour

---

### TL‑11: Ternary Expression Not Codegen'd

**Current State:** `cond ?? true_val :? maybe_val :! false_val` parsed but no codegen.

**Target State:** Codegen emits: evaluate cond, JE true_label, JL false_label (maybe label for 0), set result.

**Effort:** ~200 LOC, ~3 hours

---

### TL‑12: No goto / Labels

**Current State:** `KwGoto` exists, not implemented.

**Target State:** Parse `label:` and `goto label;`. Codegen: same as while‑loop labels but for arbitrary jumps.

**Effort:** ~100 LOC, ~2 hours

---

### TL‑13: Round‑Robin Register Allocator

**Current State:** `int AllocReg() { int r = _nextReg; _nextReg = (_nextReg+1)%6; return r; }`. No spill, no liveness analysis. Complex expressions corrupt registers.

**Target State:** Implement linear‑scan register allocation with liveness intervals. Spill to stack when registers exhausted.

**Files Changed:**
- `CodeGen/CodeGenerator.cs` — replace `_nextReg`/`AllocReg` with `LinearScanAllocator`
- New file: `CodeGen/RegisterAllocator.cs` — builds live intervals from basic blocks, assigns registers

**Effort:** ~800 LOC, ~12 hours

---

### TL‑14: No Optimizations

**Current State:** AST → code, no transforms.

**Target State:** Add optimization passes: constant folding (`1+2→3`), dead code elimination, common subexpression elimination.

**Files Changed:**
- New file: `CodeGen/Optimizer.cs` — AST‑visitor pattern, apply transforms
- `Program.cs` — `-O0`/`-O1`/`-O2` CLI flags

**Effort:** ~600 LOC, ~10 hours

---

### TL‑15: Caller‑Saved Regs Not Preserved

**Current State:** When function calls another, registers R0‑R5 may be clobbered. No save‑restore around CALL.

**Target State:** Before CALL, PUSH used registers; after CALL, POP them.

**Files Changed:**
- `CodeGen/CodeGenerator.cs` — `EmitFuncCall()`: save live registers

**Effort:** ~150 LOC, ~2 hours

---

### TL‑16: LIMM Word Reservation Broken

**Current State:** `EmitImm` emits `LIMM Rr, val` but doesn't reserve the next memory word for the value. The immediate value is placed in‑line but the next instruction's PC is not adjusted.

**Target State:** After `LIMM`, emit `.word val` directive in assembly. Assembler ensures the word is at PC+1.

**Effort:** ~100 LOC, ~2 hours

---

### TL‑17: No .map / Symbol Table

**Current State:** No output of global symbols.

**Target State:** `-map` flag: output text file listing function addresses and sizes.

**Effort:** ~200 LOC, ~3 hours

---

### TL‑18: No Source‑Location Error Messages

**Current State:** Exceptions thrown with generic messages: `"Expected Semicolon but got KwTint(tint) at 2:5"`. No file/line/column context.

**Target State:** Token carries `(line, column)` from lexer. Error messages format: `filename.t:line:col: error: expected ';' after expression`.

**Files Changed:**
- `Tokenizer.cs` — store `line, col` in each Token
- `Parser.cs` — `Error(string msg, Token? context)` helper
- `Program.cs` — catch errors, print formatted, return exit‑code 1

**Effort:** ~300 LOC, ~4 hours

---

### TL‑19: No Diagnostic Flags

**Current State:** 38 CS‑warnings. No project‑specific warnings.

**Target State:** `-Wall` (enable all), `-Wimplicit‑cast` (warn on implicit conversions), `-Werror` (treat warnings as errors).

**Effort:** ~200 LOC, ~3 hours

---

### TL‑20: No Optimization Flags

**Current State:** Only default codegen.

**Target State:** `-O0` (fast compile, no opts), `-O1` (constant folding, DCE), `-O2` (CSE, inlining).

**Effort:** ~150 LOC, ~2 hours

---

## 5. Fix Signatures & Code Changes

### 5.1 Processor Changes Summary

```
Files Modified:
  src/T3Simulator.Common/T3Config.cs            [+30 lines]
  src/T3Simulator.Common/ProcessorBase.cs       [+250 lines]
  src/T3Simulator.Common/Opcode.cs              [+20 lines]
  src/T3Simulator.InOrder/T3InOrderProcessor.cs [+600 lines]
  src/T3Simulator.Common/T3Fpu.cs               [+300 lines]

New Files:
  src/T3Simulator.InOrder/T3PipelineProcessor.cs [800 lines]
  docs/t3-isa-reference.md                      [revised spec]
```

### 5.2 Assembler Changes Summary

```
Files Modified:
  src/T3Assembler/T3AssemblerBase.cs            [+400 lines]
  src/T3Assembler/T3InOrderAssembler.cs         [+200 lines]

New Files:
  src/T3Assembler/MacroProcessor.cs             [300 lines]
  src/T3Assembler/ExpressionEvaluator.cs        [150 lines]
  src/T3Linker/T3Linker.cs                      [800 lines]
  src/T3Linker/ObjectFormat.cs                  [300 lines]
```

### 5.3 T‑lang Compiler Changes Summary

```
Files Modified:
  src/T3Compiler/Parser/Ast.cs                  [+50 lines]
  src/T3Compiler/Parser/Parser.cs               [+200 lines]
  src/T3Compiler/CodeGen/CodeGenerator.cs       [+600 lines]
  src/T3Compiler/Program.cs                     [+100 lines]
  src/T3Compiler/Lexer/Tokenizer.cs             [+20 lines]

New Files:
  src/T3Compiler/lib/tio.asm                    [200 lines]
  src/T3Compiler/lib/tmath.asm                  [300 lines]
  src/T3Compiler/CodeGen/RegisterAllocator.cs   [500 lines]
  src/T3Compiler/CodeGen/Optimizer.cs           [400 lines]
  src/T3Compiler/T3FloatConverter.cs            [150 lines]
```

---

## 6. Test Impact Analysis

### Phase 1 Tests (16 new tests)

| Test Name | Category | Expected Result |
|-----------|----------|----------------|
| `JLE_JumpConditional_LessOrEqual` | CPU‑05 | Jump taken when Cond ≤ 0 |
| `JGE_JumpConditional_GreaterOrEqual` | CPU‑05 | Jump taken when Cond ≥ 0 |
| `Macro_ExpandsWithArgs` | ASM‑01 | LI R0, 5 after expansion |
| `Macro_NestedMacros` | ASM‑01 | Nested expansion correct |
| `Expression_Arithmetic_LI` | ASM‑03 | LI R0, 2+3*4 → R0=14 |
| `Equ_Directive_SetsConstant` | ASM‑04 | .equ X, 42; LI R0, X → R0=42 |
| `Align_PadsWithNop` | ASM‑05 | .align 4 pads to boundary |
| `Org_SetsOrigin` | ASM‑06 | .org 0x100 places code at 256 |
| `Label_AsImmediate` | ASM‑11 | LI R0, main loads address |
| `ConditionalAssembly_Ifdef` | ASM‑10 | .ifdef DEBUG block assembled |
| `PrintInt_OutputsToPort0` | TL‑01 | print_int(42) → port 0 gets 42 |
| `FloatLiteral_LoadedViaFPU` | TL‑02 | tfloat x = 3.14 → FLW loads |
| `StringLiteral_StoresInMemory` | TL‑03 | return str → returns address |
| `SwitchCase_MatchesCorrectCase` | TL‑04 | switch(2) case 2: → matches |
| `ArrayInit_ListStore` | TL‑09 | int arr[3]={1,2,3} → arr[2]=3 |
| `DoWhile_LoopExecutes` | TL‑10 | do-while iterates N times |

### Phase 2 Tests (12 tests)

| Test Name | Category |
|-----------|----------|
| `RegisterWindow_SaveRestore` | CPU‑03 |
| `FpuInterrupt_OnOverflow` | CPU‑11 |
| `Atomic_LLSC_ReadWrite` | CPU‑07 |
| `Linker_MultiFile_ResolveSymbol` | ASM‑02 |
| `DebugInfo_LocMapping` | ASM‑07 |
| `Enum_Declaration_ValueAccess` | TL‑05 |
| `Cast_FtoI_Conversion` | TL‑06 |
| `Sizeof_ReturnsWordCount` | TL‑07 |
| `Typedef_AliasWorks` | TL‑08 |
| `TernaryExpr_TritValues` | TL‑11 |
| `Optimization_ConstantFold` | TL‑14 |
| `CallerSave_PreservesRegs` | TL‑15 |

---

## 7. Roadmap

### Phase 1 — Critical / Foundational (Weeks 1–4)

| Week | Deliverables | Tests Added |
|------|-------------|--------------|
| 1 | CPU: JLE/JGE, NOP, PUSHI (CPU‑05,08,09). ASM: expressions, .equ, .align, .org (ASM‑03‑06,10‑11). TL: switch, do‑while, array init (TL‑04,09,10) | 8 |
| 2 | TL: stdlib (TL‑01), float literals (TL‑02), strings (TL‑03) | 4 |
| 3 | TL: caller‑save (TL‑15), LIMM fix (TL‑16), error messages (TL‑18) | 2 |
| 4 | ASM: macros (ASM‑01), listing (ASM‑08), .include (ASM‑09) | 2 |

**Phase 1 total:** 16 new tests, ~3,200 LOC

### Phase 2 — Quality (Weeks 5–8)

| Week | Deliverables | Tests Added |
|------|-------------|--------------|
| 5‑6 | CPU: register windowing (CPU‑03). TL: register allocator (TL‑13). TL: optimizations (TL‑14) | 3 |
| 7 | ASM: linker (ASM‑02), debug info (ASM‑07). TL: enum, typedef, casts, sizeof (TL‑05‑08) | 6 |
| 8 | CPU: FPU interrupts (CPU‑11), rounding modes (CPU‑14). TL: ternary (TL‑11), goto (TL‑12), symbol table (TL‑17) | 3 |

**Phase 2 total:** 12 new tests, ~5,000 LOC

### Phase 3 — Systems (Weeks 9–12)

| Week | Deliverables |
|------|-------------|
| 9‑10 | CPU: interrupts (CPU‑01), privilege modes (CPU‑02) |
| 11‑12 | CPU: MMU (CPU‑04), atomic ops (CPU‑07), LI extended (CPU‑06) |

**Phase 3 total:** ~4,500 LOC

### Phase 4 — Performance (Weeks 13–16)

| Week | Deliverables |
|------|-------------|
| 13‑14 | CPU: pipeline (CPU‑15), shift‑through‑carry (CPU‑10) |
| 15‑16 | CPU: FPU FMA (CPU‑12), trig functions (CPU‑13) |

**Phase 4 total:** ~2,700 LOC

### Grand Total

| Metric | Value |
|--------|-------|
| Total LOC | ~15,400 |
| Total effort | ~170 hours (~4.5 weeks FTE) |
| New tests | 28 + 12 = 40 (plus existing 22) |
| New instructions | 15 (JLE, JGE, NOP, PUSHI, SAVE, RESTORE, INT, IRET, SVC, SETPRIV, TLBWR, TLBRD, TLBFL, SHLC, SHRC, LL, SC, FMADD, FMSUB, FSIN, FCOS, FTAN, FEXP, FLOG) |
| New FPU codes | 6 |
| New tools | 1 (T3Linker) |
| New source files | 15 |

---

## 8. Architecture Target State Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        T3 PROCESSOR TARGET                           │
├─────────────────────────────────────────────────────────────────────┤
│  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────────────┐ │
│  │ FETCH    │──▶│ DECODE   │──▶│ EXECUTE  │──▶│ MEMORY / WB     │ │
│  │ I‑Cache  │   │ 3‑wide   │   │ ALU+FPU  │   │ D‑Cache + MMU   │ │
│  │ 4KB      │   │          │   │ 27 regs  │   │ TLB 16 entries   │ │
│  └──────────┘   └─────────┘   └──────────┘   └──────────────────┘ │
│       ▲                                               │             │
│       │               ┌──────────────┐                │             │
│       └───────────────│ INTERRUPT    │◀───────────────┘             │
│                       │ CONTROLLER   │                               │
│                       │ 16 vectors   │                               │
│                       └──────────────┘                               │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │ SPECIAL REGISTERS: PC | SP | Cond | PR(9) | SR | FSR | WP   │   │
│  └──────────────────────────────────────────────────────────────┘   │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │ I/O PORTS (0‑255): stdout|stdin|stderr|timer|MMU|FPU|DMA    │   │
│  └────── smelled-only items -----------------------------------┘   │
└───────────────────────────────────────────────────────────────────┘
```

---

*Document version: 1.0 — Gap analysis & specification complete*