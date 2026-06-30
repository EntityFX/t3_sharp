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
    LI RW,5
    STOREI RW, RZ, -1
    LOADI RX, RZ, -1
    LI RY,1
    CMP RX,RY
    LIMM R0,then_2
    JE R0
    LOADI RW, RZ, -1
    LI RX,2
    CMP RW,RX
    LIMM RY,then_4
    JE RY
    LOADI R0, RZ, -1
    LI RW,3
    CMP R0,RW
    LIMM RX,then_6
    JE RX
    LOADI RY, RZ, -1
    LI R0,4
    CMP RY,R0
    LIMM RW,then_8
    JE RW
    LOADI RX, RZ, -1
    LI RY,5
    CMP RX,RY
    LIMM R0,then_10
    JE R0
    LI RW,1
    NEG RX,RW
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
    LIMM R0,end_9
    JMP R0
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
    LIMM RW,epilogue_0
    JMP RW
end_7:
    LIMM RX,end_5
    JMP RX
then_6:
    LI RY,3
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
end_5:
    LIMM RW,end_3
    JMP RW
then_4:
    LI RX,2
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
end_3:
    LIMM R0,end_1
    JMP R0
then_2:
    LI RW,1
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
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
