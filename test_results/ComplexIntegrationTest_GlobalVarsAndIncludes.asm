; T→T3
__entry:
    LIMM HP,699050
    LIMM R1,main
    CALL R1
    HALT
gcd:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RW, RZ, -2
    STOREI RX, RZ, -1
    LOADI RW, RZ, -1
    LI RX,0
    CMP RW,RX
    LIMM RY,then_2
    JE RY
    LIMM R0,end_1
    JMP R0
then_2:
    LOADI RW, RZ, -2
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
end_1:
    LOADI RY, RZ, -1
    LOADI R0, RZ, -2
    PUSH R0
    LOADI RW, RZ, -1
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
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
lcm:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RW, RZ, -2
    STOREI RX, RZ, -1
    LOADI RW, RZ, -2
    PUSH RW
    LOADI RX, RZ, -1
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LOADI RX, RZ, -2
    LOADI RY, RZ, -1
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
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
fib:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RW, RZ, -1
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_6
    JLE RY
    LIMM R0,end_5
    JMP R0
then_6:
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RX,epilogue_4
    JMP RX
end_5:
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
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
    LOADI RW, RZ, -1
    PUSH RW
    LI RY,2
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
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
fact:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RW, RZ, -1
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_9
    JLE RY
    LIMM R0,end_8
    JMP R0
then_9:
    LI RW,1
    MOV R2,RW
    LIMM RX,epilogue_7
    JMP RX
end_8:
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -1
    PUSH R0
    LI RW,1
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
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
gcd:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RW, RZ, -2
    STOREI RX, RZ, -1
    LOADI RW, RZ, -1
    LI RX,0
    CMP RW,RX
    LIMM RY,then_12
    JE RY
    LIMM R0,end_11
    JMP R0
then_12:
    LOADI RW, RZ, -2
    MOV R2,RW
    LIMM RX,epilogue_10
    JMP RX
end_11:
    LOADI RY, RZ, -1
    LOADI R0, RZ, -2
    PUSH R0
    LOADI RW, RZ, -1
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
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
lcm:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RW, RZ, -2
    STOREI RX, RZ, -1
    LOADI RW, RZ, -2
    PUSH RW
    LOADI RX, RZ, -1
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LOADI RX, RZ, -2
    LOADI RY, RZ, -1
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
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
fib:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RW, RZ, -1
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_16
    JLE RY
    LIMM R0,end_15
    JMP R0
then_16:
    LOADI RW, RZ, -1
    MOV R2,RW
    LIMM RX,epilogue_14
    JMP RX
end_15:
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
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
    LOADI RW, RZ, -1
    PUSH RW
    LI RY,2
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
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
fact:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 1
    STOREI RW, RZ, -1
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_19
    JLE RY
    LIMM R0,end_18
    JMP R0
then_19:
    LI RW,1
    MOV R2,RW
    LIMM RX,epilogue_17
    JMP RX
end_18:
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -1
    PUSH R0
    LI RW,1
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
    ADDI SP, SP, 1
    POP R4
    POP R3
    POP RZ
    RET
sieve:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 3
    STOREI RW, RZ, -1
    LI RW,2
    STOREI RW, RZ, -3
loop_21:
    LOADI RX, RZ, -3
    LOADI RY, RZ, -1
    CMP RX,RY
    LIMM R0,body_22
    JLE R0
    LIMM RW,wend_23
    JMP RW
body_22:
    LI RX,1
    PUSH RX
    LOADI RY, RZ, -3
    LIMM R4, primes
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LOADI RY, RZ, -3
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -3
    LIMM R0,loop_21
    JMP R0
wend_23:
    LI RW,2
    STOREI RW, RZ, -3
loop_24:
    LOADI RY, RZ, -3
    PUSH RY
    LOADI R0, RZ, -3
    POP RW
    MUL RX,RW,R0
    LOADI R0, RZ, -1
    CMP RX,R0
    LIMM RW,body_25
    JLE RW
    LIMM RY,wend_26
    JMP RY
body_25:
    LOADI R0, RZ, -3
    LIMM R4, primes
    ADD R4,R4,R0
    LOADI R0,R4, 0
    LI RW,1
    CMP R0,RW
    LIMM RX,then_28
    JE RX
    LIMM RY,end_27
    JMP RY
then_28:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI RW, RZ, -3
    POP RX
    MUL RY,RX,RW
    STOREI RY, RZ, -2
loop_29:
    LOADI RW, RZ, -2
    LOADI RX, RZ, -1
    CMP RW,RX
    LIMM R0,body_30
    JLE R0
    LIMM RW,wend_31
    JMP RW
body_30:
    LI RX,0
    PUSH RX
    LOADI RY, RZ, -2
    LIMM R4, primes
    ADD R4,R4,RY
    POP R0
    STOREI R0, R4, 0
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R0, RZ, -3
    POP RW
    ADD RX,RW,R0
    STOREI RX, RZ, -2
    LIMM R0,loop_29
    JMP R0
