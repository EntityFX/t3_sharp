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
    SUBI SP, SP, 7
    LI RW,1
    LI RX,0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RY
    STOREI R4, 0, RY
    LI RY,2
    LI RX,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,3
    LI RX,2
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LI R3,4
    LI RX,3
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R3
    POP RW
    STOREI R4, 0, RW
    LI RW,5
    LI RX,4
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RX
    STOREI R4, 0, RX
    LI RX,0
    STOREI RZ, -6, RX
loop_1:
    LOADI RX, RZ, -6
    LI RY,2
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM R3,wend_3
    JMP R3
body_2:
    LOADI RX, RZ, -6
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RW,R4, 0
    STOREI RZ, -7, RW
    LI RY,4
    PUSH RY
    LOADI R0, RZ, -6
    POP R3
    SUB RW,R3,R0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RX,R4, 0
    LOADI RW, RZ, -6
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    PUSH RX
    POP R0
    STOREI R4, 0, R0
    LOADI R0, RZ, -7
    LI RW,4
    PUSH RW
    LOADI R3, RZ, -6
    POP RX
    SUB RY,RX,R3
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LOADI R3, RZ, -6
    PUSH R3
    LI RY,1
    POP RX
    ADD R0,RX,RY
    STOREI RZ, -6, R0
    LIMM RY,loop_1
    JMP RY
wend_3:
    LI R3,0
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R3
    LOADI RX,R4, 0
    PUSH RX
    LI RW,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RX,RW,R3
    PUSH RX
    LI RW,2
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RY,RW,R3
    PUSH RY
    LI RW,3
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD R0,RW,R3
    PUSH R0
    LI RW,4
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI R3,R4, 0
    POP RW
    ADD RX,RW,R3
    MOV R2,RX
    LIMM R3,epilogue_0
    JMP R3
epilogue_0:
    ADDI SP, SP, 7
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
