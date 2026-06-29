; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 4
    LI RW,7
    LI R4, -3
    ADD R4, RZ, R4
    STOREI RW,R4, 0
    LI RX,3
    LI R4, -4
    ADD R4, RZ, R4
    STOREI RX,R4, 0
    LI RY, -3
    ADD RY, RZ, RY
    STOREI RY, RZ, -5
    LI R0, -4
    ADD R0, RZ, R0
    STOREI R0, RZ, -6
    LOADI R3, RZ, -5
    LOADI RW,R3, 0
    PUSH RW
    LOADI RX, RZ, -6
    LOADI RY,RX, 0
    POP R0
    ADD R3,R0,RY
    MOV R2,R3
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 4
    POP R4
    POP R3
    POP RZ
    RET

; --- Global Variables ---

; --- Data Section ---

; --- StdLib ---
strlen:
    PUSH RZ
    PUSH R3
    PUSH R4
    LI R2, 0
strlen_loop:
    LOADI R1, RW, 0
    CMPI R1, 0
    JE strlen_end
    ADDI R2, 1
    ADDI RW, 1
    JMP strlen_loop
strlen_end:
    POP R4
    POP R3
    POP RZ
    RET
putchar:
    PUSH RZ
    PUSH R3
    PUSH R4
    OUTI RW, 0
    POP R4
    POP R3
    POP RZ
    RET
getchar:
    PUSH RZ
    PUSH R3
    PUSH R4
    INI R2, 0
    POP R4
    POP R3
    POP RZ
    RET
