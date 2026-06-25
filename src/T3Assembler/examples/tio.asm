; T3-18 I/O Library — 9 registers: RW(-4) RX(-3) RY(-2) RZ(-1) R0(0) R1(1) R2(2) R3(3) R4(4)
PORT_OUT equ 0

putchar:
    OUTI R0, PORT_OUT
    RET

printstring:
    POP R4               ; save ret addr in R4
    MOV R1, R0
ps_loop:
    LOAD R0, R1
    CMPI R0, 0
    JE ps_end
    MOV R0, R0            ; R0 has the char
    PUSH R1
    PUSH R4
    CALL putchar
    POP R4
    POP R1
    ADDI R1, 1
    JMP ps_loop
ps_end:
    PUSH R4               ; restore ret addr
    RET

printint:
    POP R4               ; save ret addr in R4
    CMPI R0, 0
    JGE pi_div
    PUSH R0
    LI R0, 45
    PUSH R4
    CALL putchar
    POP R4
    POP R0
    NEG R0
pi_div:
    LI R1, 10
    LI R3, 0
pi_divloop:
    CMPI R0, 0
    JE pi_pop
    DIV R2, R0, R1
    MOD R0, R0, R1
    PUSH R0              ; push remainder digit
    ADDI R3, 1
    MOV R0, R2           ; R0 = quotient
    JMP pi_divloop
pi_pop:
    CMPI R3, 0
    JE pi_exit
    POP R0
    ADDI R0, 48
    PUSH R3
    PUSH R4
    CALL putchar
    POP R4
    POP R3
    SUBI R3, 1
    JMP pi_pop
pi_exit:
    PUSH R4              ; restore ret addr
    RET

printfloat:
    POP R4               ; save ret addr
    FTOI R0, F0, 0
    PUSH R4
    CALL printint
    POP R4
    
    LI R0, 46
    PUSH R4
    CALL putchar
    POP R4
    
    FMOV F1, F0, 0
    FTOI R0, F0, 0
    FMOV F2, R0, 1
    FSUB F1, F1, F2
    LI R2, 10
    FMOV F2, R2, 1
    LI R3, 0
pf_loop:
    CMPI R3, 6
    JGE pf_end
    FMUL F3, F1, F2
    FTOI R0, F3, 0
    ADDI R0, 48
    PUSH R3
    PUSH R4
    CALL putchar
    POP R4
    POP R3
    FMOV F3, R0, 1
    FMUL F4, F1, F2
    FSUB F1, F4, F3
    ADDI R3, 1
    JMP pf_loop
pf_end:
    PUSH R4              ; restore ret addr
    RET