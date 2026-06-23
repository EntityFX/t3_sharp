# T3 Processor Architecture — v2 (Research Prototype)

## Word Types

- **Word18**: 18 trits, stored as `int` (32 bits). Range: ±193,710,244
- **Word54**: 54 trits, stored as `Int128`. Range: ±2.9×10²⁵
- **T3Float**: 18-trit float (6 exponent + 12 mantissa)

## Register File (9 registers)

| Name | Trit val | Phys | FPU |
|------|----------|------|-----|
| RW | -4 | 0 | FW |
| RX | -3 | 1 | FX |
| RY | -2 | 2 | FY |
| RZ | -1 | 3 | FZ |
| R0 | 0 | 4 | F0 |
| R1 | +1 | 5 | F1 |
| R2 | +2 | 6 | F2 |
| R3 | +3 | 7 | F3 |
| R4 | +4 | 8 | F4 |

Special registers: SP, PC, Cond (1 trit), PR (9 trits = 3×3 predicate flags). 
*Note: Register windowing is currently a planned architectural feature and not yet integrated into the execution model.*

## Instruction Format

```
[Pred (3)] [Opcode (6)] [Args (9)]
```

- R-type: Args = `[Op1(3)] [Op2(3)] [Op3(3)]`
- I-type: Args = `[Op1(3)] [Imm(6)]`
- J-type: Args = `[Reg(3)] [000000]`

Registers encoded by trit value (-4..+4). Phys index = trit + 4.

## Memory

- 1M words (1,048,576) of Word18/Word54
- Stack grows downward, SP starts at MemSize-1
- MMIO: CYCLE_LOW, CYCLE_HIGH, INST_COUNT, STALL_COUNT

## Execution Model (In-Order Prototype)

The current implementation is a functional interpreter rather than a cycle-accurate pipeline.

1. Fetch: `word = Memory[PC]`
2. Decode: extract Pred, Opcode, Args
3. Predicate: skip if predicate condition is not met (Implementation under review for consistency)
4. Execute: switch on Opcode
5. PC advance (unless branch)

## Opcode Table

See [t3-isa-reference.md](t3-isa-reference.md) for full ISA.