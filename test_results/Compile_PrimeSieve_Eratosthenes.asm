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
    SUBI SP, SP, 34
    LI RW,2
    STOREI RW, RZ, -3
loop_1:
    LOADI RX, RZ, -3
    LI RY,30
    CMP RX,RY
    LIMM R0,body_2
    JLE R0
    LIMM RW,wend_3
    JMP RW
body_2:
    LI RX,1
    PUSH RX
    LOADI RY, RZ, -3
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LOADI RY, RZ, -3
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -3
    LIMM R0,loop_1
    JMP R0
wend_3:
    LI RW,2
    STOREI RW, RZ, -3
loop_4:
    LOADI RY, RZ, -3
    PUSH RY
    LOADI R0, RZ, -3
    POP RW
    MUL RX,RW,R0
    LI R0,30
    CMP RX,R0
    LIMM RW,body_5
    JLE RW
    LIMM RY,wend_6
    JMP RY
body_5:
    LOADI R0, RZ, -3
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    LI RW,1
    CMP R0,RW
    LIMM RX,then_8
    JE RX
    LIMM RY,end_7
    JMP RY
then_8:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI RW, RZ, -3
    POP RX
    MUL RY,RX,RW
    STOREI RY, RZ, -2
loop_9:
    LOADI RW, RZ, -2
    LI RX,30
    CMP RW,RX
    LIMM R0,body_10
    JLE R0
    LIMM RW,wend_11
    JMP RW
body_10:
    LI RX,0
    PUSH RX
    LOADI RY, RZ, -2
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R0, RZ, -3
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -2
    LIMM R0,loop_9
    JMP R0
wend_11:
end_7:
    LOADI RW, RZ, -3
    PUSH RW
    LI RY,1
    POP R0
    ADD RW,R0,RY
    STOREI RW, RZ, -3
    LIMM RY,loop_4
    JMP RY
wend_6:
    LI R0,0
    STOREI R0, RZ, -1
    LI RX,2
    STOREI RX, RZ, -3
loop_12:
    LOADI RY, RZ, -3
    LI R0,30
    CMP RY,R0
    LIMM RW,body_13
    JLE RW
    LIMM RX,wend_14
    JMP RX
body_13:
    LOADI RY, RZ, -3
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    LI R0,1
    CMP RY,R0
    LIMM RW,then_16
    JE RW
    LIMM RX,end_15
    JMP RX
then_16:
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -1
end_15:
    LOADI R0, RZ, -3
    PUSH R0
    LI RW,1
    POP RY
    ADD R0,RY,RW
    STOREI R0, RZ, -3
    LIMM RW,loop_12
    JMP RW
wend_14:
    LOADI RY, RZ, -1
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
