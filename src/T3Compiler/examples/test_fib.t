tint fib(tint n) {
    if (n <= 1) { return n; }
    else { return fib(n - 1) + fib(n - 2); }
}
tint main() {
    return fib(5);
}