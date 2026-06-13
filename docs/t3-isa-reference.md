# T3 Instruction Set Architecture — Reference

## 1. Overview

This document provides the complete instruction set reference for the T3 ternary processor. All instructions are encoded in a single **18‑trit word**.

### 1.1. Register Names

| Index | Name | Index | Name | Index | Name |
|-------|------|-------|------|-------|------|
| 0 | RW | 3 | RZ | 6 | R2 |
| 1 | RX | 4 | R0 | 7 | R3 |
| 2 | RY | 5 | R1 | 8 | R4 |

### 1.2. Instruction Formats

**R‑type (Register–Register):**
```
[ opcode+pred (6) | op1 (3) | op2 (3) | op3 (3) | reserve/func (3) ]
```

**I‑type (Register–Immediate):**
```
[ opcode+pred (6) | op1 (3) | op2 (3) | imm6 (6) ]
```

Where:
- `opcode+pred = base_opcode + pred_index × 28`
- `pred_index = (opcode+pred) / 28`
- `base_opcode = (opcode+pred) % 28`
- `op1, op2, op3` = register indices (0–8)
- `imm6` = signed immediate, range −364 … +364 (stored as unsigned: `imm6 + 364`)

### 1.3. Predication

If `pred_index ≠ 0`, the instruction executes only when `PR[pred_index − 1] == +1`.  
If the predicate fails, the instruction becomes a **NOP** (no state change, but PC advances).

---

## 2. Base Instructions (Codes 0–27, R‑type)

### 2.1. HALT — Halt Processor
| Field | Value |
|-------|-------|
| **Opcode** | 0 |
| **Type** | — |
| **Operands** | None (all fields ignored) |
| **Latency** | 1 |
| **Description** | Stops the processor. `Step()` returns `false`. |

---

### 2.2. LOAD — Load from Memory
| Field | Value |
|-------|-------|
| **Opcode** | 1 |
| **Type** | R |
| **Operands** | `LOAD op1, op2` |
| **Latency** | 2 |
| **Description** | `op1 = mem[op2]` — loads a word from the address in `op2` into register `op1`. `op3` is ignored. |

---

### 2.3. STORE — Store to Memory
| Field | Value |
|-------|-------|
| **Opcode** | 2 |
| **Type** | R |
| **Operands** | `STORE op1, op2` |
| **Latency** | 2 |
| **Description** | `mem[op2] = op1` — stores register `op1` to the address in `op2`. `op3` is ignored. |

---

### 2.4. MOV — Move Register
| Field | Value |
|-------|-------|
| **Opcode** | 3 |
| **Type** | R |
| **Operands** | `MOV op1, op2` |
| **Latency** | 1 |
| **Description** | `op1 = op2` — copies register `op2` to `op1`. `op3` is ignored. |

---

### 2.5. LI — Load Immediate
| Field | Value |
|-------|-------|
| **Opcode** | 4 |
| **Type** | I |
| **Operands** | `LI op1, imm6` |
| **Latency** | 1 |
| **Description** | `op1 = imm6` — loads the 6‑trit immediate value into `op1`. Always I‑type. |

**Encoding example:** `LI RW, 10`
- opcode+pred = 4, op1 = 0, imm6 = 10 → unsigned = 374 (10 + 364)
- Word = `4 × 3¹² + 0 × 3⁹ + 0 × 3⁶ + 374`

---

### 2.6. LIMM — Load Immediate from Memory
| Field | Value |
|-------|-------|
| **Opcode** | 5 |
| **Type** | R |
| **Operands** | `LIMM op1` |
| **Latency** | 2 |
| **Description** | `op1 = mem[PC]; PC += 1` — loads the next memory word into `op1`. `op2`, `op3` are ignored. |

---

### 2.7. ADD — Add
| Field | Value |
|-------|-------|
| **Opcode** | 6 |
| **Type** | R |
| **Operands** | `ADD op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 + op3` — balanced ternary addition. |

---

### 2.8. SUB — Subtract
| Field | Value |
|-------|-------|
| **Opcode** | 7 |
| **Type** | R |
| **Operands** | `SUB op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 − op3` — balanced ternary subtraction. |

