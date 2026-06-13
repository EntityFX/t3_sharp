# T3 Ternary Processor — Architecture Reference

## 1. Introduction

The **T3** processor family is a set of ternary computing cores sharing a unified instruction set architecture (ISA), operating in the **balanced ternary numeral system**. Each digit (**trit**) takes one of three values: **−1**, **0**, or **+1**, represented by the symbols `-`, `0`, and `+`.

The architecture supports two word-size configurations:

| Configuration | Word Width | VLIW | Use Case |
|---------------|------------|------|----------|
| **T3‑18** | 18 trits (3 trytes) | No | Learning, small embedded |
| **T3‑54** | 54 trits (9 trytes) | Yes | High-performance computing |

T3‑54 includes a VLIW microarchitecture with predication, speculative execution, and SIMD operations — three 18‑trit slots packed into one 54‑trit word.

---

## 2. Data Types and Formats

### 2.1. Trit, Tryte, Word

| Unit | Width | Range | Storage (C#) |
|------|-------|-------|--------------|
| **Trit** | 1 | −1, 0, +1 | `sbyte` |
| **Tryte** | 6 trits | −364 … +364 | `short` (9 bits) |
| **Word18** | 18 trits | ±193,710,244 | `long` / `int` |
| **Word54** | 54 trits | ±2.9 × 10²⁵ | `BigInteger` / `Int128` |

### 2.2. Number Representation

Numbers are stored in balanced ternary. Negative values are represented naturally — the most-significant trit is `-`.  
For example, the decimal value −5 in 6‑trit representation is `- + 0 -` (since −9 + 3 + 0 + 1 = −5).

---

## 3. Register Model

### 3.1. General-Purpose Registers

Nine logical registers: **RW, RX, RY, RZ, R0, R1, R2, R3, R4** (indices 0–8).

| Index | Name | Purpose |
|-------|------|---------|
| 0 | RW | General purpose / workspace |
| 1 | RX | General purpose |
| 2 | RY | General purpose |
| 3 | RZ | General purpose |
| 4 | R0 | General purpose |
| 5 | R1 | General purpose |
| 6 | R2 | General purpose |
| 7 | R3 | General purpose |
| 8 | R4 | General purpose |

Each register has the word width of the configuration (18 trits in T3‑18, 54 trits in T3‑54).

### 3.2. Special Registers

| Register | Width | Description |
|----------|-------|-------------|
| **SP** | Word | Stack pointer. Points to the top of the stack. Initialized to `MemSize − 1`. |
| **PC** | Word | Program counter. Points to the next instruction to execute. |
| **Cond** | 1 trit | Condition flag. Set by `CMP`, `FCMP`. Values: `-` (less), `0` (equal), `+` (greater). |
| **PR** | 9 / 27 trits | Predicate register. T3‑18: 3 flags × 3 trits each (p0, p1, p2). T3‑54: 9 flags. |

### 3.3. Register Windowing (T3‑54 Extended)

In the T3‑54 VLIW configuration, the processor may implement a **register window** mechanism with 27 physical registers and a `WP` (Window Pointer). `CALL` decrements `WP` by 4, exposing fresh registers for the callee; `RET` restores the previous window.

---

## 4. Memory and Addressing

### 4.1. Memory Layout

| Address Range | Contents |
|---------------|----------|
| `0x00000` – `0xFFFFE` | Program and data memory (1M words) |
| `0x3FFFF00` | `CYCLE_LOW` — cycle counter (read/write; write resets all counters) |
| `0x3FFFF01` | `INST_COUNT` — executed instructions/bundles |
| `0x3FFFF02` | `STALL_COUNT` — stall cycles |
| `0x3FFFF10` | `TIMER_CTRL` — timer control |
| `0x3FFFF11` | `TIMER_CMP` — timer compare value |

- Memory is **word-addressed**. Each address holds one word (18 or 54 trits).
- Maximum physical memory in the simulator: **1M words** (1,048,576).
- Stack grows **downward**; `SP` points to the top element.

### 4.2. Addressing Modes

| Mode | Syntax Example | Description |
|------|----------------|-------------|
| **Register** | `ADD RW, RX` | Operands in registers |
| **Immediate** | `LI RW, 100t` | Small constant in instruction (I‑type, imm6) |
| **Indirect** | `LOAD RW, RX` | Address held in register RX |
| **Long Immediate** | `LIMM RW, 1000000t` | Value in the next memory word |

---

## 5. Instruction Formats

All instructions occupy exactly **one 18‑trit word**.

### 5.1. R‑Type (Register–Register)

```
[ opcode+pred (6) | op1 (3) | op2 (3) | op3 (3) | reserve/func (3) ]
```

| Field | Width | Description |
|-------|-------|-------------|
| `opcode+pred` | 6 trits | `base_opcode + pred_index × 28` |
| `op1` | 3 trits | Destination register (0–8) |
| `op2` | 3 trits | Source register 1 (0–8) |
| `op3` | 3 trits | Source register 2 (0–8) |
| `reserve/func` | 3 trits | Reserved (0) or function code for FPU |

**Decoding:**
```
pred_index = (opcode+pred) / 28
base_opcode = (opcode+pred) % 28
```

### 5.2. I‑Type (Register–Immediate)

```
[ opcode+pred (6) | op1 (3) | op2 (3) | imm6 (6) ]
```

| Field | Width | Description |
|-------|-------|-------------|
| `opcode+pred` | 6 trits | Same encoding as R‑type, but base opcode ≥ 64 for I‑variants |
| `op1` | 3 trits | Destination register |
| `op2` | 3 trits | Source register (ignored for pure immediate loads) |
| `imm6` | 6 trits | Signed immediate value: −364 … +364 |

### 5.3. VLIW Slot (T3‑54 only)

Each slot within a 54‑trit VLIW bundle uses an 18‑trit format:

```
[ opcode+pred (6) | op1 (6) | op2 (6) ]
```

- 6‑trit operands: values 0–8 refer to registers; values 9+ are immediate constants (−364…+364).

---

## 6. Microarchitectures

### 6.1. In‑Order Processor (T3‑18, T3‑54)

- **Single ALU**, sequential execution.
- No pipeline, no speculation, no SIMD.
- Predication is emulated (NOP if predicate fails).
- Instructions with codes 28–44 are **invalid** in T3‑18.

**Instruction Latencies (cycles):**

| Instruction | T3‑18 | T3‑54 |
|-------------|-------|-------|
| HALT, MOV, LI, NEG, TRITAND, TRITOR, TRITXOR, CMP | 1 | 1 |
| LOAD, STORE | 2 | 2 |
| ADD, SUB, SHL, SHR | 1 | 1 |
| MUL | 5 | 8 |
| DIV, MOD | 10 | 15 |
| JMP | 1 | 1 |
| JE, JNE, JL, JG, JM (not taken / taken) | 1 / 2 | 1 / 2 |
| CALL, RET | 2 | 2 |
| PUSH, POP | 2 | 2 |
| LIMM | 2 | 2 |
| IN, OUT, INI, OUTI | 2 | 2 |

### 6.2. VLIW Processor (T3‑54 only)

- **Three identical ALUs**.
- **Bundle** = three 18‑trit slots packed in one 54‑trit word.
- Parallel execution with conflict constraints.

**VLIW Rules:**
- No two slots may write to the same register.
- At most **one memory access** per bundle (priority: slot 0 > 1 > 2).
- At most **one branch** per bundle.
- **Speculation**: shadow register file + deferred store buffer with `SPEK`/`COMMIT`/`ROLLBACK` instructions.
- **SIMD** instructions (VADD3, VSUB3, VMUL3, etc.) treat each 54‑trit word as three independent 18‑trit fields.

---

## 7. Speculation Mechanism (VLIW)

| Instruction | Description |
|-------------|-------------|
| **SPEK** | Begin speculative region: save register snapshot, redirect stores to buffer |
| **COMMIT** | Commit speculation: apply buffered stores, discard shadow state |
| **ROLLBACK** | Abort speculation: restore registers from snapshot, discard buffered stores |

Speculation allows the processor to execute code ahead of a predicted branch. If the prediction is correct, results are committed; otherwise, state is rolled back.

---

## 8. SIMD Operations (VLIW, T3‑54)

SIMD instructions operate on three 18‑trit sub‑words packed in a 54‑trit word:

| Mnemonic | Description |
|----------|-------------|
| VADD3 | Element-wise addition of three 18‑trit fields |
| VSUB3 | Element-wise subtraction |
| VMUL3 | Element-wise multiplication |
| VDOT3 | Dot product of three-element vectors |
| VCMP | Element-wise comparison |
| VTRITAND3 | Element-wise tritwise AND (minimum) |
| VTRITOR3 | Element-wise tritwise OR (maximum) |
| VTRITXOR3 | Element-wise tritwise XOR (sum mod 3) |
| VSHL3 | Element-wise shift left |
| VSHR3 | Element-wise shift right |

---

## 9. FPU (Floating-Point Unit)

### 9.1. Formats

| Format | Width | Exponent | Mantissa | Bias |
|--------|-------|----------|----------|------|
| **tfloat** | 18 trits | 6 trits | 12 trits (hidden trit) | 182 |
| **tdouble** | 36 trits | 8 trits | 28 trits (hidden trit) | 3280 |

Special values: zero, infinity (+∞, −∞), NaN, maybe‑NaN.

Default rounding: **round to nearest, ties to even least-significant trit**.

### 9.2. FPU Registers

Nine data registers: **FW, FX, FY, FZ, F0, F1, F2, F3, F4** (indices 0–8).

- **tfloat mode**: each register holds 18 trits.
- **tdouble mode**: register pairs are used — (FW, FX), (FY, FZ), (F0, F1), (F2, F3). F4 is unused in pairs.

**FSR** (Floating-Point Status Register) accessible via port `0x20`:

| Bit | Flag | Description |
|-----|------|-------------|
| 0 | I (Invalid) | Invalid operation (e.g., sqrt of negative) |
| 1 | O (Overflow) | Result magnitude too large |
| 2 | U (Underflow) | Result magnitude too small |
| 3 | Z (DivZero) | Division by zero |
| 4 | M (Maybe‑NaN) | Result is maybe‑NaN |

Exceptions do **not** trigger interrupts; the appropriate special value (∞, 0, NaN) is written to the destination register.

---

## 10. I/O Subsystem

### 10.1. Port Address Space

18‑trit port address space: up to 262,144 ports.

| Port | Device |
|------|--------|
| 0 | stdout |
| 1 | stdin |
| 2 | stderr |
| 3–7 | Free / user-defined |
| 0x10–0x1F | Timer / MMIO control |
| 0x20 | FPU status register (FSR) |

### 10.2. I/O Instructions

| Mnemonic | Type | Description |
|----------|------|-------------|
| IN | R | `op1 = port[op2]` — read from port (op2 is register with port number) |
| OUT | R | `port[op2] = op1` — write to port |
| INI | I | `op1 = port[imm6]` — read from immediate port |
| OUTI | I | `port[imm6] = op1` — write to immediate port |

If a port is not ready, `IN` causes a stall until data becomes available.

---

## 11. T‑SCII Character Encoding

T‑SCII (Ternary Standard Code for Information Interchange) is a 6‑trit (tryte) encoding compatible with CP‑1251.

### 11.1. Code Ranges

| Value (V) | Unsigned (U) | Content |
|-----------|---------------|---------|
| −364 … −1 | 0 … 363 | Greek alphabet, pseudographics, arrows, math symbols |
| **0 … 255** | **364 … 619** | **CP‑1251 compatible (ASCII + Cyrillic + special chars)** |
| 256 … 364 | 620 … 728 | Mathematical and technical symbols |

### 11.2. 9‑ary Representation (pairs of trits)

| Pair | Symbol | Pair | Symbol | Pair | Symbol |
|------|--------|------|--------|------|--------|
| `- -` | W | `0 -` | Z | `+ -` | 2 |
| `- 0` | X | `0 0` | 0 | `+ 0` | 3 |
| `- +` | Y | `0 +` | 1 | `+ +` | 4 |

A tryte is represented by three 9‑ary digits: `0n` prefix + 3 characters (e.g., `0n1Y2` for 'A').

### 11.3. 27‑ary Representation (triplets of trits)

| Triplet | Sym | Triplet | Sym | Triplet | Sym |
|---------|-----|---------|-----|---------|-----|
| `- - -` | N | `0 - -` | W | `+ - -` | 5 |
| `- - 0` | O | `0 - 0` | X | `+ - 0` | 6 |
| `- - +` | P | `0 - +` | Y | `+ - +` | 7 |
| `- 0 -` | Q | `0 0 -` | Z | `+ 0 -` | 8 |
| `- 0 0` | R | `0 0 0` | 0 | `+ 0 0` | 9 |
| `- 0 +` | S | `0 0 +` | 1 | `+ 0 +` | A |
| `- + -` | T | `0 + -` | 2 | `+ + -` | B |
| `- + 0` | U | `0 + 0` | 3 | `+ + 0` | C |
| `- + +` | V | `0 + +` | 4 | `+ + +` | D |

A tryte is represented by two 27‑ary digits: `0y` prefix + 2 characters (e.g., `0y2B` for 'A').

---

## 12. Timing Model

- **Clock frequency**: 1 GHz (1 cycle = 1 ns).
- **Hardware counters** mapped to MMIO addresses (see §4.1).
- Writing to `CYCLE_LOW` resets all counters.
- In T3‑54, counters are 54‑trit wide.

---

## 13. Exception Handling

| Condition | Behavior |
|-----------|----------|
| Invalid opcode (28–44 in T3‑18) | Processor exception (halt with error) |
| Division by zero (integer) | Processor exception |
| FPU exceptions | Set FSR flags, return special value (no interrupt) |
| Stack overflow | Implementation-defined (typically wraps or halts) |
| Memory access out of bounds | Processor exception |

---

## 14. References

- [Balanced Ternary on Wikipedia](https://en.wikipedia.org/wiki/Balanced_ternary)
- [Setun Computer](https://en.wikipedia.org/wiki/Setun) — Moscow State University, 1958
- Knuth, D.E. *The Art of Computer Programming*, Vol. 2: Seminumerical Algorithms
- Fowler, T. (1840) — originator of the balanced ternary concept

---

*Document version: 1.0 — Based on T3 processor specification v2 (18/54‑trit architecture)*