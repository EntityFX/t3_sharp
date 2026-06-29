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
    LI RW,1
    STOREI RZ, -1, RW
    LI RX,1
    STOREI RZ, -2, RX
loop_1:
    LOADI RY, RZ, -2
    LI R0,5
    CMP RY,R0
    LIMM R3,body_2
    JLE R3
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -2
    POP R0
    MUL R3,R0,RY
    STOREI RZ, -1, R3
    LOADI RY, RZ, -2
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RZ, -2, RX
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI RW, RZ, -1
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
