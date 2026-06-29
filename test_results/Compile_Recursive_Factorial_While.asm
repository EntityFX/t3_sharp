; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
fact:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RZ, -2, RW
    LI RW,1
    STOREI RZ, -1, RW
loop_1:
    LOADI RX, RZ, -2
    LI RY,1
    CMP RX,RY
    LIMM R0,body_2
    JG R0
    LIMM R3,wend_3
    JMP R3
body_2:
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -2
    POP RY
    MUL R0,RY,RX
    STOREI RZ, -1, R0
    LOADI RX, RZ, -2
    PUSH RX
    LI RY,1
    POP R3
    SUB RW,R3,RY
    STOREI RZ, -2, RW
    LIMM RY,loop_1
    JMP RY
wend_3:
    LOADI R3, RZ, -1
    MOV R2,R3
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    LI RW,7
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fact
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
