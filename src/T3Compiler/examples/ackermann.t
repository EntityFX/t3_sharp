// Ackermann function — deep recursion test (ABI v3)
// A(0, n) = n + 1
// A(m, 0) = A(m - 1, 1)
// A(m, n) = A(m - 1, A(m, n - 1))

tint ack(tint m, tint n) {
    if (m == 0) { return n + 1; }
    if (n == 0) { return ack(m - 1, 1); }
    return ack(m - 1, ack(m, n - 1));
}

tint main() {
    return ack(3, 4);  // A(3,4) = 125
}