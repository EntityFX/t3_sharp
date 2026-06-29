enum Color {
    Red = 1,
    Green,
    Blue = -1
}

tint get_val(Color c) {
    return c;
}

tint main() {
    Color myColor = Green; // should be 2
    tint val = get_val(myColor);
    return val;
}