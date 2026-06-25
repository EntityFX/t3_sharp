; Fibonacci Numbers: F(0)=0, F(1)=1, F(n)=F(n-1)+F(n-2)
; Computes first 10 numbers and outputs them

    LI R0, 10          ; count = 10
    LI R1, 0           ; a = 0 (F(0))
    LI R2, 1           ; b = 1 (F(1))
    LI R3, 0           ; i = 0

loop:
    CMP R3, R0         ; if i >= count: exit
    JGE end

    ; Print current number (R1 = F(i))
    PUSH R0            ; save count
    PUSH R1            ; save a
    PUSH R2            ; save b
    PUSH R3            ; save i
    MOV R0, R1         ; putchar expects char in R0
    ; For demonstration, output as decimal digit (0-9)
    ADDI R0, 48        ; convert to ASCII digit
    CALL putchar
    LI R0, 32          ; space
    CALL putchar
    POP R3
    POP R2
    POP R1
    POP R0

    ; Compute next: t = a + b; a = b; b = t
    ADD R4, R1, R2     ; R4 = a + b
    MOV R1, R2         ; a = b
    MOV R2, R4         ; b = t

    ADDI R3, 1         ; i++
    JMP loop

end:
    HALT