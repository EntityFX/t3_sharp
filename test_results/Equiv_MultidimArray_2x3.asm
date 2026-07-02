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
    SUBI SP, SP, 6
    LI RW,0
    PUSH RW
    LI RX, 0
    PUSH RX
    LI RY,0
    LI RZ,3
    MUL R0, RY, RZ
    POP RW
    ADD RX, RW, R0
    PUSH RX
    LI RW,0
    LI R0,1
    MUL RZ, RW, R0
    POP RY
    ADD R0, RY, RZ
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RY
    STOREI RY, R4, 0
    LI R0,1
    PUSH R0
    LI RY, 0
    PUSH RY
    LI RZ,0
    LI R0,3
    MUL RW, RZ, R0
    POP RX
    ADD RY, RX, RW
    PUSH RY
    LI RX,1
    LI RW,1
    MUL R0, RX, RW
    POP RZ
    ADD RW, RZ, R0
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RZ
    STOREI RZ, R4, 0
    LI RW,2
    PUSH RW
    LI RZ, 0
    PUSH RZ
    LI R0,0
    LI RW,3
    MUL RX, R0, RW
    POP RY
    ADD RZ, RY, RX
    PUSH RZ
    LI RY,2
    LI RX,1
    MUL RW, RY, RX
    POP R0
    ADD RX, R0, RW
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP R0
    STOREI R0, R4, 0
    LI RX,10
    PUSH RX
    LI R0, 0
    PUSH R0
    LI RW,1
    LI RX,3
    MUL RY, RW, RX
    POP RZ
    ADD R0, RZ, RY
    PUSH R0
    LI RZ,0
    LI RY,1
    MUL RX, RZ, RY
    POP RW
    ADD RY, RW, RX
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP RW
    STOREI RW, R4, 0
    LI RY,11
    PUSH RY
    LI RW, 0
    PUSH RW
    LI RX,1
    LI RY,3
    MUL RZ, RX, RY
    POP R0
    ADD RW, R0, RZ
    PUSH RW
    LI R0,1
    LI RZ,1
    MUL RY, R0, RZ
    POP RX
    ADD RZ, RX, RY
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    POP RX
    STOREI RX, R4, 0
    LI RZ,12
    PUSH RZ
    LI RX, 0
    PUSH RX
    LI RY,1
    LI RZ,3
    MUL R0, RY, RZ
    POP RW
    ADD RX, RW, R0
    PUSH RX
    LI RW,2
    LI R0,1
    MUL RZ, RW, R0
    POP RY
    ADD R0, RY, RZ
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RY
    STOREI RY, R4, 0
    LI R0, 0
    PUSH R0
    LI RY,0
    LI RZ,3
    MUL R0, RY, RZ
    POP RW
    ADD RX, RW, R0
    PUSH RX
    LI RW,0
    LI R0,1
    MUL RZ, RW, R0
    POP RY
    ADD R0, RY, RZ
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    PUSH R0
    LI RY, 0
    PUSH RY
    LI RZ,0
    LI R0,3
    MUL RW, RZ, R0
    POP RX
    ADD RY, RX, RW
    PUSH RY
    LI RX,1
    LI RW,1
    MUL R0, RX, RW
    POP RZ
    ADD RW, RZ, R0
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP RZ
    ADD R0,RZ,RW
    PUSH R0
    LI RW, 0
    PUSH RW
    LI RZ,0
    LI RW,3
    MUL RX, RZ, RW
    POP RY
    ADD RZ, RY, RX
    PUSH RZ
    LI RY,2
    LI RX,1
    MUL RW, RY, RX
    POP RZ
    ADD R0, RZ, RW
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RZ
    ADD RW,RZ,R0
    PUSH RW
    LI R0, 0
    PUSH R0
    LI RZ,1
    LI RX,3
    MUL RY, RZ, RX
    POP RW
    ADD RX, RW, RY
    PUSH RX
    LI RW,0
    LI RY,1
    MUL RX, RW, RY
    POP RZ
    ADD RY, RZ, RX
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RY
    LOADI RY,R4, 0
    POP RZ
    ADD RX,RZ,RY
    PUSH RX
    LI RY, 0
    PUSH RY
    LI RZ,1
    LI RY,3
    MUL RW, RZ, RY
    POP RZ
    ADD R0, RZ, RW
    PUSH R0
    LI RZ,1
    LI RW,1
    MUL RY, RZ, RW
    POP RZ
    ADD RW, RZ, RY
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LOADI RW,R4, 0
    POP RZ
    ADD RY,RZ,RW
    PUSH RY
    LI RW, 0
    PUSH RW
    LI RZ,1
    LI RW,3
    MUL RX, RZ, RW
    POP RY
    ADD RZ, RY, RX
    PUSH RZ
    LI RY,2
    LI RX,1
    MUL RW, RY, RX
    POP RZ
    ADD R0, RZ, RW
    LI R4, 6
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LOADI R0,R4, 0
    POP RZ
    ADD RW,RZ,R0
    MOV R2,RW
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    ADDI SP, SP, 6
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
