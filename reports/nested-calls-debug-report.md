# Отчёт: Диагностика Nested Calls в компиляторе T-lang

## 1. Проблема

Тест `Equiv_NestedFunctionCalls_Compiler` падает по таймауту (5s) — скомпилированный код зависает в бесконечном цикле.

**Исходный код:**
```c
tint f(tint x){return x*2;}
tint g(tint x){return f(x+1);}
tint main(){return g(5);}
```

**Ожидаемый результат:** `g(5) = f(5+1) = f(6) = 6*2 = 12`

## 2. Сгенерированный ASM

Файл: [`test_nested.asm`](../test_nested.asm)

### Функция `f` (строка 7-27)
```asm
f:
    PUSH RZ           ; сохранить старый FP
    PUSH R3           ; callee-saved
    PUSH R4           ; callee-saved
    MOV RZ, SP        ; RZ = FP (указывает на Saved R4)
    SUBI SP, SP, 1    ; выделить 1 слот для параметра x
    STOREI RW, RZ, -1 ; сохранить x (RW) в local[0] = RZ-1
    LOADI RW, RZ, -1  ; загрузить x обратно
    PUSH RW           ; защитить от переиспользования
    LI RX, 2
    POP RY
    MUL R0, RY, RX    ; R0 = x * 2
    MOV R2, R0        ; результат в R2
    LIMM RX, epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 1    ; освободить локальные
    POP R4
    POP R3
    POP RZ
    RET
```

### Функция `g` (строка 28-62)
```asm
g:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RW, RZ, -1  ; сохранить x=5
    LOADI RW, RZ, -1   ; загрузить x=5
    PUSH RW            ; push 5
    LI RX, 1
    POP RY             ; RY = 5
    ADD R0, RY, RX     ; R0 = 5 + 1 = 6
    PUSH RW            ; сохранить RW (5) — caller-saved
    PUSH RX            ; сохранить RX (1)
    PUSH RY            ; сохранить RY (5)
    PUSH R0            ; сохранить R0 (6)
    PUSH R1            ; сохранить R1
    MOV RW, R0         ; arg0 = 6
    LIMM R1, f
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX, R2         ; RX = результат f (12)
    MOV R2, RX         ; R2 = 12
    LIMM RY, epilogue_1
    JMP RY
epilogue_1:
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
```

### Функция `main` (строка 63-89)
```asm
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    LI RW, 5           ; RW = 5
    PUSH RW            ; сохранить RW (5)
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1, g
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX, R2         ; RX = результат g
    MOV R2, RX
    LIMM RY, epilogue_2
    JMP RY
epilogue_2:
    POP R4
    POP R3
    POP RZ
    RET
```

## 3. Анализ ASM — визуальная трассировка

### Стековый фрейм `main` (до CALL g)

```
[main saved R1]      ← SP (после 5 PUSH)
[main saved R0]
[main saved RY]
[main saved RX]
[main saved RW=5]
[main saved R4]      ← main's RZ
[main saved R3]
[main saved RZ]
[ret addr to __entry]
```

### После CALL g → пролог g

```
[g local x]          ← SP (после SUBI SP, SP, 1)
[g saved R4]         ← g's RZ
[g saved R3]
[g saved RZ]         ← main's RZ
[ret addr to g]      ← от CALL g
[main saved R1]
[main saved R0]
[main saved RY]
[main saved RX]
[main saved RW=5]
[main saved R4]      ← main's RZ
...
```

### g вычисляет x+1 = 6, сохраняет caller-saved, вызывает f

```
[f ret addr]         ← SP (после CALL f)
[g saved R1]
[g saved R0=6]
[g saved RY=5]
[g saved RX=1]
[g saved RW=5]
[g local x=5]        ← g's RZ-1
[g saved R4]         ← g's RZ
...
```

### Пролог f

```
[f local x]          ← SP (после SUBI SP, SP, 1)
[f saved R4]         ← f's RZ
[f saved R3]
[f saved RZ]         ← g's RZ
[f ret addr]
[g saved R1]
[g saved R0=6]
...
```

### f вычисляет x*2 = 12, MOV R2, R0, прыжок на эпилог

### f epilogue: ADDI SP, SP, 1 → POP R4 → POP R3 → POP RZ → RET

После RET f, SP указывает на `[g saved R1]`.

### g восстанавливает: POP R1 → POP R0 → POP RY → POP RX → POP RW

После восстановления, SP указывает на `[g local x]`.

### g: MOV RX, R2 → MOV R2, RX → LIMM RY, epilogue_1 → JMP RY

### g epilogue: ADDI SP, SP, 1 → POP R4 → POP R3 → POP RZ → RET

После RET g, SP указывает на `[main saved R1]`.

### main восстанавливает: POP R1 → POP R0 → POP RY → POP RX → POP RW

### main: MOV RX, R2 → MOV R2, RX → LIMM RY, epilogue_2 → JMP RY

### main epilogue: POP R4 → POP R3 → POP RZ → RET → HALT

**Визуальная трассировка НЕ ВЫЯВЛЯЕТ ОЧЕВИДНЫХ ПРОБЛЕМ.** Все операции со стеком корректны: PUSH/POP сбалансированы, CALL/RET парные, RZ восстанавливается правильно.

## 4. Гипотезы о причине зависания

### Гипотеза A: Проблема в `LIMM` + `JMP` для прыжка на эпилог

Генератор использует:
```asm
LIMM RX, epilogue_0
JMP RX
```

Ассемблер кодирует `LIMM` как I-type с `imm=0`, а следующее слово — адрес метки. Процессор в `LIMM`:
```csharp
case Opcode.LIMM:
    PC++;
    SetRegisterValue(instr.PhysOp1, ReadWord(PC));
    ...
    return false;
```