wend_31:
end_27:
    LOADI RW, RZ, -3
    PUSH RW
    LI RY,1
    POP R0
    ADD RW,R0,RY
    STOREI RW, RZ, -3
    LIMM RY,loop_24
    JMP RY
wend_26:
    LI R0,0
    LIMM R4, primeCount
    STOREI R0, R4, 0
    LI RX,2
    STOREI RX, RZ, -3
loop_32:
    LOADI RY, RZ, -3
    LOADI R0, RZ, -1
    CMP RY,R0
    LIMM RW,body_33
    JLE RW
    LIMM RX,wend_34
    JMP RX
body_33:
    LOADI RY, RZ, -3
    LIMM R4, primes
    ADD R4,R4,RY
    LOADI RY,R4, 0
    LI R0,1
    CMP RY,R0
    LIMM RW,then_36
    JE RW
    LIMM RX,end_35
    JMP RX
then_36:
    LIMM R4, primeCount
    LOADI RY, R4, 0
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    LIMM R4, primeCount
    STOREI RX, R4, 0
end_35:
    LOADI R0, RZ, -3
    PUSH R0
    LI RW,1
    POP RY
    ADD R0,RY,RW
    STOREI R0, RZ, -3
    LIMM RW,loop_32
    JMP RW
wend_34:
    LIMM R4, primeCount
    LOADI RY, R4, 0
    MOV R2,RY
    LIMM RW,epilogue_20
    JMP RW
epilogue_20:
    ADDI SP, SP, 3
    POP R4
    POP R3
    POP RZ
    RET
isPrime:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RW, RZ, -1
    LOADI RW, RZ, -1
    LI RX,2
    CMP RW,RX
    LIMM RY,then_39
    JL RY
    LIMM R0,end_38
    JMP R0
then_39:
    LI RW,0
    MOV R2,RW
    LIMM RX,epilogue_37
    JMP RX
end_38:
    LI RY,2
    STOREI RY, RZ, -2
loop_40:
    LOADI R0, RZ, -2
    PUSH R0
    LOADI RW, RZ, -2
    POP RX
    MUL RY,RX,RW
    LOADI RW, RZ, -1
    CMP RY,RW
    LIMM RX,body_41
    JLE RX
    LIMM R0,wend_42
    JMP R0
body_41:
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -2
    POP RY
    MOD R0,RY,RX
    LI RX,0
    CMP R0,RX
    LIMM RY,then_44
    JE RY
    LIMM RW,end_43
    JMP RW
then_44:
    LI RX,0
    MOV R2,RX
    LIMM RY,epilogue_37
    JMP RY
end_43:
    LOADI R0, RZ, -2
    PUSH R0
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -2
    LIMM RW,loop_40
    JMP RW
wend_42:
    LI RX,1
    MOV R2,RX
    LIMM R0,epilogue_37
    JMP R0
epilogue_37:
    ADDI SP, SP, 2
    POP R4
    POP R3
    POP RZ
    RET
sumPrimes:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 3
    STOREI RW, RZ, -1
    LI RW,0
    STOREI RW, RZ, -3
    LI RX,2
    STOREI RX, RZ, -2
loop_46:
    LOADI RY, RZ, -2
    LOADI R0, RZ, -1
    CMP RY,R0
    LIMM RW,body_47
    JLE RW
    LIMM RX,wend_48
    JMP RX
body_47:
    LOADI RY, RZ, -2
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
    LI RW,1
    CMP R0,RW
    LIMM RX,then_50
    JE RX
    LIMM RY,end_49
    JMP RY
then_50:
    LOADI R0, RZ, -3
    PUSH R0
    LOADI RW, RZ, -2
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -3
end_49:
    LOADI RW, RZ, -2
    PUSH RW
    LI RX,1
    POP R0
    ADD RW,R0,RX
    STOREI RW, RZ, -2
    LIMM RX,loop_46
    JMP RX
wend_48:
    LOADI R0, RZ, -3
    MOV R2,R0
    LIMM RX,epilogue_45
    JMP RX
epilogue_45:
    ADDI SP, SP, 3
    POP R4
    POP R3
    POP RZ
    RET
main:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 3
    LI RW,50
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
    STOREI RX, RZ, -3
    LI RY,48
    LI R0,180
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
    STOREI RW, RZ, -2
    LI RX,10
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
    STOREI RY, RZ, -1
    LOADI R0, RZ, -3
    PUSH R0
    LOADI RW, RZ, -2
    POP RX
    ADD RY,RX,RW
    PUSH RY
    LOADI RW, RZ, -1
    POP RX
    ADD R0,RX,RW
    MOV R2,R0
    LIMM RW,epilogue_51
    JMP RW
epilogue_51:
    ADDI SP, SP, 3
    POP R4
    POP R3
    POP RZ
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
