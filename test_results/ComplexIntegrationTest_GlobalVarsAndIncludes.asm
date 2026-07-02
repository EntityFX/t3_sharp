; T→T3
__entry:
    S.LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
gcd:
    PUSH R3
    PUSH R4
    LIMM R3,2
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -4
    ST RX, RZ, -3
    LD RW, RZ, -3
    MOV RX,0
    CMP RW,RX
    LIMM RY,then_2
    JE RY
    LIMM R0,end_1
    JMP R0
then_2:
    LD RW, RZ, -4
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
end_1:
    LD RY, RZ, -3
    LD R0, RZ, -4
    PUSH R0
    LD RW, RZ, -3
    POP RX
    MOD RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    MOV RX,RY
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    LIMM R0,2
    S.ADD SP, SP, R0
    POP R4
    POP R3
    RET
lcm:
    PUSH R3
    PUSH R4
    LIMM R3,2
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -4
    ST RX, RZ, -3
    LD RW, RZ, -4
    PUSH RW
    LD RX, RZ, -3
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LD RX, RZ, -4
    LD RY, RZ, -3
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    MOV RX,RY
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    DIV RY,RX,RW
    MOV R2,RY
    LIMM RW,epilogue_3
    JMP RW
epilogue_3:
    LIMM RX,2
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET
fib:
    PUSH R3
    PUSH R4
    LIMM R3,1
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    LD RW, RZ, -3
    MOV RX,1
    CMP RW,RX
    LIMM RY,then_6
    JLE RY
    LIMM R0,end_5
    JMP R0
then_6:
    LD RW, RZ, -3
    MOV R2,RW
    LIMM RX,epilogue_4
    JMP RX
end_5:
    LD RY, RZ, -3
    PUSH RY
    MOV R0,1
    POP RW
    SUB RX,RW,R0
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    PUSH R0
    LD RW, RZ, -3
    PUSH RW
    MOV RY,2
    POP R0
    SUB RW,R0,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    POP R0
    ADD RX,R0,RY
    MOV R2,RX
    LIMM RY,epilogue_4
    JMP RY
epilogue_4:
    LIMM R0,1
    S.ADD SP, SP, R0
    POP R4
    POP R3
    RET
fact:
    PUSH R3
    PUSH R4
    LIMM R3,1
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    LD RW, RZ, -3
    MOV RX,1
    CMP RW,RX
    LIMM RY,then_9
    JLE RY
    LIMM R0,end_8
    JMP R0
then_9:
    MOV RW,1
    MOV R2,RW
    LIMM RX,epilogue_7
    JMP RX
end_8:
    LD RY, RZ, -3
    PUSH RY
    LD R0, RZ, -3
    PUSH R0
    MOV RW,1
    POP RX
    SUB RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,fact
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    MUL R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_7
    JMP RW
epilogue_7:
    LIMM RX,1
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET
gcd:
    PUSH R3
    PUSH R4
    LIMM R3,2
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -4
    ST RX, RZ, -3
    LD RW, RZ, -3
    MOV RX,0
    CMP RW,RX
    LIMM RY,then_12
    JE RY
    LIMM R0,end_11
    JMP R0
then_12:
    LD RW, RZ, -4
    MOV R2,RW
    LIMM RX,epilogue_10
    JMP RX
end_11:
    LD RY, RZ, -3
    LD R0, RZ, -4
    PUSH R0
    LD RW, RZ, -3
    POP RX
    MOD RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    MOV RX,RY
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    MOV R2,RW
    LIMM RX,epilogue_10
    JMP RX
epilogue_10:
    LIMM R0,2
    S.ADD SP, SP, R0
    POP R4
    POP R3
    RET
lcm:
    PUSH R3
    PUSH R4
    LIMM R3,2
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -4
    ST RX, RZ, -3
    LD RW, RZ, -4
    PUSH RW
    LD RX, RZ, -3
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LD RX, RZ, -4
    LD RY, RZ, -3
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    MOV RX,RY
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    DIV RY,RX,RW
    MOV R2,RY
    LIMM RW,epilogue_13
    JMP RW
epilogue_13:
    LIMM RX,2
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET
fib:
    PUSH R3
    PUSH R4
    LIMM R3,1
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    LD RW, RZ, -3
    MOV RX,1
    CMP RW,RX
    LIMM RY,then_16
    JLE RY
    LIMM R0,end_15
    JMP R0
