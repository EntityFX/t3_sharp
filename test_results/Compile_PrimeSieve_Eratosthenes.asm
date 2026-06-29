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
    SUBI SP, SP, 34
    LI RW,2
    LI RX,-34
    ADD RY, RZ, RX
    STOREI RW, RY, 0
loop_1:
    LI RX,-34
    ADD R0, RZ, RX
    LOADI RY, R0, 0
    LI R0,30
    CMP RY,R0
    LIMM RX,body_2
    JLE RX
    LIMM R3,wend_3
    JMP R3
body_2:
    LI RW,1
    PUSH RW
    LI RY,-34
    ADD R0, RZ, RY
    LOADI RX, R0, 0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP R0
    STOREI R0,R4, 0
    LI R3,-34
    ADD RW, RZ, R3
    LOADI RY, RW, 0
    PUSH RY
    LI RW,1
    POP R3
    ADD RX,R3,RW
    LI RW,-34
    ADD R3, RZ, RW
    STOREI RX, R3, 0
    LIMM R3,loop_1
    JMP R3
wend_3:
    LI RW,2
    LI RY,-34
    ADD R0, RZ, RY
    STOREI RW, R0, 0
loop_4:
    LI RY,-34
    ADD R3, RZ, RY
    LOADI R0, R3, 0
    PUSH R0
    LI RY,-34
    ADD RW, RZ, RY
    LOADI R3, RW, 0
    POP RW
    MUL RY,RW,R3
    LI R3,30
    CMP RY,R3
    LIMM RW,body_5
    JLE RW
    LIMM RX,wend_6
    JMP RX
body_5:
    LI R0,-34
    ADD R3, RZ, R0
    LOADI RY, R3, 0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R3,R4, 0
    LI R0,1
    CMP R3,R0
    LIMM RW,then_8
    JE RW
    LIMM RX,end_7
    JMP RX
then_8:
    LI R0,-34
    ADD R3, RZ, R0
    LOADI RY, R3, 0
    PUSH RY
    LI R0,-34
    ADD RW, RZ, R0
    LOADI R3, RW, 0
    POP RW
    MUL R0,RW,R3
    LI R3,-35
    ADD RW, RZ, R3
    STOREI R0, RW, 0
loop_9:
    LI R3,-35
    ADD RX, RZ, R3
    LOADI RW, RX, 0
    LI RX,30
    CMP RW,RX
    LIMM R3,body_10
    JLE R3
    LIMM RY,wend_11
    JMP RY
body_10:
    LI R0,0
    PUSH R0
    LI RW,-35
    ADD RX, RZ, RW
    LOADI R3, RX, 0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RX
    STOREI RX,R4, 0
    LI RY,-35
    ADD R0, RZ, RY
    LOADI RW, R0, 0
    PUSH RW
    LI RY,-34
    ADD R3, RZ, RY
    LOADI R0, R3, 0
    POP R3
    ADD RY,R3,R0
    LI R0,-35
    ADD R3, RZ, R0
    STOREI RY, R3, 0
    LIMM R3,loop_9
    JMP R3
wend_11:
end_7:
    LI RW,-34
    ADD RX, RZ, RW
    LOADI R0, RX, 0
    PUSH R0
    LI RX,1
    POP RW
    ADD RY,RW,RX
    LI RX,-34
    ADD RW, RZ, RX
    STOREI RY, RW, 0
    LIMM RW,loop_4
    JMP RW
wend_6:
    LI RX,0
    LI R0,-36
    ADD R3, RZ, R0
    STOREI RX, R3, 0
    LI R3,2
    LI R0,-34
    ADD RW, RZ, R0
    STOREI R3, RW, 0
loop_12:
    LI R0,-34
    ADD RX, RZ, R0
    LOADI RW, RX, 0
    LI RX,30
    CMP RW,RX
    LIMM R0,body_13
    JLE R0
    LIMM RY,wend_14
    JMP RY
body_13:
    LI R3,-34
    ADD RW, RZ, R3
    LOADI R0, RW, 0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    LOADI RW,R4, 0
    LI R3,1
    CMP RW,R3
    LIMM RX,then_16
    JE RX
    LIMM RY,end_15
    JMP RY
then_16:
    LI R3,-36
    ADD RW, RZ, R3
    LOADI R0, RW, 0
    PUSH R0
    LI RW,1
    POP R3
    ADD RX,R3,RW
    LI RW,-36
    ADD R3, RZ, RW
    STOREI RX, R3, 0
end_15:
    LI RW,-34
    ADD RY, RZ, RW
    LOADI R3, RY, 0
    PUSH R3
    LI RY,1
    POP RW
    ADD R0,RW,RY
    LI RY,-34
    ADD RW, RZ, RY
    STOREI R0, RW, 0
    LIMM RW,loop_12
    JMP RW
wend_14:
    LI R3,-36
    ADD RW, RZ, R3
    LOADI RY, RW, 0
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 34
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
