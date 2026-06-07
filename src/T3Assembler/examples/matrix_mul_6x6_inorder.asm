; Matrix Multiplication 6x6 - In-Order Version
; A = [1...1], B = [1...1] -> C = [6...6]

; --- Code Section ---
start:
    LI A, 6       ; A = N = 6
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
    .word 1; 1
    .word 1; 2
    .word 1; 3
    .word 1; 4
    .word 1; 5
    .word 1; 6
    .word 1; 7
    .word 1; 8
    .word 1; 9
    .word 1; 10
    .word 1; 11
    .word 1; 12
    .word 1; 13
    .word 1; 14
    .word 1; 15
    .word 1; 16
    .word 1; 17
    .word 1; 18
    .word 1; 19
    .word 1; 20
    .word 1; 21
    .word 1; 22
    .word 1; 23
    .word 1; 24
    .word 1; 25
    .word 1; 26
    .word 1; 27
    .word 1; 28
    .word 1; 29
    .word 1; 30
    .word 1; 31
    .word 1; 32
    .word 1; 33
    .word 1; 34
    .word 1; 35
    .word 1; 36

addr_B:
    .word 1; 1
    .word 1; 2
    .word 1; 3
    .word 1; 4
    .word 1; 5
    .word 1; 6
    .word 1; 7
    .word 1; 8
    .word 1; 9
    .word 1; 10
    .word 1; 11
    .word 1; 12
    .word 1; 13
    .word 1; 14
    .word 1; 15
    .word 1; 16
    .word 1; 17
    .word 1; 18
    .word 1; 19
    .word 1; 20
    .word 1; 21
    .word 1; 22
    .word 1; 23
    .word 1; 24
    .word 1; 25
    .word 1; 26
    .word 1; 27
    .word 1; 28
    .word 1; 29
    .word 1; 30
    .word 1; 31
    .word 1; 32
    .word 1; 33
    .word 1; 34
    .word 1; 35
    .word 1; 36

addr_C:
    .word 0; 1
    .word 0; 2
    .word 0; 3
    .word 0; 4
    .word 0; 5
    .word 0; 6
    .word 0; 7
    .word 0; 8
    .word 0; 9
    .word 0; 10
    .word 0; 11
    .word 0; 12
    .word 0; 13
    .word 0; 14
    .word 0; 15
    .word 0; 16
    .word 0; 17
    .word 0; 18
    .word 0; 19
    .word 0; 20
    .word 0; 21
    .word 0; 22
    .word 0; 23
    .word 0; 24
    .word 0; 25
    .word 0; 26
    .word 0; 27
    .word 0; 28
    .word 0; 29
    .word 0; 30
    .word 0; 31
    .word 0; 32
    .word 0; 33
    .word 0; 34
    .word 0; 35
    .word 0; 36