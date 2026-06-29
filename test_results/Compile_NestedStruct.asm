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
    SUBI SP, SP, 2
    LI RW,10
    LI R4, -3
    ADD R4, RZ, R4
    STOREI RW,R4, 0
    LI RX,20
    LI R4, -4
    ADD R4, RZ, R4
    STOREI RX,R4, 0
    LI R4, -3
    ADD R4, RZ, R4
    LOADI RY,R4, 0
    PUSH RY
    LI R4, -4
    ADD R4, RZ, R4
    LOADI R0,R4, 0
    POP R3
    ADD RW,R3,R0
    MOV R2,RW
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    ADDI SP, SP, 2
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
