# T-lang: Детальный план реализации фич

**Дата:** 2026-07-01  
**Методология:** Итеративное добавление фич с регрессионным тестированием на каждом шаге

---

## Условные обозначения

| Метка | Значение |
|-------|----------|
| 🟢 Лёгкая | < 50 LOC, 1-2 файла |
| 🟡 Средняя | 50-200 LOC, 2-4 файла |
| 🔴 Тяжёлая | 200-500 LOC, 3-6 файлов |
| ⚫ Критическая | > 500 LOC, 5+ файлов |

---

## Итерация 1: Критические баги (сейчас — Priority 1)

### 1.1 🟢 Float-сравнения в интерпретаторе

**Проблема:** Операторы `==`, `!=`, `<`, `>`, `<=`, `>=` в [`EvalBin()`](src/T3Interpreter/T3Interpreter.cs:106) всегда используют `l.AsInt()`, что даёт неверные результаты для float.

**Изменения:**
- [`src/T3Interpreter/T3Interpreter.cs`](src/T3Interpreter/T3Interpreter.cs) — строки 106-111: добавить проверку `l.Kind == ValueKind.Float || r.Kind == ValueKind.Float` и использовать `AsFloat()` для сравнений
- [`src/T3Interpreter/T3Value.cs`](src/T3Interpreter/T3Value.cs) — возможно, добавить метод `AsFloat()` для bool (сейчас возвращает 0.0)

**LOC:** ~20 строк  
**Файлы:** 1-2  
**Риск:** Низкий  
**Тесты:** `Intrp_FloatDefaultZero` (уже починен), нужно добавить `Intrp_FloatCompare`

---

### 1.2 🔴 Nested function calls в компиляторе

**Проблема:** `Equiv_NestedFunctionCalls_Compiler` помечен `[Ignore]` — вложенные вызовы теряют регистры.

**Корень:** В [`EmitCall()`](src/T3Compiler/CodeGen/CodeGenerator.cs:655) caller-saved регистры сохраняются через PUSH/PUSH, но при генерации аргументов для вложенного вызова регистры могут переиспользоваться до того, как будут сохранены.

**Изменения:**
- [`src/T3Compiler/CodeGen/CodeGenerator.cs`](src/T3Compiler/CodeGen/CodeGenerator.cs) — в `EmitCall()`:
  1. Сначала вычислить ВСЕ аргументы и сохранить их на стеке
  2. Потом сохранить caller-saved регистры
  3. Потом загрузить аргументы из стека в регистры
- Либо: сохранять аргументы на стеке сразу после вычисления

**LOC:** ~50-80 строк  
**Файлы:** 1  
**Риск:** Средний (может сломать другие тесты)  
**Тесты:** `Equiv_NestedFunctionCalls_Compiler` (снять `[Ignore]`)

---

### 1.3 🔴 Frame Pointer (RZ) в симуляторе

**Проблема:** При запуске скомпилированного кода `RZ` остаётся `0`, что даёт отрицательные адреса при `LOADI/STOREI`.

**Корень:** В отчёте [`reports/development_report_v4.md`](reports/development_report_v4.md) указано, что `MOV RZ, SP` в прологе не отрабатывает или `RZ` затирается.

**Изменения:**
- [`src/T3Simulator.InOrder/T3InOrderProcessor.cs`](src/T3Simulator.InOrder/T3InOrderProcessor.cs) — проверить обработку `MOV` с `SP` (специальный регистр 9) в `RZ` (регистр 3)
- [`src/T3Simulator.Common/ProcessorBase.cs`](src/T3Simulator.Common/ProcessorBase.cs) — проверить, что `SP` корректно читается как операнд
- [`src/T3Compiler/CodeGen/CodeGenerator.cs`](src/T3Compiler/CodeGen/CodeGenerator.cs) — проверить генерацию пролога для `main`

**LOC:** ~30-50 строк  
**Файлы:** 2-3  
**Риск:** Высокий (может затронуть много тестов)  
**Тесты:** Все `Equiv_*` тесты

---

## Итерация 2: Базовые расширения (Priority 2)

### 2.1 🟢 Символьные литералы `'A'`

**Проблема:** В spec описаны символьные литералы, но они не реализованы.

**Изменения:**
- [`src/T3Compiler/Lexer/Tokenizer.cs`](src/T3Compiler/Lexer/Tokenizer.cs) — добавить токен `CharLiteral`
- [`src/T3Compiler/Parser/Parser.cs`](src/T3Compiler/Parser/Parser.cs) — обработка `CharLiteral` в `ParsePrimary()`
- [`src/T3Interpreter/T3Interpreter.cs`](src/T3Interpreter/T3Interpreter.cs) — `Eval()` для `CharLiteral`
- [`src/T3Compiler/CodeGen/CodeGenerator.cs`](src/T3Compiler/CodeGen/CodeGenerator.cs) — `GenExpr()` для `CharLiteral`

