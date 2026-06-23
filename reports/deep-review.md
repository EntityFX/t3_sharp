# Академическое ревью проекта T3Sharp

**Объект ревью:** `https://github.com/EntityFX/t3_sharp.git`  
**Проверенный commit:** `643c1f7d92baaff59f6dfa740d224e5a2cdd8712`  
**Дата ревью:** 2026-06-23  
**Метод:** статический анализ исходного кода, документации, тестов, CI-конфигурации и структуры решения.  
**Ограничение проверки:** локальное выполнение `dotnet build` и `dotnet test` не проводилось, так как в среде ревью отсутствует установленный .NET SDK (`dotnet: command not found`). Поэтому все выводы о прохождении тестов сформулированы осторожно: как выводы по исходному коду, внутренним отчетам проекта и CI-настройкам, а не как подтвержденный локальный прогон.

## Оглавление

1. [Краткая итоговая оценка](#1-краткая-итоговая-оценка)
2. [Оценочная матрица](#2-оценочная-матрица)
3. [Цели проекта и фактическое достижение целей](#3-цели-проекта-и-фактическое-достижение-целей)
4. [Архитектурный анализ](#4-архитектурный-анализ)
5. [Анализ троичных типов данных](#5-анализ-троичных-типов-данных)
6. [ISA, кодирование и декодирование](#6-isa-кодирование-и-декодирование)
7. [Процессорная модель](#7-процессорная-модель)
8. [Ассемблер и дизассемблер](#8-ассемблер-и-дизассемблер)
9. [FPU](#9-fpu)
10. [T-lang compiler](#10-t-lang-compiler)
11. [CLI и GUI](#11-cli-и-gui)
12. [Тестирование и CI](#12-тестирование-и-ci)
13. [Документация и управление требованиями](#13-документация-и-управление-требованиями)
14. [Репозиторная гигиена и packaging](#14-репозиторная-гигиена-и-packaging)
15. [Основные технические риски](#15-основные-технические-риски)
16. [Рекомендованный план улучшения](#16-рекомендованный-план-улучшения)
17. [Заключение](#17-заключение)
18. [Дополнительная оценка зрелости по модели исследовательского ПО](#18-дополнительная-оценка-зрелости-по-модели-исследовательского-по)
19. [Рекомендуемые приемочные тесты](#19-рекомендуемые-приемочные-тесты)
20. [Финальная управленческая рекомендация](#20-финальная-управленческая-рекомендация)

## 1. Краткая итоговая оценка

T3Sharp производит впечатление амбициозного исследовательского проекта, ориентированного на моделирование сбалансированной троичной вычислительной архитектуры. В проекте уже есть содержательное ядро: базовые троичные типы данных, представление 18- и 54-тритных слов, простая модель in-order процессора, ассемблер, дизассемблер, CLI-симулятор, начальный компилятор T-lang, FPU-слой и заметный набор unit/integration-тестов. Наличие англоязычной и русскоязычной документации, планов развития, gap-analysis отчета и CI workflow является сильной стороной проекта.

Однако текущий уровень реализации существенно ниже уровня заявлений в `README.md`. Документация позиционирует T3Sharp как почти полный “ternary computer simulator suite” с поддержкой T3-18/T3-54, VLIW, SIMD, speculation, GUI, full ISA, cycle-accurate simulation, hardware counters, CLI debugger и 269 тестами. Реальный код ближе к прототипу: реализован в основном однотактный/in-order симулятор для 18-тритных инструкций, базовый ассемблер, частичная FPU-модель и игрушечный компилятор. Компоненты VLIW/SIMD/speculation фактически не доведены до исполняемой микроархитектуры; `src/T3Simulator.VLIW` отсутствует; register windowing описан, но не интегрирован в процессор; T3-54 decode выглядит концептуально небезопасно; T-lang compiler имеет простую фиксированную модель памяти и регистров; сам отчет проекта прямо фиксирует проблемы с предикацией и сложными конструкциями T-lang.

С точки зрения инженерного качества проект находится на уровне **исследовательского прототипа с хорошей предметной идеей, но недостаточно устойчивой архитектурой и несинхронизированной документацией**. Его нельзя считать завершенным симулятором заявленной архитектуры. При этом он может служить хорошей основой для дальнейшей работы, если сначала стабилизировать спецификацию, привести документацию к фактическому состоянию и исправить ядро ISA/decoder/execution/test contracts.

**Общая оценка качества: 5.1 / 10.**

Эта оценка не является низкой для исследовательского прототипа: в проекте есть работающая основа, доменная глубина и тестовая инфраструктура. Но как публично заявленный “comprehensive simulator suite” проект пока не достигает собственных целей.

## 2. Оценочная матрица

| Критерий | Оценка 0-10 | Комментарий |
|---|---:|---|
| Соответствие заявленным целям | 4.0 | README заявляет существенно больше, чем реально реализовано. Особенно проблемны VLIW, SIMD, speculation, T3-54 execution, GUI и полнота compiler toolchain. |
| Архитектурная целостность | 5.0 | Есть разделение на `TritTypes`, `Common`, `InOrder`, `Assembler`, `CLI`, `Compiler`, но многие контракты между слоями не доведены до строгого состояния. |
| Корректность ISA и исполнения | 4.5 | Базовые инструкции выглядят работоспособными, но предикация, register windowing, Word54 decode, jump/disassembly и часть T-lang вызывают серьезные вопросы. |
| Качество типов данных | 6.5 | `Trit`, `Tryte`, `Word18`, `Word54` в целом полезны и тестируемы, но есть риск смешения checked constructors и wrap semantics, а также дорогие/неунифицированные операции. |
| Ассемблер/дизассемблер | 4.5 | Есть базовая функциональность, но парсинг ad hoc, диагностика слабая, round-trip тесты поверхностны, есть вероятная ошибка в disassembler для register jumps. |
| Процессорная модель | 5.0 | In-order processor покрывает много базовых операций, но не реализует архитектурные обещания register windowing, exception/interrupt model и полноценный T3-54/VLIW режим. |
| FPU | 5.5 | FPU слой присутствует, тесты есть, но модель выглядит упрощенной; status flags/FSR/interrupt semantics заявлены сильнее, чем реализованы. |
| T-lang compiler | 3.5 | Компилятор полезен как демонстрация, но не как надежный compiler backend: фиксированная память, слабый register allocation, отсутствие ABI/frames/scopes и сохранения live registers. |
| CLI/GUI | 5.0 | CLI достаточно полезен для прототипа. GUI проект есть, но не включен в solution и не соответствует уровню “готового” компонента. |
| Тестирование | 5.5 | Тестов много по количеству, но качество неоднородно: часть проверок сильная, часть только smoke/round-trip/contains; невозможно подтвердить прохождение локально. |
| CI и build hygiene | 5.0 | GitHub Actions есть, но solution исключает GUI, нет global SDK pinning, нет `.editorconfig`, нет license file, документация о 9 проектах/269 тестах устарела. |
| Документация | 6.0 | Документации много, но она смешивает фактическую реализацию, планы и желаемую архитектуру. Внутренние отчеты честнее README. |
| Поддерживаемость кода | 4.5 | Много плотного однострочного кода, generic contracts используются не всегда строго, диагностические ошибки общие, API не всегда инкапсулированы. |
| Исследовательская ценность | 7.5 | Идея и предметная область сильные; проект может быть ценной платформой для экспериментов с balanced ternary. |

**Итоговая интегральная оценка:** **5.1 / 10**.

## 3. Цели проекта и фактическое достижение целей

Главная цель, заявленная в README, состоит в создании комплексного набора .NET-инструментов для троичной архитектуры: симулятор процессоров T3-18/T3-54, единая ISA, in-order и VLIW микроархитектуры, FPU, предикация, hardware counters, CLI debugger, number converter, GUI, ассемблер, компилятор и документация. В таком виде цель является широкой системной целью, требующей строгой спецификации ISA, стабильной модели исполнения, понятного ABI, проверяемой модели памяти, надежного assembler/disassembler pipeline и полноценных acceptance-тестов.

Фактическое состояние проекта частично закрывает только нижние уровни этой цели. Реализованы базовые типы данных для balanced ternary, есть модель памяти, простая модель процессора, кодировщик/декодировщик инструкций, ассемблер и дизассемблер, набор инструкций in-order CPU, CLI и некоторый T-lang pipeline. Это уже немало. Однако проект пока не достигает полноты, заявленной в README. Ключевые несоответствия следующие.

Во-первых, заявлена поддержка T3-54 с VLIW, SIMD и speculation. В дереве исходников нет проекта `src/T3Simulator.VLIW`; есть только `T3VliwAssembler`, который собирает bundle-like строки в 54-тритное слово. Но ассемблер сам по себе не означает наличие исполняющей VLIW микроархитектуры, conflict detection, speculation protocol, rollback/commit state или SIMD execution. README описывает эти возможности как features, но код показывает скорее “план” или “набросок”.

Во-вторых, register windowing заявлен в архитектуре, но `ProcessorBase` имеет массив из 9 регистров, а `T3InOrderProcessor.CALL/RET` работают как обычный stack-based call с сохранением return PC. Есть класс `RegisterWindow`, где описаны 27 физических регистров и WP mapping, но он не интегрирован в процессорное исполнение. Это типичный признак незавершенного архитектурного перехода: спецификационный helper существует, тесты для helper есть, но runtime-контракт не применен.

В-третьих, README заявляет “cycle-accurate simulation”, но реализация фактически использует статические latency increments внутри большого switch. Это полезно как instruction latency accounting, но не является cycle-accurate симуляцией микроархитектуры в строгом смысле: нет pipeline state, hazard model, forwarding, stalls от structural/data/control hazards, VLIW conflict execution или memory timing model. Лучше называть это “instruction-level latency accounting”.

В-четвертых, CLI debugger заявлен как один из ключевых features. CLI действительно достаточно развит для прототипа: есть load/run/step/dump/breakpoints/trace/disassemble/memory/stack. Но это скорее interactive monitor, чем debugger в полноценном смысле: нет символов, source mapping, expression evaluation, watchpoints, call stack reconstruction или ABI-aware frame display.

В-пятых, T-lang compiler по структуре ближе к учебному compiler prototype. Он генерирует ассемблер, умеет простые функции, переменные, массивы, указатели и структуры на уровне тестовых сценариев, но его модель памяти фиксирована, область видимости упрощена, параметры функций практически не реализованы как ABI, регистры выделяются round-robin, caller-save не сохраняется, stack frames отсутствуют как строгая сущность. Это не умаляет ценности прототипа, но противоречит впечатлению “compiler suite”.

Следовательно, проект достигает цели “создать экспериментальный прототип balanced ternary simulator and tooling”, но не достигает цели “создать комплексный, архитектурно завершенный симулятор T3 family”.

## 4. Архитектурный анализ

Структурно проект разделен разумно: `TritTypes` содержит доменные типы; `T3Simulator.Common` содержит opcode, memory, devices, base processor, encoder/decoder, ALU/FPU helpers; `T3Simulator.InOrder` содержит исполняющий процессор; `T3Assembler` содержит assembler front-end; `T3Simulator.CLI` предоставляет интерактивный интерфейс; `T3Compiler` реализует T-lang frontend/backend; tests разделены на несколько проектов. Это хорошая исходная модульность.

Но архитектурная целостность нарушается из-за расхождения между спецификационными слоями и исполняющими слоями. Например, `RegisterWindow` описывает 27 physical registers, но процессор хранит только 9. `Word54` существует как тип, но decode path для 54-тритного слова не демонстрирует строгого выбора слота или VLIW execution. README говорит о VLIW processor implementation, но solution его не содержит. FPU registers описаны как shared with integer register file, но фактически есть отдельный массив `FRegisters`, что может быть нормальным design choice, но должно быть явно отражено в спецификации.

Крупный риск архитектуры состоит в том, что спецификация не является единственным источником истины. В проекте одновременно существуют README, `docs/t3-architecture.md`, `docs/t3-isa-reference.md`, `plans/*.md`, `reports/*.md` и исходный код. Эти источники расходятся. Например, предикация в документации описана как `PR[pred-1]`, а код берет trit `predIndex + 11`; отчет разработки фиксирует, что это место было проблемным. В таких проектах первичная задача не в расширении функциональности, а в стабилизации contracts: instruction format, operand encoding, predicate mapping, register model, call ABI, memory model, exception model, FPU status model.

Еще одна проблема: generic design используется не везде безопасно. `T3InOrderProcessor<TWord>` формально generic по `IT3Word<TWord>`, но FPU load/store явно кастует через `(Word18)(object)ReadWord(...)`. Это означает, что часть кода фактически предполагает `Word18`, даже когда generic тип может быть `Word54`. Это важно, потому что README обещает T3-18 и T3-54, а не только T3-18. Подобные места нужно либо ограничить типом, либо разделить execution backends, либо ввести четкий интерфейс для low/high word conversions.

С инженерной точки зрения архитектуре не хватает “contract tests” между слоями. Например, для каждой инструкции должен быть тест вида: assembler encodes, decoder decodes, processor executes, disassembler prints canonical form, assembler parses printed form, semantic state matches. Сейчас есть частичные тесты, но многие round-trip проверки недостаточны: они проверяют строковое `Contains`, количество слов или простые инструкции, но не ловят ошибки регистра в jump disassembly или сложные operand combinations.

## 5. Анализ троичных типов данных

Компонент `TritTypes` является одной из сильных частей проекта. `Trit` реализован как небольшой immutable value type с понятными операциями. `Word18` и `Word54` реализуют диапазоны balanced ternary, conversion to/from string, arithmetic operators, tritwise logical operations и comparison. Тестов на эти типы достаточно много по сравнению с остальными частями проекта.

Однако и здесь есть нюансы. Во-первых, в `Word18` public factory `FromLong` вызывает checked constructor, который выбрасывает исключение при выходе за диапазон. При этом arithmetic operators используют `FromWrapped`, то есть wrap-around semantics. Это может быть правильной моделью машинного слова, но тогда API должен явно разделять “construct exact value” и “construct machine word with wrap”. Сейчас поведение может удивлять: `new Word18(max + 1)` падает, но `max + 1` через operator может wrap. Для эмулятора CPU это нормально, для пользовательской библиотеки типов нужно очень четкое именование.

Во-вторых, `Word54.ToLong()` и `ToInt()` неизбежно теряют информацию при значениях за пределами `long`/`int`. Это ожидаемо, но в проекте есть места, где generic `TWord` затем приводится к `long` для PC, memory address, decode или display. Если T3-54 действительно должен поддерживаться, нужно строго определить, какие значения Word54 допустимы как адреса и как 18-тритные инструкции извлекаются из 54-тритного слова.

В-третьих, `GetTrit` и tritwise operations реализованы по-разному в `Word18` и `Word54`; часть операций использует repeated division/powers. Для прототипа это нормально, но если проект позиционируется как simulator suite, стоит централизовать balanced ternary digit extraction, чтобы избежать несогласованности порядка тритов, особенно потому что именно order/indexing уже всплыл в predication bug.

В-четвертых, `TritXor` семантически задан как sum modulo 3 with balanced mapping. Это предметно корректно для выбранной алгебры, но документация должна фиксировать именно эту алгебру. В троичной логике существует много вариантов XOR-like functions; если ISA выбирает один вариант, tests должны покрывать truth table полностью.

Оценка компонента типов: **6.5 / 10**. Это рабочая основа, но для production-level библиотеки нужны более строгие контракты, единый backend extraction и ясная политика overflow/wrap.

## 6. ISA, кодирование и декодирование

Самый критичный слой для такого проекта — instruction format. `InstructionEncoder` использует формат `[Pred(3)][Opcode(6)][Args(9)]`, где value = `pred*3^15 + opcode*3^9 + args`. Для 18-тритного слова это логично. Но реализация `InstructionDecoder.Decode(Word18)` использует `Math.Round(val / power)` для извлечения balanced digits. Такой подход может работать для ограниченного диапазона, но он не самый надежный способ декодирования balanced ternary fields. Более строгий подход — использовать обобщенную функцию extract balanced field by width and position, которая работает через modulo balanced range, а не через floating-point round. Даже если текущие значения безопасны для double, архитектурно это лишний риск и источник future bugs.

Предикация является явно проблемным местом. `EvaluatePredicate` разрешает только predIndex 1..3, а `GetPredicateFlag` берет trits 12..14. Документация говорит о PR как 9 тритах, three 3-trit predicate flags, а архитектурный план упоминает `PR[pred-1]`. Тесты используют `p.PR = Word18.FromLong(531441)`, что равно `3^12`, и ожидают, что predicate 1 проходит, а predicate 2 не проходит. Это означает, что tests подстроены под текущую реализацию или под промежуточный workaround, но не под ясную спецификацию. Внутренний `development_report.md` прямо говорит, что predication failing и root cause связан с trit order. До исправления этого слоя нельзя считать ISA стабильной.

Для `Word54` decode выглядит особенно тревожно. Метод `Decode(Word54 word)` берет `long val = word.ToLong(); return Decode(Word18.FromLong(val % P3_15));`. `P3_15` не соответствует 18-тритному диапазону; 18 trits имеют модуль `3^18`. Если цель — извлечь младшие 18 тритов из 54-тритного bundle, нужно использовать `3^18` и корректную balanced normalization. Если цель — decode slot 0/1/2 VLIW bundle, нужен slot selector. Текущий код выглядит как ошибочная заглушка.

Оценка ISA encoding/decoding: **4.0 / 10**. Базовый 18-тритный pipeline есть, но спецификация и код не согласованы достаточно строго.

## 7. Процессорная модель

`T3InOrderProcessor<TWord>` реализован как большой `switch` по opcode. Для прототипа это приемлемо: легко читать семантику каждой инструкции, легко добавить новые cases, latency increments находятся рядом с исполнением. Базовые arithmetic, memory, branch, stack, I/O и FPU операции покрыты.

Главный недостаток — модель процессора не соответствует архитектурным обещаниям. Register windowing не применяется. `CALL` просто уменьшает `SP`, пишет `PC+1`, затем прыгает. `RET` читает return address со стека. `WP` существует в base state, но не участвует в execution. Это расходится с README, где subroutines описаны как stack-based calls with register windowing, и с helper `RegisterWindow`.

Также отсутствует interrupt/exception model. При исключениях `Step` печатает `Processor Exception at PC ...`, выставляет `IsHalted = true` и возвращает false. Это приемлемо для раннего симулятора, но не для архитектуры с interrupt vectors, privilege modes, FPU status register и system-level features. Интересно, что `reports/t3-gap-analysis-specification.md` честно перечисляет отсутствие interrupt vectors, privilege modes, MMU, atomics и FPU interrupts. Следовательно, внутренний план проекта понимает незавершенность, но README подает ее мягче.

Memory model простая: 1M words, word-addressed. Hardware counters реализованы как специальные addresses. Это хорошая практичная часть. Но для полной архитектуры нужны права доступа, bounds behavior, interrupt-on-fault или хотя бы structured trap state. Сейчас invalid memory read/write приводит к exception и halt.

JM instruction реализован как `Cond == 0`, что совпадает с docs, но само название `JM` может ассоциироваться с maybe в троичной логике. Нужно убедиться, что это intentional. Если `JM` означает jump maybe, то `Cond == 0` корректно. Если оно дублирует `JE`, тогда есть ISA redundancy. README не объясняет это глубоко.

Оценка процессорной модели: **5.0 / 10**. Хороший учебный in-order interpreter, но не полноценная модель заявленной T3 family.

## 8. Ассемблер и дизассемблер

Ассемблер является рабочим, но слишком ad hoc. Он чистит комментарии, собирает labels в первом проходе, считает слова через `CountWords`, затем генерирует `Int128` words. Это нормальный минимальный подход. Однако реализация крайне плотная: много логики на одной строке, мало структурированных типов, нет нормальной диагностической модели, нет line/column source span, нет typed AST для assembly, нет строгой обработки директив.

Пример риска: `ResolveOperandValue` для `0n` и `0y` использует helpers `P9` и `P27`, которые на неизвестный символ добавляют пустую строку. То есть invalid input может не всегда приводить к ошибке. Для ассемблера это плохо: некорректная программа должна давать явную ошибку с адресом и позицией. В compiler side похожая проблема есть в `CodeGenerator.P9`, где неизвестный символ превращается в `"00"`, то есть silently changes program semantics.

Еще один конкретный дефект находится в `T3Disassembler.FormatInstruction`. Для jump/call инструкций при `instr.Immediate == 0` он печатает `GetRegName(instr.PhysOp2)`. Но register operand у J-type кодируется в `Op1`, а `PhysOp1` является правильным физическим индексом. Поэтому disassembler может неправильно печатать register jumps, кроме тех случаев, где оба значения случайно дают ожидаемый результат или тест поверхностный. Тесты проверяют `JMP R0`, `CALL R0`, но недостаточно проверяют все регистры.

Round-trip tests полезны, но многие из них проверяют только `Contains` или count. Более сильный тест должен декодировать каждую инструкцию и сравнивать opcode, predicate, operands, immediate, а затем исполнять semantic equivalence для программ. Для assembler/disassembler требуется canonical round-trip с полной таблицей opcodes и operand combinations.

Оценка assembler/disassembler: **4.5 / 10**. Компонент полезный, но требует переписывания вокруг явных синтаксических структур и строгой диагностики.

## 9. FPU

FPU слой выглядит как отдельная модель `T3Float` плюс операции `T3Fpu`. Тесты покрывают FADD, FSUB, FMUL, FDIV, FSQRT, FABS, FNEG, FCMP, FTOI, ITOF, FLW/FSW, FMOV, FCLASS, FSWAP, FZERO. Это хорошее покрытие для базового smoke layer.

Однако уровень FPU, заявленный в README, выше текущей реализации. README говорит о FSR, exception flags, interrupt behavior, rounding modes и full instruction set. В коде exceptions в `T3Fpu` бросают .NET exceptions, а processor ловит general exception и halt. Это не FPU status register semantics. `reports/t3-gap-analysis-specification.md` отдельно фиксирует “FPU exceptions don't generate interrupts” и “single rounding mode only”. Следовательно, FPU является базовым арифметическим helper, а не полноценным архитектурным FPU.

Также есть unsafe generic coupling: `FLW/FSW` в generic processor приводят memory word к `Word18` через object cast. Если processor инстанцирован как `T3InOrderProcessor<Word54>`, эти инструкции скорее всего некорректны. Это еще раз показывает, что T3-54 поддержка является недоведенной.

Оценка FPU: **5.5 / 10**. Для демонстраций достаточно, для архитектурного соответствия недостаточно.

## 10. T-lang compiler

T-lang compiler — самый слабый крупный компонент, если оценивать его как compiler. Он имеет lexer, parser, AST и code generator, что хорошо для прототипа. Тесты демонстрируют простую арифметику, while, nested while, arrays, structs, pointers, preprocessor и matrix multiplication. Но реализация codegen очень упрощена.

Основная проблема — отсутствие настоящей модели вызовов и памяти. Каждая функция начинает `_nextAddr = 200`; локальные переменные получают абсолютные адреса в памяти. Нет stack frame per call, нет параметров функций в codegen, нет глобальной layout model, нет scope nesting allocation, нет lifetime management. Если две функции используют локальные переменные, они могут конфликтовать по абсолютным адресам. Это может проходить в простых тестах, потому что программы ограничены, но масштабироваться не будет.

Register allocator — round-robin с исключением некоторых регистров. Он не знает liveness, caller/callee saved, expression tree pressure, temporary spilling, function call clobbers. В `reports/t3-gap-analysis-specification.md` это прямо отмечено: caller-saved regs not preserved и round-robin allocator. Такие проблемы обычно проявляются в nested expressions, function calls and loops. Тесты уже включают сложные случаи, но внутренний report фиксирует failures в nested while/matrix multiplication.

Кодогенерация silently defaults unknown expressions to `Imm(0)` для `_ => Imm(0)`. Это опасно: unsupported AST должен приводить к compile-time diagnostic, а не к неправильной программе, которая возвращает 0. Аналогично unknown identifiers в `LoadV` превращаются в `LI r,0`. Это удобно для раннего прототипа, но недопустимо для compiler correctness.

Логические операторы `||` и `&&` парсятся, но в `GenBin` operator switch не имеет отдельных cases для них и default maps to `ADD`. Это значит, что часть языка синтаксически принимается, но семантически неверно компилируется. То же касается многих C-like features из spec/gap docs.

Оценка T-lang compiler: **3.5 / 10**. Как демонстрационный компилятор — интересно; как надежный компонент toolchain — требует серьезной переработки.

## 11. CLI и GUI

CLI является полезным практическим инструментом. Он поддерживает загрузку программ, запуск, пошаговое выполнение, dump registers/memory/all, trace, breakpoints, disassemble, stack и memory ranges. Для исследовательского симулятора это правильный компонент. Качество CLI кода среднее: много console output напрямую, parsing commands вручную, exceptions в основном выводятся пользователю. Для инструмента разработчика это приемлемо.

GUI проект существует в `src/T3Simulator.GUI`, использует Avalonia и MVVM toolkit. Однако он не включен в `T3Sharp.sln`, а CI его не собирает. README в структуре называет GUI “planned”, но наличие проекта может вводить в заблуждение. Если GUI остается planned, его нужно пометить experimental и не использовать как показатель готовности. Если он должен быть поддерживаемым, его нужно включить в solution или иметь отдельную CI job, особенно потому что project target `net8.0-windows` и может требовать OS-specific handling.

Оценка CLI/GUI: **5.0 / 10**. CLI полезен; GUI пока не является поддерживаемой частью общего качества.

## 12. Тестирование и CI

В проекте действительно есть заметный набор тестов. По статическому подсчету найдено около 265 `[TestMethod]`, что близко к README claim, но не совпадает с “269 tests”. Тесты охватывают TritTypes, assembler/disassembler, common components, FPU, in-order processor, T-lang compiler and integration examples. Это сильная сторона проекта.

Но тестовая стратегия неоднородна. Тесты типов данных выглядят наиболее содержательно. Тесты процессора часто короткие и полезные, но не всегда проверяют negative paths. Ассемблерные tests часто используют `Contains`, что может пропустить неверные operands, неверные register fields, лишние строки или некорректную canonicalization. Некоторые тесты сами являются weak smoke: например, IO test ловит exceptions и ничего не assert-ит о состоянии.

Отдельно важно, что `reports/development_report.md` заявляет failing tests: `PredicationTest`, `FADD_Predicated_Honored`, `Compile_NestedWhile_SumProd`, `Compile_MatrixMul_2x2`. При этом эти тесты присутствуют в дереве как обычные `[TestMethod]`. Без локального `dotnet test` невозможно подтвердить текущее состояние, но наличие report с failures и текущая реализация predication подтверждают, что README “all tests pass” как минимум требует обновленной проверки.

CI workflow есть и запускает restore/build/test для трех test projects. Это хорошо. Но CI не собирает GUI, потому что GUI не в solution. Нет global.json, значит SDK version не зафиксирован. Нет `.editorconfig`, нет style/lint/analyzer enforcement. Нет coverage threshold, хотя coverlet dependency присутствует. Нет separate job для docs consistency или README claims.

Оценка тестирования и CI: **5.5 / 10**. Количество тестов хорошее, но качество oracle и синхронизация с документацией слабые.

## 13. Документация и управление требованиями

Документации много, и это ценное свойство. Есть README на английском и русском, architecture docs, ISA reference, ternary computing documentation, plans и reports. Это показывает, что автор проекта думает не только о коде, но и о концептуальной архитектуре.

Проблема в том, что документация не разделяет четыре разных слоя: “реализовано”, “частично реализовано”, “запланировано”, “исследовательская цель”. README говорит языком уже реализованных features, а gap-analysis честно показывает большое число критичных и high-priority gaps. Для внешнего пользователя README будет первичным источником, и он получит неверное ожидание.

Рекомендуется ввести status table в начало README:

- Stable: TritTypes, Word18, basic assembler, basic in-order simulator, CLI basic operations.
- Beta: FPU, T-lang compiler, disassembler, T3-54 word type.
- Experimental: VLIW assembler, GUI.
- Planned: VLIW processor, SIMD, speculation, register windowing, interrupts, privilege modes, MMU, atomics, full T-lang.

Также нужно использовать документацию как tests source. Например, ISA reference должен генерировать opcode table tests или наоборот генерироваться из enum/spec. Сейчас opcode values, docs and plans могут расходиться. Для такого проекта лучше иметь machine-readable `isa.yaml/json`, из которого генерируются docs, enum, assembler tables and tests.

Оценка документации: **6.0 / 10**. Большой объем и хорошая предметность, но слабая точность статусов.

## 14. Репозиторная гигиена и packaging

Проект заявляет MIT License и README ссылается на `LICENSE`, но файла `LICENSE` в репозитории нет. Это важный юридический и packaging дефект. Пользователь GitHub видит badge “license MIT”, но не может проверить текст лицензии в repo. Нужно добавить стандартный MIT license file.

Нет `.editorconfig`, нет `Directory.Build.props`, нет централизованного управления nullable/analyzers/warnings-as-errors. Часть csproj использует `ImplicitUsings enable`, часть disable. Версии test packages неоднородны: `TritTypes.Tests` использует `MSTest 4.0.2`, другие тесты используют `MSTest.TestFramework 3.6.1`. Это не обязательно ошибка, но лучше унифицировать.

Solution не включает GUI project, хотя src содержит GUI. README говорит “Build all projects (9 projects, 0 warnings)”, но по факту source csproj — 8, solution projects include solution folders and no GUI. Это мелкая, но показательная несогласованность.

Кодовый стиль неоднороден. Некоторые файлы написаны нормально, некоторые сжаты в одну строку (`using ...; using ...; namespace ...` и большие методы без форматирования). Для публичного проекта это ухудшает maintainability и code review. Нужен `dotnet format`, `.editorconfig` and analyzers.

Оценка repository hygiene: **4.5 / 10**.

## 15. Основные технические риски

Первый риск — **несогласованная спецификация ISA**. Пока predication, Word54 decode, jump representation and register model не стабилизированы, расширение функциональности будет накапливать несовместимости. Это нужно решать до добавления новых features.

Второй риск — **декларативное расхождение README и кода**. Проект может потерять доверие пользователей: пользователь ожидает VLIW processor, SIMD и full suite, а получает prototype. Это не проблема прототипа как такового; проблема в неверной упаковке.

Третий риск — **слабая диагностика assembler/compiler**. Silent fallback to zero или empty parse result создает неверные программы вместо compile errors. В компиляторах это всегда опасно.

Четвертый риск — **generic abstraction leakage**. `T3InOrderProcessor<TWord>` не полностью generic; Word54 и Word18 пути смешиваются. Это приведет к hidden runtime errors, если T3-54 начнут активно использовать.

Пятый риск — **test oracle weakness**. Большое число tests может создать иллюзию зрелости, но если tests проверяют только наличие строк или простые happy paths, они не защищают архитектуру.

## 16. Рекомендованный план улучшения

### Этап 1. Стабилизация фактического состояния

1. Обновить README: честно разделить implemented/beta/experimental/planned.
2. Добавить `LICENSE`.
3. Добавить `global.json` для SDK, `.editorconfig`, `Directory.Build.props`.
4. Запустить `dotnet format` и привести assembler/compiler code к читаемому стилю.
5. Убедиться, что CI reproduces local: `dotnet restore`, `dotnet build -warnaserror`, `dotnet test`.

### Этап 2. Стабилизация ISA core

1. Зафиксировать instruction format в одном machine-readable spec.
2. Исправить predicate mapping и переписать tests по spec.
3. Переписать decoder без floating-point rounding.
4. Исправить disassembler для jump/call register operands.
5. Добавить exhaustive tests для opcodes, registers, immediates, predicates.

### Этап 3. Процессорная модель

1. Решить: register windowing поддерживается сейчас или нет.
2. Если поддерживается — расширить register file до 27, интегрировать WP в register access, CALL/RET.
3. Если не поддерживается — удалить claims из README и docs, оставить как future.
4. Определить T3-54 execution model: scalar 54-bit in-order или VLIW bundle execution.
5. Разделить Word18 and Word54 processor backends, если generic abstraction мешает.

### Этап 4. Ассемблер и дизассемблер

1. Ввести assembly lexer/parser с source spans.
2. Запретить silent invalid character handling.
3. Реализовать canonical disassembly and parse-back tests.
4. Добавить директивы `.org`, `.align`, `.equ`, `.include` по мере необходимости.
5. Сделать error messages actionable.

### Этап 5. Компилятор T-lang

1. Ввести ABI: parameters, return register, caller/callee-saved rules.
2. Ввести stack frames или явную static memory model, но не смешивать.
3. Реализовать scopes and symbol tables.
4. Заменить round-robin allocator на хотя бы linear-scan or stack-spilling allocator.
5. Unsupported AST должен быть compile error, не `Imm(0)`.

### Этап 6. VLIW/SIMD/speculation

1. До реализации VLIW processor не заявлять его как implemented.
2. Определить bundle layout, slot decode, hazards, memory ordering, branch rules.
3. Добавить VLIW semantic tests.
4. Только после этого включать VLIW в README features.

## 17. Заключение

T3Sharp — интересный и потенциально ценный проект для исследования balanced ternary computing. Его сильные стороны: предметная идея, наличие базовых троичных типов, начальная модель CPU, ассемблер, CLI, FPU smoke coverage, большое количество документации и видимая roadmap-дисциплина. Проект явно создавался не как случайный набор файлов, а как попытка построить полноценную учебно-исследовательскую платформу.

Но текущий код не подтверждает масштаб заявлений в публичном README. Фактический уровень — beta/prototype. Архитектурные контракты еще не стабилизированы; некоторые компоненты существуют только как plans/stubs; часть ключевых features не реализована; tests не дают полного доверия; documentation status размыт. Основные усилия сейчас должны быть направлены не на добавление новых инструкций или языковых возможностей, а на синхронизацию спецификации, документации, tests and core execution.

Если проект будет развиваться дисциплинированно, он может стать сильным образовательным симулятором троичной архитектуры. Для этого нужно прежде всего признать текущий статус как “research prototype”, стабилизировать ISA core, исправить предикацию, привести assembler/disassembler к строгому контракту, очистить README от overclaiming и перестроить compiler на реальной модели вызовов и памяти.

**Финальная оценка:** **5.1 / 10**.  
**Качество идеи:** высокое.  
**Качество реализации как прототипа:** среднее-положительное.  
**Качество реализации как заявленного comprehensive simulator suite:** недостаточное.  
**Главный рекомендуемый фокус:** спецификация и корректность core ISA перед расширением функциональности.

## 18. Дополнительная оценка зрелости по модели исследовательского ПО

Если оценивать T3Sharp не как коммерческий продукт, а как исследовательское программное обеспечение, картина становится более благоприятной, но выводы о незавершенности остаются. Для research software важны не только production-grade свойства, но и воспроизводимость, прозрачность эксперимента, ясность математической модели, возможность модификации и достаточная тестируемость гипотез. По этим критериям у T3Sharp есть заметный потенциал: проект документирует balanced ternary арифметику, хранит планы развития, содержит тесты и включает несколько пользовательских инструментов. Это делает его полезным как лабораторную площадку.

Однако даже исследовательское ПО должно четко различать “модель проверена”, “модель частично реализована” и “модель описана как будущая работа”. В текущем виде эти уровни перемешаны. Например, документация о VLIW и speculation выглядит как архитектурное описание уже существующей системы, хотя код не содержит исполняющего VLIW процессора. Для научной или учебной публикации это было бы существенным методологическим дефектом: читатель не сможет понять, какие результаты можно воспроизвести, а какие являются концептуальным проектированием.

Сильным шагом было бы введение уровня зрелости для каждого модуля. Например:

| Модуль | Предлагаемый статус | Критерий перехода на следующий уровень |
|---|---|---|
| `TritTypes` | Beta / near-stable | Полное property-based тестирование для диапазонов и edge cases. |
| `InstructionEncoder/Decoder` | Alpha | Формальная спецификация field extraction и exhaustive tests. |
| `T3InOrderProcessor` | Alpha/Beta | Исправленная предикация, строгая call/return модель, тесты всех opcodes. |
| `T3Assembler` | Alpha | Нормальный parser, source diagnostics, negative tests. |
| `T3Compiler` | Prototype | ABI, scopes, frames, liveness, compile errors вместо silent fallback. |
| `T3VliwAssembler` | Experimental | Связка с реальным VLIW execution backend. |
| GUI | Experimental | Включение в solution/CI или явное исключение из supported surface. |

Такой статусный слой помог бы и пользователям, и авторам проекта. Он снижает риск неправильных ожиданий и дает понятную карту стабилизации. В академическом стиле это можно оформить как “implementation maturity matrix”.

## 19. Рекомендуемые приемочные тесты

Текущие тесты полезны, но для выхода проекта на более высокий уровень нужны приемочные тесты, проверяющие целые контракты, а не отдельные методы. Ниже приведен набор тестовых классов, которые стоит добавить.

### 19.1. ISA conformance tests

Для каждого opcode нужно автоматически проверять:

1. Ассемблер принимает canonical syntax.
2. Encoder формирует ожидаемые поля.
3. Decoder восстанавливает opcode, predicate, operands, immediate.
4. Disassembler печатает canonical syntax.
5. Повторная сборка disassembly дает тот же machine word.
6. Процессор исполняет инструкцию с ожидаемым изменением состояния.

Особенно важны матрицы по всем регистрам `RW/RX/RY/RZ/R0..R4`, всем predicate indices и representative immediate values: `-364`, `-1`, `0`, `1`, `364`, plus out-of-range cases for `LIMM`.

### 19.2. Negative assembler tests

Ассемблер должен гарантированно отклонять:

- неизвестные регистры;
- неизвестные директивы;
- некорректные символы в `0n` и `0y`;
- out-of-range immediate, если инструкция не поддерживает widening;
- дублирующиеся labels;
- unresolved labels;
- неправильное число operands.

Сейчас часть invalid input может silently transform into zero/empty result. Это нужно устранить через tests.

### 19.3. Processor state transition tests

Для процессора нужно проверять не только final register value, но и `PC`, `SP`, `CycleCount`, `InstructionCount`, memory side effects, halt state and exception behavior. Например, `CALL/RET` должен проверять, что return address записан в правильную ячейку, `SP` восстановлен, `PC` равен ожидаемому адресу, а не только итоговый результат арифметики совпадает.

### 19.4. T-lang semantic tests

T-lang нужно тестировать не только через “return expected number”, но и через generated assembly inspection. Для функций с параметрами нужно проверять ABI. Для scopes нужно проверять shadowing. Для массивов нужно проверять разные размеры и вложенные индексы. Для unsupported syntax нужно ожидать compile error, а не silent zero.

### 19.5. Documentation conformance tests

Если проект сохраняет ISA reference в Markdown, стоит добавить скрипт, который сравнивает opcode table из документации с `Opcode.cs`. Еще лучше — генерировать документацию из одного `isa.json`. Это предотвратит расхождение, которое сейчас уже видно по claims и реальному коду.

## 20. Финальная управленческая рекомендация

С управленческой точки зрения T3Sharp находится в точке, где дальнейшее добавление возможностей может ухудшить качество, если не стабилизировать основу. Проект уже достаточно большой, чтобы простое “добавим еще инструкцию” приводило к росту несогласованности. Поэтому наиболее рациональная стратегия — не feature expansion, а **architecture consolidation**.

Практически это означает следующее:

1. На один-два цикла разработки заморозить добавление новых ISA features.
2. Сделать `README.md` честной витриной текущего состояния.
3. Превратить gap-analysis из отчета в tracking checklist.
4. Исправить predication как P0-дефект.
5. Исправить disassembler jump-register bug как дешевый P0/P1-дефект.
6. Решить судьбу T3-54: либо formally unsupported, либо реализовать корректный scalar/VLIW decode.
7. Перевести compiler unsupported paths из silent fallback в explicit diagnostics.
8. Ввести регулярный CI-gate: build, tests, format, docs/spec consistency.

После этого проект можно развивать двумя путями. Первый путь — “educational in-order ternary CPU”: сфокусироваться на T3-18, сделать маленькую, но корректную ISA, хороший CLI, учебные примеры и стабильный assembler. Это даст высокое качество быстрее. Второй путь — “research architecture suite”: оставить T3-54/VLIW/SIMD/speculation, но тогда нужна полноценная спецификация, отдельный VLIW execution backend, hazard/conflict model и более строгие тесты. Смешивать эти два пути в одной README без статусов не стоит.

Моя рекомендация — выбрать первый путь как базовую stable линию, а VLIW/SIMD/speculation держать в experimental ветке или отдельном milestone. Это повысит доверие к проекту и позволит пользователям получить реально работающую троичную платформу, а не набор частично реализованных обещаний.
