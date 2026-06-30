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
    SUBI SP, SP, 1
    LI RW,100
    STOREI RW, RZ, -1
    LOADI RX, RZ, -1
    LI RY,1
    CMP RX,RY
    LIMM R0,scase_2
    JE R0
    LI RW,2
    CMP RX,RW
    LIMM RX,scase_3
    JE RX
    LIMM RY,scase_4
    JMP RY
scase_2:
    LI R0,10
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
    LIMM RX,swend_1
    JMP RX
scase_3:
    LI RY,20
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
    LIMM RW,swend_1
    JMP RW
scase_4:
    LI RX,99
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
    LIMM R0,swend_1
    JMP R0
swend_1:
epilogue_0:
    ADDI SP, SP, 1
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
