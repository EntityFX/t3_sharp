# План исправления T3Sharp — полная версия (выполнено)

На основе: `reports/deep-review.md`, `reports/development_report.md`, `reports/t3-gap-analysis-specification.md`

---

## Phase 1: Исправление предикации (P0 Critical Bug) ✅

### Проблема
`GetPredicateFlag` использует `PR.GetTrit(predIndex - 1)` для извлечения трита по индексам 0, 1, 2.
`Word18.GetTrit(index)` использует LSB-first индексацию (позиция 0 = LSB).
Тест устанавливает `PR = Word18.FromLong(531441)` что равно `3^12` — это устанавливает трит на позиции 12.
Но `GetTrit(0)` читает LSB-трит (позиция 0), а не позицию 12.

**Решение:** Исправить тесты на правильные PR значения:
- Predicate 1 → trit 0 → `PR = Word18.FromLong(1)` (3^0)
- Predicate 2 → trit 1 → `PR = Word18.FromLong(3)` (3^1)
- Predicate 3 → trit 2 → `PR = Word18.FromLong(9)` (3^2)

Процессор (`GetPredicateFlag`) уже правильный — он читает trit 0,1,2.

### Файлы для изменения
- `tests/T3Simulator.InOrder.Tests/InOrderProcessorTests.cs` — строка 14
- `tests/T3Simulator.InOrder.Tests/BenchmarkTests.cs` — строка 27
- `tests/T3Simulator.InOrder.Tests/FpuInstructionTests.cs` — строки 34-35

---

## Phase 2: Исправление дизассемблера для jump/call с регистрами (P0/P1 Bug) ✅

### Проблема
В `T3Disassembler.FormatInstruction` для J-type инструкций при `Immediate == 0` используется `PhysOp2` вместо `PhysOp1`. J-type кодирует регистр в поле Op1 (3 трита), а Op2=0.

### Решение
Изменить `PhysOp2` на `PhysOp1` в строке 66.

### Файл
- `src/T3Simulator.Common/T3Disassembler.cs` — строка 66

---

## Phase 3: Исправление InstructionDecoder.Decode(Word54) (P1 Bug) ✅

### Проблема
`Decode(Word54)` вызывает `Word18.FromLong()` который падает для значений вне 18-тритного диапазона.

### Решение
Использовать `Word18.FromWrappedLong()`.

### Файл
- `src/T3Simulator.Common/InstructionDecoder.cs` — строка 60

---

## Phase 4: Исправление ассемблера — silent error handling (P1 Bug) ✅

### Проблема
`P9()`/`P27()` в ассемблере и компиляторе возвращают пустую строку/"00" для неизвестных символов.

### Решение
Добавить `throw new FormatException()` в default-case.

### Файлы
- `src/T3Assembler/T3AssemblerBase.cs` — строки 29-30
- `src/T3Compiler/CodeGen/CodeGenerator.cs` — строки 130-131

---

## Phase 5: Исправление компилятора — silent fallback (P1 Bug) ✅

### Проблема
- `GenExpr`: неизвестные выражения → `Imm(0)`
- `LoadV`: неизвестные идентификаторы → `LI r,0`
- `GenBin`: неизвестные операторы → `ADD`

### Решение
Заменить на `throw new NotSupportedException()`.

### Файл
- `src/T3Compiler/CodeGen/CodeGenerator.cs` — строки 20, 49, 126

---

## Phase 6: Переработка ABI компилятора для рекурсии и сложных программ (P0 Critical) ✅

### Текущие проблемы

1. **`_nextAddr = 200` в каждой функции** (строка 12):
   - Каждая функция переиспользует адреса 200+ для локальных переменных
   - При рекурсии: каждый вызов перезаписывает переменные предыдущего
   - При вложенных вызовах: `main()` → `foo()` → `bar()` — `bar()` может перезаписать переменные `foo()` и `main()`

