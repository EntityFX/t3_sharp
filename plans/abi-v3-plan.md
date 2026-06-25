# ABI v3: Spill-based Stack Frames — Полный план

Дата: 2026-06-25
Статус: Plan

---

## 1. Текущие ограничения ABI (v2)

| Проблема | Причина | Последствия |
|----------|---------|-------------|
| Локальные переменные — абсолютные адреса | `_nextAddr` глобальный, инкрементируется при `Alloc()` | Рекурсия: каждый вызов перезаписывает переменные |
| FPU-регистры не сохраняются | `EmitCall` пушит только GP-регистры (RW..R4) | Float-переменные портятся после CALL |
| Нет Frame Pointer | ISA не имеет `MOV R, SP` | Классический стековый кадр невозможен |
| Ручной unoptimized spill | Нет трекинга live-переменных | Избыточные PUSH/POP, раздувание кода |

---

## 2. Решение: Spill-based ABI

### Принцип

Перед каждым `CALL` компилятор **пушит на стек**:
1. Все live локальные переменные (`LOAD [slot] → PUSH`)
2. Все live FPU-регистры (`FTOI → PUSH`)
3. Стандартные caller-saved GP-регистры (уже делается)
4. Аргументы (уже делается)

После `CALL` — восстановление в обратном порядке.

### Стек вызова (полный)

```
Перед CALL:
┌─────────────────────────┐ ← SP (до spill)
│  [caller local frame]   │  ← уже на стеке
├─────────────────────────┤
│  PUSH [slot_N]          │  ← spill locals (N слов, по числу live vars)
│  ...                    │
│  PUSH [slot_0]          │
│  FTOI R0, F4; PUSH R0   │  ← spill FPU (9×2=18 слов — FTOI + PUSH)
│  ...                    │
│  FTOI R0, F0; PUSH R0   │
│  PUSH RW                │  ← caller-saved GP (8 слов — уже было)
│  PUSH RX                │
│  PUSH RY                │
│  PUSH RZ                │
│  PUSH R0                │
│  PUSH R3                │
│  PUSH R1                │
│  PUSH R4                │
│  PUSH arg[N-1]          │  ← аргументы (M слов)
│  ...                    │
│  PUSH arg[0]            │
├─────────────────────────┤
│  CALL func              │  → [ret addr] пушится процессором
├─────────────────────────┤
│  callee prologue:       │
│  POP R2 (ret addr)      │
│  POP params → store     │
│  PUSH R2                │
│  PUSH RW..R4, RZ..R0    │  ← callee-saved (8 слов — уже было)
│  [callee body]          │
│  callee epilogue:       │
│  POP R0..RZ, R4..RW     │
│  RET                    │
├─────────────────────────┤
│  caller после CALL:     │
│  POP R4..RW             │  ← restore caller-saved GP (8 слов)
│  POP R0; ITOF F0, R0    │  ← restore FPU (18 слов)
│  ...                    │
│  POP R0; ITOF F4, R0    │
│  POP [slot_0]           │  ← restore locals (N слов)
│  ...                    │
│  POP [slot_N]           │
│  MOV result, R2         │  ← return value
└─────────────────────────┘
```

Размер spill'а на один вызов: `N + 18 + 8 + M` слов, где N = число live-локальных переменных.

---

## 3. Изменения в CodeGenerator

### 3.1. Трекинг live-переменных

```csharp
readonly HashSet<string> _liveVars = new();
bool _fpuLive = false;  // true если хоть один F-регистр использовался

void MarkLive(string name) => _liveVars.Add(name);
void MarkFpuLive() => _fpuLive = true;
```

`LoadV()` и `Store()` вызывают `MarkLive(name)`.
`EmitFloat()`, `ITOF`, `FTOI` вызывают `MarkFpuLive()`.

### 3.2. Новый EmitCall

