tint fact(tint n) {
    tint r = 1;
    while (n > 1) {
        r = r * n;
        n = n - 1;
    }
    return r;
}
tint main() {
    return fact(7);
}