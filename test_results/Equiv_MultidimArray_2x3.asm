; T→T3
__entry:
    LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 6
    LI RW,0
    PUSH RW
    LI RX, 0
    PUSH RX
    LI RY,0
    LI R0,3
    MUL RW, RY, R0
    POP RX
    ADD RY, RX, RW
    PUSH RY
    LI RX,0
    LI RW,1
    MUL R0, RX, RW
    POP RY
    ADD RW, RY, R0
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RY
    STOREI RY, R4, 0
    LI RW,1
    PUSH RW
    LI RY, 0
    PUSH RY
    LI R0,0
    LI RW,3
    MUL RX, R0, RW
    POP RY
    ADD R0, RY, RX
    PUSH R0
    LI RY,1
    LI RX,1
    MUL RW, RY, RX
    POP R0
    ADD RX, R0, RW
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP R0
    STOREI R0, R4, 0
    LI RX,2
    PUSH RX
    LI R0, 0
    PUSH R0
    LI RW,0
    LI RX,3
    MUL RY, RW, RX
    POP R0
    ADD RW, R0, RY
    PUSH RW
    LI R0,2
    LI RY,1
    MUL RX, R0, RY
    POP RW
    ADD RY, RW, RX
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP RW
    STOREI RW, R4, 0
    LI RY,10
    PUSH RY
    LI RW, 0
    PUSH RW
    LI RX,1
    LI RY,3
    MUL R0, RX, RY
    POP RW
    ADD RX, RW, R0
    PUSH RX
    LI RW,0
    LI R0,1
    MUL RY, RW, R0
    POP RX
    ADD R0, RX, RY
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RX
    STOREI RX, R4, 0
    LI R0,11
    PUSH R0
    LI RX, 0
    PUSH RX
    LI RY,1
    LI R0,3
    MUL RW, RY, R0
    POP RX
    ADD RY, RX, RW
    PUSH RY
    LI RX,1
    LI RW,1
    MUL R0, RX, RW
    POP RY
    ADD RW, RY, R0
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RY
    STOREI RY, R4, 0
    LI RW,12
    PUSH RW
    LI RY, 0
    PUSH RY
    LI R0,1
    LI RW,3
    MUL RX, R0, RW
    POP RY
    ADD R0, RY, RX
    PUSH R0
    LI RY,2
    LI RX,1
    MUL RW, RY, RX
    POP R0
    ADD RX, R0, RW
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP R0
    STOREI R0, R4, 0
    LI RX, 0
    PUSH RX
    LI R0,0
    LI RW,3
    MUL RX, R0, RW
    POP RY
    ADD R0, RY, RX
    PUSH R0
    LI RY,0
    LI RX,1
    MUL RW, RY, RX
    POP R0
    ADD RX, R0, RW
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    PUSH RX
    LI R0, 0
    PUSH R0
    LI RW,0
    LI RX,3
    MUL RY, RW, RX
    POP R0
    ADD RW, R0, RY
    PUSH RW
    LI R0,1
    LI RY,1
    MUL RX, R0, RY
    POP RW
    ADD RY, RW, RX
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP RW
    ADD RX,RW,RY
    PUSH RX
    LI RY, 0
    PUSH RY
    LI RW,0
    LI RY,3
    MUL R0, RW, RY
    POP RW
    ADD RX, RW, R0
    PUSH RX
    LI RW,2
    LI R0,1
    MUL RY, RW, R0
    POP RW
    ADD R0, RW, RY
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RW
    ADD RY,RW,R0
    PUSH RY
    LI R0, 0
    PUSH R0
    LI RW,1
    LI R0,3
    MUL RX, RW, R0
    POP RY
    ADD R0, RY, RX
    PUSH R0
    LI RY,0
    LI RX,1
    MUL R0, RY, RX
    POP RW
    ADD RX, RW, R0
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    POP RW
    ADD R0,RW,RX
    PUSH R0
    LI RX, 0
    PUSH RX
    LI RW,1
    LI RX,3
    MUL RY, RW, RX
    POP R0
    ADD RW, R0, RY
    PUSH RW
    LI R0,1
    LI RY,1
    MUL RX, R0, RY
    POP RW
    ADD RY, RW, RX
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP RW
    ADD RX,RW,RY
    PUSH RX
    LI RY, 0
    PUSH RY
    LI RW,1
    LI RY,3
    MUL R0, RW, RY
    POP RW
    ADD RX, RW, R0
    PUSH RX
    LI RW,2
    LI R0,1
    MUL RY, RW, R0
    POP RW
    ADD R0, RW, RY
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RW
    ADD RY,RW,R0
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    ADDI SP, SP, 6
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