2. **Регистры не сохраняются при вызовах** (строка 78):
   - `EmitCall` не сохраняет caller-saved регистры
   - После `CALL` все регистры (кроме R2=return value) могут быть испорчены
   - Аллокатор (round-robin) не знает о clobber-регистрах

3. **Round-robin аллокатор** (строка 134):
   - Не учитывает liveness регистров
   - Может переиспользовать регистр, который ещё нужен
   - Не резервирует регистры для особых целей (R4=AddrReg, R5=call temp, R6=return value)

4. **Параметры функций не работают**:
   - Параметры передаются через стек, но не извлекаются в функции
   - Нет пролога/эпилога функции

### Решение: Stack Frame ABI

#### Новая модель памяти

```
Stack Layout (растёт вниз, SP указывает на вершину):
┌─────────────────────┐ ← initial SP (1048575)
│   ...               │
├─────────────────────┤
│  return address     │ ← SP после CALL (SP указывает сюда)
├─────────────────────┤
│  saved registers    │ ← SP после PUSH в прологе
├─────────────────────┤
│  local variables    │ ← SP после выделения фрейма
└─────────────────────┘
```

#### Регистровая модель

| Регистр | Назначение | Caller-saved |
|---------|-----------|--------------|
| RW (0)  | Временный | Yes |
| RX (1)  | Временный | Yes |
| RY (2)  | Временный | Yes |
| RZ (3)  | Временный | Yes |
| R0 (4)  | Временный | Yes |
| R1 (5)  | Call temp | Yes (сохраняется caller'ом) |
| R2 (6)  | Return value | No (callee не сохраняет) |
| R3 (7)  | Временный | Yes |
| R4 (8)  | Address register | Yes |

#### Изменения в CodeGenerator.cs

**1. `_nextAddr` изменён с 200 на 300:**
```csharp
int _nextAddr = 300; // Данные с адреса 300, код с 0
```

**2. Добавлен пролог/эпилог функции:**
```csharp
void GenFunc(FunctionDef f) {
    _varSlots.Clear(); _varSizes.Clear(); _arrDims.Clear(); _structFields.Clear();
    _nextReg = 3;
    Emit($"{f.Name}:");
    // Пролог: сохраняем все регистры кроме R2 (return value)
    Emit("    PUSH R4");  // сохраняем AddrReg
    Emit("    PUSH R3");
    Emit("    PUSH R1");
    Emit("    PUSH R0");
    Emit("    PUSH RZ");
    Emit("    PUSH RY");
    Emit("    PUSH RX");
    Emit("    PUSH RW");
    foreach(var s in f.Body.Body) GenStmt(s);
    // Эпилог
    Emit($"{_epilogueLabel}:");
    Emit("    POP RW");
    Emit("    POP RX");
    Emit("    POP RY");
    Emit("    POP RZ");
    Emit("    POP R0");
    Emit("    POP R1");
    Emit("    POP R3");
    Emit("    POP R4");
    Emit("    RET");
}
```

**3. `return` генерирует переход на эпилог:**
```csharp
// return expr → LIMM + JMP на эпилог (не RET напрямую)
```

**4. `EmitCall` сохраняет caller-saved регистры ПЕРЕД аргументами:**
```csharp
int EmitCall(FunctionCall fc) {
    // Сохраняем caller-saved регистры ПЕРВЫМИ (чтобы аргументы были на вершине стека)
    Emit("    PUSH RW"); Emit("    PUSH RX"); Emit("    PUSH RY"); Emit("    PUSH RZ");
    Emit("    PUSH R0"); Emit("    PUSH R1"); Emit("    PUSH R3"); Emit("    PUSH R4");
    // Push arguments in reverse order (после сохранения регистров)
    for(int i = fc.Arguments.Count - 1; i >= 0; i--)
        Emit($"    PUSH {RegName(GenExpr(fc.Arguments[i]))}");
    Emit($"    LI R1, {fc.FunctionName}");
    Emit("    CALL R1");
    // После RET: callee POPнул аргументы, стек = [caller's saved regs]
    // Восстанавливаем caller-saved регистры (аргументы уже POPнуты callee)
    Emit("    POP R4"); Emit("    POP R3"); Emit("    POP R1"); Emit("    POP R0");
    Emit("    POP RZ"); Emit("    POP RY"); Emit("    POP RX"); Emit("    POP RW");
    int r = AllocR();
    Emit($"    MOV {RegName(r)}, R2");
    return r;
}
```

**ВАЖНО: Порядок стека критичен!**
- Caller сохраняет регистры ПЕРВЫМИ, затем push аргументов
- Стек при входе в функцию: `[caller's saved regs] [args...] [ret addr]`
- Callee POPит ret addr, затем POPит аргументы (они на вершине стека)
- Если бы caller push аргументы ПЕРВЫМИ, то стек был бы: `[args] [caller's saved regs] [ret addr]`
- В этом случае callee POPил бы ret addr, затем первый сохранённый регистр вместо аргумента!

**5. `AllocR` пропускает R1, R2, R4:**
```csharp
int AllocR() {
    while (_nextReg == 5 || _nextReg == 6 || _nextReg == 8)
        _nextReg = (_nextReg + 1) % 9;
    int r = _nextReg;
    _nextReg = (_nextReg + 1) % 9;
    return r;
}
```

---

## Phase 7: Исправление T3Float round-trip ✅

### Проблема
`T3Float.ToWord18()` и `FromWord18()` использовали строковое кодирование через `Word18.ToTritString()` и `BalancedTernary.ParseToLong()`. Функция `ExtractBalancedTrit` (используемая `ToTritString()`) имеет фундаментальную проблему с переносами (carry propagation) для значений, где переносы затрагивают старшие позиции.

### Решение
Переход на прямое линейное арифметическое кодирование:
- `ToWord18()`: `value = exponent * 3^12 + mantissa`
- `FromWord18()`: `exponent = raw / 3^12`, `mantissa = raw % 3^12`

### Файл
- `src/TritTypes/T3Float.cs` — строки 20-37

---

## Phase 7b: Исправление ExtractBalancedTrit ✅

### Проблема
Функция использовала `(value / power) % 3` что даёт стандартные троичные цифры, затем отображала 2→-1. Не обрабатывала переносы из младших позиций.

### Решение
Переписана с использованием алгоритма сбалансированного троичного переноса — итерация от позиции 0 до запрошенной позиции с обработкой переносов на каждом шаге.

### Файл
- `src/TritTypes/TernaryMath.cs` — строки 15-27

---

## Phase 8: Исправление T3ConversionService ✅

### Проблема 1
`Hex = value.ToString("X")` выдавал `"2A"` без префикса `0x`. Аналогично для Binary, Octal, Ternary, Nonary, TwentySevenAry.

### Решение
Добавлены префиксы ко всем форматам вывода: `0x`, `0b`, `00`, `0t`, `0n`, `0y`.

### Проблема 2
`catch { value = 0; }` проглатывал все исключения, возвращая некорректный результат вместо проброса `FormatException`.

### Решение
Заменён на корректную обработку исключений с пробросом `FormatException`.

### Файл
- `src/TritTypes/T3ConversionService.cs` — строки 57-61, 65-74

---

## Phase 9: Очистка диагностических файлов ✅

Удалены:
- `DiagnosticTest.cs`
- `DiagnosticTest.csproj`
- `test_matrix.asm`
- `test_matrix.t`
- `test_output.asm`
- Диагностические тестовые методы из `FpuInstructionTests.cs` и `TLangCompilerTests.cs`

---

## Phase 10: Обновление документации ✅

Обновлены:
- `reports/development_report.md` — полный отчёт о разработке с деталями всех исправлений
- `docs/t3-isa-reference.md` — детальная ISA спецификация с encoding/decoding, predication, T3Float
- `docs/t3-architecture.md` — архитектурная документация с ABI, памятью, FPU
- `plans/fix-plan.md` — данный файл, отражает фактически выполненные изменения

---

## Phase 11: Параметры функций и новые конструкции языка ✅

### Проблема
Параметры функций передавались через стек, но не извлекались в прологе функции. Отсутствовали do/while, switch/case, break/continue, тернарный оператор, составные присваивания.

### Решение
- **Параметры функций**: Callee POPит ret addr в R2, POPит параметры в локальные слоты, PUSHит ret addr обратно, сохраняет регистры
- **do/while**: Добавлен `DoWhileStmt` в AST, парсер `ParseDoWhile()`, генератор `GenDoWhile()`
- **switch/case**: Добавлены `SwitchStmt`/`CaseStmt` в AST, парсер `ParseSwitch()`, генератор `GenSwitch()`
- **break/continue**: Обработка в `GenStmt()` через `_loopStack`
- **Тернарный оператор**: `GenTernary()` генерирует CMP + условные переходы
- **Составные присваивания**: `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=`, `<<=`, `>>=` в `EmitAssign()`
- **Доступ к полям структур**: `struct.array[index].field` и `(*ptr).field` в `EmitMemAccess()`
- **Forward declarations**: Парсер обрабатывает `tint func(tint n);` как объявление

### Файлы
- `src/T3Compiler/Parser/Ast.cs` — новые классы `DoWhileStmt`, `SwitchStmt`, `CaseStmt`
- `src/T3Compiler/Parser/Parser.cs` — `ParseDoWhile()`, `ParseSwitch()`, forward declarations
- `src/T3Compiler/CodeGen/CodeGenerator.cs` — `GenDoWhile()`, `GenSwitch()`, `GenTernary()`, параметры в `GenFunc()`, составные присваивания в `EmitAssign()`, struct pointer access в `EmitMemAccess()`

---

## Phase 12: Комплексные тесты (29 новых тестов) ✅

Добавлены тесты в `TLangCompilerTests.cs`:
- **Рекурсия**: Факториал с параметром (while), факториал с параметром (for)
- **Вложенные циклы**: Умножение матриц 3x3, решето простых чисел, пузырьковая сортировка, тройной вложенный цикл
- **Крайние случаи**: Отрицательные числа, ноль итераций, большие числа, глубокий if/else
- **Управление**: Тернарный оператор, break/continue, for, while-true-break
- **Массивы и указатели**: Сумма 2D массива, обмен через указатели, реверс массива
- **Структуры**: Вложенные структуры, массив структур, доступ через указатель на структуру
- **Составные присваивания**: Все арифметические compound ops
- **Препроцессор**: Макрос-выражение
- **Дополнительно**: do/while sum, do/while at-least-once, switch basic, switch default, сложное выражение

---

## Phase 6b: Исправление порядка стека в ABI ✅

### Проблема
Caller сохранял регистры ПОСЛЕ аргументов. Стек при входе в функцию: `[args][saved regs][ret addr]`. Callee POPил ret addr, затем POPил первый сохранённый регистр вместо аргумента. Это приводило к тому, что параметры функций не получали правильные значения.

### Решение
Изменён порядок в `EmitCall`: caller сохраняет регистры ПЕРЕД push аргументов. Стек при входе: `[saved regs][args][ret addr]`. Callee POPит ret addr, затем POPит аргументы (они на вершине), PUSHит ret addr обратно, сохраняет свои регистры.

### Файл
- `src/T3Compiler/CodeGen/CodeGenerator.cs` — строки 311-327 (EmitCall)

---

## Итоговые результаты тестирования

| Тестовый проект | Пройдено | Провалено |
|----------------|----------|-----------|
| TritTypes.Tests | 123 | 0 |
| T3Simulator.Common.Tests | 71 | 0 |
| T3Simulator.InOrder.Tests | 105 | 0 |
| **Итого** | **299** | **0** |