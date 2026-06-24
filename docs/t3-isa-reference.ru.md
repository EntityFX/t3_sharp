# Справочник ISA процессора T3 — v2 (Исследовательский прототип)

## Формат инструкции

```
[Pred (3)] [Opcode (6)] [Args (9)]
```

Все поля хранятся в 18-тритном слове. Поля Pred и Opcode хранятся как raw unsigned (беззнаковые целые). Поля аргументов хранятся как signed balanced ternary (сбалансированные троичные) со смещением.

### Детали кодирования полей

| Поле | Позиция (LSB) | Ширина (триты) | Тип | Диапазон |
|------|---------------|----------------|-----|----------|
| Pred | 15 | 3 | Raw unsigned | 0..13 |
| Opcode | 9 | 6 | Raw unsigned | 0..364 |
| Args | 0 | 9 | Raw unsigned → sub-fields | 0..9841 |

**Args sub-fields**:
- **R-type**: `[Op1(3)] [Op2(3)] [Op3(3)]` — каждый balanced, диапазон ±13 (Phys index = value + 4)
- **I-type**: `[Op1(3)] [Imm(6)]` — Op1 balanced (±13), Imm balanced (±364)
- **J-type**: `[Reg(3)] [000000]` — Reg balanced (±13), 6 тритов padding (всегда 0)

### Encoder (InstructionEncoder)

```csharp
// Все signed значения конвертируются в unsigned через ToUnsignedField:
// unsigned = signed + offset, где offset = (3^width - 1) / 2
long EncodeR(int pred, int opcode, int op1, int op2, int op3)
long EncodeI(int pred, int opcode, int op1, long imm)
long EncodeJ(int pred, int opcode, int reg)
```

**ToUnsignedField**: `value + offset` где `offset = (3^width - 1) / 2`.
- Для 3 тритов: offset = 13, диапазон signed: ±13 → unsigned: 0..26
- Для 6 тритов: offset = 364, диапазон signed: ±364 → unsigned: 0..728 (I-type imm, LI imm)

### Decoder (InstructionDecoder)

```csharp
DecodedInstruction Decode(Word18 word)
DecodedInstruction Decode(Word54 word)  // использует Word18.FromWrappedLong()
```

**Процесс декодирования**:
1. Извлечь Pred: `ExtractRawField(word, 15, 3)` — raw unsigned
2. Извлечь Opcode: `ExtractRawField(word, 9, 6)` — raw unsigned
3. Извлечь Args: `ExtractRawField(word, 0, 9)` — raw unsigned
4. В зависимости от типа (R/I/J):
   - **R-type**: Извлечь Op1/Op2/Op3 из Args как raw unsigned (3 трита каждый), затем конвертировать в balanced: `value - 13`
   - **I-type**: Извлечь Op1 (3 трита) и Imm (6 тритов) из Args, конвертировать в balanced: Op1 -= 13, Imm -= 364
   - **J-type**: Извлечь Reg (3 трита) из Args, конвертировать в balanced: Reg -= 13. Imm = 0 (padding)

**Важно**: Для J-type imm всегда равен 0. 6 тритов padding в младших разрядах Args гарантированно равны 0, но decoder устанавливает imm = 0 явно.

### ExtractRawField и ExtractBalancedField

```csharp
// Извлекает raw unsigned значение поля: (value / 3^startPos) % 3^width
Int128 ExtractRawField(Int128 value, int startPos, int width)

// Извлекает balanced значение поля: ExtractRawField(...) - (3^width - 1) / 2
Int128 ExtractBalancedField(Int128 value, int startPos, int width)
```

**ExtractBalancedField** предназначен для полей, которые были закодированы как unsigned (value + offset). Он НЕ подходит для извлечения полей из сбалансированного троичного числа, где поля хранятся как signed значения (например, T3Float).

### ExtractBalancedTrit

```csharp
int ExtractBalancedTrit(Int128 value, int position)
```

Извлекает один сбалансированный трит из значения на заданной позиции (0 = LSB). Использует алгоритм сбалансированного троичного переноса:
- Итерирует от позиции 0 до запрошенной позиции
- На каждом шаге: `rem = remaining % 3`
  - rem == 2 → trit = -1, remaining = (remaining + 1) / 3 (перенос +1)
  - rem == -2 → trit = +1, remaining = (remaining - 1) / 3 (перенос -1)
  - rem == 1 → trit = +1, remaining = (remaining - 1) / 3
  - rem == -1 → trit = -1, remaining = (remaining + 1) / 3
  - rem == 0 → trit = 0, remaining = remaining / 3

