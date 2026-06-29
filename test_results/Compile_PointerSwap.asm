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
    SUBI SP, SP, 5
    LI RW,5
    STOREI RW, RZ, -3
    LI RX,10
    STOREI RX, RZ, -4
    LI RY, -3
    ADD RY, RZ, RY
    STOREI RY, RZ, -5
    LI R0, -4
    ADD R0, RZ, R0
    STOREI R0, RZ, -6
    LOADI R3, RZ, -5
    LOADI RW,R3, 0
    STOREI RW, RZ, -7
    LOADI RX, RZ, -6
    LOADI RY,RX, 0
    LOADI R0, RZ, -7
    LOADI R3, RZ, -3
    PUSH R3
    LOADI RW, RZ, -4
    POP RX
    ADD RY,RX,RW
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 5
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
