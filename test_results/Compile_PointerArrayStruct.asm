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
    LI RW,7
    LI R4, 4
    SUB R4, RZ, R4
    STOREI RW, R4, 0
    LI RX,3
    LI R4, 3
    SUB R4, RZ, R4
    STOREI RX, R4, 0
    LI RY, 4
    SUB RY, RZ, RY
    STOREI RY, RZ, -2
    LI R0, 3
    SUB R0, RZ, R0
    STOREI R0, RZ, -1
    LOADI RW, RZ, -2
    LOADI RX,RW, 0
    PUSH RX
    LOADI RY, RZ, -1
    LOADI R0,RY, 0
    POP RW
    ADD RX,RW,R0
    MOV R2,RX
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
