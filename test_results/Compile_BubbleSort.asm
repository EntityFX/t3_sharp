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
    SUBI SP, SP, 9
    LI RW,5
    LI RX,0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RY
    STOREI R4, 0, RY
    LI RY,3
    LI RX,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,1
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
    LI RW,2
    LI RX,4
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RX
    STOREI R4, 0, RX
    LI RX,5
    STOREI RZ, -6, RX
    LI RX,0
    STOREI RZ, -7, RX
loop_1:
    LOADI RY, RZ, -7
    LOADI R0, RZ, -6
    CMP RY,R0
    LIMM R3,body_2
    JL R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LI RX,0
    STOREI RZ, -8, RX
loop_4:
    LOADI RY, RZ, -8
    LOADI R0, RZ, -6
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
    LOADI R0, RZ, -8
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI RY,R4, 0
    LOADI R3, RZ, -8
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI R0,R4, 0
    CMP RY,R0
    LIMM RY,then_8
    JG RY
    LIMM RW,end_7
    JMP RW
then_8:
    LOADI R0, RZ, -8
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI RX,R4, 0
    STOREI RZ, -9, RX
    LOADI R3, RZ, -8
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI R0,R4, 0
    LOADI RY, RZ, -8
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    PUSH R0
    POP RW
    STOREI R4, 0, RW
    LOADI RW, RZ, -9
    LOADI RY, RZ, -8
    PUSH RY
    LI RX,1
    POP R0
    ADD R3,R0,RX
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R3
    PUSH RW
    POP RX
    STOREI R4, 0, RX
end_7:
    LOADI RX, RZ, -8
    PUSH RX
    LI R3,1
    POP R0
    ADD RW,R0,R3
    STOREI RZ, -8, RW
    LIMM R3,loop_4
    JMP R3
wend_6:
    LOADI R0, RZ, -7
    PUSH R0
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI RZ, -7, R0
    LIMM RX,loop_1
    JMP RX
wend_3:
    LI R3,0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R3
    LOADI RY,R4, 0
    PUSH RY
    LI RW,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RX,RW,R3
    PUSH RX
    LI RW,2
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RY,RW,R3
    PUSH RY
    LI RW,3
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD R0,RW,R3
    PUSH R0
    LI RW,4
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RX,RW,R3
    MOV R2,RX
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
