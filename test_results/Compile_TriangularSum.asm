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
    LI R0,4
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
    LOADI R0, RZ, -2
    CMP RY,R0
    LIMM R3,body_5
    JLE R3
    LIMM RW,wend_6
    JMP RW
body_5:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -3
    POP R0
    ADD R3,R0,RY
    STOREI RZ, -1, R3
    LOADI RY, RZ, -3
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RZ, -3, RX
    LIMM R0,loop_4
    JMP R0
wend_6:
    LOADI RW, RZ, -2
    PUSH RW
    LI RY,1
    POP R0
    ADD R3,R0,RY
    STOREI RZ, -2, R3
    LIMM RY,loop_1
    JMP RY
wend_3:
    LOADI R0, RZ, -1
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
