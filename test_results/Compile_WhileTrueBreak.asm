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
    LI RW,0
    STOREI RW, RZ, -1
loop_1:
    LI RX,1
    LI R2,0
    CMP RX,R2
    LIMM RY,body_2
    JNE RY
    LIMM R0,wend_3
    JMP R0
body_2:
    LOADI RW, RZ, -1
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -1
    LOADI RX, RZ, -1
    LI RY,10
    CMP RX,RY
    LIMM RW,then_5
    JG RW
    LIMM RX,end_4
    JMP RX
then_5:
    LIMM RY,wend_3
    JMP RY
end_4:
    LIMM R0,loop_1
    JMP R0
wend_3:
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
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
