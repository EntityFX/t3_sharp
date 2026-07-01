using TritTypes;
using T3Simulator.Common;

Console.WriteLine("=== Сравнение точности T3Float vs binary16 (half) ===");
Console.WriteLine();
Console.WriteLine("binary16: 1 sign + 5 exponent + 10 mantissa = 16 bits");
Console.WriteLine("  Мантисса: 10+1=11 бит, диапазон [1024, 2047] (нормализованная)");
Console.WriteLine("  Шаг квантования при value=3: 3 * 2^(-10) = 0.00293");
Console.WriteLine();
Console.WriteLine("T3Float: 6 exponent + 12 mantissa = 18 тритов ≈ 28.5 бит");
Console.WriteLine("  Мантисса: 12 тритов, диапазон [-265720, +265720]");
Console.WriteLine("  Шаг квантования при value=3: 3 * 3^(-12) = 3/531441 = 0.0000056");
Console.WriteLine();

// Демонстрация: какой exponent даёт максимальную точность для 3.14?
Console.WriteLine("=== Анализ FromDouble(3.14) — текущая реализация ===");
double v = 3.14;
double log3v = Math.Log(v) / Math.Log(3);
int expChosen = (int)Math.Round(log3v);
long mant = (long)Math.Round(v / Math.Pow(3, expChosen));
Console.WriteLine($"  log3({v}) = {log3v}");
Console.WriteLine($"  round = {expChosen}");
Console.WriteLine($"  mant = round({v} / 3^{expChosen}) = round({v / Math.Pow(3, expChosen)}) = {mant}");
Console.WriteLine($"  Результат: {mant} * 3^{expChosen} = {mant * Math.Pow(3, expChosen)}");
Console.WriteLine($"  Ошибка: {Math.Abs(v - mant * Math.Pow(3, expChosen)):F10}");
Console.WriteLine();

// Оптимальный exponent
Console.WriteLine("=== Оптимальный exponent для 3.14 ===");
Console.WriteLine("  Цель: найти минимальный exponent (максимальный |mant| ≤ 265720)");
long maxMant = 265720;
for (int e = 10; e >= -12; e--)
{
    double idealMant = v / Math.Pow(3, e);
    if (Math.Abs(idealMant) <= maxMant)
    {
        long optMant = (long)Math.Round(idealMant);
        double result = optMant * Math.Pow(3, e);
        double error = Math.Abs(v - result);
        Console.WriteLine($"  e={e,3}: mant=round({idealMant,10:F4})={optMant,6} -> {result,10:F10} (ошибка {error,10:F10})");
    }
}
Console.WriteLine();

// Сравнение с binary16
Console.WriteLine("=== Сравнение точности ===");
Console.WriteLine("binary16 при 3.0: шаг = 3 * 2^(-10) = 0.00293");
Console.WriteLine("T3Float при 3.0 с e=-10: шаг = 3^(-10) = 1/59049 = 0.0000169");
Console.WriteLine("T3Float в {0:F0} раз точнее binary16!", 0.00293 / (1.0/59049));
Console.WriteLine();

// Проверка: что даёт текущий FromDouble для разных значений
Console.WriteLine("=== Текущая реализация FromDouble() ===");
double[] testVals = { 3.14, 2.71, 1.5, 0.5, 0.1, 6.28, 100.0, 0.01 };
foreach (double val in testVals)
{
    var f = T3Float.FromDouble(val);
    double back = f.ToDouble();
    double error = Math.Abs(val - back);
    double optimalError = Math.Abs(val) / Math.Pow(3, 12); // теоретический минимум
    
    // Оптимальный exponent
    int bestE = -12;
    for (int e = 12; e >= -12; e--)
    {
        if (Math.Abs(val / Math.Pow(3, e)) <= maxMant)
        {
            bestE = e;
            break;
        }
    }
    long bestMant = (long)Math.Round(val / Math.Pow(3, bestE));
    double bestResult = bestMant * Math.Pow(3, bestE);
    double bestError = Math.Abs(val - bestResult);
    
    Console.WriteLine($"  {val,6:F2}: FromDouble={back,10:F6} (ошибка {error,10:F6}) | opt e={bestE,2} -> {bestResult,10:F6} (ошибка {bestError,10:F6})");
}
Console.WriteLine();

// ВЫВОД
Console.WriteLine("=== ВЫВОД ===");
Console.WriteLine("FromDouble() выбирает exponent = round(log3(|value|)), что даёт |mant| ≈ 1.");
Console.WriteLine("НО для максимальной точности нужно выбирать exponent так, чтобы |mant| → maxMant.");
Console.WriteLine();
Console.WriteLine("Текущий баг: FromDouble() НЕ нормализует мантиссу для максимальной точности.");
Console.WriteLine("Из-за этого T3Float теряет ~10 порядков точности для значений ~3.");
Console.WriteLine();
Console.WriteLine("Исправление: выбирать exponent = floor(log3(|value| / maxMant))");
Console.WriteLine("т.е. минимальный exponent, при котором |mant| ≤ maxMant.");