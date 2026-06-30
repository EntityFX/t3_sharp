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
    STOREI RW, RZ, -1
    LI RW,0
    STOREI RW, RZ, -5
    LI RX,1
    STOREI RX, RZ, -4
    LI RY,0
    STOREI RY, RZ, -3
    LI R0,0
    STOREI R0, RZ, -2
loop_1:
    LOADI RW, RZ, -2
    LOADI RX, RZ, -1
    CMP RW,RX
    LIMM RY,body_2
    JL RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI RW, RZ, -5
    PUSH RW
    LOADI RX, RZ, -4
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -3
    LOADI RX, RZ, -4
    STOREI RX, RZ, -5
    LOADI RY, RZ, -3
    STOREI RY, RZ, -4
    LOADI RW, RZ, -2
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -2
    LIMM RX,loop_1
    JMP RX
wend_3:
    LOADI RY, RZ, -5
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
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
