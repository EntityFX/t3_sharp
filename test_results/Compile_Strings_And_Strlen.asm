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
    LIMM RW,str_1
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,strlen
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    STOREI RX, RZ, -2
    LIMM RY,str_2
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,strlen
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    STOREI R0, RZ, -1
    LOADI RW, RZ, -2
    PUSH RW
    LOADI RX, RZ, -1
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

; --- Global Variables ---

; --- Data Section ---
.string str_1 "Hello"
.string str_2 "World!"

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
