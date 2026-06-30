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
    LOADI RX, RZ, -2
    PUSH RX
    LI RY,0
    POP R0
    CMP R0,RY
    LIMM RX,t_1
    JG RX
    LI RW,-1
    LIMM RY,d_2
    JMP RY
t_1:
    LI RW,1
d_2:
    LI R2,0
    CMP RW,R2
    LIMM RW,t_3
    JG RW
    LIMM RX,m_4
    JE RX
    LI RY,3
    NEG R0,RY
    MOV R0,R0
    LIMM RW,d_5
    JMP RW
m_4:
    LI RX,0
    MOV R0,RX
    LIMM RY,d_5
    JMP RY
t_3:
    LI R0,3
    MOV R0,R0
d_5:
    STOREI R0, RZ, -1
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
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
