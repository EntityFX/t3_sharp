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
    SUBI SP, SP, 3
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
    LOADI R0, RZ, -4
    CMP RY,R0
    LIMM R3,body_5
    JLE R3
    LIMM RW,wend_6
    JMP RW
body_5:
    LOADI RX, RZ, -3
    PUSH RX
    LOADI RY, RZ, -5
    POP R0
    ADD R3,R0,RY
    STOREI R3, RZ, -3
    LOADI RY, RZ, -5
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -5
    LIMM R0,loop_4
    JMP R0
wend_6:
    LOADI RW, RZ, -4
    PUSH RW
    LI RY,1
    POP R0
    ADD R3,R0,RY
    STOREI R3, RZ, -4
    LIMM RY,loop_1
    JMP RY
wend_3:
    LOADI R0, RZ, -3
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 3
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
