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
    SUBI SP, SP, 16
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
    LI R0,0
    LI R4, -7
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,6
    PUSH RW
    LI RX,1
    LI R4, -7
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,7
    PUSH R0
    LI R3,2
    LI R4, -7
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,8
    PUSH RX
    LI RY,3
    LI R4, -7
    ADD R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0,R4, 0
    LI R3,0
    LI RW,-15
    ADD RX, RZ, RW
    STOREI R3, RX, 0
loop_1:
    LI RW,-15
    ADD RY, RZ, RW
    LOADI RX, RY, 0
    LI RY,2
    CMP RX,RY
    LIMM RW,body_2
    JL RW
    LIMM R0,wend_3
    JMP R0
body_2:
    LI R3,0
    LI RW,-16
    ADD RX, RZ, RW
    STOREI R3, RX, 0
loop_4:
    LI RW,-16
    ADD RY, RZ, RW
    LOADI RX, RY, 0
    LI RY,2
    CMP RX,RY
    LIMM RW,body_5
    JL RW
    LIMM R0,wend_6
    JMP R0
body_5:
    LI R3,0
    LI RW,-17
    ADD RX, RZ, RW
    STOREI R3, RX, 0
    LI RX,0
    LI RW,-18
    ADD RY, RZ, RW
    STOREI RX, RY, 0
loop_7:
    LI RW,-18
    ADD R0, RZ, RW
    LOADI RY, R0, 0
    LI R0,2
    CMP RY,R0
    LIMM RW,body_8
    JL RW
    LIMM R3,wend_9
    JMP R3
body_8:
    LI RX,-17
    ADD RY, RZ, RX
    LOADI RW, RY, 0
    PUSH RW
    LI RX,-15
    ADD R0, RZ, RX
    LOADI RY, R0, 0
    PUSH RY
    LI R0,2
    POP RX
    MUL R3,RX,R0
    PUSH R3
    LI RX,-18
    ADD RW, RZ, RX
    LOADI R0, RW, 0
    POP RW
    ADD RX,RW,R0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    LOADI R0,R4, 0
    PUSH R0
    LI RX,-18
    ADD RY, RZ, RX
    LOADI RW, RY, 0
    PUSH RW
    LI RY,2
    POP RX
    MUL R0,RX,RY
    PUSH R0
    LI RX,-16
    ADD R3, RZ, RX
    LOADI RY, R3, 0
    POP R3
    ADD RX,R3,RY
    LI R4, -7
    ADD R4, RZ, R4
    ADD R4,R4,RX
    LOADI RY,R4, 0
    POP R3
    MUL RW,R3,RY
    POP RY
    ADD R3,RY,RW
    LI RW,-17
    ADD RY, RZ, RW
    STOREI R3, RY, 0
    LI RW,-18
    ADD RX, RZ, RW
    LOADI RY, RX, 0
    PUSH RY
    LI RX,1
    POP RW
    ADD RY,RW,RX
    LI RX,-18
    ADD RW, RZ, RX
    STOREI RY, RW, 0
    LIMM RW,loop_7
    JMP RW
wend_9:
    LI R0,-17
    ADD R3, RZ, R0
    LOADI RX, R3, 0
    PUSH RX
    LI R0,-15
    ADD RW, RZ, R0
    LOADI R3, RW, 0
    PUSH R3
    LI RW,2
    POP R0
    MUL RX,R0,RW
    PUSH RX
    LI R0,-16
    ADD RY, RZ, R0
    LOADI RW, RY, 0
    POP RY
    ADD R0,RY,RW
    LI R4, -11
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW,R4, 0
    LI R0,-16
    ADD R3, RZ, R0
    LOADI RY, R3, 0
    PUSH RY
    LI R3,1
    POP R0
    ADD RW,R0,R3
    LI R3,-16
    ADD R0, RZ, R3
    STOREI RW, R0, 0
    LIMM R0,loop_4
    JMP R0
wend_6:
    LI RX,-15
    ADD RY, RZ, RX
    LOADI R3, RY, 0
    PUSH R3
    LI RY,1
    POP RX
    ADD R0,RX,RY
    LI RY,-15
    ADD RX, RZ, RY
    STOREI R0, RX, 0
    LIMM RX,loop_1
    JMP RX
wend_3:
    LI RY,0
    LI R4, -11
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R3,R4, 0
    PUSH R3
    LI RW,1
    LI R4, -11
    ADD R4, RZ, R4
    ADD R4,R4,RW
    LOADI RX,R4, 0
    POP RY
    ADD R0,RY,RX
    PUSH R0
    LI RX,2
    LI R4, -11
    ADD R4, RZ, R4
    ADD R4,R4,RX
    LOADI RY,R4, 0
    POP R3
    ADD RW,R3,RY
    PUSH RW
    LI RY,3
    LI R4, -11
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R3,R4, 0
    POP RX
    ADD RY,RX,R3
    MOV R2,RY
    LIMM R3,epilogue_0
    JMP R3
epilogue_0:
    ADDI SP, SP, 16
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
