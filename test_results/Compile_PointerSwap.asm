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
    SUBI SP, SP, 5
    LI RW,5
    STOREI RZ, -1, RW
    LI RX,10
    STOREI RZ, -2, RX
    LI RY, 1
    SUB RY, RZ, RY
    STOREI RZ, -3, RY
    LI R0, 2
    SUB R0, RZ, R0
    STOREI RZ, -4, R0
    LOADI R3, RZ, -3
    LOADI RW,R3, 0
    STOREI RZ, -5, RW
    LOADI RX, RZ, -4
    LOADI RY,RX, 0
    LOADI R0, RZ, -5
    LOADI R3, RZ, -1
    PUSH R3
    LOADI RW, RZ, -2
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
