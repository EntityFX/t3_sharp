// T-lang Nested Loop Test — sum of i*j for i=1..3, j=1..3
tint main() {
    tint sum = 0;
    tint i = 1;
    while (i <= 3) {
        tint j = 1;
        while (j <= 3) {
            tint p = i * j;
            sum = sum + p;
            j = j + 1;
        }
        i = i + 1;
    }
    return sum;  // 1*1 + 1*2 + 1*3 + 2*1 + 2*2 + 2*3 + 3*1 + 3*2 + 3*3 = 36
}