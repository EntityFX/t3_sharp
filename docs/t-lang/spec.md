# Спецификация языка T (Целевой стандарт)

*Троичный язык системного программирования*  

Версия 2.0 (трайт = 6 тритов)

*Примечание: текущий компилятор T-lang является исследовательским прототипом. Он реализует базовый синтаксис и ограниченный набор семантики. Полное соответствие данной спецификации находится в разработке.*

## 1. Введение

Язык T — низкоуровневый язык для троичных вычислительных систем, наследующий синтаксис и философию C.  
Базовые единицы информации: **трит** (значения `-1`, `0`, `+1`, изображаемые `-`, `0`, `+`) и **трайт** — минимальная адресуемая ячейка памяти из **6 тритов**.  
Все целые типы кратны трайту и используют **сбалансированную троичную систему**. Логика трёхзначная (`true`, `false`, `maybe`). Синтаксис расширен тринарными операторами, трёхветвевым `if` и поддержкой русских ключевых слов.

## 2. Лексические соглашения

### 2.1. Триты и символы

- `-` : –1  
- `0` :  0  
- `+` : +1  

### 2.2. Литералы

#### Целочисленные литералы

- **Десятичные с суффиксом размера:**  
  `y` — tryte (6 тритов), `s` — tshort (12), `t` — tint (18), `tl` — tlong (36), `tll` — tlong long (54).  
  Примеры: `42y`, `-100t`, `100000tl`.

- **Сбалансированные троичные:**  
  Префикс `0t`, затем последовательность `-`, `0`, `+` (длина до ширины типа). Допустимо `_`.  
  Примеры: `0t+-0`, `0t+++_+++` (макс. tryte), `0t---_---` (мин. tryte).

- **27‑ричные (tryx):**  
  Префикс `0y`, затем цифры из таблицы троек тритов. Каждая цифра кодирует 3 трита.  
  Алфавит (троек от `---` до `+++`):  
  `N O P Q R S T U V` (--- … -++)  
  `W X Y Z 0 1 2 3 4` (0-- … 0++)  
  `5 6 7 8 9 A B C D` (+-- … +++)  
  Допустим нижний регистр. Число цифр = (триты типа)/3.  
  Примеры: `0yNN` (мин. tryte), `0yDD` (макс. tryte), `0y000000` (tint = 0).

- **9‑ричные (ninary):**  
  Префикс `0n`, затем цифры для пар тритов. Алфавит (старший, младший):  
  `W` (--), `X` (-0), `Y` (-+), `Z` (0-), `0` (00), `1` (0+), `2` (+-), `3` (+0), `4` (++).  
  Число цифр = (триты типа)/2.  
  Примеры: `0nW04` (-- 00 ++), `0n000` (ноль tryte).

#### Вещественные литералы

- Десятичная форма: `3.14`, `-0.001f`, `1.0e+2d`. Суффикс `f` — tfloat, `d` (или без) — tdouble.
- Троичная форма: `0t+.-0+0e+0f` (мантисса.порядок), где мантисса и порядок в `-0+`.

#### Символьные и строковые константы

- `'+'`, `'0'`, `'-'` → тип `trit` (значения +1,0,-1).
- `'A'`, `'я'` → тип `tryte` (код символа).
- Строки: `"текст"` — массив `tryte`, завершается нулевым трайтом `0t`.

### 2.3. Идентификаторы

Буквы латиницы и кириллицы, цифры, знак подчёркивания. Регистр значим.  
Примеры: `счётчик`, `myVar`, `_trit`.

### 2.4. Ключевые слова

Язык T двуязычен: все ключевые слова имеют английский и русский варианты.

