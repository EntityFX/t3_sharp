; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
factorial_goto:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RZ, -2, RW
    LI RW,1
    STOREI RZ, -1, RW
__glbl_loop_start:
    LOADI RX, RZ, -2
    LI RY,1
    CMP RX,RY
    LIMM R0,then_2
    JLE R0
    LIMM R3,end_1
    JMP R3
then_2:
    LIMM RW,__glbl_done
    JMP RW
end_1:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -2
    POP R0
    MUL R3,R0,RY
    STOREI RZ, -1, R3
    LOADI RY, RZ, -2
    PUSH RY
    LI R0,1
    POP RW
    SUB RX,RW,R0
    STOREI RZ, -2, RX
    LIMM R0,__glbl_loop_start
    JMP R0
__glbl_done:
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 9
    LI RW,1
    STOREI RZ, -1, RW
    LOADI RX, RZ, -1
    LI RY,1
    CMP RX,RY
    LIMM R0,then_5
    JE R0
    LIMM R3,end_4
    JMP R3
then_5:
    LI RW,2
    STOREI RZ, -1, RW
end_4:
    LI RX,42
    LI R4, 2
    SUB R4, RZ, R4
    STOREI R4, 0, RX
    LI R4, 2
    SUB R4, RZ, R4
    LOADI RY,R4, 0
    STOREI RZ, -4, RY
    LI R0,5
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R0
    LIMM R1,factorial_goto
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R3,R2
    STOREI RZ, -5, R3
    LI RW,0
    STOREI RZ, -6, RW
    LI RX,0
    STOREI RZ, -7, RX
loop_6:
    LOADI RY, RZ, -7
    LI R0,5
    CMP RY,R0
    LIMM R3,then_9
    JGE R3
    LIMM RW,end_8
    JMP RW
then_9:
    LIMM RX,wend_7
    JMP RX
end_8:
    LOADI RY, RZ, -6
    PUSH RY
    LOADI R0, RZ, -7
    POP R3
    ADD RW,R3,R0
    STOREI RZ, -6, RW
    LOADI R0, RZ, -7
    PUSH R0
    LI R3,1
    POP RX
    ADD RY,RX,R3
    STOREI RZ, -7, RY
    LI R3,1
    LI R2,0
    CMP R3,R2
    LIMM RX,loop_6
    JNE RX
wend_7:
    LI R0,10
    PUSH R0
    LI R3,0
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI R4, 0, RW
    LI RX,20
    PUSH RX
    LI RY,0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R4, 0, R0
    LI R3,30
    PUSH R3
    LI RW,1
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI R4, 0, RX
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -4
    POP R3
    ADD RW,R3,R0
    PUSH RW
    LOADI R0, RZ, -5
    POP R3
    ADD RX,R3,R0
    PUSH RX
    LOADI R0, RZ, -6
    POP R3
    ADD RY,R3,R0
    PUSH RY
    LI R0,0
    LI R4, 8
    SUB R4, RZ, R4
    ADD R3,R4,R0
    LOADI R3,R3, 0
    POP R0
    ADD RW,R0,R3
    PUSH RW
    LI R3,0
    LI R4, 9
    SUB R4, RZ, R4
    ADD R0,R4,R3
    LOADI R0,R0, 0
    POP RX
    ADD RY,RX,R0
    PUSH RY
    LI R0,1
    LI R4, 8
    SUB R4, RZ, R4
    ADD RX,R4,R0
    LOADI RX,RX, 0
    POP R0
    ADD R3,R0,RX
    MOV R2,R3
    LIMM RX,epilogue_3
    JMP RX
epilogue_3:
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
