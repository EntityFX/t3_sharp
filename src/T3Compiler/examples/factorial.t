// T-lang Factorial Test
// Computes factorial of 5 using while loop

tint main() {
    tint n = 5;
    tint result = 1;

    while (n > 1) {
        result = result * n;
        n = n - 1;
    }

    return result;  // should return 120
}