| Английское   | Русское      | Категория         |
|--------------|--------------|-------------------|
| `if`         | `если`       | Условие           |
| `maybe`      | `может`      | Ветвь / константа |
| `else`       | `иначе`      | Условие           |
| `switch`     | `выбор`      | Выбор             |
| `case`       | `случай`     | Метка выбора      |
| `default`    | `умолчание`  | Метка выбора      |
| `while`      | `пока`       | Цикл              |
| `do`         | `делать`     | Цикл              |
| `for`        | `для`        | Цикл              |
| `break`      | `прервать`   | Переход           |
| `continue`   | `продолжить` | Переход           |
| `return`     | `возврат`    | Переход           |
| `goto`       | `перейти`    | Переход           |
| `struct`     | `структура`  | Тип               |
| `union`      | `объединение`| Тип               |
| `enum`       | `перечисление`| Тип              |
| `typedef`    | `типрег`     | Объявление типа   |
| `const`      | `конст`      | Квалификатор      |
| `volatile`   | `изменч`     | Квалификатор      |
| `void`       | `пусто`      | Тип               |
| `trit`       | `трит`       | Тип трита         |
| `tril`       | `трил`       | Логический тип    |
| `tryte`      | `трайт`      | Целый тип (6 тритов)|
| `tshort`     | `тшорт`      | Целый тип (12 тритов)|
| `tint`       | `тцел`       | Целый тип (18 тритов)|
| `tlong`      | `тдлин`      | Целый тип (36 тритов)|
| `tlong long` | `тдлинлонг`  | Целый тип (54 трита) |
| `tfloat`     | `твещ`       | Вещественный (18 тритов)|
| `tdouble`    | `тдвойн`     | Вещественный (36 тритов)|
| `true`       | `истина`     | Константа         |
| `false`      | `ложь`       | Константа         |
| `maybe`      | `может`      | Константа         |
| `sizeof`     | `размер`     | Оператор          |
| `extern`     | `внеш`       | Класс хранения    |
| `static`     | `статич`     | Класс хранения    |
| `auto`       | `авто`       | Класс хранения    |
| `register`   | `регистр`    | Класс хранения    |
| `inline`     | `встроен`    | Спецификатор функции|

Зарезервированы, но не используются: `short`, `int`, `long`, `char`, `float`, `double` и их русские аналоги.

## 3. Основные типы данных

### 3.1. Трит и трайт

- **`trit`** — одиночный трит (–1,0,+1). В выражениях повышается до `tint`.
- **`tryte`** — трайт из 6 тритов. Синоним `char`-подобного типа. Диапазон: –364 … +364.

### 3.2. Целочисленная иерархия (сбалансированная)

| Тип          | Трайтов | Тритов | Диапазон (округлённо)         |
|--------------|---------|--------|-------------------------------|
| `tryte`      | 1       | 6      | –364 … +364                  |
| `tshort`     | 2       | 12     | –265 720 … +265 720          |
| `tint`       | 3       | 18     | –193 710 244 … +193 710 244  |
| `tlong`      | 6       | 36     | –7.6·10¹⁶ … +7.6·10¹⁶       |
| `tlong long` | 9       | 54     | –2.0·10²⁵ … +2.0·10²⁵       |

Размер измеряется в трайтах: `sizeof(tryte)==1`, `sizeof(tshort)==2` и т.д.

### 3.3. Вещественные типы

Формат: значение = `(скрытый_трит . m1...mp) × 3^(E-bias)`.  
Скрытый трит не хранится, мантисса нормализована. Смещение `bias = (3^e - 1)/2`.

| Тип      | Трайтов | Тритов | e (порядок) | bias | p (мантисса) | Точность, тритов | Диапазон порядка |
|----------|---------|--------|-------------|------|--------------|------------------|------------------|
| `tfloat` | 3       | 18     | 6           | 182  | 12           | 13               | 3^⁻182 … 3^181 |
| `tdouble`| 6       | 36     | 8           | 3280 | 28           | 29               | 3^⁻3280 … 3^3279|

Специальные значения: ноль (порядок=0, мантисса=0), бесконечность (порядок макс., мантисса=0), NaN (порядок макс., мантисса≠0), maybe-NaN (старший трит мантиссы 0). Округление по умолчанию к ближайшему, с правилом чётного младшего трита.

### 3.4. Логический тип `tril`

Трит, интерпретируемый как истинность: `true` (+), `maybe` (0), `false` (-).  
Логические операции возвращают `tril`.

### 3.5. Перечислимые типы (`enum`)

Создают целочисленный тип с именованными константами. Размер выбирается как `tint` или `tshort` по необходимости.

## 4. Преобразования типов

