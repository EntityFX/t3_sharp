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
    SUBI SP, SP, 5
    LI RW,0
    STOREI RZ, -1, RW
    LI RX,1
    STOREI RZ, -2, RX
    LI RY,1
    STOREI RZ, -3, RY
    LI R0,1
    STOREI RZ, -4, R0
    LI R3,0
    STOREI RZ, -5, R3
loop_1:
    LOADI RW, RZ, -5
    LI RX,36
    CMP RW,RX
    LIMM RY,body_2
    JLE RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI R3, RZ, -2
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RZ, -2, RY
    LOADI RW, RZ, -3
    PUSH RW
    LI RX,2
    POP R0
    ADD R3,R0,RX
    STOREI RZ, -3, R3
    LOADI RX, RZ, -4
    PUSH RX
    LI R0,3
    POP RW
    ADD RX,RW,R0
    STOREI RZ, -4, RX
    LOADI R0, RZ, -5
    PUSH R0
    LI RW,1
    POP RY
    ADD R0,RY,RW
    STOREI RZ, -5, R0
    LIMM RW,loop_1
    JMP RW
wend_3:
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R3, RZ, -3
    POP RW
    ADD RX,RW,R3
    PUSH RX
    LOADI R3, RZ, -4
    POP RW
    ADD RY,RW,R3
    STOREI RZ, -1, RY
    LOADI R3, RZ, -1
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
