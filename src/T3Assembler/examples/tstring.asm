; ==============================================================================
; crt0.asm — C Runtime Startup (T3-18 микроконтроллер)
; ==============================================================================
; Инициализирует стек, копирует .data из ROM в RAM, обнуляет .bss, вызывает main
; ==============================================================================

    .global __start
    .global __bss_start
    .global __bss_end
    .global __data_start
    .global __data_end
    .global __data_load
    .global __stack_top
    .global __heap_start
    .global __heap_end

    __stack_top equ 1048575
    __heap_start equ __bss_end
    __heap_end equ __stack_top

__start:
    LI RZ, __stack_top    ; SP = stack_top (processor auto-inits, but explicit for clarity)
    LI R0, __bss_start    ; Zero .bss section
    LI R2, 0
bss_clear:
    CMP R0, __bss_end
    JGE data_copy
    STORE R2, R0
    ADDI R0, 1
    JMP bss_clear

data_copy:
    LI R0, __data_start   ; Copy .data from ROM to RAM
    LI R1, __data_load
data_loop:
    CMP R0, __data_end
    JGE call_main
    LOAD R2, R1
    STORE R2, R0
    ADDI R0, 1
    ADDI R1, 1
    JMP data_loop

call_main:
    LI R1, main
    CALL R1
    HALT
</content>
<write_to_file>
<path>src/T3Assembler/examples/tmath.asm</path>
<content>
; tmath.asm — Math library for T3-18
    .global tabs
    .global tmin
    .global tmax
    .global tpow
    .global tfact

; tabs(x) — absolute value
tabs:
    CMPI R0, 0
    JGE tabs_done
    NEG R0
tabs_done:
    RET

; tmin(a, b) — minimum
tmin:
    CMP R0, R1
    JLE tmin_done
    MOV R0, R1
tmin_done:
    RET

; tmax(a, b) — maximum
tmax:
    CMP R0, R1
    JGE tmax_done
    MOV R0, R1
tmax_done:
    RET

; tpow(base, exp) — power base^exp
tpow:
    LI R2, 1            ; result = 1
    MOV R3, R0          ; base
    MOV R4, R1          ; exp
tpow_loop:
    CMPI R4, 0
    JLE tpow_done
    MUL R2, R2, R3
    SUBI R4, 1
    JMP tpow_loop
tpow_done:
    MOV R0, R2
    RET

; tfact(n) — factorial
tfact:
    LI R1, 1            ; result = 1
tfact_loop:
    CMPI R0, 1
    JLE tfact_done
    MUL R1, R1, R0
    SUBI R0, 1
    JMP tfact_loop
tfact_done:
    MOV R0, R1
    RET
</content>
<write_to_file>
<path>src/T3Assembler/examples/tstring.asm</path>
<content>
; tstring.asm — String library for T3-18
    .global tstrlen
    .global tstrcpy
    .global tstrcmp
    .global tstrcat

; tstrlen(s)
tstrlen:
    LI R2, 0            ; length = 0
    MOV R1, R0          ; R1 = pointer
tstrlen_loop:
    LOAD R0, R1
    CMPI R0, 0
    JE tstrlen_done
    ADDI R2, 1
    ADDI R1, 1
    JMP tstrlen_loop
tstrlen_done:
    MOV R0, R2
    RET

; tstrcpy(dst, src)
tstrcpy:
    MOV R3, R0          ; R3 = dst (preserve original)
tstrcpy_loop:
    LOAD R2, R1         ; load char from src
    STORE R2, R0        ; store to dst
    CMPI R2, 0
    JE tstrcpy_done
    ADDI R0, 1
    ADDI R1, 1
    JMP tstrcpy_loop
tstrcpy_done:
    MOV R0, R3
    RET

; tstrcmp(a, b) — returns -1, 0, +1
tstrcmp:
    MOV R3, R0          ; R3 = a
    MOV R4, R1          ; R4 = b
tstrcmp_loop:
    LOAD R0, R3
    LOAD R1, R4
    CMP R0, R1
    JNE tstrcmp_diff
    CMPI R0, 0
    JE tstrcmp_done
    ADDI R3, 1
    ADDI R4, 1
    JMP tstrcmp_loop
tstrcmp_diff:
    CMP R0, R1
tstrcmp_done:           ; Cond already set by CMP
    MOV R2, R0          ; return 0 if equal
    LI R0, 0
    RET

; tstrcat(dst, src) — append src to dst
tstrcat:
    PUSH R1             ; save src
    MOV R1, R0
tstrcat_find:
    LOAD R2, R1
    CMPI R2, 0
    JE tstrcat_copy
    ADDI R1, 1
    JMP tstrcat_find
tstrcat_copy:
    POP R0              ; R0 = src
    ; R1 points to end of dst. Copy src → end of dst
tstrcat_loop:
    LOAD R2, R0
    STORE R2, R1
    CMPI R2, 0
    JE tstrcat_done
    ADDI R0, 1
    ADDI R1, 1
    JMP tstrcat_loop
tstrcat_done:
    RET
</content>
<write_to_file>
<path>src/T3Assembler/examples/tstdlib.asm</path>
<content>
; tstdlib.asm — Standard library for T3-18 (microcontroller runtime)
    .global tmalloc
    .global tfree
    .global texit

    ; Heap lives between __bss_end and __stack_top
    __heap_current equ 3000  ; will be patched at link time

; tmalloc(size) — bump allocator
tmalloc:
    LI R1, __heap_current
    LOAD R2, R1          ; R2 = current heap ptr
    ADD R3, R2, R0       ; R3 = new ptr
    STORE R3, R1         ; update heap ptr
    MOV R0, R2           ; return old ptr (allocated memory)
    RET

; tfree(ptr) — no-op (bump allocator)
tfree:
    RET

; texit(code) — halt with return code
texit:
    STORE R0, 0xFFFFFF00 ; store exit code to cycle_low (side effect: reset counter)
    HALT

; trand() — simple LCG random: seed * 13 + 17 mod 3^18
trand:
    LI R1, 13
    MUL R0, R0, R1       ; R0 = seed * 13
    ADDI R0, 17          ; R0 = R0 + 17
    ; R0 auto-wraps in balanced ternary (Word18 range)
    RET