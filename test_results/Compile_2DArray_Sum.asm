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
    SUBI SP, SP, 11
    LI RW,0
    STOREI RW, RZ, -12
loop_1:
    LOADI RX, RZ, -12
    LI RY,9
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM R3,wend_3
    JMP R3
body_2:
    LOADI RW, RZ, -12
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    PUSH R0
    LOADI RX, RZ, -12
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LOADI R3, RZ, -12
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -12
    LIMM RW,loop_1
    JMP RW
wend_3:
    LI RX,0
    STOREI RX, RZ, -13
    LI R0,0
    STOREI R0, RZ, -12
loop_4:
    LOADI R3, RZ, -12
    LI RW,9
    CMP R3,RW
    LIMM RX,body_5
    JL RX
    LIMM RY,wend_6
    JMP RY
body_5:
    LOADI R0, RZ, -13
    PUSH R0
    LOADI R3, RZ, -12
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    LOADI RW,R4, 0
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -13
    LOADI RW, RZ, -12
    PUSH RW
    LI RX,1
    POP R0
    ADD R3,R0,RX
    STOREI R3, RZ, -12
    LIMM RX,loop_4
    JMP RX
wend_6:
    LOADI R0, RZ, -13
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 11
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
