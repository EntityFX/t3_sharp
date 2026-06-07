; Matrix Multiplication 6x6 - VLIW Version
; A = [1...1], B = [1...1] -> C = [6...6]
; Optimized for VLIW constraints: 1 memory access per bundle

; --- Code Section ---
start:
    { LI A, 6 | LI B, 0 | LI I, 1 } ; A=N=6, B=i=0, I=1
    
loop_i:
    { LI C, 0 | NOP | NOP }         ; C=j=0
    
loop_j:
    { LI D, 0 | LI E, 0 | NOP }     ; D=sum=0, E=k=0
    
loop_k:
    ; --- Calculate index_A = i * N + k ---
    { MOV F, B | NOP | NOP }        ; F = i
    { MUL F, A | NOP | NOP }        ; F = i * N
    { ADD F, E | NOP | NOP }        ; F = i * N + k
    
    ; --- Load A[i][k] ---
    { LI G, addr_A | NOP | NOP }    ; G = base A
    { ADD F, G | NOP | NOP }        ; F = addr A[i][k]
    { LOAD H, F | NOP | NOP }       ; H = A[i][k]
    
    ; --- Calculate index_B = k * N + j ---
    { MOV F, E | NOP | NOP }        ; F = k
    { MUL F, A | NOP | NOP }        ; F = k * N
    { ADD F, C | NOP | NOP }        ; F = k * N + j
    
    ; --- Load B[k][j] ---
    { LI G, addr_B | NOP | NOP }    ; G = base B
    { ADD F, G | NOP | NOP }        ; F = addr B[k][j]
    { LOAD F, F | NOP | NOP }       ; F = B[k][j]
    
    ; --- Accumulate ---
    { MUL H, F | NOP | NOP }        ; H = A[i][k] * B[k][j]
    { ADD D, H | NOP | NOP }        ; sum += H
    
    ; --- Increment k and loop ---
    { ADD E, I | NOP | NOP }        ; k++
    { LI F, loop_k | NOP | NOP }    ; target
    { CMP E, A | NOP | NOP }        ; k < N?
    { JL F | NOP | NOP }            ; loop_k
    
    ; --- Store C[i][j] ---
    { MOV F, B | NOP | NOP }        ; F = i
    { MUL F, A | NOP | NOP }        ; F = i * N
    { ADD F, C | NOP | NOP }        ; F = i * N + j
    { LI G, addr_C | NOP | NOP }    ; G = base C
    { ADD F, G | NOP | NOP }        ; F = addr C[i][j]
    { STORE D, F | NOP | NOP }      ; C[i][j] = sum
    
    ; --- Increment j and loop ---
    { ADD C, I | NOP | NOP }        ; j++
    { LI F, loop_j | NOP | NOP }    ; target
    { CMP C, A | NOP | NOP }        ; j < N?
    { JL F | NOP | NOP }            ; loop_j
    
    ; --- Increment i and loop ---
    { ADD B, I | NOP | NOP }        ; i++
    { LI F, loop_i | NOP | NOP }    ; target
    { CMP B, A | NOP | NOP }        ; i < N?
    { JL F | NOP | NOP }            ; loop_i
    
    { HALT | NOP | NOP }

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