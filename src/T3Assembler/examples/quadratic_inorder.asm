; Quadratic Equation Solver — Integer Arithmetic
; Solves ax² + bx + c = 0  for a=1, b=-3, c=2
; Discriminant D = b² - 4ac, roots = (-b ± sqrt(D)) / (2a)
; Uses FSQRT for the square root via FPU

; --- Code Section ---
start:
    ; Load coefficients
    LI A, 1        ; A = a = 1
    LI B, -3       ; B = b = -3
    LI C, 2        ; C = c = 2

    ; Compute discriminant D = b² - 4ac
    MOV D, B       ; D = b
    MUL D, B       ; D = b²
    MOV E, A       ; E = a
    MUL E, 4       ; E = 4a
    MUL E, C       ; E = 4ac
    SUB D, E       ; D = b² - 4ac = 9 - 8 = 1

    ; Store D to memory for FSQRT
    LI R0, addr_disc
    STORE D, R0

    ; Compute sqrt(D) using FPU FSQRT
    ; Convert D to float, sqrt, convert back
    ; D is at mem[addr_disc] = 1
    LI R1, addr_disc ; R1 = address of D
    FLW FW, R1, 0    ; FW = float(D)
    FSQRT FW, FW     ; FW = sqrt(D)
    LI R2, addr_sqrt
    FSW FW, R2, 0    ; store sqrt back to mem
    ; Load sqrt result back as integer
    LOAD D, R2       ; D = mem[addr_sqrt] word — use raw bits

    ; Compute -b + sqrt(D)
    MOV E, B       ; E = b = -3
    NEG E          ; E = -b = 3
    ADD E, D       ; E = -b + sqrt(D) = 3 + 1 = 4

    ; Compute 2a
    MOV F, A       ; F = a
    ADD F, A       ; F = 2a = 2

    ; root1 = (-b + sqrt(D)) / (2a)
    MOV G, E       ; G = -b + sqrt(D)
    DIV G, F       ; G = root1 = 2

    ; Store root1
    LI R0, addr_root1
    STORE G, R0

    ; Compute -b - sqrt(D)
    MOV E, B       ; E = b = -3
    NEG E          ; E = -b = 3
    SUB E, D       ; E = -b - sqrt(D) = 3 - 1 = 2

    ; root2 = (-b - sqrt(D)) / (2a)
    MOV H, E       ; H = -b - sqrt(D)
    DIV H, F       ; H = root2 = 1

    ; Store root2
    LI R0, addr_root2
    STORE H, R0

    HALT

; --- Data Section ---
addr_disc:
    .word 0      ; discriminant value
addr_sqrt:
    .word 0      ; sqrt(discriminant)
addr_root1:
    .word 0      ; root 1
addr_root2:
    .word 0      ; root 2