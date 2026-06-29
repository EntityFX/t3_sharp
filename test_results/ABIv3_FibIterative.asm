; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
fib:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 5
    STOREI RZ, -5, RW
    LI RW,0
    STOREI RZ, -1, RW
    LI RX,1
    STOREI RZ, -2, RX
    LI RY,0
    STOREI RZ, -3, RY
    LI R0,0
    STOREI RZ, -4, R0
loop_1:
    LOADI R3, RZ, -4
    LOADI RW, RZ, -5
    CMP R3,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LOADI R0, RZ, -1
    PUSH R0
    LOADI R3, RZ, -2
    POP RW
    ADD RX,RW,R3
    STOREI RZ, -3, RX
    LOADI R3, RZ, -2
    STOREI RZ, -1, R3
    LOADI RW, RZ, -3
    STOREI RZ, -2, RW
    LOADI RY, RZ, -4
    PUSH RY
    LI R0,1
    POP R3
    ADD RW,R3,R0
    STOREI RZ, -4, RW
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI R3, RZ, -1
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
    PUSH R3
    PUSH R4
    MOV RZ, SP
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
