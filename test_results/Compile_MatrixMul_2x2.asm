; T→T3
__entry:
    S.LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH R3
    PUSH R4
    LIMM R3,16
    S.SUB SP, SP, R3
    S.MOV RZ, SP
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
    POP R0
    ST R0, R4, 0
    MOV RY,3
    PUSH RY
    MOV R0,2
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    ST RW, R4, 0
    MOV R0,4
    PUSH R0
    MOV RW,3
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    ST RX, R4, 0
    MOV RW,5
    PUSH RW
    MOV RX,0
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RX
    POP RY
    ST RY, R4, 0
    MOV RX,6
    PUSH RX
    MOV RY,1
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RY
    POP R0
    ST R0, R4, 0
    MOV RY,7
    PUSH RY
    MOV R0,2
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,R0
    POP RW
    ST RW, R4, 0
    MOV R0,8
    PUSH R0
    MOV RW,3
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    ST RX, R4, 0
    MOV RW,0
    ST RW, RZ, -4
loop_1:
    LD RX, RZ, -4
    MOV RY,2
    CMP RX,RY
    LIMM R0,body_2
    JL R0
    LIMM RW,wend_3
    JMP RW
body_2:
    MOV RX,0
    ST RX, RZ, -3
loop_4:
    LD RY, RZ, -3
    MOV R0,2
    CMP RY,R0
    LIMM RW,body_5
    JL RW
    LIMM RX,wend_6
    JMP RX
body_5:
    MOV RY,0
    ST RY, RZ, -2
    MOV R0,0
    ST R0, RZ, -1
loop_7:
    LD RW, RZ, -1
    MOV RX,2
    CMP RW,RX
    LIMM RY,body_8
    JL RY
    LIMM R0,wend_9
    JMP R0
body_8:
    LD RW, RZ, -2
    PUSH RW
    LD RX, RZ, -4
    PUSH RX
    MOV RY,2
    POP R0
    MUL RW,R0,RY
    PUSH RW
    LD RY, RZ, -1
    POP R0
    ADD RX,R0,RY
    MOV R4,16
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LD RX,R4, 0
    PUSH RX
    LD RY, RZ, -1
    PUSH RY
    MOV R0,2
    POP RY
    MUL RW,RY,R0
    PUSH RW
    LD R0, RZ, -3
    POP RY
    ADD RX,RY,R0
    MOV R4,12
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LD RX,R4, 0
    POP R0
    MUL RY,R0,RX
    POP RX
    ADD R0,RX,RY
    ST R0, RZ, -2
    LD RY, RZ, -1
    PUSH RY
    MOV RX,1
    POP RY
    ADD R0,RY,RX
    ST R0, RZ, -1
    LIMM RX,loop_7
    JMP RX
wend_9:
    LD RY, RZ, -2
    PUSH RY
    LD RW, RZ, -4
    PUSH RW
    MOV RX,2
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LD RX, RZ, -3
    POP RY
    ADD RW,RY,RX
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RW
    POP RX
    ST RX, R4, 0
    LD RW, RZ, -3
    PUSH RW
    MOV RX,1
    POP RY
    ADD R0,RY,RX
    ST R0, RZ, -3
    LIMM RX,loop_4
    JMP RX
wend_6:
    LD RY, RZ, -4
    PUSH RY
    MOV RW,1
    POP RX
    ADD RY,RX,RW
    ST RY, RZ, -4
    LIMM RW,loop_1
    JMP RW
wend_3:
    MOV RX,0
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,RX
    LD RX,R4, 0
    PUSH RX
    MOV R0,1
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LD R0,R4, 0
    POP RW
    ADD RX,RW,R0
    PUSH RX
    MOV R0,2
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LD R0,R4, 0
    POP RW
    ADD RY,RW,R0
    PUSH RY
    MOV R0,3
    MOV R4,8
    SUB R4, RZ, R4
    ADD R4,R4,R0
    LD R0,R4, 0
    POP RW
    ADD RX,RW,R0
    MOV R2,RX
    LIMM R0,epilogue_0
    JMP R0
epilogue_0:
    LIMM RW,16
    S.ADD SP, SP, RW
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
    MOV R2, 0
strlen_loop:
    LD R1, RW, 0
    MOV R0, 0
    CMP R1, R0
    JE strlen_end
    ADD R2, R2, 1
    ADD RW, RW, 1
    LIMM R0, strlen_loop
    JMP R0
strlen_end:
    POP R4
    POP R3
    POP RZ
    RET
putchar:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV R0, 0
    OUT RW, R0
    POP R4
    POP R3
    POP RZ
    RET
getchar:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV R0, 0
    IN R2, R0
    POP R4
    POP R3
    POP RZ
    RET
print_int:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV R2, 0
    MOV R3, 0
    MOV R4, 0
    MOV R0, 0
    CMP RW, R0
    JGE pi_pos
    MOV R1, 45
    MOV R0, 0
    OUT R1, R0
    NEG RW, RW
pi_pos:
    MOV R2, 10
    DIV R3, RW, R2
    MOV R0, 0
    CMP R3, R0
    JE pi_one
    DIV R3, RW, R2
    MOV R0, 0
    CMP R3, R0
    JE pi_two
    DIV R3, RW, R2
    MOV R0, 0
    CMP R3, R0
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
    ADD R3, R3, 48
    MOV R0, 0
    OUT R3, R0
    MOV R0, 0
    CMP RW, R0
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
    LD R3, RW, 0
    MOV R0, 0
    CMP R3, R0
    JE ps_end
    MOV R0, 0
    OUT R3, R0
    ADD RW, RW, 1
    LIMM R0, ps_loop
    JMP R0
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
    MOV R1, 10
    MOV R0, 0
    OUT R1, R0
    POP R4
    POP R3
    POP RZ
    RET
abs:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV R0, 0
    CMP RW, R0
    JGE abs_end
    NEG RW, RW
abs_end:
    MOV R0, 0
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
    LIMM R0, min_end
    JMP R0
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
    LIMM R0, max_end
    JMP R0
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
    LD R3, RW, 0
    LD R4, RX, 0
    CMP R3, R4
    JNE tsc_diff
    MOV R0, 0
    CMP R3, R0
    JE tsc_eq
    ADD RW, RW, 1
    ADD RX, RX, 1
    LIMM R0, tsc_loop
    JMP R0
tsc_diff:
    CMP R3, R4
    JL tsc_lt
    MOV R2, 1
    LIMM R0, tsc_end
    JMP R0
tsc_lt:
    MOV R2, -1
    LIMM R0, tsc_end
    JMP R0
tsc_eq:
    MOV R2, 0
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
    LD R3, RX, 0
    ST R3, RW, 0
    MOV R0, 0
    CMP R3, R0
    JE tcp_end
    ADD RW, RW, 1
    ADD RX, RX, 1
    LIMM R0, tcp_loop
    JMP R0
tcp_end:
    POP R4
    POP R3
    POP RZ
    RET
t_strlen:
    LIMM R1, strlen
    CALL R1
    RET
