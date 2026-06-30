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
    STOREI RW, RZ, -5
    LI RX,1
    STOREI RX, RZ, -4
    LI RY,1
    STOREI RY, RZ, -3
    LI R0,1
    STOREI R0, RZ, -2
    LI RW,0
    STOREI RW, RZ, -1
loop_1:
    LOADI RX, RZ, -1
    LI RY,36
    CMP RX,RY
    LIMM R0,body_2
    JLE R0
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -4
    PUSH RX
    LI RY,1
    POP R0
    ADD RW,R0,RY
    STOREI RW, RZ, -4
    LOADI RY, RZ, -3
    PUSH RY
    LI R0,2
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -3
    LOADI R0, RZ, -2
    PUSH R0
    LI RX,3
    POP R0
    ADD RW,R0,RX
    STOREI RW, RZ, -2
    LOADI RX, RZ, -1
    PUSH RX
    LI R0,1
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -1
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI RX, RZ, -4
    PUSH RX
    LOADI R0, RZ, -3
    POP RW
    ADD RX,RW,R0
    PUSH RX
    LOADI R0, RZ, -2
    POP RW
    ADD RY,RW,R0
    STOREI RY, RZ, -5
    LOADI R0, RZ, -5
    MOV R2,R0
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
