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
    PUSH RW
    LI RX,0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,3
    PUSH RX
    LI RY,1
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,1
    PUSH RY
    LI R0,2
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,4
    PUSH R0
    LI RW,3
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,2
    PUSH RW
    LI RX,4
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,5
    STOREI RX, RZ, -4
    LI RY,0
    STOREI RY, RZ, -3
loop_1:
    LOADI R0, RZ, -3
    LOADI RW, RZ, -4
    CMP R0,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LI R0,0
    STOREI R0, RZ, -2
loop_4:
    LOADI RW, RZ, -2
    LOADI RX, RZ, -4
    PUSH RX
    LI RY,1
    POP R0
    SUB RW,R0,RY
    CMP RW,RW
    LIMM RY,body_5
    JL RY
    LIMM R0,wend_6
    JMP R0
body_5:
    LOADI RX, RZ, -2
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    LOADI RY, RZ, -2
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    CMP RX,RX
    LIMM R0,then_8
    JG R0
    LIMM RW,end_7
    JMP RW
then_8:
    LOADI RY, RZ, -2
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    STOREI RY, RZ, -1
    LOADI R0, RZ, -2
    PUSH R0
    LI RW,1
    POP RX
    ADD RY,RX,RW
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    PUSH RY
    LOADI RW, RZ, -2
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -2
    PUSH RX
    LI R0,1
    POP RW
    ADD RX,RW,R0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP R0
    STOREI R0, R4, 0
end_7:
    LOADI RX, RZ, -2
    PUSH RX
    LI R0,1
    POP RW
    ADD RY,RW,R0
    STOREI RY, RZ, -2
    LIMM R0,loop_4
    JMP R0
wend_6:
    LOADI RW, RZ, -3
    PUSH RW
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -3
    LIMM R0,loop_1
    JMP R0
wend_3:
    LI RW,0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    PUSH RW
    LI RY,1
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RW,R0,RY
    PUSH RW
    LI RY,2
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RX,R0,RY
    PUSH RX
    LI RY,3
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RW,R0,RY
    PUSH RW
    LI RY,4
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RX,R0,RY
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
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
