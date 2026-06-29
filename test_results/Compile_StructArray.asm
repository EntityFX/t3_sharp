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
    PUSH RW
    LI RX,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,20
    PUSH R0
    LI R3,0
    LI R4, -4
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD RY,R4,RX
    LOADI RY,RY, 0
    PUSH RY
    LI R0,0
    LI R4, -4
    ADD R4, RZ, R4
    ADD R3,R4,R0
    LOADI R3,R3, 0
    POP RW
    ADD RX,RW,R3
    MOV R2,RX
    LIMM R3,epilogue_0
    JMP R3
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
