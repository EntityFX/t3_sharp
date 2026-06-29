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
    LI RW,10
    LI R4, -3
    ADD R4, RZ, R4
    STOREI RW,R4, 0
    LI RX,15
    LI R4, -4
    ADD R4, RZ, R4
    STOREI RX,R4, 0
    LI RY, -3
    ADD RY, RZ, RY
    STOREI RY, RZ, -5
    LOADI R0, RZ, -5
    LI R3,0
    SUB RW,R0,R3
    LOADI RW,RW, 0
    PUSH RW
    LOADI R3, RZ, -5
    LI RX,1
    SUB RY,R3,RX
    LOADI RY,RY, 0
    POP RX
    ADD R0,RX,RY
    MOV R2,R0
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
