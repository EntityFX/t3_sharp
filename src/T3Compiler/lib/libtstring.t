// T-lang String Library — libtstring.t
// Implementation of string functions on pure T-lang.
// Strings are tryte arrays: element 0 = length, elements 1..N = characters.
// Builtins (putchar, print_int, etc.) remain in nanolib for I/O.
//
// NOTE: Search functions (t_strchr, t_strrchr, t_strstr) return tint index
// instead of tryte* pointer, since the interpreter doesn't support pointer arithmetic.
// Index 0 = not found, index 1..N = position in string.

// === Length and comparison ===

tint t_strlen(tryte* str) {
    return str[0];
}

tint t_strcmp(tryte* a, tryte* b) {
    tint i = 1;
    while (i <= a[0] && i <= b[0]) {
        if (a[i] < b[i]) return -1;
        if (a[i] > b[i]) return 1;
        i = i + 1;
    }
    if (a[0] < b[0]) return -1;
    if (a[0] > b[0]) return 1;
    return 0;
}

tint t_strncmp(tryte* a, tryte* b, tint n) {
    tint i = 1;
    tint limit = n;
    if (limit > a[0]) limit = a[0];
    if (limit > b[0]) limit = b[0];
    while (i <= limit) {
        if (a[i] < b[i]) return -1;
        if (a[i] > b[i]) return 1;
        i = i + 1;
    }
    if (n > limit) {
        if (a[0] < b[0]) return -1;
        if (a[0] > b[0]) return 1;
    }
    return 0;
}

// === Copy and concatenation ===
// Caller is responsible for ensuring dest has enough capacity.

void t_strcpy(tryte* dest, tryte* src) {
    tint i = 1;
    tint srcLen = src[0];
    while (i <= srcLen) {
        dest[i] = src[i];
        i = i + 1;
    }
    dest[0] = srcLen;
}

void t_strncpy(tryte* dest, tryte* src, tint n) {
    tint i = 1;
    tint maxCopy = n;
    if (src[0] < maxCopy) maxCopy = src[0];
    while (i <= maxCopy) {
        dest[i] = src[i];
        i = i + 1;
    }
    while (i <= n) {
        dest[i] = 0;
        i = i + 1;
    }
    dest[0] = n;
}

void t_strcat(tryte* dest, tryte* src) {
    tint destLen = dest[0];
    tint i = 1;
    tint srcLen = src[0];
    while (i <= srcLen) {
        dest[destLen + i] = src[i];
        i = i + 1;
    }
    dest[0] = destLen + srcLen;
}

// === Search (return index: 0 = not found, 1..N = position) ===

tint t_strchr(tryte* str, tryte c) {
    tint i = 1;
    while (i <= str[0]) {
        if (str[i] == c) return i;
        i = i + 1;
    }
    return 0;
}

tint t_strrchr(tryte* str, tryte c) {
    tint i = str[0];
    while (i >= 1) {
        if (str[i] == c) return i;
        i = i - 1;
    }
    return 0;
}

tint t_strstr(tryte* haystack, tryte* needle) {
    if (needle[0] == 0) return 1;
    tint i = 1;
    while (i <= haystack[0]) {
        if (haystack[i] == needle[1]) {
            tint match = 1;
            tint j = 1;
            while (j <= needle[0] && i + j - 1 <= haystack[0]) {
                if (haystack[i + j - 1] != needle[j]) {
                    match = 0;
                    break;
                }
                j = j + 1;
            }
            if (match == 1 && j > needle[0]) return i;
        }
        i = i + 1;
    }
    return 0;
}

// === Conversion ===

tint t_atoi(tryte* str) {
    if (str[0] == 0) return 0;
    tint i = 1;
    tint neg = 0;
    if (str[1] == '-') {
        neg = 1;
        i = 2;
    }
    tint val = 0;
    while (i <= str[0]) {
        val = val * 10 + (str[i] - '0');
        i = i + 1;
    }
    if (neg == 1) return -val;
    return val;
}

void t_itoa(tint value, tryte* buf) {
    tint i = 1;
    tint val = value;
    tint neg = 0;
    if (val < 0) {
        neg = 1;
        val = -val;
    }
    // Generate digits in reverse
    tint digits[20];
    tint nd = 0;
    if (val == 0) {
        digits[0] = 0;
        nd = 1;
    } else {
        while (val > 0) {
            digits[nd] = val % 10;
            val = val / 10;
            nd = nd + 1;
        }
    }
    // Write to buffer
    tint maxLen = buf[0] - 1;
    tint pos = 1;
    if (neg == 1 && pos <= maxLen) {
        buf[pos] = '-';
        pos = pos + 1;
    }
    tint j = nd - 1;
    while (j >= 0 && pos <= maxLen) {
        buf[pos] = digits[j] + '0';
        pos = pos + 1;
        j = j - 1;
    }
    buf[0] = pos - 1;
}

void t_baltoa(tint value, tryte* buf) {
    tint val = value;
    tint i = 1;
    tint maxLen = buf[0] - 1;
    // Generate balanced ternary digits
    tint digits[30];
    tint nd = 0;
    if (val == 0) {
        digits[0] = 0;
        nd = 1;
    } else {
        while (val != 0 && nd < 30) {
            tint rem = val % 3;
            if (rem > 1) rem = rem - 3;
            digits[nd] = rem;
            val = (val - rem) / 3;
            nd = nd + 1;
        }
    }
    // Write to buffer (reverse order)
    tint pos = 1;
    tint j = nd - 1;
    while (j >= 0 && pos <= maxLen) {
        if (digits[j] == 1) buf[pos] = '+';
        else if (digits[j] == -1) buf[pos] = '-';
        else buf[pos] = '0';
        pos = pos + 1;
        j = j - 1;
    }
    buf[0] = pos - 1;
}