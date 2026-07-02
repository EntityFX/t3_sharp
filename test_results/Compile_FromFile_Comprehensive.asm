; T→T3
__entry:
    LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
factorial_goto:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RW, RZ, -1
    LI RW,1
    STOREI RW, RZ, -2
__glbl_loop_start:
    LOADI RX, RZ, -1
    LI RY,1
    CMP RX,RY
    LIMM RZ,then_2
    JLE RZ
    LIMM R0,end_1
    JMP R0
then_2:
    LIMM RW,__glbl_done
    JMP RW
end_1:
    LOADI RX, RZ, -2
    PUSH RX
    LOADI RY, RZ, -1
    POP RZ
    MUL R0,RZ,RY
    STOREI R0, RZ, -2
    LOADI RY, RZ, -1
    PUSH RY
    LI RZ,1
    POP RW
    SUB RX,RW,RZ
    STOREI RX, RZ, -1
    LIMM RZ,__glbl_loop_start
    JMP RZ
__glbl_done:
    LOADI RW, RZ, -2
    MOV R2,RW
    LIMM RY,epilogue_0
    JMP RY
epilogue_0:
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 9
    LI RW,1
    STOREI RW, RZ, -9
    LOADI RX, RZ, -9
    LI RY,1
    CMP RX,RY
    LIMM RZ,then_5
    JE RZ
    LIMM R0,end_4
    JMP R0
then_5:
    LI RW,2
    STOREI RW, RZ, -9
end_4:
    LI RX,42
    LI R4, 8
    SUB R4, RZ, R4
    STOREI RX, R4, 0
    LI R4, 8
    SUB R4, RZ, R4
    LOADI RY,R4, 0
    STOREI RY, RZ, -6
    LI RZ,5
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RZ
    LIMM R1,factorial_goto
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    STOREI R0, RZ, -5
    LI RW,0
    STOREI RW, RZ, -4
    LI RX,0
    STOREI RX, RZ, -3
loop_6:
    LOADI RY, RZ, -3
    LI RZ,5
    CMP RY,RZ
    LIMM R0,then_9
    JGE R0
    LIMM RW,end_8
    JMP RW
then_9:
    LIMM RX,wend_7
    JMP RX
end_8:
    LOADI RY, RZ, -4
    PUSH RY
    LOADI RZ, RZ, -3
    POP R0
    ADD RW,R0,RZ
    STOREI RW, RZ, -4
    LOADI RZ, RZ, -3
    PUSH RZ
    LI R0,1
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -3
    LI R0,1
    LI R2,0
    CMP R0,R2
    LIMM RX,loop_6
    JNE RX
wend_7:
    LI RZ,10
    PUSH RZ
    LI R0,0
    LI RW,2
    MUL RX,R0,RW
    LI R4, 2
    SUB R4, RZ, R4
    ADD R0,R4,RX
    LI RW,0
    ADD RY,R0,RW
    POP RZ
    STOREI RZ, RY, 0
    LI RX,20
    PUSH RX
    LI RY,0
    LI RW,2
    MUL R0,RY,RW
    LI R4, 2
    SUB R4, RZ, R4
    ADD RY,R4,R0
    LI RW,1
    ADD RZ,RY,RW
    POP R0
    STOREI R0, RZ, 0
    LI R0,30
    PUSH R0
    LI RZ,1
    LI RW,2
    MUL RY,RZ,RW
    LI R4, 2
    SUB R4, RZ, R4
    ADD RZ,R4,RY
    LI RW,0
    ADD R0,RZ,RW
    POP RW
    STOREI RW, R0, 0
    LOADI RY, RZ, -9
    PUSH RY
    LOADI R0, RZ, -6
    POP RW
    ADD RZ,RW,R0
    PUSH RZ
    LOADI R0, RZ, -5
    POP RW
    ADD RX,RW,R0
    PUSH RX
    LOADI R0, RZ, -4
    POP RW
    ADD RY,RW,R0
    PUSH RY
    LI R0,0
    PUSH R0
    LI RW,2
    POP RZ
    MUL R0,RZ,RW
    LI R4, 2
    SUB R4, RZ, R4
    ADD R0,R4,R0
    LI RZ,0
    ADD RW,R0,RZ
    LOADI RW,RW, 0
    POP R0
    ADD RZ,R0,RW
    PUSH RZ
    LI RW,0
    PUSH RW
    LI R0,2
    POP R0
    MUL RX,R0,R0
    LI R4, 2
    SUB R4, RZ, R4
    ADD RW,R4,RX
    LI R0,1
    ADD R0,RW,R0
    LOADI RY,R0, 0
    POP RX
    ADD R0,RX,RY
    PUSH R0
    LI RY,1
    PUSH RY
    LI RX,2
    POP R0
    MUL RW,R0,RX
    LI R4, 2
    SUB R4, RZ, R4
    ADD RY,R4,RW
    LI R0,0
    ADD RX,RY,R0
    LOADI RZ,RX, 0
    POP RW
    ADD RX,RW,RZ
    MOV R2,RX
    LIMM RZ,epilogue_3
    JMP RZ