- `trit` → `tint` автоматически.
- Целые меньшего размера повышаются до `tint` в выражениях.
- `tlong` доминирует над `tint`, `tlong long` над `tlong`.
- Вещественные: `tdouble` старше `tfloat`; смешивание целых и вещественных приводит целое к вещественному.
- Присваивание более широкого типа узкому усекает значение с сохранением симметричного диапазона (для знаковых) или по модулю 3ⁿ (для беззнаковых, хотя беззнаковые не используются).
- `tril` ↔ `trit` неявно.
- Вещественное → целое: отбрасывание дробной части (к нулю); переполнение даёт неопределённое значение.

## 5. Выражения

### 5.1. Приоритеты операторов (от высшего к низшему)

| Приоритет | Операторы                                         | Ассоциативность |
|-----------|---------------------------------------------------|-----------------|
| 1         | `++ --` (постфиксные) `()` `[]` `.` `->`          | →               |
| 2         | `++ --` (префиксные) `+ - ! ~ * & sizeof`         | ←               |
| 3         | `* / %`                                           | →               |
| 4         | `+ -`                                             | →               |
| 5         | `<< >>`                                           | →               |
| 6         | `< > <= >=`                                       | →               |
| 7         | `== !=`                                           | →               |
| 8         | `&` (потритовый минимум)                          | →               |
| 9         | `^` (потритовая сумма)                            | →               |
| 10        | `\|` (потритовый максимум)                        | →               |
| 11        | `&&`                                              | →               |
| 12        | `\|\|`                                            | →               |
| 13        | `?? … :? … :! …` (тринарный)                      | ←               |
| 14        | `= += -= *= /= %= <<= >>= &= ^= \|=`              | ←               |

### 5.2. Унарные операторы

- `-` — арифметическое отрицание.
- `!` — логическое отрицание (возвращает `tril`): `+`↔`-`, `0` остаётся `0`.
- `~` — потритовое отрицание.
- `++`, `--` (префиксные и постфиксные).
- `sizeof(тип|выражение)` — размер в **трайтах**.
- `&`, `*` — адрес и разыменование.

### 5.3. Потритовые операторы

Работают над каждым тритом операндов:

- `&` — минимум,
- `|` — максимум,
- `^` — сумма по модулю 3 без переноса (`+ ^ + = -`, `- ^ - = +`, `+ ^ - = 0` и т.д.).

### 5.4. Логические операторы (трёхзначные)

- `x && y` — `min(x,y)`,
- `x || y` — `max(x,y)`.  

Оба операнда вычисляются всегда (нет короткого замыкания на `maybe`), если компилятор не докажет отсутствие побочных эффектов.

### 5.5. Сдвиги (только целые)

- `x << n` — умножение на 3ⁿ (сдвиг влево), младшие триты заполняются `0`.
- `x >> n` — арифметический сдвиг вправо (деление на 3ⁿ с округлением к –∞).

### 5.6. Тринарный условный оператор

```с
условие ?? выраж1 :? выраж2 :! выраж3
```

Возвращает одно из трёх выражений в зависимости от того, равно ли условие `true`, `maybe` или `false`. Вычисляются все три. Тип — общий тип операндов.

### 5.7. Арифметические операторы

`+`, `-`, `*`, `/` — для целых и вещественных.  
`%` — остаток от деления целых: `(a/b)*b + a%b == a`, деление округляется к –∞.

### 5.8. Присваивание и составные операторы

`=`, `+=`, `-=`, `*=`, `/=`, `%=`, `<<=`, `>>=`, `&=`, `|=`, `^=` работают аналогично C.

## 6. Операторы

### 6.1. Условный оператор

```c
if (выражение) оператор1
[maybe оператор2]
[else оператор3]
```

Ветви `maybe` и `else` могут следовать в любом порядке, обе необязательны. Если ветвь опущена, соответствующее состояние (`maybe` или `false`) просто пропускается.

### 6.2. Оператор выбора

```c
switch (целое_выражение) {
    case значение: ...
    default: ...
}
```

Метки — целые константы. `switch` не работает с `tril`.

### 6.3. Циклы

`while (условие)`, `do ... while(условие)`, `for(инициализация; условие; шаг)`.  
Тело выполняется, пока условие == `true`. При `maybe` или `false` цикл завершается.

### 6.4. Операторы перехода

