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
    LI RW,10
    STOREI RW, RZ, -5
    LI RX,0
    STOREI RX, RZ, -4
    LI RY,1
    STOREI RY, RZ, -3
    LI R0,0
    STOREI R0, RZ, -2
loop_1:
    LOADI RW, RZ, -2
    LOADI RX, RZ, -5
    CMP RW,RX
    LIMM RY,body_2
    JL RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI RW, RZ, -4
    PUSH RW
    LOADI RX, RZ, -3
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -1
    LOADI RX, RZ, -3
    STOREI RX, RZ, -4
    LOADI RY, RZ, -1
    STOREI RY, RZ, -3
    LOADI RW, RZ, -2
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -2
    LIMM RX,loop_1
    JMP RX
wend_3:
    LOADI RY, RZ, -4
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
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
