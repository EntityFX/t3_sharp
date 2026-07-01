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
    SUBI SP, SP, 16
    LI RW,1
    PUSH RW
    LI RX,0
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,2
    PUSH RX
    LI RY,1
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,3
    PUSH RY
    LI R0,2
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,4
    PUSH R0
    LI RW,3
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,5
    PUSH RW
    LI RX,0
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,6
    PUSH RX
    LI RY,1
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LI RY,7
    PUSH RY
    LI R0,2
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,8
    PUSH R0
    LI RW,3
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,0
    STOREI RW, RZ, -4
loop_1:
    LOADI RX, RZ, -4
    LI RY,2
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM RW,wend_3
    JMP RW
body_2:
    LI RX,0
    STOREI RX, RZ, -3
loop_4:
    LOADI RY, RZ, -3
    LI R0,2
    CMP RY,R0
    LIMM RW,body_5
    JL RW
    LIMM RX,wend_6
    JMP RX
body_5:
    LI RY,0
    STOREI RY, RZ, -2
    LI R0,0
    STOREI R0, RZ, -1
loop_7:
    LOADI RW, RZ, -1
    LI RX,2
    CMP RW,RX
    LIMM RY,body_8
    JL RY
    LIMM R0,wend_9
    JMP R0
body_8:
    LOADI RW, RZ, -2
    PUSH RW
    LOADI RX, RZ, -4
    PUSH RX
    LI RY,2
    POP R0
    MUL RW,R0,RY
    PUSH RW
    LOADI RY, RZ, -1
    POP R0
    ADD RX,R0,RY
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    PUSH RX
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,2
    POP RY
    MUL RW,RY,R0
    PUSH RW
    LOADI R0, RZ, -3
    POP RY
    ADD RX,RY,R0
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    POP R0
    MUL RY,R0,RX
    POP RX
    ADD R0,RX,RY
    STOREI R0, RZ, -2
    LOADI RY, RZ, -1
    PUSH RY
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -1
    LIMM RX,loop_7
    JMP RX
wend_9:
    LOADI RY, RZ, -2
    PUSH RY
    LOADI RW, RZ, -4
    PUSH RW
    LI RX,2
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LOADI RX, RZ, -3
    POP RY
    ADD RW,RY,RX
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LOADI RW, RZ, -3
    PUSH RW
    LI RX,1
    POP RY
    ADD R0,RY,RX
    STOREI R0, RZ, -3
    LIMM RX,loop_4
    JMP RX
wend_6:
    LOADI RY, RZ, -4
    PUSH RY
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -4
    LIMM RW,loop_1
    JMP RW
wend_3:
    LI RX,0
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LOADI RX,R4, 0
    PUSH RX
    LI R0,1
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RW
    ADD RX,RW,R0
    PUSH RX
    LI R0,2
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RW
    ADD RY,RW,R0
    PUSH RY
    LI R0,3
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RW
    ADD RX,RW,R0
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    ADDI SP, SP, 16
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
