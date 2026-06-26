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