then_16:
    LD RW, RZ, -3
    MOV R2,RW
    LIMM RX,epilogue_14
    JMP RX
end_15:
    LD RY, RZ, -3
    PUSH RY
    MOV R0,1
    POP RW
    SUB RX,RW,R0
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    PUSH R0
    LD RW, RZ, -3
    PUSH RW
    MOV RY,2
    POP R0
    SUB RW,R0,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    POP R0
    ADD RX,R0,RY
    MOV R2,RX
    LIMM RY,epilogue_14
    JMP RY
epilogue_14:
    LIMM R0,1
    S.ADD SP, SP, R0
    POP R4
    POP R3
    RET
fact:
    PUSH R3
    PUSH R4
    LIMM R3,1
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    LD RW, RZ, -3
    MOV RX,1
    CMP RW,RX
    LIMM RY,then_19
    JLE RY
    LIMM R0,end_18
    JMP R0
then_19:
    MOV RW,1
    MOV R2,RW
    LIMM RX,epilogue_17
    JMP RX
end_18:
    LD RY, RZ, -3
    PUSH RY
    LD R0, RZ, -3
    PUSH R0
    MOV RW,1
    POP RX
    SUB RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,fact
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    MUL R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_17
    JMP RW
epilogue_17:
    LIMM RX,1
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET
sieve:
    PUSH R3
    PUSH R4
    LIMM R3,3
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    MOV RW,2
    ST RW, RZ, -5
loop_21:
    LD RX, RZ, -5
    LD RY, RZ, -3
    CMP RX,RY
    LIMM R0,body_22
    JLE R0
    LIMM RW,wend_23
    JMP RW
body_22:
    MOV RX,1
    PUSH RX
    LD RY, RZ, -5
    LIMM R4, primes
    ADD R4,R4,RY
    POP R0
    ST R0, R4, 0
    LD RY, RZ, -5
    PUSH RY
    MOV R0,1
    POP RW
    ADD RX,RW,R0
    ST RX, RZ, -5
    LIMM R0,loop_21
    JMP R0
wend_23:
    MOV RW,2
    ST RW, RZ, -5
loop_24:
    LD RY, RZ, -5
    PUSH RY
    LD R0, RZ, -5
    POP RW
    MUL RX,RW,R0
    LD R0, RZ, -3
    CMP RX,R0
    LIMM RW,body_25
    JLE RW
    LIMM RY,wend_26
    JMP RY
body_25:
    LD R0, RZ, -5
    LIMM R4, primes
    ADD R4,R4,R0
    LD R0,R4, 0
    MOV RW,1
    CMP R0,RW
    LIMM RX,then_28
    JE RX
    LIMM RY,end_27
    JMP RY
then_28:
    LD R0, RZ, -5
    PUSH R0
    LD RW, RZ, -5
    POP RX
    MUL RY,RX,RW
    ST RY, RZ, -4
loop_29:
    LD RW, RZ, -4
    LD RX, RZ, -3
    CMP RW,RX
    LIMM R0,body_30
    JLE R0
    LIMM RW,wend_31
    JMP RW
body_30:
    MOV RX,0
    PUSH RX
    LD RY, RZ, -4
    LIMM R4, primes
    ADD R4,R4,RY
    POP R0
    ST R0, R4, 0
    LD RY, RZ, -4
    PUSH RY
    LD R0, RZ, -5
    POP RW
    ADD RX,RW,R0
    ST RX, RZ, -4
    LIMM R0,loop_29
    JMP R0
wend_31:
end_27:
    LD RW, RZ, -5
    PUSH RW
    MOV RY,1
    POP R0
    ADD RW,R0,RY
    ST RW, RZ, -5
    LIMM RY,loop_24
    JMP RY
wend_26:
    MOV R0,0
    LIMM R4, primeCount
    ST R0, R4, 0
    MOV RX,2
    ST RX, RZ, -5
loop_32:
    LD RY, RZ, -5
    LD R0, RZ, -3
    CMP RY,R0
    LIMM RW,body_33
    JLE RW
    LIMM RX,wend_34
    JMP RX
body_33:
    LD RY, RZ, -5
    LIMM R4, primes
    ADD R4,R4,RY
    LD RY,R4, 0
    MOV R0,1
    CMP RY,R0
    LIMM RW,then_36
    JE RW
    LIMM RX,end_35
    JMP RX
