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
    LI RX,6
    LI RX,5
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RX
    POP RY
    STOREI R4, 0, RY
    LI RY,7
    LI RX,6
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,8
    LI RX,7
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LI R3,9
    LI RX,8
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R3
    POP RW
    STOREI R4, 0, RW
    LI RW,9
    LI RX,0
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RX
    STOREI R4, 0, RX
    LI RX,8
    LI RX,1
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RX
    POP RY
    STOREI R4, 0, RY
    LI RY,7
    LI RX,2
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,6
    LI RX,3
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LI R3,5
    LI RX,4
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R3
    POP RW
    STOREI R4, 0, RW
    LI RW,4
    LI RX,5
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RW
    POP RX
    STOREI R4, 0, RX
    LI RX,3
    LI RX,6
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RX
    POP RY
    STOREI R4, 0, RY
    LI RY,2
    LI RX,7
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH RY
    POP R0
    STOREI R4, 0, R0
    LI R0,1
    LI RX,8
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RX
    PUSH R0
    POP R3
    STOREI R4, 0, R3
    LI R3,0
    LI RX,-19
    ADD RW, RZ, RX
    STOREI RW, 0, R3
    LI RW,0
    LI RX,-20
    ADD RX, RZ, RX
    STOREI RX, 0, RW
loop_1:
    LI RX,-20
    ADD RY, RZ, RX
    LOADI RX, RY, 0
    LI RY,3
    CMP RX,RY
    LIMM RX,body_2
    JL RX
    LIMM R0,wend_3
    JMP R0
body_2:
    LI RW,-19
    ADD RX, RZ, RW
    LOADI R3, RX, 0
    PUSH R3
    LI RW,0
    PUSH RW
    LI R0,-20
    ADD R3, RZ, R0
    LOADI RY, R3, 0
    POP R3
    ADD R0,R3,RY
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI RX,R4, 0
    PUSH RX
    LI R3,-20
    ADD RW, RZ, R3
    LOADI RY, RW, 0
    PUSH RY
    LI RW,3
    POP R3
    MUL RX,R3,RW
    PUSH RX
    LI RW,0
    POP R3
    ADD RY,R3,RW
    LI R4, 10
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI R0,R4, 0
    POP RY
    MUL RW,RY,R0
    POP R0
    ADD RY,R0,RW
    LI RW,-19
    ADD R0, RZ, RW
    STOREI R0, 0, RY
    LI RW,-20
    ADD R3, RZ, RW
    LOADI R0, R3, 0
    PUSH R0
    LI R3,1
    POP RW
    ADD R0,RW,R3
    LI R3,-20
    ADD RW, RZ, R3
    STOREI RW, 0, R0
    LIMM RW,loop_1
    JMP RW
wend_3:
    LI R3,-19
    ADD RW, RZ, R3
    LOADI R3, RW, 0
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
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
