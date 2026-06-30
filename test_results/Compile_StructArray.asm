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
    LI RW,10
    PUSH RW
    LI RX,0
    LI RY,2
    MUL R0,RX,RY
    LI R4, 2
    SUB R4, RZ, R4
    ADD RX,R4,R0
    LI RY,0
    ADD RW,RX,RY
    POP RX
    STOREI RX, RW, 0
    LI R0,20
    PUSH R0
    LI RW,0
    LI RY,2
    MUL RX,RW,RY
    LI R4, 2
    SUB R4, RZ, R4
    ADD RW,R4,RX
    LI RY,1
    ADD RX,RW,RY
    POP RY
    STOREI RY, RX, 0
    LI RX,0
    PUSH RX
    LI RX,2
    POP RY
    MUL RW,RY,RX
    LI R4, 2
    SUB R4, RZ, R4
    ADD RX,R4,RW
    LI RY,0
    ADD RX,RX,RY
    LOADI RY,RX, 0
    PUSH RY
    LI RW,0
    PUSH RW
    LI RX,2
    POP RY
    MUL RX,RY,RX
    LI R4, 2
    SUB R4, RZ, R4
    ADD RW,R4,RX
    LI RY,1
    ADD RX,RW,RY
    LOADI R0,RX, 0
    POP RX
    ADD RY,RX,R0
    MOV R2,RY
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
