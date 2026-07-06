# Детальный план исправления критических дефектов (P1)

Этот документ описывает технические решения для устранения архитектурных дыр и багов в T3Sharp, выявленных в ходе ревью v3.

---

## 1. RegGroup и Ассемблер: Закрытие молчаливых путей

### 1.1. Проблема "Дырявой матрицы"
В текущей реализации `T3InOrderAssembler` группа инструкции определяется по принципу "самой приоритетной" найденной группы.

**Пример бага:**
Инструкция: `ADD R1, SP, R2`
1. Ассемблер видит `SP` (группа Special).
2. Устанавливает `RegGroup = +1`.
3. Кодирует инструкцию как `ADD` с `RegGroup=+1` и индексами регистров `R1` (1), `SP` (2), `R2` (3).
4. **Результат в процессоре**: Процессор выполняет `ADD` в Special ALU. Поскольку `RegGroup=+1`, он обращается к регистру с индексом 1 в группе Special. 
   - В группе Special индекс 1 — это `HP` или `SP`.
   - Результат записывается в `HP` (или другой спец-регистр), а не в `R1`.
   - **Это приводит к молчаливой порче состояния системы.**

### 1.2. Решение: Строгая валидация групп
Ввести метод `ValidateGroupConsistency`, который вызывается перед кодированием.

**Пример реализации:**
```csharp
void ValidateGroupConsistency(string mnemonic, string[] operands) {
    int? detectedGroup = null;
    foreach (var op in operands) {
        if (IsRegister(op)) {
            int group = GetRegGroup(op);
            if (detectedGroup == null) detectedGroup = group;
            else if (detectedGroup != group) {
                throw new Exception($"Architecture Error: Mixing registers from different groups in instruction {mnemonic}. " +
                                    $"Operand {op} (Group {group}) conflicts with previously detected Group {detectedGroup}.");
            }
        }
    }
}
```

### 1.3. Унификация размера инструкций
Текущий разрыв между `CalcSize` и эмиссией кода ведет к сдвигу меток.

**Решение**: Создать статическую таблицу метаданных инструкций.
```csharp
public static class InstructionMeta {
    public static int GetSize(Opcode op, string[] args, long immValue) {
        // Единая логика:
        // if (op == Opcode.LIMM) return 2;
        // if (op == Opcode.MOV && immValue > 364) return 2;
        // ...
    }
}
```
И использовать этот метод и в `CalcSize`, и в `AssembleCore`.

---

## 2. Компилятор и ABI

### 2.1. Унификация условий в циклах
Сейчас `GenWhile` и `GenFor` не поддерживают `&&` и `||`.

**Решение**: Вынести логику генерации условия в отдельный метод `GenCondition(AstNode node)`.
```csharp
int GenCondition(AstNode node) {
    if (node is BinaryOp bo && (bo.Operator == "&&" || bo.Operator == "||")) {
        // Реализация короткозамкнутого вычисления
        // ...
    }
    // ... остальная логика CMP
}
```

### 2.2. Исправление ABI (Передача > 4 аргументов)
Текущая реализация имеет смещение. Согласно ABI v4, аргументы после 4-го должны располагаться строго по определенному смещению относительно FP.

**План**:
1. Проверить `S.SUB SP, SP, localSize` в прологе.
2. Убедиться, что аргументы $5, 6, \dots$ доступны по смещениям $\text{FP} + \text{offset}$, где offset начинается сразу после области локальных переменных.

### 2.3. Операторы `++`, `--`, `~`
Реализовать их как сахар:
- `x++` $\rightarrow$ `x = x + 1`
- `~x` $\rightarrow$ `x = -x - 1` (или аналогично для троичной логики)

---

## 3. T3Float: Борьба с дрифтом точности

### 3.1. Проблема round-trip
При переходе $\text{Word18} \to \text{T3Float} \to \text{Word18}$ отрицательные числа могут менять значение из-за особенностей `raw % pow12` в C#.

**Решение**:
В `T3Float.FromWord18` заменить стандартный остаток на "симметричный" остаток.
```csharp
long mantissa = raw % pow12;
if (raw < 0 && mantissa != 0) mantissa -= 0; // Коррекция знака остатка
// Дополнительная проверка на границы [-maxMant, maxMant]
```

### 3.2. Тесты на точность
Создать тестовый набор:
- Значения: $-1.0, -0.1, 0.0, 0.1, 1.0, \text{MaxFloat}, \text{MinFloat}$.
- Цикл: $\text{double} \to \text{tfloat} \to \text{double}$.
- Допуск: $\pm 1$ в последнем трите мантиссы.

---

## 4. Nanolib: Переработка `print_int`

### 4.1. Текущий дефект
`print_int` завязан на жестко заданные регистры и порт 0.

**Решение**:
1. Сделать `print_int` более гибким в плане использования временных регистров.
2. Реализовать алгоритм:
   - Обработка знака $\to$ вывод `-` через `putchar`.
   - Вычисление цифр $\text{mod } 10 \to \text{push}$.
   - Вывод из стека $\to \text{putchar}$.
3. Добавить тест на вывод чисел из разных регистров.

---

## 5. Матричный тест RegGroup

Для полной уверенности создать файл `tests/RegGroupMatrix.asm`, содержащий:
- Все комбинации: `(GP, GP, GP)`, `(FPU, FPU, FPU)`, `(Special, Special, Special)`.
- Запрещенные комбинации: `(GP, Special, GP)` и т.д.
- Ожидание: Ассемблер должен выдать ошибку на запрещенных комбинациях.