`break`, `continue`, `return [выражение]`, `goto метка;`.

## 7. Объявления

### 7.1. Спецификаторы типа

`void`, `trit`, `tril`, `tryte`, `tshort`, `tint`, `tlong`, `tlong long`, `tfloat`, `tdouble`, `signed` (необязателен, так как все целые знаковые), `struct`, `union`, `enum`, `typedef`-имя.

### 7.2. Классы памяти и квалификаторы

`auto`, `register`, `static`, `extern`; `const`, `volatile`.  
`inline` — для функций.

### 7.3. Инициализация

Скаляры можно инициализировать константными выражениями, массивы и структуры — списками в фигурных скобках. Строковый литерал инициализирует массив `tryte`.

## 8. Препроцессор

Препроцессор T совместим с C: `#include`, `#define`, `#if`, `#ifdef`, `#ifndef`, `#else`, `#elif`, `#endif`, `#error`, `#line`, `#pragma`.  
В условных директивах `true` → 1, `false` → 0, `maybe` → ошибка (требуется явное приведение к целому).

## 9. Стандартная библиотека (выборочно)

- **`<tio.h>`** — ввод-вывод:  
  `printtryte`, `printtshort`, `printint`, `printlong`, `printbal` (сбалансированный), `printtril`, `printfloat`, `printdouble`, `scant`, `scanf`, `getchar`, `putchar`, и т.д.
- **`<tlimits.h>`** — макросы `TRYTE_MIN`, `TRYTE_MAX`, `TSHORT_MIN`, `TINT_MIN`, `TLONG_MIN` и т.д.
- **`<tfloat.h>`** — параметры вещественных типов.
- **`<tmath.h>`** — `tabs`, `tmin`, `tmax`, `tfloor`, `tsqrt`, тригонометрические функции.
- **`<tstring.h>`** — `tstrlen`, `tstrcpy`, `tstrcmp`.
- **`<tstdlib.h>`** — `tmalloc`, `tfree`, `texit`, `trand`.
- **`<tstdarg.h>`**, **`<tsetjmp.h>`**, **`<tsignal.h>`**, **`<ttime.h>`**.

## 10. Примеры

```c
#include <tio.h>

tint main() {
    // Целые литералы
    tryte  a = 0yNN;          // -364
    tshort b = 100s;
    tint   c = 0t++0-;
    tlong  d = 1000000tl;

    // Логика
    tril flag = может;
    если (flag == истина) {
        print("Да\n");
    } может {
        print("Не знаю\n");
    } иначе {
        print("Нет\n");
    }

    // Тринарный оператор
    tryte res = (a > 0y00) ?? a :? 0y00 :! -a;

    // Вещественные
    твещ f = 0t+.-0+0e+0f;
    тдвойн pi = 3.14159;
    printdouble(tsqrt(pi));

    возврат 0y00;
}
```

## 11. Полная EBNF-грамматика

