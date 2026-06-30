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
    LI RW,10
    LI R4, 4
    SUB R4, RZ, R4
    STOREI RW, R4, 0
    LI RX,15
    LI R4, 3
    SUB R4, RZ, R4
    STOREI RX, R4, 0
    LI RY, 4
    SUB RY, RZ, RY
    STOREI RY, RZ, -2
    LOADI R0, RZ, -2
    LI RW,0
    ADD RX,R0,RW
    LOADI RX,RX, 0
    PUSH RX
    LOADI RW, RZ, -2
    LI RY,1
    ADD R0,RW,RY
    LOADI R0,R0, 0
    POP RY
    ADD RW,RY,R0
    MOV R2,RW
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
