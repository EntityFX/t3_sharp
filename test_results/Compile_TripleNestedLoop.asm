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
    SUBI SP, SP, 4
    LI RW,0
    STOREI RW, RZ, -4
    LI RX,1
    STOREI RX, RZ, -3
loop_1:
    LOADI RY, RZ, -3
    LI R0,4
    CMP RY,R0
    LIMM RW,body_2
    JLE RW
    LIMM RX,wend_3
    JMP RX
body_2:
    LI RY,1
    STOREI RY, RZ, -2
loop_4:
    LOADI R0, RZ, -2
    LI RW,4
    CMP R0,RW
    LIMM RX,body_5
    JLE RX
    LIMM RY,wend_6
    JMP RY
body_5:
    LI R0,1
    STOREI R0, RZ, -1
loop_7:
    LOADI RW, RZ, -1
    LI RX,4
    CMP RW,RX
    LIMM RY,body_8
    JLE RY
    LIMM R0,wend_9
    JMP R0
body_8:
    LOADI RW, RZ, -4
    PUSH RW
    LOADI RX, RZ, -3
    PUSH RX
    LOADI RY, RZ, -2
    POP R0
    MUL RW,R0,RY
    PUSH RW
    LOADI RY, RZ, -1
    POP R0
    MUL RX,R0,RY
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -4
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP R0
    ADD RW,R0,RY
    STOREI RW, RZ, -1
    LIMM RY,loop_7
    JMP RY
wend_9:
    LOADI R0, RZ, -2
    PUSH R0
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -2
    LIMM RX,loop_4
    JMP RX
wend_6:
    LOADI RY, RZ, -3
    PUSH RY
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -3
    LIMM RW,loop_1
    JMP RW
wend_3:
    LOADI RX, RZ, -4
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
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