---

### 2.9. MUL — Multiply
| Field | Value |
|-------|-------|
| **Opcode** | 8 |
| **Type** | R |
| **Operands** | `MUL op1, op2, op3` |
| **Latency** | 5 (T3‑18) / 8 (T3‑54) |
| **Description** | `op1 = op2 × op3` — balanced ternary multiplication. Result fits in word width (overflow truncated). |

---

### 2.10. DIV — Divide
| Field | Value |
|-------|-------|
| **Opcode** | 9 |
| **Type** | R |
| **Operands** | `DIV op1, op2, op3` |
| **Latency** | 10 (T3‑18) / 15 (T3‑54) |
| **Description** | `op1 = op2 / op3` — integer division, rounded toward −∞. Division by zero raises an exception. |

---

### 2.11. MOD — Modulo
| Field | Value |
|-------|-------|
| **Opcode** | 10 |
| **Type** | R |
| **Operands** | `MOD op1, op2, op3` |
| **Latency** | 10 (T3‑18) / 15 (T3‑54) |
| **Description** | `op1 = op2 % op3` — balanced ternary remainder. Division by zero raises an exception. |

---

### 2.12. NEG — Negate
| Field | Value |
|-------|-------|
| **Opcode** | 11 |
| **Type** | R |
| **Operands** | `NEG op1, op2` |
| **Latency** | 1 |
| **Description** | `op1 = −op2` — arithmetic negation (flip each trit: `-` ↔ `+`, `0` remains `0`). `op3` is ignored. |

---

### 2.13. TRITAND — Tritwise AND
| Field | Value |
|-------|-------|
| **Opcode** | 12 |
| **Type** | R |
| **Operands** | `TRITAND op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 & op3` — per‑trit minimum. For each trit position: result = min(trit_a, trit_b). |
| | Truth table: `- & - = -`, `- & 0 = -`, `- & + = -`, `0 & 0 = 0`, `0 & + = 0`, `+ & + = +` |

---

### 2.14. TRITOR — Tritwise OR
| Field | Value |
|-------|-------|
| **Opcode** | 13 |
| **Type** | R |
| **Operands** | `TRITOR op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 | op3` — per‑trit maximum. For each trit position: result = max(trit_a, trit_b). |
| | Truth table: `- \| - = -`, `- \| 0 = 0`, `- \| + = +`, `0 \| 0 = 0`, `0 \| + = +`, `+ \| + = +` |

---

### 2.15. TRITXOR — Tritwise XOR
| Field | Value |
|-------|-------|
| **Opcode** | 14 |
| **Type** | R |
| **Operands** | `TRITXOR op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 ^ op3` — per‑trit sum modulo 3. |
| | Truth table: `a ^ b = (a + b) mod 3`, mapped to {−1, 0, +1} |

---

### 2.16. SHL — Shift Left
| Field | Value |
|-------|-------|
| **Opcode** | 15 |
| **Type** | R |
| **Operands** | `SHL op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 << op3` — left shift by `op3` trits. Equivalent to multiplication by 3^op3. Zeros fill from right. |

---

### 2.17. SHR — Shift Right
| Field | Value |
|-------|-------|
| **Opcode** | 16 |
| **Type** | R |
| **Operands** | `SHR op1, op2, op3` |
| **Latency** | 1 |
| **Description** | `op1 = op2 >> op3` — arithmetic right shift by `op3` trits. The most-significant trit is replicated (sign extension). |

---

### 2.18. CMP — Compare
| Field | Value |
|-------|-------|
| **Opcode** | 17 |
| **Type** | R |
| **Operands** | `CMP op1, op2` |
| **Latency** | 1 |
| **Description** | `Cond = sign(op1 − op2)`. Sets the condition flag: `-` if op1 < op2, `0` if equal, `+` if op1 > op2. `op3` is ignored. |

---

### 2.19. JMP — Jump (Unconditional)
| Field | Value |
|-------|-------|
| **Opcode** | 18 |
| **Type** | R |
| **Operands** | `JMP op1` |
| **Latency** | 1 |
| **Description** | `PC = op1` — unconditional jump to the address in `op1`. |

---

