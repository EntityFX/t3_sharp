; T→T3
__entry:
    LI RW,main
    CALL RW
    HALT
main:
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH RZ
    PUSH R0
    PUSH R1
    PUSH R3
    PUSH R4
    LI RZ,5
    LI R4,300
    STORE RZ,R4
    LI R0,1
    LI R4,301
    STORE R0,R4
loop_1:
    LI R4,300
    LOAD R3,R4
    LI RW,1
    CMP R3,RW
    LIMM RX,body_2
    JG RX
    LIMM RY,wend_3
    JMP RY
body_2:
    LI R4,301
    LOAD RZ,R4
    PUSH RZ
    LI R4,300
    LOAD R0,R4
    POP R3
    MUL RW,R3,R0
    LI R4,301
    STORE RW,R4
    LI R4,300
    LOAD RX,R4
    PUSH RX
    LI RY,1
    POP RZ
    SUB R0,RZ,RY
    LI R4,300
    STORE R0,R4
    LIMM R3,loop_1
    JMP R3
wend_3:
    LI R4,301
    LOAD RW,R4
    MOV R2,RW
    LIMM RX,epilogue_0
    JMP RX
epilogue_0:
    POP R4
    POP R3
    POP R1
    POP R0
    POP RZ
    POP RY
    POP RX
    POP RW
    RET