**Важно**: Эта функция корректно обрабатывает переносы (carry propagation), в отличие от старой реализации, которая использовала `(value / 3^position) % 3` и не учитывала переносы из младших позиций.

## Регистры

| Имя | Трит | Phys | FPU |
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

## Таблица опкодов

### Системные (0-1)
| Код | Мнемоника | Тип | Описание |
|-----|-----------|-----|----------|
| 0 | HALT | – | Останов процессора |
| 1 | NOP | – | Нет операции |

### Перемещение данных (2-5)
| Код | Мнемоника | Тип | Описание |
|-----|-----------|-----|----------|
| 2 | MOV | R | `op1 = op2` |
| 3 | MOVI | I | `op1 = imm` |
| 4 | LI | I | `op1 = imm` |
| 5 | LIMM | R | `op1 = mem[PC]; PC++` |

### Арифметика (10-15 R, 20-25 I)
| Код | Мнемоника | Тип |
|-----|-----------|------|
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

### Логические (30-35)
| Код | Мнемоника | Тип |
|-----|-----------|------|
| 30 | AND | R |
| 31 | OR | R |
| 32 | XOR | R |
| 33 | ANDI | I |
| 34 | ORI | I |
| 35 | XORI | I |

### Сдвиги (40-43)
| Код | Мнемоника | Тип |
|-----|-----------|------|
| 40 | SHL | R |
| 41 | SHR | R |
| 42 | SHLI | I |
| 43 | SHRI | I |

### Память и стек (50-55)
| Код | Мнемоника | Тип |
|-----|-----------|------|
| 50 | LOAD | R |
| 51 | LOADI | I |
| 52 | STORE | R |
| 53 | STOREI | I |
| 54 | PUSH | R |
| 55 | POP | R |

### Управление (60-71)
| Код | Мнемоника | Тип | Описание |
|-----|-----------|-----|----------|
| 60 | CMP | R | `Cond = sign(op1 - op2)` |
| 61 | CMPI | I | `Cond = sign(op1 - imm)` |
| 62 | JMP | J | `PC = reg` |
| 63 | JE | J | Переход если Cond==0 |
| 64 | JNE | J | Переход если Cond!=0 |
| 65 | JL | J | Переход если Cond<0 |
| 66 | JG | J | Переход если Cond>0 |
| 67 | JM | J | Переход если Cond==0 |
| 68 | JLE | J | Переход если Cond<=0 |
| 69 | JGE | J | Переход если Cond>=0 |
| 70 | CALL | J | `Push PC; PC = reg` |
| 71 | RET | – | `PC = Pop()` |

### Ввод-вывод (80-83)
| Код | Мнемоника | Тип |
|-----|-----------|------|
| 80 | IN | R |
| 81 | OUT | R |
| 82 | INI | I |
| 83 | OUTI | I |

### FPU (100-116)
| Код | Мнемоника | Тип | Описание |
|-----|-----------|-----|----------|
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
| 114 | FCLASS | R | Классификация Fop2 |
| 115 | FSWAP | R | Обмен Fop1↔Fop2 |
| 116 | FZERO | I | `Fop1 = 0.0` |

## Предикация

Если pred > 0, инструкция выполняется только если PR[pred-1] == +1.
PR регистр: 9 тритов, три 3-тритных предикатных флага.
*Примечание: предикация выполняется на основе флагов PR[0..2]. Инструкция выполняется, если соответствующий флаг равен +1.*

## Задержки инструкций (такты)

| Инструкция | Тактов |
|-----------|--------|
| HALT, NOP, MOV, LI, NEG, CMP | 1 |
| LOAD, STORE, PUSH, POP, IN, OUT | 2 |
| ADD, SUB, SHL, SHR, AND, OR, XOR | 1 |
| MUL | 5 |
| DIV, MOD | 10 |
| JMP, JE, JNE, JL, JG | 1 (нет) / 2 (переход) |
| CALL, RET | 2 |
| LIMM | 2 |
| FADD, FSUB | 5 |
| FMUL | 7 |
| FDIV | 15 |
| FSQRT | 20 |