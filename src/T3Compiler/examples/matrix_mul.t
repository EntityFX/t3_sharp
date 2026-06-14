// T-lang Matrix Multiplication 2x2 — C[i][j] = sum_k A[i][k] * B[k][j]
// A = [[1,2],[3,4]], B = [[5,6],[7,8]]
// C = [[1*5+2*7, 1*6+2*8], [3*5+4*7, 3*6+4*8]] = [[19,22],[43,50]]
tint main() {
    tint a00 = 1; tint a01 = 2; tint a10 = 3; tint a11 = 4;
    tint b00 = 5; tint b01 = 6; tint b10 = 7; tint b11 = 8;

    tint c00 = a00*b00 + a01*b10;
    tint c01 = a00*b01 + a01*b11;
    tint c10 = a10*b00 + a11*b10;
    tint c11 = a10*b01 + a11*b11;

    tint sum = c00 + c01 + c10 + c11;
    return sum;  // 19+22+43+50 = 134
}