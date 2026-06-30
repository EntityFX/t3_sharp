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
    STOREI RX, RZ, -4
    LI RY,3
    STOREI RY, RZ, -3
    LI R0,10
    NEG RW,R0
    STOREI RW, RZ, -2
    LI RX,2
    STOREI RX, RZ, -1
    LOADI RY, RZ, -4
    PUSH RY
    LOADI R0, RZ, -3
    POP RW
    MUL RX,RW,R0
    PUSH RX
    LOADI R0, RZ, -2
    PUSH R0
    LOADI RW, RZ, -1
    POP RY
    DIV R0,RY,RW
    POP RW
    ADD RY,RW,R0
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
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
