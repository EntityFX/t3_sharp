; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
add:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 2
    STOREI RW, RZ, -3
    STOREI RX, RZ, -4
    LOADI RW, RZ, -3
    PUSH RW
    LOADI RX, RZ, -4
    POP RY
    ADD R0,RY,RX
    MOV R2,R0
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
mul:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 2
    STOREI RW, RZ, -3
    STOREI RX, RZ, -4
    LOADI RW, RZ, -3
    PUSH RW
    LOADI RX, RZ, -4
    POP RY
    MUL R0,RY,RX
    MOV R2,R0
    LIMM RX,epilogue_1
    JMP RX
epilogue_1:
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 4
    LI RW,3
    LI RX,4
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,add
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    STOREI RY, RZ, -3
    LI R0,5
    LI R3,6
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R0
    MOV RX,R3
    LIMM R1,mul
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    STOREI RW, RZ, -4
    LI RX,2
    LI RY,3
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    MOV RX,RY
    LIMM R1,add
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    STOREI R0, RZ, -5
    LI R3,4
    LI RW,5
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RX,RW
    MOV RW,R3
    LIMM R1,mul
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    STOREI RX, RZ, -6
    LOADI RY, RZ, -3
    PUSH RY
    LOADI R0, RZ, -4
    POP R3
    ADD RW,R3,R0
    MOV R2,RW
    LIMM R0,epilogue_2
    JMP R0
epilogue_2:
    ADDI SP, SP, 4
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
