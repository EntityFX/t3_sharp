; T→T3
__entry:
    LIMM R1,main
    CALL R1
    HALT
gcd:
    PUSH RZ
    PUSH R3
    PUSH R4
    MOV RZ, SP
    SUBI SP, SP, 2
    STOREI RZ, -1, RW
    STOREI RZ, -2, RX
    LOADI RW, RZ, -2
    LI RX,0
    CMP RW,RX
    LIMM RY,then_2
    JE RY
    LIMM R0,end_1
    JMP R0
then_2:
    LOADI R3, RZ, -1
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
end_1:
    LOADI RX, RZ, -2
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -2
    POP R3
    MOD RW,R3,R0
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV R0,RW
    MOV RW,RX
    MOV RX,R0
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    MOV R2,R0
    LIMM R3,epilogue_0
    JMP R3
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
    STOREI RZ, -1, RW
    STOREI RZ, -2, RX
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -2
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LOADI RX, RZ, -1
    LOADI RY, RZ, -2
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
    MOV R3,R2
    POP RW
    DIV RX,RW,R3
    MOV R2,RX
    LIMM R3,epilogue_3
    JMP R3
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
    STOREI RZ, -1, RW
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_6
    JLE RY
    LIMM R0,end_5
    JMP R0
then_6:
    LOADI R3, RZ, -1
    MOV R2,R3
    LIMM RW,epilogue_4
    JMP RW
end_5:
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP R0
    SUB R3,R0,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R3
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    PUSH RY
    LOADI R0, RZ, -1
    PUSH R0
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
    ADD R0,RX,RW
    MOV R2,R0
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
    STOREI RZ, -1, RW
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_9
    JLE RY
    LIMM R0,end_8
    JMP R0
then_9:
    LI R3,1
    MOV R2,R3
    LIMM RW,epilogue_7
    JMP RW
end_8:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
    POP R3
    SUB RW,R3,R0
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
    MOV R0,R2
    POP R3
    MUL RX,R3,R0
    MOV R2,RX
    LIMM R0,epilogue_7
    JMP R0
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
    STOREI RZ, -1, RW
    STOREI RZ, -2, RX
    LOADI RW, RZ, -2
    LI RX,0
    CMP RW,RX
    LIMM RY,then_12
    JE RY
    LIMM R0,end_11
    JMP R0
then_12:
    LOADI R3, RZ, -1
    MOV R2,R3
    LIMM RW,epilogue_10
    JMP RW
end_11:
    LOADI RX, RZ, -2
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -2
    POP R3
    MOD RW,R3,R0
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV R0,RW
    MOV RW,RX
    MOV RX,R0
    LIMM R1,gcd
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    MOV R2,R0
    LIMM R3,epilogue_10
    JMP R3
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
    STOREI RZ, -1, RW
    STOREI RZ, -2, RX
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -2
    POP RY
    MUL R0,RY,RX
    PUSH R0
    LOADI RX, RZ, -1
    LOADI RY, RZ, -2
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
    MOV R3,R2
    POP RW
    DIV RX,RW,R3
    MOV R2,RX
    LIMM R3,epilogue_13
    JMP R3
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
    STOREI RZ, -1, RW
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_16
    JLE RY
    LIMM R0,end_15
    JMP R0
then_16:
    LOADI R3, RZ, -1
    MOV R2,R3
    LIMM RW,epilogue_14
    JMP RW
end_15:
    LOADI RX, RZ, -1
    PUSH RX
    LI RY,1
    POP R0
    SUB R3,R0,RY
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH R0
    PUSH R1
    MOV RW,R3
    LIMM R1,fib
    CALL R1
    POP R1
    POP R0
    POP RY
    POP RX
    POP RW
    MOV RY,R2
    PUSH RY
    LOADI R0, RZ, -1
    PUSH R0
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
    ADD R0,RX,RW
    MOV R2,R0
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
    STOREI RZ, -1, RW
    LOADI RW, RZ, -1
    LI RX,1
    CMP RW,RX
    LIMM RY,then_19
    JLE RY
    LIMM R0,end_18
    JMP R0
then_19:
    LI R3,1
    MOV R2,R3
    LIMM RW,epilogue_17
    JMP RW
end_18:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -1
    PUSH RY
    LI R0,1
    POP R3
    SUB RW,R3,R0
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
    MOV R0,R2
    POP R3
    MUL RX,R3,R0
    MOV R2,RX
    LIMM R0,epilogue_17
    JMP R0
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
    STOREI RZ, -3, RW
    LI RW,2
    STOREI RZ, -1, RW
loop_21:
    LOADI RX, RZ, -1
    LOADI RY, RZ, -3
    CMP RX,RY
    LIMM R0,body_22
    JLE R0
    LIMM R3,wend_23
    JMP R3
body_22:
    LI RW,1
    LOADI RX, RZ, -1
    LI R4,0
    ADD R4,R4,RX
    PUSH RW
    POP RY
    STOREI R4, 0, RY
    LOADI RY, RZ, -1
    PUSH RY
    LI RX,1
    POP R0
    ADD R3,R0,RX
    STOREI RZ, -1, R3
    LIMM RX,loop_21
    JMP RX
wend_23:
    LI R0,2
    STOREI RZ, -1, R0
