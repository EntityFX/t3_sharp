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
    SUBI SP, SP, 6
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,2
    PUSH RX
    LI RY,1
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,0
    LI R0, 6
    SUB R0, RZ, R0
    ADD R0,R0,RY
    STOREI R0, RZ, -1
    LOADI RW, RZ, -1
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -1
    LOADI RX, RZ, -1
    LOADI RY,RX, 0
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 6
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
