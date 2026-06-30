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
    SUBI SP, SP, 2
    LI RW,0
    STOREI RW, RZ, -2
    LI RX,1
    STOREI RX, RZ, -1
floop_1:
    LOADI RY, RZ, -1
    LI R0,10
    CMP RY,R0
    LIMM RW,fbody_2
    JLE RW
    LIMM RX,fend_3
    JMP RX
fbody_2:
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R0, RZ, -1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -2
    LOADI R0, RZ, -1
    PUSH R0
    LI RW,1
    POP RY
    ADD R0,RY,RW
    STOREI R0, RZ, -1
    LIMM RW,floop_1
    JMP RW
fend_3:
    LOADI RY, RZ, -2
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
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
