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
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    PUSH RX
    LI RY,1
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RX,R4, 0
    POP RY
    ADD R0,RY,RX
    PUSH R0
    LI RY,2
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RX,R4, 0
    POP RY
    ADD R3,RY,RX
    PUSH R3
    LI RY,3
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RX,R4, 0
    POP RY
    ADD RW,RY,RX
    PUSH RW
    LI RY,4
    LI R4, 1
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RX,R4, 0
    POP RY
    ADD R0,RY,RX
    MOV R2,R0
    LIMM RX,epilogue_0
    JMP RX
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
