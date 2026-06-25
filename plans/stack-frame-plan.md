# План: Стековая модель, глобальные переменные и исправление тестов

Дата: 2026-06-25
На основе: `plans/fix-plan.md` (Phase 6, но Phase 6 был неполным — проблема `_nextAddr=100` не решена до конца)

---

## Текущая проблема

36 тестов падают. Две категории:

### Категория 1: Библиотечные тесты (5 тестов)

`T3LibraryTests.cs` строка 25: жёстко закодирован абсолютный путь `E:\Projects\t3_sharp\src\T3Assembler\examples\tio.asm`. Проект находится на `C:\projects\t3_sharp`. Библиотека не загружается → метки `putchar`, `printint`, `printfloat`, `printstring` не резолвятся → `Unknown: putchar`.

**Исправление:** заменить на относительный путь от базовой директории приложения.

### Категория 2: Компиляторные тесты (31 тест)

Корневая причина: `_nextAddr = 100` в `CodeGenerator.cs` строка 435 размещает переменные по абсолютным адресам начиная с 100. Сложные программы (рекрусия, матрицы, сортировки) генерируют 200+ слов кода, которые физически перезаписывают память переменных.

Дополнительные проблемы:
- **Регистровый аллокатор** `AllocR()` имеет ~3 рабочих регистра (RZ, R0, R3) — R1 зарезервирован для CALL, R2 для return value, R4 для адресации — spill-конфликты при сложных выражениях
- **Соглашение о вызовах**: 8 PUSH caller-saved + аргументы + 8 PUSH callee-saved, порядок хрупкий
- **Отсутствие Frame Pointer**: нет стабильного способа адресовать переменные относительно стекового кадра
- **Enum в параметрах функций**: `_enumConstants` не передаётся через границу функций
- **Строки и strlen**: нет полноценной поддержки строк через стек

---

## План исправления (6 шагов)

### Шаг 1: Fix library path

**Файл:** `tests/T3Simulator.InOrder.Tests/T3LibraryTests.cs`

Заменить абсолютный путь `E:\Projects\t3_sharp\src\T3Assembler\examples\tio.asm` на относительный, вычисляемый от базовой директории приложения.

### Шаг 2: Stack Frame + Frame Pointer

**Файл:** `src/T3Compiler/CodeGen/CodeGenerator.cs`

#### Новая модель памяти

```
Память:
┌─────────────────────┐ ← 0
│   Код программы     │ (переменная длина)
├─────────────────────┤ ← code_end
│   Секция данных     │ (.word, .string — глобальные переменные)
├─────────────────────┤ ← data_start (code_end)
│   ...               │
├─────────────────────┤ ← initial SP (1048575)
│   Стек (растёт ↓)   │
│   [ret addr]        │
│   [old FP]          │ ← FP
│   [saved regs]      │
│   [локальные vars]  │
│   ...               │
└─────────────────────┘
```

#### Frame Pointer — регистр RZ (индекс 3)

- **Пролог функции:**
  ```
  PUSH RZ          ; save old FP
  MOV RZ, SP       ; FP = SP
  LI R0, <frameSize>
  SUB SP, SP, R0   ; allocate frame
  ```

- **Эпилог функции:**
  ```
  MOV SP, RZ       ; deallocate frame
  POP RZ           ; restore FP
  RET
  ```

- **Адресация переменных:** `LoadV(name)` → `LI AddrReg(R4), <fp_offset>` → `ADD AddrReg, RZ, AddrReg` → `LOAD r, AddrReg`

#### Изменения в CodeGenerator:

- Убрать `_nextAddr = 100`
- Переменные размещаются как FP-относительные смещения: `_varSlots[name] = текущее_смещение`
- `Alloc()`: `_varSlots[name] = _nextFpOffset; _nextFpOffset += size`
- `LoadV()`: `Emit($"LI {RegName(AddrReg)},{offset}"); Emit($"ADD {RegName(AddrReg)},RZ,{RegName(AddrReg)}"); Emit($"LOAD {RegName(r)},{RegName(AddrReg)}");`
- `Store()`: аналогично, но STORE
- Пролог/эпилог с FP как выше

### Шаг 3: Global variables via .data section

**Файл:** `src/T3Compiler/CodeGen/CodeGenerator.cs`

Глобальные переменные размещаются в секции данных ПОСЛЕ кода:

- Компилятор генерирует код в `_output`, запоминая его длину в словах
- Глобальные переменные (из `_program.Globals`) размещаются после кода: `_globalAddr = codeSize`
- Для глобальных строк/float — те же `.word`/.`.string` директивы
- Адресация глобальных переменных: через абсолютный LIMM (адрес > 364)

### Шаг 4: Stable function call ABI

**Файл:** `src/T3Compiler/CodeGen/CodeGenerator.cs`

Упрощённое соглашение:

- **Caller** (EmitCall):
  1. Сохранить caller-saved: PUSH RW, RX, RY, RZ, R0, R1, R3, R4
  2. PUSH аргументы в обратном порядке
  3. LI R1, label + CALL R1
  4. Восстановить caller-saved: POP R4, R3, R1, R0, RZ, RY, RX, RW
  5. MOV result, R2

- **Callee** (GenFunc):
  1. POP R2 (сохранить ret addr)
  2. POP параметры во временные регистры, сохранить в переменные
  3. PUSH R2 (восстановить ret addr на стек)
  4. Сохранить FP, установить FP = SP
  5. Выделить фрейм
  6. Сохранить callee-saved регистры
  7. ... тело функции ...
  8. Эпилог: восстановить callee-saved, деаллоцировать фрейм, восстановить FP, RET

### Шаг 5: Fix enum params, strings, strlen, ternary

**Файлы:**
- `src/T3Compiler/CodeGen/CodeGenerator.cs`
- `src/T3Compiler/Parser/Ast.cs`
- `src/T3Compiler/Parser/Parser.cs`

- **Enum в параметрах:** тип `enum Color` должен быть зарегистрирован как `_typeNames` при использовании в параметрах функций. Добавить идентификатор enum-типа в `_typeNames` в парсере.
- **Строки:** убедиться, что `.string` корректно размещается в секции данных и адрес передаётся правильно
- **strlen:** встроенная функция или call — убедиться что работает со стековым ABI
- **Тернарный оператор:** текущая реализация `GenTernary` выглядит корректной, проверить на тесте

### Шаг 6: Run tests and fix remaining issues

Запустить полный набор тестов, исправить оставшиеся проблемы.

---

## Ожидаемый результат

| Тестовый проект | Пройдено | Провалено |
|----------------|----------|-----------|
| TritTypes.Tests | 123 | 0 |
| T3Simulator.Common.Tests | 71 | 0 |
| T3Simulator.InOrder.Tests | ~141 | 0 |
| **Итого** | **~335** | **0** |

---

## Файлы для изменения

| Файл | Шаг | Описание |
|------|-----|----------|
| `tests/T3Simulator.InOrder.Tests/T3LibraryTests.cs` | 1 | Относительный путь к tio.asm |
| `src/T3Compiler/CodeGen/CodeGenerator.cs` | 2-5 | FP, глобальные переменные, ABI, enum, строки |
| `src/T3Compiler/Parser/Ast.cs` | 5 | Enum types (при необходимости) |
| `src/T3Compiler/Parser/Parser.cs` | 5 | Enum type registration |