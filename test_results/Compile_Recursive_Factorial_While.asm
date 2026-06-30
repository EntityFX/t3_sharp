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
    STOREI RW, RZ, -1
    LI RW,1
    STOREI RW, RZ, -2
loop_1:
    LOADI RX, RZ, -1
    LI RY,1
    CMP RX,RY
    LIMM R0,body_2
    JG R0
    LIMM RW,wend_3
    JMP RW
body_2:
    LOADI RX, RZ, -2
    PUSH RX
    LOADI RY, RZ, -1
    POP R0
    MUL RW,R0,RY
    STOREI RW, RZ, -2
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
    POP RX
    SUB RY,RX,R0
    STOREI RY, RZ, -1
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI RX, RZ, -2
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
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
