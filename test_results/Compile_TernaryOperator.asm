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
    STOREI RZ, -1, RW
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,0
    POP R0
    CMP R0,RY
    LIMM RW,t_1
    JG RW
    LI R3,-1
    LIMM RX,d_2
    JMP RX
t_1:
    LI R3,1
d_2:
    LI R2,0
    CMP R3,R2
    LIMM R0,t_3
    JG R0
    LIMM R3,m_4
    JE R3
    LI RW,3
    NEG RX,RW
    MOV RY,RX
    LIMM RY,d_5
    JMP RY
m_4:
    LI R0,0
    MOV RY,R0
    LIMM R3,d_5
    JMP R3
t_3:
    LI RW,3
    MOV RY,RW
d_5:
    STOREI RZ, -2, RY
    LOADI RX, RZ, -2
    MOV R2,RX
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
