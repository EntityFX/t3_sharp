; T→T3
__entry:
    S.LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
main:
    PUSH R3
    PUSH R4
    S.MOV RZ, SP
    MOV RW,5
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
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
