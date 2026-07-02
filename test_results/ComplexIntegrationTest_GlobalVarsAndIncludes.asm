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
    LIMM RZ,end_1
    JMP RZ
then_2:
    LOADI R0, RZ, -2
    MOV R2,R0
    LIMM RW,epilogue_0
    JMP RW
end_1:
    LOADI RX, RZ, -1
    LOADI RY, RZ, -2
    PUSH RY
    LOADI RZ, RZ, -1
    POP R0
    MOD RW,R0,RZ
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RZ,RW
    MOV RW,RX
    MOV RX,RZ
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RZ,R2
    MOV R2,RZ
    LIMM R0,epilogue_0
    JMP R0
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
    MUL RZ,RY,RX
    PUSH RZ
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
    MOV R0,R2
    POP RW
    DIV RX,RW,R0
    MOV R2,RX
    LIMM R0,epilogue_3
    JMP R0
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
    LIMM RZ,end_5
    JMP RZ
then_6:
    LOADI R0, RZ, -1
    MOV R2,R0
    LIMM RW,epilogue_4
    JMP RW
end_5:
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP RZ
    SUB R0,RZ,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R0
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    PUSH RY
    LOADI RZ, RZ, -1
    PUSH RZ
    LI RW,2
    POP RX
    SUB RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    ADD RZ,RX,RW
    MOV R2,RZ
    LIMM RW,epilogue_4
    JMP RW
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
    LIMM RZ,end_8
    JMP RZ
then_9:
    LI R0,1
    MOV R2,R0
    LIMM RW,epilogue_7
    JMP RW
end_8:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -1
    PUSH RY
    LI RZ,1
    POP R0
    SUB RW,R0,RZ
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fact
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RZ,R2
    POP R0
    MUL RX,R0,RZ
    MOV R2,RX
    LIMM RZ,epilogue_7
    JMP RZ
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
    LIMM RZ,end_11
    JMP RZ
then_12:
    LOADI R0, RZ, -2
    MOV R2,R0
    LIMM RW,epilogue_10
    JMP RW
end_11:
    LOADI RX, RZ, -1
    LOADI RY, RZ, -2
    PUSH RY
    LOADI RZ, RZ, -1
    POP R0
    MOD RW,R0,RZ
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RZ,RW
    MOV RW,RX
    MOV RX,RZ
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RZ,R2
    MOV R2,RZ
    LIMM R0,epilogue_10
    JMP R0
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
    MUL RZ,RY,RX
    PUSH RZ
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
    MOV R0,R2
    POP RW
    DIV RX,RW,R0
    MOV R2,RX
    LIMM R0,epilogue_13
    JMP R0
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
    LIMM RZ,end_15
    JMP RZ
then_16:
    LOADI R0, RZ, -1
    MOV R2,R0
    LIMM RW,epilogue_14
    JMP RW
end_15:
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP RZ
    SUB R0,RZ,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R0
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    PUSH RY
    LOADI RZ, RZ, -1
    PUSH RZ
    LI RW,2
    POP RX
    SUB RY,RX,RW
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RW,R2
    POP RX
    ADD RZ,RX,RW
    MOV R2,RZ
    LIMM RW,epilogue_14
    JMP RW
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
    LIMM RZ,end_18
    JMP RZ
then_19:
    LI R0,1
    MOV R2,R0
    LIMM RW,epilogue_17
    JMP RW
end_18:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -1
    PUSH RY
    LI RZ,1
    POP R0
    SUB RW,R0,RZ
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    LIMM R1,fact
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RZ,R2
    POP R0
    MUL RX,R0,RZ
    MOV R2,RX
    LIMM RZ,epilogue_17
    JMP RZ
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
    LIMM RZ,body_22
    JLE RZ
    LIMM R0,wend_23
    JMP R0
body_22:
    LI RW,1
    PUSH RW
    LOADI RX, RZ, -3
    LIMM R4, primes
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LOADI RX, RZ, -3
    PUSH RX
    LI RY,1
    POP RZ
    ADD R0,RZ,RY
    STOREI R0, RZ, -3
    LIMM RY,loop_21
    JMP RY
wend_23:
    LI RZ,2
    STOREI RZ, RZ, -3
loop_24:
    LOADI RW, RZ, -3
    PUSH RW
    LOADI RX, RZ, -3
    POP RY
    MUL RZ,RY,RX
    LOADI RX, RZ, -1
    CMP RZ,RX
    LIMM RY,body_25
    JLE RY
    LIMM R0,wend_26
    JMP R0
