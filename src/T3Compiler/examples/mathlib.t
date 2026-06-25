// Math library: gcd, lcm, fib, fact

tint gcd(tint a, tint b) {
    if (b == 0) { return a; }
    return gcd(b, a % b);
}

tint lcm(tint a, tint b) {
    return (a * b) / gcd(a, b);
}

tint fib(tint n) {
    if (n <= 1) { return n; }
    return fib(n - 1) + fib(n - 2);
}

tint fact(tint n) {
    if (n <= 1) { return 1; }
    return n * fact(n - 1);
}