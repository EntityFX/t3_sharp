; T→T3
__entry:
    LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH R3
    PUSH R4
    S.MOV RZ, FP
    SUB SP, SP, 16
    MOV RW,1
    PUSH RW
    MOV RX,0
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    ST RY, R4, 0
    MOV RX,2
    PUSH RX
    MOV RY,1
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP RZ
    ST RZ, R4, 0
    MOV RY,3
    PUSH RY
    MOV RZ,2
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    POP R0
    ST R0, R4, 0
    MOV RZ,4
    PUSH RZ
    MOV R0,3
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    ST RW, R4, 0
    MOV R0,5
    PUSH R0
    MOV RW,0
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    ST RX, R4, 0
    MOV RW,6
    PUSH RW
    MOV RX,1
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    ST RY, R4, 0
    MOV RX,7
    PUSH RX
    MOV RY,2
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP RZ
    ST RZ, R4, 0
    MOV RY,8
    PUSH RY
    MOV RZ,3
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    POP R0
    ST R0, R4, 0
    MOV RZ,0
    ST RZ, RZ, -4
loop_1:
    LD R0, RZ, -4
    MOV RW,2
    CMP R0,RW
    LIMM RX,body_2
    JL RX
    LIMM RY,wend_3
    JMP RY
body_2:
    MOV RZ,0
    ST RZ, RZ, -3
loop_4:
    LD R0, RZ, -3
    MOV RW,2
    CMP R0,RW
    LIMM RX,body_5
    JL RX
    LIMM RY,wend_6
    JMP RY
body_5:
    MOV RZ,0
    ST RZ, RZ, -2
    MOV R0,0
    ST R0, RZ, -1
loop_7:
    LD RW, RZ, -1
    MOV RX,2
    CMP RW,RX
    LIMM RY,body_8
    JL RY
    LIMM RZ,wend_9
    JMP RZ
body_8:
    LD R0, RZ, -2
    PUSH R0
    LD RW, RZ, -4
    PUSH RW
    MOV RX,2
    POP RY
    MUL RZ,RY,RX
    PUSH RZ
    LD RX, RZ, -1
    POP RY
    ADD R0,RY,RX
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LD R0,R4, 0
    PUSH R0
    LD RX, RZ, -1
    PUSH RX
    MOV RY,2
    POP RW
    MUL RX,RW,RY
    PUSH RX
    LD RY, RZ, -3
    POP RW
    ADD RZ,RW,RY
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    LD RZ,R4, 0
    POP RY
    MUL RW,RY,RZ
    POP RZ
    ADD RY,RZ,RW
    ST RY, RZ, -2
    LD RW, RZ, -1
    PUSH RW
    MOV RZ,1
    POP R0
    ADD RW,R0,RZ
    ST RW, RZ, -1
    LIMM RZ,loop_7
    JMP RZ
wend_9:
    LD R0, RZ, -2
    PUSH R0
    LD RX, RZ, -4
    PUSH RX
    MOV RY,2
    POP RZ
    MUL R0,RZ,RY
    PUSH R0
    LD RY, RZ, -3
    POP RZ
    ADD RW,RZ,RY
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RY
    ST RY, R4, 0
    LD RW, RZ, -3
    PUSH RW
    MOV RY,1
    POP RZ
    ADD RX,RZ,RY
    ST RX, RZ, -3
    LIMM RY,loop_4
    JMP RY
wend_6:
    LD RZ, RZ, -4
    PUSH RZ
    MOV RY,1
    POP RZ
    ADD R0,RZ,RY
    ST R0, RZ, -4
    LIMM RY,loop_1
    JMP RY
wend_3:
    MOV RZ,0
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RZ
    LD RZ,R4, 0
    PUSH RZ
    MOV RW,1
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LD RW,R4, 0
    POP RX
    ADD RY,RX,RW
    PUSH RY
    MOV RW,2
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LD RW,R4, 0
    POP RX
    ADD RZ,RX,RW
    PUSH RZ
    MOV RW,3
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    LD RW,R4, 0
    POP RX
    ADD R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    ADD SP, SP, 16
    POP R4
    POP R3
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
