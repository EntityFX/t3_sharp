; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
fib:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RW, RZ, -1
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_2
    JLE RY
    LIMM R0,end_1
    JMP R0
then_2:
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
end_1:
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
    POP RW
    SUB RX,RW,R0
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    PUSH R0
    LOADI RW, RZ, -1
    PUSH RW
    LI RY,2
    POP R0
    SUB RW,R0,RY
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
    MOV RY,R2
    POP R0
    ADD RX,R0,RY
    MOV R2,RX
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
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
