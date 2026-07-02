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
    POP RZ
    STOREI RZ, R4, 0
    LI RY,3
    PUSH RY
    LI RZ,2
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    POP R0
    STOREI R0, R4, 0
    LI RZ,4
    PUSH RZ
    LI R0,3
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    STOREI RW, R4, 0
    LI R0,5
    PUSH R0
    LI RW,0
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    STOREI RX, R4, 0
    LI RW,6
    PUSH RW
    LI RX,1
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LI RX,7
    PUSH RX
    LI RY,2
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP RZ
    STOREI RZ, R4, 0
    LI RY,8
    PUSH RY
    LI RZ,3
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    POP R0
    STOREI R0, R4, 0
    LI RZ,0
    STOREI RZ, RZ, -4
loop_1:
    LOADI R0, RZ, -4
    LI RW,2
    CMP R0,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LI RZ,0
    STOREI RZ, RZ, -3
loop_4:
    LOADI R0, RZ, -3
    LI RW,2
    CMP R0,RW
    LIMM RX,body_5
    JL RX
    LIMM RY,wend_6
    JMP RY
body_5:
    LI RZ,0
    STOREI RZ, RZ, -2
    LI R0,0
    STOREI R0, RZ, -1
loop_7:
    LOADI RW, RZ, -1
    LI RX,2
    CMP RW,RX
    LIMM RY,body_8
    JL RY
    LIMM RZ,wend_9
    JMP RZ
body_8:
    LOADI R0, RZ, -2
    PUSH R0
    LOADI RW, RZ, -4
    PUSH RW
    LI RX,2
    POP RY
    MUL RZ,RY,RX
    PUSH RZ
    LOADI RX, RZ, -1
    POP RY
    ADD R0,RY,RX
    LI R4, 16
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    PUSH R0
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,2
    POP RW
    MUL RX,RW,RY
    PUSH RX
    LOADI RY, RZ, -3
    POP RW
    ADD RZ,RW,RY
    LI R4, 12
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    LOADI RZ,R4, 0
    POP RY
    MUL RW,RY,RZ
    POP RZ
    ADD RY,RZ,RW
    STOREI RY, RZ, -2
    LOADI RW, RZ, -1
    PUSH RW
    LI RZ,1
    POP R0
    ADD RW,R0,RZ
    STOREI RW, RZ, -1
    LIMM RZ,loop_7
    JMP RZ
wend_9:
    LOADI R0, RZ, -2
    PUSH R0
    LOADI RX, RZ, -4
    PUSH RX
    LI RY,2
    POP RZ
    MUL R0,RZ,RY
    PUSH R0
    LOADI RY, RZ, -3
    POP RZ
    ADD RW,RZ,RY
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RY
    STOREI RY, R4, 0
    LOADI RW, RZ, -3
    PUSH RW
    LI RY,1
    POP RZ
    ADD RX,RZ,RY
    STOREI RX, RZ, -3
    LIMM RY,loop_4
    JMP RY
wend_6:
    LOADI RZ, RZ, -4
    PUSH RZ
    LI RY,1
    POP RZ
    ADD R0,RZ,RY
    STOREI R0, RZ, -4
    LIMM RY,loop_1
    JMP RY
wend_3:
    LI RZ,0
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    LOADI RZ,R4, 0
    PUSH RZ
    LI RW,1
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP RX
    ADD RY,RX,RW
    PUSH RY
    LI RW,2
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP RX
    ADD RZ,RX,RW
    PUSH RZ
    LI RW,3
    LI R4, 8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP RX
    ADD R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
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
