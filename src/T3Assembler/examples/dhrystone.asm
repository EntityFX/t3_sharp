; T3 Dhrystone Benchmark — Balanced Ternary Port v3 (fixed)
; LI R0, main_start → JMP over procedures (LI imm6 limit: ±364)

    LI R0, main_start
    JMP R0

; === Procedure 1: sum of squares ===
; Input: R1=a, R2=b → Output: R2=a²+b²
proc1:
    MUL R1, R1, R1
    MUL R2, R2, R2
    ADD R2, R1, R2
    RET

; === Procedure 2: absolute value ===
; Input: R1=value → Output: R2=|value|
proc2:
    MOV R2, R1
    LI R3, 0
    CMP R1, R3
    LI R4, proc2_neg
    JL R4
    RET
proc2_neg:
    NEG R2, R1
    RET

; === Procedure 3: eval x²+x via stack ===
; Input: stack top=x → Output: push x²+x
proc3:
    POP R0
    MOV R1, R0
    MUL R1, R1, R0
    ADD R1, R1, R0
    PUSH R1
    RET

; === Main ===
main_start:
    LI R0, 10         ; mem[10]=IntGlob
    LI R1, 5
    STORE R1, R0

    ; Proc1(5, 3) → 25+9=34
    LI R0, 10
    LOAD R1, R0       ; R1=5
    LI R2, 3
    LI R3, proc1
    CALL R3           ; R2=34

    ; Proc2(34) → 34
    MOV R1, R2        ; R1=34
    LI R2, proc2
    CALL R2           ; R2=34

    ; (IntGlob*3+7)/2 → (5*3+7)/2=11
    LI R0, 10
    LOAD R3, R0       ; R3=5
    LI R4, 3
    MUL R3, R3, R4    ; 15
    LI R4, 7
    ADD R3, R3, R4    ; 22
    LI R4, 2
    DIV R3, R3, R4    ; 11
    STORE R3, R0      ; IntGlob=11

    ; Array fill mem[100..109] = {1..10}
    LI R0, 0          ; i=0
    LI R1, 100        ; base
    LI R2, 10         ; limit
loop_fill:
    MOV R3, R0
    MUL R3, R3, R0
    ADD R3, R3, R0
    ADD R3, R3, R1    ; addr = base + i*i + i
    LI R4, 1
    ADD R4, R4, R0    ; val = i+1
    STORE R4, R3
    LI R5, 1
    ADD R0, R0, R5    ; i++
    CMP R0, R2
    LI R5, loop_fill
    JL R5

    ; if IntGlob>0 → flag=1 else flag=-1
    LI R0, 10
    LOAD R1, R0       ; R1=11
    LI R2, 0
    CMP R1, R2
    LI R4, pos_b
    JG R4
    LI R1, -1
    LI R0, 11
    STORE R1, R0
    LI R4, done_c
    JMP R4
pos_b:
    LI R1, 1
    LI R0, 11
    STORE R1, R0
done_c:
    ; Proc3(100): 100²+100=10100
    LI R0, 100
    PUSH R0
    LI R1, proc3
    CALL R1
    POP R0           ; R0=10100
    LI R1, 12
    STORE R0, R1     ; mem[12]=10100

    HALT