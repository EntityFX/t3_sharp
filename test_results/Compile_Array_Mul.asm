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
    SUBI SP, SP, 7
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, 7
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,2
    PUSH RX
    LI RY,1
    LI R4, 7
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,3
    PUSH RY
    LI R0,2
    LI R4, 7
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,4
    PUSH R0
    LI RW,3
    LI R4, 7
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,5
    PUSH RW
    LI RX,4
    LI R4, 7
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,0
    STOREI RX, RZ, -2
    LI RY,1
    STOREI RY, RZ, -1
loop_1:
    LOADI R0, RZ, -2
    LI RW,5
    CMP R0,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LOADI R0, RZ, -1
    PUSH R0
    LOADI RW, RZ, -2
    LI R4, 7
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP RX
    MUL RY,RX,RW
    STOREI RY, RZ, -1
    LOADI RW, RZ, -2
    PUSH RW
    LI RX,1
    POP R0
    ADD RW,R0,RX
    STOREI RW, RZ, -2
    LIMM RX,loop_1
    JMP RX
wend_3:
    LOADI R0, RZ, -1
    MOV R2,R0
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 7
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
