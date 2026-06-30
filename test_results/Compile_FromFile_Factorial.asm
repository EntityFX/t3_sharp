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
    LI RW,5
    STOREI RW, RZ, -2
    LI RX,1
    STOREI RX, RZ, -1
loop_1:
    LOADI RY, RZ, -2
    LI R0,1
    CMP RY,R0
    LIMM RW,body_2
    JG RW
    LIMM RX,wend_3
    JMP RX
body_2:
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -2
    POP RW
    MUL RX,RW,R0
    STOREI RX, RZ, -1
    LOADI R0, RZ, -2
    PUSH R0
    LI RW,1
    POP RY
    SUB R0,RY,RW
    STOREI R0, RZ, -2
    LIMM RW,loop_1
    JMP RW
wend_3:
    LOADI RY, RZ, -1
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
