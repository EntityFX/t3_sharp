# План реализации T3 процессора — v2 (Текущий прототип)

*Примечание: данный документ описывает архитектуру текущего исследовательского прототипа. Расширенные возможности (VLIW, Register Windowing, Speculation), упомянутые в некоторых концептуальных документах, в данной реализации не интегрированы и находятся в статусе Planned.*

## Архитектура

- **Word18**: `readonly struct` на `int` (32 бита), 18 трит влезает
- **Word54**: `readonly struct` на `Int128`, 54 трита
- **IT3Word\<T\>**: общий интерфейс для обоих типов
- **T3Float**: 18-тритный float (6 порядок + 12 мантисса)

## Регистры

| Имя | Трит | Phys | FPU |
|-----|------|------|-----|
| RW | -4 | 0 | FW |
| RX | -3 | 1 | FX |
| RY | -2 | 2 | FY |
| RZ | -1 | 3 | FZ |
| R0 | 0 | 4 | F0 |
| R1 | +1 | 5 | F1 |
| R2 | +2 | 6 | F2 |
| R3 | +3 | 7 | F3 |
| R4 | +4 | 8 | F4 |

Специальные: SP, PC, Cond (1 трит), PR (9 тритов = 3 флага по 3 трита)

## Формат инструкции

```
[Pred (3)] [Opcode (6)] [Args (9)]
```
- R-тип: Args = [Op1(3)][Op2(3)][Op3(3)]
- I-тип: Args = [Op1(3)][Imm(6)]
- J-тип: Args = [Reg(3)][000000]

## Компоненты

### 1. Word18/Word54 (TritTypes)
- Хранение: int (Word18) / Int128 (Word54)
- Методы: FromLong, ToTritString, Parse, арифметика
- Тритовые операции: TritAnd, TritOr, TritXor — без строк

### 2. InstructionEncoder (T3Simulator.Common)
- `Word18 Encode(Opcode op, int pred, int op1, int op2, int op3, int imm)`
- Работает через арифметику: `value = pred*3^15 + op*3^9 + args`
- Без строковых конверсий

### 3. InstructionDecoder (T3Simulator.Common)
- `DecodedInstruction Decode(Word18 word)`
- Извлечение полей: `word / 3^15 % 3`, `word / 3^9 % 3^6`, `word % 3^9`
- `DecodedInstruction` — struct: Opcode, Pred, Op1, Op2, Op3, Imm

### 4. ProcessorBase\<T\> (T3Simulator.Common)
- Registers[9], FRegisters[9]
- Memory[1M]
- ReadWord/WriteWord с MMIO
- Run, Step, Reset

### 5. T3InOrderProcessor\<T\> (T3Simulator.InOrder)
- Последовательное выполнение
- ExecuteInstruction — switch по Opcode
- Предикация: EvaluatePredicate(pred) → проверка PR[pred-1]

### 6. T3AssemblerBase + T3InOrderAssembler
- Два прохода: сбор меток → генерация кода
- Маппинг имён регистров: RW(-4)..R4(+4) → phys 0-8
- Генерация Word18[] через InstructionEncoder

### 7. T3Disassembler
- Word18[] → текст
- Обратный маппинг phys → тритовое имя

### 8. T3Compiler (CodeGenerator)
- T-lang → Word18[] напрямую (без текстового ассемблера)
- AllocR: свободные регистры 0-8 (кроме R2=return, R4=addr)
- _nextAddr = 200+
- Лейблы разрешаются backpatching

## Тесты
- **TritTypes.Tests**: тесты Word18 арифметики
- **Common.Tests**: тесты Encoder/Decoder round-trip
- **InOrderInstructionTests**: каждая инструкция → Word18[] → run → verify
- **InOrderProcessorTests**: Predication, Stack, IO
- **FpuInstructionTests**: ITOF, FMOV, FSWAP с trit-кодированием
- **TLangCompilerTests**: T-lang → Word18[] → run → verify
- **T3AdvancedTests**: Ассемблерный код через новый ассемблер
- **BenchmarkTests**: Dhrystone, Whetstone