**LOC:** ~40-60 строк  
**Файлы:** 4  
**Риск:** Низкий  
**Тесты:** `Intrp_CharLiteral`, `Equiv_CharLiteral`

---

### 2.2 🟡 `typedef` в интерпретаторе

**Проблема:** `typedef` работает в компиляторе (парсер добавляет в `_typeNames`), но интерпретатор не обрабатывает `TypedefDef`.

**Изменения:**
- [`src/T3Interpreter/T3Interpreter.cs`](src/T3Interpreter/T3Interpreter.cs) — в конструкторе обработать `program.Typedefs` (сохранить в словарь)
- [`src/T3Interpreter/T3Interpreter.cs`](src/T3Interpreter/T3Interpreter.cs) — при разрешении типов проверять typedef-ы

**LOC:** ~20-30 строк  
**Файлы:** 1  
**Риск:** Низкий  
**Тесты:** `Intrp_Typedef_TIntAlias` (аналог `Compile_Typedef_TIntAlias`)

---

### 2.3 🟡 Float-сравнения в компиляторе

**Проблема:** Компилятор генерирует `CMP` для float через `AsInt()`, что неверно.

**Изменения:**
- [`src/T3Compiler/CodeGen/CodeGenerator.cs`](src/T3Compiler/CodeGen/CodeGenerator.cs) — в `GenBin()` добавить проверку на float-операнды и генерировать `FCMP` вместо `CMP`

**LOC:** ~30-50 строк  
**Файлы:** 1  
**Риск:** Низкий  
**Тесты:** `Equiv_FloatCompare`

---

## Итерация 3: Типы данных (Priority 2-3)

### 3.1 🟡 `tlong` (36 тритов) в интерпретаторе

**Проблема:** `tlong` хранится как `long` (64-bit), теряется точность.

**Изменения:**
- [`src/T3Interpreter/T3Value.cs`](src/T3Interpreter/T3Value.cs) — добавить `ValueKind.Long` с `Int128` или парой `(long lo, long hi)`
- [`src/T3Interpreter/T3Interpreter.cs`](src/T3Interpreter/T3Interpreter.cs) — в `VarDeclaration` для `tlong` создавать `T3Value.FromLong(0)`
- Арифметические операции для `tlong`

**LOC:** ~100-150 строк  
**Файлы:** 2  
**Риск:** Средний  
**Тесты:** `Intrp_TlongArith`, `Equiv_TlongArith`

---

### 3.2 🟡 `union` в парсере и интерпретаторе

**Проблема:** Парсер парсит `union` (строка 53: `isUnion = Peek().Type == TokenType.KwUnion`), но интерпретатор и компилятор не обрабатывают union-специфику.

**Изменения:**
- [`src/T3Interpreter/T3Interpreter.cs`](src/T3Interpreter/T3Interpreter.cs) — при создании struct/union значения все поля должны разделять память
- [`src/T3Compiler/CodeGen/CodeGenerator.cs`](src/T3Compiler/CodeGen/CodeGenerator.cs) — union должен выделять max(sizeof(fields)), а не сумму

**LOC:** ~50-80 строк  
**Файлы:** 2  
**Риск:** Низкий  
**Тесты:** `Intrp_Union`, `Equiv_Union`

---

### 3.3 🟢 `const`/`volatile` квалификаторы

**Проблема:** Квалификаторы распознаются токенизатором, но игнорируются.

**Изменения:**
- Парсер: сохранять квалификаторы в `TypeSpec`
- Интерпретатор: `const` — запрет на присваивание
- Компилятор: `const` — может использоваться для оптимизации

**LOC:** ~40-60 строк  
**Файлы:** 2-3  
**Риск:** Низкий  
**Тесты:** `Intrp_ConstVar`

---

## Итерация 4: Стандартная библиотека (Priority 3)

### 4.1 🔴 Базовая стандартная библиотека

**Проблема:** Стандартная библиотека практически пуста.

**Изменения:**
- [`src/T3Compiler/lib/tio.th`](src/T3Compiler/lib/tio.th) — заголовочный файл с объявлениями
- Создать ассемблерные файлы для:
  - `printint`, `printfloat`, `printdouble`
  - `getchar`, `putchar`
  - `strlen`, `strcpy`, `strcmp`
  - `malloc`, `free`
