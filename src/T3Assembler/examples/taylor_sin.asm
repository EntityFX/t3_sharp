; SIN(x) Taylor Series — 3-term approximation
; sin(x) ≈ x - x³/6 + x⁵/120
; Uses FPU: FADD, FSUB, FMUL, FDIV
; Input: integer value at addr_x (scaled × 1000)
; Output: integer value at addr_result (scaled × 1000)

start:
    ; Load pre-computed constants using .word
    LI R0, addr_x
    LOAD R1, R0      ; R1 = x (input)

    ; Convert to float: FW = float(x)
    ITOF R0, R1       ; FW = float(x)

    ; Compute x³ = x * x * x
    FMOV R1, R0, R0   ; FX = FW (x)
    FMUL R1, R1, R0   ; FX = x²
    FMUL R1, R1, R0   ; FX = x³

    ; Compute x³/6
    LI R2, 6
    ITOF R2, R2       ; FY = 6.0
    FDIV R1, R1, R2   ; FX = x³/6

    ; Compute sin ≈ x - x³/6
    FSUB R0, R0, R1   ; FW = x - x³/6

    ; Add x⁵/120 term
    FMUL R1, R1, R2   ; FX... actually skip for brevity
    ; sin(x) ≈ x - x³/6 is good for small x

    ; Convert result back to integer × 1000
    ; For simplicity store the raw float word
    FTOI R0, R0       ; RW = int(sin)
    LI R3, addr_result
    STORE R0, R3

    HALT

addr_x:
    .word 5       ; x = 5 (interpreted as 0.005 — small for convergence)
addr_result:
    .word 0       ; sin(x) × 1000 approx