### 2.20. JE — Jump if Equal
| Field | Value |
|-------|-------|
| **Opcode** | 19 |
| **Type** | R |
| **Operands** | `JE op1` |
| **Latency** | 1 (not taken) / 2 (taken) |
| **Description** | If `Cond == 0`, then `PC = op1`. |

---

### 2.21. JNE — Jump if Not Equal
| Field | Value |
|-------|-------|
| **Opcode** | 20 |
| **Type** | R |
| **Operands** | `JNE op1` |
| **Latency** | 1 (not taken) / 2 (taken) |
| **Description** | If `Cond != 0`, then `PC = op1`. |

---

### 2.22. JL — Jump if Less
| Field | Value |
|-------|-------|
| **Opcode** | 21 |
| **Type** | R |
| **Operands** | `JL op1` |
| **Latency** | 1 (not taken) / 2 (taken) |
| **Description** | If `Cond < 0`, then `PC = op1`. |

---

### 2.23. JG — Jump if Greater
| Field | Value |
|-------|-------|
| **Opcode** | 22 |
| **Type** | R |
| **Operands** | `JG op1` |
| **Latency** | 1 (not taken) / 2 (taken) |
| **Description** | If `Cond > 0`, then `PC = op1`. |

---

### 2.24. JM — Jump if Minus (Maybe)
| Field | Value |
|-------|-------|
| **Opcode** | 23 |
| **Type** | R |
| **Operands** | `JM op1` |
| **Latency** | 1 (not taken) / 2 (taken) |
| **Description** | If `Cond == 0`, then `PC = op1`. (Alias for JE in current specification.) |

---

### 2.25. CALL — Call Subroutine
| Field | Value |
|-------|-------|
| **Opcode** | 24 |
| **Type** | R |
| **Operands** | `CALL op1` |
| **Latency** | 2 |
| **Description** | `SP -= 1; mem[SP] = PC; PC = op1`. Pushes return address onto stack and jumps to `op1`. In T3‑54 with register windowing, also shifts the window: `WP -= 4`. |

---

### 2.26. RET — Return from Subroutine
| Field | Value |
|-------|-------|
| **Opcode** | 25 |
| **Type** | R |
| **Operands** | `RET` |
| **Latency** | 2 |
| **Description** | `PC = mem[SP]; SP += 1`. Pops return address from stack. In T3‑54 with register windowing: `WP += 4`. |

---

### 2.27. PUSH — Push onto Stack
| Field | Value |
|-------|-------|
| **Opcode** | 26 |
| **Type** | R |
| **Operands** | `PUSH op1` |
| **Latency** | 2 |
| **Description** | `SP -= 1; mem[SP] = op1`. Pushes register `op1` onto the stack. |

---

### 2.28. POP — Pop from Stack
| Field | Value |
|-------|-------|
| **Opcode** | 27 |
| **Type** | R |
| **Operands** | `POP op1` |
| **Latency** | 2 |
| **Description** | `op1 = mem[SP]; SP += 1`. Pops the top of the stack into register `op1`. |

---

## 3. I‑Type Variants (Codes 65–91)

I‑type instructions have `base_opcode = R_opcode + 64`. They use the I‑type format where `imm6` replaces `op3` and the 3‑trit reserve field.

| Code | Mnemonic | R‑Base | Operation |
|------|----------|--------|-----------|
| 65 | LOADI | LOAD (1) | `op1 = mem[op2 + imm6]` |
| 66 | STOREI | STORE (2) | `mem[op2 + imm6] = op1` |
| 67 | MOVI | MOV (3) | `op1 = imm6` (op2 ignored) |
| 70 | ADDI | ADD (6) | `op1 = op2 + imm6` |
| 71 | SUBI | SUB (7) | `op1 = op2 − imm6` |
| 72 | MULI | MUL (8) | `op1 = op2 × imm6` |
| 73 | DIVI | DIV (9) | `op1 = op2 / imm6` |
| 74 | MODI | MOD (10) | `op1 = op2 % imm6` |
| 75 | NEGI | NEG (11) | `op1 = −imm6` |
| 76 | TRITANDI | TRITAND (12) | `op1 = op2 & imm6` |
| 77 | TRITORI | TRITOR (13) | `op1 = op2 \| imm6` |
| 78 | TRITXORI | TRITXOR (14) | `op1 = op2 ^ imm6` |
| 79 | SHLI | SHL (15) | `op1 = op2 << imm6` |
| 80 | SHRI | SHR (16) | `op1 = op2 >> imm6` |
| 81 | CMPI | CMP (17) | `Cond = sign(op1 − imm6)` |

