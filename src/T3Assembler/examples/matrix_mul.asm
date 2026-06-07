; Matrix Multiplication 2x2
; A = [1, 2; 3, 4], B = [5, 6; 7, 8]
; Result C = [19, 22; 43, 50]

; --- Code Section ---
start:
    LI A, 2       ; A = N = 2
    LI B, 0       ; B = i = 0
    LI I, 1       ; I = constant 1
    
loop_i:
    LI C, 0       ; C = j = 0
    
loop_j:
    LI D, 0       ; D = sum = 0
    LI E, 0       ; E = k = 0
    
loop_k:
    ; index_A = i * N + k
    MOV F, B      ; F = i
    MUL F, A      ; F = i * N
    ADD F, E      ; F = i * N + k
    
    ; Load A[i][k]
    LI G, addr_A
    ADD F, G
    LOAD H, F     ; H = A[i][k]
    
    ; index_B = k * N + j
    MOV F, E      ; F = k
    MUL F, A      ; F = k * N
    ADD F, C      ; F = k * N + j
    
    LI G, addr_B
    ADD F, G
    LOAD F, F     ; F = B[k][j]
    
    MUL H, F      ; H = A[i][k] * B[k][j]
    ADD D, H      ; sum += H
    
    ADD E, I      ; k++
    LI F, loop_k
    CMP E, A      ; k < N?
    JL F
    
    ; Store C[i][j] = sum
    ; index_C = i * N + j
    MOV F, B      ; F = i
    MUL F, A      ; F = i * N
    ADD F, C      ; F = i * N + j
    
    LI G, addr_C
    ADD F, G
    STORE D, F    ; C[i][j] = sum
    
    ADD C, I      ; j++
    LI F, loop_j
    CMP C, A      ; j < N?
    JL F
    
    ADD B, I      ; i++
    LI F, loop_i
    CMP B, A      ; i < N?
    JL F
    
    HALT

; --- Data Section ---
addr_A:
    .word 1
    .word 2
    .word 3
    .word 4

addr_B:
    .word 5
    .word 6
    .word 7
    .word 8

addr_C:
    .word 0
    .word 0
    .word 0
    .word 0
