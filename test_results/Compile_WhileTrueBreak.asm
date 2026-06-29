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
    LI RW,0
    STOREI RW, RZ, -3
loop_1:
    LI RX,1
    LI R2,0
    CMP RX,R2
    LIMM RY,body_2
    JNE RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI R3, RZ, -3
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -3
    LOADI RW, RZ, -3
    LI RX,10
    CMP RW,RX
    LIMM R0,then_5
    JG R0
    LIMM R3,end_4
    JMP R3
then_5:
    LIMM RW,wend_3
    JMP RW
end_4:
    LIMM RX,loop_1
    JMP RX
wend_3:
    LOADI RY, RZ, -3
    MOV R2,RY
    LIMM R0,epilogue_0
    JMP R0
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
