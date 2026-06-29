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
    SUBI SP, SP, 5
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,1
    STOREI RX, RZ, -4
    LI RY,1
    STOREI RY, RZ, -5
    LI R0,1
    STOREI R0, RZ, -6
    LI R3,0
    STOREI R3, RZ, -7
loop_1:
    LOADI RW, RZ, -7
    LI RX,36
    CMP RW,RX
    LIMM RY,body_2
    JLE RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI R3, RZ, -4
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -4
    LOADI RW, RZ, -5
    PUSH RW
    LI RX,2
    POP R0
    ADD R3,R0,RX
    STOREI R3, RZ, -5
    LOADI RX, RZ, -6
    PUSH RX
    LI R0,3
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -6
    LOADI R0, RZ, -7
    PUSH R0
    LI RW,1
    POP RY
    ADD R0,RY,RW
    STOREI R0, RZ, -7
    LIMM RW,loop_1
    JMP RW
wend_3:
    LOADI RY, RZ, -4
    PUSH RY
    LOADI R3, RZ, -5
    POP RW
    ADD RX,RW,R3
    PUSH RX
    LOADI R3, RZ, -6
    POP RW
    ADD RY,RW,R3
    STOREI RY, RZ, -3
    LOADI R3, RZ, -3
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 5
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