```csharp
int EmitCall(FunctionCall fc) {
    // 1. Spill live locals
    foreach(var name in _liveVars.Reverse())
        EmitCode($"    PUSH {RegName(LoadV(name, 0))}");
    
    // 2. Spill FPU
    if (_fpuLive) {
        for(int i = 8; i >= 0; i--) {
            EmitCode($"    FTOI R0, F{RegName(i)}, 0");
            EmitCode($"    PUSH R0");
        }
    }
    
    // 3. Caller-saved GP + args + CALL (existing)
    EmitCode("    PUSH RW"); ... EmitCode("    PUSH R4");
    for(int i = fc.Arguments.Count - 1; i >= 0; i--)
        EmitCode($"    PUSH {RegName(GenExpr(fc.Arguments[i]))}");
    EmitCode($"    LI R1, {fc.FunctionName}");
    EmitCode("    CALL R1");
    
    // 4. Restore caller-saved GP (existing)
    EmitCode("    POP R4"); ... EmitCode("    POP RW");
    
    // 5. Restore FPU
    if (_fpuLive) {
        for(int i = 0; i <= 8; i++) {
            EmitCode("    POP R0");
            EmitCode($"    ITOF F{RegName(i)}, R0");
        }
    }
    
    // 6. Restore locals
    foreach(var name in _liveVars)
        Store(name, /* pop'd reg */ ...); // POP + STORE
    
    // 7. Return value
    int r = AllocR();
    EmitCode($"    MOV {RegName(r)}, R2");
    
    // Clear tracking for next call
    _liveVars.Clear();
    _fpuLive = false;
    return r;
}
```

---

## 4. Изменения в ассемблере

### 4.1. `.include` директива (TBD)
Для многофайловых проектов. Не критично для ABI v3.

### 4.2. Выражения в `.equ`
Уже работает через `ResolveExpression()`.

### 4.3. Нет изменений в кодировании инструкций
ABI v3 не требует новых опкодов. Все операции через существующую ISA.

---

## 5. Изменения в процессоре

### 5.1. Не требуются
ABI v3 — чисто компиляторное изменение. Процессор (`T3InOrderProcessor`) не меняется.

Опционально: добавить **FTOI/ITOF без округления** (битовое копирование), если текущая реализация делает математическое преобразование. Проверить `T3Fpu.ToInt`/`FromInt`.

---

## 6. Тесты

### 6.1. Регрессия (существующие 122 теста)
Все должны продолжать проходить.

### 6.2. Новые тесты

| Тест | Что проверяет |
|------|---------------|
| `Compile_Factorial_Deep` | fact(7) = 5040 с spill locals |
| `Compile_Fibonacci_Deep` | fib(15) с многими локальными переменными |
| `Compile_MutualRecursion_Float` | isEven/isOdd с float-аргументами, spill FPU |
| `Compile_NestedCalls_Float` | `add(3.0, mul(4.0, 5.0))` — вложенные вызовы с FPU |
| `Compile_ManyLocals_Recursion` | 50+ локальных переменных + рекурсия |
| `Compile_FpuSpill_Deep` | Функция с 9 float-аргументами, рекурсивный вызов |

---

## 7. План реализации (6 шагов)

| # | Файл | Изменение | Сложность |
|---|------|-----------|-----------|
| 1 | `CodeGenerator.cs` | `_liveVars` + `_fpuLive` трекинг в `LoadV`/`Store`/`EmitFloat` | Низкая |
| 2 | `CodeGenerator.cs` | Spill locals + FPU в `EmitCall` перед caller-saved | Средняя |
| 3 | `CodeGenerator.cs` | Restore FPU + locals после caller-saved в `EmitCall` | Средняя |
| 4 | `CodeGenerator.cs` | `_liveVars.Clear()` + `_fpuLive = false` после каждого CALL | Низкая |
| 5 | `T3Fpu.cs` | Проверить `ToInt`/`FromInt` — bit-preserving roundtrip | Низкая |
| 6 | `TLangCompilerTests.cs` | 6 новых тестов (глубокая рекурсия, float spill) | Низкая |

---

## 8. Ожидаемый результат

| Метрика | До (v2) | После (v3) |
|---------|---------|-------------|
| Всего тестов | 122 | 128+ |
| Рекурсия с локальными переменными | ❌ ломается | ✅ |
| Float в рекурсивных функциях | ❌ портятся | ✅ |
| Глубина рекурсии | ~1 (перезапись) | Не ограничена (доступная память стека) |
| Размер кода на вызов | ~24 слова | ~24 + N×2 + 18 слов |