epilogue_3:
    ADDI SP, SP, 9
    POP R4
    POP R3
    POP RZ
    RET

; --- Global Variables ---

; --- Data Section ---

; --- StdLib ---

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
print_int:
    PUSH RZ
    PUSH R3
    PUSH R4
    LI R2, 0
    LI R3, 0
    LI R4, 0
    CMPI RW, 0
    JGE pi_pos
    LI R1, 45
    OUT R1, 0
    NEG RW, RW
pi_pos:
    LI R2, 10
    DIV R3, RW, R2
    CMPI R3, 0
    JE pi_one
    DIV R3, RW, R2
    CMPI R3, 0
    JE pi_two
    DIV R3, RW, R2
    CMPI R3, 0
    JE pi_three
pi_three:
    MOD R3, RW, R2
    PUSH R3
    DIV RW, RW, R2
pi_two:
    MOD R3, RW, R2
    PUSH R3
    DIV RW, RW, R2
pi_one:
    MOD R3, RW, R2
    PUSH R3
    DIV RW, RW, R2
pi_emit:
    POP R3
    ADDI R3, 48
    OUTI R3, 0
    CMPI RW, 0
    JNE pi_emit
    POP R4
    POP R3
    POP RZ
    RET
print_str:
    PUSH RZ
    PUSH R3
    PUSH R4
ps_loop:
    LOADI R3, RW, 0
    CMPI R3, 0
    JE ps_end
    OUTI R3, 0
    ADDI RW, 1
    JMP ps_loop
ps_end:
    POP R4
    POP R3
    POP RZ
    RET
puts:
    PUSH RZ
    PUSH R3
    PUSH R4
    LIMM R1, print_str
    CALL R1
    LI R1, 10
    OUT R1, 0
    POP R4
    POP R3
    POP RZ
    RET
abs:
    PUSH RZ
    PUSH R3
    PUSH R4
    CMPI RW, 0
    JGE abs_end
    NEG RW, RW
abs_end:
    MOV R2, RW
    POP R4
    POP R3
    POP RZ
    RET
min:
    PUSH RZ
    PUSH R3
    PUSH R4
    CMP RW, RX
    JLE min_rw
    MOV R2, RX
    JMP min_end
min_rw:
    MOV R2, RW
min_end:
    POP R4
    POP R3
    POP RZ
    RET
max:
    PUSH RZ
    PUSH R3
    PUSH R4
    CMP RW, RX
    JGE max_rw
    MOV R2, RX
    JMP max_end
max_rw:
    MOV R2, RW
max_end:
    POP R4
    POP R3
    POP RZ
    RET
t_strcmp:
    PUSH RZ
    PUSH R3
    PUSH R4
tsc_loop:
    LOADI R3, RW, 0
    LOADI R4, RX, 0
    CMP R3, R4
    JNE tsc_diff
    CMPI R3, 0
    JE tsc_eq
    ADDI RW, 1
    ADDI RX, 1
    JMP tsc_loop
tsc_diff:
    CMP R3, R4
    JL tsc_lt
    LI R2, 1
    JMP tsc_end
tsc_lt:
    LI R2, -1
    JMP tsc_end
tsc_eq:
    LI R2, 0
tsc_end:
    POP R4
    POP R3
    POP RZ
    RET
t_strcpy:
    PUSH RZ
    PUSH R3
    PUSH R4
tcp_loop:
    LOADI R3, RX, 0
    STOREI R3, RW, 0
    CMPI R3, 0
    JE tcp_end
    ADDI RW, 1
    ADDI RX, 1
    JMP tcp_loop
tcp_end:
    POP R4
    POP R3
    POP RZ
    RET
t_strlen:
    LIMM R1, strlen
    CALL R1
    RET
