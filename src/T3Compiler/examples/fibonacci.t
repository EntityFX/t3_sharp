// T-lang Fibonacci Test
// Computes 10th Fibonacci number iteratively

tint main() {
    tint n = 10;
    tint a = 0;
    tint b = 1;
    tint i = 0;

    while (i < n) {
        tint tmp = a + b;
        a = b;
        b = tmp;
        i = i + 1;
    }

    return a;  // should return 55
}