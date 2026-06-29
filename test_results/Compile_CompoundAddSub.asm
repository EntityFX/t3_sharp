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
    SUBI SP, SP, 1
    LI RW,10
    STOREI RW, RZ, -3
    LOADI RX, RZ, -3
    PUSH RX
    LI RY,5
    POP R0
    ADD R3,R0,RY
    STOREI R3, RZ, -3
    LOADI RY, RZ, -3
    PUSH RY
    LI R0,3
    POP RW
    SUB RX,RW,R0
    STOREI RX, RZ, -3
    LOADI R0, RZ, -3
    PUSH R0
    LI RW,2
    POP RY
    MUL R0,RY,RW
    STOREI R0, RZ, -3
    LOADI RW, RZ, -3
    PUSH RW
    LI RY,4
    POP R3
    DIV RW,R3,RY
    STOREI RW, RZ, -3
    LOADI RY, RZ, -3
    PUSH RY
    LI R3,4
    POP RX
    ADD RY,RX,R3
    STOREI RY, RZ, -3
    LOADI R3, RZ, -3
    MOV R2,R3
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 1
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
