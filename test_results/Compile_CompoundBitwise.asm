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
    SUBI SP, SP, 2
    LI RW,5
    STOREI RZ, -1, RW
    LI RX,3
    STOREI RZ, -2, RX
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -2
    POP R3
    AND RW,R3,R0
    STOREI RZ, -1, RW
    LOADI R0, RZ, -1
    PUSH R0
    LOADI R3, RZ, -2
    POP RX
    OR RY,RX,R3
    STOREI RZ, -1, RY
    LOADI R3, RZ, -1
    PUSH R3
    LOADI RX, RZ, -2
    POP R0
    XOR R3,R0,RX
    STOREI RZ, -1, R3
    LOADI RX, RZ, -1
    PUSH RX
    LOADI R0, RZ, -2
    POP RW
    ADD RX,RW,R0
    MOV R2,RX
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
