# T3Sharp Roadmap

**Дата:** 10 июля 2026 г.
**Версия:** v3.1 (ISA v5, ABI v5)
**Статус:** Активная разработка
**Тестов:** 561 (0 failures)

---

## Сводка реализованного

### Процессор (T3Simulator.InOrder)
- ✅ ISA v5: 38 базовых инструкций с ортогональной системой групп (GP/FPU/Special)
- ✅ 9 GP + 9 FPU + 6 Special регистров (RW..R4, FW..F4, FP, HP, SP, CD, PR, WD)
- ✅ Предикация (PR, 3 флага)
- ✅ Стек (SP, CALL/RET, 1M слов)
- ✅ Device Manager (IN/OUT, T-SCII output)
- ✅ LIMM (2-словные инструкции для констант > ±364)
- ✅ T3Float: 6+12 тритов (bias=182), FTOI/ITOF bit-preserving round-trip
- ✅ Расширенная отладочная трассировка (CALL/RET/LD/ST/LIMM/PUSH/POP)

### Ассемблер (T3Assembler)
- ✅ Два прохода: labels + binary
- ✅ `.equ` константы с expression evaluator (`label+4`, `a+b*2`)
- ✅ `.string` / `.word` директивы
- ✅ `CALL label` → `LIMM R1, addr + CALL R1` (3 слова, регистрово-косвенный)
- ✅ `Jxx label` → `LIMM R1, addr + Jxx R1` (3 слова)
- ✅ `LI label` → всегда `LIMM` (2 слова, I-type encoding)
- ✅ Дизассемблер (T3Disassembler)
- ✅ Точная классификация регистров (IsRegister без эвристик по первой букве)
- ✅ Исправлен LD/ST с R0 как базовым регистром

### T-lang компилятор
- ✅ Полный рекурсивный спуск (Lexer → Parser → Preprocessor → CodeGen)
- ✅ Типы: `void`, `trit`, `tril`, `tryte`, `tshort`, `tint`, `tlong`, `tfloat`, `tdouble`
- ✅ Управляющие конструкции: `if/maybe/else`, `while`, `for`, `do/while`, `switch/case`, `break/continue`, `goto`, метки
- ✅ Функции: параметры, рекурсия, взаимная рекурсия, forward declarations
- ✅ Структуры/объединения, enum'ы, указатели, многомерные массивы
- ✅ Препроцессор: `#define`, `#include`, `#if/#else/#endif`
- ✅ Тринарный оператор `?? :? :!`, составные присваивания (`+=`, `-=`, etc.)
- ✅ `sizeof(тип)`, type casts `(tint)expr`, char literals `'A'`
- ✅ Строки: `.string` + `strlen` в stdlib
- ✅ `*ptr = value` (запись через указатель, включая `malloc`-память)
- ✅ Nested function calls (взаимная рекурсия с локальными переменными)
- ✅ ABI v5: спецификация соглашения о вызовах (FP как Special-регистр, CALL/RET автосохранение)
- ✅ Отключаемая отладочная трассировка (EnableDebugTrace / CompilerDebugConfig.EnableCodeGenTrace)

### Типы данных (TritTypes)
- ✅ Trit, Tryte, Word18, Word54, T3Float, T3Double
- ✅ BalancedTernary, TernaryMath, T3Alu, T3Fpu
- ✅ T-SCII (729 символов: CP1251 + дополнительные символы)

### Документация
- ✅ `docs/t3-abi-v5-specification.md` — спецификация ABI v5 (соответствует реальному коду)

---

## TODO: Приоритетные задачи (Phase 14+)

### 🔴 Критические (безопасность/корректность)

| Задача | Файлы | Оценка |
|--------|-------|--------|
| **Array spill** — большие массивы не должны целиком пушиться на стек | `CodeGenerator.cs` | ~50 LOC |
| **Stack overflow detection** — проверка SP при CALL/PUSH | `ProcessorBase.cs` | ~20 LOC |

### 🟡 Высокий приоритет (язык/ассемблер/долг)

| Задача | Файлы | Оценка |
|--------|-------|--------|
| **`.include` для .asm** — многофайловые проекты | `T3InOrderAssembler.cs` | ~50 LOC |
| **Better register allocator** — расширить пул (R3/R4) или добавить спилл | `CodeGenerator.cs` | ~200 LOC |
| **Source-level error messages** — строка и позиция в исходнике | `Parser.cs`, `CodeGenerator.cs` | ~150 LOC |
| **ABI > 4 аргументов** — нет теста, смещение в коде исправлено | `CodeGenerator.cs`, tests | ~20 LOC |
| **print_int тест на вывод** — проверять поток, а не только R2 | tests | ~20 LOC |

### 🟢 Низкий приоритет (архитектурные)

| Задача | Файлы | Оценка |
|--------|-------|--------|
| **PUSHI/POPI** — push/pop immediate (новые опкоды) | `Opcode.cs`, `Processor.cs`, `Assembler.cs` | ~100 LOC |
| **T3-54 (Word54) процессор** — 54-тритные инструкции | `T3VliwProcessor.cs` | ~1500 LOC |
| **VLIW/SIMD** — пакетная обработка | `T3VliwAssembler.cs` + новый проект | ~2000 LOC |
| **Pipeline simulation** — cycle-accurate вместо интерпретатора | `T3InOrderProcessor.cs` | ~1500 LOC |
| **Register windowing** — 27 физических регистров, WP mapping | `RegisterWindow.cs`, `Processor.cs` | ~400 LOC |
| **MMU / virtual memory** | `Memory.cs`, новый `MMU.cs` | ~2000 LOC |

### GUI
| Задача | Файлы | Оценка |
|--------|-------|--------|
| **T3Simulator.GUI** — визуальный симулятор (Avalonia) | `T3Simulator.GUI/` | ~3000 LOC |
| **T3Calculator.GUI** — троичный калькулятор | `T3Calculator.GUI/` | ~1000 LOC |

---

## Файловая структура (v2.2)

```
t3_sharp/
├── ROADMAP.md              ← этот файл
├── README.md, README.ru.md
├── docs/
│   ├── t3-architecture.md  (архитектура процессора, ABI)
│   ├── t3-isa-reference.md (ISA, кодирование, тайминги)
│   ├── ternary-computing.md
│   └── t-lang/
│       ├── spec.md         (спецификация языка T)
│       └── t-lang.ebnf     (EBNF грамматика)
├── reports/
│   └── development_report.md (полный отчёт о разработке)
├── src/
│   ├── TritTypes/
│   ├── T3Simulator.Common/
│   ├── T3Simulator.InOrder/
│   ├── T3Assembler/
│   └── T3Compiler/
└── tests/
    ├── TritTypes.Tests/
    ├── T3Simulator.Common.Tests/
    └── T3Simulator.InOrder.Tests/
```

---

## История версий

| Версия | Дата | Ключевые изменения | Тесты |
|--------|------|-------------------|-------|
| v1.0 | Март 2026 | Базовый процессор, типы, ассемблер | 200+ |
| v2.0 | Июнь 2026 | FPU, предикация, ABI, компилятор T-lang | 299 |
| v2.1 | 23 июня | Switch/case, do/while, тернарный, рекурсия, 29 тестов | 316 |
| v2.2 | 25 июня | ABI v3 (spill locals+FPU), goto, sizeof, casts, expression evaluator | 316 |
| v3.0 | 6 июля | Миграция на ISA v5 (RegGroup), расширение интерпретатора | 556 |
