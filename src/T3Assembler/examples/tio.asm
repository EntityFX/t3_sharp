; ==============================================================================
; T-lang Standard I/O Library (tio.asm)
; 
; This library provides basic I/O functionality via port 0 (T-SCII Output).
; All functions assume the T3-18/54 architecture.
; ==============================================================================

; --- Constants ---
PORT_OUT equ 0
T_ZERO   equ 0
T_TEN    equ 10

; ------------------------------------------------------------------------------
; putchar(char c)
; Input: R0 = character to print
; ------------------------------------------------------------------------------
putchar:
    OUTI R0, PORT_OUT
    RET

; ------------------------------------------------------------------------------
; printstring(string *s)
; Input: R0 = pointer to null-terminated string
; ------------------------------------------------------------------------------
printstring:
    LOAD R1, R0          ; Load character from string pointer
    CMP R1, 0            ; Check for null terminator
    JE end_printstring
    PUSH R0              ; Save current pointer
    MOV R0, R1           ; Move char to R0 for putchar
    CALL putchar
    POP R0               ; Restore pointer
    ADDI R0, 1           ; Move to next character
    JMP printstring
end_printstring:
    RET

; ------------------------------------------------------------------------------
; printint(int n)
; Input: R0 = integer to print
; Note: Uses stack to reverse digits.
; ------------------------------------------------------------------------------
printint:
    ; Handle negative numbers
    CMP R0, 0
    JGE start_div
    PUSH R0
    MOV R0, 45           ; '-' in ASCII/T-SCII
    CALL putchar
    POP R0
    NEG R0               ; Make positive for processing

start_div:
    MOV R1, 10           ; Divisor
    MOV R2, R0           ; Current value
    MOV R3, 0            ; Digit count for stack popping
div_loop:
    DIV R4, R2, R1       ; R4 = R2 / 10
    MOD R5, R2, R1       ; R5 = R2 % 10
    PUSH R5              ; Store digit on stack
    ADDI R3, 1           ; Increment digit count
    MOV R2, R4           ; Update value to quotient
    CMP R2, 0
    JNE div_loop
pop_loop:
    POP R0               ; Get digit from stack
    ADDI R0, 48          ; Convert to T-SCII ('0' = 48)
    CALL putchar
    SUB R3, 1
    CMP R3, 0
    JNE pop_loop
    RET

; ------------------------------------------------------------------------------
; printfloat(float f)
; Input: F0 = float value to print
; ------------------------------------------------------------------------------
printfloat:
    ; 1. Print Integer Part
    FTOI R1, F0, 0       ; R1 = (int)F0
    PUSH F0              ; Save original float
    MOV R0, R1
    CALL printint
    POP F0
    
    ; 2. Print Decimal Point
    MOV R0, 46           ; '.' in ASCII/T-SCII
    CALL putchar
    
    ; 3. Print Fractional Part
    FMOV F1, F0, 0       ; F1 = F0
    FMOV F2, R1, 1       ; F2 = (float)R1
    FSUB F1, F1, F2      ; F1 = F0 - int(F0)
    
    MOV R2, 10
    FMOV F2, R2, 1       ; F2 = 10.0
    
    MOV R3, 0            ; Precision counter
    MOV R4, 6            ; Limit to 6 decimal places
frac_loop:
    FMUL F3, F1, F2      ; F3 = F1 * 10.0
    FTOI R0, F3, 0       ; R0 = (int)(F1 * 10.0)
    PUSH F1              ; Save current fraction
    ADDI R0, 48          ; Convert to T-SCII
    CALL putchar
    POP F1
    FMOV F4, R0, 1       ; F4 = (float)digit
    FSUB F1, F3, F4      ; F1 = (F1 * 10.0) - digit
    ADDI R3, 1
    CMP R3, R4
    JNE frac_loop
    RET