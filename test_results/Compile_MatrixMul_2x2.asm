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
    SUBI SP, SP, 16
    LI RW,1
    LI RX,0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RY
    STOREI R4, 0, RY
    LI RY,2
    LI RX,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,3
    LI RX,2
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LI R3,4
    LI RX,3
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R3
    POP RW
    STOREI R4, 0, RW
    LI RW,5
    LI RX,0
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RX
    STOREI R4, 0, RX
    LI RX,6
    LI RX,1
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RX
    POP RY
    STOREI R4, 0, RY
    LI RY,7
    LI RX,2
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,8
    LI RX,3
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LI R3,0
    STOREI RZ, -13, R3
loop_1:
    LOADI RX, RZ, -13
    LI RW,2
    CMP RX,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LI R0,0
    LI R3,-14
    ADD RW, RZ, R3
    STOREI RW, 0, R0
loop_4:
    LI R3,-14
    ADD RX, RZ, R3
    LOADI RW, RX, 0
    LI RX,2
    CMP RW,RX
    LIMM R3,body_5
    JL R3
    LIMM RY,wend_6
    JMP RY
body_5:
    LI R0,0
    LI R3,-15
    ADD RW, RZ, R3
    STOREI RW, 0, R0
    LI RW,0
    LI R3,-16
    ADD RX, RZ, R3
    STOREI RX, 0, RW
loop_7:
    LI R3,-16
    ADD RY, RZ, R3
    LOADI RX, RY, 0
    LI RY,2
    CMP RX,RY
    LIMM R3,body_8
    JL R3
    LIMM R0,wend_9
    JMP R0
body_8:
    LI RW,-15
    ADD RX, RZ, RW
    LOADI R3, RX, 0
    PUSH R3
    LOADI RW, RZ, -13
    PUSH RW
    LI RY,2
    POP R0
    MUL R3,R0,RY
    PUSH R3
    LI R0,-16
    ADD RW, RZ, R0
    LOADI RY, RW, 0
    POP RW
    ADD R0,RW,RY
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI RX,R4, 0
    PUSH RX
    LI RW,-16
    ADD RX, RZ, RW
    LOADI RY, RX, 0
    PUSH RY
    LI RX,2
    POP RW
    MUL RY,RW,RX
    PUSH RY
    LI RW,-14
    ADD R0, RZ, RW
    LOADI RX, R0, 0
    POP R0
    ADD RW,R0,RX
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R0,R4, 0
    POP RW
    MUL RX,RW,R0
    POP R0
    ADD RW,R0,RX
    LI RX,-15
    ADD R0, RZ, RX
    STOREI R0, 0, RW
    LI RX,-16
    ADD R0, RZ, RX
    LOADI R0, R0, 0
    PUSH R0
    LI R0,1
    POP RX
    ADD R3,RX,R0
    LI R0,-16
    ADD RX, RZ, R0
    STOREI RX, 0, R3
    LIMM RX,loop_7
    JMP RX
wend_9:
    LI RW,-15
    ADD RX, RZ, RW
    LOADI R0, RX, 0
    LOADI RX, RZ, -13
    PUSH RX
    LI RW,2
    POP RY
    MUL R0,RY,RW
    PUSH R0
    LI RY,-14
    ADD R3, RZ, RY
    LOADI RW, R3, 0
    POP R3
    ADD RY,R3,RW
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    PUSH R0
    POP RW
    STOREI R4, 0, RW
    LI RY,-14
    ADD R3, RZ, RY
    LOADI RW, R3, 0
    PUSH RW
    LI R3,1
    POP RY
    ADD RW,RY,R3
    LI R3,-14
    ADD RY, RZ, R3
    STOREI RY, 0, RW
    LIMM RY,loop_4
    JMP RY
wend_6:
    LOADI R3, RZ, -13
    PUSH R3
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI RZ, -13, R0
    LIMM RX,loop_1
    JMP RX
wend_3:
    LI R3,0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,R3
    LOADI RY,R4, 0
    PUSH RY
    LI RW,1
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RX,RW,R3
    PUSH RX
    LI RW,2
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RY,RW,R3
    PUSH RY
    LI RW,3
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD R0,RW,R3
    MOV R2,R0
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
