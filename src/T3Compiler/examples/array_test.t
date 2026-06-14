// T-lang Array Test — sum array elements
tint main() {
    tint arr[5];
    arr[0] = 1;
    arr[1] = 2;
    arr[2] = 3;
    arr[3] = 4;
    arr[4] = 5;
    tint sum = arr[0] + arr[1] + arr[2] + arr[3] + arr[4];
    return sum; // 15
}