- Подключить через `#include <tio.h>` в компиляторе

**LOC:** ~200-400 строк ассемблера  
**Файлы:** 5-8  
**Риск:** Средний  
**Тесты:** `Equiv_PrintInt`, `Equiv_Strlen`

---

### 4.2 🟡 Математическая библиотека

- `tabs`, `tmin`, `tmax`, `tsqrt`
- Тригонометрические функции (tsin, tcos)

**LOC:** ~100-200 строк  
**Файлы:** 2-3  
**Риск:** Низкий

---

## Итерация 5: Продвинутые фичи (Priority 3-4)

### 5.1 🟡 Vararg-функции

**Проблема:** Нет поддержки `...` в параметрах.

**Изменения:**
- Парсер: распознавать `...` в параметрах
- Интерпретатор: передавать все аргументы
- Компилятор: ABI v4 vararg rules

**LOC:** ~100-150 строк  
**Файлы:** 3  
**Риск:** Средний

---

### 5.2 🟢 `inline` для функций

- Парсер: распознавать `inline`
- Компилятор: инлайнить маленькие функции

**LOC:** ~50-100 строк  
**Файлы:** 2  
**Риск:** Низкий

---

### 5.3 🟢 Сбалансированные троичные float-литералы

- `0t+.-0e+0f` — парсинг balanced ternary float
- Конвертация в T3Float

**LOC:** ~50-80 строк  
**Файлы:** 2  
**Риск:** Низкий

---

## Итерация 6: Полировка (Priority 4)

### 6.1 🟢 Улучшенная обработка ошибок

- Номера строк в ошибках интерпретатора
- `-Werror`/`-Wall` флаги

**LOC:** ~50-100 строк  
**Файлы:** 2-3  
**Риск:** Низкий

---

### 6.2 🟢 `#line` и `#pragma` в препроцессоре

**LOC:** ~30-50 строк  
**Файлы:** 1  
**Риск:** Низкий

---

## Сводная таблица

| # | Фича | Сложность | LOC | Файлы | Риск | Итерация |
|---|------|-----------|-----|-------|------|----------|
| 1.1 | Float-сравнения (интерпретатор) | 🟢 | 20 | 1-2 | Низкий | 1 |
| 1.2 | Nested calls (компилятор) | 🔴 | 50-80 | 1 | Средний | 1 |
| 1.3 | Frame Pointer (симулятор) | 🔴 | 30-50 | 2-3 | Высокий | 1 |
| 2.1 | Символьные литералы | 🟢 | 40-60 | 4 | Низкий | 2 |
| 2.2 | `typedef` в интерпретаторе | 🟢 | 20-30 | 1 | Низкий | 2 |
| 2.3 | Float-сравнения (компилятор) | 🟡 | 30-50 | 1 | Низкий | 2 |
| 3.1 | `tlong` (36 тритов) | 🟡 | 100-150 | 2 | Средний | 3 |
| 3.2 | `union` | 🟡 | 50-80 | 2 | Низкий | 3 |
| 3.3 | `const`/`volatile` | 🟢 | 40-60 | 2-3 | Низкий | 3 |
| 4.1 | Стандартная библиотека | 🔴 | 200-400 | 5-8 | Средний | 4 |
| 4.2 | Математическая библиотека | 🟡 | 100-200 | 2-3 | Низкий | 4 |
| 5.1 | Vararg | 🟡 | 100-150 | 3 | Средний | 5 |
| 5.2 | `inline` | 🟢 | 50-100 | 2 | Низкий | 5 |
| 5.3 | Balanced float literal | 🟢 | 50-80 | 2 | Низкий | 5 |
| 6.1 | Обработка ошибок | 🟢 | 50-100 | 2-3 | Низкий | 6 |
| 6.2 | `#line`/`#pragma` | 🟢 | 30-50 | 1 | Низкий | 6 |
| | **ИТОГО** | | **~970-1690** | | | |

---

## Процесс реализации

Для каждой итерации:

1. **Code mode** реализует фичу
2. **Запуск тестов** `dotnet test`
3. Если тесты падают — **Debug mode** для анализа
4. **Возврат в Architect** для проверки регрессии
5. Переход к следующей фиче

### Команды для регрессии:

```bash
# Все тесты
dotnet test

# Только интерпретатор
dotnet test tests/T3Interpreter.Tests/T3Interpreter.Tests.csproj

# Только компилятор + симулятор
dotnet test tests/T3Simulator.InOrder.Tests/T3Simulator.InOrder.Tests.csproj

# Конкретный тест
dotnet test --filter "Intrp_FloatDefaultZero"