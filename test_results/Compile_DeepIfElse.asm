; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 1
    LI RW,5
    STOREI RW, RZ, -3
    LOADI RX, RZ, -3
    LI RY,1
    CMP RX,RY
    LIMM R0,then_2
    JE R0
    LOADI R3, RZ, -3
    LI RW,2
    CMP R3,RW
    LIMM RX,then_4
    JE RX
    LOADI RY, RZ, -3
    LI R0,3
    CMP RY,R0
    LIMM R3,then_6
    JE R3
    LOADI RW, RZ, -3
    LI RX,4
    CMP RW,RX
    LIMM RY,then_8
    JE RY
    LOADI R0, RZ, -3
    LI R3,5
    CMP R0,R3
    LIMM RW,then_10
    JE RW
    LI RX,1
    NEG RY,RX
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
    LIMM R3,end_9
    JMP R3
then_10:
    LI RW,5
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
end_9:
    LIMM RY,end_7
    JMP RY
then_8:
    LI R0,4
    MOV R2,R0
    LIMM R3,epilogue_0
    JMP R3
end_7:
    LIMM RW,end_5
    JMP RW
then_6:
    LI RX,3
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
end_5:
    LIMM R0,end_3
    JMP R0
then_4:
    LI R3,2
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
end_3:
    LIMM RX,end_1
    JMP RX
then_2:
    LI RY,1
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
end_1:
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
