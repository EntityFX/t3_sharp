# T3 ISA Reference — v5 (Research Prototype)

## Instruction Format

```
[Pred (3)] [RegGroup (1)] [Fmt (1)] [Opcode (4)] [Args (9)]
```

Все поля хранятся в 18-тритном слове. Поля Pred, RegGroup, Fmt и Opcode хранятся как raw unsigned (беззнаковые целые). Поля аргументов хранятся как signed balanced ternary (сбалансированные троичные) со смещением.

### Детали кодирования полей

| Поле | Позиция (LSB) | Ширина (триты) | Тип | Диапазон |
|------|---------------|----------------|-----|----------|
| Args | 0 | 9 | Raw unsigned → sub-fields | 0..19682 |
| Opcode | 9 | 4 | Raw unsigned | 0..80 |
| Fmt | 13 | 1 | Raw unsigned | -1, 0, +1 |
| RegGroup | 14 | 1 | Raw unsigned | -1, 0, +1 |
| Pred | 15 | 3 | Raw unsigned | 0..13 |

**Args sub-fields**:
- **R-type**: `[Op1(3)] [Op2(3)] [Op3(3)]` — каждый balanced, диапазон ±4
- **I-type**: `[Op1(3)] [Imm(6)]` — Op1 balanced (±4), Imm balanced (±364)
- **J-type**: `[Reg(3)] [000000]` — Reg balanced (±4), 6 тритов padding (всегда 0)
- **S-type** (LOADI/STOREI): `[Op1(3)] [Op2(3)] [Imm(3)]` — Op1/Op2 balanced (±4), Imm balanced (±13)

### Encoder (InstructionEncoder)

