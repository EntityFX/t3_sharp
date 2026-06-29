; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
fib:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    SUBI SP, SP, 1
    STOREI RW, RZ, -3
    LOADI RW, RZ, -3
    LI RX,1
    CMP RW,RX
    LIMM RY,then_2
    JLE RY
    LIMM R0,end_1
    JMP R0
then_2:
    LOADI R3, RZ, -3
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
end_1:
    LOADI RX, RZ, -3
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
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    PUSH RY
    LOADI R0, RZ, -3
    PUSH R0
    LI RW,2
    POP RX
    SUB RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    ADD R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    GETSP RZ
    PUSH R3
    PUSH R4
    LI RW,6
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    MOV R2,RX
    LIMM RY,epilogue_3
    JMP RY
epilogue_3:
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
