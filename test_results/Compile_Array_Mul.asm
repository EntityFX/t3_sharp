; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 7
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,2
    PUSH R0
    LI R3,1
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,3
    PUSH RX
    LI RY,2
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0,R4, 0
    LI R3,4
    PUSH R3
    LI RW,3
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX,R4, 0
    LI RY,5
    PUSH RY
    LI R0,4
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,0
    STOREI RW, RZ, -8
    LI RX,1
    STOREI RX, RZ, -9
loop_1:
    LOADI RY, RZ, -8
    LI R0,5
    CMP RY,R0
    LIMM R3,body_2
    JL R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -9
    PUSH RX
    LOADI RY, RZ, -8
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R0,R4, 0
    POP R3
    MUL RW,R3,R0
    STOREI RW, RZ, -9
    LOADI R0, RZ, -8
    PUSH R0
    LI R3,1
    POP RX
    ADD RY,RX,R3
    STOREI RY, RZ, -8
    LIMM R3,loop_1
    JMP R3
wend_3:
    LOADI RX, RZ, -9
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
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
