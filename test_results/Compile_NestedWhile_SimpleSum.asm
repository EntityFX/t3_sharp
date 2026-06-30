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
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -3
    LOADI RW, RZ, -1
    PUSH RW
    LI RX,1
    POP R0
    ADD RW,R0,RX
    STOREI RW, RZ, -1
    LIMM RX,loop_4
    JMP RX
wend_6:
    LOADI R0, RZ, -2
    PUSH R0
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -2
    LIMM RX,loop_1
    JMP RX
wend_3:
    LOADI RY, RZ, -3
    MOV R2,RY
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
