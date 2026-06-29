; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 4
    LI RW,5
    NEG RX,RW
    STOREI RZ, -1, RX
    LI RY,3
    STOREI RZ, -2, RY
    LI R0,10
    NEG R3,R0
    STOREI RZ, -3, R3
    LI RW,2
    STOREI RZ, -4, RW
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -2
    POP R0
    MUL R3,R0,RY
    PUSH R3
    LOADI RY, RZ, -3
    PUSH RY
    LOADI R0, RZ, -4
    POP RW
    DIV RX,RW,R0
    POP R0
    ADD RW,R0,RX
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
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
