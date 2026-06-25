tint countPrimes(tint n) {
    tint sieve[101];
    tint i = 2;
    while (i <= n) { sieve[i] = 1; i = i + 1; }
    i = 2;
    while (i * i <= n) {
        if (sieve[i] == 1) {
            tint j = i * i;
            while (j <= n) { sieve[j] = 0; j = j + i; }
        }
        i = i + 1;
    }
    tint count = 0; i = 2;
    while (i <= n) {
        if (sieve[i] == 1) { count = count + 1; }
        i = i + 1;
    }
    return count;
}

tint main() {
    return countPrimes(100);
}