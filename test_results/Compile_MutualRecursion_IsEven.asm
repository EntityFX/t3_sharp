; T→T3
__entry:
    LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
isEven:
    PUSH R3
    PUSH R4
    S.MOV RZ, FP
    SUB SP, SP, 1
    ST RW, RZ, -1
    LD RW, RZ, -1
    MOV RX,0
    CMP RW,RX
    LIMM RY,then_2
    JE RY
    LIMM RZ,end_1
    JMP RZ
then_2:
    MOV R0,1
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
end_1:
    LD RX, RZ, -1
    PUSH RX
    MOV RY,1
    POP RZ
    SUB R0,RZ,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R0
    LIMM R1,isOdd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    MOV R2,RY
    LIMM RZ,epilogue_0
    JMP RZ
epilogue_0:
    ADD SP, SP, 1
    POP R4
    POP R3
    RET
isOdd:
    PUSH R3
    PUSH R4
    S.MOV RZ, FP
    SUB SP, SP, 1
    ST RW, RZ, -1
    LD RW, RZ, -1
    MOV RX,0
    CMP RW,RX
    LIMM RY,then_5
    JE RY
    LIMM RZ,end_4
    JMP RZ
then_5:
    MOV R0,0
    MOV R2,R0
    LIMM RW,epilogue_3
    JMP RW
end_4:
    LD RX, RZ, -1
    PUSH RX
    MOV RY,1
    POP RZ
    SUB R0,RZ,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R0
    LIMM R1,isEven
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    MOV R2,RY
    LIMM RZ,epilogue_3
    JMP RZ
epilogue_3:
    ADD SP, SP, 1
    POP R4
    POP R3
    RET
main:
    PUSH R3
    PUSH R4
    S.MOV RZ, FP
    MOV RW,10
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,isEven
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    MOV R2,RX
    LIMM RY,epilogue_6
    JMP RY
epilogue_6:
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