```ebnf
(* ============================================================
   Полная EBNF-грамматика языка T
   (трайт = 6 тритов, сбалансированная троичная система)
   ============================================================ *)

(* --- 1. Лексическая структура --- *)
trit_digit         = "-" | "0" | "+" .

identifier         = ( letter | "_" ) { letter | digit | "_" } .
letter             = latin_letter | cyrillic_letter .
latin_letter       = "A".."Z" | "a".."z" .
cyrillic_letter    = "А".."Я" | "а".."я" | "Ё" | "ё" .
digit              = "0".."9" .

decimal_suffix     = "y" | "s" | "t" | "tl" | "tll" .
decimal_literal    = [ "+" | "-" ] digit { digit } decimal_suffix .
balanced_prefix    = "0t" .
balanced_digits    = trit_digit { trit_digit } [ "_" trit_digit ] .
balanced_literal   = balanced_prefix balanced_digits .

tryx_prefix        = "0y" .
tryx_digit         = "N" | "O" | "P" | "Q" | "R" | "S" | "T" | "U" | "V"
                   | "W" | "X" | "Y" | "Z"
                   | "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
                   | "A" | "B" | "C" | "D"
                   | "n" | "o" | "p" | "q" | "r" | "s" | "t" | "u" | "v"
                   | "w" | "x" | "y" | "z"
                   | "a" | "b" | "c" | "d" .
tryx_literal       = tryx_prefix tryx_digit { tryx_digit } .

nine_prefix        = "0n" .
nine_digit         = "W" | "X" | "Y" | "Z" | "0" | "1" | "2" | "3" | "4"
                   | "w" | "x" | "y" | "z" .
nine_literal       = nine_prefix nine_digit { nine_digit } .

integer_literal    = decimal_literal | balanced_literal | tryx_literal | nine_literal .

float_suffix       = "f" | "d" .
decimal_float      = [ "+" | "-" ] digit { digit } "." digit { digit }
                     [ "e" [ "+" | "-" ] digit { digit } ] [ float_suffix ] .
balanced_float     = balanced_prefix mantissa_part "e" exponent_part float_suffix .
mantissa_part      = trit_digit { trit_digit } [ "_" trit_digit ]
                     "." trit_digit { trit_digit } .
exponent_part      = trit_digit { trit_digit } .
float_literal      = decimal_float | balanced_float .

char_literal       = "'" ( trit_digit | any_character ) "'" .
string_literal     = '"' { any_character } '"' .

true_const         = "true" | "истина" .
false_const        = "false" | "ложь" .
maybe_const        = "maybe" | "может" .
constant           = integer_literal | float_literal | char_literal
                   | string_literal | true_const | false_const | maybe_const .

(* --- 2. Выражения --- *)
primary_expr       = identifier | constant | "(" expression ")" .
postfix_expr       = primary_expr
                   | postfix_expr "[" expression "]"
                   | postfix_expr "(" [ argument_list ] ")"
                   | postfix_expr "." identifier
                   | postfix_expr "->" identifier
                   | postfix_expr "++"
                   | postfix_expr "--" .
argument_list      = expression { "," expression } .

unary_expr         = postfix_expr
                   | "++" unary_expr
                   | "--" unary_expr
                   | unary_operator unary_expr .
unary_operator     = "+" | "-" | "!" | "~" | "*" | "&"
                   | "sizeof" | "размер" .

multiplicative_expr = unary_expr { ( "*" | "/" | "%" ) unary_expr } .
additive_expr      = multiplicative_expr { ( "+" | "-" ) multiplicative_expr } .
shift_expr         = additive_expr { ( "<<" | ">>" ) additive_expr } .
relational_expr    = shift_expr { ( "<" | ">" | "<=" | ">=" ) shift_expr } .
equality_expr      = relational_expr { ( "==" | "!=" ) relational_expr } .

bitwise_and_expr   = equality_expr { "&" equality_expr } .
bitwise_xor_expr   = bitwise_and_expr { "^" bitwise_and_expr } .
bitwise_or_expr    = bitwise_xor_expr { "|" bitwise_xor_expr } .
logical_and_expr   = bitwise_or_expr { "&&" bitwise_or_expr } .
logical_or_expr    = logical_and_expr { "||" logical_and_expr } .

ternary_expr       = logical_or_expr
                     "??" expression ":?" expression ":!" expression .
assignment_expr    = ternary_expr
                   | unary_expr assignment_operator assignment_expr .
assignment_operator = "=" | "+=" | "-=" | "*=" | "/=" | "%="
                     | "<<=" | ">>=" | "&=" | "|=" | "^=" .
expression         = assignment_expr .

(* --- 3. Объявления --- *)
void_type          = "void" | "пусто" .
tryte_type         = "tryte" | "трайт" .
tshort_type        = "tshort" | "тшорт" | "short" | "коротк" .
tint_type          = "tint" | "тцел" .
tlong_type         = "tlong" | "тдлин" .
tlong_long_type    = "tlong" "long" | "тдлинлонг" .
tfloat_type        = "tfloat" | "твещ" .
tdouble_type       = "tdouble" | "тдвойн" .
trit_type          = "trit" | "трит" .
tril_type          = "tril" | "трил" .
signed_spec        = "signed" | "знак" .
unsigned_spec      = "unsigned" | "беззнак" .

type_specifier     = void_type | tryte_type | tshort_type | tint_type
                   | tlong_type | tlong_long_type
                   | tfloat_type | tdouble_type
                   | trit_type | tril_type
                   | signed_spec | unsigned_spec
                   | struct_or_union_specifier | enum_specifier
                   | typedef_name .

struct_or_union    = "struct" | "структура" | "union" | "объединение" .
struct_or_union_specifier = struct_or_union [ identifier ] "{" { struct_declaration } "}"
                         | struct_or_union identifier .
struct_declaration = type_specifier struct_declarator_list ";" .
struct_declarator_list = struct_declarator { "," struct_declarator } .
struct_declarator  = declarator | [ declarator ] ":" expression .

enum_kw            = "enum" | "перечисление" .
enum_specifier     = enum_kw [ identifier ] "{" enumerator_list "}"
                   | enum_kw identifier .
enumerator_list    = enumerator { "," enumerator } .
enumerator         = identifier [ "=" expression ] .

type_qualifier     = "const" | "конст" | "volatile" | "изменч" .

declaration_specifiers = ( type_specifier | type_qualifier )
                         { type_specifier | type_qualifier } .
declarator         = pointer_decl direct_declarator | direct_declarator .
direct_declarator  = identifier
                   | "(" declarator ")"
                   | direct_declarator "[" [ expression ] "]"
                   | direct_declarator "(" parameter_list ")"
                   | direct_declarator "(" ")" .
pointer_decl       = "*" [ type_qualifier { type_qualifier } ] [ pointer_decl ] .
parameter_list     = parameter_declaration { "," parameter_declaration } .
parameter_declaration = declaration_specifiers [ declarator | abstract_declarator ] .
abstract_declarator = pointer_decl [ direct_abstract_declarator ]
                    | direct_abstract_declarator .
direct_abstract_declarator = "(" abstract_declarator ")"
                           | "[" [ expression ] "]"
                           | "(" [ parameter_list ] ")" .
initializer        = expression | "{" initializer_list [ "," ] "}" .
initializer_list   = initializer { "," initializer } .
declaration        = declaration_specifiers [ init_declarator_list ] ";" .
init_declarator_list = init_declarator { "," init_declarator } .
init_declarator    = declarator [ "=" initializer ] .

(* --- 4. Операторы --- *)
if_kw              = "if" | "если" .
maybe_kw           = "maybe" | "может" .
else_kw            = "else" | "иначе" .
switch_kw          = "switch" | "выбор" .
case_kw            = "case" | "случай" .
default_kw         = "default" | "умолчание" .
while_kw           = "while" | "пока" .
do_kw              = "do" | "делать" .
for_kw             = "for" | "для" .
continue_kw        = "continue" | "продолжить" .
break_kw           = "break" | "прервать" .
return_kw          = "return" | "возврат" .
goto_kw            = "goto" | "перейти" .

statement          = expression_statement
                   | compound_statement
                   | selection_statement
                   | switch_statement
                   | labeled_statement
                   | iteration_statement
                   | jump_statement .
expression_statement = [ expression ] ";" .
compound_statement   = "{" { declaration | statement } "}" .
selection_statement  = if_kw "(" expression ")" statement
                       ( [ maybe_kw statement ] [ else_kw statement ]
                       | [ else_kw statement ] [ maybe_kw statement ] ) .
switch_statement     = switch_kw "(" expression ")" statement .
case_label           = case_kw expression ":" .
default_label        = default_kw ":" .
labeled_statement    = case_label statement | default_label statement .
iteration_statement  = while_kw "(" expression ")" statement
                     | do_kw statement while_kw "(" expression ")" ";"
                     | for_kw "(" [ expression ] ";" [ expression ] ";"
                               [ expression ] ")" statement .
jump_statement       = continue_kw ";"
                     | break_kw ";"
                     | return_kw [ expression ] ";"
                     | goto_kw identifier ";" .

(* --- 5. Внешние определения --- *)
extern_kw          = "extern" | "внеш" .
static_kw          = "static" | "статич" .
auto_kw            = "auto" | "авто" .
register_kw        = "register" | "регистр" .
inline_kw          = "inline" | "встроен" .
typedef_kw         = "typedef" | "типрег" .

translation_unit   = { external_definition } .
external_definition = function_definition | declaration .
function_definition  = declaration_specifiers declarator
                      [ declaration_list ] compound_statement .
declaration_list    = { declaration } .
```