Codes 64, 69, and 82–91 are reserved for future extensions.

---

## 4. I/O Instructions (Codes 41–44)

| Code | Mnemonic | Type | Operation | Latency |
|------|----------|------|-----------|---------|
| 41 | IN | R | `op1 = port[op2]` — read from port (op2 = register with port number). Stalls if data not ready. | 2 |
| 42 | OUT | R | `port[op2] = op1` — write to port. | 2 |
| 43 | INI | I | `op1 = port[imm6]` — read from immediate port number. | 2 |
| 44 | OUTI | I | `port[imm6] = op1` — write to immediate port number. | 2 |

---

## 5. FPU Instructions (Codes 92–108)

FPU operates on registers **FW, FX, FY, FZ, F0, F1, F2, F3, F4** (indices 0–8).  
The `func` field (3 trits, occupying the reserve field in R‑type) selects the data type or sub‑operation.

| Code | Mnemonic | Type | Operation | Latency |
|------|----------|------|-----------|---------|
| 92 | FADD | R | `Fop1 = Fop2 + Fop3` | 5 |
| 93 | FSUB | R | `Fop1 = Fop2 − Fop3` | 5 |
| 94 | FMUL | R | `Fop1 = Fop2 × Fop3` | 7 |
| 95 | FDIV | R | `Fop1 = Fop2 / Fop3` | 15 |
| 96 | FSQRT | R | `Fop1 = sqrt(Fop2)` (op3 ignored) | 20 |
| 97 | FABS | R | `Fop1 = abs(Fop2)` (op3 ignored) | 1 |
| 98 | FNEG | R | `Fop1 = −Fop2` (op3 ignored) | 1 |
| 99 | FCMP | R | `Cond = sign(Fop1 − Fop2)` (op3 ignored) | 1 |
| 100 | FTOI | R | `Rop1 = int(Fop2)`. `func`: 0 = 18‑trit, 1 = 54‑trit integer. Round toward zero. | 3 |
| 101 | ITOF | R | `Fop1 = float(Rop2)`. `func` selects integer width. | 3 |
| 102 | FTOF | R | Convert tfloat ↔ tdouble. `func`: 0 = tfloat→tdouble, 1 = tdouble→tfloat. | 2 |
| 103 | FLW | I | `Fop1 = mem[Rop2 + imm6]` — FPU load from memory. | 2 |
| 104 | FSW | I | `mem[Rop2 + imm6] = Fop1` — FPU store to memory. | 2 |
| 105 | FMOV | R | Move: `func=0`: `Fop1 = Fop2`; `func=1`: `Rop1 = Fop2`; `func=2`: `Fop1 = Rop2`. | 1 |
| 106 | FCLASS | R | Classify `Fop2` → `Fop1`: 0=zero, 1=∞, 2=NaN, 3=maybe‑NaN, 4=normal. | 1 |
| 107 | FSWAP | R | Swap `Fop1` and `Fop2`. | 1 |
| 108 | FZERO | R | `Fop1 = 0.0` | 1 |

FPU exceptions set flags in **FSR** (port 0x20) and write special values (∞, 0, NaN) to the destination. No interrupts are generated.

---

## 6. VLIW Instructions (T3‑54 only, Codes 28–40)

