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
    SUBI SP, SP, 2
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,1
    STOREI RX, RZ, -4
floop_1:
    LOADI RY, RZ, -4
    LI R0,10
    CMP RY,R0
    LIMM R3,fbody_2
    JLE R3
    LIMM RW,fend_3
    JMP RW
fbody_2:
    LOADI RX, RZ, -3
    PUSH RX
    LOADI RY, RZ, -4
    POP R0
    ADD R3,R0,RY
    STOREI R3, RZ, -3
    LOADI RY, RZ, -4
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -4
    LIMM R0,floop_1
    JMP R0
fend_3:
    LOADI RW, RZ, -3
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 2
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
