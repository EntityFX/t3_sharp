; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
fib:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 5
    STOREI RW, RZ, -7
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,1
    STOREI RX, RZ, -4
    LI RY,0
    STOREI RY, RZ, -5
    LI R0,0
    STOREI R0, RZ, -6
loop_1:
    LOADI R3, RZ, -6
    LOADI RW, RZ, -7
    CMP R3,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI R3, RZ, -4
    POP RW
    ADD RX,RW,R3
    STOREI RX, RZ, -5
    LOADI R3, RZ, -4
    STOREI R3, RZ, -3
    LOADI RW, RZ, -5
    STOREI RW, RZ, -4
    LOADI RY, RZ, -6
    PUSH RY
    LI R0,1
    POP R3
    ADD RW,R3,R0
    STOREI RW, RZ, -6
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI R3, RZ, -3
    MOV R2,R3
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 5
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    LI RW,10
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    MOV R2,RX
    LIMM RY,epilogue_4
    JMP RY
epilogue_4:
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
