; FPU Demonstration — Arithmetic and Square Root
; Demonstrates FADD, FSUB, FMUL, FDIV, FSQRT, FABS, FNEG

; --- Code ---
start:
    ; Initialize integer values for FPU conversion
    LI R0, 9       ; R0 = 9
    LI R1, 3       ; R1 = 3
    LI R2, -27     ; R2 = -27

    ; Convert to float using ITOF
    ITOF R0, R0    ; FW = 9.0
    ITOF R1, R1    ; FX = 3.0
    ITOF R2, R2    ; FY = -27.0

    ; FADD: FW = FW + FX = 9 + 3 = 12
    FADD R0, R0, R1

    ; FSUB: FX = FX - FY = 3 - (-27) = 30  (using FZ = 0 as temp)
    FSUB R3, R1, R2   ; FZ = FX - FY

    ; FMUL: FY = FY * FW = -27 * 12
    FMUL R2, R2, R0

    ; FDIV: FZ = FZ / FX = 30 / 3 = 10
    FDIV R3, R3, R1

    ; FSQRT: FW = sqrt(FW) = sqrt(12)
    FSQRT R0, R0

    ; FABS: FY = abs(FY) — FY was negative, now positive
    FABS R2, R2

    ; FNEG: FZ = -FZ = -10
    FNEG R3, R3

    ; Convert results back to integer and store
    ; Store FW (R0) result at addr_res1
    LI R4, addr_res1
    FTOI R0, R0     ; RW = int(FW)
    STORE R0, R4

    ; Store FZ (R3) result at addr_res2
    LI R4, addr_res2
    FTOI R3, R3     ; RZ = int(FZ)
    STORE R3, R4

    HALT

addr_res1:
    .word 0     ; result 1: int(sqrt(12)) = 3
addr_res2:
    .word 0     ; result 2: int(-10) = -10