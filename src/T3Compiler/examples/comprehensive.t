// ===== Comprehensive Integration Test for T-lang =====
// Covers: union, enum, goto, do-while with break, struct array-of-structs

enum Status { Idle=0, Running=1, Done=2, Error=-1 };

union Data {
    tint i;
    tryte t;
};

struct Point {
    tint x;
    tint y;
};

tint factorial_goto(tint n) {
    tint result = 1;
loop_start:
    if (n <= 1) { goto done; }
    result = result * n;
    n = n - 1;
    goto loop_start;
done:
    return result;
}

tint main() {
    // 1. Enum test
    tint st = Running;
    if (st == 1) { st = Done; }

    // 2. Union test
    union Data d;
    d.i = 42;
    tint union_val = d.i;

    // 3. Goto-based factorial
    tint fact5 = factorial_goto(5);

    // 4. do-while with break
    tint dw = 0;
    tint dw_i = 0;
    do {
        if (dw_i >= 5) { break; }
        dw = dw + dw_i;
        dw_i = dw_i + 1;
    } while (1);

    // 5. Array of structs
    struct Point pts[3];
    pts[0].x = 10;
    pts[0].y = 20;
    pts[1].x = 30;

    return st + union_val + fact5 + dw + pts[0].x + pts[0].y + pts[1].x;
    //   2  +    42     +  120  + 10 +   10    +   20    +   30   = 234
}