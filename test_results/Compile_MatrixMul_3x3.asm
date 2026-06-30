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
    SUBI SP, SP, 20
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,2
    PUSH RX
    LI RY,1
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,3
    PUSH RY
    LI R0,2
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,4
    PUSH R0
    LI RW,3
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,5
    PUSH RW
    LI RX,4
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,6
    PUSH RX
    LI RY,5
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,7
    PUSH RY
    LI R0,6
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,8
    PUSH R0
    LI RW,7
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,9
    PUSH RW
    LI RX,8
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,9
    PUSH RX
    LI RY,0
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,8
    PUSH RY
    LI R0,1
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,7
    PUSH R0
    LI RW,2
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,6
    PUSH RW
    LI RX,3
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,5
    PUSH RX
    LI RY,4
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,4
    PUSH RY
    LI R0,5
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,3
    PUSH R0
    LI RW,6
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,2
    PUSH RW
    LI RX,7
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,1
    PUSH RX
    LI RY,8
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,0
    STOREI RY, RZ, -2
    LI R0,0
    STOREI R0, RZ, -1
loop_1:
    LOADI RW, RZ, -1
    LI RX,3
    CMP RW,RX
    LIMM RY,body_2
    JL RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI RW, RZ, -2
    PUSH RW
    LI RX,0
    PUSH RX
    LOADI RY, RZ, -1
    POP R0
    ADD RW,R0,RY
    LI R4, 20
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    PUSH RW
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,3
    POP RX
    MUL RY,RX,R0
    PUSH RY
    LI R0,0
    POP RX
    ADD RW,RX,R0
    LI R4, 11
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP R0
    MUL RX,R0,RW
    POP RW
    ADD R0,RW,RX
    STOREI R0, RZ, -2
    LOADI RX, RZ, -1
    PUSH RX
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -1
    LIMM RW,loop_1
    JMP RW
wend_3:
    LOADI RX, RZ, -2
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    ADDI SP, SP, 20
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
