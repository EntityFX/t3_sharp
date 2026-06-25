tint partition(tint arr, tint low, tint high) {
    tint pivot = arr[high];
    tint i = low - 1;
    tint j = low;
    while (j < high) {
        if (arr[j] <= pivot) {
            i = i + 1;
            tint tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
        j = j + 1;
    }
    tint tmp = arr[i + 1];
    arr[i + 1] = arr[high];
    arr[high] = tmp;
    return i + 1;
}

tint main() {
    tint arr[8];
    arr[0]=5; arr[1]=2; arr[2]=8; arr[3]=1;
    arr[4]=9; arr[5]=3; arr[6]=7; arr[7]=4;
    return arr[0] + arr[1] + arr[2] + arr[3] + arr[4] + arr[5] + arr[6] + arr[7];
}