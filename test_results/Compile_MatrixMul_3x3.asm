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
    SUBI SP, SP, 20
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,2
    PUSH R0
    LI R3,1
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,3
    PUSH RX
    LI RY,2
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0,R4, 0
    LI R3,4
    PUSH R3
    LI RW,3
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX,R4, 0
    LI RY,5
    PUSH RY
    LI R0,4
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,6
    PUSH RW
    LI RX,5
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,7
    PUSH R0
    LI R3,6
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,8
    PUSH RX
    LI RY,7
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0,R4, 0
    LI R3,9
    PUSH R3
    LI RW,8
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX,R4, 0
    LI RY,9
    PUSH RY
    LI R0,0
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,8
    PUSH RW
    LI RX,1
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,7
    PUSH R0
    LI R3,2
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,6
    PUSH RX
    LI RY,3
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0,R4, 0
    LI R3,5
    PUSH R3
    LI RW,4
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX,R4, 0
    LI RY,4
    PUSH RY
    LI R0,5
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,R0
    POP R3
    STOREI R3,R4, 0
    LI RW,3
    PUSH RW
    LI RX,6
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY,R4, 0
    LI R0,2
    PUSH R0
    LI R3,7
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,R3
    POP RW
    STOREI RW,R4, 0
    LI RX,1
    PUSH RX
    LI RY,8
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0,R4, 0
    LI R3,0
    LI RW,-21
    ADD RX, RZ, RW
    STOREI R3, RX, 0
    LI RX,0
    LI RW,-22
    ADD RY, RZ, RW
    STOREI RX, RY, 0
loop_1:
    LI RW,-22
    ADD R0, RZ, RW
    LOADI RY, R0, 0
    LI R0,3
    CMP RY,R0
    LIMM RW,body_2
    JL RW
    LIMM R3,wend_3
    JMP R3
body_2:
    LI RX,-21
    ADD RY, RZ, RX
    LOADI RW, RY, 0
    PUSH RW
    LI RY,0
    PUSH RY
    LI R0,-22
    ADD R3, RZ, R0
    LOADI RX, R3, 0
    POP R3
    ADD R0,R3,RX
    LI R4, -3
    ADD R4, RZ, R4
    ADD R4,R4,R0
    LOADI RX,R4, 0
    PUSH RX
    LI RW,-22
    ADD RX, RZ, RW
    LOADI R3, RX, 0
    PUSH R3
    LI RX,3
    POP RW
    MUL RY,RW,RX
    PUSH RY
    LI RX,0
    POP RW
    ADD R0,RW,RX
    LI R4, -12
    ADD R4, RZ, R4
    ADD R4,R4,R0
    LOADI RX,R4, 0
    POP RW
    MUL R3,RW,RX
    POP RX
    ADD RW,RX,R3
    LI R3,-21
    ADD RX, RZ, R3
    STOREI RW, RX, 0
    LI R3,-22
    ADD RW, RZ, R3
    LOADI RX, RW, 0
    PUSH RX
    LI RW,1
    POP R3
    ADD RX,R3,RW
    LI RW,-22
    ADD R3, RZ, RW
    STOREI RX, R3, 0
    LIMM R3,loop_1
    JMP R3
wend_3:
    LI RY,-21
    ADD R0, RZ, RY
    LOADI RW, R0, 0
    MOV R2,RW
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
