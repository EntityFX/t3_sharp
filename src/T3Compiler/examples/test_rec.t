tint fact(tint n) {
    if (n <= 1) { return 1; }
    return n * fact(n - 1);
}
tint main() {
    return fact(7);
}