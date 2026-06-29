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
    LI RW,10
    STOREI RW, RZ, -3
    LI RX,0
    STOREI RX, RZ, -4
    LI RY,1
    STOREI RY, RZ, -5
    LI R0,0
    STOREI R0, RZ, -6
loop_1:
    LOADI R3, RZ, -6
    LOADI RW, RZ, -3
    CMP R3,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LOADI R0, RZ, -4
    PUSH R0
    LOADI R3, RZ, -5
    POP RW
    ADD RX,RW,R3
    STOREI RX, RZ, -7
    LOADI R3, RZ, -5
    STOREI R3, RZ, -4
    LOADI RW, RZ, -7
    STOREI RW, RZ, -5
    LOADI RY, RZ, -6
    PUSH RY
    LI R0,1
    POP R3
    ADD RW,R3,R0
    STOREI RW, RZ, -6
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI R3, RZ, -4
    MOV R2,R3
    LIMM RX,epilogue_0
    JMP RX
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