then_36:
    LIMM R4, primeCount
    LD RY, R4, 0
    PUSH RY
    MOV R0,1
    POP RW
    ADD RX,RW,R0
    LIMM R4, primeCount
    ST RX, R4, 0
end_35:
    LD R0, RZ, -5
    PUSH R0
    MOV RW,1
    POP RY
    ADD R0,RY,RW
    ST R0, RZ, -5
    LIMM RW,loop_32
    JMP RW
wend_34:
    LIMM R4, primeCount
    LD RY, R4, 0
    MOV R2,RY
    LIMM RW,epilogue_20
    JMP RW
epilogue_20:
    LIMM RX,3
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET
isPrime:
    PUSH R3
    PUSH R4
    LIMM R3,2
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    LD RW, RZ, -3
    MOV RX,2
    CMP RW,RX
    LIMM RY,then_39
    JL RY
    LIMM R0,end_38
    JMP R0
then_39:
    MOV RW,0
    MOV R2,RW
    LIMM RX,epilogue_37
    JMP RX
end_38:
    MOV RY,2
    ST RY, RZ, -4
loop_40:
    LD R0, RZ, -4
    PUSH R0
    LD RW, RZ, -4
    POP RX
    MUL RY,RX,RW
    LD RW, RZ, -3
    CMP RY,RW
    LIMM RX,body_41
    JLE RX
    LIMM R0,wend_42
    JMP R0
body_41:
    LD RW, RZ, -3
    PUSH RW
    LD RX, RZ, -4
    POP RY
    MOD R0,RY,RX
    MOV RX,0
    CMP R0,RX
    LIMM RY,then_44
    JE RY
    LIMM RW,end_43
    JMP RW
then_44:
    MOV RX,0
    MOV R2,RX
    LIMM RY,epilogue_37
    JMP RY
end_43:
    LD R0, RZ, -4
    PUSH R0
    MOV RW,1
    POP RX
    ADD RY,RX,RW
    ST RY, RZ, -4
    LIMM RW,loop_40
    JMP RW
wend_42:
    MOV RX,1
    MOV R2,RX
    LIMM R0,epilogue_37
    JMP R0
epilogue_37:
    LIMM RW,2
    S.ADD SP, SP, RW
    POP R4
    POP R3
    RET
sumPrimes:
    PUSH R3
    PUSH R4
    LIMM R3,3
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    ST RW, RZ, -3
    MOV RW,0
    ST RW, RZ, -5
    MOV RX,2
    ST RX, RZ, -4
loop_46:
    LD RY, RZ, -4
    LD R0, RZ, -3
    CMP RY,R0
    LIMM RW,body_47
    JLE RW
    LIMM RX,wend_48
    JMP RX
body_47:
    LD RY, RZ, -4
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,isPrime
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    MOV RW,1
    CMP R0,RW
    LIMM RX,then_50
    JE RX
    LIMM RY,end_49
    JMP RY
then_50:
    LD R0, RZ, -5
    PUSH R0
    LD RW, RZ, -4
    POP RX
    ADD RY,RX,RW
    ST RY, RZ, -5
end_49:
    LD RW, RZ, -4
    PUSH RW
    MOV RX,1
    POP R0
    ADD RW,R0,RX
    ST RW, RZ, -4
    LIMM RX,loop_46
    JMP RX
wend_48:
    LD R0, RZ, -5
    MOV R2,R0
    LIMM RX,epilogue_45
    JMP RX
epilogue_45:
    LIMM RY,3
    S.ADD SP, SP, RY
    POP R4
    POP R3
    RET
main:
    PUSH R3
    PUSH R4
    LIMM R3,3
    S.SUB SP, SP, R3
    S.MOV RZ, FP
    MOV RW,50
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,sieve
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RX,R2
    ST RX, RZ, -5
    MOV RY,48
    MOV R0,180
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    MOV RX,R0
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    ST RW, RZ, -4
    MOV RX,10
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    ST RY, RZ, -3
    LD R0, RZ, -5
    PUSH R0
    LD RW, RZ, -4
    POP RX
    ADD RY,RX,RW
    PUSH RY
    LD RW, RZ, -3
    POP RX
    ADD R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_51
    JMP RW
epilogue_51:
    LIMM RX,3
    S.ADD SP, SP, RX
    POP R4
    POP R3
    RET

; --- Global Variables ---
primes: .word 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
primeCount: .word 0

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
