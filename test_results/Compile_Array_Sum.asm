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
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,2
    PUSH RX
    LI RY,1
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,3
    PUSH RY
    LI R0,2
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,4
    PUSH R0
    LI RW,3
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,5
    PUSH RW
    LI RX,4
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,0
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    PUSH RX
    LI RY,1
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RW,R0,RY
    PUSH RW
    LI RY,2
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RX,R0,RY
    PUSH RX
    LI RY,3
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RW,R0,RY
    PUSH RW
    LI RY,4
    LI R4, 5
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP R0
    ADD RX,R0,RY
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
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
