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
    SUBI SP, SP, 4
    LI RW,10
    LI R4, 1
    SUB R4, RZ, R4
    STOREI R4, 0, RW
    LI RX,15
    LI R4, 2
    SUB R4, RZ, R4
    STOREI R4, 0, RX
    LI RY, 1
    SUB RY, RZ, RY
    STOREI RZ, -3, RY
    LOADI R0, RZ, -3
    LI R3,0
    ADD RW,R0,R3
    LOADI RW,RW, 0
    PUSH RW
    LOADI R3, RZ, -3
    LI RX,1
    ADD RY,R3,RX
    LOADI RY,RY, 0
    POP RX
    ADD R0,RX,RY
    MOV R2,R0
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 4
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
