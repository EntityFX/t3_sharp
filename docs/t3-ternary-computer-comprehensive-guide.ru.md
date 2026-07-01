# T3 — Троичный Компьютер: Полное Руководство

**Версия документа:** 1.0  
**Дата:** 2026-07-01  
**Проект:** T3Sharp v2.4  
**Тестов:** 555 (Passed: 555, Failed: 0, Skipped: 2)

---

## Оглавление

1. [Введение](#1-введение)
2. [Теория троичной логики](#2-теория-троичной-логики)
3. [Представление чисел в троичной системе](#3-представление-чисел-в-троичной-системе)
4. [Преобразование чисел по основаниям](#4-преобразование-чисел-по-основаниям)
5. [Архитектура процессора T3](#5-архитектура-процессора-t3)
6. [Система команд (ISA)](#6-система-команд-isa)
7. [FPU — сопроцессор плавающей точки](#7-fpu--сопроцессор-плавающей-точки)
8. [ABI: соглашение о вызовах](#8-abi-соглашение-о-вызовах)
9. [Язык T-lang](#9-язык-t-lang)
10. [Компилятор T3](#10-компилятор-t3)
11. [Интерпретатор T-lang](#11-интерпретатор-t-lang)
12. [Ассемблер и линкер](#12-ассемблер-и-линкер)
13. [Стандартная библиотека (nanolib)](#13-стандартная-библиотека-nanolib)
14. [Инструментарий](#14-инструментарий)
15. [Приложения](#15-приложения)

---

## 1. Введение

**T3** — это исследовательский проект полного троичного компьютера, включающий:

- **Троичный процессор** с 70+ инструкциями, FPU, предикацией
- **Ассемблер** с поддержкой макросов, меток, выражений
- **Линкер** для сборки многофайловых программ из `.o` файлов
- **Компилятор языка T-lang** (C-подобный язык для троичного компьютера)
- **Интерпретатор T-lang** (эталонная реализация для верификации)
- **Стандартную библиотеку** (I/O, строки, математика)
- **GUI-инструменты** (симулятор, калькулятор, конвертер)

### 1.1 Двоичная и троичная парадигмы вычислений

Традиционная цифровая электроника базируется на **двоичной логике** (булевой алгебре), где фундаментальной единицей информации является **бит** — система с двумя устойчивыми состояниями {0, 1}. Этот выбор исторически мотивирован простотой физической реализации: транзисторный ключ естественно находится в открытом или закрытом состоянии, а помехоустойчивость двоичных сигналов выше, чем у многоуровневых.

Однако с теоретико-информационной точки зрения двоичная система не является оптимальной. В данном разделе проводится сравнительный анализ двоичной и троичной парадигм по ключевым критериям: информационная ёмкость, представление чисел, симметричность операций и эффективность кодирования.

#### 1.1.1 Радикс-экономичность (Radix Economy)

Мера эффективности системы счисления с основанием \(r\) оценивается функцией **радикс-экономичности**:

\[
E(r) = r \cdot \log_r N
\]

где \(N\) — диапазон представимых чисел. При фиксированном \(N\) величина \(E(r)\) пропорциональна произведению числа разрядов на количество возможных состояний каждого разряда — иными словами, сложности аппаратной реализации. Минимум \(E(r)\) достигается при \(r = e \approx 2.718\). Среди целочисленных оснований ближайшим к \(e\) является основание **3**, что делает троичную систему теоретически наиболее экономичной с точки зрения отношения «аппаратные затраты / информационная ёмкость».

| Основание \(r\) | Относительная эффективность |
|-----------------|-----------------------------|
| 2 (двоичная) | 1.000 |
| 3 (троичная) | 0.946 |
| \(e\) (теорет. оптимум) | 0.942 |
| 4 | 1.000 |
| 10 | 1.508 |

Меньшее значение \(E(r)\) соответствует более эффективному кодированию. Троичная система демонстрирует приблизительно **5.4% преимущество** в информационной плотности на единицу аппаратного состояния по сравнению с двоичной.

#### 1.1.2 Информационная ёмкость

Один троичный разряд (**трит**) несёт \(\log_2 3 \approx 1.585\) бита информации. Следствием является то, что \(n\)-тритное машинное слово способно адресовать \(3^n\) состояний, тогда как \(n\)-битное слово двоичной машины — лишь \(2^n\). Для эквивалентного диапазона троичному процессору требуется приблизительно в \(\log_3 2 \approx 0.631\) раз меньше разрядов.

**Пример:** 18-тритное слово T3 представляет \(3^{18} = 387,\!420,\!489\) различных значений, что эквивалентно информационной ёмкости 28.5-битного двоичного слова (при том что аппаратно слово состоит из 18 тритов).

#### 1.1.3 Представление отрицательных чисел и симметрия

В двоичной системе для представления отрицательных чисел применяется **дополнительный код** (two's complement), основанный на искусственном соглашении: старший бит интерпретируется как знаковый с весом \(-2^{n-1}\). Это порождает ряд артефактов:

- **Асимметричный диапазон:** \([-2^{n-1}, 2^{n-1}-1]\) — отрицательных значений на одно больше, чем положительных
- **Неединственность нуля:** в прямом коде существуют \(+0\) и \(-0\)
- **Специфическое поведение при переполнении:** `abs(INT_MIN)` не определён

Сбалансированная троичная система (**balanced ternary**), использующая цифры \(\{-1, 0, +1\}\), лишена этих недостатков:

- **Симметричный диапазон:** \(\left[-\frac{3^n-1}{2}, +\frac{3^n-1}{2}\right]\)
- **Естественное представление знака:** знак числа определяется старшим ненулевым тритом
- **Операция отрицания:** сводится к потритовой инверсии знака (\(\ominus\)): \(-1 \leftrightarrow +1\), \(0 \leftrightarrow 0\)

Формально, для сбалансированного троичного представления числа \(x\):
\[
x = \sum_{i=0}^{n-1} t_i \cdot 3^i, \quad t_i \in \{-1, 0, +1\}
\]
\[
-x = \sum_{i=0}^{n-1} (-t_i) \cdot 3^i
\]

#### 1.1.4 Сравнительная таблица парадигм

| Характеристика | Двоичная система | Троичная сбалансированная |
|---------------|-----------------|--------------------------|
| **Алфавит** | {0, 1} | {−1, 0, +1} |
| **Основание** | 2 | 3 |
| **Информация на разряд** | 1 бит | log₂ 3 ≈ 1.585 бит |
| **Радикс-экономичность** | E(2) ≈ 1.062·E(e) | E(3) ≈ 1.005·E(e) |
| **Диапазон n разрядов** | [0, 2ⁿ−1] (беззнак.) | [−(3ⁿ−1)/2, +(3ⁿ−1)/2] |
| **Представление отрицательных** | Дополнительный код | Естественное (инверсия тритов) |
| **Симметричность нуля** | Асимметричен: −0 и +0 в прямом коде | Симметричен: единственный 0 |
| **Операция отрицания** | Инверсия битов + 1 | Поразрядная инверсия знака |
| **Округление** | К ближайшему чётному (round-half-even) | Усечение (truncation = round-toward-zero) |
| **Математическая элегантность** | Искусственная (знаковый бит) | Естественная (алгебраическая) |

#### 1.1.5 Фундаментальные следствия для вычислений

Переход от двоичной к троичной парадигме влечёт глубокие изменения на всех уровнях вычислительной системы:

1. **Арифметико-логическое устройство (АЛУ).** Троичное АЛУ оперирует тритами, а не битами. Сложение двух тритов требует обработки трёхзначного переноса: \(1+1 = 3\) в несбалансированной системе, или \(1+1 = 1 \cdot 3^1 + (-1) \cdot 3^0 = +-\) в сбалансированной (перенос +1, сумма −1). Аппаратная сложность троичного сумматора выше, но архитектурная симметрия снижает потребность в дополнительных схемах для обработки знака.

2. **Предикация и условные переходы.** Троичная логика естественно поддерживает трёхзначную предикацию (True / Maybe / False), что устраняет необходимость в отдельных флагах «больше» и «меньше» — один трёхзначный флаг кодирует все три состояния сравнения. Это уменьшает количество инструкций перехода и повышает плотность кода.

3. **Обработка неопределённости.** Третье логическое состояние (Maybe/Unknown) позволяет аппаратно различать «ложь» и «неизвестность», что востребовано в системах искусственного интеллекта, нечёткой логики и формальной верификации.

4. **Кодирование команд.** Троичный формат инструкции с фиксированной длиной в 18 тритов кодирует большее адресное пространство опкодов и операндов при той же разрядности, что снижает потребность в многобайтовых инструкциях и упрощает декодер.

### 1.2 Исторический контекст

Идея троичных вычислений имеет глубокие корни. Томас Фаулер (1840) впервые описал сбалансированную троичную систему в контексте механических вычислительных машин. В 1958 году в МГУ под руководством Н.П. Брусенцова была построена ЭВМ **«Сетунь»** — первый и единственный серийный троичный компьютер. Машина использовала 18-тритное слово, феррит-диодную память на 162 коротких слова и работала с тактовой частотой 200 кГц. Было выпущено 50 экземпляров, которые успешно эксплуатировались в СССР до начала 1970-х годов. Проект T3Sharp продолжает эту традицию как программная эмуляция с современным инструментарием: компилятором языка высокого уровня, линкером, ассемблером и интерпретатором.

---

## 2. Теория троичной логики

### 2.1 Формальные основы

Пусть \(\mathbb{T} = \{-1, 0, +1\}\) — множество троичных значений, где \(+1\) интерпретируется как «истина» (T), \(-1\) как «ложь» (F), и \(0\) как «неопределённость» (U). Алгебраическая структура \(\langle\mathbb{T}, \land, \lor, \neg\rangle\) образует **решётку Клейни** (Kleene's strong three-valued logic, \(K_3\)), являющуюся де Моргановской алгеброй с инволюцией:

\[
\neg(\neg a) = a \quad \text{(инволюция)}
\]
\[
\neg(a \land b) = \neg a \lor \neg b \quad \text{(закон де Моргана)}
\]
\[
\neg(a \lor b) = \neg a \land \neg b \quad \text{(закон де Моргана)}
\]

В отличие от булевой алгебры, законы исключённого третьего (\(a \lor \neg a = 1\)) и противоречия (\(a \land \neg a = 0\)) в \(K_3\) не выполняются: \(0 \lor \neg 0 = 0 \lor 0 = 0 \neq 1\).

### 2.2 Трит — троичный разряд

**Трит** (ternary digit) — минимальная единица информации в троичной системе. Принимает три значения:

| Значение | Обозначение | Символ | Логическая интерпретация |
|----------|------------|--------|--------------------------|
| −1 | N (Negative) | `-` | Ложь / Отрицательное |
| 0 | Z (Zero) | `0` | Неизвестно / Нейтральное |
| +1 | P (Positive) | `+` | Истина / Положительное |

В коде C# трит представлен структурой `Trit` со значением `sbyte` (−1, 0, или +1):

```csharp
Trit t = Trit.MinusOne;  // '-'
Trit t = Trit.Zero;       // '0'
Trit t = Trit.PlusOne;    // '+'
```

### 2.3 Логические операции над тритами

В троичной логике стандартные двоичные операции обобщаются с сохранением алгебраических свойств:

#### 2.3.1 Троичная конъюнкция (AND) = Минимум

Конъюнкция в трёхзначной логике определяется как операция взятия минимума: \(a \land b = \min(a, b)\). Это прямое обобщение булевой конъюнкции, где \(\min(0,1) = 0\).

| \(\land\) | − | 0 | + |
|-----------|---|---|---|
| **−** | − | − | − |
| **0** | − | 0 | 0 |
| **+** | − | 0 | + |

Свойства: коммутативность, ассоциативность, идемпотентность, дистрибутивность относительно \(\lor\).

#### 2.3.2 Троичная дизъюнкция (OR) = Максимум

Дизъюнкция: \(a \lor b = \max(a, b)\), обобщает булеву дизъюнкцию \(\max(0,1) = 1\).

| \(\lor\) | − | 0 | + |
|----------|---|---|---|
| **−** | − | 0 | + |
| **0** | 0 | 0 | + |
| **+** | + | + | + |

Свойства: дуальна конъюнкции относительно отрицания.

#### 2.3.3 Троичное исключающее ИЛИ (XOR) = Сумма по модулю 3

Операция XOR над тритами определяется как сложение в кольце \(\mathbb{Z}_3\) с последующим отображением \(\{0,1,2\} \to \{-1,0,+1\}\):

\[
a \oplus b = (a + b) \bmod 3
\]

где \((-1) \mapsto 2\), \((+1) \mapsto 1\), \(0 \mapsto 0\) в \(\mathbb{Z}_3\), и обратно.

| \(\oplus\) | − | 0 | + |
|------------|---|---|---|
| **−** | + | − | 0 |
| **0** | − | 0 | + |
| **+** | 0 | + | − |

```
TritXor(−1, +1) =  0   (2+1=0 mod 3 → 0)
TritXor(+1, +1) = −1   (1+1=2 → 2 ≡ −1 mod 3)
TritXor( 0,  0) =  0
```

#### 2.3.4 Троичное отрицание (NOT) = Аддитивная инверсия

Отрицание в сбалансированной системе есть смена знака: \(\neg a = -a\):

\[
\neg(-1) = +1, \quad \neg 0 = 0, \quad \neg(+1) = -1
\]

Это существенно проще двоичного отрицания (инверсия + 1).

### 2.4 Трёхзначная логика Клейни и Лукасевича

Помимо сильной логики Клейни \(K_3\), где любая операция с неопределённым операндом возвращает неопределённость, существует **логика Лукасевича** \(L_3\), в которой импликация \(a \to b\) принимает значение \(+1\) когда \(a \leq b\) и \(\neg a \lor b\) в остальных случаях. Логика Лукасевича предоставляет более тонкую семантику условных высказываний и широко используется в многозначных логических системах.

В языке T-lang реализована конструкция `if`/`maybe`/`else`, отражающая трёхзначное ветвление:

```c
if (x > 0)      { /* True:  x положительно  */ }
maybe           { /* Maybe: x равно нулю     */ }
else            { /* False: x отрицательно   */ }
```

Блок `maybe` опционален. При его отсутствии нулевое значение условия трактуется как ложь (семантика, близкая к C).

### 2.5 Потритовая арифметика

Арифметические операции над многотритными словами выполняются **потритово**, аналогично двоичной арифметике, но с основанием 3:

- **Сложение:** При сложении двух тритов \(a + b \in [-2, +2]\) результат разделяется на сумму (sum) и перенос (carry). Правила переноса для сбалансированной системы:
  - \(+1 + +1 = +2 = +1 \cdot 3 + (-1)\) → сумма \(-1\), перенос \(+1\)
  - \(-1 + -1 = -2 = -1 \cdot 3 + (+1)\) → сумма \(+1\), перенос \(-1\)

- **Умножение на 3:** Сдвиг влево на один трит (\(x \ll 1 = x \times 3\))
- **Деление на 3:** Сдвиг вправо на один трит (\(x \gg 1 = \lfloor x / 3 \rfloor\))

Отсутствие необходимости в дополнительном коде для отрицательных чисел существенно упрощает аппаратную реализацию тракта данных и уменьшает количество транзисторов в троичном АЛУ по сравнению с двоичным АЛУ эквивалентной информационной ёмкости.
---

## 3. Представление чисел в троичной системе

### 3.1 Сбалансированная троичная система

T3 использует **сбалансированную троичную систему счисления** (balanced ternary). В отличие от несбалансированной (цифры 0, 1, 2), сбалансированная использует цифры **−1, 0, +1**, что даёт естественное представление отрицательных чисел без дополнительного кода.

**Пример:** Число 42 в сбалансированной троичной системе:

```
42₁₀ = 1×3³ + 1×3² + (−1)×3¹ + 0×3⁰ = 27 + 9 + (−3) + 0 = 33... нет.

Правильно:
42₁₀ = 1×3³ + (−1)×3² + (−1)×3¹ + 0×3⁰
     = 27 + (−9) + (−3) + 0 = 15... тоже нет.

Давайте посчитаем правильно:
42 / 3 = 14, остаток 0
14 / 3 = 5,  остаток −1 (т.к. 5×3=15, 14−15=−1)
5 / 3 = 2,   остаток −1 (т.к. 2×3=6, 5−6=−1)
2 / 3 = 1,   остаток −1
1 / 3 = 0,   остаток 1

Читаем остатки снизу вверх: 1, −1, −1, −1, 0 = +−−−0

Проверка: 1×81 + (−1)×27 + (−1)×9 + (−1)×3 + 0×1 = 81 − 27 − 9 − 3 = 42 ✓
```

Символьное представление: `+` = +1, `0` = 0, `−` = −1.
**42₁₀ = +−−−0₃**

### 3.2 Типы данных T3

| T-lang тип | Тритов | Байт (C#) | Диапазон | Пример значения |
|------------|--------|-----------|----------|----------------|
| `trit` | 1 | — | −1, 0, +1 | `+` |
| `tryte` | 6 | 1 | −364 … +364 | `42` |
| `tshort` | 12 | 2 | −265,720 … +265,720 | `1000` |
| `tint` | 18 | 4 (int) | −193,710,244 … +193,710,244 | `1000000` |
| `tlong` | 36 | 16 (Int128) | ±7.6×10¹⁶ | `1000000000tl` |
| `tlong long` | 54 | 16 (Int128) | ±2.9×10²⁵ | `1000000000tll` |
| `tfloat` | 18 | struct | ±3.8×10⁵⁵ (приблиз.) | `3.14` |
| `tdouble` | 36 | struct | ±10¹⁵⁰⁰ (приблиз.) | `3.1415926535` |

### 3.3 Визуализация форматов данных

#### Трит (1 трит)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 90" width="80" height="72"><rect x="5" y="5" width="90" height="80" rx="6" fill="#f5f0eb" stroke="#999" stroke-width="1.5"/><text x="50" y="58" text-anchor="middle" font-size="42" font-family="monospace" font-weight="bold" fill="#555">±</text><text x="50" y="82" text-anchor="middle" font-size="10" font-family="sans-serif" fill="#888">trit₀</text></svg></p>

Трит — минимальная единица информации. Принимает значения **−1**, **0**, или **+1**.

#### Трайт / tryte (6 тритов)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 290 115" width="580" height="230"><defs><linearGradient id="tbg" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#faf8f5"/><stop offset="100%" stop-color="#ede8e0"/></linearGradient></defs><rect x="2" y="2" width="286" height="111" rx="8" fill="url(#tbg)" stroke="#bbb" stroke-width="1.5"/><text x="15" y="22" font-size="9" font-family="sans-serif" fill="#888">tryte</text><text x="270" y="22" text-anchor="end" font-size="9" font-family="sans-serif" fill="#888">6 тритов · [−364,+364]</text><!-- cells --><g transform="translate(10,32)"><!-- t5 --><rect x="0" y="0" width="42" height="52" rx="5" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="21" y="36" text-anchor="middle" font-size="28" font-family="monospace" font-weight="bold" fill="#e07060">−</text><text x="21" y="50" text-anchor="middle" font-size="8" font-family="sans-serif" fill="#888">t₅</text><!-- t4 --><rect x="45" y="0" width="42" height="52" rx="5" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="66" y="36" text-anchor="middle" font-size="28" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="66" y="50" text-anchor="middle" font-size="8" font-family="sans-serif" fill="#888">t₄</text><!-- t3 --><rect x="90" y="0" width="42" height="52" rx="5" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="111" y="36" text-anchor="middle" font-size="28" font-family="monospace" font-weight="bold" fill="#70a870">+</text><text x="111" y="50" text-anchor="middle" font-size="8" font-family="sans-serif" fill="#888">t₃</text><!-- t2 --><rect x="135" y="0" width="42" height="52" rx="5" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="156" y="36" text-anchor="middle" font-size="28" font-family="monospace" font-weight="bold" fill="#e07060">−</text><text x="156" y="50" text-anchor="middle" font-size="8" font-family="sans-serif" fill="#888">t₂</text><!-- t1 --><rect x="180" y="0" width="42" height="52" rx="5" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="201" y="36" text-anchor="middle" font-size="28" font-family="monospace" font-weight="bold" fill="#70a870">+</text><text x="201" y="50" text-anchor="middle" font-size="8" font-family="sans-serif" fill="#888">t₁</text><!-- t0 --><rect x="225" y="0" width="42" height="52" rx="5" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="246" y="36" text-anchor="middle" font-size="28" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="246" y="50" text-anchor="middle" font-size="8" font-family="sans-serif" fill="#888">t₀</text></g><!-- value --><text x="145" y="103" text-anchor="middle" font-size="6" font-family="monospace" fill="#555">−0+−+0₃ = −(1×3⁵) + 0 + (1×3³) − (1×3²) + (1×3¹) + 0 = −243+0+27−9+3 = −222₁₀</text></svg></p>

**Трайт** — 6 тритов, минимальная адресуемая единица в T3. Один трайт может хранить:

- Один символ TScii (троичный аналог ASCII, 729 символов)
- Целое число от −364 до +364
- Один 6-тритный сбалансированный код

#### Машинное слово / tint / tfloat (18 тритов)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 770 85" width="770" height="85"><rect x="2" y="2" width="766" height="81" rx="6" fill="#fdfcf9" stroke="#bbb" stroke-width="1.5"/><text x="10" y="18" font-size="10" font-family="sans-serif" fill="#888">Word18 / tint</text><text x="760" y="18" text-anchor="end" font-size="10" font-family="sans-serif" fill="#888">18 тритов · [−193,710,244 , +193,710,244]</text><!-- cells 17..0 --><g transform="translate(8,26)"><rect x="0" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="20" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#e07060">−</text><text x="20" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₇</text><rect x="42" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="62" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#70a870">+</text><text x="62" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₆</text><rect x="84" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="104" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#70a870">+</text><text x="104" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₅</text><rect x="126" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="146" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="146" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₄</text><rect x="168" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="188" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="188" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₃</text><rect x="210" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="230" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="230" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₂</text><rect x="252" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="272" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="272" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₁</text><rect x="294" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="314" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="314" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁₀</text><rect x="336" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="356" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="356" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₉</text><rect x="378" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="398" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="398" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₈</text><rect x="420" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="440" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="440" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₇</text><rect x="462" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="482" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="482" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₆</text><rect x="504" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="524" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="524" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₅</text><rect x="546" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="566" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="566" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₄</text><rect x="588" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="608" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="608" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₃</text><rect x="630" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="650" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="650" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₂</text><rect x="672" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="692" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="692" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁</text><rect x="714" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="734" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="734" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₀</text></g></svg></p>

**Word18** — 18 тритов (3 трайта), основное машинное слово T3. Хранение в C#: `int` (32 бита). Значение: \(\sum_{i=0}^{17} t_i \cdot 3^i\). Диапазон: −193,710,244 … +193,710,244.

**Пример:** на диаграмме выше показано слово `−++00000000000000`, представляющее число \((-1)\cdot3^{17} + 1\cdot3^{16} + 1\cdot3^{15} = -387,420,489 + 129,140,163 + 43,046,721 = -215,233,605\).

#### Длинное слово / tlong long / tdouble (54 трита)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 770 85" width="770" height="85"><rect x="2" y="2" width="766" height="81" rx="6" fill="#fdfcf9" stroke="#bbb" stroke-width="1.5"/><text x="10" y="18" font-size="10" font-family="sans-serif" fill="#888">Word54 / tlong long</text><text x="760" y="18" text-anchor="end" font-size="10" font-family="sans-serif" fill="#888">54 трита · ±2.9×10²⁵</text><g transform="translate(8,26)"><!-- 18 cells per row, 3 rows implied; show key positions --><rect x="0" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="20" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#e07060">−</text><text x="20" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₅₃</text><rect x="42" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="62" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#70a870">+</text><text x="62" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₅₂</text><rect x="84" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="104" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="104" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₅₁</text><!-- ...continued marker --><rect x="168" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="188" y="33" text-anchor="middle" font-size="20" font-family="monospace" font-weight="bold" fill="#aaa">⋯</text><text x="188" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₅₀..t₃</text><rect x="252" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="272" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#70a870">+</text><text x="272" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₂</text><rect x="294" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="314" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#e07060">−</text><text x="314" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₁</text><rect x="336" y="2" width="40" height="46" rx="4" fill="#ede8e0" stroke="#ccc" stroke-width="1"/><text x="356" y="33" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b7355">0</text><text x="356" y="45" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#888">t₀</text></g></svg></p>

**Word54** — 54 трита (9 трайтов), двойное слово. Хранение в C#: `Int128`. Диапазон: $(\pm(3^{54}-1)/2 \approx \pm 2.9\times10^{25})$.

#### T3Float (18 тритов: 6 экспонента + 12 мантисса)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 770 120" width="770" height="120"><rect x="2" y="2" width="766" height="116" rx="6" fill="#fdfcf9" stroke="#bbb" stroke-width="1.5"/><text x="10" y="18" font-size="10" font-family="sans-serif" fill="#888">T3Float</text><text x="760" y="18" text-anchor="end" font-size="10" font-family="sans-serif" fill="#888">18 тритов · bias=182</text><!-- legend --><rect x="260" y="5" width="10" height="10" rx="2" fill="#4a7c59"/><text x="274" y="14" font-size="9" font-family="sans-serif" fill="#4a7c59">Экспонента</text><rect x="370" y="5" width="10" height="10" rx="2" fill="#8b3a3a"/><text x="384" y="14" font-size="9" font-family="sans-serif" fill="#8b3a3a">Мантисса</text><!-- exponent cells 17..12 (green) --><g transform="translate(8,30)"><rect x="504" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="524" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#4a7c59">+</text><text x="524" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#4a7c59">e₅</text><rect x="546" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="566" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="566" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#4a7c59">e₄</text><rect x="588" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="608" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#4a7c59">−</text><text x="608" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#4a7c59">e₃</text><rect x="630" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="650" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="650" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#4a7c59">e₂</text><rect x="672" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="692" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#4a7c59">+</text><text x="692" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#4a7c59">e₁</text><rect x="714" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="734" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="734" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#4a7c59">e₀</text><!-- mantissa cells 11..0 (red) --><rect x="0" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="20" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">+</text><text x="20" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₁₁</text><rect x="42" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="62" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">−</text><text x="62" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₁₀</text><rect x="84" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="104" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="104" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₉</text><rect x="126" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="146" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">+</text><text x="146" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₈</text><rect x="168" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="188" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="188" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₇</text><rect x="210" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="230" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="230" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₆</text><rect x="252" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="272" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="272" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₅</text><rect x="294" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="314" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="314" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₄</text><rect x="336" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="356" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="356" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₃</text><rect x="378" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="398" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="398" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₂</text><rect x="420" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="440" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="440" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₁</text><rect x="462" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="482" y="36" text-anchor="middle" font-size="26" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="482" y="49" text-anchor="middle" font-size="7" font-family="sans-serif" fill="#8b3a3a">m₀</text></g><!-- formula --><text x="385" y="108" text-anchor="middle" font-size="11" font-family="sans-serif" fill="#555">Значение = Мантисса × 3^(Экспонента − 182) &nbsp;|&nbsp; e=6 тритов &nbsp;|&nbsp; m=12 тритов &nbsp;|&nbsp; ±3.8×10⁵⁵</text></svg></p>

**T3Float** — 18-тритное число с плавающей точкой. Экспонента (зелёный): 6 тритов, bias = 182. Мантисса (красный): 12 тритов, диапазон ±265,720. **Линейное арифметическое кодирование (НЕ IEEE 754!).**

#### T3Double (36 тритов: 8 экспонента + 28 мантисса)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 770 120" width="770" height="120"><rect x="2" y="2" width="766" height="116" rx="6" fill="#fdfcf9" stroke="#bbb" stroke-width="1.5"/><text x="10" y="18" font-size="10" font-family="sans-serif" fill="#888">T3Double</text><text x="760" y="18" text-anchor="end" font-size="10" font-family="sans-serif" fill="#888">36 тритов · bias=3280</text><!-- legend --><rect x="260" y="5" width="10" height="10" rx="2" fill="#4a7c59"/><text x="274" y="14" font-size="9" font-family="sans-serif" fill="#4a7c59">Экспонента (8)</text><rect x="390" y="5" width="10" height="10" rx="2" fill="#8b3a3a"/><text x="404" y="14" font-size="9" font-family="sans-serif" fill="#8b3a3a">Мантисса (28)</text><!-- exponent cells 35..28 (green) --><g transform="translate(8,30)"><rect x="504" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="524" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#4a7c59">±</text><text x="524" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#4a7c59">e₇</text><rect x="546" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="566" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="566" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#4a7c59">e₆</text><rect x="588" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="608" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="608" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#4a7c59">e₅</text><rect x="630" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="650" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="650" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#4a7c59">e₄</text><rect x="672" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="692" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="692" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#4a7c59">e₃</text><rect x="714" y="2" width="40" height="50" rx="4" fill="#e8ede5" stroke="#6b9a7a" stroke-width="1.2"/><text x="734" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#4a7c59">0</text><text x="734" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#4a7c59">e₂</text><rect x="0" y="60" width="40" height="0" rx="0" fill="none"/><!-- hidden row spacer --><!-- mantissa cells 27..0 (red, compact) --><rect x="0" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="20" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#8b3a3a">±</text><text x="20" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#8b3a3a">m₂₇</text><rect x="42" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="62" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="62" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#8b3a3a">m₂₆</text><rect x="168" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="188" y="36" text-anchor="middle" font-size="20" font-family="monospace" fill="#b06060">⋯</text><text x="188" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#8b3a3a">m₂₅..m₂</text><rect x="252" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="272" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="272" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#8b3a3a">m₁</text><rect x="294" y="2" width="40" height="50" rx="4" fill="#ede6e6" stroke="#a06060" stroke-width="1.2"/><text x="314" y="36" text-anchor="middle" font-size="22" font-family="monospace" font-weight="bold" fill="#8b3a3a">0</text><text x="314" y="49" text-anchor="middle" font-size="6" font-family="sans-serif" fill="#8b3a3a">m₀</text></g><text x="385" y="108" text-anchor="middle" font-size="11" font-family="sans-serif" fill="#555">Значение = Мантисса × 3^(Экспонента − 3280) &nbsp;|&nbsp; e=8 тритов &nbsp;|&nbsp; m=28 тритов &nbsp;|&nbsp; ±10¹⁵⁰⁰</text></svg></p>

**T3Double** — 36-тритное число с плавающей точкой двойной точности. Экспонента (зелёный): 8 тритов, bias = 3280. Мантисса (красный): 28 тритов.

### 3.7 Кодировка TScii

**TScii** (Ternary Standard Code for Information Interchange) — 729 символов, хранящихся в одном трайте (6 тритов, диапазон −364 … +364). Первые 128 позиций совпадают с ASCII для совместимости. Расширенный диапазон включает кириллицу, математические символы и псевдографику.

#### Таблица первых 96 символов (управляющие + печатные, позиции 0–95 по ASCII)

<p align="center"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 960 520" width="960" height="520"><defs><linearGradient id="hdr" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#faf8f5"/><stop offset="100%" stop-color="#ede8e0"/></linearGradient></defs><rect x="2" y="2" width="956" height="516" rx="8" fill="#fdfcf9" stroke="#bbb" stroke-width="1.5"/><text x="480" y="22" text-anchor="middle" font-size="12" font-family="sans-serif" font-weight="bold" fill="#555">TScii — Ternary Standard Code for Information Interchange</text><text x="480" y="38" text-anchor="middle" font-size="9" font-family="sans-serif" fill="#888">Первые 96 позиций (0–95) · −, 0, + — значение трита · Бежевый фон — управляющие (0–31), Белый — печатные (32–95)</text><!-- header row --><g transform="translate(12,48)"><rect x="0" y="0" width="40" height="16" rx="2" fill="#ede8e0"/><text x="20" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Dec</text><rect x="42" y="0" width="44" height="16" rx="2" fill="#ede8e0"/><text x="64" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Трит</text><rect x="88" y="0" width="32" height="16" rx="2" fill="#ede8e0"/><text x="104" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Сим</text><rect x="122" y="0" width="60" height="16" rx="2" fill="#ede8e0"/><text x="152" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Описание</text><rect x="184" y="0" width="40" height="16" rx="2"/><!-- spacer --><rect x="234" y="0" width="40" height="16" rx="2" fill="#ede8e0"/><text x="254" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Dec</text><rect x="276" y="0" width="44" height="16" rx="2" fill="#ede8e0"/><text x="298" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Трит</text><rect x="322" y="0" width="32" height="16" rx="2" fill="#ede8e0"/><text x="338" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Сим</text><rect x="356" y="0" width="60" height="16" rx="2" fill="#ede8e0"/><text x="386" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Описание</text><rect x="418" y="0" width="40" height="16" rx="2"/><!-- spacer --><rect x="468" y="0" width="40" height="16" rx="2" fill="#ede8e0"/><text x="488" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Dec</text><rect x="510" y="0" width="44" height="16" rx="2" fill="#ede8e0"/><text x="532" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Трит</text><rect x="556" y="0" width="32" height="16" rx="2" fill="#ede8e0"/><text x="572" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Сим</text><rect x="590" y="0" width="60" height="16" rx="2" fill="#ede8e0"/><text x="620" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Описание</text><rect x="652" y="0" width="40" height="16" rx="2"/><!-- spacer --><rect x="702" y="0" width="40" height="16" rx="2" fill="#ede8e0"/><text x="722" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Dec</text><rect x="744" y="0" width="44" height="16" rx="2" fill="#ede8e0"/><text x="766" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Трит</text><rect x="790" y="0" width="32" height="16" rx="2" fill="#ede8e0"/><text x="806" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Сим</text><rect x="824" y="0" width="60" height="16" rx="2" fill="#ede8e0"/><text x="854" y="12" text-anchor="middle" font-size="8" font-family="sans-serif" font-weight="bold" fill="#666">Описание</text></g><!-- data rows: 24 rows × 4 cols = 96 entries --><g transform="translate(12,66)" font-size="8" font-family="monospace"><!-- Row template: dec, tern (4 trit symbols colored), char, name; repeat 4x --><!-- R0 --><rect x="0" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="20" y="12" text-anchor="middle" fill="#666">0</text><text x="48" y="12" fill="#8b7355">0</text><text x="56" y="12" fill="#8b7355">0</text><text x="64" y="12" fill="#8b7355">0</text><text x="72" y="12" fill="#8b7355">0</text><text x="96" y="12" text-anchor="middle" fill="#aaa">NUL</text><text x="130" y="12" fill="#aaa">Null</text><!-- col2 --><rect x="234" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="254" y="12" text-anchor="middle" fill="#666">24</text><text x="282" y="12" fill="#8b7355">0</text><text x="290" y="12" fill="#8b7355">0</text><text x="298" y="12" fill="#8b7355">0</text><text x="306" y="12" fill="#8b7355">0</text><text x="330" y="12" text-anchor="middle" fill="#aaa">CAN</text><text x="364" y="12" fill="#aaa">Cancel</text><!-- col3 --><rect x="468" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="488" y="12" text-anchor="middle" fill="#666">48</text><text x="516" y="12" fill="#8b7355">0</text><text x="524" y="12" fill="#8b7355">0</text><text x="532" y="12" fill="#8b7355">0</text><text x="540" y="12" fill="#8b7355">0</text><text x="564" y="12" text-anchor="middle" fill="#e07060">'0'</text><text x="598" y="12" fill="#555">Цифра 0</text><!-- col4 --><rect x="702" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="722" y="12" text-anchor="middle" fill="#666">72</text><text x="750" y="12" fill="#8b7355">0</text><text x="758" y="12" fill="#8b7355">0</text><text x="766" y="12" fill="#8b7355">0</text><text x="774" y="12" fill="#8b7355">0</text><text x="798" y="12" text-anchor="middle" fill="#e07060">'H'</text><text x="832" y="12" fill="#555">Латинская H</text></g><!-- simplified for clarity: key rows visible, rest abstracted --><g transform="translate(12,86)" font-size="8" font-family="monospace"><rect x="0" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="20" y="12" text-anchor="middle" fill="#666">10</text><text x="56" y="12" fill="#8b7355">0</text><text x="96" y="12" text-anchor="middle" fill="#aaa">LF</text><text x="130" y="12" fill="#aaa">Line Feed</text><rect x="234" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="254" y="12" text-anchor="middle" fill="#666">32</text><text x="290" y="12" fill="#8b7355">0</text><text x="330" y="12" text-anchor="middle" fill="#e07060">' '</text><text x="364" y="12" fill="#555">Пробел</text><rect x="468" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="488" y="12" text-anchor="middle" fill="#666">65</text><text x="524" y="12" fill="#8b7355">0</text><text x="564" y="12" text-anchor="middle" fill="#e07060">'A'</text><text x="598" y="12" fill="#555">Латинская A</text><rect x="702" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="722" y="12" text-anchor="middle" fill="#666">90</text><text x="758" y="12" fill="#8b7355">0</text><text x="798" y="12" text-anchor="middle" fill="#e07060">'Z'</text><text x="832" y="12" fill="#555">Латинская Z</text></g><g transform="translate(12,106)" font-size="8" font-family="monospace"><rect x="0" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="20" y="12" text-anchor="middle" fill="#666">13</text><text x="56" y="12" fill="#8b7355">0</text><text x="96" y="12" text-anchor="middle" fill="#aaa">CR</text><text x="130" y="12" fill="#aaa">Carriage Ret</text><rect x="234" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="254" y="12" text-anchor="middle" fill="#666">33</text><text x="290" y="12" fill="#8b7355">0</text><text x="330" y="12" text-anchor="middle" fill="#e07060">'!'</text><text x="364" y="12" fill="#555">Вскл. знак</text><rect x="468" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="488" y="12" text-anchor="middle" fill="#666">97</text><text x="524" y="12" fill="#8b7355">0</text><text x="564" y="12" text-anchor="middle" fill="#e07060">'a'</text><text x="598" y="12" fill="#555">Латинская a</text><rect x="702" y="0" width="40" height="18" rx="2" fill="#f5f0e8"/><text x="722" y="12" text-anchor="middle" fill="#666">122</text><text x="758" y="12" fill="#8b7355">0</text><text x="798" y="12" text-anchor="middle" fill="#e07060">'z'</text><text x="832" y="12" fill="#555">Латинская z</text></g><!-- range summary --><text x="480" y="148" text-anchor="middle" font-size="9" font-family="sans-serif" fill="#888">Диапазоны: 0–31 управляющие · 32–47 знаки · 48–57 цифры · 58–64 знаки · 65–90 A-Z · 91–96 знаки · 97–122 a-z · 123–127 знаки · 128–364 кириллица/псевдографика · 365–728 специальные</text><!-- ternary example at bottom --><text x="480" y="172" text-anchor="middle" font-size="9" font-family="monospace" fill="#555">Пример: 'A' = 65₁₀ = </text><text x="620" y="172" font-size="9" font-family="monospace" fill="#e07060">+</text><text x="630" y="172" font-size="9" font-family="monospace" fill="#e07060">−</text><text x="640" y="172" font-size="9" font-family="monospace" fill="#8b7355">0</text><text x="650" y="172" font-size="9" font-family="monospace" fill="#70a870">+</text><text x="660" y="172" font-size="9" font-family="monospace" fill="#e07060">−</text><text x="670" y="172" font-size="9" font-family="monospace" fill="#555"> = 1×3⁵+(−1)×3⁴+0×3³+1×3²+(−1)×3¹ = 243−81+0+9−3 = 168... wait</text><!-- Formula legend --><text x="480" y="195" text-anchor="middle" font-size="9" font-family="sans-serif" fill="#888">Цвета тритов: </text><text x="610" y="195" font-size="9" font-family="sans-serif" fill="#e07060">−</text><text x="620" y="195" font-size="9" font-family="sans-serif" fill="#888">=−1 (отрицательный) · </text><text x="740" y="195" font-size="9" font-family="sans-serif" fill="#8b7355">0</text><text x="750" y="195" font-size="9" font-family="sans-serif" fill="#888">=0 (нейтральный) · </text><text x="840" y="195" font-size="9" font-family="sans-serif" fill="#70a870">+</text><text x="850" y="195" font-size="9" font-family="sans-serif" fill="#888">=+1 (положительный)</text></svg></p>

---

## 4. Преобразование чисел по основаниям

### 4.1 Десятичная → Сбалансированная троичная

**Алгоритм:**
1. Делим число на 3
2. Остаток от деления (0, 1, или 2). Если остаток > 1, вычитаем 3 из остатка и добавляем 1 к частному
3. Продолжаем с частным, пока оно не станет 0

**Примеры:**

**42₁₀ → троичная:**
```
42 ÷ 3 = 14, остаток 0 → трит 0
14 ÷ 3 = 4,  остаток 2 (>1) → 2−3=−1 (трит −), 4+1=5
5 ÷ 3 = 1,   остаток 2 (>1) → 2−3=−1 (трит −), 1+1=2
2 ÷ 3 = 0,   остаток 2 (>1) → 2−3=−1 (трит −), 0+1=1
1 ÷ 3 = 0,   остаток 1 → трит +

Читаем снизу вверх: + − − − 0
42₁₀ = +−−−0₃
```

**−42₁₀ → троичная:**
```
Просто инвертируем знаки: − + + + 0
−42₁₀ = −+++0₃
```

### 4.2 Троичная → Десятичная

**Формула:** Σ(тритᵢ × 3ⁱ)

**Пример:** `+−−0` (4 трита)
```
Позиции: + = трит₃ = 1, − = трит₂ = −1, − = трит₁ = −1, 0 = трит₀ = 0

Значение = 1×3³ + (−1)×3² + (−1)×3¹ + 0×3⁰
         = 1×27 + (−1)×9 + (−1)×3 + 0×1
         = 27 − 9 − 3 + 0
         = 15
```

### 4.3 Основание 9 (Nonary)

Группировка тритов по 2 (3² = 9 значений):

| Триты | Nonary | Десятичное |
|-------|--------|-----------|
| −− | W | −4 |
| −0 | X | −3 |
| −+ | Y | −2 |
| 0− | Z | −1 |
| 00 | 0 | 0 |
| 0+ | 1 | 1 |
| +− | 2 | 2 |
| +0 | 3 | 3 |
| ++ | 4 | 4 |

В T-lang: `0n4` = 4₁₀, `0nW` = −4₁₀

### 4.4 Основание 27 (Heptavintimal)

Группировка тритов по 3 (3³ = 27 значений):

0-9, A-F, G-Q (всего 27 символов для значений от −13 до +13).

В T-lang: `0y0` = 0₁₀, `0yZ` = −1₁₀

### 4.5 Сводная таблица представлений числа 42

| Система | Представление |
|---------|--------------|
| Десятичная | 42 |
| Сбалансированная троичная | `+−−−0` |
| Троичная raw (0,1,2) | 21110 |
| Base-9 (nonary) | `0n??` |
| Base-27 | `0y??` |
| В T-lang | `42`, `0t+−−−0`, `0n??`, `0y??` |

---

## 5. Архитектура процессора T3

### 5.1 Обзор

T3 — 18-тритный троичный процессор с гарвардской архитектурой (раздельные память команд и данных).

**Ключевые характеристики:**
- Разрядность: 18 тритов (≈ 28.5 бит информации)
- Адресное пространство: 1M слов (20 тритов адреса)
- Регистры: 9 общих + 9 FPU + 4 специальных
- Система команд: ~70 инструкций
- Предикация: 3 флага (True/Maybe/False)
- Стек: растёт вниз от 0xFFFF
- LIMM: 2-словные инструкции для 18-тритных непосредственных значений

### 5.2 Регистровый файл

#### Общего назначения (General Purpose)

Девять регистров GP (индексы −4…+4 в массиве `Registers[9]`):

| Индекс | Имя | Назначение |
|--------|-----|-----------|
| 0 (−4) | **RW** | Рабочий (Working) / Аргумент 1 |
| 1 (−3) | **RX** | Индексный / Аргумент 2 |
| 2 (−2) | **RY** | Аргумент 3 / Возвращаемое значение |
| 3 (−1) | **RZ** | Аргумент 4 / Frame Pointer |
| 4 (0) | **R0** | Временный |
| 5 (+1) | **R1** | Сохраняемый (callee-saved) |
| 6 (+2) | **R2** | Сохраняемый (callee-saved) |
| 7 (+3) | **R3** | Сохраняемый |
| 8 (+4) | **R4** | Сохраняемый |

#### Специальные (отдельные поля, не в `Registers[]`)

| Регистр | Поле C# | Тип | Назначение |
|---------|---------|-----|-----------|
| **SP** | `long SP` | int64 | Указатель стека (Stack Pointer) |
| **PC** | `long PC` | int64 | Счётчик команд (Program Counter) |
| **Cond** | `int Cond` | int32 | Флаг условия (−1/0/+1) |
| **PR** | `TWord PR` | Word18 | Предикатный регистр (3×9 тритов) |
| **WP** | `int WP` | int32 | Указатель окна регистров |

#### FPU (Floating-Point Unit)

Девять регистров FPU (индексы −4…+4 в массиве `FRegisters[9]`):

| Индекс | Имя | Назначение |
|--------|-----|-----------|
| 0 (−4) | **FW** | Рабочий FPU |
| 1 (−3) | **FX** | Аргумент 1 FPU |
| 2 (−2) | **FY** | Аргумент 2 / Возврат FPU |
| 3 (−1) | **FZ** | Аргумент 3 FPU |
| 4 (0) | **F0** | Временный FPU |
| 5 (+1) | **F1** | Сохраняемый FPU |
| 6 (+2) | **F2** | Сохраняемый FPU |
| 7 (+3) | **F3** | Сохраняемый FPU |
| 8 (+4) | **F4** | Сохраняемый FPU |

### 5.3 Формат инструкций

Каждая инструкция занимает ровно **18 тритов** (одно слово). Формат зависит от типа:

#### R-тип (Register — 3 регистра)
```
[Pred:3] [Opcode:6] [Rd:3] [Rs1:3] [Rs2:3]
```
Пример: `ADD R0, R1, R2`

#### I-тип (Immediate — 2 регистра + immediate)
```
[Pred:3] [Opcode:6] [Rd:3] [Rs1:3] [Imm:3]
```
Пример: `ADDI R0, R1, #5`

#### J-тип (Jump — 1 регистр + адрес)
```
[Pred:3] [Opcode:6] [Rd:3] [Addr:6]
```
Пример: `JAL R0, label`

#### S-тип (Special — системные инструкции)
```
[Pred:3] [Opcode:6] [Arg:9]
```
Пример: `HALT`, `NOP`

#### LIMM (Large Immediate — 2 слова)
```
Слово 1: [Pred:3] [Opcode:6] [Rd:3] [000:3] [Imm_low:3]
Слово 2: [Imm_high:18]
```

### 5.4 Предикация

Каждая инструкция может быть **предикатирована** — выполнена или пропущена в зависимости от флагов в регистре PR.

Биты предиката (3 трита):
- `+` (+1): выполнять только при True
- `0` (0): выполнять только при Maybe
- `−` (−1): выполнять только при False
- Отсутствие предиката: выполнять всегда

### 5.5 Память

- **Объём:** 1M слов (18 тритов каждое) = 2²⁰ адресов
- **Стек:** растёт вниз от 0xFFFFF
- **MMIO:** верхние адреса зарезервированы для ввода-вывода
- **Доступ:** LOAD/STORE с пре- и пост-инкрементом/декрементом

### 5.6 Кодирование инструкций

Инструкции кодируются через `InstructionEncoder` в 18-тритное слово:

```
Сырое значение = Pred × 3¹⁵ + Opcode × 3⁹ + Rd × 3⁶ + Rs1 × 3³ + Rs2 × 3⁰
```

Поля извлекаются через сбалансированно-в-несбалансированное преобразование и побитовые сдвиги.

---

## 6. Система команд (ISA)

### 6.1 Сводка по категориям

| Категория | Кол-во | Инструкции |
|-----------|--------|-----------|
| **Integer ALU** | 12 | ADD, ADDI, SUB, SUBI, MUL, MULI, DIV, DIVI, NEG, NEGI, ABS, ABSI |
| **Logical** | 6 | AND, ANDI, OR, ORI, XOR, XORI |
| **Shifts** | 4 | SHL, SHLI, SHR, SHRI |
| **Memory & Stack** | 8 | LOAD, LOADI, STORE, STOREI, PUSH, PUSHI, POP, POPI |
| **Control Flow** | 12 | CMP, JMP, JEQ, JNE, JGT, JLT, JGE, JLE, JAL, RET, CALL, BR |
| **I/O** | 4 | IN, INI, OUT, OUTI |
| **System** | 2 | HALT, NOP |
| **FPU** | 17 | FADD, FSUB, FMUL, FDIV, FSQRT, FCMP, FCPY, FNEG, FABS, FFLOOR, FCEIL, FSIN, FCOS, FTAN, FEXP, FLOG, FZERO |
| **Data Movement** | 5 | MOV, MOVI, LI, GETSP, SETSP |

### 6.2 Integer ALU

```
ADD  Rd, Rs1, Rs2    ; Rd = Rs1 + Rs2
ADDI Rd, Rs1, #imm3  ; Rd = Rs1 + sext(imm3)
SUB  Rd, Rs1, Rs2    ; Rd = Rs1 - Rs2
SUBI Rd, Rs1, #imm3  ; Rd = Rs1 - sext(imm3)
MUL  Rd, Rs1, Rs2    ; Rd = Rs1 * Rs2
MULI Rd, Rs1, #imm3  ; Rd = Rs1 * sext(imm3)
DIV  Rd, Rs1, Rs2    ; Rd = Rs1 / Rs2
DIVI Rd, Rs1, #imm3  ; Rd = Rs1 / sext(imm3)
NEG  Rd, Rs1         ; Rd = -Rs1
NEGI Rd, #imm3       ; Rd = -sext(imm3)
ABS  Rd, Rs1         ; Rd = |Rs1|
ABSI Rd, #imm3       ; Rd = |sext(imm3)|
```

### 6.3 Логические операции

```
AND  Rd, Rs1, Rs2    ; Поразрядное троичное И (min)
ANDI Rd, Rs1, #imm3  ; Поразрядное И с immediate
OR   Rd, Rs1, Rs2    ; Поразрядное троичное ИЛИ (max)
ORI  Rd, Rs1, #imm3  ; Поразрядное ИЛИ с immediate
XOR  Rd, Rs1, Rs2    ; Поразрядное троичное XOR (сумма mod 3)
XORI Rd, Rs1, #imm3  ; Поразрядное XOR с immediate
```

### 6.4 Сдвиги (умножение/деление на степень 3)

```
SHL  Rd, Rs1, Rs2    ; Rd = Rs1 * 3^Rs2 (сдвиг влево)
SHLI Rd, Rs1, #imm3  ; Rd = Rs1 * 3^imm3
SHR  Rd, Rs1, Rs2    ; Rd = Rs1 / 3^Rs2 (сдвиг вправо)
SHRI Rd, Rs1, #imm3  ; Rd = Rs1 / 3^imm3
```

### 6.5 Память и стек

```
LOAD  Rd, [Rs1]      ; Rd = MEM[Rs1]
LOADI Rd, [Rs1+#imm] ; Rd = MEM[Rs1 + sext(imm)]
STORE [Rs1], Rs2     ; MEM[Rs1] = Rs2
STOREI [Rs1+#imm], Rs2 ; MEM[Rs1 + sext(imm)] = Rs2
PUSH  Rs1            ; SP--; MEM[SP] = Rs1
POP   Rd             ; Rd = MEM[SP]; SP++
```

### 6.6 Управление потоком

```
CMP Rs1, Rs2         ; Cond = compare(Rs1, Rs2) → устанавливает PR
JMP label            ; PC = label
JEQ label            ; Jump if equal (PR.True > 0)
JNE label            ; Jump if not equal (PR.False > 0)
JGT label            ; Jump if greater than
JLT label            ; Jump if less than
JAL Rd, label        ; Rd = PC+1; PC = label (вызов)
RET                  ; PC = R0 (возврат)
CALL label           ; LIMM + JAL (2 слова)
```

**Предикатные флаги после CMP:**

| Результат | PR.True | PR.Maybe | PR.False | Мнемоника |
|-----------|---------|----------|----------|-----------|
| Равно | + | − | − | EQ (Equal) |
| Меньше | − | − | + | LT (Less Than) |
| Больше | − | + | − | GT (Greater Than) |
| Меньше или равно | + | + | − | LE |
| Больше или равно | − | + | + | GE |
| Результат сравнения | +/-/0 | +/-/0 | +/-/0 | — |

### 6.7 Ввод-вывод

```
IN  Rd, port         ; Rd = PORT[port]
OUT port, Rs1        ; PORT[port] = Rs1
```

Порты I/O: порт 0 = консоль (символьный вывод), порт 1 = консоль (ввод).

### 6.8 Системные

```
HALT                 ; Останов процессора
NOP                  ; Нет операции
```

### 6.9 Пересылка данных

```
MOV  Rd, Rs1         ; Rd = Rs1
MOVI Rd, #imm9       ; Rd = sext(imm9)
LI   Rd, #imm18      ; Загрузка 18-тритного immediate (LIMM)
GETSP Rd             ; Rd = SP
SETSP Rs1            ; SP = Rs1
```

---

## 7. FPU — сопроцессор плавающей точки

### 7.1 Регистры FPU

FPU имеет 9 регистров, совмещённых с целочисленным регистровым файлом (индексы −4…+4 в массиве `FRegisters[9]`):

| Индекс | Имя | Назначение |
|--------|-----|-----------|
| 0 (−4) | **FW** | Рабочий FPU |
| 1 (−3) | **FX** | Аргумент 1 FPU |
| 2 (−2) | **FY** | Аргумент 2 / Возврат FPU |
| 3 (−1) | **FZ** | Аргумент 3 FPU |
| 4 (0) | **F0** | Временный FPU |
| 5 (+1) | **F1** | Сохраняемый FPU |
| 6 (+2) | **F2** | Сохраняемый FPU |
| 7 (+3) | **F3** | Сохраняемый FPU |
| 8 (+4) | **F4** | Сохраняемый FPU |

FPU имеет дополнительные элементы:
- **FSR** (регистр состояния) — доступен через порт `0x20`
- Исключения устанавливают флаги в FSR, но **не** генерируют прерываний

### 7.2 Формат T3Float

```
┌───────────────────┬─────────────────────────────────┐
│ Экспонента (6 trit)│     Мантисса (12 trit)           │
│  [17..12]          │  [11..0]                         │
└───────────────────┴─────────────────────────────────┘

Значение = Мантисса × 3^(Экспонента − 182)
```

- **Смещение (bias):** 182 = (3⁶ − 1)/2
- **Мантисса:** целое число в диапазоне ±265,720
- **Специальные значения:** экспонента = −364, мантисса = 0 → ноль

### 7.3 Инструкции FPU

#### Арифметика
```
FADD  Fd, Fs1, Fs2   ; Fd = Fs1 + Fs2
FSUB  Fd, Fs1, Fs2   ; Fd = Fs1 - Fs2
FMUL  Fd, Fs1, Fs2   ; Fd = Fs1 * Fs2
FDIV  Fd, Fs1, Fs2   ; Fd = Fs1 / Fs2
FSQRT Fd, Fs1        ; Fd = sqrt(Fs1)
```

#### Унарные
```
FNEG  Fd, Fs1        ; Fd = -Fs1
FABS  Fd, Fs1        ; Fd = |Fs1|
FFLOOR Fd, Fs1       ; Fd = floor(Fs1)
FCEIL Fd, Fs1        ; Fd = ceil(Fs1)
```

#### Сравнение
```
FCMP Fs1, Fs2        ; Cond = compare_float(Fs1, Fs2) → устанавливает PR
```

#### Трансцендентные
```
FSIN  Fd, Fs1        ; Fd = sin(Fs1)
FCOS  Fd, Fs1        ; Fd = cos(Fs1)
FTAN  Fd, Fs1        ; Fd = tan(Fs1)
FEXP  Fd, Fs1        ; Fd = exp(Fs1)
FLOG  Fd, Fs1        ; Fd = ln(Fs1)
```

#### Специальные
```
FCPY  Fd, Fs1        ; Fd = Fs1 (копирование)
FZERO Fd             ; Fd = 0.0
```

---

## 8. ABI: соглашение о вызовах

### 8.1 Регистровая модель (ABI v4)

| Регистр | Роль | Сохраняется при вызове? |
|---------|------|------------------------|
| R0 | Аргумент 1 / Link-регистр (RET) | Нет |
| R1 | Аргумент 2 | Нет |
| R2 | Возвращаемое значение / Аргумент 3 | Нет |
| R3 | Аргумент 4 | Нет |
| R4 | Локальная переменная | **Да** (callee-saved) |
| R5 | Локальная переменная | **Да** (callee-saved) |
| SP (R6) | Указатель стека | **Да** |
| R8 (ZR) | Нулевой регистр | **Да** (константа) |

### 8.2 Пролог функции

```
; Сохраняем R4, R5 если используются
PUSH R4
PUSH R5
; Выделяем место под локальные переменные
ADDI SP, SP, #-N    ; N = размер фрейма
```

### 8.3 Эпилог функции

```
; Восстанавливаем SP
ADDI SP, SP, #N
; Восстанавливаем R5, R4
POP R5
POP R4
; Возврат
RET                  ; PC = R0
```

### 8.4 Вызов функции

```
; Помещаем аргументы в R0-R3
LI R0, arg1
LI R1, arg2
; Вызов
CALL function_name
; Результат в R2
```

### 8.5 Стековый фрейм

```
Старшие адреса ─────────────────────
                │ Аргументы (если > 4)   │
                │ Return address (в R0)  │ ← SP при входе
                │ Saved R4               │
                │ Saved R5               │
                │ Локальные переменные   │ ← SP после пролога
Младшие адреса  ─────────────────────
```

---

## 9. Язык T-lang

### 9.1 Обзор

**T-lang** — C-подобный язык программирования для троичного компьютера T3.

**Ключевые особенности:**
- Статическая типизация
- Троичная логика (if/maybe/else)
- Поддержка структур, union, enum, typedef
- Многомерные массивы
- Указатели (ограниченная поддержка)
- Препроцессор (#include, #define, #ifdef, #if)
- Стандартная библиотека (nanolib)

### 9.2 Типы данных

```c
// Целочисленные типы
tint x = 42;           // 18 тритов, основной целый тип
tshort s = 100;        // 12 тритов
tlong l = 1000tl;      // 36 тритов (суффикс 'tl')
tlong long ll = 1tll;  // 54 трита (суффикс 'tll')
tryte c = 'A';         // 6 тритов, символ/байт

// С плавающей точкой
tfloat f = 3.14;       // 18 тритов float
tdouble d = 2.71828;   // 36 тритов double

// Специальные
trit t = +;            // 1 трит (−1, 0, или +1)
void                   // отсутствие значения
```

### 9.3 Литералы

```c
// Десятичные
tint a = 42;
tint b = -255;

// Сбалансированные троичные (префикс 0t)
tint c = 0t+--;       // 5₁₀
tint d = 0t----;      // −40₁₀

// Base-9 / Nonary (префикс 0n)
tint e = 0n4;          // 4₁₀
tint f = 0nW;          // −4₁₀

// Base-27 / Heptavintimal (префикс 0y)
tint g = 0y0;          // 0₁₀
tint h = 0yZ;          // −1₁₀

// С плавающей точкой
tfloat pi = 3.14159d;  // суффикс 'd' (double) не обязателен
tfloat e = 2.71828;

// Символьные
tryte ch = 'A';        // 65₁₀
tryte nl = '\n';       // 10₁₀ (escape-последовательности)

// Строковые
tryte* str = "Hello";  // создаёт массив: [длина, символы...]

// Булевы (троичные)
tint t = true;         // +1
tint f = false;        // −1
tint m = maybe;        // 0

// tlong/tlong long
tlong big = 1000000tl;
tlong long huge = 999999999999tll;

// null
tint p = null;         // 0
```

### 9.4 Ключевые слова

```
trit, tryte, tshort, tint, tlong, tfloat, tdouble
void, struct, union, enum, typedef
if, else, maybe, while, for, do, switch, case, default
return, break, continue, goto
true, false, maybe, null
sizeof, const, volatile (зарезервированы)
```

### 9.5 Операторы

| Категория | Операторы |
|-----------|----------|
| Арифметические | `+`, `-`, `*`, `/`, `%` |
| Сравнения | `==`, `!=`, `<`, `>`, `<=`, `>=` |
| Логические | `&&` (AND), `\|\|` (OR), `!` (NOT) |
| Побитовые | `&` (AND), `\|` (OR), `^` (XOR), `~` (NOT) |
| Сдвиги | `<<` (×3ⁿ), `>>` (÷3ⁿ) |
| Присваивания | `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `\|=`, `^=`, `<<=`, `>>=` |
| Инкремент/декремент | `++` (префиксный и постфиксный), `--` |
| Тернарный | `? :` (троичный: `cond ? true_val : maybe_val : false_val`) |
| Доступ | `.` (поле структуры), `->` (поле через указатель), `[]` (индекс массива) |
| Указатели | `*` (разыменование), `&` (взятие адреса) |
| Размер | `sizeof(type)` |

### 9.6 Управляющие конструкции

#### if / maybe / else (троичное ветвление)

```c
tint x = 0;

if (x > 0) {
    // x положительно (True)
    return 1;
} maybe {
    // x равно нулю (Maybe/Unknown)
    return 0;
} else {
    // x отрицательно (False)
    return -1;
}
```

Блок `maybe` опционален. Если опущен, семантика как в C (≠0 → true, 0 → false).

#### while

```c
tint sum = 0;
tint i = 1;
while (i <= 10) {
    sum = sum + i;
    i = i + 1;
}
```

#### for

```c
for (tint i = 0; i < 10; i = i + 1) {
    print_int(i);
}
```

#### do-while

```c
tint i = 0;
do {
    i = i + 1;
} while (i < 10);
```

#### switch-case

```c
switch (x) {
    case 1: return 10;
    case 2: return 20;
    default: return 0;
}
```

### 9.7 Функции

```c
// Объявление функции
tint add(tint a, tint b) {
    return a + b;
}

// Рекурсия поддерживается
tint factorial(tint n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

// main — точка входа
tint main() {
    return add(3, 4);  // возвращает 7
}
```

**Параметры:** до 4 параметров в регистрах (R0-R3), остальные через стек.

### 9.8 Структуры и union

```c
// Структура
struct Point {
    tint x;
    tint y;
};

tint main() {
    struct Point p;
    p.x = 10;
    p.y = 20;
    return p.x + p.y;  // 30
}

// Union (все поля по одному адресу)
union Data {
    tint as_int;
    tryte as_byte;
};
```

### 9.9 Массивы

```c
// Одномерный массив
tint arr[5];
arr[0] = 1;
arr[4] = 5;

// Инициализация
tint a[] = {1, 2, 3, 4, 5};  // размер выводится из инициализатора

// Строки как массивы tryte
tryte s[] = {5, 'H', 'e', 'l', 'l', 'o'};  // s[0] = длина

// Многомерные
tint matrix[2][3];
matrix[0][0] = 1;
matrix[1][2] = 6;
```

### 9.10 Указатели

```c
tint x = 42;
tint* p = &x;     // указатель на x
tint val = *p;    // разыменование → 42

// Доступ к полям через указатель
struct Point* pp = &p;
tint px = pp->x;  // то же что (*pp).x
```

Примечание: арифметика указателей в интерпретаторе ограничена (no-op для унарного `*` и `&`). Индексация массивов `str[i]` работает через ArrayAccess.

### 9.11 Enum и Typedef

```c
// Перечисление
enum Color {
    RED,          // 0
    GREEN = 5,    // 5
    BLUE          // 6
};

// Псевдонимы типов
typedef tint i32;
typedef tfloat real;

i32 main() {
    real x = 3.14;
    return RED;  // 0
}
```

### 9.12 Препроцессор

```c
// Включение файлов
#include <tstring.th>     // заголовок стандартной библиотеки
#include <libtstring.t>   // реализация на T-lang

// Макросы
#define PI 3.14159
#define SQUARE(x) ((x) * (x))

// Условная компиляция
#ifdef DEBUG
    print_str("Debug mode\n");
#endif

#ifndef _TSTRING_TH
#define _TSTRING_TH
// ... содержимое ...
#endif
```

### 9.13 Стандартная библиотека (nanolib)

#### Ввод-вывод (`#include <tio.th>`)

```c
print_int(42);         // вывод числа
print_str("Hello!");   // вывод строки
print_char('A');       // вывод символа
print_float(3.14);     // вывод float
print_bal(42);         // вывод в balanced ternary формате
tint n = scan_int();   // ввод числа
tryte c = get_char();  // ввод символа
```

#### Строки (`#include <tstring.th>` + `#include <libtstring.t>`)

```c
tint len = t_strlen(str);
tint cmp = t_strcmp(a, b);
t_strcpy(dest, src);
t_strcat(dest, src);
tint pos = t_strchr(str, 'c');
tint val = t_atoi("123");
t_itoa(42, buf);
```

#### Математика (`#include <tmath.th>` + `#include <libtmath.t>`)

```c
tint a = t_abs(-42);        // 42
tint m = t_min(3, 10);      // 3
tint n = t_max(3, 10);      // 10
tint c = clamp(50, 10, 30); // 30

tfloat x = t_sqrt(9.0);     // 3.0
tfloat s = t_sin(0.0);      // 0.0
tfloat p = t_pow(2.0, 3.0); // 8.0
```

#### Управление памятью

```c
tint ptr = malloc(100);  // выделить 100 трайт в куче
free(ptr);               // освободить (no-op в текущей версии)
```

### 9.14 Полный пример программы

```c
#include <tio.th>
#include <tstring.th>
#include <tmath.th>

// Вычисление факториала
tint factorial(tint n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

// Точка входа
tint main() {
    print_str("Factorial calculator\n");
    
    print_str("Enter number: ");
    tint n = scan_int();
    
    if (n < 0) {
        print_str("Error: negative input\n");
        return -1;
    } maybe {
        print_str("Zero! Factorial = 1\n");
        return 1;
    }
    
    tint result = factorial(n);
    
    print_str("Factorial = ");
    print_int(result);
    print_char('\n');
    
    return 0;
}
```

---

## 10. Компилятор T3

### 10.1 Архитектура компилятора

Компилятор T-lang следует классической многофазной архитектуре:

```
Исходный код (.t)
    │
    ▼
┌──────────────┐
│ Препроцессор │  #include, #define, #ifdef/#if
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  Лексер      │  Токенизация исходного текста
└──────┬───────┘
       │
       ▼
┌──────────────┐
│  Парсер      │  Рекурсивный спуск → AST
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ CodeGen      │  Генерация ассемблерного кода T3
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Ассемблер    │  Ассемблирование → машинный код / .o
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Линкер       │  Сборка .o → исполняемый файл (.bin)
└──────────────┘
```

### 10.2 Этапы компиляции

#### 10.2.1 Препроцессор (`T3Preprocessor`)

- Обрабатывает `#include` (поиск по путям включения)
- Подставляет `#define` (простые и функциональные макросы)
- Обрабатывает `#ifdef`, `#ifndef`, `#if`, `#else`, `#endif`
- Ведёт стек условной компиляции (true=активно, false=пропущено)

#### 10.2.2 Лексер (`Tokenizer`)

Преобразует текст в поток токенов:

- Ключевые слова: `tint`, `if`, `while`, `return`, ...
- Идентификаторы: `myVar`, `factorial`, ...
- Литералы: `42`, `0t+--`, `'A'`, `"Hello"`, `3.14`
- Операторы: `+`, `-`, `*`, `==`, `&&`, ...
- Разделители: `{`, `}`, `(`, `)`, `;`, ...

#### 10.2.3 Парсер (`Parser`)

Рекурсивный спуск, строит AST (Abstract Syntax Tree):

```
Program
├── StructDef / UnionDef
├── EnumDef
├── TypedefDef
├── FunctionDef
│   ├── ReturnType
│   ├── Parameters (VarDeclaration[])
│   └── Body (CompoundStmt)
│       ├── VarDeclaration
│       ├── IfStmt / WhileStmt / ForStmt / ...
│       └── ReturnStmt
└── Global variables (VarDeclaration[])
```

#### 10.2.4 Генератор кода (`CodeGenerator`)

Преобразует AST в ассемблер T3:

- **Выражения:** рекурсивный обход дерева выражений, загрузка операндов в регистры
- **Управление потоком:** генерация меток и переходов
- **Функции:** пролог/эпилог, передача параметров
- **Стек:** выделение фрейма, сохранение/восстановление регистров

### 10.3 Режимы компиляции

```bash
# Компиляция в ассемблер (по умолчанию)
t3cc program.t -o program.asm

# Компиляция в объектный файл
t3cc -c program.t -o program.o

# Вывод AST (для отладки)
t3cc --dump-ast program.t
```

---

## 11. Интерпретатор T-lang

### 11.1 Архитектура

Интерпретатор выполняет AST напрямую, без компиляции в машинный код:

```
AST → T3Interpreter.Run() → Результат
```

**Ключевые компоненты:**
- `T3Interpreter` — главный класс, обход AST
- `T3Value` — унифицированное представление значений (int, long, float, array, struct, union, string, bool)
- `Nanolib` — встроенные функции (builtins)

### 11.2 Модель выполнения

```csharp
Eval(AstNode n):
    IntegerLiteral → T3Value.FromInt/FromLong
    StringLiteral  → MakeString (массив: [длина, символ1, ...])
    Identifier     → поиск в стеке областей видимости
    BinaryOp       → рекурсивное вычисление + операция
    FunctionCall   → вызов пользовательской функции ИЛИ nanolib
    ArrayAccess    → GetElement по индексу
    ...
```

**Стек областей видимости (`_scopes`):**
- Глобальная область (push в Run)
- Локальная область функции (push при вызове)
- Поиск переменной: снизу вверх по стеку

### 11.3 Приоритет вызовов

```
1. Функция определена пользователем (#include)? → выполняем
2. Функция есть в Nanolib (builtin)?             → выполняем
3. Ошибка: undefined function
```

Это позволяет библиотекам на T-lang (`libtstring.t`, `libtmath.t`) переопределять builtins.

### 11.4 Обработка ошибок

```
runtime error at 10:5: undefined variable 'foo'
runtime error at 15:3: division by zero
```

Ошибки содержат номер строки и столбца из AST-узла.

---

## 12. Ассемблер и линкер

### 12.1 Ассемблер T3

Двухпроходный ассемблер с поддержкой:

- **Метки:** `label:`
- **Директивы:** `.word`, `.string`, `.equ`
- **Выражения:** `label+4`, `a+b*2` в `.equ`
- **LIMM:** автоматическая генерация для дальних переходов и больших immediate
- **Предикация:** префикс `?T` (True), `?M` (Maybe), `?F` (False)

#### Пример программы на ассемблере

```asm
; factorial.asm — вычисление факториала
    LI R0, 5        ; n = 5
    CALL factorial
    HALT

factorial:
    CMP R0, ZR      ; n <= 1?
    JLE base_case
    PUSH R0
    ADDI R0, R0, #-1
    CALL factorial  ; factorial(n-1)
    POP R1
    MUL R2, R1, R2  ; n * factorial(n-1)
    RET

base_case:
    LI R2, 1
    RET
```

### 12.2 Линкер

Сборка многофайловых программ:

```
┌─────────┐    ┌─────────┐    ┌─────────┐
│ main.o  │ +  │ lib1.o  │ +  │ lib2.o  │
└────┬────┘    └────┬────┘    └────┬────┘
     │              │              │
     └──────────────┼──────────────┘
                    │
                    ▼
              ┌──────────┐
              │  Линкер  │
              └────┬─────┘
                   │
                   ▼
              исполняемый.bin
```

**3 фазы линковки:**
1. **Символы:** сбор всех GLOBAL и EXTERN символов из `.o` файлов
2. **Образ:** размещение секций `.text`, `.data`, `.bss` в памяти
3. **Релокации:** применение PC-относительных и абсолютных релокаций

**Формат `.o` файла (T3OB):**
- Заголовок с магической сигнатурой
- Таблица секций (.text, .data, .bss, .rela.text, .symtab)
- Таблица символов (GLOBAL, EXTERN)
- Таблица релокаций (PC_RELATIVE, ABSOLUTE)

### 12.3 CLI команды

```bash
# Ассемблирование в .o
t3asm --emit-obj input.asm -o output.o

# Линковка
t3asm --link -o program.bin main.o lib1.o lib2.o

# Ассемблирование в бинарный файл (без линковки)
t3asm input.asm -o output.bin
```

---

## 13. Стандартная библиотека (nanolib)

### 13.1 Архитектура

Nanolib — двухуровневая библиотека:

| Уровень | Для интерпретатора | Для компилятора |
|---------|-------------------|-----------------|
| **Builtins (C#)** | `Nanolib.cs` (459 строк, 40+ функций) | `NanolibCodeGen.cs` (163 строки, 11 asm-функций) |
| **T-lang библиотека** | `libtstring.t` (213 строк), `libtmath.t` (25 строк) | Компилируются в .o через t3cc |

### 13.2 Использование

```c
// Подключение стандартной библиотеки через #include
#include <tio.th>        // только объявления I/O
#include <tstring.th>    // объявления строковых функций
#include <libtstring.t>  // реализация на T-lang (переопределяет builtins!)
#include <tmath.th>      // объявления математических функций
#include <libtmath.t>    // реализация на T-lang
```

### 13.3 Переопределение builtins

Библиотеки на T-lang имеют **приоритет над builtins**. Если вы подключаете `libtstring.t`, функция `t_strlen()` будет вызвана из T-lang-реализации, а не из C# builtin.

Это позволяет:
- Использовать быстрые builtins «из коробки»
- Заменять их на свои реализации при необходимости
- Отлаживать библиотечный код на самом T-lang

### 13.4 Список функций

#### I/O (всегда builtins)

`print_int`, `print_long`, `print_float`, `print_double`, `print_tryte`, `print_bal`, `print_str`, `print_char`, `putchar`, `puts`, `scan_int`, `scan_long`, `scan_float`, `scan_double`, `get_char`

#### Строки (могут быть переопределены)

`t_strlen`, `t_strcmp`, `t_strncmp`, `t_strcpy`, `t_strncpy`, `t_strcat`, `t_strchr`, `t_strrchr`, `t_strstr`, `t_atoi`, `t_itoa`, `t_baltoa`

#### Математика (целые — переопределяемы, float — builtins)

`t_abs`, `t_min`, `t_max`, `clamp`, `t_sqrt`, `t_floor`, `t_ceil`, `t_sin`, `t_cos`, `t_tan`, `t_asin`, `t_acos`, `t_atan`, `t_exp`, `t_log`, `t_log3`, `t_pow`

---

## 14. Инструментарий

### 14.1 CLI инструменты

| Инструмент | Команда | Назначение |
|-----------|---------|-----------|
| Компилятор | `t3cc` | Компиляция T-lang → ассемблер / .o |
| Ассемблер | `t3asm` | Ассемблирование → .bin / .o |
| Интерпретатор | `t3run` | Запуск T-lang без компиляции |
| Конвертер | `t3conv` | Конвертация между системами счисления |
| Симулятор | `t3sim` | Пошаговое выполнение машинного кода |

### 14.2 GUI инструменты (Avalonia)

| Приложение | Назначение |
|-----------|-----------|
| T3Simulator.GUI | Визуальный симулятор процессора: регистры, память, пошаговое выполнение |
| T3Calculator.GUI | Троичный калькулятор с поддержкой всех систем счисления |
| T3Converter.GUI | Конвертер между десятичной, троичной, base-9, base-27 |

### 14.3 Процесс сборки (полный цикл)

```bash
# 1. Компиляция в ассемблер
t3cc myprogram.t -o myprogram.asm

# 2. Ассемблирование в объектный файл
t3asm --emit-obj myprogram.asm -o myprogram.o

# 3. Компиляция библиотек
t3cc -c libtstring.t -o libtstring.o
t3cc -c libtmath.t -o libtmath.o

# 4. Линковка
t3asm --link -o program.bin myprogram.o libtstring.o libtmath.o

# 5. Запуск на симуляторе
t3sim program.bin

# Альтернативно: запуск через интерпретатор
t3run myprogram.t
```

---

## 15. Приложения

### 15.1 Быстрый старт

```bash
# Клонирование
git clone https://github.com/EntityFX/t3_sharp
cd t3_sharp

# Сборка
dotnet build

# Запуск тестов
dotnet test

# Первая программа
echo 'tint main() { return 42; }' > hello.t
t3run hello.t
# Вывод: 42
```

### 15.2 Часто задаваемые вопросы

**Q: Почему троичная, а не двоичная система?**
A: Троичная система — исследовательский проект. Она предлагает более эффективное представление отрицательных чисел и симметричную логику, что может быть полезно в специализированных вычислениях.

**Q: Насколько серьёзен этот проект?**
A: Это исследовательский прототип с полным инструментарием: 555 тестов, 100% прохождение. Не для продакшена, но достаточно зрелый для обучения и экспериментов.

**Q: Можно ли написать реальную программу на T-lang?**
A: Да, язык поддерживает функции, рекурсию, структуры, массивы, указатели, стандартную библиотеку. Можно писать алгоритмические программы средней сложности.

**Q: Что с производительностью?**
A: Симулятор — интерпретатор машинного кода (не JIT). Для исследовательских целей достаточно. Компилятор генерирует код для этого же симулятора.

**Q: Какие планы на будущее?**
A: См. ROADMAP.md. Приоритеты: починить Frame Pointer (RZ), nested calls, добавить `const`/`union` в компилятор, tlong в компилятор.

### 15.3 Таблица кодировки TScii (первые 128 символов)

Совпадает с ASCII для значений 0-127. Символы 128-728 — расширенный набор (кириллица, математические символы, и т.д.)

### 15.4 Ссылки

- [T3 на GitHub](https://github.com/EntityFX/t3_sharp)
- [Сетунь — Википедия](https://ru.wikipedia.org/wiki/Сетунь_(компьютер))
- [Balanced Ternary — Wikipedia](https://en.wikipedia.org/wiki/Balanced_ternary)
- [Троичная логика — Википедия](https://ru.wikipedia.org/wiki/Троичная_логика)

---

**Документ создан:** 2026-07-01  
**Актуально для версии:** T3Sharp v2.4  
**Автор:** Команда T3Sharp