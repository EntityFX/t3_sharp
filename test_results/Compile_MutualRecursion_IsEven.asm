; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
isEven:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RZ, -1, RW
    LOADI RW, RZ, -1
    LI RX,0
    CMP RW,RX
    LIMM RY,then_2
    JE RY
    LIMM R0,end_1
    JMP R0
then_2:
    LI R3,1
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
end_1:
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP R0
    SUB R3,R0,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R3
    LIMM R1,isOdd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
isOdd:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RZ, -1, RW
    LOADI RW, RZ, -1
    LI RX,0
    CMP RW,RX
    LIMM RY,then_5
    JE RY
    LIMM R0,end_4
    JMP R0
then_5:
    LI R3,0
    MOV R2,R3
    LIMM RW,epilogue_3
    JMP RW
end_4:
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP R0
    SUB R3,R0,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R3
    LIMM R1,isEven
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    MOV R2,RY
    LIMM R0,epilogue_3
    JMP R0
epilogue_3:
    ADDI SP, SP, 1
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
    LIMM R1,isEven
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    MOV R2,RX
    LIMM RY,epilogue_6
    JMP RY
epilogue_6:
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
