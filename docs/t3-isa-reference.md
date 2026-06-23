# T3 ISA Reference — v2 (Research Prototype)

## Instruction Format

```
[Pred (3)] [Opcode (6)] [Args (9)]
```

Registers encoded by trit value (-4..+4). Phys index = trit + 4.

| Name | Trit | Phys | FPU |
|------|------|------|-----|
| RW | -4 | 0 | FW |
| RX | -3 | 1 | FX |
| RY | -2 | 2 | FY |
| RZ | -1 | 3 | FZ |
| R0 | 0 | 4 | F0 |
| R1 | +1 | 5 | F1 |
| R2 | +2 | 6 | F2 |
| R3 | +3 | 7 | F3 |
| R4 | +4 | 8 | F4 |

## Opcode Table

### System (0-1)
| Op | Mnemonic | Type | Description |
|----|----------|------|-------------|
| 0 | HALT | – | Stop processor |
| 1 | NOP | – | No operation |

### Data Movement (2-5)
| Op | Mnemonic | Type | Description |
|----|----------|------|-------------|
| 2 | MOV | R | `op1 = op2` |
| 3 | MOVI | I | `op1 = imm` |
| 4 | LI | I | `op1 = imm` |
| 5 | LIMM | R | `op1 = mem[PC]; PC++` |

### Arithmetic (10-15 R, 20-25 I)
| Op | Mnemonic | Type |
|----|----------|------|
| 10 | ADD | R |
| 11 | SUB | R |
| 12 | MUL | R |
| 13 | DIV | R |
| 14 | MOD | R |
| 15 | NEG | R |
| 20 | ADDI | I |
| 21 | SUBI | I |
| 22 | MULI | I |
| 23 | DIVI | I |
| 24 | MODI | I |
| 25 | NEGI | I |

### Logical (30-35)
| Op | Mnemonic | Type |
|----|----------|------|
| 30 | AND | R |
| 31 | OR | R |
| 32 | XOR | R |
| 33 | ANDI | I |
| 34 | ORI | I |
| 35 | XORI | I |

### Shifts (40-43)
| Op | Mnemonic | Type |
|----|----------|------|
| 40 | SHL | R |
| 41 | SHR | R |
| 42 | SHLI | I |
| 43 | SHRI | I |

### Memory & Stack (50-55)
| Op | Mnemonic | Type |
|----|----------|------|
| 50 | LOAD | R |
| 51 | LOADI | I |
| 52 | STORE | R |
| 53 | STOREI | I |
| 54 | PUSH | R |
| 55 | POP | R |

### Control Flow (60-71)
| Op | Mnemonic | Type | Description |
|----|----------|------|-------------|
| 60 | CMP | R | `Cond = sign(op1 - op2)` |
| 61 | CMPI | I | `Cond = sign(op1 - imm)` |
| 62 | JMP | J | `PC = reg` |
| 63 | JE | J | Jump if Cond==0 |
| 64 | JNE | J | Jump if Cond!=0 |
| 65 | JL | J | Jump if Cond<0 |
| 66 | JG | J | Jump if Cond>0 |
| 67 | JM | J | Jump if Cond==0 |
| 68 | JLE | J | Jump if Cond<=0 |
| 69 | JGE | J | Jump if Cond>=0 |
| 70 | CALL | J | `Push PC; PC = reg` |
| 71 | RET | – | `PC = Pop()` |

### I/O (80-83)
| Op | Mnemonic | Type |
|----|----------|------|
| 80 | IN | R |
| 81 | OUT | R |
| 82 | INI | I |
| 83 | OUTI | I |

### FPU (100-116)
| Op | Mnemonic | Type | Description |
|----|----------|------|-------------|
| 100 | FADD | R | `Fop1 = Fop2 + Fop3` |
| 101 | FSUB | R | `Fop1 = Fop2 - Fop3` |
| 102 | FMUL | R | `Fop1 = Fop2 * Fop3` |
| 103 | FDIV | R | `Fop1 = Fop2 / Fop3` |
| 104 | FSQRT | R | `Fop1 = sqrt(Fop2)` |
| 105 | FABS | R | `Fop1 = abs(Fop2)` |
| 106 | FNEG | R | `Fop1 = -Fop2` |
| 107 | FCMP | R | `Cond = sign(Fop1 - Fop2)` |
| 108 | FTOI | R | `Rop1 = int(Fop2)` |
| 109 | ITOF | R | `Fop1 = float(Rop2)` |
| 110 | FTOF | R | tfloat ↔ tdouble |
| 111 | FLW | R | `Fop1 = mem[Rop2 + op3]` |
| 112 | FSW | R | `mem[Rop2 + op3] = Fop1` |
| 113 | FMOV | R | func:0=F→F,1=R→F,2=F→R |
| 114 | FCLASS | R | Classify Fop2 |
| 115 | FSWAP | R | Swap Fop1↔Fop2 |
| 116 | FZERO | I | `Fop1 = 0.0` |

## Predication

If pred > 0, instruction executes only if PR[pred-1] == +1.
PR register: 9 trits, three 3-trit predicate flags (p0, p1, p2).
*Note: The implementation of predication is currently under review for consistency between the specification and the execution engine.*

## Timing

| Instruction | Cycles |
|------------|--------|
| HALT, NOP, MOV, LI, NEG, CMP | 1 |
| LOAD, STORE, PUSH, POP, IN, OUT | 2 |
| ADD, SUB, SHL, SHR, AND, OR, XOR | 1 |
| MUL | 5 |
| DIV, MOD | 10 |
| JMP, JE, JNE, JL, JG | 1 (not taken) / 2 (taken) |
| CALL, RET | 2 |
| LIMM | 2 |
| FADD, FSUB | 5 |
| FMUL | 7 |
| FDIV | 15 |
| FSQRT | 20 |