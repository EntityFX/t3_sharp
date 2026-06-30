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
    SUBI SP, SP, 11
    LI RW,0
    STOREI RW, RZ, -2
loop_1:
    LOADI RX, RZ, -2
    LI RY,9
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -2
    PUSH RX
    LI RY,1
    POP R0
    ADD RW,R0,RY
    PUSH RW
    LOADI RY, RZ, -2
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LOADI RY, RZ, -2
    PUSH RY
    LI R0,1
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -2
    LIMM R0,loop_1
    JMP R0
wend_3:
    LI RX,0
    STOREI RX, RZ, -1
    LI R0,0
    STOREI R0, RZ, -2
loop_4:
    LOADI RW, RZ, -2
    LI RX,9
    CMP RW,RX
    LIMM RY,body_5
    JL RY
    LIMM R0,wend_6
    JMP R0
body_5:
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -2
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -1
    LOADI RX, RZ, -2
    PUSH RX
    LI RY,1
    POP RW
    ADD RX,RW,RY
    STOREI RX, RZ, -2
    LIMM RY,loop_4
    JMP RY
wend_6:
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 11
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
