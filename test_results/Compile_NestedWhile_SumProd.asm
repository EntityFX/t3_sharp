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
    SUBI SP, SP, 3
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,1
    STOREI RX, RZ, -2
loop_1:
    LOADI RY, RZ, -2
    LI R0,3
    CMP RY,R0
    LIMM RW,body_2
    JLE RW
    LIMM RX,wend_3
    JMP RX
body_2:
    LI RY,1
    STOREI RY, RZ, -1
loop_4:
    LOADI R0, RZ, -1
    LI RW,3
    CMP R0,RW
    LIMM RX,body_5
    JLE RX
    LIMM RY,wend_6
    JMP RY
body_5:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI RW, RZ, -2
    PUSH RW
    LOADI RX, RZ, -1
    POP RY
    MUL R0,RY,RX
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -3
    LOADI R0, RZ, -1
    PUSH R0
    LI RX,1
    POP RW
    ADD RY,RW,RX
    STOREI RY, RZ, -1
    LIMM RX,loop_4
    JMP RX
wend_6:
    LOADI RW, RZ, -2
    PUSH RW
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -2
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI RW, RZ, -3
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
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
