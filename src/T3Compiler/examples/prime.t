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

// Prime number library: uses global primes[] and primeCount

tint primes[200];
tint primeCount;

tint sieve(tint n) {
    tint i = 2;
    while (i <= n) { primes[i] = 1; i = i + 1; }
    i = 2;
    while (i * i <= n) {
        if (primes[i] == 1) {
            tint j = i * i;
            while (j <= n) { primes[j] = 0; j = j + i; }
        }
        i = i + 1;
    }
    primeCount = 0;
    i = 2;
    while (i <= n) {
        if (primes[i] == 1) { primeCount = primeCount + 1; }
        i = i + 1;
    }
    return primeCount;
}

tint isPrime(tint n) {
    if (n < 2) { return 0; }
    tint i = 2;
    while (i * i <= n) {
        if (n % i == 0) { return 0; }
        i = i + 1;
    }
    return 1;
}

tint sumPrimes(tint n) {
    tint sum = 0;
    tint i = 2;
    while (i <= n) {
        if (isPrime(i) == 1) { sum = sum + i; }
        i = i + 1;
    }
    return sum;
}