А в `Step()`:
```csharp
if (!jumped) { PC++; }
```

После LIMM: PC был инкрементирован на 1 внутри ExecuteInstruction (теперь указывает на слово-иммедиат), потом ещё на 1 в Step() (теперь указывает на следующую инструкцию после иммедиата). **Это корректно.**

### Гипотеза B: Проблема в `MOV RZ, SP` — симулятор не поддерживает RZ как SP

В симуляторе `MOV`:
```csharp
case Opcode.MOV:
    SetRegisterValue(instr.PhysOp1, GetRegisterValue(instr.PhysOp2));
```

`GetRegisterValue` использует `logicalIndex`:
```csharp
private TWord GetRegisterValue(int logicalIndex)
{
    if (logicalIndex == 9) return FromLong(SP);
    return Registers[logicalIndex];
}
```

`PhysOp1` для RZ (trit=-1) = -1 + 4 = 3. `Registers[3]` — это обычный регистр, а не SP. **RZ НЕ МАППИТСЯ НА SP.** Это означает, что `MOV RZ, SP` сохраняет значение SP в RZ как в обычный регистр, что корректно. Но `LOADI RW, RZ, -1` использует RZ как базовый регистр, и симулятор читает `Registers[3]`, а не SP. **Это корректно** — RZ хранит адрес фрейма, а не указатель стека.

### Гипотеза C: Проблема в `LIMM` с прыжком на эпилог — неверный адрес

Если ассемблер неправильно вычисляет адрес метки `epilogue_0`, `epilogue_1` или `epilogue_2`, процессор может прыгнуть не туда. Но все остальные тесты (36 из 37 Equiv) проходят, что говорит о корректной работе ассемблера в целом.

### Гипотеза D: Проблема в `LIMM` с меткой функции — `LIMM R1, f`

Ассемблер кодирует `LIMM R1, f` как:
```
EncodeI(pred=0, opcode=LIMM, op1=1(R1), imm=0)
```
следом слово с адресом `f`.

Процессор читает:
```csharp
case Opcode.LIMM:
    PC++;
    SetRegisterValue(instr.PhysOp1, ReadWord(PC));
```

`PhysOp1` для R1 (trit=1) = 1 + 4 = 5. `Registers[5]` = R1. **Корректно.**

### Гипотеза E: Проблема в `CALL` — неверный адрес возврата

```csharp
case Opcode.CALL:
    SP -= 1;
    WriteWord(SP, FromLong(PC + 1));
    if (instr.Immediate == 0)
        PC = (int)ToLong(GetRegisterValue(instr.PhysOp1));
```

`PC + 1` — адрес следующей инструкции после CALL. CALL — это J-type (1 слово), так что `PC + 1` корректен. **Проблемы нет.**

### Гипотеза F: Проблема в `LIMM` при прыжке на эпилог — несовместимость с predication check

В `Step()`:
```csharp
if (!EvaluatePredicate(instr.Predicate))
{
    if (instr.Opcode == Opcode.LIMM) PC += 2;
    else PC++;
    return true;
}
```

Если `LIMM` имеет predicate != 0 и он false, то PC += 2 пропускает LIMM и его иммедиат. **Корректно.**

## 5. Вывод

**Визуальный анализ ASM не выявил очевидной причины зависания.** Сгенерированный код выглядит корректным с точки зрения ABI v4: стек сбалансирован, RZ сохраняется/восстанавливается, caller-saved регистры сохраняются перед вызовом.

**Наиболее вероятные скрытые причины:**

1. **Проблема в симуляторе/декодере** — какая-то инструкция декодируется или выполняется неверно, что приводит к бесконечному циклу. Например, `LIMM` может неправильно взаимодействовать с `JMP` при определённых условиях.

2. **Проблема с адресацией LIMM** — если ассемблер неправильно вычисляет адрес для `LIMM R1, f` или `LIMM RX, epilogue_0`, процессор может прыгнуть в неверное место.

3. **Проблема с `LIMM` в predication check** — если `LIMM` имеет ненулевой predicate, и он срабатывает ложно, то `PC += 2` может пропустить не те слова.

**Для дальнейшей диагностики необходимо:**
- Запустить процессор с включённым трейсингом (`EnableTracing`) для этого конкретного теста
- Проанализировать полный лог выполнения до момента зависания
- Проверить, какая именно инструкция выполняется в цикле

## 6. Рекомендации по исправлению

### Вариант 1: Изменить `EmitCall()` — сохранять caller-saved регистры ДО вычисления аргументов

В [`EmitCall()`](../src/T3Compiler/CodeGen/CodeGenerator.cs:655) аргументы вычисляются до сохранения регистров. Если при вычислении аргументов используются RW/RX/RY/R0/R1, их значения теряются. Решение: сохранять регистры до вычисления аргументов.

### Вариант 2: Не сохранять caller-saved регистры в `EmitCall()`

Если caller-saved регистры не содержат живых значений на момент вызова (что обычно верно для leaf-функций), их сохранение избыточно. Однако это может сломать другие случаи.

### Вариант 3: Использовать `LI` вместо `LIMM` для прыжка на эпилог

Вместо:
```asm
LIMM RX, epilogue_0
JMP RX
```
использовать прямую метку в JMP (ассемблер сам развернёт в LIMM R1 + JMP R1):
```asm
JMP epilogue_0
```

### Вариант 4: Полный редизайн `EmitCall()` с учётом вложенных вызовов

Текущий ABI v4 не рассчитан на вложенные вызовы с параметрами, требующими вычислений. Необходимо:
1. Вычислить все аргументы во временные регистры/стек
2. Сохранить caller-saved регистры
3. Загрузить аргументы в регистры для вызова
4. Вызвать функцию
5. Восстановить caller-saved регистры