body_25:
    LOADI RW, RZ, -3
    LIMM R4, primes
    ADD R4,R4,RW
    LOADI RW,R4, 0
    LI RX,1
    CMP RW,RX
    LIMM RY,then_28
    JE RY
    LIMM RZ,end_27
    JMP RZ
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
    LIMM RZ,body_30
    JLE RZ
    LIMM R0,wend_31
    JMP R0
body_30:
    LI RW,0
    PUSH RW
    LOADI RX, RZ, -2
    LIMM R4, primes
    ADD R4,R4,RX
    POP RY
    STOREI RY, R4, 0
    LOADI RX, RZ, -2
    PUSH RX
    LOADI RY, RZ, -3
    POP RZ
    ADD R0,RZ,RY
    STOREI R0, RZ, -2
    LIMM RY,loop_29
    JMP RY
wend_31:
end_27:
    LOADI RZ, RZ, -3
    PUSH RZ
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RY, RZ, -3
    LIMM RW,loop_24
    JMP RW
wend_26:
    LI RX,0
    LIMM R4, primeCount
    STOREI RX, R4, 0
    LI RZ,2
    STOREI RZ, RZ, -3
loop_32:
    LOADI R0, RZ, -3
    LOADI RW, RZ, -1
    CMP R0,RW
    LIMM RX,body_33
    JLE RX
    LIMM RY,wend_34
    JMP RY
body_33:
    LOADI RZ, RZ, -3
    LIMM R4, primes
    ADD R4,R4,RZ
    LOADI RZ,R4, 0
    LI R0,1
    CMP RZ,R0
    LIMM RW,then_36
    JE RW
    LIMM RX,end_35
    JMP RX
then_36:
    LIMM R4, primeCount
    LOADI RY, R4, 0
    PUSH RY
    LI RZ,1
    POP R0
    ADD RW,R0,RZ
    LIMM R4, primeCount
    STOREI RW, R4, 0
end_35:
    LOADI RZ, RZ, -3
    PUSH RZ
    LI R0,1
    POP RX
    ADD RY,RX,R0
    STOREI RY, RZ, -3
    LIMM R0,loop_32
    JMP R0
wend_34:
    LIMM R4, primeCount
    LOADI RX, R4, 0
    MOV R2,RX
    LIMM RZ,epilogue_20
    JMP RZ
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
    LIMM RZ,end_38
    JMP RZ
then_39:
    LI R0,0
    MOV R2,R0
    LIMM RW,epilogue_37
    JMP RW
end_38:
    LI RX,2
    STOREI RX, RZ, -2
loop_40:
    LOADI RY, RZ, -2
    PUSH RY
    LOADI RZ, RZ, -2
    POP R0
    MUL RW,R0,RZ
    LOADI RZ, RZ, -1
    CMP RW,RZ
    LIMM R0,body_41
    JLE R0
    LIMM RX,wend_42
    JMP RX
body_41:
    LOADI RY, RZ, -1
    PUSH RY
    LOADI RZ, RZ, -2
    POP R0
    MOD RW,R0,RZ
    LI RZ,0
    CMP RW,RZ
    LIMM R0,then_44
    JE R0
    LIMM RX,end_43
    JMP RX
then_44:
    LI RY,0
    MOV R2,RY
    LIMM RZ,epilogue_37
    JMP RZ
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
    LIMM RZ,epilogue_37
    JMP RZ
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
    LOADI RZ, RZ, -1
    CMP RY,RZ
    LIMM R0,body_47
    JLE R0
    LIMM RW,wend_48
    JMP RW
body_47:
    LOADI RX, RZ, -2
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RX
    LIMM R1,isPrime
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    LI RZ,1
    CMP RY,RZ
    LIMM R0,then_50
    JE R0
    LIMM RW,end_49
    JMP RW
then_50:
    LOADI RX, RZ, -3
    PUSH RX
    LOADI RY, RZ, -2
    POP RZ
    ADD R0,RZ,RY
    STOREI R0, RZ, -3
end_49:
    LOADI RY, RZ, -2
    PUSH RY
    LI RZ,1
    POP RW
    ADD RX,RW,RZ
    STOREI RX, RZ, -2
    LIMM RZ,loop_46
    JMP RZ
wend_48:
    LOADI RW, RZ, -3
    MOV R2,RW
    LIMM RY,epilogue_45
    JMP RY
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
    LI RZ,180
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,RY
    MOV RX,RZ
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    STOREI R0, RZ, -2
    LI RW,10
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
    MOV RX,R2
    STOREI RX, RZ, -1
    LOADI RY, RZ, -3
    PUSH RY
    LOADI RZ, RZ, -2
    POP R0
    ADD RW,R0,RZ
    PUSH RW
    LOADI RZ, RZ, -1
    POP R0
    ADD RX,R0,RZ
    MOV R2,RX
    LIMM RZ,epilogue_51
    JMP RZ
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
