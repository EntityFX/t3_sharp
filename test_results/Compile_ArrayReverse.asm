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
    SUBI SP, SP, 7
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,2
    PUSH R0
    LI R3,1
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,3
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
    LI RY,5
    PUSH RY
    LI R0,4
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,0
    STOREI RW, RZ, -8
loop_1:
    LOADI RX, RZ, -8
    LI RY,2
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM R3,wend_3
    JMP R3
body_2:
    LOADI RW, RZ, -8
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    LOADI RX,R4, 0
    STOREI RX, RZ, -9
    LI RY,4
    PUSH RY
    LOADI R0, RZ, -8
    POP R3
    SUB RW,R3,R0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    LOADI R0,R4, 0
    PUSH R0
    LOADI R3, RZ, -8
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RX
    STOREI RX,R4, 0
    LOADI RY, RZ, -9
    PUSH RY
    LI R0,4
    PUSH R0
    LOADI R3, RZ, -8
    POP RW
    SUB RX,RW,R3
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP R3
    STOREI R3,R4, 0
    LOADI RW, RZ, -8
    PUSH RW
    LI RY,1
    POP R0
    ADD R3,R0,RY
    STOREI R3, RZ, -8
    LIMM RY,loop_1
    JMP RY
wend_3:
    LI R0,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    LOADI RW,R4, 0
    PUSH RW
    LI RX,1
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    LOADI RY,R4, 0
    POP R0
    ADD R3,R0,RY
    PUSH R3
    LI RY,2
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    LOADI R0,R4, 0
    POP RW
    ADD RX,RW,R0
    PUSH RX
    LI R0,3
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    LOADI RW,R4, 0
    POP RY
    ADD R0,RY,RW
    PUSH R0
    LI RW,4
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    LOADI RY,R4, 0
    POP R3
    ADD RW,R3,RY
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
