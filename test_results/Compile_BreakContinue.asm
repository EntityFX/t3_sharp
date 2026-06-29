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
    SUBI SP, SP, 2
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,1
    STOREI RX, RZ, -4
loop_1:
    LOADI RY, RZ, -4
    LI R0,10
    CMP RY,R0
    LIMM R3,body_2
    JLE R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -4
    LI RY,5
    CMP RX,RY
    LIMM R0,then_5
    JE R0
    LIMM R3,end_4
    JMP R3
then_5:
    LOADI RW, RZ, -4
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -4
    LIMM RX,loop_1
    JMP RX
end_4:
    LOADI RY, RZ, -4
    LI R3,8
    CMP RY,R3
    LIMM RW,then_7
    JG RW
    LIMM RX,end_6
    JMP RX
then_7:
    LIMM RY,wend_3
    JMP RY
end_6:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI R3, RZ, -4
    POP RW
    ADD RX,RW,R3
    STOREI RX, RZ, -3
    LOADI R3, RZ, -4
    PUSH R3
    LI RW,1
    POP RY
    ADD R0,RY,RW
    STOREI R0, RZ, -4
    LIMM RW,loop_1
    JMP RW
wend_3:
    LOADI RY, RZ, -3
    MOV R2,RY
    LIMM R3,epilogue_0
    JMP R3
epilogue_0:
    ADDI SP, SP, 2
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
