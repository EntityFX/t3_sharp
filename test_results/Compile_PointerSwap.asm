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
    STOREI RW, RZ, -5
    LI RX,10
    STOREI RX, RZ, -4
    LI RY, 5
    SUB RY, RZ, RY
    STOREI RY, RZ, -3
    LI R0, 4
    SUB R0, RZ, R0
    STOREI R0, RZ, -2
    LOADI RW, RZ, -3
    LOADI RX,RW, 0
    STOREI RX, RZ, -1
    LOADI RY, RZ, -2
    LOADI R0,RY, 0
    LOADI RW, RZ, -1
    LOADI RX, RZ, -5
    PUSH RX
    LOADI RY, RZ, -4
    POP R0
    ADD RW,R0,RY
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
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