```csharp
// Все signed значения конвертируются в unsigned через ToUnsignedField:
// unsigned = signed + offset, где offset = (3^width - 1) / 2
long EncodeR(int pred, int opcode, int op1, int op2, int op3)
long EncodeI(int pred, int opcode, int op1, long imm)
long EncodeJ(int pred, int opcode, int reg)
long EncodeS(int pred, int opcode, int op1, int op2, long imm3)  // LOADI/STOREI
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
    - **LOADI/STOREI** (S-type): Извлечь Op1 (3 трита), Op2 (3 трита), Imm (3 трита) из Args, конвертировать в balanced: Op1 -= 13, Op2 -= 13, Imm -= 13

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

Регистры кодируются значением трита (-4..+4). Физический индекс = трит + 4.

### GP Регистры (General Purpose)
| Name | Trit | Phys | Назначение | Caller-saved |
|------|------|------|-----------|--------------|
| RW | -4 | 0 | Временный / Arg 0 | Да |
| RX | -3 | 1 | Временный / Arg 1 | Да |
| RY | -2 | 2 | Временный / Arg 2 | Да |
| RZ | -1 | 3 | База кадра / Callee-saved | Нет |
| R0 | 0 | 4 | Временный / Arg 3 | Да |
| R1 | +1 | 5 | Адрес вызова | Да |
| R2 | +2 | 6 | Возвращаемое значение | Да |
| R3 | +3 | 7 | Callee-saved | Нет |
| R4 | +4 | 8 | Адресный регистр | Нет |

### FPU Регистры (Floating Point)
| Name | Phys | Назначение | Caller-saved |
|------|------|-----------|--------------|
| FW | 0 | Временный | Да |
| FX | 1 | Временный | Да |
| FY | 2 | Временный | Да |
| FZ | 3 | Временный | Да |
| F0 | 4 | Временный | Да |
| F1 | 5 | Временный | Да |
| F2 | 6 | Возвращаемое значение | Да |
| F3 | 7 | Callee-saved | Нет |
| F4 | 8 | Callee-saved | Нет |

### Специальные регистры (S-group)
- **FP**: Frame Pointer (Указатель кадра)
- **HP**: Heap Pointer (Указатель кучи)
- **SP**: Stack Pointer (Указатель стека)
- **CD**: Condition Flag (Результат сравнения)
- **PR**: Predicate Register (9 тритов)
- **WD**: Window Pointer (Указатель окна)
- **PC**: Program Counter (Счётчик команд)

## Opcode Table (ISA v5)

Благодаря RegGroup, большинство инструкций теперь ортогональны. Префиксы:
- (без префикса) $\rightarrow$ GP
- `F.` $\rightarrow$ FPU
- `S.` $\rightarrow$ Special

| Op | Mnemonic | Type | Description |
|----|----------|------|-------------|
| 0 | HALT | – | Stop processor |
| 1 | NOP | – | No operation |
| 2 | MOV | R/I | `dst = src` (или `dst = imm`) |
| 3 | LIMM | R | `dst = mem[PC]; PC++` (2-word) |

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
| 51 | LOADI | S |
| 52 | STORE | R |
| 53 | STOREI | S |
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
| 67 | JM | J | Jump if Cond==0 (alias for JE) |
| 68 | JLE | J | Jump if Cond<=0 |
| 69 | JGE | J | Jump if Cond>=0 |
| 70 | CALL | J | `SP--; Memory[SP] = PC+1; PC = reg` |
| 71 | RET | – | `PC = Memory[SP]; SP++` |

**Важно**: J-type инструкции кодируют регистр в поле Op1 (3 трита). Поле Op2 (3 трита) и Op3 (3 трита) игнорируются (padding = 0). Дизассемблер использует `PhysOp1` для отображения регистра.

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

Если pred > 0, инструкция выполняется только если PR[pred-1] == +1.

**PR регистр**: 9 тритов, разделённых на 3 группы по 3 трита:
- Триты 0-2: группа предикации 1 (p0)
- Триты 3-5: группа предикации 2 (p1)
- Триты 6-8: группа предикации 3 (p2)

**Извлечение флага**: `GetPredicateFlag(predIndex)` читает `PR.GetTrit(predIndex - 1)`:
- `PR.GetTrit(0)` — флаг для predicate 1
- `PR.GetTrit(1)` — флаг для predicate 2
- `PR.GetTrit(2)` — флаг для predicate 3

**Установка PR** (LSB-first, позиция 0 = LSB):
- Predicate 1 = true → `PR = Word18.FromLong(1)` (3^0)
- Predicate 2 = true → `PR = Word18.FromLong(3)` (3^1)
- Predicate 3 = true → `PR = Word18.FromLong(9)` (3^2)

**Важно**: `Word18.GetTrit(index)` использует LSB-first индексацию. Позиция 0 — это LSB (3^0), позиция 17 — MSB (3^17). Не путать с `ToTritString()`, который возвращает строку от MSB к LSB.

## T3Float Format

**Формат**: 6 тритов экспоненты + 12 тритов мантиссы = 18 тритов.

- Экспонента: signed balanced, диапазон ±364, bias = 182
- Мантисса: signed balanced, диапазон ±88,573
- Значение: `mantissa * 3^(exponent - 182)`
- Кодирование в Word18: `value = exponent * 3^12 + mantissa` (линейное)

**ToWord18()**:
```csharp
long encoded = Exponent * (long)TernaryMath.Pow3(12) + Mantissa;
return Word18.FromLong(encoded);
```

**FromWord18()**:
```csharp
long pow12 = (long)TernaryMath.Pow3(12);
long raw = word.ToLong();
long exponent = raw / pow12;
long mantissa = raw % pow12;
return new T3Float(exponent, mantissa);
```

**Важно**: T3Float использует прямое линейное арифметическое кодирование, а не строковое. Строковое кодирование через `Word18.ToTritString()` + `BalancedTernary.ParseToLong()` НЕ РАБОТАЕТ для T3Float, так как `ExtractBalancedTrit` (используемый `ToTritString`) имеет фундаментальную проблему с переносами между полями экспоненты и мантиссы. Линейное кодирование корректно, так как деление и модуль работают с целыми числами, и переносы в сбалансированном троичном представлении не влияют на целочисленное деление.

## LIMM (Large Immediate)

LIMM — 2-словная инструкция для загрузки значений вне диапазона ±364 (который может быть закодирован в I-type imm).

```
Слово 1: [Pred(3)] [Opcode=LIMM(5)] [Reg(3)] [000000]
Слово 2: [данные (18 тритов)]
```

Процессор: `Register[reg] = Memory[PC]; PC++` (читает следующее слово как данные).

## Компилятор T-lang ABI

### Регистровая модель компилятора (ABI v4)

| Регистр | Phys | Назначение | Сохраняется |
|---------|------|-----------|-------------|
| RW (0) | 0 | Temporary, arg 0 | Caller-saved |
| RX (1) | 1 | Temporary, arg 1 | Caller-saved |
| RY (2) | 2 | Temporary, arg 2 | Caller-saved |
| RZ (3) | 3 | **Frame Pointer** (FP) | Callee-saved |
| R0 (4) | 4 | Temporary, arg 3 | Caller-saved |
| R1 (5) | 5 | Call temp | Caller-saved |
| R2 (6) | 6 | Return value | NOT saved |
| R3 (7) | 7 | Temporary | Callee-saved |
| R4 (8) | 8 | Address register | Callee-saved |

### Calling Convention (ABI v4)

**Caller**:
1. Сохранить caller-saved регистры через стек
2. Передать первые 4 аргумента в RW,RX,RY,R0; остальные — через стек
3. `LIMM R1, function; CALL R1`
4. Скопировать результат из R2

**Callee (пролог)**:
```asm
PUSH RZ          ; save old FP
PUSH R3          ; callee-saved
PUSH R4          ; callee-saved  
MOV RZ, SP       ; set FP
SUBI SP, SP, N   ; allocate local frame
```

**Callee (эпилог)**:
```asm
ADDI SP, SP, N   ; deallocate frame
POP R4
POP R3
POP RZ
RET
```

**return**: Генерирует `MOV R2, value` + `LIMM` + `JMP` на метку эпилога (не `RET` напрямую).

**Важно**: R2 (return value) не сохраняется и не восстанавливается в прологе/эпилоге. Caller копирует значение из R2 после возврата. RZ используется как frame pointer.

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

*Примечание: текущая реализация является интерпретатором, а не cycle-accurate симулятором. Timing-таблица является справочной.*