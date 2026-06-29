; T→T3
__entry:
    LI RW,main
    CALL RW
    HALT
fact:
    POP R2
    POP R3
    LI R4,300
    STORE R3,R4
    PUSH R2
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH RZ
    PUSH R0
    PUSH R1
    PUSH R3
    PUSH R4
    LI RZ,1
    LI R4,301
    STORE RZ,R4
loop_1:
    LI R4,300
    LOAD R0,R4
    LI R3,1
    CMP R0,R3
    LIMM RW,body_2
    JG RW
    LIMM RX,wend_3
    JMP RX
body_2:
    LI R4,301
    LOAD RY,R4
    PUSH RY
    LI R4,300
    LOAD RZ,R4
    POP R0
    MUL R3,R0,RZ
    LI R4,301
    STORE R3,R4
    LI R4,300
    LOAD RW,R4
    PUSH RW
    LI RX,1
    POP RY
    SUB RZ,RY,RX
    LI R4,300
    STORE RZ,R4
    LIMM R0,loop_1
    JMP R0
wend_3:
    LI R4,301
    LOAD R3,R4
    MOV R2,R3
    LIMM RW,epilogue_0
    JMP RW
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
main:
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH RZ
    PUSH R0
    PUSH R1
    PUSH R3
    PUSH R4
    LI RZ,7
    PUSH RZ
    PUSH RW
    PUSH RX
    PUSH RY
    PUSH RZ
    PUSH R0
    PUSH R1
    PUSH R3
    PUSH R4
    LI R1,fact
    CALL R1
    POP R4
    POP R3
    POP R1
    POP R0
    POP RZ
    POP RY
    POP RX
    POP RW
    MOV R0,R2
    MOV R2,R0
    LIMM R3,epilogue_4
    JMP R3
epilogue_4:
    POP R4
    POP R3
    POP R1
    POP R0
    POP RZ
    POP RY
    POP RX
    POP RW
    RET
