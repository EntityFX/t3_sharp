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
    LIMM R0,then_2
    JLE R0
    LIMM RW,end_1
    JMP RW
then_2:
    LIMM RX,__glbl_done
    JMP RX
end_1:
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R0, RZ, -1
    POP RW
    MUL RX,RW,R0
    STOREI RX, RZ, -2
    LOADI R0, RZ, -1
    PUSH R0
    LI RW,1
    POP RY
    SUB R0,RY,RW
    STOREI R0, RZ, -1
    LIMM RW,__glbl_loop_start
    JMP RW
__glbl_done:
    LOADI RY, RZ, -2
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
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
    LIMM R0,then_5
    JE R0
    LIMM RW,end_4
    JMP RW
then_5:
    LI RX,2
    STOREI RX, RZ, -9
end_4:
    LI RY,42
    LI R4, 8
    SUB R4, RZ, R4
    STOREI RY, R4, 0
    LI R4, 8
    SUB R4, RZ, R4
    LOADI R0,R4, 0
    STOREI R0, RZ, -6
    LI RW,5
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,factorial_goto
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    STOREI RX, RZ, -5
    LI RY,0
    STOREI RY, RZ, -4
    LI R0,0
    STOREI R0, RZ, -3
loop_6:
    LOADI RW, RZ, -3
    LI RX,5
    CMP RW,RX
    LIMM RY,then_9
    JGE RY
    LIMM R0,end_8
    JMP R0
then_9:
    LIMM RW,wend_7
    JMP RW
end_8:
    LOADI RX, RZ, -4
    PUSH RX
    LOADI RY, RZ, -3
    POP R0
    ADD RW,R0,RY
    STOREI RW, RZ, -4
    LOADI RY, RZ, -3
    PUSH RY
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
    LI R0,10
    PUSH R0
    LI RW,0
    LI RX,2
    MUL RY,RW,RX
    LI R4, 2
    SUB R4, RZ, R4
    ADD RW,R4,RY
    LI RX,0
    ADD R0,RW,RX
    POP RW
    STOREI RW, R0, 0
    LI RY,20
    PUSH RY
    LI R0,0
    LI RX,2
    MUL RW,R0,RX
    LI R4, 2
    SUB R4, RZ, R4
    ADD R0,R4,RW
    LI RX,1
    ADD RW,R0,RX
    POP RX
    STOREI RX, RW, 0
    LI RW,30
    PUSH RW
    LI RW,1
    LI RX,2
    MUL R0,RW,RX
    LI R4, 2
    SUB R4, RZ, R4
    ADD RW,R4,R0
    LI RX,0
    ADD RX,RW,RX
    POP RY
    STOREI RY, RX, 0
    LOADI R0, RZ, -9
    PUSH R0
    LOADI RX, RZ, -6
    POP RW
    ADD RY,RW,RX
    PUSH RY
    LOADI RX, RZ, -5
    POP RW
    ADD R0,RW,RX
    PUSH R0
    LOADI RX, RZ, -4
    POP RW
    ADD RY,RW,RX
    PUSH RY
    LI RX,0
    PUSH RX
    LI RW,2
    POP R0
    MUL RW,R0,RW
    LI R4, 2
    SUB R4, RZ, R4
    ADD RX,R4,RW
    LI R0,0
    ADD RW,RX,R0
    LOADI RX,RW, 0
    POP RW
    ADD R0,RW,RX
    PUSH R0
    LI RX,0
    PUSH RX
    LI RW,2
    POP RX
    MUL RY,RX,RW
    LI R4, 2
    SUB R4, RZ, R4
    ADD RX,R4,RY
    LI RX,1
    ADD RW,RX,RX
    LOADI R0,RW, 0
    POP RY
    ADD RW,RY,R0
    PUSH RW
    LI R0,1
    PUSH R0
    LI RY,2
    POP RX
    MUL RX,RX,RY
    LI R4, 2
    SUB R4, RZ, R4
    ADD R0,R4,RX
    LI RX,0
    ADD RY,R0,RX
    LOADI RW,RY, 0
    POP RX
    ADD RY,RX,RW
    MOV R2,RY
    LIMM RW,epilogue_3
    JMP RW
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
