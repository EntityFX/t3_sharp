; T→T3
__entry:
    LI RW,main
    CALL RW
    HALT
main:
    LI R3,1
    LI R4,200
    STORE R3,R4
    LI R0,1
    LI R4,201
    STORE R0,R4
loop_0:
    LI R4,201
    LOAD R1,R4
    LI R3,5
    CMP R1,R3
    LIMM R0,body_1
    JLE R0
    LIMM R1,wend_2
    JMP R1
body_1:
    LI R4,200
    LOAD R3,R4
    LI R4,201
    LOAD R0,R4
    MUL R1,R3,R0
    LI R4,200
    STORE R1,R4
    LI R4,201
    LOAD R3,R4
    LI R0,1
    ADD R1,R3,R0
    LI R4,201
    STORE R1,R4
    LIMM R3,loop_0
    JMP R3
wend_2:
    LI R4,200
    LOAD R0,R4
    MOV R2,R0
    RET
    RET