| Code | Mnemonic | Description |
|------|----------|-------------|
| 28 | SPEK | Begin speculative execution: save register snapshot, redirect stores to buffer |
| 29 | COMMIT | Commit speculation: apply buffered stores, discard shadow state |
| 30 | ROLLBACK | Abort speculation: restore registers from snapshot, discard stores |
| 31 | VADD3 | Element-wise addition of three 18‑trit fields |
| 32 | VSUB3 | Element-wise subtraction of three 18‑trit fields |
| 33 | VMUL3 | Element-wise multiplication of three 18‑trit fields |
| 34 | VDOT3 | Dot product of three-element vectors |
| 35 | VCMP | Element-wise comparison (sets Cond per field) |
| 36 | VTRITAND3 | Element-wise tritwise AND |
| 37 | VTRITOR3 | Element-wise tritwise OR |
| 38 | VTRITXOR3 | Element-wise tritwise XOR |
| 39 | VSHL3 | Element-wise shift left |
| 40 | VSHR3 | Element-wise shift right |

---

## 7. Opcode Distribution Map

| Range | Purpose |
|-------|---------|
| 0–27 | Base instructions (R‑type) |
| 28–40 | VLIW extensions (T3‑54 only) |
| 41–44 | I/O instructions (IN, OUT, INI, OUTI) |
| 45–62 | Reserved |
| 63 | NOP (reserved encoding) |
| 64–91 | I‑type variants of base instructions |
| 92–108 | FPU instructions |
| 109–127 | Reserved for future extensions |

---

## 8. Encoding Examples

### ADD R0, R1, R2 (R‑type)
- `base_opcode = 6`, `pred_index = 0` → `opcode+pred = 6`
- `op1 = 4` (R0), `op2 = 5` (R1), `op3 = 6` (R2), `reserve = 0`
- Word = `6 × 3¹² + 4 × 3⁹ + 5 × 3⁶ + 6 × 3³ + 0`

### ADDI R0, R1, 100 (I‑type)
- `base_opcode = 70`, `pred_index = 0` → `opcode+pred = 70`
- `op1 = 4` (R0), `op2 = 5` (R1), `imm6 = 100` → unsigned = `464` (100 + 364)
- Word = `70 × 3¹² + 4 × 3⁹ + 5 × 3⁶ + 464`

### LI RW, 10
- `base_opcode = 4`, `op1 = 0` (RW), `imm6 = 10` → unsigned = `374`
- Word = `4 × 3¹² + 0 × 3⁹ + 0 × 3⁶ + 374`

### INI RW, 7
- `base_opcode = 43`, `op1 = 0` (RW), `imm6 = 7` → unsigned = `371`
- Word = `43 × 3¹² + 0 × 3⁹ + 0 × 3⁶ + 371`

### FADD FW, FX, FY (tfloat)
- `base_opcode = 92`, `op1 = 0` (FW), `op2 = 1` (FX), `op3 = 2` (FY), `func = 0`
- Word = `92 × 3¹² + 0 × 3⁹ + 1 × 3⁶ + 2 × 3³ + 0`

---

## 9. Predication Encoding

Each instruction can be predicated by setting `pred_index` (0–9 for T3‑54, 0–3 for T3‑18):

| pred_index | Predicate Flag | Effect |
|------------|---------------|--------|
| 0 | None | Always execute |
| 1 | PR[0] (p0) | Execute if `PR[0] == +1` |
| 2 | PR[1] (p1) | Execute if `PR[1] == +1` |
| 3 | PR[2] (p2) | Execute if `PR[2] == +1` |
| 4–9 | PR[3]–PR[8] | T3‑54 only |

Predicate flags are set by comparison instructions (`CMP`, `FCMP`, `VCMP`) or by direct writes to the `PR` register.

---

## 10. Assembly Syntax

```
; Comments start with semicolon

; Labels
main:
    LI RW, 10          ; Load immediate
    LI RX, 20
    ADD RY, RW, RX     ; RY = RW + RX
    CMP RY, RW         ; Compare: Cond = sign(RY - RW)
    JG  greater        ; Jump if Cond > 0
    MOV RZ, RW         ; else: RZ = RW
    JMP done
greater:
    MOV RZ, RY         ; RZ = RY
done:
    HALT
```

### VLIW Bundle Syntax (T3‑54)

```
{ ADD R0, R1, R2 | MUL R3, R4, R5 | LOAD R6, R7 }
;                            ^                   ^
;     Slot 0                   Slot 1    Slot 2
```

Slots are separated by `|`. Unused slots are automatically filled with `NOP`.

---

*Document version: 1.0*