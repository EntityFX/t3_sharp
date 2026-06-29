tint strlen(tint s);

tint main() {
    tint len1 = strlen("Hello"); // length 5
    tint len2 = strlen("World!"); // length 6
    return len1 + len2; // should return 11
}