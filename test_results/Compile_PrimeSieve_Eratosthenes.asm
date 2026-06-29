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
    LI RX,-32
    ADD RY, RZ, RX
    STOREI RY, 0, RW
loop_1:
    LI RX,-32
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
    LI RY,-32
    ADD R0, RZ, RY
    LOADI RX, R0, 0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP R0
    STOREI R4, 0, R0
    LI RX,-32
    ADD RY, RZ, RX
    LOADI R0, RY, 0
    PUSH R0
    LI RY,1
    POP RX
    ADD R3,RX,RY
    LI RY,-32
    ADD RX, RZ, RY
    STOREI RX, 0, R3
    LIMM RX,loop_1
    JMP RX
wend_3:
    LI RY,2
    LI RW,-32
    ADD RX, RZ, RW
    STOREI RX, 0, RY
loop_4:
    LI RW,-32
    ADD RY, RZ, RW
    LOADI RX, RY, 0
    PUSH RX
    LI RW,-32
    ADD R0, RZ, RW
    LOADI RY, R0, 0
    POP R0
    MUL RW,R0,RY
    LI RY,30
    CMP RW,RY
    LIMM R0,body_5
    JLE R0
    LIMM R3,wend_6
    JMP R3
body_5:
    LI RY,-32
    ADD R0, RZ, RY
    LOADI RX, R0, 0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RW,R4, 0
    LI RX,1
    CMP RW,RX
    LIMM R0,then_8
    JE R0
    LIMM RY,end_7
    JMP RY
then_8:
    LI RW,-32
    ADD RX, RZ, RW
    LOADI R3, RX, 0
    PUSH R3
    LI RW,-32
    ADD RY, RZ, RW
    LOADI RX, RY, 0
    POP RY
    MUL RW,RY,RX
    LI RX,-33
    ADD RY, RZ, RX
    STOREI RY, 0, RW
loop_9:
    LI RX,-33
    ADD R0, RZ, RX
    LOADI RY, R0, 0
    LI R0,30
    CMP RY,R0
    LIMM RX,body_10
    JLE RX
    LIMM R3,wend_11
    JMP R3
body_10:
    LI RW,0
    LI RY,-33
    ADD R0, RZ, RY
    LOADI RX, R0, 0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP R0
    STOREI R4, 0, R0
    LI RX,-33
    ADD RY, RZ, RX
    LOADI R0, RY, 0
    PUSH R0
    LI RX,-32
    ADD R3, RZ, RX
    LOADI RY, R3, 0
    POP R3
    ADD RX,R3,RY
    LI RY,-33
    ADD R3, RZ, RY
    STOREI R3, 0, RX
    LIMM R3,loop_9
    JMP R3
wend_11:
end_7:
    LI RW,-32
    ADD RX, RZ, RW
    LOADI RY, RX, 0
    PUSH RY
    LI RX,1
    POP RW
    ADD RY,RW,RX
    LI RX,-32
    ADD RW, RZ, RX
    STOREI RW, 0, RY
    LIMM RW,loop_4
    JMP RW
wend_6:
    LI RX,0
    LI R0,-34
    ADD R3, RZ, R0
    STOREI R3, 0, RX
    LI R3,2
    LI R0,-32
    ADD RW, RZ, R0
    STOREI RW, 0, R3
loop_12:
    LI R0,-32
    ADD RX, RZ, R0
    LOADI RW, RX, 0
    LI RX,30
    CMP RW,RX
    LIMM R0,body_13
    JLE R0
    LIMM RY,wend_14
    JMP RY
body_13:
    LI RW,-32
    ADD RX, RZ, RW
    LOADI R3, RX, 0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R3
    LOADI R0,R4, 0
    LI R3,1
    CMP R0,R3
    LIMM RX,then_16
    JE RX
    LIMM RW,end_15
    JMP RW
then_16:
    LI R0,-34
    ADD R3, RZ, R0
    LOADI RY, R3, 0
    PUSH RY
    LI R3,1
    POP R0
    ADD RW,R0,R3
    LI R3,-34
    ADD R0, RZ, R3
    STOREI R0, 0, RW
end_15:
    LI R3,-32
    ADD RX, RZ, R3
    LOADI R0, RX, 0
    PUSH R0
    LI RX,1
    POP R3
    ADD RY,R3,RX
    LI RX,-32
    ADD R3, RZ, RX
    STOREI R3, 0, RY
    LIMM R3,loop_12
    JMP R3
wend_14:
    LI R0,-34
    ADD R3, RZ, R0
    LOADI RX, R3, 0
    MOV R2,RX
    LIMM R3,epilogue_0
    JMP R3
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
