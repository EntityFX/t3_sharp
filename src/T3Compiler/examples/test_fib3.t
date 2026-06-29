tint fib(tint n) {
    if (n <= 1) { return n; }
    tint a = fib(n - 1);
    tint b = fib(n - 2);
    return a + b;
}
tint main() {
    return fib(2);
}