// T-lang Math Library — libtmath.t
// Implementation of integer math functions on pure T-lang.
// Float functions (t_sqrt, t_sin, t_cos, etc.) remain as builtins in nanolib
// since the interpreter/compiler doesn't support user-defined float arithmetic.

tint t_abs(tint x) {
    if (x < 0) return -x;
    return x;
}

tint t_min(tint a, tint b) {
    if (a < b) return a;
    return b;
}

tint t_max(tint a, tint b) {
    if (a > b) return a;
    return b;
}

tint clamp(tint v, tint lo, tint hi) {
    if (v < lo) return lo;
    if (v > hi) return hi;
    return v;
}