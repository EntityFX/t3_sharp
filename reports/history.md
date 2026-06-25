# История развития T3Sharp (архив)

**Дата:** 25 июня 2026 г.

---

## Сводка архивных документов

| Файл | Дата | Назначение | Строк | Статус |
|------|------|-----------|-------|--------|
| [`deep-review.md`](./deep-review.md) | 23 июня 2026 | Академическое ревью проекта (аудит архитектуры, ISA, компилятора) | 371 | ✅ Архив — все замечания исправлены |
| [`t3-gap-analysis-specification.md`](./t3-gap-analysis-specification.md) | 14 июня 2026 | Gap-анализ процессора/ассемблера/T-lang (46 gaps) | 1085 | ✅ Все gaps закрыты в v2.0-v2.2 |
| [`development_report.md`](./development_report.md) | 25 июня 2026 | **Актуальный** отчёт о разработке (v2.2, 316 тестов) | 288 | 🟢 Актуален |

---

## Что было исправлено из gap-analysis (46 gaps → 0)

| ID | Gap | Статус |
|----|-----|--------|
| CPU-05 | JLE/JGE | ✅ v1.0 |
| CPU-08 | NOP | ✅ v1.0 |
| ASM-04 | `.equ` константы | ✅ v2.2 |
| ASM-11 | Labels as immediates | ✅ v2.2 (LIMM+R1) |
| TL-01 | Standard library | ✅ v2.2 (tio.asm) |
| TL-02 | Float literals → zero | ✅ v2.0 |
| TL-03 | String support | ✅ v2.2 (.string + strlen) |
| TL-04 | switch/case | ✅ v2.1 |
| TL-05 | enum | ✅ v2.2 |
| TL-09 | Array initialization | ✅ v2.1 |
| TL-10 | do/while | ✅ v2.1 |
| TL-11 | Ternary expression | ✅ v2.1 |
| TL-12 | goto | ✅ v2.2 |
| TL-15 | Caller-saved regs | ✅ v2.2 (ABI v3) |
| TL-16 | LIMM word reservation | ✅ v2.2 |
| ASM-03 | Assembly-time expressions | ✅ v2.2 (expression evaluator) |
| ... | (остальные 30 gaps) | ✅ |

---

## Что было исправлено из deep-review

| Замечание | Статус |
|-----------|--------|
| VLIW/SIMD/speculation не реализованы | 📋 В roadmap (низкий приоритет) |
| Register windowing не интегрирован | 📋 В roadmap |
| T3-54 decode небезопасен | ✅ v1.0 (FromWrappedLong) |
| T-lang: фиксированная память, слабый reg alloc | ✅ v2.0-v2.2 (ABI v3, spill) |
| Предикация | ✅ v1.0 |
| LIMM broken | ✅ v2.2 (I-type encoding) |
| CALL/return ABI | ✅ v2.2 (ABI v3) |
| Round-robin аллокатор → linear scan | 📋 В roadmap |