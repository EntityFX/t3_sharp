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
    LOADI RY, RZ, -3
    PUSH RY
    LOADI R0, RZ, -4
    POP R3
    ADD RW,R3,R0
    STOREI RW, RZ, -3
    LOADI R0, RZ, -4
    PUSH R0
    LI R3,1
    POP RX
    ADD RY,RX,R3
    STOREI RY, RZ, -4
    LOADI R3, RZ, -4
    LI RX,10
    CMP R3,RX
    LIMM R0,loop_1
    JLE R0
wend_2:
    LOADI R3, RZ, -3
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
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
