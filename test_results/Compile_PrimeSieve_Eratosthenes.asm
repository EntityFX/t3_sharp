; T→T3
__entry:
    LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 34
    LI RW,2
    STOREI RW, RZ, -3
loop_1:
    LOADI RX, RZ, -3
    LI RY,30
    CMP RX,RY
    LIMM RZ,body_2
    JLE RZ
    LIMM R0,wend_3
    JMP R0
body_2:
    LI RW,1
    PUSH RW
    LOADI RX, RZ, -3
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LOADI RX, RZ, -3
    PUSH RX
    LI RY,1
    POP RZ
    ADD R0,RZ,RY
    STOREI R0, RZ, -3
    LIMM RY,loop_1
    JMP RY
wend_3:
    LI RZ,2
    STOREI RZ, RZ, -3
loop_4:
    LOADI RW, RZ, -3
    PUSH RW
    LOADI RX, RZ, -3
    POP RY
    MUL RZ,RY,RX
    LI RX,30
    CMP RZ,RX
    LIMM RY,body_5
    JLE RY
    LIMM R0,wend_6
    JMP R0
body_5:
    LOADI RW, RZ, -3
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    LI RX,1
    CMP RW,RX
    LIMM RY,then_8
    JE RY
    LIMM RZ,end_7
    JMP RZ
then_8:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI RW, RZ, -3
    POP RX
    MUL RY,RX,RW
    STOREI RY, RZ, -2
loop_9:
    LOADI RW, RZ, -2
    LI RX,30
    CMP RW,RX
    LIMM RZ,body_10
    JLE RZ
    LIMM R0,wend_11
    JMP R0
body_10:
    LI RW,0
    PUSH RW
    LOADI RX, RZ, -2
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LOADI RX, RZ, -2
    PUSH RX
    LOADI RY, RZ, -3
    POP RZ
    ADD R0,RZ,RY
    STOREI R0, RZ, -2
    LIMM RY,loop_9
    JMP RY
wend_11:
end_7:
    LOADI RZ, RZ, -3
    PUSH RZ
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -3
    LIMM RW,loop_4
    JMP RW
wend_6:
    LI RX,0
    STOREI RX, RZ, -1
    LI RZ,2
    STOREI RZ, RZ, -3
loop_12:
    LOADI R0, RZ, -3
    LI RW,30
    CMP R0,RW
    LIMM RX,body_13
    JLE RX
    LIMM RY,wend_14
    JMP RY
body_13:
    LOADI RZ, RZ, -3
    LI R4, 34
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    LOADI RZ,R4, 0
    LI R0,1
    CMP RZ,R0
    LIMM RW,then_16
    JE RW
    LIMM RX,end_15
    JMP RX
then_16:
    LOADI RY, RZ, -1
    PUSH RY
    LI RZ,1
    POP R0
    ADD RW,R0,RZ
    STOREI RW, RZ, -1
end_15:
    LOADI RZ, RZ, -3
    PUSH RZ
    LI R0,1
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -3
    LIMM R0,loop_12
    JMP R0
wend_14:
    LOADI RX, RZ, -1
    MOV R2,RX
    LIMM RZ,epilogue_0
    JMP RZ
epilogue_0:
    ADDI SP, SP, 34
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
