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
    SUBI SP, SP, 1
    LI RW,10
    STOREI RW, RZ, -1
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,5
    POP R0
    ADD RW,R0,RY
    STOREI RW, RZ, -1
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,3
    POP RX
    SUB RY,RX,R0
    STOREI RY, RZ, -1
    LOADI R0, RZ, -1
    PUSH R0
    LI RX,2
    POP R0
    MUL RW,R0,RX
    STOREI RW, RZ, -1
    LOADI RX, RZ, -1
    PUSH RX
    LI R0,4
    POP RX
    DIV RY,RX,R0
    STOREI RY, RZ, -1
    LOADI R0, RZ, -1
    PUSH R0
    LI RX,4
    POP R0
    ADD RW,R0,RX
    STOREI RW, RZ, -1
    LOADI RX, RZ, -1
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
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
