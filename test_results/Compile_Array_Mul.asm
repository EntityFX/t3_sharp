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
    SUBI SP, SP, 7
    LI RW,1
    LI RX,0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RY
    STOREI R4, 0, RY
    LI RY,2
    LI RX,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,3
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
    LI RW,5
    LI RX,4
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RX
    STOREI R4, 0, RX
    LI RX,0
    STOREI RZ, -6, RX
    LI RX,1
    STOREI RZ, -7, RX
loop_1:
    LOADI RY, RZ, -6
    LI R0,5
    CMP RY,R0
    LIMM R3,body_2
    JL R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -7
    PUSH RX
    LOADI R0, RZ, -6
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI RY,R4, 0
    POP R0
    MUL R3,R0,RY
    STOREI RZ, -7, R3
    LOADI RY, RZ, -6
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RZ, -6, RX
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI RW, RZ, -7
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 7
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
