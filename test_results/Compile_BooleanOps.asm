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
    STOREI RW, RZ, -2
    LI RX,-1
    STOREI RX, RZ, -1
    LOADI RY, RZ, -2
    LI R0,1
    CMP RY,R0
    LIMM RW,then_2
    JE RW
    LIMM RX,end_1
    JMP RX
then_2:
    LI RY,1
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
end_1:
    LI RW,1
    NEG RX,RW
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
