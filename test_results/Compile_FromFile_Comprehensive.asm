; T→T3
__entry:
    S.LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
factorial_goto:
    PUSH R3
    PUSH R4
    LIMM R3,2
    S.SUB SP, SP, R3
    S.MOV RZ, SP
    ST RW, RZ, -1
    MOV RW,1
    ST RW, RZ, -2
__glbl_loop_start:
    LD RX, RZ, -1
    MOV RY,1
    CMP RX,RY
    LIMM R0,then_2
    JLE R0
    LIMM RW,end_1
    JMP RW
then_2:
    LIMM RX,__glbl_done
    JMP RX
end_1:
    LD RY, RZ, -2
    PUSH RY
    LD R0, RZ, -1
    POP RW
    MUL RX,RW,R0
    ST RX, RZ, -2
    LD R0, RZ, -1
    PUSH R0
    MOV RW,1
    POP RY
    SUB R0,RY,RW
    ST R0, RZ, -1
    LIMM RW,__glbl_loop_start
    JMP RW
__glbl_done:
    LD RY, RZ, -2
    MOV R2,RY
    LIMM RW,epilogue_0
    JMP RW
epilogue_0:
    LIMM RX,2
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET
main:
    PUSH R3
    PUSH R4
    LIMM R3,9
    S.SUB SP, SP, R3
    S.MOV RZ, SP
    MOV RW,1
    ST RW, RZ, -9
    LD RX, RZ, -9
    MOV RY,1
    CMP RX,RY
    LIMM R0,then_5
    JE R0
    LIMM RW,end_4
    JMP RW
then_5:
    MOV RX,2
    ST RX, RZ, -9
end_4:
    MOV RY,42
    MOV R4,8
    SUB R4, RZ, R4
    ST RY, R4, 0
    MOV R4,8
    SUB R4, RZ, R4
    LD R0,R4, 0
    ST R0, RZ, -6
    MOV RW,5
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
    ST RX, RZ, -5
    MOV RY,0
    ST RY, RZ, -4
    MOV R0,0
    ST R0, RZ, -3
loop_6:
    LD RW, RZ, -3
    MOV RX,5
    CMP RW,RX
    LIMM RY,then_9
    JGE RY
    LIMM R0,end_8
    JMP R0
then_9:
    LIMM RW,wend_7
    JMP RW
end_8:
    LD RX, RZ, -4
    PUSH RX
    LD RY, RZ, -3
    POP R0
    ADD RW,R0,RY
    ST RW, RZ, -4
    LD RY, RZ, -3
    PUSH RY
    MOV R0,1
    POP RX
    ADD RY,RX,R0
    ST RY, RZ, -3
    MOV R0,1
    MOV R2,0
    CMP R0,R2
    LIMM RX,loop_6
    JNE RX
wend_7:
    MOV R0,10
    PUSH R0
    MOV RW,0
    MOV RX,2
    MUL RY,RW,RX
    MOV R4,2
    SUB R4, RZ, R4
    ADD RW,R4,RY
    MOV RX,0
    ADD R0,RW,RX
    POP RW
    ST RW, R0, 0
    MOV RY,20
    PUSH RY
    MOV R0,0
    MOV RX,2
    MUL RW,R0,RX
    MOV R4,2
    SUB R4, RZ, R4
    ADD R0,R4,RW
    MOV RX,1
    ADD RW,R0,RX
    POP RX
    ST RX, RW, 0
    MOV RW,30
    PUSH RW
    MOV RW,1
    MOV RX,2
    MUL R0,RW,RX
    MOV R4,2
    SUB R4, RZ, R4
    ADD RW,R4,R0
    MOV RX,0
    ADD RX,RW,RX
    POP RY
    ST RY, RX, 0
    LD R0, RZ, -9
    PUSH R0
    LD RX, RZ, -6
    POP RW
    ADD RY,RW,RX
    PUSH RY
    LD RX, RZ, -5
    POP RW
    ADD R0,RW,RX
    PUSH R0
    LD RX, RZ, -4
    POP RW
    ADD RY,RW,RX
    PUSH RY
    MOV RX,0
    PUSH RX
    MOV RW,2
    POP R0
    MUL RW,R0,RW
    MOV R4,2
    SUB R4, RZ, R4
    ADD RX,R4,RW
    MOV R0,0
    ADD RW,RX,R0
    LD RX,RW, 0
    POP RW
    ADD R0,RW,RX
    PUSH R0
    MOV RX,0
    PUSH RX
    MOV RW,2
    POP RX
    MUL RY,RX,RW
    MOV R4,2
    SUB R4, RZ, R4
    ADD RX,R4,RY
    MOV RX,1
    ADD RW,RX,RX
    LD R0,RW, 0
    POP RY
    ADD RW,RY,R0
    PUSH RW
    MOV R0,1
    PUSH R0
    MOV RY,2
    POP RX
    MUL RX,RX,RY
    MOV R4,2
    SUB R4, RZ, R4
    ADD R0,R4,RX
    MOV RX,0
    ADD RY,R0,RX
    LD RW,RY, 0
    POP RX
    ADD RY,RX,RW
    MOV R2,RY
    LIMM RW,epilogue_3
    JMP RW
epilogue_3:
    LIMM RX,9
    S.ADD SP, SP, RX
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
