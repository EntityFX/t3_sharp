; T‑lang → T3 assembly
; ===================

__entry:
    LI R0, main
    CALL R0
    HALT

main:
    LI R0, 5
    LI R4, 10
    STORE R0, R4
    LI R1, 1
    LI R4, 11
    STORE R1, R4
loop_0:
    LI R4, 10
    LOAD R3, R4
    LI R5, 1
    CMP R3, R5
    LIMM R6, body_1
    JG R6
    LIMM R7, wend_2
    JMP R7
body_1:
    LI R4, 11
    LOAD R8, R4
    LI R4, 10
    LOAD R0, R4
    MUL R1, R8, R0
    LI R4, 11
    STORE R1, R4
    LI R4, 10
    LOAD R3, R4
    LI R5, 1
    SUB R6, R3, R5
    LI R4, 10
    STORE R6, R4
    LIMM R7, loop_0
    JMP R7
wend_2:
    LI R4, 11
    LOAD R8, R4
    MOV R2, R8
    RET
    RET

