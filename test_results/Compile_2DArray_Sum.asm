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
    STOREI RZ, -10, RW
loop_1:
    LOADI RX, RZ, -10
    LI RY,9
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM R3,wend_3
    JMP R3
body_2:
    LOADI RW, RZ, -10
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    LOADI RX, RZ, -10
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP RY
    STOREI R4, 0, RY
    LOADI RY, RZ, -10
    PUSH RY
    LI RX,1
    POP R3
    ADD RW,R3,RX
    STOREI RZ, -10, RW
    LIMM RX,loop_1
    JMP RX
wend_3:
    LI R3,0
    STOREI RZ, -11, R3
    LI RX,0
    STOREI RZ, -10, RX
loop_4:
    LOADI RY, RZ, -10
    LI R0,9
    CMP RY,R0
    LIMM R3,body_5
    JL R3
    LIMM RW,wend_6
    JMP RW
body_5:
    LOADI RX, RZ, -11
    PUSH RX
    LOADI R0, RZ, -10
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI RY,R4, 0
    POP R0
    ADD R3,R0,RY
    STOREI RZ, -11, R3
    LOADI RY, RZ, -10
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RZ, -10, RX
    LIMM R0,loop_4
    JMP R0
wend_6:
    LOADI RW, RZ, -11
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
