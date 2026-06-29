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
    SUBI SP, SP, 4
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,1
    STOREI RX, RZ, -4
loop_1:
    LOADI RY, RZ, -4
    LI R0,4
    CMP RY,R0
    LIMM R3,body_2
    JLE R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LI RX,1
    STOREI RX, RZ, -5
loop_4:
    LOADI RY, RZ, -5
    LI R0,4
    CMP RY,R0
    LIMM R3,body_5
    JLE R3
    LIMM RW,wend_6
    JMP RW
body_5:
    LI RX,1
    STOREI RX, RZ, -6
loop_7:
    LOADI RY, RZ, -6
    LI R0,4
    CMP RY,R0
    LIMM R3,body_8
    JLE R3
    LIMM RW,wend_9
    JMP RW
body_8:
    LOADI RX, RZ, -3
    PUSH RX
    LOADI RY, RZ, -4
    PUSH RY
    LOADI R0, RZ, -5
    POP R3
    MUL RW,R3,R0
    PUSH RW
    LOADI R0, RZ, -6
    POP R3
    MUL RX,R3,R0
    POP R0
    ADD R3,R0,RX
    STOREI R3, RZ, -3
    LOADI RX, RZ, -6
    PUSH RX
    LI R0,1
    POP RY
    ADD R3,RY,R0
    STOREI R3, RZ, -6
    LIMM R0,loop_7
    JMP R0
wend_9:
    LOADI RY, RZ, -5
    PUSH RY
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -5
    LIMM RW,loop_4
    JMP RW
wend_6:
    LOADI RX, RZ, -4
    PUSH RX
    LI R0,1
    POP R3
    ADD RW,R3,R0
    STOREI RW, RZ, -4
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI R3, RZ, -3
    MOV R2,R3
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 4
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
