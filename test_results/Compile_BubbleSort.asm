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
    SUBI SP, SP, 9
    LI RW,5
    PUSH RW
    LI RX,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,3
    PUSH R0
    LI R3,1
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,1
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
    LI RY,2
    PUSH RY
    LI R0,4
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,5
    STOREI RW, RZ, -8
    LI RX,0
    STOREI RX, RZ, -9
loop_1:
    LOADI RY, RZ, -9
    LOADI R0, RZ, -8
    CMP RY,R0
    LIMM R3,body_2
    JL R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LI RX,0
    STOREI RX, RZ, -10
loop_4:
    LOADI RY, RZ, -10
    LOADI R0, RZ, -8
    PUSH R0
    LI R3,1
    POP RW
    SUB RX,RW,R3
    CMP RY,RX
    LIMM R3,body_5
    JL R3
    LIMM RW,wend_6
    JMP RW
body_5:
    LOADI RY, RZ, -10
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R0,R4, 0
    LOADI R3, RZ, -10
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI RW,R4, 0
    CMP R0,RW
    LIMM RX,then_8
    JG RX
    LIMM R0,end_7
    JMP R0
then_8:
    LOADI R3, RZ, -10
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    LOADI RW,R4, 0
    STOREI RW, RZ, -11
    LOADI RX, RZ, -10
    PUSH RX
    LI RY,1
    POP R0
    ADD R3,R0,RY
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    LOADI RY,R4, 0
    PUSH RY
    LOADI R0, RZ, -10
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW,R4, 0
    LOADI RX, RZ, -11
    PUSH RX
    LOADI RY, RZ, -10
    PUSH RY
    LI R0,1
    POP R3
    ADD RW,R3,R0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    POP R0
    STOREI R0,R4, 0
end_7:
    LOADI R3, RZ, -10
    PUSH R3
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -10
    LIMM RX,loop_4
    JMP RX
wend_6:
    LOADI RY, RZ, -9
    PUSH RY
    LI R3,1
    POP RW
    ADD RX,RW,R3
    STOREI RX, RZ, -9
    LIMM R3,loop_1
    JMP R3
wend_3:
    LI RW,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    LOADI RY,R4, 0
    PUSH RY
    LI R0,1
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    LOADI R3,R4, 0
    POP RW
    ADD RX,RW,R3
    PUSH RX
    LI R3,2
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    LOADI RW,R4, 0
    POP RY
    ADD R0,RY,RW
    PUSH R0
    LI RW,3
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    LOADI RY,R4, 0
    POP R3
    ADD RW,R3,RY
    PUSH RW
    LI RY,4
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R3,R4, 0
    POP RX
    ADD RY,RX,R3
    MOV R2,RY
    LIMM R3,epilogue_0
    JMP R3
epilogue_0:
    ADDI SP, SP, 9
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