loop_24:
    LOADI RW, RZ, -1
    PUSH RW
    LOADI RX, RZ, -1
    POP RY
    MUL R0,RY,RX
    LOADI RX, RZ, -3
    CMP R0,RX
    LIMM RY,body_25
    JLE RY
    LIMM R3,wend_26
    JMP R3
body_25:
    LOADI RX, RZ, -1
    LI R4,0
    ADD R4,R4,RX
    LOADI RW,R4, 0
    LI RX,1
    CMP RW,RX
    LIMM RY,then_28
    JE RY
    LIMM R0,end_27
    JMP R0
then_28:
    LOADI R3, RZ, -1
    PUSH R3
    LOADI RW, RZ, -1
    POP RX
    MUL RY,RX,RW
    STOREI RZ, -2, RY
loop_29:
    LOADI RW, RZ, -2
    LOADI RX, RZ, -3
    CMP RW,RX
    LIMM R0,body_30
    JLE R0
    LIMM R3,wend_31
    JMP R3
body_30:
    LI RW,0
    LOADI RX, RZ, -2
    LI R4,0
    ADD R4,R4,RX
    PUSH RW
    POP RY
    STOREI R4, 0, RY
    LOADI RY, RZ, -2
    PUSH RY
    LOADI RX, RZ, -1
    POP R0
    ADD R3,R0,RX
    STOREI RZ, -2, R3
    LIMM RX,loop_29
    JMP RX
wend_31:
end_27:
    LOADI R0, RZ, -1
    PUSH R0
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RZ, -1, RY
    LIMM RW,loop_24
    JMP RW
wend_26:
    LI RX,0
    LI R4,200
    STOREI R4, 0, RX
    LI R0,2
    STOREI RZ, -1, R0
loop_32:
    LOADI R3, RZ, -1
    LOADI RW, RZ, -3
    CMP R3,RW
    LIMM RX,body_33
    JLE RX
    LIMM RY,wend_34
    JMP RY
body_33:
    LOADI R3, RZ, -1
    LI R4,0
    ADD R4,R4,R3
    LOADI R0,R4, 0
    LI R3,1
    CMP R0,R3
    LIMM RW,then_36
    JE RW
    LIMM RX,end_35
    JMP RX
then_36:
    LI R4,200
    LOADI RY,R4, 0
    PUSH RY
    LI R0,1
    POP R3
    ADD RW,R3,R0
    LI R4,200
    STOREI R4, 0, RW
end_35:
    LOADI R0, RZ, -1
    PUSH R0
    LI R3,1
    POP RX
    ADD RY,RX,R3
    STOREI RZ, -1, RY
    LIMM R3,loop_32
    JMP R3
wend_34:
    LI R4,200
    LOADI RX,R4, 0
    MOV R2,RX
    LIMM R0,epilogue_20
    JMP R0
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
    STOREI RZ, -2, RW
    LOADI RW, RZ, -2
    LI RX,2
    CMP RW,RX
    LIMM RY,then_39
    JL RY
    LIMM R0,end_38
    JMP R0
then_39:
    LI R3,0
    MOV R2,R3
    LIMM RW,epilogue_37
    JMP RW
end_38:
    LI RX,2
    STOREI RZ, -1, RX
loop_40:
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -1
    POP R3
    MUL RW,R3,R0
    LOADI R0, RZ, -2
    CMP RW,R0
    LIMM R3,body_41
    JLE R3
    LIMM RX,wend_42
    JMP RX
body_41:
    LOADI RY, RZ, -2
    PUSH RY
    LOADI R0, RZ, -1
    POP R3
    MOD RW,R3,R0
    LI R0,0
    CMP RW,R0
    LIMM R3,then_44
    JE R3
    LIMM RX,end_43
    JMP RX
then_44:
    LI RY,0
    MOV R2,RY
    LIMM R0,epilogue_37
    JMP R0
end_43:
    LOADI R3, RZ, -1
    PUSH R3
    LI RW,1
    POP RX
    ADD RY,RX,RW
    STOREI RZ, -1, RY
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
    STOREI RZ, -3, RW
    LI RW,0
    STOREI RZ, -1, RW
    LI RX,2
    STOREI RZ, -2, RX
loop_46:
    LOADI RY, RZ, -2
    LOADI R0, RZ, -3
    CMP RY,R0
    LIMM R3,body_47
    JLE R3
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
    LI R0,1
    CMP RY,R0
    LIMM R3,then_50
    JE R3
    LIMM RW,end_49
    JMP RW
then_50:
    LOADI RX, RZ, -1
    PUSH RX
    LOADI RY, RZ, -2
    POP R0
    ADD R3,R0,RY
    STOREI RZ, -1, R3
end_49:
    LOADI RY, RZ, -2
    PUSH RY
    LI R0,1
    POP RW
    ADD RX,RW,R0
    STOREI RZ, -2, RX
    LIMM R0,loop_46
    JMP R0
wend_48:
    LOADI RW, RZ, -1
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
    STOREI RZ, -1, RX
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
    MOV R3,R2
    STOREI RZ, -2, R3
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
    STOREI RZ, -3, RX
    LOADI RY, RZ, -1
    PUSH RY
    LOADI R0, RZ, -2
    POP R3
    ADD RW,R3,R0
    PUSH RW
    LOADI R0, RZ, -3
    POP R3
    ADD RX,R3,R0
    MOV R2,RX
    LIMM R0,epilogue_51
    JMP R0
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
