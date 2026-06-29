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
    STOREI RZ, -1, RW
    LI RX,1
    STOREI RZ, -2, RX
loop_1:
    LOADI RY, RZ, -2
    LI R0,3
    CMP RY,R0
    LIMM R3,body_2
    JLE R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LI RX,1
    STOREI RZ, -3, RX
loop_4:
    LOADI RY, RZ, -3
    LI R0,3
    CMP RY,R0
    LIMM R3,body_5
    JLE R3
    LIMM RW,wend_6
    JMP RW
body_5:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R0, RZ, -3
    POP R3
    MUL RW,R3,R0
    POP R0
    ADD R3,R0,RW
    STOREI RZ, -1, R3
    LOADI RW, RZ, -3
    PUSH RW
    LI R0,1
    POP RX
    ADD RY,RX,R0
    STOREI RZ, -3, RY
    LIMM R0,loop_4
    JMP R0
wend_6:
    LOADI RX, RZ, -2
    PUSH RX
    LI R0,1
    POP R3
    ADD RW,R3,R0
    STOREI RZ, -2, RW
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI R3, RZ, -1
    MOV R2,R3
    LIMM RX,epilogue_0
    JMP RX
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
